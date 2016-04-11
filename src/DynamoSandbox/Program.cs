using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Reflection;
using System.IO;
using Dynamo.Controls;
using Dynamo.DynamoSandbox;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Applications;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace DynamoSandbox
{
    internal class Program
    {
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
                assemblyPath = Path.Combine(Dynamo.Applications.StartupUtils.DynamoCorePath, assemblyName);
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

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

        [DllImport("msvcrt.dll")]
        public static extern int _putenv(string env);

        [STAThread]
        public static void Main(string[] args)
        {
            
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
            var locale = StartupUtils.SetLocale(cmdLineArgs);
            _putenv(locale);

            var setup = new DynamoCoreSetup(cmdLineArgs.CommandFilePath);
            var app = new Application();
            setup.RunApplication(app);
            
        }
    }
}
