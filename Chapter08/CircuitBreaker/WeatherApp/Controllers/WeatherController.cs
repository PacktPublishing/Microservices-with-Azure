using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace WeatherApp.Controllers
{
    using System.Threading;

    using Communication;

    public class WeatherController : ApiController
    {
        [SwaggerOperation("GetWeather")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public WeatherReport GetWeather(string postCode)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            var random = new Random().Next(25, 35);
            return new WeatherReport
            {
                Temperature = random,
                WeatherConditions = random < 30 ? "Cloudy" : "Sunny",
                ReportTime = DateTime.UtcNow
            };
        }
    }
}
