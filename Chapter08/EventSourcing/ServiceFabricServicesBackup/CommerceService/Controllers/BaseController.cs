namespace CommerceService.Controllers
{
    using System.Web.Http;

    using EventManager;

    using Redis;

    public class BaseController : ApiController
    {
        protected readonly CacheProxy CacheProxy;

        protected readonly EventProcessor EventProcessor;

        public BaseController()
        {
            this.EventProcessor = new EventProcessor(CommerceService.GetConfigurationValue("ESConnectionString"));
            this.CacheProxy = new CacheProxy(CommerceService.GetConfigurationValue("RedisConnectionString"));
        }
    }
}