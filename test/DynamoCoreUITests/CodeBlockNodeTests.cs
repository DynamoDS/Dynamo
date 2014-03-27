using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    //public delegate void CommandCallback(string commandTag);

    [TestFixture]
    public class CodeBlockNodeTests : DSEvaluationUnitTest
    {
        #region Generic Set-up Routines and Data Members

        private System.Random randomizer = null;
        private CommandCallback commandCallback = null;

        // For access within test cases.
        protected WorkspaceModel workspace = null;
        protected WorkspaceViewModel workspaceViewModel = null;

        public override void Init()
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
            if (this.Controller != null)
            {
                this.Controller.ShutDown(true);
                this.Controller = null;
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



        #region Private Helper Methods

        protected ModelBase GetNode(string guid)
        {
            Guid id = Guid.Parse(guid);
            return workspace.GetModelInternal(id);
        }

        protected void RunCommandsFromFile(string commandFileName,
            bool autoRun = false, CommandCallback commandCallback = null)
        {
            string commandFilePath = DynamoTestUI.GetTestDirectory(ExecutingDirectory);
            commandFilePath = Path.Combine(commandFilePath, @"core\recorded\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            if (this.Controller != null)
            {
                var message = "Multiple DynamoController detected!";
                throw new InvalidOperationException(message);
            }

            // Create the controller to run alongside the view.
            this.Controller = DynamoController.MakeSandbox(commandFilePath);
            var controller = this.Controller;
            controller.DynamoViewModel.DynamicRunEnabled = autoRun;
            DynamoController.IsTestMode = true;

            RegisterCommandCallback(commandCallback);

            // Create the view.
            var dynamoView = new DynamoView();
            dynamoView.DataContext = controller.DynamoViewModel;
            controller.UIDispatcher = dynamoView.Dispatcher;
            dynamoView.ShowDialog();

            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.DynamoModel);
            Assert.IsNotNull(controller.DynamoModel.CurrentWorkspace);
            workspace = controller.DynamoModel.CurrentWorkspace;
            workspaceViewModel = controller.DynamoViewModel.CurrentSpaceViewModel;
        }

        private void RegisterCommandCallback(CommandCallback commandCallback)
        {
            if (commandCallback == null)
                return;

            if (this.commandCallback != null)
                throw new InvalidOperationException("RunCommandsFromFile called twice");

            this.commandCallback = commandCallback;
            var automation = this.Controller.DynamoViewModel.Automation;
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
            where CmdType : DynamoViewModel.RecordableCommand
        {
            Assert.IsNotNull(command); // Ensure we have an input command.

            // Serialize the command into an XmlElement.
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement element = command.Serialize(xmlDocument);
            Assert.IsNotNull(element);

            // Deserialized the XmlElement into a new instance of the command.
            var duplicate = DynamoViewModel.RecordableCommand.Deserialize(element);
            Assert.IsNotNull(duplicate);
            Assert.IsTrue(duplicate is CmdType);
            return duplicate as CmdType;
        }

        #endregion
    }
}
