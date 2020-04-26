using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace Dynamo.Configuration
{
    internal class DebugModes
    {
        static readonly Dictionary<string, DebugMode> debugModes;
        private static bool DebugModesEnabled;
 
        public class DebugMode
        {
            public string Name;
            public string Description;
            public bool Enabled;
        }

        static void AddDebugMode(string name, string description)
        {
            debugModes[name] = new DebugMode()
            {
                Name = name,
                Description = description,
                Enabled = false
            };
        }
        static void RegisterDebugModes()
        {
            // Add new debug modes here!!
            //
            // Example:
            // AddDebugMode("TestDebugMode", "Enabe/disable TestDebugMode.");
        }

        static DebugModes()
        {
            debugModes = new Dictionary<string, DebugMode>();
            DebugModesEnabled = false;
            try
            {
                RegisterDebugModes();

                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                AppSettingsSection section = (AppSettingsSection)config.GetSection("DebugModes");

                if (section == null) { return; }

                DebugModesEnabled = true;
                foreach (var key in section.Settings.AllKeys)
                {
                    if (!debugModes.ContainsKey(key)) { continue; }

                    debugModes[key].Enabled = bool.TryParse(section.Settings[key].Value, out bool enabled) ? enabled : false;
                }
            }
            catch (Exception)
            { }
        }

        public static bool Enabled(string name)
        {
            return DebugModesEnabled && debugModes.TryGetValue(name, out DebugMode dMode) ? dMode.Enabled : false;
        }
    }
}
