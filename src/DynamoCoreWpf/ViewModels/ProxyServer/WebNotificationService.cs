using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Dynamo.Wpf.ViewModels.ProxyServer;

/// <summary>
/// Central pub/sub service for broadcasting events to all connected WebView2 instances via SSE.
/// Uses Channel&lt;T&gt; to distribute messages to all connected clients.
/// </summary>
internal class WebNotificationService
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    /// <summary>
    /// Publishes an event to all connected SSE clients.
    /// </summary>
    /// <param name="eventName">The event name (e.g., "library" or "global").</param>
    /// <param name="eventData">The event data (e.g., "library-modified" or "shutdown-started").</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    public ValueTask PublishAsync(string eventName, string eventData)
    {
        return _channel.Writer.WriteAsync($"{eventName}:{eventData}");
    }

    /// <summary>
    /// Listens for events and returns them as an async enumerable for SSE streaming.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop listening.</param>
    /// <returns>An async enumerable of events.</returns>
    public IAsyncEnumerable<string> ListenAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    /// <summary>
    /// Maps the SSE endpoint to the web application. Handles Server-Sent Events streaming
    /// for real-time communication from C# controllers to JavaScript clients.
    /// </summary>
    /// <param name="app">The web application to map the endpoint to.</param>
    public void MapSseEndpoint(WebApplication app)
    {
        app.MapGet("/sse", async (HttpContext context, WebNotificationService notifications) =>
        {
            context.Response.Headers["Content-Type"] = "text/event-stream";
            context.Response.Headers["Cache-Control"] = "no-cache";
            context.Response.Headers["Connection"] = "keep-alive";

            var ct = context.RequestAborted;

            try
            {
                await foreach (var msg in notifications.ListenAsync(ct))
                {
                    // Parse message format: "{eventName}:{eventData}"
                    // Examples: "library:library-modified", "global:shutdown-started"
                    var parts = msg.Split(':', 2);
                    var eventName = parts[0]; // e.g., "library" or "global"
                    var eventData = parts.Length > 1 ? parts[1] : "";

                    await context.Response.WriteAsync($"event: {eventName}\n", ct);
                    await context.Response.WriteAsync($"data: {eventData}\n\n", ct);
                    await context.Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected, which is expected
            }
            catch (Exception ex)
            {
                Log($"Error in SSE endpoint: {ex.Message}");
            }
        });
    }

    private void Log(string message)
    {
        Trace.WriteLine($"[WebNotificationService] {message}");
    }
}
