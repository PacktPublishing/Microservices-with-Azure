namespace HRLeaveApprovalService
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Net;
    using System.Text;
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

        private static async Task ProcessInputRequest(HttpListenerContext context, CancellationToken cancellationToken)
        {
            using (var response = context.Response)
            {
                var outBytes = Encoding.UTF8.GetBytes("true");
                response.OutputStream.Write(outBytes, 0, outBytes.Length);
            }
        }

        private ICommunicationListener CreateInputListener(StatelessServiceContext context)
        {
            var inputEndpoint = context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            var uriPrefix = $"{inputEndpoint.Protocol}://+:{inputEndpoint.Port}/hrleaveapprovalservice/";
            var uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            return new HttpCommunicationListener(uriPrefix, uriPublished, ProcessInputRequest);
        }
    }
}