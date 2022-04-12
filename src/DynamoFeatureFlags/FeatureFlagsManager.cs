using System;
using System.Configuration;
using System.Xml;

namespace DynamoFeatureFlags
{
    //TODO make internal
    public class FeatureFlagsManager:IDisposable
    {
        private LaunchDarkly.Sdk.User user;
        private static LaunchDarkly.Sdk.Client.LdClient ldClient;
        private const string sharedUserKey = "SHARED_DYNAMO_USER_KEY1";
        public static event Action<string> LogRequest; 
        public FeatureFlagsManager(string userkey, string mobileKey = null)
        {
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
            if (userkey == null)
            {
                LogRequest?.Invoke("The userkey was null when starting feature flag manager, using a shared key," +
                    " possibly analytics was disabled, test mode is active, or headless mode is active.");
                userkey = sharedUserKey;
            }
            //send user as anonymous//https://docs.launchdarkly.com/home/users/anonymous-users
            user = LaunchDarkly.Sdk.User.Builder(userkey).Anonymous(true).Build();
            Init(mobileKey);
        }

        internal async void Init(string mobileKey)
        {
            //start up client.
            ldClient = await LaunchDarkly.Sdk.Client.LdClient.InitAsync(mobileKey, user);
            if (ldClient.Initialized)
            {
                LogRequest?.Invoke($"launch darkly initalized");
            }
            else
            {
                LogRequest?.Invoke($"launch darkly failed to initalize");
            }
        }

        // TODO as we need more flag types, implement cases here or break out
        // into more specific methods.
        public static T CheckFeatureFlag<T>(string flagkey,T defaultval)
        {
            try
            {
                if (ldClient == null || !ldClient.Initialized)
                {
                    LogRequest($"feature flags client not initalized for requested flagkey: {flagkey}, returning default value: {defaultval}");
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
                LogRequest?.Invoke($"failed to check feature flag key ex: {ex},{System.Environment.NewLine} returning default value: {defaultval}");
                return defaultval;
            }
        }

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
