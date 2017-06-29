namespace Contracts
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    public class DeliveredToCustomerEvent : DomainEvent
    {
        public DeliveredToCustomerEvent(Product product, Customer customer, DateTime occurred)
            : base(product.Id, occurred)
        {
            this.Product = product;
            this.Customer = customer;
        }

        public Customer Customer { get; set; }

        public Product Product { get; set; }

        public override async Task<JObject> Process()
        {
            await this.Customer.Receive(this.Product);
            return JObject.FromObject(new { Result = 1 });
        }

        public override string Message => $"{DateTime.UtcNow:G}: Delivered to customer";
    }
}