using Dynamo.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;


namespace DynamoUtilities
{
    

    /// <summary>
    /// A wrapper around the DynamoFeatureFlags CLI tool.
    /// Which is itself a wrapper around LaunchDarkly.
    /// Not thread safe.
    /// </summary>
    internal class DynamoFeatureFlagsManager : CLIWrapper
    {
        private const string checkFeatureFlagCommandToken = @"<<<<<CheckFeatureFlag>>>>>";
        private string relativePath = Path.Combine("DynamoFeatureFlags", "DynamoFeatureFlags.exe");
        internal event Action<string> MessageLogged;
        public override void Dispose()
        {
            KillProcess();
        }
        /// <summary>
        /// Constructor
        /// Start the CLI tool and keep it around...
        /// </summary>
        internal DynamoFeatureFlagsManager(string userkey)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,

                UseShellExecute = false,
                Arguments = $"-u {userkey} -p {Process.GetCurrentProcess().Id}",
                FileName = GetToolPath(relativePath)
            };

            process.StartInfo = startInfo;
            try
            {
                process.Start();
                started = true;
            }
            catch (Win32Exception)
            {
                // Do nothing
            }
        }

        internal T CheckFeatureFlag<T>(string featureFlagKey, T defaultval)
        {
            if(!(defaultval is bool || defaultval is string)){
                MessageLogged?.Invoke("unsupported flag type");
                return defaultval;
            }
            if (!started)
            {
                 MessageLogged?.Invoke(GetCantStartErrorMessage());
                 return defaultval;
            }
            try
            {
                process.StandardInput.WriteLine(checkFeatureFlagCommandToken);
                process.StandardInput.WriteLine(featureFlagKey);
                process.StandardInput.WriteLine(defaultval);
                process.StandardInput.WriteLine(typeof(T).FullName);
                process.StandardInput.WriteLine(endOfDataToken);
            }
            catch (Exception e) when (e is IOException || e is ObjectDisposedException)
            {
                KillProcess();
                MessageLogged?.Invoke(GetCantCommunicateErrorMessage());
                return defaultval;
            }
            //TODO if we start moving more complex types than bool/string we should use
            //JSON (either newtonsoft or system.runtime.serializer which we already reference in this csproj).
            var dataFromCLI = GetData();
            //convert from string to string or bool.
            var output = Convert.ChangeType(dataFromCLI, typeof(T));

            return (T)output;
        }


        protected override string GetCantStartErrorMessage()
        {
            return $"$can't start { GetToolPath(relativePath)}";
        }

        protected override string GetCantCommunicateErrorMessage()
        {
            return $"$can't communicate with { GetToolPath(relativePath)}";
        }
    }
}
