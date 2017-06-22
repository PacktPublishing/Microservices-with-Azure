namespace HelloWorldActor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using global::HelloWorldActor.Interfaces;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Runtime;

    [StatePersistence(StatePersistence.Persisted)]
    internal class HelloWorldActor : Actor, IHelloWorldActor, IRemindable
    {
        public HelloWorldActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<List<(string reminderMessage, DateTime scheduledReminderTimeUtc)>> GetRemindersAsync(
            CancellationToken cancellationToken)
        {
            return await this.StateManager.GetStateAsync<List<(string reminderMessage, DateTime scheduledReminderTimeUtc)>>("reminders", cancellationToken);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var payloadTicks = BitConverter.ToInt64(state, 0);
            // Get the reminder from state.
            var cancellationToken = CancellationToken.None;
            var existingReminders = await this.StateManager
                .GetStateAsync<List<(string reminderMessage, DateTime scheduledReminderTimeUtc)>>(
                    "reminders",
                    cancellationToken);
            var reminderMessage =
                existingReminders.FirstOrDefault(
                    reminder => reminder.scheduledReminderTimeUtc == new DateTime(payloadTicks));
            if (reminderMessage.reminderMessage != string.Empty)
            {
                // Trigger an event for the client.
                var evt = this.GetEvent<IReminderActivatedEvent>();
                evt.ReminderActivated(reminderMessage.reminderMessage);
            }

            // Unregister the reminder.
            var thisReminder = this.GetReminder(reminderName);
            await this.UnregisterReminderAsync(thisReminder);
        }

        public async Task SetReminderAsync(
            string reminderMessage,
            DateTime scheduledReminderTimeUtc,
            CancellationToken cancellationToken)
        {
            var existingReminders = await this.StateManager
                .GetStateAsync<List<(string reminderMessage, DateTime scheduledReminderTimeUtc)>>(
                    "reminders",
                    cancellationToken);
            // Add another reminder.
            existingReminders.Add((reminderMessage, scheduledReminderTimeUtc));
            // Set a reminder.
            await this.RegisterReminderAsync(
                           $"{this.Id}:{Guid.NewGuid()}",
                           BitConverter.GetBytes(scheduledReminderTimeUtc.Ticks),
                           scheduledReminderTimeUtc - DateTime.UtcNow,
                           TimeSpan.FromMilliseconds(-1));
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return this.StateManager.TryAddStateAsync("reminders", new List<(string reminderMessage, DateTime scheduledReminderTimeUtc)>());
        }
    }
}