using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using DynamoShapeManager;
using Microsoft.Win32;
using Dynamo.Configuration;
using Dynamo.Updates;
using Dynamo.Logging;
using Dynamo.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using Dynamo.DynamoSandbox;

namespace DynamoSandbox
{
    internal class SandboxPathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        public SandboxPathResolver(string preloaderLocation)
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
                "Analysis.dll",
                "Display.dll"
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

    class DynamoCoreSetup
    {
        private SandboxPathResolver pathResolver;
        private SettingsMigrationWindow migrationWindow;
        private DynamoViewModel viewModel = null;
        private CommandLineArguments argumentSettings;

        public DynamoCoreSetup(CommandLineArguments args)
        {
            argumentSettings = args;
        }

        public void RunApplication(Application app)
        {
            try
            {
                
                var exePath = Assembly.GetExecutingAssembly().Location;
                var geometryFactoryPath = string.Empty;
                var preloaderLocation = string.Empty;
                PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);
                pathResolver = new SandboxPathResolver(preloaderLocation);

                DynamoModel.RequestMigrationStatusDialog += MigrationStatusDialogRequested;

                var model = DynamoModel.Start(
                    new DynamoModel.DefaultStartConfiguration()
                    {
                        PathResolver = pathResolver,
                        DynamoCorePath = Program.DynamoCorePath,
                        DynamoHostPath = Path.GetDirectoryName(exePath),
                        GeometryFactoryPath = geometryFactoryPath,
                        UpdateManager = InitializeUpdateManager(),
                        ProcessMode = Dynamo.Scheduler.TaskProcessMode.Asynchronous
                    });

                viewModel = DynamoViewModel.Start(
                    new DynamoViewModel.StartConfiguration()
                    {
                        CommandFilePath = argumentSettings.CommandFilePath,
                        DynamoModel = model,
                        Watch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(new Watch3DViewModelStartupParams(model), model.Logger),
                        ShowLogin = true
                    });

                var view = new DynamoView(viewModel);
                view.Loaded += (sender, args) => CloseMigrationWindow();

                app.Run(view);

                DynamoModel.RequestMigrationStatusDialog -= MigrationStatusDialogRequested;
               
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

        private static IUpdateManager InitializeUpdateManager()
        {
            var cfg = UpdateManagerConfiguration.GetSettings(new SandboxLookUp());
            var um = new Dynamo.Updates.UpdateManager(cfg);
            Debug.Assert(cfg.DynamoLookUp != null);
            return um;
        }

        private void CloseMigrationWindow()
        {
            if (migrationWindow == null)
                return;

            migrationWindow.Close();
            migrationWindow = null;
        }

        private void MigrationStatusDialogRequested(SettingsMigrationEventArgs args)
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
        
        private void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
        {
            var versions = new[]
            {
                LibraryVersion.Version220,
                LibraryVersion.Version221,
                LibraryVersion.Version222,
            };

            var preloader = new Preloader(Program.DynamoCorePath, versions);

            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            preloaderLocation = preloader.PreloaderLocation;
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
}
