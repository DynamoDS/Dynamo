using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Scheduler;
using Microsoft.Win32;
using NUnit.Framework;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class DynamoAnalyticsClientTest : DynamoModelTestBase
    {
        private const string DEFAULT_RETURN_VALUE = "Default Not Found";
        private const string SHUTDOWN_TYPE_NAME = "CleanShutdown";
        private const string REG_KEY = "HKEY_CURRENT_USER\\Software\\DynamoStability";

        //We need to override this function because the one in DynamoModelTestBase is setting StartInTestMode = true
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = false,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous
            };
        }

        [SetUp]
        public void Init()
        {

        }

        /// <summary>
        /// This test method will execute the StabilityCookie.WriteCrashingShutdown(); method
        /// Also it will execute the event of adding a workspace model (AnalyticsService.OnWorkspaceAdded)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestDynamoAnalyticsSession_Dispose()
        {
            //Simulating that the system crashed by some reason
            DynamoModel.IsCrashing = true; //This will allow to execute the  StabilityCookie.WriteCrashingShutdown(); method inside the Dispose method

            //Arrange
            // Open/Run XML test graph
            string openPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            RunModel(openPath);
            int InitialNodesCount = CurrentDynamoModel.CurrentWorkspace.Nodes.Count();

            // Convert a DSFunction node Line.ByPointDirectionLength to custom node.
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var node = workspace.Nodes.OfType<DSFunction>().First();

            List<NodeModel> selectionSet = new List<NodeModel>() { node };
            var customWorkspace = CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet.AsEnumerable(),
                Enumerable.Empty<Dynamo.Graph.Notes.NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__AnalyticsServiceTest__",
                    Success = true
                }) as CustomNodeWorkspaceModel;

            //Act
            //This will execute the custom workspace assigment and trigger the workspace assigment event
            //The DynamoAnalyticsSession.Dispose() is executed automatically inside the Model and it will go to the crashing section.
            CurrentDynamoModel.OpenCustomNodeWorkspace(customWorkspace.CustomNodeId);

            //This will add a new custom node to the workspace
            var ws = CurrentDynamoModel.CustomNodeManager.CreateCustomNode("someNode", "someCategory", "");
            var csid = (ws as CustomNodeWorkspaceModel).CustomNodeId;
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(csid);

            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);

            //This will get the value recorded in Registry about the crash/shoutdown
            string foundRegValue = (string)Registry.GetValue(REG_KEY, SHUTDOWN_TYPE_NAME, DEFAULT_RETURN_VALUE);

            DynamoModel.IsCrashing = false;

            //Assert
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), InitialNodesCount + 1);
            Assert.IsFalse(string.IsNullOrEmpty(foundRegValue));
        }

        /// <summary>
        /// This test method execute the DynamoAnalyticsClient.GetUserID() to reach the end section
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void Test_DynamoAnalytics_GetUserID()
        {
            //Because surely the InstrumentationGUID entry in Registry already exists we need to delete it

            //Arrange
            // The name of the key must include a valid root.
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\DynamoUXG";
            const string keyName = userRoot + "\\" + subkey;
            string guid = string.Empty;

            //First we try to get the value from the Windows Registry
            var tryGetValue = Registry.GetValue(keyName, "InstrumentationGUID", null) as string;

            if (tryGetValue != null)
            {
                //If the keyName already exists then we should remove it because the DynamoAnalyticsSession.GetUserID method will insert the value again
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subkey, true))
                {
                    key.DeleteValue("InstrumentationGUID");
                }
            }

            //This will execute several track events missing from coverage
            Analytics.TrackPreference("Test", "Test Value", 1);

            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Delete, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Open, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Close, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Read, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Write, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Save, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.SaveAs, 5);
            Analytics.TrackFileOperationEvent(this.TempFolder, Actions.New, 5);

            //Act
            guid = DynamoAnalyticsSession.GetUserID();

            //Assert
            //We just check the consistence of the inserted registry value
            Assert.That(Registry.GetValue(keyName, "InstrumentationGUID", null) as string, Is.Not.Null.Or.Empty);
            Assert.That(guid, Is.Not.Null.Or.Empty);
            //The FileAction throws an ArgumentException when the action is not in the list (switch)
            Assert.Throws<ArgumentException>(() => Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Copy, 5));
        }

        [TearDown]
        public void Dispose()
        {
        }
    }
}
