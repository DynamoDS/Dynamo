using System;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace DynamoSandbox
{
    internal class Program
    {
        private static string dynamopath;

        [STAThread]
        public static void Main(string[] args)
        {   
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

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
                throw new Exception(string.Format("The location of the assembly, {0} could not be resolved for loading.", assemblyPath), ex);
            }
        }

        /// <summary>
        /// Gets the path of Dynamo Core installation.
        /// </summary>
        public static string DynamoCorePath
        {
            get
            {
                if (string.IsNullOrEmpty(dynamopath))
                {
                    dynamopath = GetDynamoCorePath();
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

            var installs = DynamoInstallDetective.DynamoProducts.FindDynamoInstallations(Path.GetDirectoryName(assembly.Location));
            if (installs == null) return string.Empty;

            return installs.Products
                .Where(p => p.VersionInfo.Item1 == version.Major && p.VersionInfo.Item2 == version.Minor)
                .Select(p => p.InstallLocation)
                .FirstOrDefault();
        }
    }
}
