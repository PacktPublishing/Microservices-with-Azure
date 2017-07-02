namespace Contracts
{
    using System;

    public class RequestLog
    {
        public string CircuitState { get; set; }

        public double RequestDuration { get; set; }

        public DateTime RequestTime { get; set; }
    }
}