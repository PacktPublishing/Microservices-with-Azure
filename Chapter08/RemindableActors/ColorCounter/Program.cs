namespace ColorCounter
{
    using System;
    using System.Threading;

    using Microsoft.ServiceFabric.Actors.Runtime;

    internal static class Program
    {
        /// <summary>
        ///     This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                ActorRuntime.RegisterActorAsync<ColorCounter>(
                    (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

                ActorRuntime.RegisterActorAsync<ResultAggregator>(
                    (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}