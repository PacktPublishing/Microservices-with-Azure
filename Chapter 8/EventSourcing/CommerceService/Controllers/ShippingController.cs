namespace CommerceService.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Contracts;

    [ServiceRequestActionFilter]
    public class ShippingController : BaseController
    {
        public async Task<HttpResponseMessage> Post(string productName, string warehouseName, string customerName)
        {
            try
            {
                var product = new Product(this.CacheProxy) { Name = productName };
                var wareHouse = new Warehouse(this.CacheProxy) { Name = warehouseName };
                var customer = new Customer(this.CacheProxy) { Name = customerName };
                var result =
                    await this.EventProcessor.Process(
                        new ShipFromWareHouseEvent(DateTime.UtcNow, product, wareHouse, customer));
                return this.Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}