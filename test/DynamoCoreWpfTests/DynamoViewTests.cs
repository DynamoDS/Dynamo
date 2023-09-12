using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.ViewModels.Core;
using DynamoCoreWpfTests.Utility;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpDX.DXGI;


namespace DynamoCoreWpfTests
{
    public class DynamoViewTests : DynamoTestUIBase
    {
        // adapted from: NodeViewTests.cs

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("FFITarget.dll");
        }

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        [Test]
        public void FooterNotificationControlTest()
        {
            // Arrange
            Open(@"UI\ZoomNodeColorStates.dyn");

            var workspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            Debug.Assert(workspace != null, nameof(workspace) + " != null");
            workspace.Run();

            List<NodeModel> errorNodes = ViewModel.Model.CurrentWorkspace.Nodes.ToList().FindAll(n => n.State == ElementState.Error);
            List<NodeModel> warningNodes = ViewModel.Model.CurrentWorkspace.Nodes.ToList().FindAll(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);

            // We should have 3 nodes in Error state and 3 nodes in Warning state
            Assert.AreEqual(3, warningNodes.Count());
            Assert.AreEqual(3, errorNodes.Count());

            NotificationsControl notificationsControl = View.ChildOfType<NotificationsControl>();
            Debug.Assert(notificationsControl != null, nameof(notificationsControl) + " != null");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            // Check if the run message indicates error
            Assert.AreEqual(notificationsControl.runNotificationTextBlock.Text, "Run completed with errors.");

            ItemCollection items = notificationsControl.footerIconItemsControl.Items;

            // We should have 3 FooterIconItems, 1 for error, 1 for warning, and 1 for Info
            Assert.AreEqual(items.Count, 3);

            // The first item should be the Error, the second should be the Warning
            Assert.AreEqual((items[0] as FooterNotificationItem).NotificationCount, 3);
            Assert.AreEqual((items[0] as FooterNotificationItem).NotificationImage, "/DynamoCoreWpf;component/UI/Images/error.png");

            Assert.AreEqual((items[1] as FooterNotificationItem).NotificationCount, 3);
            Assert.AreEqual((items[1] as FooterNotificationItem).NotificationImage, "/DynamoCoreWpf;component/UI/Images/warning_16px.png");

            errorNodes.ForEach(en => (workspace as WorkspaceModel).RemoveAndDisposeNode(en, true));
            workspace.Run();

            // Check if the run message indicates warning
            Assert.AreEqual(notificationsControl.runNotificationTextBlock.Text, "Run completed with warnings.");

            // After deleting all Error nodes, the counter should get to 0 
            Assert.AreEqual((items[0] as FooterNotificationItem).NotificationCount, 0);

            warningNodes.ForEach(wn => (workspace as WorkspaceModel).RemoveAndDisposeNode(wn, true));
            workspace.Run();

            // Check if the run message indicates OK
            Assert.AreEqual(notificationsControl.runNotificationTextBlock.Text, "Run completed.");

            // After deleting all Warning nodes, the counter should get to 0 
            Assert.AreEqual((items[1] as FooterNotificationItem).NotificationCount, 0);
        }

        [Test]
        public void OpeningWorkspaceWithTrustWarning()
        {
            // Open workspace with test mode as false, to verify trust warning.
            DynamoModel.IsTestMode = false;
            Open(@"core\CustomNodes\TestAdd.dyn");

            Assert.IsTrue(ViewModel.FileTrustViewModel.ShowWarningPopup);

            // Close workspace
            Assert.IsTrue(ViewModel.CloseHomeWorkspaceCommand.CanExecute(null));
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);

            // Asert that the warning popup is closed, when the workspace is closed.
            Assert.IsFalse(ViewModel.FileTrustViewModel.ShowWarningPopup);
            DynamoModel.IsTestMode = true;
        }

        [Test]
        public void ElementBinding_SaveAs()
        {
            var prebindingPathInTestDir = @"core\callsite\trace_test-prebinding.dyn";
            var prebindingPath = Path.Combine(GetTestDirectory(ExecutingDirectory), prebindingPathInTestDir);

            var pathInTestsDir = @"core\callsite\trace_test.dyn";
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), pathInTestsDir);

            // Always start with a fresh workspace with no binding data for this test.
            File.Copy(prebindingPath, filePath);
            OpenAndRun(pathInTestsDir);

            // Assert that the node doesn't have trace data the first time it's run.
            var hasTraceData = Model.CurrentWorkspace.Nodes.FirstOrDefault(x =>
                x.Name == "IncrementerTracedClass.WasCreatedWithTrace");
            Assert.AreEqual(false, hasTraceData.CachedValue.Data);

            // Saving the workspace after a run serializes trace data to the DYN.
            ViewModel.SaveCommand.Execute(null);

            DynamoUtilities.PathHelper.isValidJson(filePath, out string fileContents, out Exception ex);
            var obj = DSCore.Data.ParseJSON(fileContents) as Dictionary<string, object>;
            Assert.AreEqual(1, (obj["Bindings"] as IEnumerable<object>).Count());

            var saveAsPathInTestDir = @"core\callsite\trace_test2.dyn";
            var saveAsPath = Path.Combine(GetTestDirectory(ExecutingDirectory), saveAsPathInTestDir);

            // SaveAs current workspace, close workspace.
            ViewModel.SaveAsCommand.Execute(saveAsPath);
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);

            Open(saveAsPathInTestDir);

            // Assert saved as file doesn't have binding data after open.
            DynamoUtilities.PathHelper.isValidJson(filePath, out fileContents, out ex);
            obj = DSCore.Data.ParseJSON(fileContents) as Dictionary<string, object>;
            Assert.AreEqual(1, (obj["Bindings"] as IEnumerable<object>).Count());

            File.Delete(filePath);
            File.Delete(saveAsPath);
        }
    }
}
