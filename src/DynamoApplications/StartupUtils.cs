using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using CommandLine;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Updates;
using DynamoApplications.Properties;
using DynamoShapeManager;
using DynamoUtilities;
using Microsoft.Win32;

namespace Dynamo.Applications
{
    internal class CMDLineOptions
    {
        [Option('l', "Locale", Required = false, HelpText = "Running Dynamo under a different locale setting.")]
        public string Locale { get; set; } = String.Empty;
        [Option('c', "CommandFilePath", Required = false, HelpText = "Instruct Dynamo to open a commandfile and run the commands it contains at this path," +
            "this option is only supported when run from DynamoSandbox")]
        public string CommandFilePath { get; set; } = String.Empty;
        [Option('o', "OpenFilePath", Required = false, HelpText = "Instruct Dynamo to open headless and run a dyn file at this path.")]
        public string OpenFilePath { get; set; } = String.Empty;
        [Option('v', "Verbose", Required = false, HelpText = "Instruct Dynamo to output all evalautions it performs to an xml file at this path.")]
        public string Verbose { get; set; } = String.Empty;
        [Option('x', "ConvertFile", Required = false, HelpText = "When used in combination with the 'O' flag, opens a .dyn file from the specified path and converts it to .json." +
            "File will have the .json extension and be located in the same directory as the original file.")]
        public bool ConvertFile { get; set; }
        [Option('g', "Geometry", Required = false, HelpText = "Instruct Dynamo to output geometry from all evaluations to a json file at this path.")]
        public string GeometryFilePath { get; set; } = String.Empty;
        [Option('i', "Import", Required = false, HelpText = "Instruct Dynamo to import an assembly as a node library.This argument should be a filepath to a single.dll" +
            " - if you wish to import multiple dlls - list the dlls separated by a space: -i 'assembly1.dll' 'assembly2.dll'")]
        public IEnumerable<String> ImportedPaths { get; set; } = new List<string>();
        [Option("GeometryPath", Required = false, HelpText = "relative or absolute path to a directory containing ASM. When supplied, instead of searching the hard disk for ASM, it will be loaded directly from this path.")]
        public string ASMPath { get; set; } = String.Empty;
        [Option('k', "KeepAlive", Required = false, HelpText = "Keepalive mode, leave the Dynamo process running until a loaded extension shuts it down.")]
        public bool KeepAlive { get; set; }
        [Option('n', "NoConsole", Required = false, HelpText = "Don't rely on the console window to interact with CLI in Keepalive mode.")]
        public bool NoConsole { get; set; }
        [Option('u', "UserData", Required = false, HelpText = "Specify user data folder to be used by PathResolver with CLI.")]
        public string UserDataFolder { get; set; } = String.Empty;
        [Option("CommonData", Required = false, HelpText = "Specify common data folder to be used by PathResolver with CLI.")]
        public string CommonDataFolder { get; set; } = String.Empty;
        [Option("HostName", Required = false, HelpText = "Identify Dynamo variation associated with host.")]
        public string HostName { get; set; } = String.Empty;
        [Option("DisableAnalytics", Required = false, HelpText = "Disables analytics in Dynamo for the process lifetime.")]
        public bool DisableAnalytics { get; set; }
        [Option("NoNetworkMode", Required = false, HelpText = "Disables network traffic in Dynamo at startup. Disables some features such as Notifications, Sign In, and ML Node Autocomplete for process lifetime.")]
        public bool NoNetworkMode { get; set; }
        [Option('p', "ParentId", Required = false, HelpText = "Identify Dynamo host analytics parent id.")]
        public string ParentId { get; set; } = String.Empty;
        [Option('s', "SessionId", Required = false, HelpText = "Identify Dynamo host analytics session id.")]
        public string SessionId { get; set; } = String.Empty;
        [Option("CERLocation", Required = false, HelpText = "Specify the crash error report tool location on disk.")]
        public string CERLocation { get; set; } = String.Empty;
        [Option("ServiceMode", Required = false, HelpText = "Specify the service mode startup.")]
        public bool ServiceMode { get; set; }
    }

    public static class StartupUtils
    {
        //TODO internal?
        /// <summary>
        /// Raised when loading of the ASM binaries fails. A failure message is passed as a parameter.
        /// </summary>
        public static event Action<string> ASMPreloadFailure;

        public struct CommandLineArguments
        {
            public static CommandLineArguments Parse(string[] args)
            {
                var parser = new Parser(options => { options.IgnoreUnknownArguments = true; options.HelpWriter = Console.Error;
                    options.CaseSensitive = false;
                });
                return parser.ParseArguments<CMDLineOptions>(args).MapResult((cmdArgs) => {
                    if (!string.IsNullOrEmpty(cmdArgs.Verbose) && string.IsNullOrEmpty(cmdArgs.OpenFilePath))
                    {
                        Console.WriteLine("you must supply a file to open if you want to save an evaluation output ");
                    }

                    return new CommandLineArguments()
                    {
                        Locale = cmdArgs.Locale,
                        CommandFilePath = cmdArgs.CommandFilePath,
                        OpenFilePath = cmdArgs.OpenFilePath,
                        Verbose = cmdArgs.Verbose,
                        ConvertFile = cmdArgs.ConvertFile,
                        GeometryFilePath = cmdArgs.GeometryFilePath,
                        ImportedPaths = cmdArgs.ImportedPaths,
                        ASMPath = cmdArgs.ASMPath,
                        KeepAlive = cmdArgs.KeepAlive,
                        NoConsole = cmdArgs.NoConsole,
                        UserDataFolder = cmdArgs.UserDataFolder,
                        CommonDataFolder = cmdArgs.CommonDataFolder,
                        DisableAnalytics = cmdArgs.DisableAnalytics,
                        NoNetworkMode = cmdArgs.NoNetworkMode,
                        AnalyticsInfo = new HostAnalyticsInfo() { HostName = cmdArgs.HostName, ParentId = cmdArgs.ParentId, SessionId = cmdArgs.SessionId },
                        CERLocation = cmdArgs.CERLocation,
                        ServiceMode = cmdArgs.ServiceMode
                    };
                }, errs => new CommandLineArguments());
            }

            internal void SetDisableAnalytics()
            {
                if (DisableAnalytics || NoNetworkMode)
                {
                    Analytics.DisableAnalytics = true;
                }
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
            public bool NoConsole { get; set; }
            public string UserDataFolder { get; set; }
            public string CommonDataFolder { get; set; }
            public bool DisableAnalytics { get; set; }
            public bool NoNetworkMode { get; set; }
            public HostAnalyticsInfo AnalyticsInfo { get; set; }
            public string CERLocation { get; set; }

            /// <summary>
            /// Boolean indication of launching Dynamo in service mode, this mode is optimized for minimal launch time
            /// </summary>
            public bool ServiceMode { get; set; }
        }

        /// <summary>
        /// Attempts to load the geometry library binaries using the location params.
        /// </summary>
        /// <param name="geometryFactoryPath">libG ProtoInterface path</param>
        /// <param name="preloaderLocation">libG folder path</param>
#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        [Obsolete("The API has been deprecated and will be removed in a future release of Dynamo. Use the overload that returns asmVersion and asmPath out parameters for better diagnostics.")]
        public static void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                new Version(232,0,0),
                new Version(231,0,0),
            };

            var preloader = new Preloader(rootFolder, versions);
            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            preloaderLocation = preloader.PreloaderLocation;
        }

        /// <summary>
        /// Attempts to load the geometry library binaries and returns ASM version information.
        /// </summary>
        /// <param name="geometryFactoryPath">libG ProtoInterface path</param>
        /// <param name="preloaderLocation">libG folder path</param>
        /// <param name="asmVersion">Version of the loaded ASM, or null if version cannot be determined</param>
        /// <param name="asmPath">Path where ASM binaries are located, or empty string if not found</param>
#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        public static void PreloadShapeManager(
            ref string geometryFactoryPath,
            ref string preloaderLocation,
            out Version asmVersion,
            out string asmPath)
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                new Version(232,0,0),
                new Version(231,0,0),
            };

            var preloader = new Preloader(rootFolder, versions);
            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            preloaderLocation = preloader.PreloaderLocation;
            asmVersion = preloader.Version2;
            asmPath = preloader.ShapeManagerPath;

            // If version wasn't detected but we have a path, try to extract version from the ASM binaries
            if (asmVersion == null && !string.IsNullOrEmpty(asmPath))
            {
                try
                {
                    asmVersion = DynamoShapeManager.Utilities.GetVersionFromPath(asmPath);
                }
                catch
                {
                    Console.WriteLine("ASM Version extraction failed");
                }
            }
        }

        /// <summary>
        /// Use this overload to construct a DynamoModel in CLI context when the location of ASM to use is known, host analytics info is known and you want to set data paths.
        /// </summary>
        /// <param name="asmPath">Path to directory containing geometry library binaries</param>
        /// <param name="userDataFolder">Path to be used by PathResolver for UserDataFolder</param>
        /// <param name="commonDataFolder">Path to be used by PathResolver for CommonDataFolder</param>
        /// <param name="info">Host analytics info specifying Dynamo launching host related information.</param>
        /// <returns></returns>
        public static DynamoModel MakeCLIModel(string asmPath, string userDataFolder, string commonDataFolder, HostAnalyticsInfo info = new HostAnalyticsInfo())
        {
            // Preload ASM and display corresponding message on splash screen
            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Resources.SplashScreenPreLoadingAsm, 10));
            var isASMloaded = PreloadASM(asmPath, out string geometryFactoryPath, out string preloaderLocation, out Version asmVersion, out string asmLoadedFrom);
            var model = StartDynamoWithDefaultConfig(true, userDataFolder, commonDataFolder, geometryFactoryPath, preloaderLocation, false, info, asmVersion: asmVersion, asmPath: asmLoadedFrom, isASMLoaded: isASMloaded);
            model.IsASMLoaded = isASMloaded;
            return model;
        }

        private static DynamoModel PrepareModel(
            string cliLocale,
            string asmPath,
            bool noNetworkMode,
            HostAnalyticsInfo analyticsInfo,
            bool cliMode = true,
            string userDataFolder = "",
            string commonDataFolder = "",
            bool serviceMode = false)
        {
            var normalizedCLILocale = string.IsNullOrEmpty(cliLocale) ? null : cliLocale;
            IPathResolver pathResolver = CreatePathResolver(false, string.Empty, string.Empty, string.Empty);
            PathManager.Instance.AssignHostPathAndIPathResolver(string.Empty, pathResolver);
            DynamoModel.SetUICulture(normalizedCLILocale ?? PreferenceSettings.Instance.Locale);
            DynamoModel.OnDetectLanguage();

            // Preload ASM and display corresponding message on splash screen
            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Resources.SplashScreenPreLoadingAsm, 10));
            var isASMloaded = PreloadASM(asmPath, out string geometryFactoryPath, out string preloaderLocation, out Version asmVersion, out string asmLoadedFrom);
            var model = StartDynamoWithDefaultConfig(
                CLImode: cliMode,
                userDataFolder: userDataFolder,
                commonDataFolder: commonDataFolder,
                geometryFactoryPath: geometryFactoryPath,
                preloaderLocation: preloaderLocation,
                noNetworkMode: noNetworkMode,
                info: analyticsInfo,
                isServiceMode: serviceMode,
                cliLocale: normalizedCLILocale,
                asmVersion: asmVersion,
                asmPath: asmLoadedFrom,
                isASMLoaded: isASMloaded
            );
            model.IsASMLoaded = isASMloaded;
            return model;
        }

        /// <summary>
        /// Use this overload to construct a DynamoModel in CLI context when the location of ASM to use is known, host analytics info is known and you want to set data paths.
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        public static DynamoModel MakeCLIModel(CommandLineArguments cmdLineArgs)
        {
            var asmPath = string.IsNullOrEmpty(cmdLineArgs.ASMPath) ? string.Empty : cmdLineArgs.ASMPath;
            var model = PrepareModel(
                cliLocale: cmdLineArgs.Locale,
                asmPath: asmPath,
                noNetworkMode: cmdLineArgs.NoNetworkMode,
                analyticsInfo: cmdLineArgs.AnalyticsInfo,
                userDataFolder: cmdLineArgs.UserDataFolder,
                commonDataFolder: cmdLineArgs.CommonDataFolder,
                serviceMode: cmdLineArgs.ServiceMode);
            return model;
        }

        /// <summary>
        /// Use this overload to construct a DynamoModel when the location of ASM to use is known and host name is known.
        /// </summary>
        /// <param name="CLImode">CLI mode starts the model in test mode and uses a separate path resolver.</param>
        /// <param name="asmPath">Path to directory containing geometry library binaries</param>
        /// <param name="hostName">Dynamo variation identified by host.</param>
        /// <returns></returns>
        public static DynamoModel MakeModel(bool CLImode, string asmPath = "", string hostName ="")
        {
            var model = PrepareModel(
                cliLocale: string.Empty,
                asmPath: asmPath,
                noNetworkMode: false,
                analyticsInfo: new HostAnalyticsInfo() { HostName = hostName },
                cliMode: CLImode);
            return model;
        }

        /// <summary>
        /// Use this overload to construct a DynamoModel when the location of ASM to use is known and host analytics info is known.
        /// </summary>
        /// <param name="CLImode">CLI mode starts the model in test mode and uses a separate path resolver.</param>
        /// <param name="noNetworkMode">Option to initialize Dynamo in no-network mode</param>
        /// <param name="asmPath">Path to directory containing geometry library binaries</param>
        /// <param name="info">Host analytics info specifying Dynamo launching host related information.</param>
        /// <returns></returns>
        public static DynamoModel MakeModel(bool CLImode, bool noNetworkMode, string asmPath = "", HostAnalyticsInfo info = new HostAnalyticsInfo())
        {
            var model = PrepareModel(
                cliLocale: string.Empty,
                asmPath: asmPath,
                noNetworkMode: noNetworkMode,
                analyticsInfo: info,
                cliMode: CLImode);
            return model;
        }

        /// <summary>
        /// Use this overload to construct a DynamoModel when the location of ASM to use is known and host analytics info is known.
        /// </summary>
        /// <param name="CLImode">CLI mode starts the model in test mode and uses a separate path resolver.</param>
        /// <param name="CLIlocale">CLI argument to force dynamo locale</param>
        /// <param name="noNetworkMode">Option to initialize Dynamo in no-network mode</param>
        /// <param name="asmPath">Path to directory containing geometry library binaries</param>
        /// <param name="info">Host analytics info specifying Dynamo launching host related information.</param>
        /// <returns></returns>
        public static DynamoModel MakeModel(bool CLImode, string CLIlocale, bool noNetworkMode, string asmPath = "",
            HostAnalyticsInfo info = new HostAnalyticsInfo())
        {
            var model = PrepareModel(
                cliLocale: CLIlocale,
                asmPath: asmPath,
                noNetworkMode: noNetworkMode,
                analyticsInfo: info,
                cliMode: CLImode);
            return model;
        }

        /// <summary>
        /// It returns an IPathResolver based on the mode and some locations
        /// </summary>
        /// <param name="CLImode">CLI mode starts the model in test mode and uses a seperate path resolver.</param>
        /// <param name="preloaderLocation">Path to be used by PathResolver for preLoaderLocation</param>
        /// <param name="userDataFolder">Path to be used by PathResolver for UserDataFolder</param>
        /// <param name="commonDataFolder">Path to be used by PathResolver for CommonDataFolder</param>
        /// <returns></returns>
        private static IPathResolver CreatePathResolver(bool CLImode, string preloaderLocation, string userDataFolder, string commonDataFolder)
        {
            IPathResolver pathResolver = CLImode ? new CLIPathResolver(preloaderLocation, userDataFolder, commonDataFolder) as IPathResolver : new SandboxPathResolver(preloaderLocation) as IPathResolver;
            return pathResolver;
        }
        
        private static bool PreloadASM(string asmPath, out string geometryFactoryPath, out string preloaderLocation, out Version asmVersion, out string asmLoadedFrom)
        {
            if (string.IsNullOrEmpty(asmPath) && OSHelper.IsWindows())
            {
                geometryFactoryPath = string.Empty;
                preloaderLocation = string.Empty;
                try
                {
                    PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation, out asmVersion, out asmLoadedFrom);
                }
                catch (Exception e)
                {
                    asmVersion = null;
                    asmLoadedFrom = string.Empty;
                    ASMPreloadFailure?.Invoke(e.Message);
                    return false;
                }
                // If the output locations are not valid, return false
                if (!Directory.Exists(preloaderLocation) && !File.Exists(geometryFactoryPath))
                {
                    return false;
                }
                return true;
            }

            // get sandbox executing location - this is where libG will be located.
            var rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // defaults - preload these will fail.
            preloaderLocation = "libg_0_0_0";
            geometryFactoryPath = Path.Combine(preloaderLocation, DynamoShapeManager.Utilities.GeometryFactoryAssembly);

            try
            {
                if (!Directory.Exists(asmPath))
                {
                    throw new FileNotFoundException($"{nameof(asmPath)}:{asmPath}");
                }
                Version asmBinariesVersion = DynamoShapeManager.Utilities.GetVersionFromPath(asmPath, OSHelper.IsWindows() ? "*ASMAHL*.dll" : "*ASMahl*.so");

                //get version of libG that matches the asm version that was supplied from geometryLibraryPath.
                preloaderLocation = DynamoShapeManager.Utilities.GetLibGPreloaderLocation(asmBinariesVersion, rootFolder);
                geometryFactoryPath = Path.Combine(preloaderLocation, DynamoShapeManager.Utilities.GeometryFactoryAssembly);

                //load asm and libG.
                DynamoShapeManager.Utilities.PreloadAsmFromPath(preloaderLocation, asmPath);
                asmVersion = asmBinariesVersion;
                asmLoadedFrom = asmPath;
                return true;
            }
            catch (Exception e)
            {
                asmVersion = null;
                asmLoadedFrom = string.Empty;
                Console.WriteLine("A problem occurred while trying to load ASM or LibG");
                Console.WriteLine($"{e?.Message} : {e?.StackTrace}");
                return false;
            }
        }

        private static DynamoModel StartDynamoWithDefaultConfig(bool CLImode,
            string userDataFolder,
            string commonDataFolder,
            string geometryFactoryPath,
            string preloaderLocation,
            bool noNetworkMode,
            HostAnalyticsInfo info = new HostAnalyticsInfo(),
            bool isServiceMode = false,
            string cliLocale = null,
            Version asmVersion = null,
            string asmPath = null,
            bool isASMLoaded = true)
        {

            var config = new DynamoModel.DefaultStartConfiguration
            {
                GeometryFactoryPath = geometryFactoryPath,
                ProcessMode = CLImode ? TaskProcessMode.Synchronous : TaskProcessMode.Asynchronous,
                HostAnalyticsInfo = info,
                CLIMode = CLImode,
                AuthProvider = CLImode || noNetworkMode ? null : new Core.IDSDKManager(),
                StartInTestMode = CLImode,
                PathResolver = CreatePathResolver(CLImode, preloaderLocation, userDataFolder, commonDataFolder),
                IsServiceMode = isServiceMode,
                Preferences = PreferenceSettings.Instance,
                NoNetworkMode = noNetworkMode,
                CLILocale = cliLocale
                //Breaks all Lucene calls. TI enable this would require a lot of refactoring around Lucene usage in Dynamo.
                //IsHeadless = CLImode
            };
            var model = DynamoModel.Start(config);

            // Log ASM version and path
            if (asmVersion != null)
            {
                model.Logger.Log(string.Format("ASM version {0} loaded from: {1}",
                    asmVersion, string.IsNullOrEmpty(asmPath) ? "unknown location" : asmPath));
            }
            else if (isASMLoaded)
            {
                model.Logger.Log("ASM loaded successfully (version could not be determined)");
            }
            else
            {
                model.Logger.LogWarning("ASM could not be loaded", WarningLevel.Moderate);
            }

            return model;
        }

        public static string SetLocale(CommandLineArguments cmdLineArgs)
        {
            var supportedLocale = new HashSet<string>(Configuration.Configurations.SupportedLocaleDic.Values);
            string libgLocale = string.Empty;

            if (!string.IsNullOrEmpty(cmdLineArgs.Locale))
            {
                // Change the application locale, if a locale information is supplied.
                DynamoModel.SetUICulture(cmdLineArgs.Locale);
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
        private static readonly String[] assemblyNamesToIgnore = { "Newtonsoft.Json", "RevitAPI.dll", "RevitAPIUI.dll" };

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
        /// </summary>
        private static List<Exception> GetVersionMismatchedReferencesInAppDomain(Assembly assembly, String[] assemblyNamesToIgnore)
        {
            // We will only investigate the ALC of the input assembly. It is a valid use case to have a different
            // version of the input assembly or one of its references loaded in a different ALC.
            // While we don't know in which ALC the referenced assemblies will be loaded, it's fair to make an
            // educated guess and assume they will be loaded in the same ALC as the main input assembly.
            // Ignore some assemblies(Revit assemblies) that we know work and have changed their version number format or do not align
            // with semantic versioning.
            var loadedAssemblies = AssemblyLoadContext.GetLoadContext(assembly).Assemblies
                .Where(a => !assemblyNamesToIgnore.Contains(a.GetName().Name))
                .ToList();

            var output = new List<Exception>();
            var loadedAssemblyDict = loadedAssemblies.GroupBy(a => a.GetName().Name).ToDictionary(g => g.Key, g => g.FirstOrDefault());

            foreach (var currentReferencedAssembly in assembly.GetReferencedAssemblies())
            {
                if (loadedAssemblyDict.TryGetValue(currentReferencedAssembly.Name, out Assembly loadedAssembly))
                {
                    //if the dll is already loaded, then check lthat our required version is not greater than the currently loaded one.
                    if (currentReferencedAssembly.Version.Major > loadedAssembly.GetName().Version.Major)
                    {
                        //there must exist a loaded assembly which references the newer version of the assembly which we require - lets find it:
                        var referencingNewerVersions = new List<AssemblyName>();
                        foreach (var originalLoadedAssembly in loadedAssemblies)
                        {
                            foreach (var refedAssembly in originalLoadedAssembly.GetReferencedAssemblies())
                            {
                                //if the version matches then this is one our guys
                                if (refedAssembly.Version == loadedAssembly.GetName().Version)
                                {
                                    referencingNewerVersions.Add(originalLoadedAssembly.GetName());
                                }
                            }
                        }

                        output.Add(new FileLoadException(
                            string.Format(Resources.MismatchedAssemblyVersion, assembly.FullName, currentReferencedAssembly.FullName)
                            + Environment.NewLine + Resources.MismatchedAssemblyList + Environment.NewLine +
                            string.Join(", ", referencingNewerVersions.Select(x => x.Name).Distinct())));
                    }
                }
            }
            return output;
        }
    }
}
