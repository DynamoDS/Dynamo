using System.IO;
using System.Reflection;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.Tests.UI
{
    public class DynamoTestUI
    {
        protected static DynamoController Controller { get; set; }
        
        protected static DynamoViewModel Vm { get; set; }
        
        protected static DynamoView Ui { get; set; }
        
        protected static DynamoModel Model { get; set; }

        protected static string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected static string TempFolder;

        #region Utility functions

        public static void EmptyTempFolder()
        {
            var directory = new DirectoryInfo(TempFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        public static string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        #endregion
    }
}
