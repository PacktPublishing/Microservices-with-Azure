namespace WeatherService
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;

    using Contracts;

    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    using Newtonsoft.Json;

    using RestSharp;

    /// <summary>
    ///     An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class WeatherService : StatefulService, IWeatherService
    {
        private CircuitBreaker circuitBreaker;

        public WeatherService(StatefulServiceContext context)
            : base(context)
        {
            this.circuitBreaker = new CircuitBreaker(this.StateManager, 30000);
        }

        public async Task<WeatherReport> GetReport(string postCode)
        {

            var counterState = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(
                "weatherState");
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.ThrowIfCancellationRequested();
            WeatherReport result = null;
            await this.circuitBreaker.Invoke(
                async () =>
                {
                    var client = new RestClient(ConfigurationManager.AppSettings["weatherapi"]);
                    var request = new RestRequest("?postCode={postCode}", Method.GET);
                    request.AddUrlSegment("postCode", postCode);
                    request.Timeout = TimeSpan.FromSeconds(10).Milliseconds;
                    var response = client.Execute<WeatherReport>(request);
                    if (response?.Data != null)
                    {
                        result = response.Data;
                        result.CircuitState = "Open";
                    }
                    else
                    {
                        throw new ApplicationException();
                    }

                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        await counterState.AddOrUpdateAsync(
                            tx,
                            "savedWeather",
                            key => JsonConvert.SerializeObject(result),
                            (key, val) => JsonConvert.SerializeObject(result));
                        await tx.CommitAsync();
                    }
                },
                async () =>
                {
                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        // service faulted. read old value and populate.
                        var value = await counterState.TryGetValueAsync(tx, "savedWeather");
                        if (value.HasValue)
                        {
                            result = JsonConvert.DeserializeObject<WeatherReport>(value.Value);
                        }
                        else
                        {
                            result = new WeatherReport { ReportTime = DateTime.UtcNow, Temperature = 0, WeatherConditions = "Unknown" };
                            await counterState.AddOrUpdateAsync(
                                tx,
                                "savedWeather",
                                key => JsonConvert.SerializeObject(result),
                                (key, val) => JsonConvert.SerializeObject(result));
                            await tx.CommitAsync();
                        }

                        result.CircuitState = "Closed";
                    }
                });

            return result;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener((context) =>this.CreateServiceRemotingListener(context)) };
        }
    }
}