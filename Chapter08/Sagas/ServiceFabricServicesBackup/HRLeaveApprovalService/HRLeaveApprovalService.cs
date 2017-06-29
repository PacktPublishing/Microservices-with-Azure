namespace HRLeaveApprovalService
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Common;

    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal sealed class HRLeaveApprovalService : StatelessService
    {
        public HRLeaveApprovalService(StatelessServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateInputListener(context)) };
        }

        private ICommunicationListener CreateInputListener(StatelessServiceContext context)
        {
            var inputEndpoint = context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            var uriPrefix = $"{inputEndpoint.Protocol}://+:{inputEndpoint.Port}/hrleaveapprovalservice/";
            var uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            return new HttpCommunicationListener(uriPrefix, uriPublished, ProcessInputRequest);
        }

        private static Task ProcessInputRequest(HttpListenerContext arg1, CancellationToken arg2)
        {
            return null;
        }
    }
}