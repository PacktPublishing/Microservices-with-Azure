namespace Contracts
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    public class AddToInventoryEvent : DomainEvent
    {
        public AddToInventoryEvent(string correlationId, DateTime occurred, Product product, Warehouse warehouse)
            : base(correlationId, occurred)
        {
            this.Product = product;
            this.Warehouse = warehouse;
        }

        public Product Product { get; set; }

        public Warehouse Warehouse { get; set; }

        public override async Task<JObject> Process()
        {
            await this.Warehouse.AddToInventory(this.Product);
            return JObject.FromObject(new { Result = 1 });
        }

        public override string Message => $"{DateTime.UtcNow:G}: Added product to warehouse inventory";
    }
}