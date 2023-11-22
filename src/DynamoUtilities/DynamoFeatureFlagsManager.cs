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
        /// <summary>
        /// set to true after the flags are retrieved from launch darkly.
        /// </summary>
        private bool flagsCacheAttempted = false;
        /// <summary>
        /// set to true after a null cache is logged.
        /// </summary>
        private bool nullCacheLogged = false;

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
            var dataFromCLI = GetData(2000).Result;    
            //convert from json string to dictionary.
            try
            {
                AllFlagsCache = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataFromCLI);
                //invoke the flags retrieved event on the sync context which should be the main ui thread.
                syncContext?.Send((_) =>
                {
                    FlagsRetrieved?.Invoke();

                }, null);

            }
            catch (Exception e)
            {
                RaiseMessageLogged($"{e?.Message}");
            }
            finally
            {
                flagsCacheAttempted = true;
            }
        }

        /// <summary>
        /// Check feature flag value, if not exist, return defaultval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="featureFlagKey"></param>
        /// <param name="defaultval"></param>
        /// <returns></returns>
        internal T CheckFeatureFlag<T>(string featureFlagKey, T defaultval)
        {
            if(!(defaultval is bool || defaultval is string)){
                RaiseMessageLogged("unsupported flag type");
                return defaultval;
            }
            // if we have not retrieved flags from the cli return empty
            // and log once,
           
            if(AllFlagsCache == null)
            {
                switch (flagsCacheAttempted)
                {
                    case true when !nullCacheLogged:
                        RaiseMessageLogged("The flags cache is null, something went wrong retrieving feature flags. This message will not be logged again, and future calls to CheckFeatureFlags will return default values.");
                        nullCacheLogged = true;
                        break;
                    case false:
                        RaiseMessageLogged("Please wait longer for the cache to populate before checking a flag state, you can use the static FlagsRetrieved event for this purpose.");
                        break;
                }

                return defaultval;
            }
            if (AllFlagsCache.ContainsKey(featureFlagKey))
            {
                return (T)AllFlagsCache[featureFlagKey];
            }
            else
            {
                RaiseMessageLogged($"failed to get value for feature flag key ex: {featureFlagKey},{System.Environment.NewLine} returning default value: {defaultval}");
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
