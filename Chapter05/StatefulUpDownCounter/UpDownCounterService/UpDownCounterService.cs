namespace UpDownCounterService
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal sealed class UpDownCounterService : StatefulService
    {
        public UpDownCounterService(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("counter");
            var myPartitionName = (this.Partition.PartitionInfo as NamedPartitionInformation)?.Name;
            switch (myPartitionName)
            {
                case "UpCounter":
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            var result = await myDictionary.TryGetValueAsync(tx, "Counter");
                            ServiceEventSource.Current.ServiceMessage(
                                this,
                                "Current Counter Value: {0}",
                                result.HasValue ? result.Value.ToString() : "Value does not exist.");
                            await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);
                            await tx.CommitAsync();
                        }

                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                case "DownCounter":
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            var result = await myDictionary.TryGetValueAsync(tx, "Counter");
                            ServiceEventSource.Current.ServiceMessage(
                                this,
                                "Current Counter Value: {0}",
                                result.HasValue ? result.Value.ToString() : "Value does not exist.");
                            await myDictionary.AddOrUpdateAsync(tx, "Counter", 1000, (key, value) => --value);
                            await tx.CommitAsync();
                        }
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
            }
        }
    }
}