namespace Common
{
    using System;
    using System.Fabric;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public sealed class HttpCommunicationListener : ICommunicationListener
    {
        private readonly HttpListener httpListener;

        private readonly Func<HttpListenerContext, CancellationToken, Task> processRequest;

        private readonly CancellationTokenSource processRequestsCancellation = new CancellationTokenSource();

        private readonly string publishUri;

        private StatelessServiceContext context;

        public HttpCommunicationListener(
            string uriPrefix,
            string uriPublished,
            Func<HttpListenerContext, CancellationToken, Task> processRequest)
        {
            this.publishUri = uriPublished;
            this.processRequest = processRequest;
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(uriPrefix);
        }

        public void Abort()
        {
            this.processRequestsCancellation.Cancel();
            this.httpListener.Abort();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.processRequestsCancellation.Cancel();
            this.httpListener.Close();
            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            this.httpListener.Start();

            var openTask = this.ProcessRequestsAsync(this.processRequestsCancellation.Token);

            return Task.FromResult(this.publishUri);
        }

        private async Task ProcessRequestsAsync(CancellationToken processRequests)
        {
            while (!processRequests.IsCancellationRequested)
            {
                var request = await this.httpListener.GetContextAsync();

                // The ContinueWith forces rethrowing the exception if the task fails.
                Task requestTask = this.processRequest(request, this.processRequestsCancellation.Token)
                    .ContinueWith(
                        async t => await t /* Rethrow unhandled exception */,
                        TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}