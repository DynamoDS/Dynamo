using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

/// <summary>
/// Base class for web component entry points to register controllers, services,
/// and static files with the proxy server.
/// </summary>
public abstract class WebComponentEntryPoint
{
    /// <summary>
    /// Gets the assembly containing controllers for this web component.
    /// </summary>
    public abstract Assembly Assembly { get; }

    /// <summary>
    /// Gets the component name (e.g., "console-panel", "library").
    /// Used to derive UI routes and static file paths.
    /// </summary>
    protected abstract string ComponentName { get; }

    /// <summary>
    /// Registers services for this web component with the dependency injection container.
    /// </summary>
    /// <param name="initializationParams">The initialization parameters.</param>
    public abstract void Initialize(InitializationParams initializationParams);

    /// <summary>
    /// Called when Dynamo is shutting down, allowing the web component to perform cleanup.
    /// </summary>
    public virtual void Shutdown()
    {
    }

    /// <summary>
    /// Configures static file serving for this web component's web assets.
    /// Default implementation serves files from wwwroot/{component-name}/ at /{component-name}/.
    /// Override this method to customize static file serving behavior.
    /// </summary>
    /// <param name="app">The web application to configure static files for.</param>
    /// <param name="wwwrootPath">The base path to the wwwroot directory containing
    /// static files.</param>
    public virtual void ConfigureStaticFiles(WebApplication app, string wwwrootPath)
    {
        var componentPath = Path.Combine(wwwrootPath, ComponentName);
        var fileProvider = new PhysicalFileProvider(componentPath);
        var requestPath = $"/{ComponentName}";

        // Configure default files (so /{component-name}/ serves index.html)
        var defaultFilesOptions = new DefaultFilesOptions
        {
            FileProvider = fileProvider,
            RequestPath = requestPath,
            DefaultFileNames = ["index.html"]
        };

        app.UseDefaultFiles(defaultFilesOptions);

        // Configure static file serving
        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = requestPath
        };

        app.UseStaticFiles(staticFileOptions);
    }
}
