namespace AgeCalculatorService
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Linq;

    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal sealed class AgeCalculatorService : StatelessService
    {
        public AgeCalculatorService(StatelessServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var endpoints =
                this.Context.CodePackageActivationContext.GetEndpoints()
                    .Where(
                        endpoint =>
                            (endpoint.Protocol == EndpointProtocol.Http)
                            || (endpoint.Protocol == EndpointProtocol.Https))
                    .Select(endpoint => endpoint.Name);

            return
                endpoints.Select(
                    endpoint =>
                        new ServiceInstanceListener(
                            serviceContext =>
                                new OwinCommunicationListener(
                                    Startup.ConfigureApp,
                                    serviceContext,
                                    ServiceEventSource.Current,
                                    endpoint),
                            endpoint));
        }
    }
}