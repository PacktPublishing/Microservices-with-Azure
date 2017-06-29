namespace LineManagerLeaveApprovalService
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Common;

    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal sealed class LineManagerLeaveApprovalService : StatelessService
    {
        public LineManagerLeaveApprovalService(StatelessServiceContext context)
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
            var uriPrefix = $"{inputEndpoint.Protocol}://+:{inputEndpoint.Port}/linemanagerleaveapprovalservice/";
            var uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            return new HttpCommunicationListener(uriPrefix, uriPublished, ProcessInputRequest);
        }

        private static async Task ProcessInputRequest(HttpListenerContext context, CancellationToken cancelRequest)
        {
            var employeeName = context.Request.QueryString["EmployeeName"];
            var dateOfRequest = context.Request.QueryString["DateOfRequest"];
            await Task.FromResult(true);
        }
    }
}