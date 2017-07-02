namespace Communication
{
    using System;

    public class WeatherReport
    {
        public int Temperature { get; set; }
        public string WeatherConditions { get; set; }
        public DateTime ReportTime { get; set; }
    }
}