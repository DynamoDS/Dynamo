using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.FSchemeInterop;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo
{
    public class UnitTestBase
    {

        protected string ExecutingDirectory { get; set; }
        protected string TempFolder { get; private set; }

        [SetUp]
        public virtual void Init()
        {
            SetupDirectories();
        }

        [TearDown]
        public virtual void Cleanup()
        {
            try
            {
                EmptyTempFolder();
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

        public string GetNewFileNameOnTempPath(string fileExtension = "dyn")
        {
            return Path.Combine(TempFolder, Guid.NewGuid().ToString() + "." + fileExtension);
        }

        public string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        protected void SetupDirectories()
        {
            ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempPath = Path.GetTempPath();

            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
            else
                EmptyTempFolder();
        }
    }
}
