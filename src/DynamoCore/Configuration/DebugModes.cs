﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Provide functionality around debug modes. Similar to feature flags.
    /// </summary>
    internal static class DebugModes
    {
        static private readonly Dictionary<string, DebugMode> debugModes = new Dictionary<string, DebugMode>();

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
            public bool IsEnabled;
        }

        internal static void AddDebugMode(string name, string description, bool isEnabled = false)
        {
            debugModes[name] = new DebugMode()
            {
                Name = name,
                Description = description,
                IsEnabled = isEnabled
            };
        }

        private static void RegisterDebugModes()
        {
            // Register app wide new debug modes here.
            AddDebugMode("DynamoPreferencesMenuDebugMode", "Enable/Disable the Preferences Panel new features in the Dynamo menu.", false);
            AddDebugMode("DynamoPackageStates", "Enable/Disable package states in the InstalledPackages UI.", false);
        }

        internal static void LoadDebugModesStatusFromConfig(string configPath)
        {
            try
            {
                XmlDocument xmlDoc;
                Exception ex;
                if (DynamoUtilities.PathHelper.isValidXML(configPath, out xmlDoc, out ex))
                {
                    var debugItems = xmlDoc.DocumentElement.SelectNodes("DebugMode");
                    foreach (XmlNode item in debugItems)
                    {
                        var name = item.Attributes["name"].Value;
                        bool enabled;
                        Boolean.TryParse(item.Attributes["enabled"].Value, out enabled);

                        if (!debugModes.ContainsKey(name)) { continue; }
                        debugModes[name].IsEnabled = enabled;
                    }
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static DebugModes()
        {
            RegisterDebugModes();
            LoadDebugModesStatusFromConfig(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "debug.config"));
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
                dbgMode.IsEnabled = enabled;
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
        public static bool IsEnabled(string name)
        {
            DebugMode dbgMode;
            return debugModes.TryGetValue(name, out dbgMode) && dbgMode.IsEnabled;
        }
    }
}
