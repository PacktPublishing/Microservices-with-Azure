namespace Entities
{
    using System;

    using NServiceBus;

    public class LeaveRequest : IMessage
    {
        public bool Approved { get; set; }

        public string EmployeeName { get; set; }

        public DateTime StartDate { get; set; }

        public int Length { get; set; }
    }
}