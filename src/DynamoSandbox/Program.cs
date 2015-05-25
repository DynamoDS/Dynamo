using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Dynamo.ViewModels;
using DynamoShapeManager;
using DynamoUtilities;
using System.Threading;
using System.Globalization;

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

            // Open Dynamo headless and open file at path
            // DynamoSandbox.exe /o "C:\file path\graph.dyn"
            //
            var openfilepath = string.Empty;

            // set current opened graph to state by name 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /s "state1"
            //
            var presetFile = string.Empty;

            // set current opened graph to state by name 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /s "state1"
            //
            var presetStateid = string.Empty;

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

                    case 'o':
                    case 'O':
                        if (i < args.Length - 1)
                            openfilepath = args[++i];
                        break;
                    
                    case 's':
                    case 'S':
                        if (i < args.Length - 1)
                            presetStateid = args[++i];
                        break;

                    case 'p':
                    case 'P':
                        if (i < args.Length - 1)
                            presetFile = args[++i];
                        break;
                }
            }

            return new CommandLineArguments
            {
                Locale = locale,
                CommandFilePath = commandFilePath,
                OpenFilePath = openfilepath,
                PresetStateID = presetStateid,
                PresetFilePath = presetFile,
            };
        }

        internal string Locale { get; set; }
        internal string CommandFilePath { get; set; }
        internal string OpenFilePath { get; set; }
        internal string PresetStateID { get; set; }
        internal string PresetFilePath { get; set; }
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

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = new PathResolver(preloaderLocation),
                    GeometryFactoryPath = geometryFactoryPath
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

        private static void MakeModelAndSetState(CommandLineArguments cmdLineArgs)
        {
            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);

           // DynamoModel.RequestMigrationStatusDialog += MigrationStatusDialogRequested;

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = new PathResolver(preloaderLocation),
                    GeometryFactoryPath = geometryFactoryPath
                });
            model.OpenFileFromPath(cmdLineArgs.OpenFilePath);
            Console.WriteLine("loaded file");

            if (!string.IsNullOrEmpty(cmdLineArgs.PresetFilePath))
            {
                //load the states contained in this file, it should be structured so
                //that there is a PresetsModel element containing multiple PresetStates elements
                //load it pointing to the file we opened so that states will point to the correct nodes
                model.CurrentWorkspace.PresetsCollection.ImportStates( 
                    PresetsModel.LoadFromXmlPaths(cmdLineArgs.PresetFilePath,cmdLineArgs.OpenFilePath,model.NodeFactory));
            }

            //build a list of states, for now, none, a single state, or all of them
            //this must be done after potentially loading states from external file
            var stateNames = new List<String>();
            if (!string.IsNullOrEmpty(cmdLineArgs.PresetStateID))
            {
                if (cmdLineArgs.PresetStateID == "all")
                {
                    foreach (var state in model.CurrentWorkspace.PresetsCollection.DesignStates)
                    {
                        stateNames.Add(state.Name);
                    }
                }
                else
                {
                    stateNames.Add(cmdLineArgs.PresetStateID);
                }
            }
            else
            {
                stateNames.Add("default");
            }


            foreach (var stateName in stateNames)
            {
                model.CurrentWorkspace.SetWorkspaceToState(stateName);
                model.ExecuteCommand(new DynamoModel.RunCancelCommand(false, false));
                Thread.Sleep(250);
            }
           
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
                //if we have an openfilepath arg then open headless and attempt to set that file to a state
                //if supplied, if we have supplied a presetsFile, then append that as the presetsModel for our workspace
                if (!string.IsNullOrEmpty(cmdLineArgs.OpenFilePath))
                {
                    MakeModelAndSetState(cmdLineArgs);
                }
                else
                {
                    MakeStandaloneAndRun(cmdLineArgs.CommandFilePath, out viewModel);
                }
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
