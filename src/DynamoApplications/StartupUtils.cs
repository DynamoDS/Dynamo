﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Interfaces;
using Dynamo.Scheduler;
using DynamoShapeManager;
using System.Reflection;
using System.IO;
using Dynamo.Models;
using Dynamo.Updates;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using NDesk.Options;


namespace Dynamo.Applications
{
    public class StartupUtils
    {
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

                bool showHelp = false;
                var optionsSet = new OptionSet().Add("o=|O=", "OpenFilePath, Instruct Dynamo to open headless and run a dyn file at this path", o => openfilepath = o)
                .Add("c=|C=", "CommandFilePath, Instruct Dynamo to open a commandfile and run the commands it contains at this path,"+ 
                "this option is only supported when run from DynamoSandbox", c => commandFilePath = c)
                .Add("l=|L=", "Running Dynamo under a different locale setting",l => locale = l)
                .Add("p=|P=", "PresetFile, Instruct Dynamo to import the presets at this path into the opened .dyn", p => presetFile = p)
                .Add("s=|S=", "PresetStateID, Instruct Dynamo to set the graph to the specified preset by name," +
                "this can be set to a statename or 'all', which will evaluate all states in the dyn", s => presetStateid = s)
                .Add("v=|V=", "Verbose, Instruct Dynamo to output all evalautions it performs to an xml file at this path", v => verbose = v)
                .Add("h|H|help", "Get some help", h => showHelp = h!=null);

                optionsSet.Parse(args);

                if (showHelp)
                {
                    ShowHelp(optionsSet);
                }

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

          
            private static void ShowHelp(OptionSet opSet)
            {
                Console.WriteLine("options:");
                opSet.WriteOptionDescriptions(Console.Out);
            }

            public string Locale { get; set; }
            public string CommandFilePath { get; set; }
            public string OpenFilePath { get; set; }
            public string PresetStateID { get; set; }
            public string PresetFilePath { get; set; }
            public string Verbose { get; set; }
        }

        public static void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                LibraryVersion.Version220,
                LibraryVersion.Version221
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

        public static DynamoModel MakeModel(bool CLImode)
        {

            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);

            var config = new DynamoModel.DefaultStartConfiguration()
                  {
                      GeometryFactoryPath = geometryFactoryPath,
                      ProcessMode = TaskProcessMode.Asynchronous
                  };

            config.UpdateManager = CLImode ? null : InitializeUpdateManager();
            config.StartInTestMode = CLImode ? true : false;
            config.PathResolver = CLImode ? new CLIPathResolver(preloaderLocation) as IPathResolver : new SandboxPathResolver(preloaderLocation) as IPathResolver ; 

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

    }
}
