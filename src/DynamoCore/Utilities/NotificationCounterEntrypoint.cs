using System;
using System.IO;
using System.Reflection;

namespace Dynamo.Utilities
{
    internal static class NotificationCounterEntrypoint
    {
        /// <summary>
        /// Tries to initialize the NotificationCounter class.
        /// </summary>
        /// <returns>true if debug mode 'NotificationCounter' is turned on and if Harmony.dll was successfully loaded. Returns false otherwise</returns>
        internal static void Initialize()
        {
            string location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NotificationCounter.dll");
            if (!File.Exists(location))
                return;

            try
            {
                var assem = Assembly.LoadFrom(location);
                var tp = assem.GetType("Dynamo.Utilities.NotificationCounter");
                var method = tp .GetMethod("StartCounting", BindingFlags.Static | BindingFlags.NonPublic);
                method.Invoke(null, []);
            }
            catch
            {
            }
        }
    }
}
