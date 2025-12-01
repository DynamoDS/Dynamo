using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

/// <summary>
/// Discovers and registers web component DLLs that implement <see cref="WebComponentEntryPoint"/>.
/// Loads DLLs and registers their controllers, services, and static files with the proxy server.
/// </summary>
internal class WebComponentLoader
{
    private readonly DynamoViewModel dynamoViewModel;
    private readonly List<WebComponentEntryPoint> loadedEntryPoints = new List<WebComponentEntryPoint>();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebComponentLoader"/> class.
    /// </summary>
    /// <param name="dynamoViewModel">The DynamoViewModel instance to pass to web components.</param>
    public WebComponentLoader(DynamoViewModel dynamoViewModel)
    {
        this.dynamoViewModel = dynamoViewModel;
    }

    /// <summary>
    /// Gets the list of successfully loaded web component entry points.
    /// </summary>
    public IReadOnlyList<WebComponentEntryPoint> LoadedEntryPoints => loadedEntryPoints;

    /// <summary>
    /// Discovers and loads web component DLLs, registering their controllers and services
    /// with the web application builder. DLLs are loaded from the web-components directory
    /// alongside DynamoCoreWpf.dll.
    /// </summary>
    /// <param name="builder">The web application builder to register controllers and services with.</param>
    /// <returns>A task representing the asynchronous loading operation.</returns>
    public async Task LoadAndRegisterComponentsAsync(WebApplicationBuilder builder)
    {
        var webComponentsDirectory = GetWebComponentsDirectory();

        if (!Directory.Exists(webComponentsDirectory))
        {
            this.Log($"Web components directory does not exist: '{webComponentsDirectory}'");
            return;
        }

        var dllPaths = Directory.GetFiles(webComponentsDirectory, "*.dll", SearchOption.TopDirectoryOnly);

        if (dllPaths.Length == 0)
        {
            this.Log($"No DLLs found in '{webComponentsDirectory}'");
            return;
        }

        var mvcBuilder = builder.Services.AddControllers();
        var initializationParams = new InitializationParams(builder.Services, this.dynamoViewModel);

        // Load and register components sequentially
        foreach (var dllPath in dllPaths)
        {
            var entryPoint = await LoadComponentAsync(dllPath);
            if (entryPoint != null)
            {
                // Register controllers from this assembly and its services
                mvcBuilder.AddApplicationPart(entryPoint.Assembly);
                entryPoint.Initialize(initializationParams);
                loadedEntryPoints.Add(entryPoint);
            }
        }
    }

    /// <summary>
    /// Notifies all loaded web component entry points that Dynamo is shutting down.
    /// </summary>
    public void Shutdown()
    {
        foreach (var entryPoint in loadedEntryPoints)
        {
            try
            {
                entryPoint.Shutdown();
            }
            catch (Exception ex)
            {
                var componentName = entryPoint.GetType().Assembly.GetName().Name;
                this.Log($"Error shutting down {componentName}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Configures static file serving for all loaded web components.
    /// Static files are served from the wwwroot directory under the web-components directory.
    /// </summary>
    /// <param name="app">The web application to configure static files for.</param>
    public void ConfigureStaticFiles(WebApplication app)
    {
        var wwwrootPath = GetWwwRootPath();

        if (!Directory.Exists(wwwrootPath))
        {
            this.Log($"wwwroot directory does not exist: '{wwwrootPath}'");
            return;
        }

        foreach (var entryPoint in loadedEntryPoints)
        {
            try
            {
                var componentName = entryPoint.GetType().Assembly.GetName().Name;
                entryPoint.ConfigureStaticFiles(app, wwwrootPath);
            }
            catch (Exception ex)
            {
                var componentName = entryPoint.GetType().Assembly.GetName().Name;
                this.Log($"Error configuring static files for {componentName}: {ex.Message}");
            }
        }
    }

    private async Task<WebComponentEntryPoint?> LoadComponentAsync(string dllPath)
    {
        // Offload I/O and reflection work to thread pool
        return await Task.Run(() =>
        {
            var fileName = Path.GetFileName(dllPath);

            try
            {
                // Load the assembly
                var assembly = Assembly.LoadFrom(dllPath);

                // Find the entry point type
                var entryPointType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(WebComponentEntryPoint).IsAssignableFrom(t)
                                       && !t.IsInterface
                                       && !t.IsAbstract);

                if (entryPointType == null)
                {
                    return null;
                }

                // Instantiate the entry point
                var entryPoint = Activator.CreateInstance(entryPointType) as WebComponentEntryPoint;
                if (entryPoint == null)
                {
                    this.Log($"Failed to create entry point instance from {fileName}");
                    return null;
                }

                return entryPoint;
            }
            catch (Exception ex)
            {
                this.Log($"Error loading {fileName}: {ex.Message}");
                return null;
            }
        });
    }

    private string GetWebComponentsDirectory()
    {
        // Web components DLLs are deployed alongside DynamoCoreWpf.dll
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var baseDirectory = Path.GetDirectoryName(assemblyLocation);
        return Path.Combine(baseDirectory ?? string.Empty, "web-components");
    }

    private string GetWwwRootPath()
    {
        // Static files are in wwwroot directory under the web-components directory
        var webComponentsDirectory = GetWebComponentsDirectory();
        return Path.Combine(webComponentsDirectory, "wwwroot");
    }

    private void Log(string message)
    {
        Trace.WriteLine($"[WebComponentLoader] {message}");
    }
}
