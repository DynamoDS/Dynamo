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
//using Greg.AuthProviders;
using Microsoft.Win32;
using Dynamo.Configuration;
using Dynamo.Updates;
using Dynamo.Logging;
using Dynamo.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using Dynamo.DynamoSandbox;
using Dynamo.Applications;

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
    /*
    class DynamoProPathResolver : IPathResolver
    {
        private readonly HashSet<string> additionalNodeDirectories;
        private readonly HashSet<string> additionalResolutionPaths;
        private readonly HashSet<string> preloadedLibraryPaths;
        private readonly string userDataRootFolder;
        private readonly string commonDataRootFolder;

        internal DynamoProPathResolver(string userDataFolder, string commonDataFolder)
        {
            // The executing assembly is DynamoStudio.exe, locate Dynamo Studio folder
            var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var currentAssemblyDir = Path.GetDirectoryName(currentAssemblyPath);

            var nodesDirectory = Path.Combine(currentAssemblyDir, "nodes");
           // var translationDll = Path.Combine(currentAssemblyDir, "Translation.dll");

            // Add an additional node processing folder
            additionalNodeDirectories = new HashSet<string>();

            if (Directory.Exists(nodesDirectory))
            {
                additionalNodeDirectories.Add(nodesDirectory);
            }

            // Add the DynamoStudio folder for assembly resolution
            additionalResolutionPaths = new HashSet<string> { currentAssemblyDir };

            preloadedLibraryPaths = new HashSet<string>
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
                "Display.dll",
             //   translationDll
            };
            this.userDataRootFolder = userDataFolder;
            this.commonDataRootFolder = commonDataFolder;
        }

        internal void AddAdditionalResolutionPath(string directory)
        {
            if (!additionalResolutionPaths.Contains(directory))
                additionalResolutionPaths.Add(directory);
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return additionalNodeDirectories; }
        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return additionalResolutionPaths; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return preloadedLibraryPaths; }
        }

        public string UserDataRootFolder
        {
            get { return userDataRootFolder; }
        }

        public string CommonDataRootFolder
        {
            get { return commonDataRootFolder; }
        }
    }
    
#if DYNAMO_LICENSING
    internal class DynamoProLicensing
    {
        private static ClwLib licensing;

        private DynamoProLicensing() { }

        public static ClwLib Licensing
        {
            get 
            { 
                if (licensing == null)
                {
                    licensing = new ClwLib();
                }
                return licensing;
            }
        }
    }
#endif
    */

    class DynamoCoreSetup
    {
        private SandboxPathResolver pathResolver;
        //private DynamoProPathResolver pathResolver;
        private SettingsMigrationWindow migrationWindow;
        private DynamoViewModel viewModel = null;
        private const string DownloadSourcePath = "http://dyn-studio-data.s3.amazonaws.com/";
        private const string SignatureSourcePath = "http://dyn-studio-data-sig.s3.amazonaws.com/";
        private const string InstallerNameBase = "DynamoStudio";
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

                /*
                // TODO: Update DownloadLink and TeamEmail when they come
                Configurations.DynamoSiteLink = "http://dynamobim.org/design/";
                Configurations.DynamoWikiLink = "http://dynamobim.org/design/source/git/wiki/";
                Configurations.DynamoBimForum = "http://dynamobim.org/design/forum/";
                Configurations.DynamoTeamEmail = "";
                Configurations.DynamoVideoTutorials = "http://dynamobim.org/design/learn/video/";
                Configurations.DynamoPrimer = "http://dynamoprimer.com/";
                Configurations.DynamoDownloadLink = "";
                Configurations.GitHubDynamoLink = "http://dynamobim.org/design/source/git/";
                Configurations.GitHubBugReportingLink = "http://dynamobim.org/design/source/git/issues/";
                */
                //var model = Dynamo.Applications.StartupUtils.MakeModel(false);
         
                var model = DynamoModel.Start(
                        new DynamoModel.DefaultStartConfiguration()
                        {
                            PathResolver = pathResolver,
                            DynamoCorePath = Program.DynamoCorePath,
                            DynamoHostPath = Path.GetDirectoryName(exePath),
                            GeometryFactoryPath = geometryFactoryPath,
                            UpdateManager = InitializeUpdateManager(),
                            //AuthProvider = authProvider,
                            ProcessMode = Dynamo.Scheduler.TaskProcessMode.Asynchronous
                        });

                var startconfig = new DynamoViewModel.StartConfiguration
                {
                    CommandFilePath = argumentSettings.CommandFilePath,
                    DynamoModel = model,
                    Watch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(new Watch3DViewModelStartupParams(model), model.Logger),
                    ShowLogin = true
                };

                viewModel = DynamoViewModel.Start(startconfig);
                //viewModel = DynamoViewModel.Start();

                //var startUpFilePath = argumentSettings.StartUpDynFilePath;
                var view = new DynamoView(viewModel);
                view.Loaded += (sender, args) => CloseMigrationWindow();
                /*
                view.Loaded += (sender, a) =>
                {
                    CloseMigrationWindow();
                    view.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // If there is a start-up file path, open it first.
                        if (!string.IsNullOrEmpty(startUpFilePath))
                            viewModel.OpenCommand.Execute(startUpFilePath);
                    }));
                };
                */
#if (DYNAMO_LICENSING)

                var rootFolder = Path.GetDirectoryName(exePath);

                DynamoProLicensing.Licensing.Initialize("en-US", SaveWorkspaceAlertAndClose);

                if (DynamoProLicensing.Licensing.CheckOut())
                {
                    app.Run(view);

                    DynamoProLicensing.Licensing.CheckIn();
                    DynamoProLicensing.Licensing.CleanUp();
                }
#else
                //var app = new Application();
                app.Run(view);
#endif
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
       
        /*
        private UpdateManagerConfiguration GetUpdateManagerConfiguration()
        {

            var config = UpdateManagerConfiguration.GetSettings(new SandboxLookUp());
            if (string.IsNullOrEmpty(config.ConfigFilePath))
            {
                config.DownloadSourcePath = DownloadSourcePath;
                config.SignatureSourcePath = SignatureSourcePath;
                config.InstallerNameBase = InstallerNameBase;
            };

            Debug.Assert(config.DynamoLookUp != null);
            return config;
        }
        */

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
            /*
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                LibraryVersion.Version220,
                LibraryVersion.Version221,
                LibraryVersion.Version222,
            };

            var preloader = new Preloader(Program.DynamoCorePath, versions);
            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            */
            //preloaderLocation = preloader.PreloaderLocation;
            /*
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            const LibraryVersion version = LibraryVersion.Version222;

            var binaryFileName = string.Format("ASMAHL{0}A.dll", ((int)version));

            var shapeManagerPath = Path.Combine(rootFolder,
                string.Format("libg_{0}", ((int)version)));

            if (!File.Exists(Path.Combine(shapeManagerPath, binaryFileName)))
            {
                // First look under 'libg_xxx' folder for ASM binaries. If not 
                // found, try looking for them directly under 'rootFolder'.
                // 
                shapeManagerPath = rootFolder;
            }
            */
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

            /*
            var userDataFolder = string.Empty; //Let's use default location location from Dynamo Core
            var commonDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Dynamo", "Dynamo Revit");
            */
            

            //pathResolver = new DynamoProPathResolver(userDataFolder, commonDataFolder);

            //pathResolver.AddAdditionalResolutionPath(preloader.PreloaderLocation);
            
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
    /*
    internal class StudioLookUp : DynamoLookUp
    {
        public override IEnumerable<string> GetDynamoInstallLocations()
        {
            const string regKey64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            //Open HKLM for 64bit registry
            var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //Open Windows/CurrentVersion/Uninstall registry key
            regKey = regKey.OpenSubKey(regKey64);

            //Get "InstallLocation" value as string for all the subkey that starts with "Dynamo"
            return regKey.GetSubKeyNames().Where(s => s.StartsWith("Autodesk Dynamo Studio")).Select(
                (s) => regKey.OpenSubKey(s).GetValue("InstallLocation") as string);
        }
    }
    */
}
