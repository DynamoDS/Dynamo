using Dynamo.Applications;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;


namespace DynamoSandbox
{
    struct CommandLineArguments
    {
        internal static CommandLineArguments FromArguments(string[] args)
        {
            // Running Dynamo sandbox with a command file:
            // DynamoStudio.exe /c "C:\file path\file.xml"
            // 
            var commandFilePath = string.Empty;

            // Running Dynamo sandbox with a dyn/dyf file:
            // DynamoStudio.exe "C:\file path\file.dyn"
            //
            var startUpDynFilePath = string.Empty;

            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i];

                // Looking for '/c'
                if (arg.Length != 2 || (arg[0] != '/'))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(arg) && File.Exists(arg))
                        {
                            var extension = Path.GetExtension(arg).ToLower();
                            if (extension.Equals(".dyn") || extension.Equals(".dyf"))
                                startUpDynFilePath = arg;
                        }
                    }
                    catch { }

                    continue;
                }

                switch (arg[1])
                {
                    case 'c':
                    case 'C':
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[++i];
                        break;
                }
            }

            return new CommandLineArguments
            {
                CommandFilePath = commandFilePath,
                StartUpDynFilePath = startUpDynFilePath
            };
        }

        internal string CommandFilePath { get; set; }
        internal string StartUpDynFilePath { get; set; }
    }

    class Program
    {
        private static string dynamopath;

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

        [DllImport("msvcrt.dll")]
        public static extern int _putenv(string env);

        [STAThread]
        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            
            //Include Dynamo Core path in System Path variable for helix to load properly.
            UpdateSystemPathForProcess();

            var cmdLineArgs = CommandLineArguments.FromArguments(args);

            var setup = new DynamoCoreSetup(cmdLineArgs);
            var app = new Application();
            setup.RunApplication(app);

        }

        /// <summary>
        /// Finds the Dynamo Core path by looking into registery.
        /// </summary>
        /// <returns>The root folder path of Dynamo Core.</returns>
        private static string GetDynamoCorePath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            var installs = DynamoInstallDetective.DynamoProducts.FindDynamoInstallations();
            if (installs == null) return string.Empty;

            return installs.Products
                .Where(p => p.VersionInfo.Item1 == version.Major && p.VersionInfo.Item2 == version.Minor)
                .Select(p => p.InstallLocation)
                .FirstOrDefault();
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
                assemblyPath = Path.Combine(Program.DynamoCorePath, assemblyName);
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

        /// <summary>
        /// Add Dynamo Core location to the PATH system environment variable.
        /// This is to make sure dependencies (e.g. Helix assemblies) can be located.
        /// </summary>
        private static void UpdateSystemPathForProcess()
        {
            var path =
                    Environment.GetEnvironmentVariable(
                        "Path",
                        EnvironmentVariableTarget.Process) + ";" + Program.DynamoCorePath;
            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
        }
    }

}
