using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

/// <summary>
/// Base controller for all Dynamo web component controllers.
/// Provides default service info endpoint at GET /api/{component-name}
/// Routes are automatically derived from ComponentName:
/// - UI route: /{component-name}/
/// - API route: /api/{component-name}
/// </summary>
[ApiController]
public abstract class DynamoWebControllerBase : ControllerBase
{
    /// <summary>
    /// Gets the component name (e.g., "console-panel", "library")
    /// </summary>
    protected abstract string ComponentName { get; }

    /// <summary>
    /// Gets the UI route path, automatically derived from ComponentName
    /// </summary>
    protected string UiRoute => $"/{ComponentName}/"; // with trailing slash

    /// <summary>
    /// Gets the API route path, automatically derived from ComponentName
    /// </summary>
    protected string ApiRoute => $"/api/{ComponentName}";

    /// <summary>
    /// Default GET endpoint that returns service information.
    /// Override this method in derived controllers to provide custom behavior.
    /// </summary>
    [HttpGet]
    public virtual IActionResult Get()
    {
        var assembly = GetType().Assembly; // The derived controller's assembly
        var assemblyName = assembly.GetName();

        var assemblyFileName = !string.IsNullOrEmpty(assembly.Location)
            ? Path.GetFileName(assembly.Location)
            : assemblyName.Name;

        return Ok(new
        {
            name = ComponentName,
            version = assemblyName.Version?.ToString() ?? "1.0.0",
            status = "active",
            assembly = assemblyFileName,
            endpoints = new
            {
                ui = UiRoute,
                api = ApiRoute
            }
        });
    }
}
