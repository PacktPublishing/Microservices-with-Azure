namespace ColorCounter.Web.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using ColorCounter.Interfaces;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;

    /// <summary>
    ///     Class ImageController.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class ConcurrencyDemoController : ApiController
    {
        /// <summary>
        ///     The service URI
        /// </summary>
        private static readonly Uri ServiceUri = new Uri("fabric:/RemindableActors/ColorCounterActorService");

        /// <summary>
        ///     Demonstrates concurrency requirement of Actors.
        /// </summary>
        /// <returns>Task&lt;HttpResponseMessage&gt;.</returns>
        public async Task<HttpResponseMessage> Get(string actorId)
        {
            try
            {
                var colorCounterActor = ActorProxy.Create<IColorCounter>(new ActorId(actorId), ServiceUri);
                await colorCounterActor.LongRunningProcess();
                return this.Request.CreateResponse(HttpStatusCode.OK, "completed");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}