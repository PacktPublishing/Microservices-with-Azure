namespace LeaveSagaService
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Common;

    using Entities;

    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    using NServiceBus;

    internal sealed class LeaveSagaService : StatelessService
    {
        static IEndpointInstance endpointInstance;

        public LeaveSagaService(StatelessServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateInputListener(context)) };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var endpointConfiguration = new EndpointConfiguration("leavetransport");
                var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();
                persistence.ConnectionString("UseDevelopmentStorage=true");
                endpointConfiguration.SendFailedMessagesTo("error");
                var transport = endpointConfiguration.UseTransport<AzureStorageQueueTransport>();
                transport.ConnectionString("UseDevelopmentStorage=true");
                endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
                cancellationToken.WaitHandle.WaitOne();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (endpointInstance != null)
                {
                    await endpointInstance.Stop().ConfigureAwait(false);
                }
            }
        }

        private ICommunicationListener CreateInputListener(ServiceContext context)
        {
            var inputEndpoint = context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            var uriPrefix = $"{inputEndpoint.Protocol}://+:{inputEndpoint.Port}/leavesagaapplication/";
            var uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            return new HttpCommunicationListener(uriPrefix, uriPublished, this.ProcessInputRequest);
        }

        private async Task ProcessInputRequest(HttpListenerContext context, CancellationToken cancelRequest)
        {
            string output = null;
            var employeeName = context.Request.QueryString["name"];
            var startDate = context.Request.QueryString["startdate"];
            var length = context.Request.QueryString["length"];
            try
            {
                if (string.IsNullOrWhiteSpace(employeeName) && string.IsNullOrWhiteSpace(startDate) && string.IsNullOrWhiteSpace(length))
                {
                    output = "parameters not specified";
                }
                else
                {
                    await endpointInstance.Send("leavetransport", new LeaveRequest { EmployeeName = employeeName, StartDate = DateTime.Parse(startDate), Length = int.Parse(length) });
                    output = "request accepted";
                }
                using (var response = context.Response)
                {
                    if (output != null)
                    {
                        var outBytes = Encoding.UTF8.GetBytes(output);
                        response.OutputStream.Write(outBytes, 0, outBytes.Length);
                    }
                }
            }
            catch (ApplicationException e)
            {
            }
        }
    }
}