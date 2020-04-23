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


        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnRequestDispatcherBeginInvoke
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestDispatcherBeginInvoke()
        {
            //Arrance
            //We need to create an event and include an exception
            EvaluationCompletedEventArgs e = new EvaluationCompletedEventArgs(true, new Exception("Test"));

            //Act
            //This will call the OnRequestDispatcherBeginInvoke method in the else section (no subscribers to the event)
            CurrentDynamoModel.OnEvaluationCompleted(this, e);

            //This will subscribe our local method to the RequestDispatcherBeginInvoke event
            DynCmd.RequestDispatcherBeginInvoke += DynamoModel_RequestDispatcherBeginInvoke;

            //This will call the OnRequestDispatcherBeginInvoke method when we have subscribers
            CurrentDynamoModel.OnEvaluationCompleted(this, e);

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(dispatcherExecuted);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnRequestDispatcherBeginInvoke
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
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(migrationStatusDialog);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnRequestLayoutUpdate
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
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(layoutUpdate);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnWorkspaceClearing
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnWorkspaceClearing()
        {
            //Act
            //This will subscribe our local method to the RequestLayoutUpdate event
            CurrentDynamoModel.WorkspaceClearing += CurrentDynamoModel_WorkspaceClearing;

            //Act
            //Internall this will execute the OnWorkspaceClearing() method in DynamoModelEvents
            CurrentDynamoModel.ClearCurrentWorkspace();

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(workspaceClearing);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnWorkspaceRemoveStarted
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnWorkspaceRemoveStarted()
        {
            //Act
            //This will subscribe our local method to the WorkspaceRemoveStarted event
            CurrentDynamoModel.WorkspaceRemoveStarted += CurrentDynamoModel_WorkspaceRemoveStarted;

            //Act
            //Internall this will execute the OnWorkspaceRemoveStarted() method in DynamoModelEvents
            CurrentDynamoModel.RemoveWorkspace(CurrentDynamoModel.CurrentWorkspace);

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(workspaceRemoveStarted);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnDeletionStarted
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnDeletionStarted()
        {
            //Act
            //This will subscribe our local method to the DeletionStarted event
            CurrentDynamoModel.DeletionStarted += CurrentDynamoModel_DeletionStarted;
            List<ModelBase> modelsToDelete = new List<ModelBase>();
            var annotations = CurrentDynamoModel.Workspaces.SelectMany(ws => ws.Annotations);

            foreach (var annotation in annotations)
            {
                modelsToDelete.Insert(0, annotation);
            }
            
            //Act
            var cancelEventArgs = new System.ComponentModel.CancelEventArgs();
            CurrentDynamoModel.OnDeletionStarted(modelsToDelete, cancelEventArgs);
            
            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(deletionStarted);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent event OnDeletionComplete
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnDeletionComplete()
        {
            //Act
            //This will subscribe our local method to the DeletionComplete event
            CurrentDynamoModel.DeletionComplete += CurrentDynamoModel_DeletionComplete;
          

            //Act
            var cancelEventArgs = new EventArgs();
            CurrentDynamoModel.OnDeletionComplete(this, cancelEventArgs);

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(deletionComplete);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent private method OnRequestCancelActiveStateForNode
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestCancelActiveStateForNode()
        {
            //Act
            //This will subscribe our local method to the RequestCancelActiveStateForNode event
            CurrentDynamoModel.RequestCancelActiveStateForNode += CurrentDynamoModel_RequestCancelActiveStateForNode;
            var codeBlockNode1 = CreateCodeBlockNode();

            //Act
            //Using reflection we execute the private OnRequestCancelActiveStateForNode method passing the code block node as parameter
            MethodInfo dynMethod = typeof(DynamoModel).GetMethod("OnRequestCancelActiveStateForNode", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(CurrentDynamoModel, new object[] { codeBlockNode1 });

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestCancelActiveStateForNode);
        }

        /// <summary>
        /// This test method will execute the DynamoModelEvent private method OnRequestsRedraw
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestsRedraw()
        {
            //Act
            //This will subscribe our local method to the RequestCancelActiveStateForNode event
            CurrentDynamoModel.RequestsRedraw += CurrentDynamoModel_RequestsRedraw;

            //Act
            //Using reflection we execute the private OnRequestCancelActiveStateForNode method passing the code block node as parameter
            CurrentDynamoModel.OnRequestsRedraw(this, new ModelEventArgs(null));

            //Assert
            //This will validate that the local handler was executed and set the flag in true
            Assert.IsTrue(requestsRedraw);
        }



        #region SubscriberEvents

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
