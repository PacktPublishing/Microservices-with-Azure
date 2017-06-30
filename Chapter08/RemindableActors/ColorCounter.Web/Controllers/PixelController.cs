namespace ColorCounter.Web.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using ColorCounter.Interfaces;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;

    /// <summary>
    ///     Class PixelController.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class PixelController : ApiController
    {
        /// <summary>
        ///     The service URI
        /// </summary>
        private static readonly Uri ColorCounterServiceUri = new Uri(
            "fabric:/RemindableActors/ColorCounterActorService");

        private static readonly Uri ResultAggregatorServiceUri =
            new Uri("fabric:/RemindableActors/ResultAggregatorActorService");

        /// <summary>
        ///     The CTS
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        ///     Returns the count of pixels of requested color.
        /// </summary>
        /// <param name="actorId">The actor identifier.</param>
        /// <returns>Task&lt;HttpResponseMessage&gt;.</returns>
        public async Task<HttpResponseMessage> Get(string actorId)
        {
            try
            {
                var resultActor = ActorProxy.Create<IResultAggregator>(new ActorId(actorId), ResultAggregatorServiceUri);
                var token = this.cts.Token;
                var result = await resultActor.Result(token);
                return this.Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        /// <summary>
        ///     Request an actor instance to count the pixels of a certain color.
        /// </summary>
        /// <param name="actorId">The actor identifier.</param>
        /// <param name="colorName">Name of the color.</param>
        /// <returns>Task&lt;HttpResponseMessage&gt;.</returns>
        public async Task<HttpResponseMessage> Post(string actorId, string colorName)
        {
            try
            {
                var colorCounterActor = ActorProxy.Create<IColorCounter>(new ActorId(actorId), ColorCounterServiceUri);
                var token = this.cts.Token;
                await colorCounterActor.CountPixels(colorName, token);
                return this.Request.CreateResponse(HttpStatusCode.OK, "submitted");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}