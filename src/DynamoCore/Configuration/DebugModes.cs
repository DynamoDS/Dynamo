using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Provide functionality around debug modes. Similar to feature flags.
    /// </summary>
    public class DebugModes
    {
        static private readonly Dictionary<string, DebugMode> debugModes;
        private static bool debugModesEnabled;

        /// <summary>
        /// Represents an instance of a debug mode
        /// </summary>
        public class DebugMode
        {
            /// <summary>
            /// Name of the debug mode
            /// </summary>
            public string Name;
            /// <summary>
            /// Description of the debug mode
            /// </summary>
            public string Description;
            /// <summary>
            /// Whether debug mode is enabled or not
            /// </summary>
            public bool Enabled;
        }
        private static void AddDebugMode(string name, string description)
        {
            debugModes[name] = new DebugMode()
            {
                Name = name,
                Description = description,
                Enabled = true
            };
        }
        private static void RegisterDebugModes()
        {
            // Add new debug modes here!!
            //
            // Example:
            // AddDebugMode("TestDebugMode", "Enabe/disable TestDebugMode.");
        }
        /// <summary>
        /// Static constructor
        /// </summary>
        static DebugModes()
        {
            debugModes = new Dictionary<string, DebugMode>();
            debugModesEnabled = false;
            try
            {
                RegisterDebugModes();

                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                AppSettingsSection section = (AppSettingsSection)config.GetSection("DebugModes");

                if (section == null) { return; }

                debugModesEnabled = true;
                foreach (var key in section.Settings.AllKeys)
                {
                    if (!debugModes.ContainsKey(key)) { continue; }

                    bool enabled = false;
                    debugModes[key].Enabled = Boolean.TryParse(section.Settings[key].Value, out enabled) ? enabled : false;
                }
            }
            catch (Exception)
            { }
        }
        /// <summary>
        /// Enables/Disables a debug mode
        /// </summary>
        /// <param name="name">Name of the debug mode</param>
        /// <param name="enabled">Enable/Disable debug mode</param>
        public static void SetDebugModeEnabled(string name, bool enabled)
        {
            DebugMode dbgMode;
            if (debugModes.TryGetValue(name, out dbgMode))
            {
                dbgMode.Enabled = enabled;
            }
        }
        /// <summary>
        /// Returns a dictionary of all the debug modes
        /// </summary>
        public static Dictionary<string, DebugMode> GetDebugModes()
        {
            return debugModes;
        }
        /// <summary>
        /// Retrieves a debug mode
        /// </summary>
        /// <param name="name">Name of the debug mode</param>
        public static DebugMode GetDebugMode(string name)
        {
            DebugMode dMode;
            return debugModes.TryGetValue(name, out dMode) ? dMode : null;
        }
        /// <summary>
        /// Retrieves the state of a debug mode (enabled/disabled)
        /// </summary>
        /// <param name="name">Name of the debug mode</param>
        public static bool Enabled(string name)
        {
            DebugMode dbgMode;
            return debugModesEnabled && debugModes.TryGetValue(name, out dbgMode) ? dbgMode.Enabled : false;
        }
    }
}
