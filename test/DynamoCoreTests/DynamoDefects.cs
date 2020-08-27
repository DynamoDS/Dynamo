﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;
using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    [TestFixture]
    class DynamoDefects : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("Analysis.dll");
            base.GetLibrariesToPreload(libraries);
        }

        // Note: Please only add test cases those are related to defects.

        [Test, Category("RegressionTests")]
        public void T01_Defect_MAGN_110()
        {
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_110.dyn");
            RunModel(openPath);

            var validationData = new Dictionary<int, object>
            {
                {2,1},
            };

            SelectivelyAssertPreviewValues("339dd778-8d2c-4ae2-9fdc-26c1572f8eb6", validationData);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_Equal()
        {
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_942_Equal.dyn");
            RunModel(openPath);

            var validationData = new Dictionary<int, object>
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
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_942_GreaterThan.dyn");
            RunModel(openPath);

            var validationData = new Dictionary<int, object>
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
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_942_GreaterThanOrEqual.dyn");
            RunModel(openPath);

            var validationData = new Dictionary<int, object>
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
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_942_LessThan.dyn");
            RunModel(openPath);

            AssertPreviewValue("7ec8271d-be03-4d53-ae78-b94c4db484e1", new[] { 1, 1, 1, 1, 1 });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_942_LessThanOrEqual()
        {
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_942_LessThanOrEqual.dyn");
            RunModel(openPath);

            AssertPreviewValue("6adba162-ef10-4664-9c9c-d0280a56a52a", new[] { 0, 1, 1, 1, 0, 1 });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1206()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1206
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_1206.dyn");
            RunModel(openPath);

            var add = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("ccb2eda9-0966-4ab8-a186-0d5f844559c1");
            AssertPreviewValue("ccb2eda9-0966-4ab8-a186-0d5f844559c1", 20);
        }

        [Test, Category("RegressionTests")]
        [Category("Failure")]
        public void Defect_MAGN_2566()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2566
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_2566.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //ViewModel.HomeSpace.Run();

            // Checking Point.X
            AssertPreviewValue("eea90465-db68-4494-a85e-4d7c687b68e6", 0);

            // Checking Sphere.Radius
            AssertPreviewValue("18dbc746-18a4-4e4e-839b-14523e648f79", 1);

            // Checking Cylinder.Radius
            AssertPreviewValue("ca9b1501-bc63-49f4-af2d-8d4b4aab2e80", new[] { 1, 1 });

            // Checking Cylinder.Height
            AssertPreviewValue("310b22ae-e3ce-462a-8391-c7e0c9a532f2", new[] { 10, 1 });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3256()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3256
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3256.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            BeginRun();

            AssertPreviewValue("21780859-f85e-44ff-bedb-bd016ca7398d", 5.706339);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3646()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3646
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3646.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            BeginRun();

            AssertPreviewValue("d12c17f4-2f73-42fa-9990-7fe9a723e6a1", 0.00001);
            AssertPreviewValue("d12c17f4-2f73-42fa-9990-7fe9a723e6a1", 0.0000000000000001);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3648()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3648
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3648.dyn");

            OpenModel(openPath);
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //Check the CBN for input and output ports count and for error as well.
            string nodeID = "e378c03e-4aae-4bde-b4c5-f16a6bed358f";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            Assert.DoesNotThrow(BeginRun);

            AssertPreviewValue(nodeID, 1);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_2169()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2169
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_2169.dyn");
            RunModel(openPath);
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("ff354c76-cbcc-4903-a88a-89184905dba0");
            string var = cbn.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = CurrentDynamoModel.EngineController.GetMirror(var);
            Assert.IsNotNull(mirror);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3468()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3468
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3468.dyn");
            OpenModel(openPath);

            Assert.DoesNotThrow(BeginRun);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_2264()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2264
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_2264.dyn");
            OpenModel(openPath);

            Assert.DoesNotThrow(BeginRun);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1292()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1292
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_1292.dyn");
            RunModel(openPath);
            var listContainsItemNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("3011fcbe-00d5-42e6-84c8-55bdf38b6a3b");
            string var = listContainsItemNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = CurrentDynamoModel.EngineController.GetMirror(var);
            Assert.IsNotNull(mirror);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1905()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1905
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_1905.dyn");
            RunModel(openPath);
            AssertPreviewValue("c4b9077d-3e6c-40b9-a715-078083e29655", 11);
            BoolSelector b = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("c9da5b60-9d52-453b-836d-0682687728bf") as BoolSelector;
            b.Value = true;
            BeginRun();
            AssertPreviewValue("c4b9077d-3e6c-40b9-a715-078083e29655", 3);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3726()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3726
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3726.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            BeginRun();

            AssertPreviewValue("02985f61-2ece-4fe2-b78a-dfb21aa589ff",
                new[] { "0a", "10a", "20a", "30a", "40a", "50a" });
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_847()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-847
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_847.dyn");
            RunModel(openPath);
            AssertPreviewCount("2ea813c4-7729-45b5-b23b-d7a3377f0b31", 4);
            var doubleInput = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("7eba96c0-4715-47f0-a874-01f1887ac465") as CodeBlockNodeModel;
            doubleInput.SetCodeContent("6..8;", new ProtoCore.Namespace.ElementResolver());
            BeginRun();
            AssertPreviewCount("2ea813c4-7729-45b5-b23b-d7a3377f0b31", 3);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3548()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3548
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3548.dyn");
            RunModel(openPath);
            var point = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("a3da7834-f56f-4e73-b8f1-56796b6c37b3");
            string var = point.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = CurrentDynamoModel.EngineController.GetMirror(var);
            Assert.IsNotNull(mirror);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_4105()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4105
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_4105.dyn");
            RunModel(openPath);
            AssertPreviewCount("1499d976-e7d5-486f-89bf-bc050eac4489", 4);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_2555()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2555
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_2555.dyn");
            RunModel(openPath);
            var geometryTranslateNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("f49e3857-2a08-4a1f-83ac-89f64b24a592");
            string var = geometryTranslateNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = CurrentDynamoModel.EngineController.GetMirror(var);
            Assert.IsNotNull(mirror);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1971()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1971
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_1971.dyn");
            RunModel(openPath);
            var pythonNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("768780b5-0902-4756-93dc-2d6a8690df53");
            string var = pythonNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = CurrentDynamoModel.EngineController.GetMirror(var);
            Assert.IsNotNull(mirror);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_4046()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4046
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_4046.dyn");
            RunModel(openPath);
            AssertPreviewCount("354ec30b-b13f-4399-beb2-a68753c09bfc", 1);
            var integerInput = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("65d226ea-cfb5-4c5a-940e-a5c4eab1915d") as IntegerSlider;
            for (int i = 0; i <= 10; i++)
            {
                integerInput.Value = 5 + i;
                BeginRun();
                AssertPreviewCount("354ec30b-b13f-4399-beb2-a68753c09bfc", 1);
            }
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_3998()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3998
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_3998.dyn");
            RunModel(openPath);
            AssertPreviewCount("7e825844-c428-4067-a916-11ff14bc0715", 100);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_2607()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2607
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_2607.dyn");
            RunModel(openPath);
            AssertPreviewValue("99975a42-f887-4b99-9b0a-e36513d2bd6d", 12);
            var input = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("7cbafd1f-cec2-48b2-ac52-c9605acfb644") as IntegerSlider;
            input.Value = 12;
            BeginRun();
            AssertPreviewValue("99975a42-f887-4b99-9b0a-e36513d2bd6d", 24);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_1968()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1968
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_1968.dyn");
            RunModel(openPath);
            int[] listResult = { 0, 1, 2 };
            AssertPreviewValue("522e092c-4493-4959-9b89-a02d045070cc", listResult);
        }

        [Test, Category("RegressionTests")]
        public void Defect_MAGN_4364()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4364
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Defect_MAGN_4364.dyn");

            RunModel(openPath);
            string watchNodeID1 = "4d3e33b8-877e-4c7f-9b2e-25c5e2f7ca83";

            AssertPreviewValue(watchNodeID1, new[] { "10", "Jack", "Dynamo", "King", "40" });

            string watchNodeID2 = "0a0f829d-fe4d-4f82-8565-dd620f39b702";
            AssertPreviewValue(watchNodeID2, new[] { "foo", "bar" });

            string watchNodeID3 = "2ac27d00-2c4f-4a07-8ae4-e5ce8784e2dc";
            AssertPreviewValue(watchNodeID3, new[] { "a", "b", "c", "d" });

            string watchNodeID4 = "b96146c7-7fbc-4ae5-b8e4-563c72bd6522";
            AssertPreviewValue(watchNodeID4, "a");

            string watchNodeID5 = "0805b17c-c007-461e-9c2e-33dc2bc6551e";
            AssertPreviewValue(watchNodeID5, new[] { 1, 2, 3 });

            string watchNodeID6 = "775db841-b486-4e41-9a6b-2f319696d469";
            AssertPreviewValue(watchNodeID6, new[] { 1, 2, 3, 4 });

            string watchNodeID7 = "862f1f50-4a40-47e0-b0aa-d8c9a28f4597";
            AssertPreviewValue(watchNodeID7, new[] { 1, 2, 1, 2 });
        }

        [Test, Category("RegressionTests")]
        public void Boolean_LowerCase_3420()
        {
            // No consistent notation of true / false
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3420
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\Bool_Case_3420.dyn");
            RunModel(openPath);
            AssertPreviewValue("ebd1f642-ee18-46d7-ad76-490d6adcf3f0", true);
            AssertPreviewValue("457dfbe9-0c65-4c43-aa83-667069ef562a", 6.9115038378975449);

            AssertPreviewValue("bf766c0b-15b1-49f7-90e3-bc3db3dd1987", true);
        }

        [Test, Category("RegressionTests")]
        public void VMFailOnCBN_5173()
        {
            // VM CompilerInternalException on CBN with Vector.ZAxis
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5173
            string openPath = Path.Combine(TestDirectory, @"core\DynamoDefects\VMFailOnCBN_5173.dyn");
            OpenModel(openPath);
            Assert.DoesNotThrow(BeginRun);
        }

        [Test, Category("RegressionTests")]
        public void Defect5561_ListCombineCrash_AddingArrayToSingleItem()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5561
            string openPath = Path.Combine(TestDirectory, 
                        @"core\DynamoDefects\5561_ListCombineCrash_AddingArrayToSingleItem.dyn");
            RunModel(openPath);

            AssertPreviewCount("d24b1f14-c31e-4ee9-9a1b-e3ebfbb936ba", 101);
            var combinedValue = GetPreviewValueAtIndex("d24b1f14-c31e-4ee9-9a1b-e3ebfbb936ba", 0);
            Assert.IsNotNull(combinedValue);
        }

        [Test, Category("RegressionTests")]
        public void EqualEqualTest_ForStrings_Defect6694()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6694

            string openPath = Path.Combine(TestDirectory, 
                                        @"core\DynamoDefects\EqualEqualTestForSting_6694.dyn");
            RunModel(openPath);
            // check for number of Nodes and Connectors
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            // Different Stinrg input
            AssertPreviewValue("68e9538f-278d-44a7-8b35-a594cea07dca", false);

            // Same Stinrg input
            AssertPreviewValue("56f3c0fd-d39c-46cb-a4ea-4f266f7a9fce", true);
        }

        [Test]
        public void CyclicDependency_Defect8827()
        {
            string openPath = Path.Combine(TestDirectory,
                                        @"core\DynamoDefects\Defect_MAGN_8827.dyn");
            RunModel(openPath);
            // check for number of Nodes and Connectors
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("385281c8-3177-4359-a348-a3084e16a41a");
            var node2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("1ad8632e-7ddc-4cc7-bfa5-58cb899a5ddf");

            Assert.IsTrue(node1.ToolTipText.Equals(ProtoCore.Properties.Resources.kInvalidStaticCyclicDependency));
            Assert.IsTrue(node2.ToolTipText.Equals(ProtoCore.Properties.Resources.kInvalidStaticCyclicDependency));
        }
    }
}
