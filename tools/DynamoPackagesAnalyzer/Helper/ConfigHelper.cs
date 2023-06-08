using Microsoft.Extensions.Configuration;

namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to handle appsettings.json
    /// </summary>
    internal static class ConfigHelper
    {
        private static IConfigurationRoot config;
        internal static IConfigurationRoot GetConfiguration()
        {
            return config ??= new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
        }
    }
}
