namespace CounterService
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;

    using Communication;

    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    using Newtonsoft.Json;

    /// <summary>
    ///     An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CounterService : StatefulService, ICounterService
    {
        readonly CircuitBreaker circuitBreaker;

        public CounterService(StatefulServiceContext context)
            : base(context)
        {
            this.circuitBreaker = new CircuitBreaker(this.StateManager, 30000);
        }

        public async Task<CounterResult> GetValue()
        {
            var random = new Random().Next(1, 1000);
            var counterState = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(
                "counterState");
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.ThrowIfCancellationRequested();
            CounterResult result = null;
            await this.circuitBreaker.Invoke(
                async () =>
                    {
                        // mocking service result. randomly failing the service call.
                        var failureSeed = new Random().Next(1, 20);
                        if (failureSeed % 3 == 0)
                        {
                            throw new ApplicationException();
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        result = new CounterResult
                        {
                            Value = random,
                            ReportTime = DateTime.UtcNow,
                            CircuitState = "Open"
                        };
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            await counterState.AddOrUpdateAsync(
                                tx,
                                "savedCount",
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
                            var value = await counterState.TryGetValueAsync(tx, "savedCount");
                            if (value.HasValue)
                            {
                                result = JsonConvert.DeserializeObject<CounterResult>(value.Value);
                            }
                            else
                            {
                                result = new CounterResult { ReportTime = DateTime.UtcNow, Value = 0 };
                                await counterState.AddOrUpdateAsync(
                                    tx,
                                    "savedCount",
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
            return new[] { new ServiceReplicaListener(this.CreateServiceRemotingListener) };
        }
    }
}