namespace Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Redis;

    public class Customer : DomainEntity
    {
        public Customer(CacheProxy cacheProxy)
            : base(cacheProxy)
        {
        }

        public string Name { get; set; }

        public Task Receive(Product product)
        {
            var receipts = this.CacheProxy.Get<List<Receipt>>("receipts") ?? new List<Receipt>();
            receipts.Add(new Receipt { Product = product, Customer = this });
            this.CacheProxy.Set("receipts", receipts);
            return Task.FromResult(1);
        }
    }
}