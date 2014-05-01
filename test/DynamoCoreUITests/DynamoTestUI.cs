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
    public class DynamoTestUI
    {
        protected DynamoController Controller { get; set; }
        
        protected DynamoViewModel Vm { get; set; }
        
        protected DynamoView Ui { get; set; }
        
        protected DynamoModel Model { get; set; }

        protected string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected string TempFolder;

        [SetUp]
        public virtual void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

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

            Controller = DynamoController.MakeSandbox();
            DynamoController.IsTestMode = true;

            //create the view
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Model = Controller.DynamoModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TearDown]
        public void Exit()
        {
            if (Ui.IsLoaded)
                Ui.Close();

            Controller.ShutDown(false);

            Controller = null;
            Vm = null;
            Ui = null;
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
