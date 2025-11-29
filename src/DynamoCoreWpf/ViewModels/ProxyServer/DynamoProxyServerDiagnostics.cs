using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

/// <summary>
/// Provides diagnostics information for the proxy server. This class is only
/// instantiated when diagnostics are requested to avoid startup overhead.
/// </summary>
internal class DynamoProxyServerDiagnostics
{
    private readonly int port;
    private readonly WebComponentLoader? componentLoader;

    public DynamoProxyServerDiagnostics(int port, WebComponentLoader? componentLoader = null)
    {
        this.port = port;
        this.componentLoader = componentLoader;
    }

    public object GetDiagnostics()
    {
        var diagnostics = new
        {
            status = "running",
            serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            serverUrl = $"http://localhost:{this.port}",
            versions = new
            {
                dotNet = Environment.Version.ToString(),
                dotNetFramework = GetDotNetFrameworkVersion(),
                aspNetCore = GetAspNetCoreVersion(),
                dynamoCore = GetDynamoCoreVersion(),
                dynamoCoreWpf = GetDynamoCoreWpfVersion(),
                os = Environment.OSVersion.ToString(),
                machineName = Environment.MachineName
            },
            webComponents = GetLoadedWebComponents()
        };

        return diagnostics;
    }

    private IList<string> GetLoadedWebComponents()
    {
        if (this.componentLoader == null)
        {
            return Array.Empty<string>();
        }

        return this.componentLoader.LoadedEntryPoints
            .Select(ep => ep.Assembly.Location)
            .ToList();
    }

    public string ToJson()
    {
        var diagnostics = GetDiagnostics();
        return JsonSerializer.Serialize(diagnostics, new JsonSerializerOptions { WriteIndented = true });
    }

    private string GetDotNetFrameworkVersion()
    {
        try
        {
            return System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetAspNetCoreVersion()
    {
        try
        {
            var aspNetCoreAssembly = Assembly.Load("Microsoft.AspNetCore.App");
            var version = aspNetCoreAssembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            // Try alternative method
            try
            {
                var assembly = typeof(global::Microsoft.AspNetCore.Builder.WebApplication).Assembly;
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    private string GetDynamoCoreVersion()
    {
        try
        {
            var dynamoCoreAssembly = Assembly.Load("DynamoCore");
            var version = dynamoCoreAssembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Not found";
        }
    }

    private string GetDynamoCoreWpfVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}

