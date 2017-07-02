namespace CompositeWeb
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class CompositeWeb : StatelessService
    {
        public CompositeWeb(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
                {
                    new ServiceInstanceListener(serviceContext =>
                        new WebListenerCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                            new WebHostBuilder()
                                .UseWebListener()
                                .ConfigureServices(
                                    services => services
                                        .AddSingleton<StatelessServiceContext>(serviceContext))
                                .UseContentRoot(Directory.GetCurrentDirectory())
                                .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                .UseStartup<Startup>()
                                .UseUrls(url)
                                .Build()))
                };
        }
    }
}