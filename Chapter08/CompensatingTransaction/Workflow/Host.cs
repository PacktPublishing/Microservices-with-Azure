namespace Workflow
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    using Newtonsoft.Json;

    public class Host
    {
        const string BookFlightQueueName = QueuePathPrefix + "/Bf";

        const string BookHotelQueueName = QueuePathPrefix + "/Bh";

        const string CancelFlightQueueName = QueuePathPrefix + "/Cf";

        const string CancelHotelQueueName = QueuePathPrefix + "/Ch";

        const string QueuePathPrefix = "booking/1";

        const string ResultQueueName = QueuePathPrefix + "/result";

        const string SagaInputQueueName = QueuePathPrefix + "/input";

        static int pendingTransactions;

        private NamespaceManager namespaceManager;

        private IEnumerable<QueueDescription> queues;

        private MessagingFactory senderMessagingFactory;

        public async Task BookTravel(Booking booking)
        {
            var sender = await this.senderMessagingFactory.CreateMessageSenderAsync(SagaInputQueueName);
            var sagaTerminator = new CancellationTokenSource();
            var workflow = RunWorkflow(this.senderMessagingFactory, sagaTerminator);

            var message =
                new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(booking))))
                    {
                        ContentType = "application/json",
                        Label = "TravelBooking",
                        TimeToLive = TimeSpan.FromMinutes(15)
                    };
            await sender.SendAsync(message);
            Console.WriteLine("Sending booking message. Close application after the workflow completes.");
            var iter = workflow.GetEnumerator();
            while (iter.MoveNext())
            {
                var task = (Task)iter.Current;
                task.Wait();
            }

            await workflow.Task;
        }

        public async Task DeleteQueues()
        {
            if (this.queues != null)
            {
                foreach (var queueDescription in this.queues.Reverse())
                {
                    await this.namespaceManager.DeleteQueueAsync(queueDescription.Path);
                }
            }
        }

        public async Task Run(string namespaceAddress, string manageKeyName, string manageKey)
        {
            this.namespaceManager = new NamespaceManager(
                namespaceAddress,
                TokenProvider.CreateSharedAccessSignatureTokenProvider(manageKeyName, manageKey));

            this.queues = await this.SetupSagaTopologyAsync(this.namespaceManager);
            this.senderMessagingFactory = await MessagingFactory.CreateAsync(
                namespaceAddress,
                TokenProvider.CreateSharedAccessSignatureTokenProvider(manageKeyName, manageKey));
        }

        static WorkflowTaskManager RunWorkflow(
            MessagingFactory workersMessageFactory,
            CancellationTokenSource terminator)
        {
            var workflow = new WorkflowTaskManager(workersMessageFactory, terminator.Token)
                {
                    { BookHotelQueueName, TravelBookingHandlers.BookHotel, BookFlightQueueName, CancelHotelQueueName },
                    { CancelHotelQueueName, TravelBookingHandlers.CancelHotel, ResultQueueName, string.Empty },
                    { BookFlightQueueName, TravelBookingHandlers.BookFlight, ResultQueueName, CancelFlightQueueName },
                    { CancelFlightQueueName, TravelBookingHandlers.CancelFlight, CancelHotelQueueName, string.Empty }
                };
            return workflow;
        }

        async Task<IEnumerable<QueueDescription>> SetupSagaTopologyAsync(NamespaceManager nm)
        {
            return new List<QueueDescription>
                {
                    await nm.QueueExistsAsync(ResultQueueName)
                        ? await nm.GetQueueAsync(ResultQueueName)
                        : await nm.CreateQueueAsync(ResultQueueName),
                    await nm.QueueExistsAsync(CancelFlightQueueName)
                        ? await nm.GetQueueAsync(CancelFlightQueueName)
                        : await nm.CreateQueueAsync(new QueueDescription(CancelFlightQueueName)),
                    await nm.QueueExistsAsync(BookFlightQueueName)
                        ? await nm.GetQueueAsync(BookFlightQueueName)
                        : await nm.CreateQueueAsync(
                            new QueueDescription(BookFlightQueueName)
                                {
                                    // on failure, we move deadletter messages off to the flight 
                                    // booking compensator's queue
                                    EnableDeadLetteringOnMessageExpiration = true,
                                    ForwardDeadLetteredMessagesTo = CancelFlightQueueName
                                }),
                    await nm.QueueExistsAsync(CancelHotelQueueName)
                        ? await nm.GetQueueAsync(CancelHotelQueueName)
                        : await nm.CreateQueueAsync(new QueueDescription(CancelHotelQueueName)),
                    await nm.QueueExistsAsync(BookHotelQueueName)
                        ? await nm.GetQueueAsync(BookHotelQueueName)
                        : await nm.CreateQueueAsync(
                            new QueueDescription(BookHotelQueueName)
                                {
                                    // on failure, we move deadletter messages off to the hotel 
                                    // booking compensator's queue
                                    EnableDeadLetteringOnMessageExpiration = true,
                                    ForwardDeadLetteredMessagesTo = CancelHotelQueueName
                                }),
                    await nm.QueueExistsAsync(SagaInputQueueName)
                        ? await nm.GetQueueAsync(SagaInputQueueName)
                        : await nm.CreateQueueAsync(
                            new QueueDescription(SagaInputQueueName) { ForwardTo = BookHotelQueueName })
                };
        }
    }
}