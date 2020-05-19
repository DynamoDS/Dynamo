using Dynamo.Scheduler;
using Dynamo.Wpf.ViewModels.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Regression tests at that require UI features and make use of geometry related functions.
    /// </summary>
    [TestFixture]
    public class GeometryRegressionTests : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test addresses two failures:
        /// 1. A mishandled exception causing the Executive to not be restored during a function
        /// endpoint call, resulting in an inconsistent state which then resulted in a crash.
        /// 2. An error with the transpose built-in method when it received a combination of empty
        /// arrays and scalars, resulting in a crash.
        /// </summary>
        [Test]
        public void DeletingNodesShouldNotMakeTransposeFailCausingCrash()
        {
            TaskStateChangedEventHandler evaluationDidNotFailHandler =
                (DynamoScheduler sender, TaskStateChangedEventArgs args) =>
                {
                    Assert.AreNotEqual(TaskStateChangedEventArgs.State.ExecutionFailed, args.CurrentState);
                };
            try
            {
                Model.Scheduler.TaskStateChanged += evaluationDidNotFailHandler;

                // Open graph
                Open(@"core\regressions\front_grill_error_standalone.dyn");

                // Delete node group
                var workspaceVM = ViewModel.Workspaces.First() as HomeWorkspaceViewModel;
                var groupVM = workspaceVM.Annotations.First(a => a.AnnotationText == "Delete causes errors");
                groupVM.Select();
                groupVM.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);

                // The graph run successfully and, more importantly, execution did not cause a crash.
                Assert.IsTrue(workspaceVM.IsInIdleState);
                Assert.AreEqual(NotificationLevel.Mild, workspaceVM.CurrentNotificationLevel);
            }
            finally
            {
                Model.Scheduler.TaskStateChanged -= evaluationDidNotFailHandler;
            }
        }
    }
}
