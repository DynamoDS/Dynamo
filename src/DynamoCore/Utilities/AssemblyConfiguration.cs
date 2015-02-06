using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     A tool for obtaining configuration values from the current Library.  
    /// </summary>
    internal sealed class AssemblyConfiguration
    {
        private readonly Configuration configuration;

        private static AssemblyConfiguration instance;

        internal static AssemblyConfiguration Instance
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

        internal string GetAppSetting(string key)
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
