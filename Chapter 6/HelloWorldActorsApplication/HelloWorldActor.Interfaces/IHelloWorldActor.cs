using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace HelloWorldActor.Interfaces
{
    public interface IHelloWorldActor : IActor, IActorEventPublisher<IReminderActivatedEvent>
    {
        Task<List<(string reminderMessage, DateTime scheduledReminderTimeUtc)>> GetRemindersAsync(CancellationToken cancellationToken);

        Task SetReminderAsync(string reminderMessage, DateTime scheduledReminderTimeUtc, CancellationToken cancellationToken);
    }
}
