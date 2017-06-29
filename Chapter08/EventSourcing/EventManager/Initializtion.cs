namespace EventManager
{
    using NEventStore;
    using NEventStore.Persistence.Sql.SqlDialects;

    internal static class Initializtion
    {
        internal static IStoreEvents InitEventStore(string connectionString)
        {
            return
                Wireup.Init()
                    .UsingSqlPersistence("EMConnection", "System.Data.SqlClient", connectionString)
                    .WithDialect(new MsSqlDialect())
                    .EnlistInAmbientTransaction()
                    .InitializeStorageEngine()
                    .UsingJsonSerialization()
                    .Build();
        }
    }
}