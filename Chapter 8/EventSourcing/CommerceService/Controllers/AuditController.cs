namespace CommerceService.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [ServiceRequestActionFilter]
    public class AuditController : BaseController
    {
        public async Task<HttpResponseMessage> Get(string correlationCode)
        {
            try
            {
                var result = this.EventProcessor.ReadStream(correlationCode);
                return this.Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}