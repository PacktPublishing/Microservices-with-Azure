namespace LeaveSagaService
{
    using System;
    using System.Fabric;
    using System.Threading.Tasks;

    using Common;

    using Entities;

    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;

    using NServiceBus;

    class LeaveApprovalSaga : Saga<LeaveData>, IAmStartedByMessages<LeaveRequest>
    {
        public Task Handle(LeaveRequest message, IMessageHandlerContext context)
        {
            ServiceEventSource.Current.Message($"Leave approval message received for employee: {message.EmployeeName}");
            var awaiter = this.AskForApproval(message.EmployeeName, message.StartDate, message.Length).GetAwaiter();
            awaiter.GetResult();
            return Task.CompletedTask;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<LeaveData> mapper)
        {
            mapper.ConfigureMapping<LeaveRequest>(message => message.EmployeeName).ToSaga(saga => saga.EmployeeName);
        }

        private async Task AskForApproval(string employeeName, DateTime startDate, int length)
        {
            Data.RequestDate = startDate;
            Data.Length = length;
            var isLeaveApproved = false;
            var fabricClient = new FabricClient();
            var communicationFactory =
                new HttpCommunicationClientFactory(new ServicePartitionResolver(() => fabricClient));
            var lineManagerServiceUri = new Uri(
                FabricRuntime.GetActivationContext().ApplicationName + "/LineManagerLeaveApprovalService");
            var lineManagerClient = new ServicePartitionClient<HttpCommunicationClient>(
                communicationFactory,
                lineManagerServiceUri,
                new ServicePartitionKey());

            var hrManagerServiceUri = new Uri(
                FabricRuntime.GetActivationContext().ApplicationName + "/HRLeaveApprovalService");
            var hrClient = new ServicePartitionClient<HttpCommunicationClient>(
                communicationFactory,
                hrManagerServiceUri,
                new ServicePartitionKey());

            await lineManagerClient.InvokeWithRetryAsync(
                async client1 =>
                    {
                        var lineManagerResponse =
                            await client1.HttpClient.GetStringAsync(
                                new Uri(client1.Url, $"Employee?name={employeeName}&startDate={startDate}&length={length}"));
                        if (lineManagerResponse == "true")
                        {
                            ServiceEventSource.Current.Message($"Line manager approval received for employee: {employeeName}");
                            await hrClient.InvokeWithRetryAsync(
                                async client2 =>
                                    {
                                        var hrLeaveApprovalResponse = await client2.HttpClient.GetStringAsync(
                                            new Uri(client2.Url, $"Employee?name={employeeName}&startDate={startDate}&length={length}"));
                                        if (hrLeaveApprovalResponse == "true")
                                        {
                                            ServiceEventSource.Current.Message($"HR approval received for employee: {employeeName}");
                                            isLeaveApproved = true;
                                        }
                                    });
                        }
                    });

            Data.Approved = isLeaveApproved;
            // Send notification to employee here and finally mark the saga as complete.
            ServiceEventSource.Current.Message($"Leave approval process completed for employee: {employeeName}");
            this.MarkAsComplete();
        }
    }
}