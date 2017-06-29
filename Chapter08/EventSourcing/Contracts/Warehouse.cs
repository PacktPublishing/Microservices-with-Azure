namespace Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Redis;

    public class Warehouse : DomainEntity
    {
        public Warehouse(CacheProxy cacheProxy)
            : base(cacheProxy)
        {
        }

        public string Name { get; set; }

        public Task AddToInventory(Product product)
        {
            var inventory = this.CacheProxy.Get<List<Product>>($"{this.Name.ToLowerInvariant()}-inventory")
                ?? new List<Product>();
            inventory.Add(product);
            this.CacheProxy.Set($"{this.Name.ToLowerInvariant()}-inventory", inventory);
            return Task.FromResult(1);
        }

        public Task<string> Ship(Product product, Customer customer)
        {
            var inventory = this.CacheProxy.Get<List<Product>>($"{this.Name.ToLowerInvariant()}-inventory")
                ?? new List<Product>();
            var selectedProduct = inventory.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase));
            if (selectedProduct)
            {
                var productId = string.Empty;
                foreach (var inventoryProduct in inventory)
                {
                    if (!inventoryProduct.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    productId = inventoryProduct.Id;
                    inventory.Remove(inventoryProduct);
                    break;
                }

                this.CacheProxy.Set($"{this.Name.ToLowerInvariant()}-inventory", inventory);
                return Task.FromResult(productId);
            }

            return Task.FromResult(string.Empty);
        }
    }
}