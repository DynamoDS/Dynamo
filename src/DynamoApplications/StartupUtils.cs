using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Interfaces;
using DynamoShapeManager;
using System.Reflection;
using System.IO;
using Dynamo.Models;
using Dynamo.UpdateManager;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using NDesk.Options;

namespace Dynamo.Applications.StartupUtils
{
    internal class PathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        public PathResolver(string preloaderLocation, IEnumerable<string> librariesToPreload)
        {
            // If a suitable preloader cannot be found on the system, then do 
            // not add invalid path into additional resolution. The default 
            // implementation of IPathManager in Dynamo insists on having valid 
            // paths specified through "IPathResolver" implementation.
            // 
            additionalResolutionPaths = new List<string>();
            if (Directory.Exists(preloaderLocation))
                additionalResolutionPaths.Add(preloaderLocation);

            additionalNodeDirectories = new List<string>();
            preloadedLibraryPaths = librariesToPreload.ToList();
        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return additionalResolutionPaths; }
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return additionalNodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return preloadedLibraryPaths; }
        }

        public string UserDataRootFolder
        {
            get { return string.Empty; }
        }

        public string CommonDataRootFolder
        {
            get { return string.Empty; }
        }
    }

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
    //this class is left unimplemented,unclear how to
    //lookup installation locations on nix/mac
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
        public static CommandLineArguments FromArguments(string[] args)
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

            // import a set of presets from another dyn or presetfile 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /p "C:\states.dyn"
            //
            var presetFile = string.Empty;

            // set current opened graph to state by name 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /s "state1"
            //
            var presetStateid = string.Empty;

            // print the resulting values of all nodes to the console 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /v "C:\someoutputfilepath.xml"
            //
            var verbose = string.Empty;


            var optionsSet = new OptionSet().Add("o=|O=", "OpenFilePath, Instruct Dynamo to open headless and run a dyn file at this path", o => openfilepath = o)
            .Add("c=|C=", "CommandFilePath, Instruct Dynamo to open a commandfile and run the commands it contains at this path", c => commandFilePath = c)
            .Add("l=|L=", "Running Dynamo under a different locale setting", l => locale = l)
            .Add("p=|P=", "PresetFile, Instruct Dynamo to import the presets at this path into the opened .dyn", p => presetFile = p)
            .Add("s=|S=", "PresetStateID, Instruct Dynamo to set the graph to the specified preset by name,"+
            "this can be set to a statename or 'all', which will evaluate all states in the dyn", s => presetStateid = s)
            .Add("v=|V=", "Verbose, Instruct Dynamo to output all evalautions it performs to an xml file at this path", v => verbose = v);

            optionsSet.Parse(args);

            //check for incompatabile parameters
            if ((!string.IsNullOrEmpty(presetStateid) || (!string.IsNullOrEmpty(presetFile) || (!string.IsNullOrEmpty(verbose)))) && string.IsNullOrEmpty(openfilepath))
            {
                Console.WriteLine("you must supply a file to open if you want to load a preset or presetFile, or want to save an evaluation output ");
                
            }


            return new CommandLineArguments
            {
                Locale = locale,
                CommandFilePath = commandFilePath,
                OpenFilePath = openfilepath,
                PresetStateID = presetStateid,
                PresetFilePath = presetFile,
                Verbose = verbose,
            };
        }

        public string Locale { get; set; }
        public string CommandFilePath { get; set; }
        public string OpenFilePath { get; set; }
        public string PresetStateID { get; set; }
        public string PresetFilePath { get; set; }
        public string Verbose { get; set; }
    }

    public class Preloading
    {
        public static void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                LibraryVersion.Version219,
                LibraryVersion.Version220,
                LibraryVersion.Version221
            };

            var preloader = new Preloader(rootFolder, versions);
            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            preloaderLocation = preloader.PreloaderLocation;
        }

        public static DynamoModel MakeModel(bool CLI, IEnumerable<string> librariesToPreload)
        {

            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);

            var config = new DynamoModel.DefaultStartConfiguration()
                  {
                      PathResolver = new PathResolver(preloaderLocation, librariesToPreload),
                      GeometryFactoryPath = geometryFactoryPath,
                      StartInTestMode = true
                  };

            //if we are building a model for CLI mode, then we don't want to start an updateManager
            //for now, building an updatemanager instance requires finding Dynamo install location
            //which if we are running on mac os or *nix will use different logic then SandboxLookup 

            UpdateManagerConfiguration umConfig = null;
            if (!CLI)
            {
                umConfig = UpdateManagerConfiguration.GetSettings(new SandboxLookUp());
                Debug.Assert(umConfig.DynamoLookUp != null);
                config.UpdateManager = new Dynamo.UpdateManager.UpdateManager(umConfig);
                config.StartInTestMode = false;
            }

            var model = DynamoModel.Start(config);
            return model;
        }

        public static List<string> SandBoxLibraries
        {
            get
            {
                return new List<string> 
                {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSIronPython.dll",
                "FunctionObject.ds",
                "Optimize.ds",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll"
                };

            }
        }

        public static List<string> CLILibraries
        {
            get
            {
                return new List<string> 
                {
                //"ProtoGeometry.dll",
                //"DSCoreNodes.dll",
                //"DSIronPython.dll",
                //"FunctionObject.ds",
                //"VMDataBridge.dll",
                };

            }
        }

    }

    public static class Locale
    {
        public static string SetLocale(CommandLineArguments cmdLineArgs)
        {
            var supportedLocale = new HashSet<string>(new[]
                        {
                            "cs-CZ", "de-DE", "en-US", "es-ES", "fr-FR", "it-IT",
                            "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-CN", "zh-TW"
                        });
            string libgLocale = string.Empty ;

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
    }
}
