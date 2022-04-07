using System;
using System.Configuration;

namespace DynamoFeatureFlags
{
    //TODO make internal
    public class FeatureFlagsManager:IDisposable
    {
        private LaunchDarkly.Sdk.User user;
        private static LaunchDarkly.Sdk.Client.LdClient ldClient;
        public static event Action<string> LogRequest; 
        public FeatureFlagsManager(string userkey, string mobileKey = null)
        {
            //todo load sdk key from config if null
            if(mobileKey == null)
            {
                //load from config
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
            user = LaunchDarkly.Sdk.User.Builder(userkey).Anonymous(true).Build();
            Init(mobileKey);
        }

        internal async void Init(string mobileKey)
        {
            //start up client.
            //TODO timeout?
            ldClient = await LaunchDarkly.Sdk.Client.LdClient.InitAsync(mobileKey, user);
            if (ldClient.Initialized)
            {
                LogRequest($"launch darkly initalized");
            }
        }

        // TODO as we need more flag types, implement cases here or break out
        // into more specific methods.
        public static T CheckFeatureFlag<T>(string flagkey,T defaultval)
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
                case string _ :
                    output = ldClient.StringVariation(flagkey,defaultval as string);
                    break;
            }
            return (T)output;
        }

        public void Dispose()
        {
            //TODO unclear how this should work in Hosts that can start Dynamo multiple times - 
            //needs to be tested.
            ldClient.Dispose();
        }

    }
}
