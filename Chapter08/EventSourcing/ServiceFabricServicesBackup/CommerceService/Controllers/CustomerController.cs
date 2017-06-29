namespace CommerceService.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Contracts;

    [ServiceRequestActionFilter]
    public class CustomerController : BaseController
    {
        public async Task<HttpResponseMessage> Post(string productId, string customerName)
        {
            try
            {
                var product = new Product(this.CacheProxy) { Id = productId };
                var customer = new Customer(this.CacheProxy) { Name = customerName };
                await this.EventProcessor.Process(new DeliveredToCustomerEvent(product, customer, DateTime.UtcNow));
                return this.Request.CreateResponse(HttpStatusCode.OK, "delivered");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        public async Task<HttpResponseMessage> Put(string productId, string customerName)
        {
            try
            {
                var product = new Product(this.CacheProxy) { Id = productId };
                var customer = new Customer(this.CacheProxy) { Name = customerName };
                await this.EventProcessor.Process(new DeliveryToCustomerFailedEvent(product, customer, DateTime.UtcNow));
                return this.Request.CreateResponse(HttpStatusCode.OK, "status updated");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}