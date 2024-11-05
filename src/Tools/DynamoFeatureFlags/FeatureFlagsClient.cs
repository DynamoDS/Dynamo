using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using LaunchDarkly.Sdk;

namespace DynamoFeatureFlags
{
    /// <summary>
    /// An entry point for checking the state of Dynamo feature flags. Currently this wraps Launch Darkly,
    /// but should not expose any LD types.
    /// </summary>
    internal class FeatureFlagsClient : IDisposable
    {
        private LaunchDarkly.Sdk.User user;
        private static LaunchDarkly.Sdk.Client.LdClient ldClient;
        /// <summary>
        /// shared key, when stable key is not provided.
        /// </summary>
        private const string sharedUserKey = "SHARED_DYNAMO_USER_KEY1";
        /// <summary>
        /// Event for clients to attach to console log.
        /// </summary>
        internal static event Action<string> MessageLogged;
        /// <summary>
        /// Cache of all user's flags in internal as LDtype. Needs to be converted to primitive object before
        /// being handed to clients.
        /// </summary>
        private LaunchDarkly.Sdk.LdValue AllFlags { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userkey">key for a specific user, should be stable between sessions.</param>
        /// <param name="mobileKey">mobile sdk key, do not use full sdk key, if null, will load from config.</param>
        /// <param name="testMode">if true, will not call ld APIs, will return hardcoded data.</param>
        /// <exception cref="ArgumentException"></exception>
        internal FeatureFlagsClient(string userkey, string mobileKey = null, bool testMode = false)
        {
           
            var sw = new Stopwatch();
            sw.Start();
            //todo load sdk key from config if null
            if (mobileKey == null)
            {
                //if test mode just use a mock key
                if (testMode)
                {
                    mobileKey = "hardcoded_test_mobilekey";
                }
                else
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
            }
            //if mobile key is still null after loading from config, bail.
            if (mobileKey == null)
            {
                throw new ArgumentException("ld mobile key was null");
            }
            if (string.IsNullOrEmpty(userkey))
            {
                MessageLogged?.Invoke("The userkey was null when starting feature flag manager, using a shared key," +
                    " possibly analytics was disabled, test mode is active, or headless mode is active.");
                userkey = sharedUserKey;
            }
            if (testMode)
            {
                MessageLogged?.Invoke($"LD startup: testmode true, no LD connection. ");
                MessageLogged?.Invoke($"LD startup time: {sw.ElapsedMilliseconds} ms ");
                AllFlags = LdValue.ObjectFrom(new Dictionary<string,LdValue> { { "TestFlag1",LdValue.Of(true) },
                    { "TestFlag2", LdValue.Of("I am a string") },
                    //in tests we want instancing on so we can test it.
                    { "graphics-primitive-instancing", LdValue.Of(true) },
                    { "IsolatePackages", LdValue.Of("Package1,Package2,Package") },
                    { "DoNotIsolatePackages", LdValue.Of("Package") }
                });
                return;
            }

            //send user as anonymous//https://docs.launchdarkly.com/home/users/anonymous-users
            user = LaunchDarkly.Sdk.User.Builder(userkey).Anonymous(true).Build();

            Init(mobileKey);
            sw.Stop();
            MessageLogged?.Invoke($"Launch Darkly startup time: {sw.ElapsedMilliseconds} ms");
            //gather all the user's flags and create a top level ldvalue object containing all of them.
            if (ldClient.Initialized)
            {
                AllFlags = LdValue.ObjectFrom(new ReadOnlyDictionary<string, LaunchDarkly.Sdk.LdValue>(ldClient.AllFlags()));
            }

        }
        internal void Init(string mobileKey)
        {
            //start up client.
            ldClient =  LaunchDarkly.Sdk.Client.LdClient.Init(mobileKey, LaunchDarkly.Sdk.Client.ConfigurationBuilder.AutoEnvAttributes.Disabled, user, TimeSpan.FromSeconds(5));
            if (ldClient.Initialized)
            {
                MessageLogged?.Invoke($"Launch Darkly initalized");
            }
            else
            {
                MessageLogged?.Invoke($"Launch Darkly failed to initalize");
            }
        }

        internal string GetAllFlagsAsJSON()
        {
            return AllFlags.ToJsonString();
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
