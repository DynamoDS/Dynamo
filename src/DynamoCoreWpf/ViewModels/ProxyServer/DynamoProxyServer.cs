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
        private WebComponentLoader? componentLoader;
        private int port;
        private bool disposed = false;

        public int Port => this.port;

        public async Task StartAsync()
        {
            // Find an available port by letting the OS assign one, then releasing it
            this.port = FindAvailablePort();
            var builder = WebApplication.CreateBuilder();

            // Discover and register web component DLLs
            this.componentLoader = new WebComponentLoader();
            await this.componentLoader.LoadAndRegisterComponentsAsync(builder);

            // Build the application after registering all components
            var webApp = builder.Build();

            // Configure static file serving for all loaded web components
            this.componentLoader.ConfigureStaticFiles(webApp);

            // Map controllers
            webApp.MapControllers();

            // Diagnostics endpoint that returns JSON with version information
            // Diagnostics class is only instantiated when requested to avoid startup overhead
            webApp.MapGet("/", async (HttpContext context) =>
            {
                var diagnostics = new DynamoProxyServerDiagnostics(this.port, this.componentLoader);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(diagnostics.ToJson());
            });

            // Configure the server to listen on the discovered port
            webApp.Urls.Clear();
            webApp.Urls.Add($"http://localhost:{this.port}");

            this.app = webApp;

            await this.app.StartAsync();

            var serverUrl = $"http://localhost:{this.port}";
            this.Log($"Server started successfully on {serverUrl}");
        }

        public async Task StopAsync()
        {
            if (this.app != null && !this.disposed)
            {
                try
                {
                    await this.app.StopAsync();
                    await this.app.DisposeAsync();
                }
                catch (Exception ex)
                {
                    this.Log($"Error stopping server: {ex.Message}");
                }
                finally
                {
                    this.app = null;
                }
            }
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
                        this.Log("StopAsync timed out after 5 seconds");
                    }
                }
                catch (Exception ex)
                {
                    this.Log($"Error during Dispose: {ex.Message}");
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

        private void Log(string message)
        {
            Trace.WriteLine($"[DynamoProxyServer] {message}");
        }
    }
}
