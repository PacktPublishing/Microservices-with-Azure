namespace CommerceService
{
    using System.Linq;
    using System.Web.Http;

    using Owin;

    using Swashbuckle.Application;

    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.EnableSwagger(
                c =>
                    {
                        c.SingleApiVersion("v1", "Pattern: Event Sourcing");
                        c.ResolveConflictingActions(x => x.First());
                    }).EnableSwaggerUi();

            appBuilder.UseWebApi(config);
        }
    }
}