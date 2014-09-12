using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Models;
using System.Collections.Generic;
using Dynamo.Nodes;
using DSCoreNodesUI;
using ProtoCore.Mirror;
using Dynamo.Tests;
using Dynamo.Controls;
using Dynamo.ViewModels;
using System;

namespace Dynamo.Tests
{
    public delegate void CommandCallback(string commandTag);
    [TestFixture]
    class DynamoDefects : DSEvaluationViewModelUnitTest
    {
        private IEnumerable<string> customNodesToBeLoaded = null;
        private CommandCallback commandCallback = null;

        protected DynamoView dynamoView = null;
        protected WorkspaceModel workspace = null;
        protected WorkspaceViewModel workspaceViewModel = null;
        // Note: Pelase only add test cases those are related to defects.

        [Test, Category("RegressionTests")]
        public void T01_Defect_MAGN_110()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_110.dyn");
            RunModel(openPath);
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {2,1},
            };

            SelectivelyAssertPreviewValues("339dd778-8d2c-4ae2-9fdc-26c1572f8eb6", validationData);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_Equal()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_Equal.dyn");
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {5,1},
                {10,0},          
            };

            SelectivelyAssertPreviewValues("3806c656-56bd-4878-9082-b2d27644abd1", validationData);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_GreaterThan()
        {
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_GreaterThan.dyn");
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {1,0},
                {7,1},    
                {9,1},
            };

            SelectivelyAssertPreviewValues("7ec8271d-be03-4d53-ae78-b94c4db484e1", validationData);

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_GreaterThanOrEqual()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_GreaterThanOrEqual.dyn");
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {2,0},
                {4,0},
                {6,1},    
                {8,1},
            };

            SelectivelyAssertPreviewValues("3806c656-56bd-4878-9082-b2d27644abd1", validationData);

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_LessThan()
        {
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_LessThan.dyn");
            RunModel(openPath);

            AssertPreviewValue("7ec8271d-be03-4d53-ae78-b94c4db484e1", new int[] { 1, 1, 1, 1, 1 });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_LessThanOrEqual()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_LessThanOrEqual.dyn");
            RunModel(openPath);

            AssertPreviewValue("6adba162-ef10-4664-9c9c-d0280a56a52a", new int[] { 0, 1, 1, 1, 0, 1 });

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1206()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1206
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_1206.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            ViewModel.Model.RunExpression();
            var add = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DSFunction>("ccb2eda9-0966-4ab8-a186-0d5f844559c1");
            Assert.AreEqual(20, add.CachedValue.Data);
        }

        [Test, Category("RegressionTests")]
        [Category("Failure")]
        public void Defect_MAGN_2566()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2566
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_2566.dyn");
            RunModel(openPath);

             // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            //ViewModel.Model.RunExpression();

            // Checking Point.X
            AssertPreviewValue("eea90465-db68-4494-a85e-4d7c687b68e6", 0);

            // Checking Sphere.Radius
            AssertPreviewValue("18dbc746-18a4-4e4e-839b-14523e648f79", 1);

            // Checking Cylinder.Radius
            AssertPreviewValue("ca9b1501-bc63-49f4-af2d-8d4b4aab2e80", new int[] { 1, 1 });

            // Checking Cylinder.Height
            AssertPreviewValue("310b22ae-e3ce-462a-8391-c7e0c9a532f2", new int[] { 10, 1 });

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3256()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3256
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3256.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            ViewModel.Model.RunExpression();

            AssertPreviewValue("21780859-f85e-44ff-bedb-bd016ca7398d", 5.706339);

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3646()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3646
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3646.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);

            ViewModel.Model.RunExpression();

            AssertPreviewValue("d12c17f4-2f73-42fa-9990-7fe9a723e6a1", 0.00001);
            AssertPreviewValue("d12c17f4-2f73-42fa-9990-7fe9a723e6a1", 0.0000000000000001);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3648()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3648

            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3648.dyn");

            OpenModel(openPath);

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            //Check the CBN for input and output ports count and for error as well.
            string nodeID = "e378c03e-4aae-4bde-b4c5-f16a6bed358f";
            var cbn = model.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            AssertPreviewValue(nodeID, 1);

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3468()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3468
            var model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3468.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1905()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1905
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_1905.dyn");
            RunModel(openPath);
            AssertPreviewValue("c4b9077d-3e6c-40b9-a715-078083e29655", 11);
            BoolSelector b = model.CurrentWorkspace.NodeFromWorkspace
                ("c9da5b60-9d52-453b-836d-0682687728bf") as BoolSelector;
            b.Value = true;
            RunCurrentModel();
            AssertPreviewValue("c4b9077d-3e6c-40b9-a715-078083e29655", 3);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3113()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3113
            RunCommandsFromFile("Defect_MAGN_3113.xml", true);
            AssertPreviewValue("2b50812f-c6bf-449e-9dd4-f6a906ad4473", new int[] { 6 });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3726()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3726
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3726.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            ViewModel.Model.RunExpression();

            AssertPreviewValue("02985f61-2ece-4fe2-b78a-dfb21aa589ff",
                new string[] { "0a", "10a", "20a", "30a", "40a", "50a" });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_847()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-847
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_847.dyn");
            RunModel(openPath);
            AssertPreviewCount("2ea813c4-7729-45b5-b23b-d7a3377f0b31", 4);
            DoubleInput doubleInput = model.CurrentWorkspace.NodeFromWorkspace
                ("7eba96c0-4715-47f0-a874-01f1887ac465") as DoubleInput;
            doubleInput.Value = "6..8";
            RunCurrentModel();
            AssertPreviewCount("2ea813c4-7729-45b5-b23b-d7a3377f0b31", 3);

        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3548()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3548
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3548.dyn");
            RunModel(openPath);
            var point = model.CurrentWorkspace.NodeFromWorkspace
                ("a3da7834-f56f-4e73-b8f1-56796b6c37b3");
            string var = point.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = ViewModel.Model.EngineController.GetMirror(var);
            Assert.IsNotNull(mirror);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_4105()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4105
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_4105.dyn");
            RunModel(openPath);
            AssertPreviewCount("1499d976-e7d5-486f-89bf-bc050eac4489", 4);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_4046()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4046
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_4046.dyn");
            RunModel(openPath);
            AssertPreviewCount("354ec30b-b13f-4399-beb2-a68753c09bfc", 1);
            IntegerSlider integerInput = model.CurrentWorkspace.NodeFromWorkspace
                ("65d226ea-cfb5-4c5a-940e-a5c4eab1915d") as IntegerSlider;
            for (int i = 0; i <= 10; i++)
            {
                integerInput.Value = 5 + i;
                RunCurrentModel();
                AssertPreviewCount("354ec30b-b13f-4399-beb2-a68753c09bfc", 1);
            }
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3998()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3998
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3998.dyn");
            RunModel(openPath);
            AssertPreviewCount("7e825844-c428-4067-a916-11ff14bc0715", 100);
        }

        protected void RunCommandsFromFile(string commandFileName,
            bool autoRun = false, CommandCallback commandCallback = null)
        {
            this.ViewModel = null;
            string commandFilePath = GetTestDirectory(ExecutingDirectory);
            commandFilePath = Path.Combine(commandFilePath, @"core\DynamoDefects\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            if (this.ViewModel != null)
            {
                var message = "Multiple DynamoViewModel instances detected!";
                throw new InvalidOperationException(message);
            }

            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true
                });

            // Create the DynamoViewModel to control the view
            this.ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    CommandFilePath = commandFilePath,
                    DynamoModel = model
                });

            this.ViewModel.DynamicRunEnabled = autoRun;

            // Load all custom nodes if there is any specified for this test.
            if (this.customNodesToBeLoaded != null)
            {
                foreach (var customNode in this.customNodesToBeLoaded)
                {
                    if (ViewModel.Model.CustomNodeManager.AddFileToPath(customNode) == null)
                    {
                        throw new System.IO.FileFormatException(string.Format(
                            "Failed to load custom node: {0}", customNode));
                    }
                }
            }

            RegisterCommandCallback(commandCallback);

            // Create the view.
            dynamoView = new DynamoView(this.ViewModel);
            dynamoView.ShowDialog();
            workspace = this.ViewModel.Model.CurrentWorkspace;
            workspaceViewModel = this.ViewModel.CurrentSpaceViewModel;
        }

        public static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
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
    }
}
