using System.Configuration;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     A tool for obtaining configuration values from the current Library.  
    /// </summary>
    public sealed class AssemblyConfiguration
    {
        private readonly Configuration configuration;

        private static AssemblyConfiguration instance;

        public static AssemblyConfiguration Instance
        {
            get { return instance ?? new AssemblyConfiguration(); }
        }

        private AssemblyConfiguration()
        {
            this.configuration = LoadConfiguration();
        }

        private Configuration LoadConfiguration()
        {
            var path = this.GetType().Assembly.Location;
            Configuration config;

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(path);
            }
            catch
            {
                return null;
            }

            return config;
        }

        public string GetAppSetting(string key)
        {
            if (configuration == null) return string.Empty;

            var element = configuration.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }
    }
}
