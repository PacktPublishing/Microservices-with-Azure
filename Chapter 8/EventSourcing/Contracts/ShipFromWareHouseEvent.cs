namespace Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    public class ShipFromWareHouseEvent : DomainEvent
    {
        public ShipFromWareHouseEvent(
            DateTime occurred,
            Product product,
            Warehouse warehouse,
            Customer customer,
            string correlationId = "")
            : base(correlationId, occurred)
        {
            this.Product = product;
            this.Warehouse = warehouse;
            this.Customer = customer;
        }

        public Customer Customer { get; set; }

        public Product Product { get; set; }

        public Warehouse Warehouse { get; set; }

        public override async Task<JObject> Process()
        {
            var shippedItemCode = this.Warehouse.Ship(this.Product, this.Customer);
            if (shippedItemCode.Result != string.Empty)
            {
                this.Id = shippedItemCode.Result;
                return JObject.FromObject(new { this.Id });
            }

            throw new KeyNotFoundException(this.Product.Name);
        }

        public override string Message => $"{DateTime.UtcNow:G}: Shipped from warehouse";
    }
}