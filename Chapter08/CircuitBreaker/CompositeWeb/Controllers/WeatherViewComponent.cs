namespace CompositeWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Linq;
    using System.Threading.Tasks;

    using Contracts;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Remoting.Client;

    using Newtonsoft.Json;

    public class WeatherViewComponent : ViewComponent
    {
        private static readonly string RequestLog = nameof(RequestLog);
        private static readonly Uri WeatherServiceUri =
            new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/WeatherService");

        private readonly IWeatherService weatherServiceClient =
            ServiceProxy.Create<IWeatherService>(WeatherServiceUri, new ServicePartitionKey("basic"));

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                List<RequestLog> requestLog = null;
                if (this.HttpContext.Session.Keys.Contains(RequestLog))
                {
                    var value = this.HttpContext.Session.GetString(RequestLog);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        requestLog = JsonConvert.DeserializeObject<List<RequestLog>>(value);
                    }
                }

                var watch = Stopwatch.StartNew();
                var result = await this.weatherServiceClient.GetReport("2010");
                watch.Stop();
                result.ResponseTimeInSeconds = watch.Elapsed.TotalSeconds;
                if (requestLog == null)
                {
                    requestLog = new List<RequestLog>();
                }

                requestLog.Add(new RequestLog { CircuitState = result.CircuitState, RequestDuration = result.ResponseTimeInSeconds, RequestTime = DateTime.UtcNow });
                this.HttpContext.Session.SetString(RequestLog, JsonConvert.SerializeObject(requestLog));
                return this.View(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
        }
    }
}