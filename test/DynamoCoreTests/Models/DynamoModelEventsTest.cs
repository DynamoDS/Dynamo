using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;


namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class have several test method for execute the events in the DynamoModelEvents class
    /// </summary>
    [TestFixture]
    class DynamoModelEventsTest : DynamoModelTestBase
    {
        private bool dispatcherExecuted = false;
        private bool migrationStatusDialog = false;
        private bool layoutUpdate = false;
        private bool workspaceClearing = false;
        private bool workspaceRemoveStarted = false;
        private bool deletionStarted = false;
        private bool deletionComplete = false;
        private bool requestCancelActiveStateForNode = false;
        private bool requestsRedraw = false;
        private bool requestNodeSelect = false;
        private bool runCompleted = false;
        private bool requestsCrashPrompt = false;
        private bool requestTaskDialog = false;
        private bool requestDownloadDynamo = false;
        private bool requestBugReport = false;
        private bool evaluationCompleted = false;
        private bool refreshCompleted = false;


        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        /// <summary>
        /// This test method will execute the event OnRequestDispatcherBeginInvoke
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestDispatcherBeginInvoke()
        {
            //Arrange
            //We need to create an event and include an exception
            EvaluationCompletedEventArgs e = new EvaluationCompletedEventArgs(true, new Exception("Test"));

            //This will subcribe to the EvaluationCompleted event
            CurrentDynamoModel.EvaluationCompleted += CurrentDynamoModel_EvaluationCompleted;

            //Act
            //This will call the OnRequestDispatcherBeginInvoke method in the else section (no subscribers to the event)
            CurrentDynamoModel.OnEvaluationCompleted(this, e);

            //This will subscribe our local method to the RequestDispatcherBeginInvoke event
            DynCmd.RequestDispatcherBeginInvoke += DynamoModel_RequestDispatcherBeginInvoke;
            
            //This will call the OnRequestDispatcherBeginInvoke method when we have subscribers
            CurrentDynamoModel.OnEvaluationCompleted(this, e);

            //Assert
            DynCmd.RequestDispatcherBeginInvoke -= DynamoModel_RequestDispatcherBeginInvoke;
            CurrentDynamoModel.EvaluationCompleted -= CurrentDynamoModel_EvaluationCompleted;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(dispatcherExecuted);
            Assert.IsTrue(evaluationCompleted);
        }

       

        /// <summary>
        /// This test method will execute the event OnRequestDispatcherBeginInvoke
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestMigrationStatusDialog()
        {
            //Arrange
            //This will subscribe our local method to the RequestMigrationStatusDialog event
            DynCmd.RequestMigrationStatusDialog += DynamoModel_RequestMigrationStatusDialog;

            //Act
            //This will execute the OnRequestMigrationStatusDialog() in the DynamoModelEvents class
            DynCmd.OnRequestMigrationStatusDialog(new SettingsMigrationEventArgs(
                SettingsMigrationEventArgs.EventStatusType.Begin));

            //Assert
            DynCmd.RequestMigrationStatusDialog -= DynamoModel_RequestMigrationStatusDialog;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(migrationStatusDialog);
        }

        /// <summary>
        /// This test method will execute the event OnRequestLayoutUpdate
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestLayoutUpdate()
        {
            //Arrange
            EvaluationCompletedEventArgs e = new EvaluationCompletedEventArgs(true, new Exception("Test"));

            //Act
            //This will subscribe our local method to the RequestLayoutUpdate event
            CurrentDynamoModel.RequestLayoutUpdate += Model_RequestLayoutUpdate;

            //This method needs to be called directly since we are not loading any dyn file to manage events
            CurrentDynamoModel.OnRequestLayoutUpdate(this, e);

            //Assert
            CurrentDynamoModel.RequestLayoutUpdate -= Model_RequestLayoutUpdate;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(layoutUpdate);
        }

        /// <summary>
        /// This test method will execute the event OnWorkspaceClearing
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnWorkspaceClearing()
        {
            //Arrange
            //This will subscribe our local method to the RequestLayoutUpdate event
            CurrentDynamoModel.WorkspaceClearing += CurrentDynamoModel_WorkspaceClearing;

            //Act
            //Internally this will execute the OnWorkspaceClearing() method in DynamoModelEvents
            CurrentDynamoModel.ClearCurrentWorkspace();

            //Assert
            CurrentDynamoModel.WorkspaceClearing -= CurrentDynamoModel_WorkspaceClearing;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(workspaceClearing);
        }

        /// <summary>
        /// This test method will execute the event OnWorkspaceRemoveStarted
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnWorkspaceRemoveStarted()
        {
            //Arrange
            //This will subscribe our local method to the WorkspaceRemoveStarted event
            CurrentDynamoModel.WorkspaceRemoveStarted += CurrentDynamoModel_WorkspaceRemoveStarted;

            //Act
            //Internally this will execute the OnWorkspaceRemoveStarted() method in DynamoModelEvents
            CurrentDynamoModel.RemoveWorkspace(CurrentDynamoModel.CurrentWorkspace);

            //Assert
            CurrentDynamoModel.WorkspaceRemoveStarted -= CurrentDynamoModel_WorkspaceRemoveStarted;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(workspaceRemoveStarted);
        }

        /// <summary>
        /// This test method will execute the event OnDeletionStarted
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnDeletionStarted()
        {
            //Arrange
            //This will subscribe our local method to the DeletionStarted event
            CurrentDynamoModel.DeletionStarted += CurrentDynamoModel_DeletionStarted;
            List<ModelBase> modelsToDelete = new List<ModelBase>();
            Guid groupid = Guid.NewGuid();
            CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupid);

            var annotations = CurrentDynamoModel.Workspaces.SelectMany(ws => ws.Annotations);

            foreach (var annotation in annotations)
            {
                modelsToDelete.Insert(0, annotation);
            }
            
            //Act
            var cancelEventArgs = new System.ComponentModel.CancelEventArgs();
            CurrentDynamoModel.OnDeletionStarted(modelsToDelete, cancelEventArgs);

            //Inside the OnDeletionStarted there is a condition checking the Cancel state, then we need to set the value to true
            cancelEventArgs.Cancel = true;
            CurrentDynamoModel.OnDeletionStarted(modelsToDelete, cancelEventArgs);

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            CurrentDynamoModel.DeletionStarted -= CurrentDynamoModel_DeletionStarted;
            Assert.IsTrue(deletionStarted);
        }

        /// <summary>
        /// This test method will execute the event OnDeletionComplete
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnDeletionComplete()
        {
            //Arrange
            //This will subscribe our local method to the DeletionComplete event
            CurrentDynamoModel.DeletionComplete += CurrentDynamoModel_DeletionComplete;
          

            //Act
            var cancelEventArgs = new EventArgs();
            CurrentDynamoModel.OnDeletionComplete(this, cancelEventArgs);

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            CurrentDynamoModel.DeletionComplete -= CurrentDynamoModel_DeletionComplete;
            Assert.IsTrue(deletionComplete);
        }

        /// <summary>
        /// This test method will execute the private method OnRequestCancelActiveStateForNode
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestCancelActiveStateForNode()
        {
            //Arrange
            //This will subscribe our local method to the RequestCancelActiveStateForNode event
            CurrentDynamoModel.RequestCancelActiveStateForNode += CurrentDynamoModel_RequestCancelActiveStateForNode;
            var codeBlockNode1 = CreateCodeBlockNode();

            //Act
            //Using reflection we execute the private OnRequestCancelActiveStateForNode method passing the code block node as parameter
            MethodInfo dynMethod = typeof(DynamoModel).GetMethod("OnRequestCancelActiveStateForNode", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(CurrentDynamoModel, new object[] { codeBlockNode1 });

            //Assert
            CurrentDynamoModel.RequestCancelActiveStateForNode -= CurrentDynamoModel_RequestCancelActiveStateForNode;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestCancelActiveStateForNode);
        }

        /// <summary>
        /// This test method will execute the method OnRequestsRedraw
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestsRedraw()
        {
            //Arrange
            //This will subscribe our local method to the RequestsRedraw event
            CurrentDynamoModel.RequestsRedraw += CurrentDynamoModel_RequestsRedraw;

            //Act
            //This will execute the OnRequestsRedraw method 
            CurrentDynamoModel.OnRequestsRedraw(this, new ModelEventArgs(null));

            //Assert
            CurrentDynamoModel.RequestsRedraw -= CurrentDynamoModel_RequestsRedraw;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestsRedraw);
        }

        /// <summary>
        /// This test method will execute the method OnRequestSelect()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestSelect()
        {
            //Arrange
            //This will subscribe our local method to the RequestNodeSelect event
            CurrentDynamoModel.RequestNodeSelect += CurrentDynamoModel_RequestNodeSelect;

            //Act
            //This will execute the OnRequestSelect() method
            CurrentDynamoModel.OnRequestSelect(this, new ModelEventArgs(null));

            //Assert
            CurrentDynamoModel.RequestNodeSelect -= CurrentDynamoModel_RequestNodeSelect;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestNodeSelect);
        }

        /// <summary>
        /// This test method will execute the method OnRunCompleted()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRunCompleted()
        {
            //Arrange
            //This will subscribe our local method to the RunCompleted event
            CurrentDynamoModel.RunCompleted += CurrentDynamoModel_RunCompleted;

            //Act
            //This will execute the OnRunCompleted() method 
            CurrentDynamoModel.OnRunCompleted(this, true);

            //Assert
            //Unsubcribe from event
            CurrentDynamoModel.RunCompleted -= CurrentDynamoModel_RunCompleted;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(runCompleted);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent method OnRequestsCrashPrompt()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestsCrashPrompt()
        {
            //Arrange
            //This will subscribe our local method to the RequestsCrashPrompt event
            CurrentDynamoModel.RequestsCrashPrompt += CurrentDynamoModel_RequestsCrashPrompt;
            var crashArgs = new Dynamo.Core.CrashPromptArgs("Crash Event", "Test Message");

            //Act
            CurrentDynamoModel.OnRequestsCrashPrompt(this, crashArgs);

            //Assert
            //Unsubcribe from event
            CurrentDynamoModel.RequestsCrashPrompt -= CurrentDynamoModel_RequestsCrashPrompt;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestsCrashPrompt);
        }

     
        /// <summary>
        /// This test method will execute the method OnRequestTaskDialog()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestTaskDialog()
        {
            //Arrange
            //This will subscribe our local method to the RequestTaskDialog event
            CurrentDynamoModel.RequestTaskDialog += CurrentDynamoModel_RequestTaskDialog;
            var args = new TaskDialogEventArgs(
              new Uri("localhost", UriKind.Relative), "Test Dialog", "Summary", "Description");

            //Act
            CurrentDynamoModel.OnRequestTaskDialog(null, args);

            //Assert
            CurrentDynamoModel.RequestTaskDialog -= CurrentDynamoModel_RequestTaskDialog;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestTaskDialog);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent method OnRequestDownloadDynamo()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestDownloadDynamo()
        {
            //Arrange
            //This will subscribe our local method to the RequestDownloadDynamo event
            CurrentDynamoModel.RequestDownloadDynamo += CurrentDynamoModel_RequestDownloadDynamo;

            //Act
            CurrentDynamoModel.OnRequestDownloadDynamo();

            //Assert
            CurrentDynamoModel.RequestDownloadDynamo -= CurrentDynamoModel_RequestDownloadDynamo;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestDownloadDynamo);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent method OnRequestBugReport()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestBugReport()
        {
            //Arrange
            //This will subscribe our local method to the RequestBugReport event
            CurrentDynamoModel.RequestBugReport += CurrentDynamoModel_RequestBugReport;

            //Act
            CurrentDynamoModel.OnRequestBugReport();

            //Assert
            CurrentDynamoModel.RequestBugReport -= CurrentDynamoModel_RequestBugReport;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestBugReport);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent method OnRefreshCompleted()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRefreshCompleted()
        {
            //Arrange
            //This will subscribe our local method to the RefreshCompleted event
            CurrentDynamoModel.RefreshCompleted += CurrentDynamoModel_RefreshCompleted;

            //Act
            CurrentDynamoModel.OnRefreshCompleted(CurrentDynamoModel.CurrentWorkspace, new EventArgs());

            //Assert
            CurrentDynamoModel.RefreshCompleted -= CurrentDynamoModel_RefreshCompleted;
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(refreshCompleted);
        }



        #region SubscriberEvents
        private void CurrentDynamoModel_RefreshCompleted(Graph.Workspaces.HomeWorkspaceModel obj)
        {
            refreshCompleted = true;
        }

        private void CurrentDynamoModel_EvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            evaluationCompleted = true;
        }

        private void CurrentDynamoModel_RequestBugReport()
        {
            requestBugReport = true;
        }

        private void CurrentDynamoModel_RequestDownloadDynamo()
        {
            requestDownloadDynamo = true;
        }

        private void CurrentDynamoModel_RequestTaskDialog(object sender, TaskDialogEventArgs e)
        {
            requestTaskDialog = true;
        }

        private void CurrentDynamoModel_RequestsCrashPrompt(object sender, Dynamo.Core.CrashPromptArgs e)
        {
            requestsCrashPrompt = true;
        }

        private void CurrentDynamoModel_RunCompleted(object sender, bool success)
        {
            runCompleted = true;
        }

        private void CurrentDynamoModel_RequestNodeSelect(object sender, EventArgs e)
        {
            requestNodeSelect = true;
        }

        private void CurrentDynamoModel_RequestsRedraw(object sender, EventArgs e)
        {
            requestsRedraw = true;
        }

        private void CurrentDynamoModel_RequestCancelActiveStateForNode(Graph.Nodes.NodeModel node)
        {
            requestCancelActiveStateForNode = true;
        }

        private void CurrentDynamoModel_DeletionComplete(object sender, EventArgs e)
        {
            deletionComplete = true;
        }

        private void CurrentDynamoModel_DeletionStarted()
        {
            deletionStarted = true;
        }

        private void CurrentDynamoModel_WorkspaceRemoveStarted(Graph.Workspaces.WorkspaceModel obj)
        {
            workspaceRemoveStarted = true;
        }

        private void CurrentDynamoModel_WorkspaceClearing()
        {
            workspaceClearing = true;
        }

        private void Model_RequestLayoutUpdate(object sender, EventArgs e)
        {
            layoutUpdate = true;
        }

        private void DynamoModel_RequestMigrationStatusDialog(SettingsMigrationEventArgs args)
        {
            migrationStatusDialog = true;
        }

        private void DynamoModel_RequestDispatcherBeginInvoke(Action action)
        {
            dispatcherExecuted = true;
        }
        #endregion
    }
}
