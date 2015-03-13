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
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.ViewModels;
using DynamoShapeManager;
using DynamoUtilities;

namespace DynamoSandbox
{
    internal class PathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        internal PathResolver(string preloaderLocation)
        {
            additionalResolutionPaths = new List<string>
            {
                preloaderLocation
            };

            additionalNodeDirectories = new List<string>();
            preloadedLibraryPaths = new List<string>();
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
    }

    internal class Program
    {
        private static void MakeStandaloneAndRun(string commandFilePath, out DynamoViewModel viewModel)
        {
            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);

            // TODO(PATHMANAGER): Do we really libg_xxx folder on resolution path?
            // If not, PathResolver will be completely redundant so please remove it.
            var pathResolver = new PathResolver(preloaderLocation);

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    GeometryFactoryPath = geometryFactoryPath
                });

            viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    CommandFilePath = commandFilePath,
                    DynamoModel = model
                });

            var view = new DynamoView(viewModel);

            var app = new Application();
            app.Run(view);
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
                // Running Dynamo sandbox with a command file:
                // DynamoSandbox.exe /c "C:\file path\file.xml"
                // 
                var commandFilePath = string.Empty;

                for (var i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];

                    // Looking for '/c'
                    if (arg.Length != 2 || (arg[0] != '/'))
                        continue;

                    if (arg[1] == 'c' || (arg[1] == 'C'))
                    {
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[i + 1];
                    }
                }

                MakeStandaloneAndRun(commandFilePath, out viewModel);
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
