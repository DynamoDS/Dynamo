using CoreNodeModels.Input;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DynamoCoreWpfTests
{
    class DynamoCommandViewTests : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void EnsureSaveDialogIsShownOnOpenIfSaveCommand()
        {
            //add a node to the current workspace which will ensure home space has some changes
            var model = Model;
            model.CurrentWorkspace.HasUnsavedChanges = true;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), (@"UI\GroupTest.dyn"));

            //execute open command on some other .dyn file
            ViewModel.OpenIfSavedCommand.Execute(new Dynamo.Models.DynamoModel.OpenFileCommand(openPath));
            //ensure that a modal dialog is being shown and that the request save dialog event was raised
            DispatcherUtil.DoEvents();
            //check the window is open
            View.OwnedWindows.Cast
                 
        }

        [Test]
        public void EnsureNoCrashIfSaveDialogIsRequestedWhileAlreadyInModalDialog()
        {
            //add a node to the current workspace which will ensure home space has some changes

            //execute open command on some other .dyn file

            //ensure that a modal dialog is being shown and that the request save dialog event was raised

            //check the property for modal state on the dynamoView?
        }
    }
}
