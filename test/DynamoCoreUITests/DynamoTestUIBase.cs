using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    public class DynamoTestUIBase
    {
        protected DynamoViewModel ViewModel { get; set; }
        protected DynamoView View { get; set; }
        protected DynamoModel Model { get; set; }

        protected string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected string TempFolder;

        [SetUp]
        public virtual void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder(TempFolder);
            }

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DYNAMO_TEST_PATH = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            Model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true
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
            Model.HomeSpace.HasUnsavedChanges = false;

            if (View.IsLoaded)
                View.Close();

            Model.ShutDown(false);

            ViewModel = null;
            View = null;
            Model = null;

            GC.Collect();
        }

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            //Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        #region Utility functions

        public static void EmptyTempFolder(string tempFolder)
        {
            var directory = new DirectoryInfo(tempFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        public static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        #endregion
    }
}
