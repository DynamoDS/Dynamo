using Microsoft.Extensions.Configuration;

namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to handle appsettings.json
    /// </summary>
    internal static class ConfigHelper
    {
        private static IConfigurationRoot config;
        public static IConfigurationRoot GetConfiguration()
        {
            return config ??= new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
        }
    }
}
