namespace CompositeWeb.Controllers
{
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json;

    public class HomeController : Controller
    {
        private static readonly string RequestLog = nameof(RequestLog);
        public IActionResult Error()
        {
            return this.View();
        }

        public IActionResult Index()
        {
            List<RequestLog> requestLog = null;
            if (this.HttpContext.Session.Keys.Contains(RequestLog))
            {
                var value = this.HttpContext.Session.GetString(RequestLog);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    requestLog = JsonConvert.DeserializeObject<List<RequestLog>>(value);
                }
            }
            if (requestLog == null)
            {
                requestLog = new List<RequestLog>();
            }

            return this.View(requestLog);
        }
    }
}