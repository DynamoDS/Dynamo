using Microsoft.Extensions.DependencyInjection;

namespace Dynamo.Wpf.ViewModels.ProxyServer
{
    /// <summary>
    /// Parameters provided to view DLLs during initialization for registering services
    /// and configuring the proxy server.
    /// </summary>
    public class InitializationParams
    {
        /// <summary>
        /// Gets the service collection for registering services with dependency injection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationParams"/> class.
        /// </summary>
        /// <param name="services">The service collection to register services with.</param>
        public InitializationParams(IServiceCollection services)
        {
            Services = services;
        }
    }
}
