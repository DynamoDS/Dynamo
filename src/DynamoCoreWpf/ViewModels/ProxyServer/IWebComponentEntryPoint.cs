using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamo.Wpf.ViewModels.ProxyServer
{
    /// <summary>
    /// Entry point interface for web component DLLs to register controllers, services,
    /// and static files with the proxy server.
    /// </summary>
    public interface IWebComponentEntryPoint
    {
        /// <summary>
        /// Gets the assembly containing controllers for this web component.
        /// </summary>
        Assembly Assembly { get; }

        /// <summary>
        /// Registers services for this web component with the dependency injection container.
        /// </summary>
        /// <param name="initializationParams">The initialization parameters.</param>
        void Initialize(InitializationParams initializationParams);

        /// <summary>
        /// Configures static file serving for this web component's web assets.
        /// </summary>
        /// <param name="app">The web application to configure static files for.</param>
        /// <param name="wwwrootPath">The base path to the wwwroot directory containing
        /// static files.</param>
        void ConfigureStaticFiles(WebApplication app, string wwwrootPath);
    }
}
