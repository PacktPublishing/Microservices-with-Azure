namespace Contracts
{
    using System;

    public class WeatherReport
    {
        public int Temperature { get; set; }
        public string WeatherConditions { get; set; }
        public DateTime ReportTime { get; set; }
        public string CircuitState { get; set; }
        public double ResponseTimeInSeconds { get; set; }
    }
}