using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamo.Utilities
{
    /// <summary>
    /// Utility class for network connectivity operations
    /// </summary>
    internal static class NetworkUtilities
    {
        private static HttpClient httpClient;
        private static string[] endPoints;
        private static CancellationTokenSource cancelationToken;

        /// <summary>
        /// Cancels any ongoing internet connectivity checks.
        /// </summary>
        internal static void InitInternetCheck()
        {
            httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            endPoints = new string[]{ "https://www.google.com", "https://www.microsoft.com" };
            cancelationToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancels any ongoing internet connectivity checks.
        /// </summary>
        internal static void StopInternetCheck()
        {
            cancelationToken.Cancel();
        }

        /// <summary>
        /// Checks online access by pinging reliable external services
        /// </summary>
        /// <returns>
        /// First return item is True if internet is accessible, false otherwise.
        /// Second item is True if the check completed, false if it was canceled ( we only cancel it on shutdown ).
        /// </returns>
        internal static async Task<(bool, bool)> CheckOnlineAccessAsync()
        {
            bool online = false;
            foreach (var endpoint in endPoints)
            {
                var result = await PingUrlAsync(httpClient, endpoint);
                if (!result.Item2)
                {
                    return (online, false);
                }

                if (result.Item1)
                {
                    online = true;
                }
            }
                    
            return (online, true);
        }

        /// <summary>
        /// Pings a URL using HEAD request only (no content download).
        /// </summary>
        /// <param name="client">HTTP client to use</param>
        /// <param name="url">URL to ping</param>
        /// <returns>
        /// First item : True if ping successful, false otherwise.
        /// Second item : True if the check completed, false if it was canceled.
        /// </returns>
        internal static async Task<(bool, bool)> PingUrlAsync(HttpClient client, string url)
        {
            try
            {
                // Use HEAD request only - no content download
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                using (var response = await client.SendAsync(request, cancelationToken.Token))
                {
                    return (response.IsSuccessStatusCode, true);
                }
            }
            catch(OperationCanceledException ex)
            {
                return ex.CancellationToken == cancelationToken.Token ? (false, false) : (false, true);
            }
            catch (Exception)
            {
                return (false, true);
            }
        }
    }
}
