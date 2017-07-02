namespace Workflow
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceBus.Messaging;

    class WorkflowTaskManager : IEnumerable
    {
        readonly MessagingFactory messagingFactory;

        readonly Collection<Task> tasks = new Collection<Task>();

        CancellationToken cancellationToken;

        public WorkflowTaskManager(MessagingFactory messagingFactory, CancellationToken cancellationToken)
        {
            this.messagingFactory = messagingFactory;
            this.cancellationToken = cancellationToken;
        }

        public Task Task => Task.WhenAll(this.tasks);

        public void Add(
            string taskQueueName,
            Func<BrokeredMessage, MessageSender, MessageSender, Task> doWork,
            string nextStepQueue,
            string compensatorQueue)
        {
            var tcs = new TaskCompletionSource<bool>();
            var rcv = this.messagingFactory.CreateMessageReceiver(taskQueueName);
            var nextStepSender = this.messagingFactory.CreateMessageSender(nextStepQueue, taskQueueName);
            var compensatorSender = this.messagingFactory.CreateMessageSender(compensatorQueue, taskQueueName);

            this.cancellationToken.Register(
                () =>
                    {
                        rcv.Close();
                        tcs.SetResult(true);
                    });
            rcv.OnMessageAsync(
                m => doWork(m, nextStepSender, compensatorSender),
                new OnMessageOptions { AutoComplete = false });
            this.tasks.Add(tcs.Task);
        }

        public IEnumerator GetEnumerator()
        {
            return this.tasks.GetEnumerator();
        }
    }
}