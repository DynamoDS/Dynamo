using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using net.riversofdata.dhlogger;

namespace Dynamo.Services
{
    /// <summary>
    /// Interception class to handle whether logging is called or not
    /// </summary>
    public class InstrumentationLogger
    {
        public static void Start()
        {
            userID = GetUserID();
            sessionID = Guid.NewGuid().ToString();
            loggerImpl = new Log("Dynamo", userID, sessionID);
            heartbeat = Heartbeat.GetInstance();

        }


       static string userID = GetUserID();
       static string sessionID = Guid.NewGuid().ToString();
       private static bool loggingEnabled = true;
        private static Log loggerImpl;
        private static Heartbeat heartbeat;


        private static String GetUserID()
        {
            // The name of the key must include a valid root.
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\DynamoUXG";
            const string keyName = userRoot + "\\" + subkey;

            // An int value can be stored without specifying the
            // registry data type, but long values will be stored
            // as strings unless you specify the type. Note that
            // the int is stored in the default name/value
            // pair.

            String tryGetValue = Registry.GetValue(keyName, "InstrumentationGUID", null) as String;

            if (tryGetValue != null)
            {
                System.Diagnostics.Debug.WriteLine("User id found: " + tryGetValue);
                return tryGetValue;
            }
            else
            {
                String newGUID = Guid.NewGuid().ToString();
                Registry.SetValue(keyName, "InstrumentationGUID", newGUID);
                System.Diagnostics.Debug.WriteLine("New User id: " + newGUID);
                return newGUID;
            }
        }


        

        public static void EnableLogging(bool enable)
        {
            loggingEnabled = enable;
        }

        public static void LogInfo(string tag, string data)
        {
            if (!loggingEnabled)
                return;

            loggerImpl.Info(tag, data);
        }

        public static void LogDebug(string tag, string data)
        {
            if (!loggingEnabled)
                return;

            loggerImpl.Debug(tag, data);
        }

        public static void LogPerf(string tag, string data)
        {
            if (!loggingEnabled)
                return;

            loggerImpl.Info("Perf-" + tag, data);
        }

        public static void LogError(string tag, string data)
        {
            if (!loggingEnabled)
                return;

            loggerImpl.Error(tag, data);
        }

    }

}
