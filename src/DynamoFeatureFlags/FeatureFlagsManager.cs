using System;
using System.Configuration;

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
                var path = GetType().Assembly.Location;
                var config = ConfigurationManager.OpenExeConfiguration(path);
                var key = config.AppSettings.Settings[keystring];
               
                if (key != null)
                {
                    mobileKey = key.Value;
                }

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

                Object output = default(T);
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
            //TODO unclear how this should work in Hosts that can start Dynamo multiple times - 
            //needs to be tested.//https://github.com/launchdarkly/dotnet-client-sdk/issues/8#issuecomment-1092278630
            //ldClient.Dispose();
        }

    }
}
