namespace LeaveSagaService
{
    using System;
    using System.Fabric;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Common;

    using Entities;

    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;

    using NServiceBus;

    class LeaveApprovalSaga : Saga<LeaveData>,IAmStartedByMessages<LeaveRequest>
    {
        public Task Handle(LeaveRequest message, IMessageHandlerContext context)
        {
            AskForApproval(message.EmployeeName);
        }

        private async void AskForApproval(string employeeName)
        {
            var fabricClient = new FabricClient();
            var communicationFactory = new HttpCommunicationClientFactory(new ServicePartitionResolver(() => fabricClient));
            var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/LineManagerLeaveApprovalService");

            ServicePartitionClient<HttpCommunicationClient> partitionClient =
                new ServicePartitionClient<HttpCommunicationClient>(
                    communicationFactory,
                    serviceUri,
                    new ServicePartitionKey());

        await
                partitionClient.InvokeWithRetryAsync(
                    async (client) =>
                        {
                            await client.HttpClient.PutAsync(
                                new Uri(client.Url, "Employee/" + employeeName), 
                                new StringContent(String.Empty));
                        });
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<LeaveData> mapper)
        {
            mapper.ConfigureMapping<LeaveRequest>(message => message.EmployeeName).ToSaga(saga => saga.EmployeeName);
        }
    }
}