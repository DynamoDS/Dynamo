using System;
using System.Configuration;
using System.Diagnostics;
using System.Xml;

namespace DynamoFeatureFlags
{
    /// <summary>
    /// An entry point for checking the state of Dynamo feature flags. Currently this wraps Launch Darkly,
    /// but should not expose any LD types.
    /// </summary>
    internal class FeatureFlagsManager: IDisposable
    {
        private LaunchDarkly.Sdk.User user;
        private static LaunchDarkly.Sdk.Client.LdClient ldClient;
        private const string sharedUserKey = "SHARED_DYNAMO_USER_KEY1";
        internal static event Action<string> MessageLogged;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userkey">key for a specific user, should be stable between sessions.</param>
        /// <param name="mobileKey">mobile sdk key, do not use full sdk key, if null, will load from config.</param>
        /// <exception cref="ArgumentException"></exception>
        internal FeatureFlagsManager(string userkey, string mobileKey = null)
        {
            var sw = new Stopwatch();
            sw.Start();
            //todo load sdk key from config if null
            if(mobileKey == null)
            {
                //load key from config depending on build config.
#if DEBUG
                var keystring = "ldmobilekey_dev";
#else
                var keystring = "ldmobilekey_prd";
#endif
                //don't use configuration manager as it is not net std 2 compliant.
                var path = $"{ GetType().Assembly.Location}.config";
                var key = GetConfigurationItem(keystring, path);
               
                if (key != null)
                {
                    mobileKey = key;
                }
            }
            //if mobile key is still null after loading from config, bail.
            if(mobileKey == null)
            {
                throw new ArgumentException("ld mobile key was null");
            }
            if (string.IsNullOrEmpty(userkey))
            {
                MessageLogged?.Invoke("The userkey was null when starting feature flag manager, using a shared key," +
                    " possibly analytics was disabled, test mode is active, or headless mode is active.");
                userkey = sharedUserKey;
            }
            //send user as anonymous//https://docs.launchdarkly.com/home/users/anonymous-users
            user = LaunchDarkly.Sdk.User.Builder(userkey).Anonymous(true).Build();

            Init(mobileKey);
            sw.Stop();
            MessageLogged?.Invoke($"startup time: {sw.ElapsedMilliseconds} ");
            MessageLogged?.Invoke("<<<<<InitDone>>>>>");
        }
        internal async void Init(string mobileKey)
        {
            //start up client.
            ldClient = await LaunchDarkly.Sdk.Client.LdClient.InitAsync(mobileKey, user);
            if (ldClient.Initialized)
            {
                MessageLogged?.Invoke($"launch darkly initalized");
            }
            else
            {
                MessageLogged?.Invoke($"launch darkly failed to initalize");
            }
        }

        // TODO as we need more flag types, implement cases here or break out
        // into more specific methods.
        /// <summary>
        /// Check the value of a specific flag. If the internal client is not initialized yet, will simply return the default value you provide.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flagkey">string key of the flag to lookup.</param>
        /// <param name="defaultval">value to return if there is an error reading the flag value.</param>
        /// <returns></returns>
        internal static T CheckFeatureFlag<T>(string flagkey,T defaultval)
        {
            try
            {
                if (ldClient == null || !ldClient.Initialized)
                {
                    MessageLogged?.Invoke($"feature flags client not initalized for requested flagkey: {flagkey}, returning default value: {defaultval}");
                    return defaultval;
                }

                object output = default(T);
                switch (default(T))
                {
                    case bool _:
                        output = ldClient.BoolVariation(flagkey);
                        break;
                    case string _:
                        output = ldClient.StringVariation(flagkey, defaultval as string);
                        break;
                }
                return (T)output;
            }
            catch(Exception ex)
            {
                MessageLogged?.Invoke($"failed to check feature flag key ex: {ex},{System.Environment.NewLine} returning default value: {defaultval}");
                return defaultval;
            }
        }

        /// <summary>
        /// A simplified version of the generic method above, that is simpler to call with dynamic types.
        /// </summary>
        /// <param name="flagkey"></param>
        /// <param name="t"></param>
        /// <param name="defaultval"></param>
        /// <returns></returns>
        internal static object CheckFeatureFlag(string flagkey,Type t, object defaultval)
        {
            try
            {
                if (ldClient == null || !ldClient.Initialized)
                {
                    MessageLogged?.Invoke($"feature flags client not initalized for requested flagkey: {flagkey}, returning default value: {defaultval}");
                    return defaultval;
                }

                object output = defaultval;
                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.Boolean:
                        output = ldClient.BoolVariation(flagkey);
                        break;
                    case TypeCode.String:
                        output = ldClient.StringVariation(flagkey, defaultval as string);
                        break;
                }
                return output;
            }
            catch (Exception ex)
            {
                MessageLogged?.Invoke($"failed to check feature flag key ex: {ex},{System.Environment.NewLine} returning default value: {defaultval}");
                return defaultval;
            }
        }

        /// <summary>
        /// cleanup.
        /// </summary>
        public void Dispose()
        {
            //https://github.com/launchdarkly/dotnet-client-sdk/issues/8#issuecomment-1092278630
            ldClient.Dispose();
        }

        private static string GetConfigurationItem(String key,string configFilePath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(configFilePath);
                if (doc != null)
                {
                    XmlNode node = doc.SelectSingleNode("//appSettings");

                    XmlElement value = (XmlElement)node.SelectSingleNode(string.Format("//add[@key='{0}']", key));
                    return value.Attributes["value"].Value;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The referenced configuration item, {0}, could not be retrieved", key);
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
