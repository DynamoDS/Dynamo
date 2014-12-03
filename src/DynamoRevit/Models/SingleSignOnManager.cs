using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

using Greg;

namespace Dynamo.Applications
{
    internal static class SingleSignOnManager
    {
        /// <summary>
        ///     A reference to the the SSONET assembly to prevent reloading.
        /// </summary>
        private static Assembly singleSignOnAssembly;
        public static Dispatcher UIDispatcher { get; set; }

        /// <summary>
        ///     Delay loading of the SSONet.dll, which is used by the package manager for
        ///     authentication information.
        /// </summary>
        /// <returns>The SSONet assembly</returns>
        private static Assembly LoadSSONet()
        {
            // get the location of RevitAPI assembly.  SSONet is in the same directory.
            Assembly revitAPIAss = Assembly.GetAssembly(typeof(Autodesk.Revit.DB.XYZ));
            string revitAPIDir = Path.GetDirectoryName(revitAPIAss.Location);
            Debug.Assert(revitAPIDir != null, "revitAPIDir != null");

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = Path.Combine(revitAPIDir, "SSONET.dll");

            //Load the assembly from the specified path. 					
            return Assembly.LoadFrom(strTempAssmbPath);
        }

        /// <summary>
        ///     Callback for registering an authentication provider with the package manager
        /// </summary>
        /// <param name="client">The client, to which the provider will be attached</param>
        internal static void RegisterSingleSignOn(PackageManager.PackageManagerClient client)
        {
            singleSignOnAssembly = singleSignOnAssembly ?? LoadSSONet();
            client.Client.Provider = client.Client.Provider
                ?? new RevitOxygenProvider(new DispatcherSynchronizationContext(UIDispatcher));
        }

    }
}
