namespace HelloWorldActor.Interfaces
{
    using Microsoft.ServiceFabric.Actors;

    public interface IReminderActivatedEvent : IActorEvents
    {
        void ReminderActivated(string message);
    }
}