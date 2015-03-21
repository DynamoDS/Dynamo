using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoShapeManager;
using DynamoUtilities;

using NUnit.Framework;

namespace DynamoCoreUITests
{
    public class DynamoTestUIBase
    {
        private Preloader preloader;

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

            Model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath
                });

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

        #endregion
    }
}
