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
internal class DynamoProxyServerDiagnostics(int port, WebComponentLoader componentLoader)
{
    public object GetDiagnostics()
    {
        var diagnostics = new
        {
            status = "running",
            serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            serverUrl = $"http://localhost:{port}",
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

    private List<string> GetLoadedWebComponents()
    {
        return componentLoader.LoadedEntryPoints
            .Select(ep => ep.Assembly.Location)
            .ToList();
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string ToJson()
    {
        var diagnostics = GetDiagnostics();
        return JsonSerializer.Serialize(diagnostics, JsonOptions);
    }

    private static string GetDotNetFrameworkVersion()
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

    private static string GetAspNetCoreVersion()
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

    private static string GetDynamoCoreVersion()
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

    private static string GetDynamoCoreWpfVersion()
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

