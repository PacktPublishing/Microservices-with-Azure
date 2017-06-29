namespace Contracts
{
    using Redis;

    public class DomainEntity
    {
        public string Id;

        public DomainEntity(CacheProxy cacheProxy)
        {
            this.CacheProxy = cacheProxy;
        }

        public CacheProxy CacheProxy { get; set; }

        public bool ShouldSerializeCacheProxy()
        {
            return false;
        }
    }
}