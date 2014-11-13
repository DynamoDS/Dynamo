using System;
using System.IO;
using System.Reflection;
using System.Threading;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoUtilities;

using NUnit.Framework;

using TestServices;

namespace SystemTestServices
{
    /// <summary>
    /// SystemTestBase is the base class for all 
    /// Dynamo system tests.
    /// </summary>
    public abstract class SystemTestBase
    {
        protected string workingDirectory;

        #region protected properties

        protected DynamoViewModel ViewModel { get; set; }

        protected DynamoView View { get; set; }

        protected DynamoModel Model { get; set; }

        protected string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected string TempFolder { get; private set; }

        #endregion

        #region public methods

        [SetUp]
        public virtual void Setup()
        {
            AssemblyResolver.Setup();

            SetupCore();

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            DynamoPathManager.PreloadAsmLibraries(DynamoPathManager.Instance);

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;
            CreateTemporaryFolder();

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DYNAMO_TEST_PATH = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            StartDynamo();
        }

        public virtual void SetupCore()
        {

        }

        public virtual void StartDynamo()
        {
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
        public virtual void TearDown()
        {
            //Ensure that we leave the workspace marked as
            //not having changes.
            Model.HomeSpace.HasUnsavedChanges = false;

            if (View.IsLoaded)
                View.Close();

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(false, false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            View = null;
            Model = null;

            GC.Collect();

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
        public virtual void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            //Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        /// <summary>
        /// Open and run a Dynamo definition given a relative
        /// path from the working directory.
        /// </summary>
        /// <param name="subPath"></param>
        protected void OpenAndRunDynamoDefinition(string subPath)
        {
            OpenDynamoDefinition(subPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        /// <summary>
        /// Open a Dynamo definition given a relative
        /// path from the working directory
        /// </summary>
        /// <param name="relativeFilePath"></param>
        public void OpenDynamoDefinition(string relativeFilePath)
        {
            string samplePath = Path.Combine(workingDirectory, relativeFilePath);
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(File.Exists(testPath), string.Format("Could not find file: {0} for testing.", testPath));

            ViewModel.OpenCommand.Execute(testPath);
        }

        #endregion

        #region Utility functions

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
