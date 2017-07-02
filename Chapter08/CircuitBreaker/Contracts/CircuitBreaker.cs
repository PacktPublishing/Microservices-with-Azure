namespace Contracts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    public class CircuitBreaker
    {
        private IReliableStateManager stateManager;

        private readonly int resetTimeoutInMilliseconds;

        public CircuitBreaker(IReliableStateManager stateManager, int resetTimeoutInMilliseconds)
        {
            this.stateManager = stateManager;
            this.resetTimeoutInMilliseconds = resetTimeoutInMilliseconds;
        }

        public async Task Invoke(Func<Task> func, Func<Task> failAction)
        {
            var errorHistory = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, DateTime>>("errorHistory");
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.ThrowIfCancellationRequested();

            using (var tx = this.stateManager.CreateTransaction())
            {
                var errorTime = await errorHistory.TryGetValueAsync(tx, "errorTime");
                if (errorTime.HasValue)
                {
                    if ((DateTime.UtcNow - errorTime.Value).TotalMilliseconds < this.resetTimeoutInMilliseconds)
                    {
                        await failAction();
                        return;
                    }
                }
                try
                {
                    await func();
                    await errorHistory.AddOrUpdateAsync(
                        tx,
                        "errorTime",
                        key => DateTime.MinValue,
                        (key, value) => DateTime.MinValue);
                }
                catch (Exception)
                {
                    await failAction();
                    await errorHistory.AddOrUpdateAsync(
                        tx,
                        "errorTime",
                        key => DateTime.UtcNow,
                        (key, value) => DateTime.UtcNow);
                }
                finally
                {
                    await tx.CommitAsync();
                }
            }
        }
    }
}