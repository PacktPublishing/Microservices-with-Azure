namespace HelloWorldActor.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using HelloWorldActor.Interfaces;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;

    partial class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };
            MainAsync(args, cts.Token).Wait(cts.Token);
        }

        private static async Task MainAsync(string[] args, CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter Your Name");
            var userName = Console.ReadLine();
            var actorClientProxy = ActorProxy.Create<IHelloWorldActor>(
                new ActorId(userName),
                "fabric:/HelloWorldActorsApplication");
            await actorClientProxy.SetReminderAsync(
                "Wake me up in 2 minutes.",
                DateTime.UtcNow + TimeSpan.FromMinutes(1),
                token);
            await actorClientProxy.SetReminderAsync(
                "Another reminder to wake up after a minute.",
                DateTime.UtcNow + TimeSpan.FromMinutes(1),
                token);
            Console.WriteLine("Here are your reminders");
            var reminders = await actorClientProxy.GetRemindersAsync(token);
            foreach (var reminder in reminders)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Reminder at: {reminder.scheduledReminderTimeUtc} with message: {reminder.reminderMessage}");
            }

            Console.ResetColor();
            await actorClientProxy.SubscribeAsync<IReminderActivatedEvent>(new ReminderHandler());
            Console.ReadKey();
        }
    }
}