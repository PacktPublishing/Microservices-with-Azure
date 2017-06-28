namespace CommerceService.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Contracts;

    [ServiceRequestActionFilter]
    public class InventoryController : BaseController
    {
        public async Task<HttpResponseMessage> Post(
            string productId,
            string productName,
            string supplierName,
            string warehouseCode)
        {
            try
            {
                var product = new Product(this.CacheProxy)
                    {
                        Id = productId,
                        Name = productName,
                        Supplier = supplierName
                    };
                var wareHouse = new Warehouse(this.CacheProxy) { Name = warehouseCode };
                await this.EventProcessor.Process(
                    new AddToInventoryEvent(productId, DateTime.UtcNow, product, wareHouse));
                return this.Request.CreateResponse(HttpStatusCode.OK, "done");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}