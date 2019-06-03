using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using DynamoShapeManager;
using NUnit.Framework;
using TestServices;

namespace DynamoCoreWpfTests
{
    public class DynamoTestUIBase
    {
        protected Preloader preloader;

        protected DynamoViewModel ViewModel { get; set; }
        protected DynamoView View { get; set; }
        protected DynamoModel Model { get; set; }

        protected string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected string TempFolder { get; private set; }

        [SetUp]
        public virtual void Start()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            preloader = new Preloader(Path.GetDirectoryName(assemblyPath));
            preloader.Preload();
            CreateTemporaryFolder();

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            TestPathResolver pathResolver = null;
            var preloadedLibraries = new List<string>();
            GetLibrariesToPreload(preloadedLibraries);

            if (preloadedLibraries.Any())
            {
                // Only when any library needs preloading will a path resolver be 
                // created, otherwise DynamoModel gets created without preloading 
                // any library.
                // 
                pathResolver = new TestPathResolver();
                foreach (var preloadedLibrary in preloadedLibraries.Distinct())
                {
                    pathResolver.AddPreloadLibraryPath(preloadedLibrary);
                }
            }

            Model = DynamoModel.Start(
               this.CreateStartConfiguration(pathResolver));

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model
                });

            //create the view
            View = new DynamoView(ViewModel);
            View.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        /// <summary>
        /// Derived test classes can override this method to provide different configurations.
        /// </summary>
        /// <param name="pathResolver">A path resolver to pass to the DynamoModel. </param>
        protected virtual DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous
            };
        }

        [TearDown]
        public void Exit()
        {
            //Ensure that we leave the workspace marked as
            //not having changes.
            ViewModel.HomeSpace.HasUnsavedChanges = false;

            if (View.IsLoaded)
                View.Close();

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false, allowCancellation: false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            View = null;
            Model = null;
            preloader = null;

            try
            {
                var directory = new DirectoryInfo(TempFolder);
                directory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            //Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        protected virtual void GetLibrariesToPreload(List<string> libraries)
        {
            // Nothing here...
        }

        #region Utility functions

        /// <summary>
        /// Open a file from the Dynamo test directory
        /// </summary>
        /// <param name="pathInTestsDir">A relative path from the test directory</param>
        public virtual void Open(string pathInTestsDir)
        {
            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), pathInTestsDir);
            ViewModel.OpenCommand.Execute(openPath);
        }

        public void OpenAndRun(string pathInTestsDir)
        {
            Open(pathInTestsDir);
            Run();
        }

        /// <summary>
        /// Run the current workspace.  Doesn't return until 
        /// </summary>
        public virtual void Run()
        {
            var complete = false;

            EventHandler<EvaluationCompletedEventArgs> markDone = (e, a) => { complete = true;  };
            ViewModel.Model.EvaluationCompleted += markDone;

            ViewModel.HomeSpace.Run();

            while (!complete)
            {
                Thread.Sleep(1);
            }

            ViewModel.Model.EvaluationCompleted -= markDone;
        }

        public static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        protected void CreateTemporaryFolder()
        {
            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }

        public NodeView NodeViewOf<T>() where T : NodeModel
        {
            var nodeViews = View.NodeViewsInFirstWorkspace();
            var nodeViewsOfType = nodeViews.OfNodeModelType<T>();
            Assert.AreEqual(1, nodeViewsOfType.Count(), "Expected a single NodeView of provided type in the workspace!");

            return nodeViewsOfType.First();
        }

        public NodeView NodeViewWithGuid(string guid)
        {
            var nodeViews = View.NodeViewsInFirstWorkspace();
            var nodeViewsOfType = nodeViews.Where(x => x.ViewModel.NodeLogic.GUID.ToString() == guid);
            Assert.AreEqual(1, nodeViewsOfType.Count(), "Expected a single NodeView with guid: " + guid);

            return nodeViewsOfType.First();
        }

        public NoteView NoteViewWithGuid(string guid)
        {
            var noteViews = View.NoteViewsInFirstWorkspace();
            var noteViewsOfType = noteViews.Where(x => x.ViewModel.Model.GUID.ToString() == guid);
            Assert.AreEqual(1, noteViewsOfType.Count(), "Expected a single NoteView with guid: " + guid);

            return noteViewsOfType.First();
        }

        #endregion
    }
}
