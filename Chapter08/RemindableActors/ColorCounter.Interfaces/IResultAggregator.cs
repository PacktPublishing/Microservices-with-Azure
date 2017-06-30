namespace ColorCounter.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Actors;

    /// <summary>
    ///     This interface defines the methods exposed by an actor.
    ///     Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IResultAggregator : IActor
    {
        Task AggregateResult(string colorName, long count);

        Task TotalPixels(long count);

        Task<Dictionary<string, string>> Result(CancellationToken token);
    }
}