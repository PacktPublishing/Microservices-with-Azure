namespace CommerceService
{
    using System.Collections.Generic;
    using System.Fabric;

    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    ///     The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class CommerceService : StatelessService
    {
        private static StatelessServiceContext Context;

        public CommerceService(StatelessServiceContext context)
            : base(context)
        {
            Context = context;
        }

        internal static string GetConfigurationValue(string key)
        {
            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            return configurationPackage.Settings.Sections["ApplicationSettings"].Parameters[key].Value;
        }

        /// <summary>
        ///     Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
                {
                    new ServiceInstanceListener(
                        serviceContext =>
                            new OwinCommunicationListener(
                                Startup.ConfigureApp,
                                serviceContext,
                                ServiceEventSource.Current,
                                "ServiceEndpoint"))
                };
        }
    }
}