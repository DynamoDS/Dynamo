using System;
using System.IO;
using System.Reflection;
using Dynamo.FSchemeInterop;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class DynamoUnitTest
    {
        protected DynamoController Controller;
        protected string ExecutingDirectory { get; set; }

        #region startup and shutdown

        protected string TempFolder { get; private set; }

        [SetUp]
        public virtual void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public virtual void Cleanup()
        {
            try
            {
                DynamoLogger.Instance.FinishLogging();
                Controller.ShutDown();

                EmptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void StartDynamo()
        {
            try
            {
                ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string tempPath = Path.GetTempPath();

                TempFolder = Path.Combine(tempPath, "dynamoTmp");

                if (!Directory.Exists(TempFolder))
                {
                    Directory.CreateDirectory(TempFolder);
                }
                else
                {
                    EmptyTempFolder();
                }

                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                Controller = new DynamoController(new ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE)
                {
                    Testing = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Gives a unique file name located in the temp folder
        /// </summary>
        /// <returns>String representing the path</returns>
        public string GetNewFileNameOnTempPath(string fileExtension = "dyn")
        {
            return Path.Combine(TempFolder, Guid.NewGuid().ToString() + "." + fileExtension);
        }

        #endregion

        #region utility methods

        public string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        #endregion
    }
}