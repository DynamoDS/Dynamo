using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
using DynamoUtilities;
using NUnit.Framework;
using TestServices;

namespace DynamoCoreWpfTests
{
    internal class TestDiagnostics
    {
        internal int DispatcherOpsCounter = 0;
        // Use this flag to skip trying to execute all the dispatched operations during the test lifetime.
        // This flag should only be used very sparingly
        internal bool SkipDispatcherFlush = false;

        private void Hooks_OperationPosted(object sender, DispatcherHookEventArgs e)
        {
            e.Operation.Task.ContinueWith((t) => {
                Interlocked.Decrement(ref DispatcherOpsCounter);
            }, TaskScheduler.Default);
            Interlocked.Increment(ref DispatcherOpsCounter);
        }

        private void PrettyPrint(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            Console.WriteLine("{");
            foreach (var property in properties)
            {
                try
                {
                    Console.WriteLine($"  {property.Name}: {property.GetValue(obj)}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  {property.Name}: {e.Message}");
                }
            }
            Console.WriteLine("}");
        }

        internal void StartupDiagnostics()
        {
            System.Console.WriteLine($"PID {Process.GetCurrentProcess().Id} Start test: {TestContext.CurrentContext.Test.Name}");
            TestUtilities.WebView2Tag = TestContext.CurrentContext.Test.Name;

            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

            Dispatcher.CurrentDispatcher.Hooks.OperationPosted += Hooks_OperationPosted;
        }

        internal void BeforeCleanupDiagnostics()
        {
            Dispatcher.CurrentDispatcher.Hooks.OperationPosted -= Hooks_OperationPosted;
            if (!SkipDispatcherFlush)
            {
                DispatcherUtil.DoEventsLoop(() => DispatcherOpsCounter == 0);
            }
        }

        internal void AfterCleanupDiagnostics()
        {
            TestUtilities.WebView2Tag = string.Empty;
            using (var currentProc = Process.GetCurrentProcess())
            {
                int id = currentProc.Id;
                var name = TestContext.CurrentContext.Test.Name;
                System.Console.WriteLine($"PID {id} Finished test: {name} with DispatcherOpsCounter = {DispatcherOpsCounter}");
                System.Console.WriteLine($"PID {id} Finished test: {name} with WorkingSet = {currentProc.WorkingSet64}");
                System.Console.WriteLine($"PID {id} Finished test: {name} with PrivateBytes = {currentProc.PrivateMemorySize64}");
                System.Console.Write($"PID {id} Finished test: {name} with GC Memory Info: ");
                PrettyPrint(GC.GetGCMemoryInfo());
            }
        }

        internal void CleanupDiagnostics()
        {
            BeforeCleanupDiagnostics();
            AfterCleanupDiagnostics();
        }
    }

    public class DynamoTestUIBase
    {
        protected Preloader preloader;

        protected DynamoViewModel ViewModel { get; set; }
        protected DynamoView View { get; set; }
        protected DynamoModel Model { get; set; }

        // Use this flag to skip trying to execute all the dispatched operations during the test lifetime.
        // This flag should only be used very sparingly
        protected bool SkipDispatcherFlush {
            get => testDiagnostics.SkipDispatcherFlush;
            set => testDiagnostics.SkipDispatcherFlush = value;
        }

        [Obsolete("This property will be deprecated as it is for internal use only.")]
        protected int DispatcherOpsCounter {
            get => testDiagnostics.DispatcherOpsCounter;
        } 

        private TestDiagnostics testDiagnostics = new();

        protected string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected string TempFolder { get; private set; }

        [SetUp]
        public virtual void Start()
        {
            testDiagnostics.StartupDiagnostics();
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
        }

        protected static void RaiseLoadedEvent(FrameworkElement element)
        {
            MethodInfo eventMethod = typeof(FrameworkElement).GetMethod("OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic);

            RoutedEventArgs args = new RoutedEventArgs(FrameworkElement.LoadedEvent);

            eventMethod.Invoke(element, new object[] { args });
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
            testDiagnostics.BeforeCleanupDiagnostics();

            //Ensure that we leave the workspace marked as
            //not having changes.
            ViewModel.HomeSpace.HasUnsavedChanges = false;

            if (View != null && View.IsLoaded)
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
            testDiagnostics.AfterCleanupDiagnostics();
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

        protected static string GetAppDataFolder()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dynamoVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var appDataFolder = Path.Combine(Path.Combine(folder, "Dynamo", "Dynamo Core"),
                $"{dynamoVersion.FileMajorPart}.{dynamoVersion.FileMinorPart}");

            return appDataFolder;
        }
        protected static string GetCommonDataDirectory()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dynamoVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var commonDataFolder = Path.Combine(Path.Combine(folder, "Dynamo", "Dynamo Core"),
                $"{dynamoVersion.FileMajorPart}.{dynamoVersion.FileMinorPart}");
            return commonDataFolder;
        }

        #endregion
    }
}
