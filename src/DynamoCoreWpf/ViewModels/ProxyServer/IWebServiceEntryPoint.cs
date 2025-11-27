using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamo.Wpf.ViewModels.ProxyServer
{
    /// <summary>
    /// Entry point interface for view DLLs to register controllers, services,
    /// and static files with the proxy server.
    /// </summary>
    public interface IWebServiceEntryPoint
    {
        /// <summary>
        /// Gets the assembly containing controllers for this view.
        /// </summary>
        Assembly Assembly { get; }

        /// <summary>
        /// Registers services for this view with the dependency injection container.
        /// </summary>
        /// <param name="initializationParams">The initialization parameters.</param>
        void Initialize(InitializationParams initializationParams);

        /// <summary>
        /// Configures static file serving for this view's web assets.
        /// </summary>
        /// <param name="app">The web application to configure static files for.</param>
        /// <param name="wwwrootPath">The base path to the wwwroot directory containing
        /// static files.</param>
        void ConfigureStaticFiles(WebApplication app, string wwwrootPath);
    }
}
