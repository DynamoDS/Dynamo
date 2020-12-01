using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Updates;
using DynamoApplications.Properties;
using DynamoShapeManager;
using Microsoft.Win32;
using NDesk.Options;

namespace Dynamo.Applications
{
    public class StartupUtils
    {
        //TODO internal?
        /// <summary>
        /// Raised when loading of the ASM binaries fails. A failure message is passed as a parameter.
        /// </summary>
        public static event Action<string> ASMPreloadFailure;
        
        internal class SandboxLookUp : DynamoLookUp
        {
            public override IEnumerable<string> GetDynamoInstallLocations()
            {
                const string regKey64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
                //Open HKLM for 64bit registry
                var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                //Open Windows/CurrentVersion/Uninstall registry key
                regKey = regKey.OpenSubKey(regKey64);

                //Get "InstallLocation" value as string for all the subkey that starts with "Dynamo"
                return regKey.GetSubKeyNames().Where(s => s.StartsWith("Dynamo")).Select(
                    (s) => regKey.OpenSubKey(s).GetValue("InstallLocation") as string);
            }
        }

        /// <summary>
        ///this class is left unimplemented,unclear how to
        ///lookup installation locations on nix/mac
        /// </summary>
        internal class CLILookUp : DynamoLookUp
        {
            public override IEnumerable<string> GetDynamoInstallLocations()
            {
                throw new NotImplementedException();
                int p = (int)Environment.OSVersion.Platform;
                if ((p == 4) || (p == 6) || (p == 128))
                {
                    Console.WriteLine("Running on Unix");
                }
                else
                {
                    Console.WriteLine("NOT running on Unix");
                }

                return null;
            }
        }

        public struct CommandLineArguments
        {
            public static CommandLineArguments Parse(string[] args)
            {
                // Running Dynamo sandbox with a command file:
                // DynamoSandbox.exe /c "C:\file path\file.xml"
                // 
                var commandFilePath = string.Empty;

                // Running Dynamo under a different locale setting:
                // DynamoSandbox.exe /l "ja-JP"
                //
                var locale = string.Empty;

                // Open Dynamo headless and open file at path
                // DynamoSandbox.exe /o "C:\file path\graph.dyn"
                //
                var openfilepath = string.Empty;

                // print the resulting values of all nodes to the console 
                // DynamoSandbox.exe /o "C:\file path\graph.dyn" /v "C:\someoutputfilepath.xml"
                //
                var verbose = string.Empty;

                var convertFile = false;

                // Generate geometry json file
                var geometryFilePath = string.Empty;

                // dll paths we'll import before running a graph 
                var importPaths = new List<string>() ;

                var asmPath = string.Empty;

                bool keepAlive = false;
                bool showHelp = false;
                var optionsSet = new OptionSet().Add("o=|O=", "OpenFilePath, Instruct Dynamo to open headless and run a dyn file at this path", o => openfilepath = o)
                .Add("c=|C=", "CommandFilePath, Instruct Dynamo to open a commandfile and run the commands it contains at this path," +
                "this option is only supported when run from DynamoSandbox", c => commandFilePath = c)
                .Add("l=|L=", "Running Dynamo under a different locale setting", l => locale = l)
                .Add("v=|V=", "Verbose, Instruct Dynamo to output all evalautions it performs to an xml file at this path", v => verbose = v)
                .Add("x|X", "When used in combination with the 'O' flag, opens a .dyn file from the specified path and converts it to .json." +
                "File will have the .json extension and be located in the same directory as the original file.", x => convertFile = x != null)
                .Add("h|H|help", "Get some help", h => showHelp = h != null)
                .Add("g=|G=|geometry", "Geometry, Instruct Dynamo to output geometry from all evaluations to a json file at this path", g => geometryFilePath = g)
                .Add("i=|I=|import", "Import, Instruct Dynamo to import an assembly as a node library. This argument should be a filepath to a single .dll" +
                " - if you wish to import multiple dlls - use this flag multiple times: -i 'assembly1.dll' -i 'assembly2.dll' ", i => importPaths.Add(i))
                .Add("gp=|GP=|geometrypath=|GeometryPath=", "relative or absolute path to a directory containing ASM. When supplied, instead of searching the hard disk for ASM, it will be loaded directly from this path.", gp => asmPath = gp)
                .Add("k|K|keepalive", "Keepalive mode, leave the Dynamo process running until a loaded extension shuts it down.", k => keepAlive = k != null)
                ;
                optionsSet.Parse(args);

                if (showHelp)
                {
                    ShowHelp(optionsSet);
                }

                //check for incompatabile parameters
                if (!string.IsNullOrEmpty(verbose) && string.IsNullOrEmpty(openfilepath))
                {
                    Console.WriteLine("you must supply a file to open if you want to save an evaluation output ");
                }
                return new CommandLineArguments
                {
                    Locale = locale,
                    CommandFilePath = commandFilePath,
                    OpenFilePath = openfilepath,
                    Verbose = verbose,
                    ConvertFile = convertFile,
                    GeometryFilePath = geometryFilePath,
                    ImportedPaths = importPaths,
                    ASMPath = asmPath,
                    KeepAlive = keepAlive
                };
            }

            private static void ShowHelp(OptionSet opSet)
            {
                Console.WriteLine("options:");
                opSet.WriteOptionDescriptions(Console.Out);
            }

            public string Locale { get; set; }
            public string CommandFilePath { get; set; }
            public string OpenFilePath { get; set; }
            public string Verbose { get; set; }
            public bool ConvertFile { get; set; }
            public string GeometryFilePath { get; set; }
            public IEnumerable<String> ImportedPaths { get; set; }
            public string ASMPath { get; set; }
            public bool KeepAlive { get; set; }
        }

        public static void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                new Version(227,0,0),
                new Version(226,0,0), 
                new Version(225,0,0)
            };

            var preloader = new Preloader(rootFolder, versions);
            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            preloaderLocation = preloader.PreloaderLocation;
        }

        /// <summary>
        ///if we are building a model for CLI mode, then we don't want to start an updateManager
        ///for now, building an updatemanager instance requires finding Dynamo install location
        ///which if we are running on mac os or *nix will use different logic then SandboxLookup 
        /// </summary>
        private static IUpdateManager InitializeUpdateManager()
        {
            var cfg = UpdateManagerConfiguration.GetSettings(new SandboxLookUp());
            var um = new Dynamo.Updates.UpdateManager(cfg);
            Debug.Assert(cfg.DynamoLookUp != null);
            return um;
        }

        

        /// <summary>
        /// Use this overload to construct a DynamoModel when the location of ASM to use is known.
        /// </summary>
        /// <param name="CLImode">CLI mode starts the model in test mode and uses a seperate path resolver.</param>
        /// <param name="asmPath">Path to directory containing geometry library binaries</param>
        /// <returns></returns>
        public static DynamoModel MakeModel(bool CLImode, string asmPath)
        {
            //get sandbox executing location - this is where libG will be located.
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);
            //defaults - these will fail.
            var preloaderLocation = "libg_0_0_0";
            var geometryFactoryPath = Path.Combine(preloaderLocation, DynamoShapeManager.Utilities.GeometryFactoryAssembly);

            try
            {
                if (!Directory.Exists(asmPath))
                {
                    throw new FileNotFoundException($"{nameof(asmPath)}:{asmPath}");
                }
                Version asmBinariesVersion = DynamoShapeManager.Utilities.GetVersionFromPath(asmPath);

                //get version of libG that matches the asm version that was supplied from geometryLibraryPath.
                preloaderLocation = DynamoShapeManager.Utilities.GetLibGPreloaderLocation(asmBinariesVersion, rootFolder);
                geometryFactoryPath = Path.Combine(preloaderLocation, DynamoShapeManager.Utilities.GeometryFactoryAssembly);

                //load asm and libG.
                DynamoShapeManager.Utilities.PreloadAsmFromPath(preloaderLocation, asmPath);
            }
            catch(Exception e)
            {
                Console.WriteLine("A problem occured while trying to load ASM or LibG");
                Console.WriteLine($"{e?.Message} : {e?.StackTrace}");
            }
            return StartDynamoWithDefaultConfig(CLImode, geometryFactoryPath, preloaderLocation);

        }
        //TODO (DYN-2118) remove this method in 3.0 and unify this method with the overload above.
        public static DynamoModel MakeModel(bool CLImode)
        {
            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            try
            {
                PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);
            }
            catch(Exception e)
            {
                ASMPreloadFailure?.Invoke(e.Message);
            }

            return StartDynamoWithDefaultConfig(CLImode, geometryFactoryPath, preloaderLocation);
        }

        private static DynamoModel StartDynamoWithDefaultConfig(bool CLImode, string geometryFactoryPath, string preloaderLocation)
        {
            var config = new DynamoModel.DefaultStartConfiguration()
            {
                GeometryFactoryPath = geometryFactoryPath,
                ProcessMode = TaskProcessMode.Asynchronous
            };

            config.UpdateManager = CLImode ? null : InitializeUpdateManager();
            config.StartInTestMode = CLImode ? true : false;
            config.PathResolver = CLImode ? new CLIPathResolver(preloaderLocation) as IPathResolver : new SandboxPathResolver(preloaderLocation) as IPathResolver;

            var model = DynamoModel.Start(config);
            return model;
        }

        public static string SetLocale(CommandLineArguments cmdLineArgs)
        {
            var supportedLocale = new HashSet<string>(new[]
                        {
                            "cs-CZ", "de-DE", "en-US", "es-ES", "fr-FR", "it-IT",
                            "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-CN", "zh-TW"
                        });
            string libgLocale = string.Empty;

            if (!string.IsNullOrEmpty(cmdLineArgs.Locale))
            {
                // Change the application locale, if a locale information is supplied.
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cmdLineArgs.Locale);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cmdLineArgs.Locale);
                libgLocale = cmdLineArgs.Locale;
            }
            else
            {
                // In case no language is specified, libG's locale should be that of the OS.
                // There is no need to set Dynamo's locale in this case.
                libgLocale = CultureInfo.InstalledUICulture.ToString();
            }

            // If locale is not supported by Dynamo, default to en-US.
            if (!supportedLocale.Any(s => s.Equals(libgLocale, StringComparison.InvariantCultureIgnoreCase)))
            {
                libgLocale = "en-US";
            }
            // Change the locale that LibG depends on.
            StringBuilder sb = new StringBuilder("LANGUAGE=");
            sb.Append(libgLocale.Replace("-", "_"));
            return sb.ToString();
        }

        /// <summary>
        /// The white list of dependencies to be ignored.
        /// </summary>
        private static String[] assemblyNamesToIgnore = { "Newtonsoft.Json", "RevitAPI.dll", "RevitAPIUI.dll" };

        /// <summary>
        /// Checks that an assembly does not have any dependencies that have already been loaded into the 
        /// appDomain with an incompatible to the one Dynamo requires.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>returns a list of fileLoad exceptions - if the list is empty no mismatched assemblies were encountered </returns>
        public static List<Exception> CheckAssemblyForVersionMismatches(Assembly assembly)
        {
            return GetVersionMismatchedReferencesInAppDomain(assembly, assemblyNamesToIgnore);
        }

        /// <summary>
        /// Handler for an assembly load event into a host's appdomain - we need to make sure
        /// that another addin or package has not loaded another version of a .dll that we require.
        /// If this happens Dynamo will most likely crash. We should alert the user they
        /// have an incompatible addin/package installed.. this is only called if the host calls or
        /// subscribes to it during AppDomain.AssemblyLoad event.
        /// 
        private static List<Exception> GetVersionMismatchedReferencesInAppDomain(Assembly assembly, String[] assemblyNamesToIgnore)
        {
            // Get all assemblies that are currently loaded into the appdomain.
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            // Ignore some assemblies(Revit assemblies) that we know work and have changed their version number format or do not align
            // with semantic versioning.

            var loadedAssemblyNames = loadedAssemblies.Select(assem => assem.GetName()).ToList();
            loadedAssemblyNames.RemoveAll(assemblyName =>assemblyNamesToIgnore.Contains(assemblyName.Name));

            //build dict- ignore those with duplicate names.
            var loadedAssemblyDict = loadedAssemblyNames.GroupBy(assm => assm.Name).ToDictionary(g => g.Key, g => g.First());

            var output = new List<Exception>();

            foreach (var currentReferencedAssembly in assembly.GetReferencedAssemblies().Concat(new AssemblyName[] { assembly.GetName() }))
            {
                if (loadedAssemblyDict.ContainsKey(currentReferencedAssembly.Name))
                {
                    //if the dll is already loaded, then check that our required version is not greater than the currently loaded one.
                    var loadedAssembly = loadedAssemblyDict[currentReferencedAssembly.Name];
                    if (currentReferencedAssembly.Version.Major > loadedAssembly.Version.Major)
                    {
                        //there must exist a loaded assembly which references the newer version of the assembly which we require - lets find it:

                        var referencingNewerVersions = new List<AssemblyName>();
                        foreach(var originalLoadedAssembly in loadedAssemblies )
                        {
                            foreach(var refedAssembly in originalLoadedAssembly.GetReferencedAssemblies())
                            {
                                //if the version matches then this is one our guys
                                if(refedAssembly.Version == loadedAssembly.Version)
                                {
                                    referencingNewerVersions.Add(originalLoadedAssembly.GetName());
                                }
                            }
                        }

                        output.Add(new FileLoadException(
                            string.Format(Resources.MismatchedAssemblyVersion, assembly.FullName, currentReferencedAssembly.FullName)
                            + Environment.NewLine + Resources.MismatchedAssemblyList + Environment.NewLine +
                            String.Join(", ", referencingNewerVersions.Select(x => x.Name).Distinct().ToArray())));
                    }
                }
            }
            return output;
        }



    }
}
