using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dynamo.Utilities
{
    /// <summary>
    /// Utility class for network connectivity operations
    /// </summary>
    internal static class NetworkUtilities
    {
        private static readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
        private static readonly string[] endPoints = { "https://www.google.com",  "https://www.microsoft.com" };
        /// <summary>
        /// Checks online access by pinging reliable external services
        /// </summary>
        /// <returns>True if network is accessible, false otherwise</returns>
        internal static async Task<bool> CheckOnlineAccessAsync()
        {
            try
            {
                foreach (var endpoint in endPoints)
                {
                    if (await PingUrlAsync(httpClient, endpoint))
                    {
                        return true;
                    }
                }
                    
                return false;
            }
            catch
            {
                // If any exception occurs, assume no connectivity
                return false;
            }
        }

        /// <summary>
        /// Pings a URL using HEAD request only (no content download).
        /// </summary>
        /// <param name="client">HTTP client to use</param>
        /// <param name="url">URL to ping</param>
        /// <returns>True if ping successful, false otherwise</returns>
        internal static async Task<bool> PingUrlAsync(HttpClient client, string url)
        {
            try
            {
                // Use HEAD request only - no content download
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                using (var response = await client.SendAsync(request))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
