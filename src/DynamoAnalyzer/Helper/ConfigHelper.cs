using Microsoft.Extensions.Configuration;

namespace DynamoAnalyzer.Helper
{
    public static class ConfigHelper
    {
        private static IConfigurationRoot _config;
        public static IConfigurationRoot GetConfiguration()
        {
            return _config ??= new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
        }
    }
}
