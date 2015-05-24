using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.DynamoSandbox;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.UpdateManager;
using Dynamo.ViewModels;
using DynamoShapeManager;
using DynamoUtilities;
using System.Threading;
using System.Globalization;

using Microsoft.Win32;

namespace DynamoSandbox
{
    internal class PathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        internal PathResolver(string preloaderLocation)
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
            preloadedLibraryPaths = new List<string>
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

    struct CommandLineArguments
    {
        internal static CommandLineArguments FromArguments(string[] args)
        {
            // Running Dynamo sandbox with a command file:
            // DynamoSandbox.exe /c "C:\file path\file.xml"
            // 
            var commandFilePath = string.Empty;

            // Running Dynamo under a different locale setting:
            // DynamoSandbox.exe /l "ja-JP"
            //
            var locale = string.Empty;

            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg.Length != 2 || (arg[0] != '/'))
                {
                    continue; // Not a "/x" type of command switch.
                }

                switch (arg[1])
                {
                    case 'c':
                    case 'C':
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[++i];
                        break;

                    case 'l':
                    case 'L':
                        if (i < args.Length - 1)
                            locale = args[++i];
                        break;
                }
            }

            return new CommandLineArguments
            {
                Locale = locale,
                CommandFilePath = commandFilePath
            };
        }

        internal string Locale { get; set; }
        internal string CommandFilePath { get; set; }
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

    internal class Program
    {
        private static SettingsMigrationWindow migrationWindow;

        private static void MakeStandaloneAndRun(string commandFilePath, out DynamoViewModel viewModel)
        {
            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);

            DynamoModel.RequestMigrationStatusDialog += MigrationStatusDialogRequested;

            var umConfig = UpdateManagerConfiguration.GetSettings(new SandboxLookUp());
            Debug.Assert(umConfig.DynamoLookUp != null);

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = new PathResolver(preloaderLocation),
                    GeometryFactoryPath = geometryFactoryPath,
                    UpdateManager = new UpdateManager(umConfig)
                });

            viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    CommandFilePath = commandFilePath,
                    DynamoModel = model
                });

            var view = new DynamoView(viewModel);
            view.Loaded += (sender, args) => CloseMigrationWindow();

            var app = new Application();
            app.Run(view);

            DynamoModel.RequestMigrationStatusDialog -= MigrationStatusDialogRequested;
        }

        private static void CloseMigrationWindow()
        {
            if (migrationWindow == null)
                return;

            migrationWindow.Close();
            migrationWindow = null;
        }

        private static void MigrationStatusDialogRequested(SettingsMigrationEventArgs args)
        {
            if (args.EventStatus == SettingsMigrationEventArgs.EventStatusType.Begin)
            {
                migrationWindow = new SettingsMigrationWindow();
                migrationWindow.Show();
            }
            else if (args.EventStatus == SettingsMigrationEventArgs.EventStatusType.End)
            {
                CloseMigrationWindow();
            }
        }

        private static void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
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

        [STAThread]
        public static void Main(string[] args)
        {
            DynamoViewModel viewModel = null;

            try
            {
                var cmdLineArgs = CommandLineArguments.FromArguments(args);

                if (!string.IsNullOrEmpty(cmdLineArgs.Locale))
                {
                    // Change the application locale, if a locale information is supplied.
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(cmdLineArgs.Locale);
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(cmdLineArgs.Locale);
                }

                MakeStandaloneAndRun(cmdLineArgs.CommandFilePath, out viewModel);
            }
            catch (Exception e)
            {
                try
                {
#if DEBUG
                    // Display the recorded command XML when the crash happens, 
                    // so that it maybe saved and re-run later
                    if (viewModel != null)
                        viewModel.SaveRecordedCommand.Execute(null);
#endif

                    DynamoModel.IsCrashing = true;
                    InstrumentationLogger.LogException(e);
                    StabilityTracking.GetInstance().NotifyCrash();

                    if (viewModel != null)
                    {
                        // Show the unhandled exception dialog so user can copy the 
                        // crash details and report the crash if she chooses to.
                        viewModel.Model.OnRequestsCrashPrompt(null,
                            new CrashPromptArgs(e.Message + "\n\n" + e.StackTrace));

                        // Give user a chance to save (but does not allow cancellation)
                        viewModel.Exit(allowCancel: false);
                    }
                }
                catch
                {
                }

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
