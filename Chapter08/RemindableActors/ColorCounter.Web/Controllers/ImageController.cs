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
    ///     Class ImageController.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class ImageController : ApiController
    {
        /// <summary>
        ///     The service URI
        /// </summary>
        private static readonly Uri ServiceUri = new Uri("fabric:/RemindableActors/ColorCounterActorService");

        /// <summary>
        ///     The CTS
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        ///     Submit image to be processed by the actor.
        /// </summary>
        /// <param name="actorId">The actor identifier.</param>
        /// <param name="uri">Public URI of the image to process.</param>
        /// <returns>Task&lt;HttpResponseMessage&gt;.</returns>
        public async Task<HttpResponseMessage> Post(string actorId, Uri uri)
        {
            try
            {
                var colorCounterActor = ActorProxy.Create<IColorCounter>(new ActorId(actorId), ServiceUri);
                var token = this.cts.Token;
                await colorCounterActor.SetImage(uri, token);
                return this.Request.CreateResponse(HttpStatusCode.OK, "submitted");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}