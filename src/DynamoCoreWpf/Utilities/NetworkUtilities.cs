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
        /// <summary>
        /// Checks online access by pinging reliable external services
        /// </summary>
        /// <returns>True if network is accessible, false otherwise</returns>
        internal static async Task<bool> CheckOnlineAccessAsync()
        {
            try
            {
                // Use a simple HTTP HEAD request to check connectivity
                // This is faster than GET and doesn't download content
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    return await PingUrlAsync(client, "https://www.autodesk.com");
                }
            }
            catch
            {
                // If any exception occurs, assume no connectivity
                return false;
            }
        }

        /// <summary>
        /// Pings a URL.
        /// </summary>
        /// <param name="client">HTTP client to use</param>
        /// <param name="url">URL to ping</param>
        /// <returns>True if ping successful, false otherwise</returns>
        internal static async Task<bool> PingUrlAsync(HttpClient client, string url)
        {
            try
            {
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
