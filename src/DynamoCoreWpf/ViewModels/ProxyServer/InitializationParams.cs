using Dynamo.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

/// <summary>
/// Parameters provided to view DLLs during initialization for registering services
/// and configuring the proxy server.
/// </summary>
public class InitializationParams
{
    /// <summary>
    /// Gets or sets the service collection for registering services with dependency injection.
    /// </summary>
    public IServiceCollection Services { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DynamoViewModel instance, providing access to the view model and model.
    /// </summary>
    public DynamoViewModel DynamoViewModel { get; set; } = null!;

    /// <summary>
    /// Gets or sets the callback for publishing events to connected WebView2 instances
    /// via SSE. Controllers can use this to notify frontend components of state changes.
    /// </summary>
    public Func<string, string, ValueTask> PublishEventAsync { get; set; } = null!;
}
