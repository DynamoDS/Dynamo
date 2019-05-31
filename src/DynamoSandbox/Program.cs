using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DynamoSandbox
{
    internal class Program
    {
        private static string dynamopath;

        [STAThread]
        public static void Main(string[] args)
        {   
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            //Display a message box and exit the program if Dynamo Core is unresolved.
            if (string.IsNullOrEmpty(DynamoCorePath)) return;

            //Include Dynamo Core path in System Path variable for helix to load properly.
            UpdateSystemPathForProcess();

            var setup = new DynamoCoreSetup(args);
            var app = new Application();
            setup.RunApplication(app);
        }

        /// <summary>
        /// Handler to the ApplicationDomain's AssemblyResolve event.
        /// If an assembly's location cannot be resolved, an exception is
        /// thrown. Failure to resolve an assembly will leave Dynamo in 
        /// a bad state, so we should throw an exception here which gets caught 
        /// by our unhandled exception handler and presents the crash dialogue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyPath = string.Empty;
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";

            try
            {
                assemblyPath = Path.Combine(DynamoCorePath, assemblyName);
                if (File.Exists(assemblyPath))
                    return Assembly.LoadFrom(assemblyPath);

                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                assemblyPath = Path.Combine(assemblyDirectory, assemblyName);
                return (File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The location of the assembly, {0} could not be resolved for loading.", assemblyName), ex);
            }
        }

        /// <summary>
        /// Returns the path of Dynamo Core installation.
        /// </summary>
        public static string DynamoCorePath
        {
            get
            {
                if (string.IsNullOrEmpty(dynamopath))
                {
                    dynamopath = GetDynamoCorePath();

                    if (string.IsNullOrEmpty(dynamopath))
                    {
                        NotifyUserDynamoCoreUnresolved();
                    }
                }
                return dynamopath;
            }
        }
        
        /// <summary>
        /// Finds the Dynamo Core path by looking into registery or potentially a config file.
        /// </summary>
        /// <returns>The root folder path of Dynamo Core.</returns>
        private static string GetDynamoCorePath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var dynamoRoot = Path.GetDirectoryName(assembly.Location);

            try
            {
                return DynamoInstallDetective.DynamoProducts.GetDynamoPath(version, dynamoRoot);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Add Dynamo Core location to the PATH system environment variable.
        /// This is to make sure dependencies (e.g. Helix assemblies) can be located.
        /// </summary>
        private static void UpdateSystemPathForProcess()
        {
            var path =
                    Environment.GetEnvironmentVariable(
                        "Path",
                        EnvironmentVariableTarget.Process) + ";" + DynamoCorePath;
            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
        }
        
        /// <summary>
        /// If Dynamo Sandbox fails to acquire Dynamo Core path, show a dialog that
        /// redirects to download DynamoCore.msi, and the program should exit.
        /// </summary>
        private static void NotifyUserDynamoCoreUnresolved()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var shortversion = version.Major + "." + version.Minor;

            // Hard-coding the strings in English, since in order to access the
            // Resources files we would need prior resolution to Dynamo Core itself
            if (MessageBoxResult.OK ==
                MessageBox.Show(
                    string.Format(
                        "Dynamo Sandbox {0} is not able to find an installation of " +
                        "Dynamo Core version {0} or higher.\n\nWould you like to download the " +
                        "latest version of DynamoCore.msi from http://dynamobim.org now?", shortversion),
                    "Dynamo Core component missing",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error))
            {
                Process.Start("http://dynamobim.org/download/");
            }
        }
    }
}
