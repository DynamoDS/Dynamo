using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Dynamo.Wpf.ViewModels.ProxyServer
{
    /// <summary>
    /// Embedded ASP.NET Core proxy server for WebView2 communication.
    /// </summary>
    internal class DynamoProxyServer : IDisposable
    {
        private WebApplication? app;
        private int port;
        private bool disposed = false;

        public int Port => this.port;

        public async Task StartAsync()
        {
            // Find an available port by letting the OS assign one, then releasing it
            this.port = FindAvailablePort();
            var builder = WebApplication.CreateBuilder();
            var webApp = builder.Build();

            // Diagnostics endpoint that returns JSON with version information
            // Diagnostics class is only instantiated when requested to avoid startup overhead
            webApp.MapGet("/", async (HttpContext context) =>
            {
                var diagnostics = new DynamoProxyServerDiagnostics(this.port);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(diagnostics.ToJson());
            });

            // Configure the server to listen on the discovered port
            webApp.Urls.Clear();
            webApp.Urls.Add($"http://localhost:{this.port}");

            this.app = webApp;

            var serverUrl = $"http://localhost:{this.port}";
            Trace.WriteLine($"[DynamoProxyServer] Server starting on {serverUrl}");

            await this.app.StartAsync();

            Trace.WriteLine($"[DynamoProxyServer] Server started successfully on {serverUrl}");
        }

        public async Task StopAsync()
        {
            if (this.app != null && !this.disposed)
            {
                Trace.WriteLine($"[DynamoProxyServer] Stopping server on port {this.port}");
                try
                {
                    await this.app.StopAsync();
                    await this.app.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[DynamoProxyServer] Error stopping server: {ex.Message}");
                }
                finally
                {
                    this.app = null;
                }
            }
        }

        private int FindAvailablePort()
        {
            // Use port 0 to let the OS automatically assign an available port
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            // Get the assigned port number
            var endpoint = (System.Net.IPEndPoint)listener.LocalEndpoint;
            int assignedPort = endpoint.Port;

            // Release the port so we can use it
            listener.Stop();

            return assignedPort;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // Stop asynchronously with a timeout to avoid blocking shutdown
                try
                {
                    var stopTask = StopAsync();
                    if (!stopTask.Wait(TimeSpan.FromSeconds(5)))
                    {
                        Trace.WriteLine($"[DynamoProxyServer] StopAsync timed out after 5 seconds");
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[DynamoProxyServer] Error during Dispose: {ex.Message}");
                }
            }
        }
    }
}

