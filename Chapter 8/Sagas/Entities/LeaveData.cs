namespace Entities
{
    using System;

    using NServiceBus;

    public class LeaveData : IContainSagaData
    {
        public virtual string EmployeeName { get; set; }

        public Guid Id { get; set; }

        public string OriginalMessageId { get; set; }

        public string Originator { get; set; }

        public DateTime RequestDate { get; set; }

        public int Length { get; set; }

        public bool Approved { get; set; }
    }
}