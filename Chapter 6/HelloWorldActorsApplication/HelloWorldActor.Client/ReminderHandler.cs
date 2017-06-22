namespace HelloWorldActor.Client
{
    using System;

    using HelloWorldActor.Interfaces;

    public class ReminderHandler : IReminderActivatedEvent
    {
        public void ReminderActivated(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Reminder Activated: {message}");
            Console.ResetColor();
        }
    }
}