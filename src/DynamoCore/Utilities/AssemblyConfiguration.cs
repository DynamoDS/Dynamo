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
        private static Configuration _config;
        private static AssemblyConfiguration _assemConfig;

        private AssemblyConfiguration()
        {
        }

        internal static AssemblyConfiguration Instance
        {
            get { return _assemConfig ?? new AssemblyConfiguration(); }
        }

        private Configuration LoadConfiguration()
        {
            if (_config != null) return _config;

            var path = this.GetType().Assembly.Location;
            try
            {
                _config = ConfigurationManager.OpenExeConfiguration(path);
            }
            catch
            {
                return null;
            }

            return _config;
        }

        internal string GetAppSetting(string key)
        {
            var element = LoadConfiguration().AppSettings.Settings[key];
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
