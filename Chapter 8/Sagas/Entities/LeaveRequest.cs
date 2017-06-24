namespace Entities
{
    using NServiceBus;

    public class LeaveRequest : IMessage
    {
        public bool Approved { get; set; }

        public string EmployeeName { get; set; }
    }
}