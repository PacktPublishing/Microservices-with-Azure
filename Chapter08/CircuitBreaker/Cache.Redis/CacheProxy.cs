using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.Redis
{
    using Newtonsoft.Json;

    using StackExchange.Redis;

    public class CacheProxy
    {
        private readonly ConnectionMultiplexer connection;

        public CacheProxy(string connectionString)
        {
            this.connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public T Get<T>(string key)
        {
            var cache = this.connection.GetDatabase();
            var data = cache.StringGet(key);
            return string.IsNullOrEmpty(data) ? default(T) : JsonConvert.DeserializeObject<T>(data);
        }

        public void Set<T>(string key, T value)
        {
            var cache = this.connection.GetDatabase();
            cache.StringSet(key, JsonConvert.SerializeObject(value));
        }
    }
}
