using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using SystemTestServices;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Dynamo.UI.Controls;
using Dynamo.Utilities;

namespace DynamoCoreUITests
{
    //public delegate void CommandCallback(string commandTag);

    [TestFixture]
    public class CodeBlockNodeTests : DSEvaluationViewModelUnitTest
    {
        #region Generic Set-up Routines and Data Members

        private System.Random randomizer = null;
        private CommandCallback commandCallback = null;

        // For access within test cases.
        protected WorkspaceModel workspace = null;
        protected WorkspaceViewModel workspaceViewModel = null;

        public override void Setup()
        {
            // We do not call "base.Init()" here because we want to be able 
            // to create our own copy of Controller here with command file path.
        }

        [SetUp]
        public void Start()
        {
            // Fixed seed randomizer for predictability.
            randomizer = new System.Random(123456);
            SetupDirectories();
        }

        [TearDown]
        protected void Exit()
        {
            if (this.ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false, allowCancellation: false);

                this.ViewModel.PerformShutdownSequence(shutdownParams);
                this.ViewModel = null;
                this.commandCallback = null;
            }

            GC.Collect();
        }

        #endregion

        [Test, RequiresSTA]
        //Run the CBN without an input to get the error message
        //Connent a input to the CBN and run
        //Change the value of the input and run 
        //  : the result will not be updated

        //Create CBN node with Math.Sin(x);
        //Execute
        //Create node with value 10 and attach to the CBN
        //Execute(InitialRun)
        //Update the value from 10 to 0
        //Execute(SecondRun)

        public void CodeBlockNode_ReassignInput()
        {
            RunCommandsFromFile("CodeBlockNode_ReassignInput.xml", false, (commandTag) =>
            {
                if (commandTag == "InitialRun")
                {
                    AssertPreviewValue("e31166a6-083f-4279-bd6b-8bfe57f7ee04", 0.173648);
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("e31166a6-083f-4279-bd6b-8bfe57f7ee04", 0.000000);
                }
            });
        }


        [Test, RequiresSTA]
        //Assign a vale to a CodeBlockNode and run
        //Remove the input of this CBN by assign a literals to the CBN's text and run
        //Change the value of the literals and run 
        //  : the result will not be updated

        //Create CBN node with Math.Sin(x);
        //Create node with value 1 and attach to the CBN
        //Execute(InitialRun)
        //Disattach the two nodes and update the CBN to Math.Sin(2)
        //Execute(SecondRun)
        //Update the CBN to Math.Sin(3)
        //Execute(ThirdRun)

        public void CodeBlockNode_ReassignInput_2()
        {
            RunCommandsFromFile("CodeBlockNode_ReassignInput_2.xml", false, (commandTag) =>
            {
                if (commandTag == "InitialRun")
                {
                    AssertPreviewValue("f2b8dd8d-dcb2-4e50-bc9e-29fd1ecce84b", 0.017452);
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("f2b8dd8d-dcb2-4e50-bc9e-29fd1ecce84b", 0.034899);
                }
                else if (commandTag == "ThirdRun")
                {
                    AssertPreviewValue("f2b8dd8d-dcb2-4e50-bc9e-29fd1ecce84b", 0.052336);
                }
            });
        }


        [Test, RequiresSTA, Category("Failure")]
        // Create a cyclic chain of three code block nodes, and verify that a
        // warning is shown on one of the cyclic nodes.
        // Reconnect a valid value to one of the chain items, and verify that the
        // warning is turned off and the values are evaluated properly.
        // Create another cyclic chain of two nodes, and verify the same behavior.
        //
        public void CodeBlockNode_ReassignCyclic()
        {
            RunCommandsFromFile("CodeBlockNode_ReassignCyclic.xml", true, (commandTag) =>
            {
                var nodeA = GetNode("2e0d1d7e-7ef3-4cf5-9884-93ac77697e5f") as NodeModel;
                var nodeB = GetNode("9699d07d-ec4e-48ad-9a3d-170154a4a106") as NodeModel;
                var nodeC = GetNode("73959903-fd79-4645-9b58-28fe88545f8b") as NodeModel;

                if (commandTag == "NormalThreeNodes")
                {
                    // Create four code block nodes [3;], [a;], [b;], [c;]
                    // Connect [3;]-[a;], connect [a;]-[b;], connect [b;]-[c;]

                    AssertPreviewValue("73959903-fd79-4645-9b58-28fe88545f8b", 3);
                }
                else if (commandTag == "CyclicThreeNodes")
                {
                    // Connect [c;]-[a;]

                    bool hasWarning = false;

                    if (nodeA.State == ElementState.Warning) hasWarning = true;
                    if (nodeB.State == ElementState.Warning) hasWarning = true;
                    if (nodeC.State == ElementState.Warning) hasWarning = true;

                    Assert.AreEqual(true, hasWarning);
                }
                else if (commandTag == "Recover")
                {
                    // Change the code block node [3;] into [4;]
                    // Connect [4;]-[c;]

                    bool hasWarning = false;

                    if (nodeA.State == ElementState.Warning) hasWarning = true;
                    if (nodeB.State == ElementState.Warning) hasWarning = true;
                    if (nodeC.State == ElementState.Warning) hasWarning = true;

                    Assert.AreEqual(false, hasWarning);
                    AssertPreviewValue("73959903-fd79-4645-9b58-28fe88545f8b", 4);
                }
                else if (commandTag == "CyclicTwoNodes")
                {
                    // Create two more code block nodes [d;] and [e;]
                    // Connect [4;]-[d;] and [d;]-[e;], then connect [e;]-[d;]

                    bool hasWarning = false;

                    var nodeD = GetNode("05126ec5-1612-47cb-9ccc-fd96aec269b1") as NodeModel;
                    var nodeE = GetNode("de271687-bb0d-49fc-81a1-e83680250f55") as NodeModel;

                    if (nodeD.State == ElementState.Warning) hasWarning = true;
                    if (nodeE.State == ElementState.Warning) hasWarning = true;

                    Assert.AreEqual(true, hasWarning);
                }
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestSyntaxHighlightRuleForDigits()
        {
            string text = "{-2468.2342E+04, dfsgdfg34534, 34534.345345, 23423, -98.7, 0..10..2, -555};";

            var rule = CodeBlockEditorUtils.CreateDigitRule().Regex;
            var matches = rule.Matches(text);

            // Expected results (8):
            // -2468.2342E+04
            // 34534.345345
            // 23423
            // -98.7
            // 0
            // 10
            // 2
            // -555
            Assert.AreEqual(8, matches.Count);
            var actual = matches.Cast<Match>().Select(m => m.Value).ToArray();
            string[] expected = new string[] { "-2468.2342E+04", "34534.345345", "23423", "-98.7", "0", "10", "2", "-555" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }


        #region Private Helper Methods

        protected ModelBase GetNode(string guid)
        {
            Guid id = Guid.Parse(guid);
            return ViewModel.Model.CurrentWorkspace.GetModelInternal(id);
        }

        protected void RunCommandsFromFile(string commandFileName,
            bool autoRun = false, CommandCallback commandCallback = null)
        {
            string commandFilePath = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            commandFilePath = Path.Combine(commandFilePath, @"core\recorded\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            if (this.ViewModel != null)
            {
                var message = "Multiple DynamoViewModel instances detected!";
                throw new InvalidOperationException(message);
            }

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true
                });

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = model,
                    CommandFilePath = commandFilePath
                });

            ViewModel.HomeSpace.RunSettings.RunType = autoRun ? 
                RunType.Automatic : 
                RunType.Manual;

            RegisterCommandCallback(commandCallback);

            // Create the view.
            var dynamoView = new DynamoView(this.ViewModel);
            dynamoView.ShowDialog();

            Assert.IsNotNull(ViewModel);
            Assert.IsNotNull(ViewModel.Model);
            Assert.IsNotNull(ViewModel.Model.CurrentWorkspace);
            workspace = ViewModel.Model.CurrentWorkspace;
            workspaceViewModel = ViewModel.CurrentSpaceViewModel;
        }

        private void RegisterCommandCallback(CommandCallback commandCallback)
        {
            if (commandCallback == null)
                return;

            if (this.commandCallback != null)
                throw new InvalidOperationException("RunCommandsFromFile called twice");

            this.commandCallback = commandCallback;
            var automation = this.ViewModel.Automation;
            automation.PlaybackStateChanged += OnAutomationPlaybackStateChanged;
        }

        private void OnAutomationPlaybackStateChanged(object sender, PlaybackStateChangedEventArgs e)
        {
            if (e.OldState == AutomationSettings.State.Paused)
            {
                if (e.NewState == AutomationSettings.State.Playing)
                {
                    // Call back to the delegate registered by the test case. We
                    // only handle command transition from Paused to Playing. Note 
                    // that "commandCallback" is not checked against "null" value 
                    // because "OnAutomationPlaybackStateChanged" would not have 
                    // been called if the "commandCallback" was not registered.
                    // 
                    this.commandCallback(e.NewTag);
                }
            }
        }

        private CmdType DuplicateAndCompare<CmdType>(CmdType command)
            where CmdType : DynamoModel.RecordableCommand
        {
            Assert.IsNotNull(command); // Ensure we have an input command.

            // Serialize the command into an XmlElement.
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement element = command.Serialize(xmlDocument);
            Assert.IsNotNull(element);

            // Deserialized the XmlElement into a new instance of the command.
            var duplicate = DynamoModel.RecordableCommand.Deserialize(element);
            Assert.IsNotNull(duplicate);
            Assert.IsTrue(duplicate is CmdType);
            return duplicate as CmdType;
        }

        #endregion
    }

}
