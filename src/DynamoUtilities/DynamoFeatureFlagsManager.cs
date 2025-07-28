using Dynamo.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoUtilities
{


    /// <summary>
    /// A wrapper around the DynamoFeatureFlags CLI tool.
    /// Which is itself a wrapper around LaunchDarkly.
    /// Not thread safe.
    /// </summary>
    internal class DynamoFeatureFlagsManager : CLIWrapper
    {
        private string relativePath = Path.Combine("DynamoFeatureFlags", "DynamoFeatureFlags.exe");
        private Dictionary<string, object> AllFlagsCache { get; set; }//TODO lock is likely overkill.
        private SynchronizationContext syncContext;
        internal static event Action FlagsRetrieved;
        
        //TODO(DYN-6464)- remove this field!.
        /// <summary>
        /// set to true after some FF issue is logged. For now we only log once to avoid clients overwhelming the logger.
        /// </summary>
        private bool loggedFFIssueOnce = false;
        /// <summary>
        /// Timeout in ms for feature flag communication with CLI process.
        /// </summary>
        private const int featureFlagTimeoutMs = 5000;

        /// <summary>
        /// Constructor
        /// Start the CLI tool and keep it around...
        /// </summary>
        /// <param name="userkey">non PII key to identify this user.</param>
        /// <param name="syncContext">context used for raising FlagRetrieved event.</param>
        /// <param name="testmode">will not contact feature flag service in testmode, will respond with defaults.</param>
        internal DynamoFeatureFlagsManager(string userkey, SynchronizationContext syncContext, bool testmode=false)
        {  
            this.syncContext = syncContext;
            //dont pass userkey arg if null/empty
            var userkeyarg = $"-u {userkey}";
            var testmodearg = string.Empty;
            if (string.IsNullOrEmpty(userkey))
            {
                userkeyarg = String.Empty;
            }
            if (testmode == true)
            {
                testmodearg = "-t";
            }
            var args = $"{userkeyarg} -p {Process.GetCurrentProcess().Id} {testmodearg}";
            StartProcess(relativePath, args);
        }

        internal void CacheAllFlags()
        {

            //wait for response
            var dataFromCLI = GetData(featureFlagTimeoutMs);    
            //convert from json string to dictionary.
            try
            {
                AllFlagsCache = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataFromCLI);
                // Invoke the flags retrieved event on the sync context which should be the main ui thread (if in Dynamo with UI) or the default SyncContext (if in headless mode).
                syncContext?.Send((_) =>
                {
                    FlagsRetrieved?.Invoke();

                }, null);
                var formattedFlags = JsonConvert.SerializeObject(AllFlagsCache, Formatting.Indented);
                OnLogMessage($"retrieved feature flags with value: {formattedFlags}");

            }
            catch (Exception e)
            {
                RaiseMessageLogged($"{e?.Message}");
            }
        }

        /// <summary>
        /// Check feature flag value, if it does not exist, return the defaultval.
        /// </summary>
        /// <typeparam name="T">Must be a bool, string or double, flags of only these types should be created unless this implementation is improved.</typeparam>
        /// <param name="featureFlagKey">feature flag name</param>
        /// <param name="defaultval">Currently the flag and default val MUST be a bool, or long(int64) or double.</param>
        /// <returns></returns>
        internal T CheckFeatureFlag<T>(string featureFlagKey, T defaultval)
        {
            if(!(defaultval is bool || defaultval is string || defaultval is long || defaultval is double)){
                throw new ArgumentException("unsupported flag type", defaultval.GetType().ToString());
            }
            // if we have not retrieved flags from the cli return empty
            // and log once.
           
            if(AllFlagsCache == null)
            {   //TODO(DYN-6464) Revisit this and log more when the logger is not easily overwhelmed.
                if (!loggedFFIssueOnce)
                {
                    RaiseMessageLogged(
                        $"The flags cache was null while checking {featureFlagKey}, something went wrong retrieving feature flags," +
                        " or you need to wait longer for the cache to populate before checking for flags, you can use the static FlagsRetrieved event for this purpose." +
                        "This message will not be logged again, and future calls to CheckFeatureFlags will return default values!!!");
                }

                loggedFFIssueOnce = true;
                return defaultval;
            }
            if (AllFlagsCache.TryGetValue(featureFlagKey, out var flagVal))
            {
                try
                {
                    if (typeof(T) == typeof(double) && flagVal is int intValue)
                    {
                        flagVal = Convert.ToDouble(intValue);
                    }
                    return (T)flagVal;
                }
                catch (InvalidCastException e)
                {
                    RaiseMessageLogged(
                        $"failed to cast feature flag value for {featureFlagKey} to {typeof(T)}, ex: {e.Message}, returning default value: {defaultval}");
                    return defaultval;
                }
            }
            else
            {
                if (!loggedFFIssueOnce)
                {
                    RaiseMessageLogged(
                        $"failed to get value for feature flag key ex: {featureFlagKey},{System.Environment.NewLine} returning default value: {defaultval}");
                }
                loggedFFIssueOnce = true;
                return defaultval;
            }
        }


        protected override string GetCantStartErrorMessage()
        {
            return $"can't start { GetToolPath(relativePath)}";
        }

        protected override string GetCantCommunicateErrorMessage()
        {
            return $"can't communicate with { GetToolPath(relativePath)}";
        }
    }
}
