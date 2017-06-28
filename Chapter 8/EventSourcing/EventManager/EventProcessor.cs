namespace EventManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;

    using Contracts;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using NEventStore;

    public class EventProcessor
    {
        private readonly string connectionString;

        public EventProcessor(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<JObject> Process(DomainEvent @event)
        {
            JObject result;
            using (var scope = new TransactionScope())
            {
                using (var store = Initializtion.InitEventStore(this.connectionString))
                {
                    result = await @event.Process(); // Allows the entity to carry out processing requirements such as removing article from inventory
                    var streamId = @event.Id;
                    using (var stream = store.OpenStream(streamId, 0))
                    {
                        stream.Add(new EventMessage { Body = JsonConvert.SerializeObject(@event) });
                        stream.CommitChanges(Guid.NewGuid());
                    }
                }

                scope.Complete();
            }

            return result;
        }

        public List<string> ReadStream(string streamId)
        {
            var response = new List<string>();
            var resolvedEvents = new List<EventMessage>();
            using (var store = Initializtion.InitEventStore(this.connectionString))
            {
                using (var stream = store.OpenStream(streamId, 0))
                {
                    resolvedEvents = stream.CommittedEvents.ToList();
                    foreach (var @event in resolvedEvents)
                    {
                        response.Add(JObject.Parse(@event.Body.ToString()).Property("Message").Value.ToString());
                    }
                }
            }

            return response;
        }
    }
}