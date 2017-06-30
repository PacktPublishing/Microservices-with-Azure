namespace ColorCounter.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Actors;

    /// <summary>
    ///     This interface defines the methods exposed by an actor.
    ///     Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IColorCounter : IActor
    {
        Task CountPixels(string color, CancellationToken token);

        Task SetImage(Uri imageUri, CancellationToken token);

        Task LongRunningProcess();
    }
}