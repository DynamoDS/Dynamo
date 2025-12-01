using Dynamo.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

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
    /// Gets the DynamoViewModel instance, providing access to the view model and model.
    /// </summary>
    public DynamoViewModel DynamoViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializationParams"/> class.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="dynamoViewModel">The DynamoViewModel instance.</param>
    public InitializationParams(IServiceCollection services, DynamoViewModel dynamoViewModel)
    {
        Services = services;
        DynamoViewModel = dynamoViewModel;
    }
}
