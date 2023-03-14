using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CoreNodeModels;
using CoreNodeModels.Input;
using CoreNodeModels.Logic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.PackageManager;
using NUnit.Framework;
using PythonNodeModels;

namespace Dynamo.Tests
{
    [Category("JsonTestExclude")]
    public class NodeMigrationTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        #region Dynamo Core Node Migration Tests

        [Test]
        [Category("Failure")]
        public void TestMigration_Analyze_Color()
        {
            TestMigration("TestMigration_Analyze_Color.dyn");
        }

        [Test]
        [Category("Failure")]
        public void TestMigration_Analyze_Structure()
        {
            TestMigration("TestMigration_Analyze_Structure.dyn");
        }

        [Test]
        public void TestMigration_BuiltIn()
        {
            TestMigration("TestMigration_BuiltIn.dyn");
        }

        [Test]
        [Category("Failure")]
        public void TestMigration_Core_Evaluate()
        {
            TestMigration("TestMigration_Core_Evaluate.dyn");
        }

        [Test]
        [Category("Failure")]
        public void TestMigration_Core_Functions()
        {
            TestMigration("TestMigration_Core_Functions.dyn");
        }

        [Test,Category("FailureNET6")]
        public void TestMigration_Core_Input()
        {
            TestMigration("TestMigration_Core_Input.dyn");
        }

        [Test]
        public void TestMigration_DSCoreNodesUI_to_CoreNodeModels()
        {
            TestMigration("TestMigration_DSCoreNodesUI_to_CoreNodeModels.dyn");
        }

        [Test]
        public void TestMigration_Core_List()
        {
            TestMigration("TestMigration_Core_List.dyn");
        }

        [Test]
        public void TestMigration_Core_Scripting()
        {
            TestMigration("TestMigration_Core_Scripting.dyn");
        }

        [Test]
        public void TestMigration_Core_Strings()
        {
            TestMigration("TestMigration_Core_Strings.dyn");
        }

        [Test]
        [Category("Failure")]
        public void TestMigration_Core_Time()
        {
            TestMigration("TestMigration_Core_Time.dyn");
        }

        [Test]
        public void TestMigration_Core_Watch()
        {
            TestMigration("TestMigration_Core_Watch.dyn");
        }

        [Test, Category("FailureNET6")]
        public void TestMigration_Excel()
        {
            TestMigration("TestMigration_Excel.dyn");
        }

        [Test]
        public void TestMigration_File_Directory()
        {
            TestMigration("TestMigration_File_Directory.dyn");
        }
        
        [Test]
        public void TestMigration_ImportExportCSV()
        {
            TestMigration("TestMigration_ImportExportCSV.dyn");
        }

        [Test]
        public void TestMigration_DSCore_IO_FilePath()
        {
            TestMigration("TestMigration_DSCore_IO_FilePath.dyn");
        }

        [Test]
        public void TestMigration_DSCore_Math()
        {
            TestMigration("TestMigration_DSCore_Math.dyn");
        }

        [Test,Category("FailureNET6")]
        public void TestMigration_InputOutput_Excel()
        {
            TestMigration("TestMigration_InputOutput_Excel.dyn");
        }
        //TODO_MSIL pull csv nodes into their own file with a partial class?
        //or mark excel methods windows only.
        [Test]
        public void TestMigration_InputOutput_File()
        {
            TestMigration("TestMigration_InputOutput_File.dyn");
        }

        [Test]
        public void TestMigration_FileSystem()
        {
            TestMigration("TestMigration_Core_FileSystem.dyn");
        }

        [Test]
        [Category("Failure")]
        public void TestMigration_InputOutput_Hardware()
        {
            TestMigration("TestMigration_InputOutput_Hardware.dyn");
        }

        [Test]
        public void TestMigration_Logic_Comparison()
        {
            TestMigration("TestMigration_Logic_Comparison.dyn");
        }

        [Test]
        public void TestMigration_Logic_Conditional()
        {
            TestMigration("TestMigration_Logic_Conditional.dyn");
        }

        [Test]
        public void TestMigration_Logic_Effect()
        {
            TestMigration("TestMigration_Logic_Effect.dyn");
        }

        [Test]
        public void TestMigration_Logic_Math()
        {
            TestMigration("TestMigration_Logic_Math.dyn");
        }

        [Test]
        public void TestMigration_DSCore_Display_ByGeometryColor()
        {
            TestMigration("TestMigration_DSCore_Display_ByGeometryColor.dyn");
        }

        [Test]
        public void TestMigration_Display_ByGeometryColor()
        {
            TestMigration("TestMigration_Display_ByGeometryColor.dyn");
        }

        [Test]
        public void TestMigration_Display_BySurfaceColors()
        {
            TestMigration("TestMigration_Display_BySurfaceColors.dyn");
        }

        [Test]
        public void TestMigration_ColorRange()
        {
            TestMigration("TestMigration_ColorRange.dyn");
        }

        [Test]
        public void TestStringInput()
        {
            OpenModel(GetDynPath("TestStringInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var strNode = workspace.NodeFromWorkspace<StringInput>(
                "dc27fc31-fdad-40b5-906e-bbba9caf43a6");

            Assert.AreEqual(2, workspace.Nodes.Count());
            Assert.AreEqual(1, workspace.Connectors.Count());

            Assert.NotNull(strNode); // Ensure the StringInput node is migrated.
            Assert.AreEqual("First line\r\nSecond line with\ttab\r\nThird line with \"quotes\"", strNode.Value);

            RunCurrentModel(); // Execute the opened file.
            AssertPreviewValue("f6d7a6c3-5df4-45c0-911b-04d39b4c1959", 58);
        }

        [Test]
        public void TestLessThan()
        {
            OpenModel(GetDynPath("TestLessThan.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var dsfn1 = workspace.NodeFromWorkspace<DSFunction>(
                "dfcf9eed-6552-496d-a410-c358aec19bad");
            var dsfn2 = workspace.NodeFromWorkspace<DSFunction>(
                "09f99160-c7d4-4a8b-b8d5-8610c39a1507");
            var dsfn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "3f361451-01e7-4608-bb07-fb80b3a09063");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(dsfn1);
            Assert.NotNull(dsfn2);
            Assert.NotNull(dsfn3); 

            RunCurrentModel();
            AssertPreviewValue("dfcf9eed-6552-496d-a410-c358aec19bad", true);
            AssertPreviewValue("09f99160-c7d4-4a8b-b8d5-8610c39a1507", false);
            AssertPreviewValue("3f361451-01e7-4608-bb07-fb80b3a09063", false);
        }

        [Test]
        public void TestLessThanOrEqual()
        {
            OpenModel(GetDynPath("TestLessThanOrEqual.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var dsfn1 = workspace.NodeFromWorkspace<DSFunction>(
                "0675d6cf-7674-46cd-af68-b1b4b0579dad");
            var dsfn2 = workspace.NodeFromWorkspace<DSFunction>(
                "f9110b2d-f4bb-48a5-89d2-174c9813bcb1");
            var dsfn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "36ca91dc-537d-46bc-80b6-6e5e65bff303");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(dsfn1);
            Assert.NotNull(dsfn2);
            Assert.NotNull(dsfn3);

            RunCurrentModel();
            AssertPreviewValue("0675d6cf-7674-46cd-af68-b1b4b0579dad", true);
            AssertPreviewValue("f9110b2d-f4bb-48a5-89d2-174c9813bcb1", false);
            AssertPreviewValue("36ca91dc-537d-46bc-80b6-6e5e65bff303", true);
        }

        [Test]
        public void TestGreaterThan()
        {
            OpenModel(GetDynPath("TestGreaterThan.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var dsfn1 = workspace.NodeFromWorkspace<DSFunction>(
                "8ed4f1a0-b74b-4ca0-b4bd-69db32918da9");
            var dsfn2 = workspace.NodeFromWorkspace<DSFunction>(
                "57d38d6e-1b66-4e23-93dc-e2399de14417");
            var dsfn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "30fa3b8d-878f-458b-a8da-7867f9144eb9");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(dsfn1);
            Assert.NotNull(dsfn2);
            Assert.NotNull(dsfn3);

            RunCurrentModel();
            AssertPreviewValue("8ed4f1a0-b74b-4ca0-b4bd-69db32918da9", false);
            AssertPreviewValue("57d38d6e-1b66-4e23-93dc-e2399de14417", true);
            AssertPreviewValue("30fa3b8d-878f-458b-a8da-7867f9144eb9", false);
        }

        [Test]
        public void TestGreaterThanOrEqual()
        {
            OpenModel(GetDynPath("TestGreaterThanOrEqual.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var dsfn1 = workspace.NodeFromWorkspace<DSFunction>(
                "7e1c8fcc-3725-4338-aab8-6a55a4dbe705");
            var dsfn2 = workspace.NodeFromWorkspace<DSFunction>(
                "f3d151a6-9c94-43dd-bc5b-e2f585456c63");
            var dsfn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "ecd771cc-5025-4511-8871-cdc1e7318097");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(dsfn1);
            Assert.NotNull(dsfn2);
            Assert.NotNull(dsfn3);

            RunCurrentModel();
            AssertPreviewValue("7e1c8fcc-3725-4338-aab8-6a55a4dbe705", false);
            AssertPreviewValue("f3d151a6-9c94-43dd-bc5b-e2f585456c63", true);
            AssertPreviewValue("ecd771cc-5025-4511-8871-cdc1e7318097", true);
        }

        [Test]
        public void TestEqual()
        {
            OpenModel(GetDynPath("TestEqual.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var dsfn1 = workspace.NodeFromWorkspace<DSFunction>(
                "e617dce2-a65e-45c3-8c43-2cb1d13a47be");
            var dsfn2 = workspace.NodeFromWorkspace<DSFunction>(
                "66787824-b9b6-4f14-abce-08dd4662ce89");
            var dsfn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "07464c17-ca41-42e7-ac93-220e4c50cc0b");

            Assert.AreEqual(6, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(dsfn1);
            Assert.NotNull(dsfn2);
            Assert.NotNull(dsfn3);

            RunCurrentModel();
            AssertPreviewValue("e617dce2-a65e-45c3-8c43-2cb1d13a47be", false);
            AssertPreviewValue("66787824-b9b6-4f14-abce-08dd4662ce89", true);
            AssertPreviewValue("07464c17-ca41-42e7-ac93-220e4c50cc0b", true);
        }

        [Test]
        public void TestAnd()
        {
            OpenModel(GetDynPath("TestAnd.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<And>(
                "0ac391e1-d11a-40ed-96b2-d3aabbdad5c7");
            var logicn2 = workspace.NodeFromWorkspace<And>(
                "0dff8bbb-6a02-444c-8c96-c44c6a248357");
            var logicn3 = workspace.NodeFromWorkspace<And>(
                 "4a61ddb0-999d-412d-9330-52f0a982b214");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);
            Assert.NotNull(logicn3);

            RunCurrentModel();
            AssertPreviewValue("0ac391e1-d11a-40ed-96b2-d3aabbdad5c7", false);
            AssertPreviewValue("0dff8bbb-6a02-444c-8c96-c44c6a248357", false);
            AssertPreviewValue("4a61ddb0-999d-412d-9330-52f0a982b214", true);
        }

        [Test]
        public void TestAnd_NumberInput()
        {
            OpenModel(GetDynPath("TestAnd_NumberInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<And>(
                "0ac391e1-d11a-40ed-96b2-d3aabbdad5c7");
            var logicn2 = workspace.NodeFromWorkspace<And>(
                "0dff8bbb-6a02-444c-8c96-c44c6a248357");
            var logicn3 = workspace.NodeFromWorkspace<And>(
                 "4a61ddb0-999d-412d-9330-52f0a982b214");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);
            Assert.NotNull(logicn3);

            RunCurrentModel();
            AssertPreviewValue("0ac391e1-d11a-40ed-96b2-d3aabbdad5c7", false);
            AssertPreviewValue("0dff8bbb-6a02-444c-8c96-c44c6a248357", false);
            AssertPreviewValue("4a61ddb0-999d-412d-9330-52f0a982b214", true);
        }

        [Test]
        public void TestOr()
        {
            OpenModel(GetDynPath("TestOr.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<Or>(
                "64cfe13f-370c-446e-9f51-58d60278cdff");
            var logicn2 = workspace.NodeFromWorkspace<Or>(
                "a0b23231-737f-44f6-aa12-640ee5390fa5");
            var logicn3 = workspace.NodeFromWorkspace<Or>(
                 "60a2b505-f173-4f65-a42e-cdd3708900f8");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);
            Assert.NotNull(logicn3);

            RunCurrentModel();
            AssertPreviewValue("64cfe13f-370c-446e-9f51-58d60278cdff", true);
            AssertPreviewValue("a0b23231-737f-44f6-aa12-640ee5390fa5", false);
            AssertPreviewValue("60a2b505-f173-4f65-a42e-cdd3708900f8", true);
        }

        [Test]
        public void TestOr_NumberInput()
        {
            OpenModel(GetDynPath("TestOr_NumberInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<Or>(
                "64cfe13f-370c-446e-9f51-58d60278cdff");
            var logicn2 = workspace.NodeFromWorkspace<Or>(
                "a0b23231-737f-44f6-aa12-640ee5390fa5");
            var logicn3 = workspace.NodeFromWorkspace<Or>(
                 "60a2b505-f173-4f65-a42e-cdd3708900f8");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);
            Assert.NotNull(logicn3);

            RunCurrentModel();
            AssertPreviewValue("64cfe13f-370c-446e-9f51-58d60278cdff", true);
            AssertPreviewValue("a0b23231-737f-44f6-aa12-640ee5390fa5", false);
            AssertPreviewValue("60a2b505-f173-4f65-a42e-cdd3708900f8", true);
        }

        [Test]
        public void TestXor()
        {
            OpenModel(GetDynPath("TestXor.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<DSFunction>(
                "950a1260-417d-484f-95e9-5a3d164fc537");
            var logicn2 = workspace.NodeFromWorkspace<DSFunction>(
                "0e1e3c27-436d-48c5-b25f-cd319d836ac1");
            var logicn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "17aa70a5-8038-425a-b049-9627a73a071c");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);
            Assert.NotNull(logicn3);

            RunCurrentModel();
            AssertPreviewValue("950a1260-417d-484f-95e9-5a3d164fc537", true);
            AssertPreviewValue("0e1e3c27-436d-48c5-b25f-cd319d836ac1", false);
            AssertPreviewValue("17aa70a5-8038-425a-b049-9627a73a071c", false);
        }

        [Test]
        public void TestXor_NumberInput()
        {
            OpenModel(GetDynPath("TestXor_NumberInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<DSFunction>(
                "950a1260-417d-484f-95e9-5a3d164fc537");
            var logicn2 = workspace.NodeFromWorkspace<DSFunction>(
                "0e1e3c27-436d-48c5-b25f-cd319d836ac1");
            var logicn3 = workspace.NodeFromWorkspace<DSFunction>(
                 "17aa70a5-8038-425a-b049-9627a73a071c");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);
            Assert.NotNull(logicn3);

            RunCurrentModel();
            AssertPreviewValue("950a1260-417d-484f-95e9-5a3d164fc537", true);
            AssertPreviewValue("0e1e3c27-436d-48c5-b25f-cd319d836ac1", false);
            AssertPreviewValue("17aa70a5-8038-425a-b049-9627a73a071c", false);
        }

        [Test]
        public void TestNot()
        {
            OpenModel(GetDynPath("TestNot.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<DSFunction>(
                "4efaa4dd-00d7-4478-8619-364dd5528637");
            var logicn2 = workspace.NodeFromWorkspace<DSFunction>(
                "6ceae932-650f-409e-a836-009c5e0b9707");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);

            RunCurrentModel();
            AssertPreviewValue("4efaa4dd-00d7-4478-8619-364dd5528637", true);
            AssertPreviewValue("6ceae932-650f-409e-a836-009c5e0b9707", false);
        }

        [Test]
        public void TestNot_NumberInput()
        {
            OpenModel(GetDynPath("TestNot_NumberInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<DSFunction>(
                "4efaa4dd-00d7-4478-8619-364dd5528637");
            var logicn2 = workspace.NodeFromWorkspace<DSFunction>(
                "6ceae932-650f-409e-a836-009c5e0b9707");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);

            RunCurrentModel();
            AssertPreviewValue("4efaa4dd-00d7-4478-8619-364dd5528637", true);
            AssertPreviewValue("6ceae932-650f-409e-a836-009c5e0b9707", false);
        }

        [Test]
        public void TestAdd()
        {
            OpenModel(GetDynPath("TestAdd.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "2b5a7c02-7c21-4c1e-83f6-c8073f8e2473");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "aa4872a0-741b-43fc-8e73-3c1e8655ac3b");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "91fffb4d-f6cc-4770-b9c7-b64accaeca8c");

            Assert.AreEqual(7, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);

            RunCurrentModel();
            AssertPreviewValue("2b5a7c02-7c21-4c1e-83f6-c8073f8e2473", 7);
            AssertPreviewValue("aa4872a0-741b-43fc-8e73-3c1e8655ac3b", 1);
            AssertPreviewValue("91fffb4d-f6cc-4770-b9c7-b64accaeca8c", 4.5);
        }

        [Test]
        public void TestSubtract()
        {
            OpenModel(GetDynPath("TestSubtract.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "c716fe96-15c2-4fc1-a683-ffcdbd864d9d");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "dc8aaac2-6709-4f84-bdfc-432a1cc04f33");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "945b64a3-6504-43f1-87fa-f46c4bc23f1a");

            Assert.AreEqual(7, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);

            RunCurrentModel();
            AssertPreviewValue("c716fe96-15c2-4fc1-a683-ffcdbd864d9d", -3);
            AssertPreviewValue("dc8aaac2-6709-4f84-bdfc-432a1cc04f33", 3);
            AssertPreviewValue("945b64a3-6504-43f1-87fa-f46c4bc23f1a", -0.5);
        }

        [Test]
        public void TestMultiply()
        {
            OpenModel(GetDynPath("TestMultiply.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "a1582b3f-388a-47a5-8785-3ee3700878e1");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "a02fdbea-c30c-439b-a967-b0a46d981344");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "0cd1e263-1c38-4f1e-893d-874b593f939b");

            Assert.AreEqual(7, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);

            RunCurrentModel();
            AssertPreviewValue("a1582b3f-388a-47a5-8785-3ee3700878e1", 10);
            AssertPreviewValue("a02fdbea-c30c-439b-a967-b0a46d981344", -2);
            AssertPreviewValue("0cd1e263-1c38-4f1e-893d-874b593f939b", 5);
        }

        [Test]
        public void TestDivide()
        {
            OpenModel(GetDynPath("TestDivide.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "d2311f8d-2bf8-4aed-bf4b-708b993171ac");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "802d27f8-9259-4050-bd7c-214ff83fa98a");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "5c498c26-1536-4b51-8d0f-f613fc025896");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "1737dfb3-f470-4a32-bac9-34aa4c18606b");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "9b902150-dc32-4e00-8ba6-1819887528ae");

            Assert.AreEqual(10, workspace.Nodes.Count());
            Assert.AreEqual(10, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);

            RunCurrentModel();
            AssertPreviewValue("d2311f8d-2bf8-4aed-bf4b-708b993171ac", 0.4);
            AssertPreviewValue("802d27f8-9259-4050-bd7c-214ff83fa98a", -2);
            AssertPreviewValue("5c498c26-1536-4b51-8d0f-f613fc025896", 0.8);
            AssertPreviewValue("1737dfb3-f470-4a32-bac9-34aa4c18606b", 0);
            //AssertInfinity("9b902150-dc32-4e00-8ba6-1819887528ae");
            
        }

        [Test]
        [Category("Failure")]
        public void TestModulo()
        {
            OpenModel(GetDynPath("TestModulo.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "3f4c4485-5149-479a-aa11-e66e72c76b37");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "99c18e93-bb19-458b-97f3-b3467aa10364");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "85f6a5e8-3e3c-4944-a7cb-ba856d755f87");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "2be464f1-81d0-4427-b588-a22d94e8118c");

            Assert.AreEqual(9, workspace.Nodes.Count());
            Assert.AreEqual(8, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);

            RunCurrentModel();
            AssertPreviewValue("3f4c4485-5149-479a-aa11-e66e72c76b37", 3);
            AssertPreviewValue("99c18e93-bb19-458b-97f3-b3467aa10364", 1);

            //the new one does not support double
            AssertPreviewValue("85f6a5e8-3e3c-4944-a7cb-ba856d755f87", null);  

            AssertPreviewValue("2be464f1-81d0-4427-b588-a22d94e8118c", 1);
        }

        [Test]
        public void TestPower()
        {
            OpenModel(GetDynPath("TestPower.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "f5a5aa45-dadf-4d4e-901e-3fe40ade85b9");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "78d7cf92-0fbb-44ac-82bd-dc1b629c921a");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "e84563cd-9bc5-402f-9808-2a685efa3e9b");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "d7709ae9-ab6c-4923-8624-d1348fa66fde");

            Assert.AreEqual(8, workspace.Nodes.Count());
            Assert.AreEqual(8, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);

            RunCurrentModel();
            AssertPreviewValue("f5a5aa45-dadf-4d4e-901e-3fe40ade85b9", 243);
            AssertPreviewValue("78d7cf92-0fbb-44ac-82bd-dc1b629c921a", 10 / 11);
            AssertPreviewValue("e84563cd-9bc5-402f-9808-2a685efa3e9b", -32);
            AssertPreviewValue("d7709ae9-ab6c-4923-8624-d1348fa66fde", 15.58846);
        }

        [Test]
        public void TestRound()
        {
            OpenModel(GetDynPath("TestRound.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "dafc3f8e-3a6a-413a-996a-6014c40c0df0");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "02d52330-4bb3-4127-abd2-3a16d6bbb701");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "05ff29d6-48d8-4923-90ab-6bc8acaaa9fb");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "8ce8a844-70d2-4861-962b-3caccb9398d9");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "dbd083ba-91ec-4e70-a1b9-10efd09daf33");

            Assert.AreEqual(10, workspace.Nodes.Count());
            Assert.AreEqual(5, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);
            Assert.NotNull(operationn5);

            RunCurrentModel();
            AssertPreviewValue("dafc3f8e-3a6a-413a-996a-6014c40c0df0", 3);
            AssertPreviewValue("02d52330-4bb3-4127-abd2-3a16d6bbb701", -2);
            AssertPreviewValue("05ff29d6-48d8-4923-90ab-6bc8acaaa9fb", 2);
            AssertPreviewValue("8ce8a844-70d2-4861-962b-3caccb9398d9", 2);
            AssertPreviewValue("dbd083ba-91ec-4e70-a1b9-10efd09daf33", 3);
        }

        [Test]
        public void TestFloor()
        {
            OpenModel(GetDynPath("TestFloor.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "802f2203-5164-4940-a5e7-6e2760c3c8c9");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "89a095a4-28c8-4178-9936-3c47a05f412b");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "7aeb508e-fc46-47bd-bdb9-7873a83f3bcb");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "a40332b1-ebf2-4d6a-8f82-60cdb990678f");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "16ecedce-1dff-4e71-aa7c-a09e1c7e2041");
            var operationn6 = workspace.NodeFromWorkspace<DSFunction>(
                "3120bce8-45de-49b1-9a7e-743b94608ff4");

            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);
            Assert.NotNull(operationn5);
            Assert.NotNull(operationn6);

            RunCurrentModel();
            AssertPreviewValue("802f2203-5164-4940-a5e7-6e2760c3c8c9", 3);
            AssertPreviewValue("89a095a4-28c8-4178-9936-3c47a05f412b", -2);
            AssertPreviewValue("7aeb508e-fc46-47bd-bdb9-7873a83f3bcb", 2);
            AssertPreviewValue("a40332b1-ebf2-4d6a-8f82-60cdb990678f", 2);
            AssertPreviewValue("16ecedce-1dff-4e71-aa7c-a09e1c7e2041", 2);
            AssertPreviewValue("3120bce8-45de-49b1-9a7e-743b94608ff4", -3);
        }

        [Test]
        public void TestCeiling()
        {
            OpenModel(GetDynPath("TestCeiling.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "3e185854-ff13-403e-9667-8abe48f5125e");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "d18afe2d-cac8-4042-8edf-68554eb69814");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "ebeed92b-68cc-4850-b13b-e6d230bc9a8d");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "5bfae2bc-3c0f-4adf-931a-20709bd9e6ad");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "7f999e43-cc26-4dab-923e-e7fc1d1fb902");
            var operationn6 = workspace.NodeFromWorkspace<DSFunction>(
                "1ef510c8-5f3b-4654-8fb7-bcd7827ccb07");

            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);
            Assert.NotNull(operationn5);
            Assert.NotNull(operationn6);

            RunCurrentModel();
            AssertPreviewValue("3e185854-ff13-403e-9667-8abe48f5125e", 3);
            AssertPreviewValue("d18afe2d-cac8-4042-8edf-68554eb69814", -2);
            AssertPreviewValue("ebeed92b-68cc-4850-b13b-e6d230bc9a8d", 3);
            AssertPreviewValue("5bfae2bc-3c0f-4adf-931a-20709bd9e6ad", 3);
            AssertPreviewValue("7f999e43-cc26-4dab-923e-e7fc1d1fb902", 3);
            AssertPreviewValue("1ef510c8-5f3b-4654-8fb7-bcd7827ccb07", -2);
        }

        [Test]
        public void TestEulersNumber()
        {
            OpenModel(GetDynPath("TestEulersNumber.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var constantn1 = workspace.NodeFromWorkspace<DSFunction>(
                "74416af6-c22c-4822-8b65-c5deea710a38");

            Assert.AreEqual(1, workspace.Nodes.Count());
            Assert.AreEqual(0, workspace.Connectors.Count());

            Assert.NotNull(constantn1);

            RunCurrentModel();
            AssertPreviewValue("74416af6-c22c-4822-8b65-c5deea710a38", 2.718282);
        }

        [Test]
        public void TestNumberRange()
        {
            OpenModel(GetDynPath("TestNumberRange.dyn"));

            AssertPreviewValue("b2b256b2-ab76-428c-93be-3ad03fd8e527", new int[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        public void TestNumberSequence()
        {
            OpenModel(GetDynPath("TestNumberSequence.dyn"));

            AssertPreviewValue("0d42e506-7463-410e-8273-6aa1020c298d", new int[] { 2, 4, 6 });
        }

        [Test]
        public void TestPi()
        {
            OpenModel(GetDynPath("TestPi.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var constantn1 = workspace.NodeFromWorkspace<DSFunction>(
                "3e82b16c-b928-4d20-a9cb-1dc27498255f");

            Assert.AreEqual(1, workspace.Nodes.Count());
            Assert.AreEqual(0, workspace.Connectors.Count());

            Assert.NotNull(constantn1);

            RunCurrentModel();
            AssertPreviewValue("3e82b16c-b928-4d20-a9cb-1dc27498255f", 3.141593);
        }

        [Test]
        public void Test2Pi()
        {
            OpenModel(GetDynPath("Test2Pi.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var constantn1 = workspace.NodeFromWorkspace<DSFunction>(
                "3017f3cb-7097-4180-b72e-9dcc19d7d690");

            Assert.AreEqual(1, workspace.Nodes.Count());
            Assert.AreEqual(0, workspace.Connectors.Count());

            Assert.NotNull(constantn1);

            RunCurrentModel();
            AssertPreviewValue("3017f3cb-7097-4180-b72e-9dcc19d7d690", 6.283185);
        }

        [Test]
        public void TestSine()
        {
            OpenModel(GetDynPath("TestSine.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "0b6eeb02-bcd1-4a35-8b7b-97e2064edc64");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "5e5129e0-96e6-4734-92d4-93cfeabf1361");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "ccd4a119-a4b0-46ad-9947-082af9671554");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "5742cca7-b528-478e-93ec-47dbf41a4159");

            //During migraton, the manager will add a toDegree node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(8 + 4, workspace.Nodes.Count());
            Assert.AreEqual(4 + 4, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);

            RunCurrentModel();
            AssertPreviewValue("0b6eeb02-bcd1-4a35-8b7b-97e2064edc64", 0);
            AssertPreviewValue("5e5129e0-96e6-4734-92d4-93cfeabf1361", 0.14112);
            AssertPreviewValue("ccd4a119-a4b0-46ad-9947-082af9671554", 0.9589234);
            AssertPreviewValue("5742cca7-b528-478e-93ec-47dbf41a4159", -0.6665387);
        }

        [Test]
        public void TestCosine()
        {
            OpenModel(GetDynPath("TestCosine.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "af6f8751-fa5e-4727-b6c6-713cf2c75d15");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "c0f5111c-3a3b-427e-8188-ba76648261d3");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "19f7e93f-a2d4-4e7d-b4e2-154770b45598");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "84ba68a3-d409-4267-a677-c22daf3136e4");

            //During migraton, the manager will add a toDegree node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(8 + 4, workspace.Nodes.Count());
            Assert.AreEqual(4 + 4, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);

            RunCurrentModel();
            AssertPreviewValue("af6f8751-fa5e-4727-b6c6-713cf2c75d15", 1);
            AssertPreviewValue("c0f5111c-3a3b-427e-8188-ba76648261d3", -0.9899925);
            AssertPreviewValue("19f7e93f-a2d4-4e7d-b4e2-154770b45598", 0.2836622);
            AssertPreviewValue("84ba68a3-d409-4267-a677-c22daf3136e4", 0.7454705);
        }

        [Test]
        public void TestTangent()
        {
            OpenModel(GetDynPath("TestTangent.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "990cacd7-a552-484a-bc46-564416dca5e5");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "7e4399cc-4236-425d-ab5d-96ca98fe7cc3");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "b3bc9d5e-9fd2-4072-900c-16b71f7b76f4");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "5c04bbde-85c2-4c63-bacf-c802a9aca7d7");

            //During migraton, the manager will add a toDegree node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(8 + 4, workspace.Nodes.Count());
            Assert.AreEqual(4 + 4, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);

            RunCurrentModel();
            AssertPreviewValue("990cacd7-a552-484a-bc46-564416dca5e5", 0);
            AssertPreviewValue("7e4399cc-4236-425d-ab5d-96ca98fe7cc3", -0.1425465);
            AssertPreviewValue("b3bc9d5e-9fd2-4072-900c-16b71f7b76f4", 3.380515);
            AssertPreviewValue("5c04bbde-85c2-4c63-bacf-c802a9aca7d7", -0.8941181);
        }

        [Test]
        public void TestInverseSine()
        {
            OpenModel(GetDynPath("TestInverseSine.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "041a0818-393a-4d47-a534-3471774adfe5");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "9e267757-8a43-45e6-93c1-84f37fc52e82");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "1bf23e76-f4b4-4b31-a7bf-cd84f87c56ed");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "6b28ac3e-509f-459f-bd8b-99b550efeb56");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "ecb6438c-ed36-4f16-b6e5-ea9c6481d234");

            //During migraton, the manager will add a toRadius node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(10 + 5, workspace.Nodes.Count());
            Assert.AreEqual(5 + 5, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);
            Assert.NotNull(operationn5);

            RunCurrentModel();
            AssertPreviewValue("041a0818-393a-4d47-a534-3471774adfe5", 0);
            AssertPreviewValue("9e267757-8a43-45e6-93c1-84f37fc52e82", 0.5235988);
            AssertPreviewValue("1bf23e76-f4b4-4b31-a7bf-cd84f87c56ed", 1.570796);
            AssertPreviewValue("6b28ac3e-509f-459f-bd8b-99b550efeb56", -0.4115168);
            //AssertPreviewValue("ecb6438c-ed36-4f16-b6e5-ea9c6481d234", null);
        }

        [Test]
        public void TestInverseCosine()
        {
            OpenModel(GetDynPath("TestInverseCosine.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "0d6cdacb-3a11-47dc-b26a-ce4cbc621a7a");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "5527c25b-0131-406d-a543-62b07d39a847");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "40812d36-596a-4d8e-85aa-fdeb36995942");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "a8d4c4d4-d0b4-4dc2-9334-80a29835924e");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "e644aebb-8bc6-449e-9250-8c5a6cc486ec");

            //During migraton, the manager will add a toRadius node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(10 + 5, workspace.Nodes.Count());
            Assert.AreEqual(5 + 5, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);
            Assert.NotNull(operationn5);

            RunCurrentModel();
            AssertPreviewValue("0d6cdacb-3a11-47dc-b26a-ce4cbc621a7a", 1.570796);
            AssertPreviewValue("5527c25b-0131-406d-a543-62b07d39a847", 1.047198);
            AssertPreviewValue("40812d36-596a-4d8e-85aa-fdeb36995942", 0);
            AssertPreviewValue("a8d4c4d4-d0b4-4dc2-9334-80a29835924e", 1.982313);
            //AssertPreviewValue("e644aebb-8bc6-449e-9250-8c5a6cc486ec", null);
        }

        [Test]
        public void TestInverseTangent()
        {
            OpenModel(GetDynPath("TestInverseTangent.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "bdaeb25f-654b-4db9-9c2a-9377d9ebe3f3");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "15b42154-c3ed-4216-93fe-a3a0b7146074");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "2a8fbba7-e455-406a-88e2-6caa4f3983eb");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "22787711-c6a0-4fd6-beb6-4e2eefa7f1d7");
            var operationn5 = workspace.NodeFromWorkspace<DSFunction>(
                "969df084-1636-4654-8d13-a52502861123");

            //During migraton, the manager will add a toRadius node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(10 + 5, workspace.Nodes.Count());
            Assert.AreEqual(5 + 5, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);
            Assert.NotNull(operationn5);

            RunCurrentModel();
            AssertPreviewValue("bdaeb25f-654b-4db9-9c2a-9377d9ebe3f3", 0);
            AssertPreviewValue("15b42154-c3ed-4216-93fe-a3a0b7146074", 0.4636476);
            AssertPreviewValue("2a8fbba7-e455-406a-88e2-6caa4f3983eb", 0.7853982);
            AssertPreviewValue("22787711-c6a0-4fd6-beb6-4e2eefa7f1d7", -0.3805064);
            AssertPreviewValue("969df084-1636-4654-8d13-a52502861123", 1.107149);
        }

        [Test]
        public void TestAverage()
        {
            OpenModel(GetDynPath("TestAverage.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "d4f242c5-9c20-4633-b661-157ab45a416c");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "3d59ccad-57ed-44bc-9d55-27574fc725de");
            var operationn3 = workspace.NodeFromWorkspace<DSFunction>(
                "d65de7e9-f7f7-4f2b-9be7-daad3b3c837a");
            var operationn4 = workspace.NodeFromWorkspace<DSFunction>(
                "af486a6c-a558-4a0b-860f-8c3800f5b8b5");

            Assert.AreEqual(8, workspace.Nodes.Count());
            Assert.AreEqual(4, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
            Assert.NotNull(operationn3);
            Assert.NotNull(operationn4);

            RunCurrentModel();
            AssertPreviewValue("d4f242c5-9c20-4633-b661-157ab45a416c", 5.5);
            AssertPreviewValue("3d59ccad-57ed-44bc-9d55-27574fc725de", 5.5);
            AssertPreviewValue("d65de7e9-f7f7-4f2b-9be7-daad3b3c837a", -5.5);
            AssertPreviewValue("af486a6c-a558-4a0b-860f-8c3800f5b8b5", 5);
        }

        [Test]
        public void TestIf()
        {
            OpenModel(GetDynPath("TestIf.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var logicn1 = workspace.NodeFromWorkspace<If>(
                "c3685d47-d29e-4015-83d1-4b7e20274c0e");
            var logicn2 = workspace.NodeFromWorkspace<If>(
                "274166dc-4c76-4e42-8856-817978a0dd7c");

            Assert.AreEqual(6, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(logicn1);
            Assert.NotNull(logicn2);

            RunCurrentModel();
            AssertPreviewValue("c3685d47-d29e-4015-83d1-4b7e20274c0e", 3);
            AssertPreviewValue("274166dc-4c76-4e42-8856-817978a0dd7c", 2);
        }

        [Test]
        public void TestListCreate()
        {
            OpenModel(GetDynPath("TestListCreate.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<CreateList>(
                "db161881-4239-408c-9ab2-d507fcb4d25f");
            var listn2 = workspace.NodeFromWorkspace<CreateList>(
                "f336c24a-3617-4da4-ace2-d0bd5fe02ebc");
            var listn3 = workspace.NodeFromWorkspace<CreateList>(
                "ec723754-21fe-48bc-98ca-d8231e6879af");
            var listn4 = workspace.NodeFromWorkspace<CreateList>(
                "82a91a49-0c3b-4ed3-851d-ffe9d64593ea");
            var listn5 = workspace.NodeFromWorkspace<CreateList>(
                "9bb7f4ae-3ace-43c4-ab91-2cc6126975c1");
            var listn6 = workspace.NodeFromWorkspace<CreateList>(
                "e8f77740-93b5-4129-9cf2-9ae7b4a0aa06");

            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(11, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);
            Assert.NotNull(listn3);
            Assert.NotNull(listn4);
            Assert.NotNull(listn5);
            Assert.NotNull(listn6);

            RunCurrentModel();

            //does not check the _singleFunction value for "db161881-4239-408c-9ab2-d507fcb4d25f"

            AssertPreviewValue("f336c24a-3617-4da4-ace2-d0bd5fe02ebc", 
                new object[] {1, -1.5, -1.5});
            AssertPreviewValue("ec723754-21fe-48bc-98ca-d8231e6879af", 
                new object[] {"Hi,", "I am", "a test"});
            AssertPreviewValue("82a91a49-0c3b-4ed3-851d-ffe9d64593ea",
                new object[] {new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}});
            AssertPreviewValue("9bb7f4ae-3ace-43c4-ab91-2cc6126975c1", new object[] {1, "Hi,"});
            AssertPreviewValue("e8f77740-93b5-4129-9cf2-9ae7b4a0aa06",
                new object[] {new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, 1});
        }

        [Test]
        public void TestListFlatten()
        {
            // This file contains a code block node with value [1,3,5,[2,4,[6],8],7,9,[[0]]],
            // connected to List.Flatten(list) node and List.Flatten(list, amt) node with amt = 1.
            OpenModel(GetDynPath("TestListFlatten.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var flattenWithoutAmt = workspace.NodeFromWorkspace<DSFunction>(
                "eba368d6-f0cf-4bbe-9cf8-949943ab7789");
            var flattenWithAmt = workspace.NodeFromWorkspace<DSFunction>(
                "6e175514-15ed-43f6-a124-a51dd43f15e7");

            // 2 code block nodes (one with the list and one with amt) + 2 List.Flatten nodes
            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(flattenWithoutAmt);
            Assert.NotNull(flattenWithAmt);

            // List.Flatten(list) should be migrated and flatten the given list completely.
            AssertPreviewValue("eba368d6-f0cf-4bbe-9cf8-949943ab7789",
                new object[] { 1, 3, 5, 2, 4, 6, 8, 7, 9, 0});

            // List.Flatten(list, amt) should have amt = 1 instead of the default value (-1).
            AssertPreviewValue("6e175514-15ed-43f6-a124-a51dd43f15e7",
                new object[] { 1, 3, 5, 2, 4, new object[] { 6 }, 8, 7, 9, new object[] { 0 } });
        }

        [Test]
        public void TestAddToList()
        {
            OpenModel(GetDynPath("TestAddToList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "d3e45f5d-9200-450f-84a3-1de1f26a1a72");
            var listn2 = workspace.NodeFromWorkspace<DSFunction>(
                "7f4729ca-d023-4d76-a45a-5641223eaa15");
            var listn3 = workspace.NodeFromWorkspace<DSFunction>(
                "0f086699-08f6-4c58-8ac6-c9d7a79c6163");
            var listn4 = workspace.NodeFromWorkspace<DSFunction>(
                "a6c40764-1009-4d43-9728-17fc1e03caa8");

            Assert.AreEqual(8, workspace.Nodes.Count());
            Assert.AreEqual(8, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);
            Assert.NotNull(listn3);
            Assert.NotNull(listn4);

            RunCurrentModel();
            AssertPreviewValue("d3e45f5d-9200-450f-84a3-1de1f26a1a72", 
                new object[] {0, 1, 2, 3, 4, 5});
            AssertPreviewValue("7f4729ca-d023-4d76-a45a-5641223eaa15",
                new object[] {0.1, 1, 2, 3, 4, 5});
            AssertPreviewValue("0f086699-08f6-4c58-8ac6-c9d7a79c6163",
                new object[] {"oh", 1, 2, 3, 4, 5});
            AssertPreviewValue("a6c40764-1009-4d43-9728-17fc1e03caa8",
                new object[] {new object[] {1, 2, 3, 4, 5}, 1, 2, 3, 4, 5});
        }

        [Test]
        public void TestEmptyList()
        {
            OpenModel(GetDynPath("TestEmptyList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "1201c055-31a3-46ff-997c-e634c7d061fa");

            Assert.AreEqual(1, workspace.Nodes.Count());
            Assert.AreEqual(0, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("1201c055-31a3-46ff-997c-e634c7d061fa", new object[] { });
        }

        [Test]
        public void TestIsEmptyList()
        {
            OpenModel(GetDynPath("TestIsEmptyList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "f03dd785-bdc3-478f-b281-ea9db063b356");
            var listn2 = workspace.NodeFromWorkspace<DSFunction>(
                "79d4216d-695d-425e-b1e7-51535e46ae98");
            var listn3 = workspace.NodeFromWorkspace<DSFunction>(
                "1ec03940-2cad-431f-807e-c6ec0f7ae3bb");
            var listn4 = workspace.NodeFromWorkspace<DSFunction>(
                "ecd5e943-e6b5-44ca-bb52-3b5c39971ea7");

            Assert.AreEqual(8, workspace.Nodes.Count());
            Assert.AreEqual(4, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);
            Assert.NotNull(listn3);
            Assert.NotNull(listn4);

            RunCurrentModel();
            AssertPreviewValue("f03dd785-bdc3-478f-b281-ea9db063b356", false);
            AssertPreviewValue("79d4216d-695d-425e-b1e7-51535e46ae98", false);
            AssertPreviewValue("1ec03940-2cad-431f-807e-c6ec0f7ae3bb", false);
            AssertPreviewValue("ecd5e943-e6b5-44ca-bb52-3b5c39971ea7", true);
        }

        [Test]
        public void TestListLength()
        {
            OpenModel(GetDynPath("TestListLength.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "b3c61406-d429-43d4-8db0-7da92fce1eb5");
            var listn2 = workspace.NodeFromWorkspace<DSFunction>(
                "badd9669-7cb7-4ea4-a271-1fe81fe437b4");
            var listn3 = workspace.NodeFromWorkspace<DSFunction>(
                "4477b43e-0f51-486d-98a5-27ee0b312819");

            Assert.AreEqual(6, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);
            Assert.NotNull(listn3);

            RunCurrentModel();
            AssertPreviewValue("b3c61406-d429-43d4-8db0-7da92fce1eb5", 1);
            AssertPreviewValue("badd9669-7cb7-4ea4-a271-1fe81fe437b4", 10);
            AssertPreviewValue("4477b43e-0f51-486d-98a5-27ee0b312819", 0);
        }

        [Test]
        public void TestListLength_NestedList()
        {
            OpenModel(GetDynPath("TestListLength_NestedList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "23b91324-69db-46b7-aa0b-b57fcd723264");

            Assert.AreEqual(3, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("23b91324-69db-46b7-aa0b-b57fcd723264", 2);
        }

        [Test]
        public void TestFirstOfList()
        {
            OpenModel(GetDynPath("TestFirstOfList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "28383b05-d53a-47e0-ab4c-5c5d83208f25");
            var listn2 = workspace.NodeFromWorkspace<DSFunction>(
                "5b093fdd-c63a-4efa-a0b7-4bd7c2330752");
            var listn3 = workspace.NodeFromWorkspace<DSFunction>(
                "218c3a8e-9c4a-4a8c-8b13-6f2fb758df3f");

            Assert.AreEqual(6, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);
            Assert.NotNull(listn3);

            RunCurrentModel();
            AssertPreviewValue("28383b05-d53a-47e0-ab4c-5c5d83208f25", 1);
            AssertPreviewValue("5b093fdd-c63a-4efa-a0b7-4bd7c2330752", null);
            AssertPreviewValue("218c3a8e-9c4a-4a8c-8b13-6f2fb758df3f", 1);
        }

        [Test]
        public void TestFirstOfList_NestedList()
        {
            OpenModel(GetDynPath("TestFirstOfList_NestedList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "6a575df0-0540-46ff-8b9d-15787835f064");

            Assert.AreEqual(3, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("6a575df0-0540-46ff-8b9d-15787835f064", 
                new object[] {1, 2, 3, 4, 5});
        }

        [Test]
        public void TestRandomSeed()
        {
            OpenModel(GetDynPath("TestRandomSeed.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "e069c343-46be-4e01-a3b3-9321e89d0775");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "71149321-db95-4064-a311-aadfe0cec404");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
        }

        [Test]
        public void TestRandom()
        {
            OpenModel(GetDynPath("TestRandom.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "8a4329e2-d0d5-4fe2-9bba-f4291502eb1c");

            Assert.AreEqual(1, workspace.Nodes.Count());
            Assert.AreEqual(0, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
        }

        [Test]
        public void TestRandomList()
        {
            OpenModel(GetDynPath("TestRandomList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var operationn1 = workspace.NodeFromWorkspace<DSFunction>(
                "399e524f-15b6-4100-b7bd-9331c329a717");
            var operationn2 = workspace.NodeFromWorkspace<DSFunction>(
                "010adb65-ae28-408f-a91e-c5b0fae2c387");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(operationn1);
            Assert.NotNull(operationn2);
        }

        [Test]
        public void TestTakeFromList()
        {
            OpenModel(GetDynPath("TestTakeFromList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "f08875de-8aa4-4bae-aedd-8bb26ae73a35");

            Assert.AreEqual(3, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("f08875de-8aa4-4bae-aedd-8bb26ae73a35",
                new object[] {1, 2});
        }

        [Test]
        public void TestTakeFromList_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestTakeFromList_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "f08875de-8aa4-4bae-aedd-8bb26ae73a35");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("f08875de-8aa4-4bae-aedd-8bb26ae73a35",
                new object[] {new object[] {1, 2, 3, 4, 5}, new object[] {1, 2, 3, 4, 5}});
        }

        [Test]
        [Category("Failure")]
        public void TestTakeEveryNth()
        {
            OpenModel(GetDynPath("TestTakeEveryNth.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "adffbefb-4f91-4b6e-bcef-59f8f7adf9f4");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("adffbefb-4f91-4b6e-bcef-59f8f7adf9f4",
                new object[] {4, 7, 10});
        }

        [Test]
        public void TestTakeEveryNth_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestTakeEveryNth_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "adffbefb-4f91-4b6e-bcef-59f8f7adf9f4");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(9, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("adffbefb-4f91-4b6e-bcef-59f8f7adf9f4",
                new object[] { new object[] { 1, 2}, new object[] { 1, 2} });
        }

        [Test]
        public void TestRemoveFromList()
        {
            OpenModel(GetDynPath("TestRemoveFromList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "5295f03d-531c-4f0e-b852-47eef1f8c38c");
            var listn2 = workspace.NodeFromWorkspace<DSFunction>(
                "b6769722-96ae-437d-9c64-cc82f2f6fb01");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(4, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);

            RunCurrentModel();
            AssertPreviewValue("5295f03d-531c-4f0e-b852-47eef1f8c38c",
                new object[] {1, 2, 3, 4, 5, 7, 8, 9, 10});
            AssertPreviewValue("b6769722-96ae-437d-9c64-cc82f2f6fb01",
                new object[] {1, 2, 7, 8, 9, 10});
        }

        [Test]
        public void TestRemoveFromList_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestRemoveFromList_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "5295f03d-531c-4f0e-b852-47eef1f8c38c");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("5295f03d-531c-4f0e-b852-47eef1f8c38c",
                new object[] {new object[] {1, 2}, new object[] {1, 2}, new object[] {1, 2}});
        }

        [Test]
        public void TestDropFromList()
        {
            OpenModel(GetDynPath("TestDropFromList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "c250a8d2-4e16-4e87-a8a8-f738329e61b1");

            Assert.AreEqual(3, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("c250a8d2-4e16-4e87-a8a8-f738329e61b1",
                new object[] {9, 10});
        }

        [Test]
        public void TestDropFromList_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestDropFromList_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "c250a8d2-4e16-4e87-a8a8-f738329e61b1");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("c250a8d2-4e16-4e87-a8a8-f738329e61b1",
                new object[] { new object[] { 1, 2, 3, 4, 5 }, new object[] { 1, 2, 3, 4, 5 } });
        }

        [Test]
        public void TestDropEveryNth()
        {
            OpenModel(GetDynPath("TestDropEveryNth.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "9e3e4a46-9874-4322-a126-2ada785f3f80");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("9e3e4a46-9874-4322-a126-2ada785f3f80",
                new object[] {2, 3, 5, 6, 8, 9});
        }

        [Test]
        public void TestDropEveryNth_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestDropEveryNth_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "9e3e4a46-9874-4322-a126-2ada785f3f80");

            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(9, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("9e3e4a46-9874-4322-a126-2ada785f3f80",
                new object[] {new object[] {1, 2}, new object[] {1, 2}, 
                    new object[] {1, 2}, new object[] {1, 2}});
        }

        [Test]
        public void TestSort()
        {
            OpenModel(GetDynPath("TestSort.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "77a79c75-15a4-4b0a-b326-00df04c689b6");

            Assert.AreEqual(8, workspace.Nodes.Count());
            Assert.AreEqual(7, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("77a79c75-15a4-4b0a-b326-00df04c689b6",
                new object[] {-7, 0, 2, 2, 2.5, 9});
        }

        [Test]
        public void TestSortByKey()
        {
            OpenModel(GetDynPath("TestSortByKey.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "3c619222-858f-4f7c-b001-3a4a248f8f77");

            Assert.AreEqual(10, workspace.Nodes.Count());
            Assert.AreEqual(9, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("3c619222-858f-4f7c-b001-3a4a248f8f77",
                new object[] {2, 2.5, 2, 9, 0, -7});
        }

        [Test]
        public void TestNewList()
        {
            OpenModel(GetDynPath("TestNewList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<CreateList>(
                "ff8f5f64-c9f3-4814-896a-6ef679a35275");
            var listn2 = workspace.NodeFromWorkspace<CreateList>(
                "2d58dbc4-62ad-4a12-974b-52d5986053b5");
            var listn3 = workspace.NodeFromWorkspace<CreateList>(
                "0c98e395-e2f4-49c1-abda-d1bcb3c24cbd");

            Assert.AreEqual(10, workspace.Nodes.Count());
            Assert.AreEqual(12, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);
            Assert.NotNull(listn3);

            RunCurrentModel();
            AssertPreviewValue("ff8f5f64-c9f3-4814-896a-6ef679a35275", 
                new object[] { 1,1,-1,2.5 });
            AssertPreviewValue("2d58dbc4-62ad-4a12-974b-52d5986053b5",
                new object[] {"hi,", "I am", 1, "test"});
            AssertPreviewValue("0c98e395-e2f4-49c1-abda-d1bcb3c24cbd",
                new object[] {new object[] {1, 2, 3}, 1, 1, 1});
        }

        [Test]
        public void TestShiftListIndices()
        {
            OpenModel(GetDynPath("TestShiftListIndices.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "1dd7ff84-90db-4e1c-a0ca-9fe9119dbea6");

            Assert.AreEqual(3, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("1dd7ff84-90db-4e1c-a0ca-9fe9119dbea6",
                new object[] {6, 7, 8, 9, 10, 1, 2, 3, 4, 5});
        }

        [Test]
        public void TestShiftListIndices_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestShiftListIndices_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "1dd7ff84-90db-4e1c-a0ca-9fe9119dbea6");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("1dd7ff84-90db-4e1c-a0ca-9fe9119dbea6",
                new object[] {new object[] {1, 2}, new object[] {1, 2}, 
                    new object[] {1, 2}, new object[] {1, 2}});
        }

        [Test]
        public void TestGetFromList()
        {
            OpenModel(GetDynPath("TestGetFromList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "ce22d1d1-c5e4-4684-8414-9a115848a06f");

            Assert.AreEqual(3, workspace.Nodes.Count());
            Assert.AreEqual(2, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("ce22d1d1-c5e4-4684-8414-9a115848a06f", 5);
        }

        [Test]
        public void TestGetFromList_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestGetFromList_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "ce22d1d1-c5e4-4684-8414-9a115848a06f");

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("ce22d1d1-c5e4-4684-8414-9a115848a06f",
                new object[] {1, 2});

        }

        [Test]
        [Category("Failure")]
        public void TestSliceList()
        {
            OpenModel(GetDynPath("TestSliceList.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "fbe895a7-e97a-47f3-b5bf-536d652aa603");
            var listn2 = workspace.NodeFromWorkspace<DSFunction>(
                "797b1b02-c815-4808-9889-296bc6d176fd");

            //During migraton, the manager will add a toRadius node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(6 + 2, workspace.Nodes.Count());
            Assert.AreEqual(6 + 2, workspace.Connectors.Count());

            Assert.NotNull(listn1);
            Assert.NotNull(listn2);

            RunCurrentModel();
            AssertPreviewValue("fbe895a7-e97a-47f3-b5bf-536d652aa603", 
                new object[] {6});
            AssertPreviewValue("797b1b02-c815-4808-9889-296bc6d176fd", 
                new object[] {6, 7, 8});
        }

        [Test]
        [Category("Failure")]
        public void TestSliceList_ListOfListAsInput()
        {
            OpenModel(GetDynPath("TestSliceList_ListOfListAsInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var listn1 = workspace.NodeFromWorkspace<DSFunction>(
                "fbe895a7-e97a-47f3-b5bf-536d652aa603");

            //During migraton, the manager will add a toRadius node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(5 + 1, workspace.Nodes.Count());
            Assert.AreEqual(7 + 1, workspace.Connectors.Count());

            Assert.NotNull(listn1);

            RunCurrentModel();
            AssertPreviewValue("fbe895a7-e97a-47f3-b5bf-536d652aa603",
                new object[] {new object[] {1, 2}, new object[] {1, 2}});
        }

        [Test]
        public void TestCompose()
        {
            OpenModel(GetDynPath("TestCompose.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            //During migraton, the manager will add a toRadius node. 
            //So the number of node and connector will be increased.
            Assert.AreEqual(8, workspace.Nodes.Count());
            Assert.AreEqual(7, workspace.Connectors.Count());

            RunCurrentModel();
            AssertPreviewValue("a748df54-06dd-4159-a339-f824f190d5ea", 6);
        }

        [Test]
        public void TestNumberInput()
        {
            OpenModel(GetDynPath("TestNumberInput.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(16, workspace.Connectors.Count());

            var number5 = workspace.NodeFromWorkspace<DoubleInput>(
                "ddf4b266-29b6-4609-b1fe-dba814d4babd");
            var number10 = workspace.NodeFromWorkspace<DoubleInput>(
                "27d6c83d-602c-4d44-a9b6-ab229cb2143d");
            var number50 = workspace.NodeFromWorkspace<DoubleInput>(
                "ffd50d99-b51d-4104-9e19-041219ca5740");
            var numberRange = workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                "eb6aa95d-8be4-4ca7-95a6-e696904a71fa");
            var numberStep = workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                "5fde015f-8a95-4b46-ba64-29de06850938");
            var numberCount = workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                "ace69b4e-5092-42cf-9fba-9aee6729509d");
            var numberApprox = workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                "30e9b7fd-fc09-4243-a34a-146ad841868a");
            var numberIncr = workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                "bede0d80-6382-4430-9403-a14c3916e041");

            Assert.NotNull(number5);
            Assert.NotNull(number10);
            Assert.NotNull(number50);
            Assert.NotNull(numberRange);
            Assert.NotNull(numberStep);
            Assert.NotNull(numberCount);
            Assert.NotNull(numberApprox);
            Assert.NotNull(numberIncr);

            RunCurrentModel(); // Execute the migrated graph.

            AssertPreviewValue("ddf4b266-29b6-4609-b1fe-dba814d4babd", 5);
            AssertPreviewValue("27d6c83d-602c-4d44-a9b6-ab229cb2143d", 10);
            AssertPreviewValue("ffd50d99-b51d-4104-9e19-041219ca5740", 50);

            AssertPreviewValue("eb6aa95d-8be4-4ca7-95a6-e696904a71fa",
                new int[] {
                    10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                    20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                    30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                    40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                    50 });

            AssertPreviewValue("5fde015f-8a95-4b46-ba64-29de06850938",
                new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 });

            AssertPreviewValue("ace69b4e-5092-42cf-9fba-9aee6729509d",
                new int[] { 10, 20, 30, 40, 50 });

            AssertPreviewValue("30e9b7fd-fc09-4243-a34a-146ad841868a",
                new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 });

            AssertPreviewValue("bede0d80-6382-4430-9403-a14c3916e041", 5);
        }

        [Test]
        [Category("Failure")]
        public void TestXyzAverage()
        {
            OpenModel(GetDynPath("TestXyzAverage.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var x = workspace.NodeFromWorkspace<DSFunction>(
                "024dbc25-b0a9-478f-9cc7-7005e44f0c5e");
            var y = workspace.NodeFromWorkspace<DSFunction>(
                "38085784-b781-4429-a37c-89fa56c97f68");
            var z = workspace.NodeFromWorkspace<DSFunction>(
                "6e68a338-d71e-4b72-a806-9c6b9e917c50");

            Assert.AreEqual(14 + 6, workspace.Nodes.Count());
            Assert.AreEqual(19 + 8, workspace.Connectors.Count());

            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.NotNull(z);

            RunCurrentModel();
            AssertPreviewValue("024dbc25-b0a9-478f-9cc7-7005e44f0c5e", 0);
            AssertPreviewValue("38085784-b781-4429-a37c-89fa56c97f68", -1.666667);
            AssertPreviewValue("6e68a338-d71e-4b72-a806-9c6b9e917c50", 0);
        }

        [Test]
        public void TestXyPlane()
        {
            OpenModel(GetDynPath("TestXyPlane.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(14, workspace.Connectors.Count());

            RunCurrentModel();
            AssertPreviewValue("4957d6b3-27c4-4cb5-939c-057ccf17ac48", new double[]{1});
            AssertPreviewValue("e5621d44-334a-4814-ad29-aa37fdd059be", new double[]{1});
            AssertPreviewValue("3890ec6d-1d41-4988-b827-11d9965b6cf8", new double[]{0});
        }

        [Test]
        public void TestYzPlane()
        {
            OpenModel(GetDynPath("TestYzPlane.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(14, workspace.Connectors.Count());

            RunCurrentModel();
            AssertPreviewValue("36b34044-9251-4e1d-af19-37db6396cd23", new double[] { 0 });
            AssertPreviewValue("34363f7d-42f6-4985-9684-667a5190e428", new double[] { 3 });
            AssertPreviewValue("fb96ef37-84d5-4c83-9a06-ed78b5b3d8ca", new double[] { 3 });
        }

        [Test]
        public void TestXzPlane()
        {
            OpenModel(GetDynPath("TestXzPlane.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            Assert.AreEqual(12, workspace.Nodes.Count());
            Assert.AreEqual(14, workspace.Connectors.Count());

            RunCurrentModel();
            AssertPreviewValue("5acac8cc-65ca-4410-a851-5b86a3987c1b", new double[] { 2 });
            AssertPreviewValue("66e28a4b-09c6-4386-98c4-958bf7aad87f", new double[] { 0 });
            AssertPreviewValue("635a4533-d522-4bf7-83a7-4f178fabd18f", new double[] { 2 });
        }

        [Test]
        public void TestWriteText()
        {
            OpenModel(GetDynPath("TestWriteFile.dyn"));

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            Assert.AreEqual(4, workspace.Nodes.Count());
            Assert.AreEqual(3, workspace.Connectors.Count());

            var path = workspace.NodeFromWorkspace<StringInput>("1651f446-1b0f-4d5b-be59-c59bf9f80142");
            string fullPath = Path.Combine(TempFolder, "filewriter.txt");
            path.Value = fullPath;

            RunCurrentModel();

            Assert.IsTrue(File.Exists(fullPath));
            Assert.AreEqual("test", File.ReadAllText(fullPath));
        }

        [Test]
        public void TestMigration_GroupByKey()
        {
            TestMigration("TestMigration_GroupByKey.dyn");
        }

        [Test]
        public void TestMigration_GroupByKeyUI()
        {
            TestMigration("TestMigration_GroupByKeyUI.dyn");
        }

        [Test]
        public void TestMigration_SortByKey()
        {
            TestMigration("TestMigration_SortByKey.dyn");
        }

        [Test]
        public void TestMigration_SortByKeyUI()
        {
            TestMigration("TestMigration_SortByKeyUI.dyn");
        }

        [Test]
        public void TestMigration_BuiltIns_To_DSBuiltInClass()
        {
            TestMigration("TestMigrateBuiltIn.dyn");
        }

        [Test]
        public void TestFunctionApplyAndCompose()
        {
            OpenModel(GetDynPath("TestMigration_FunctionApplyAndCompose.dyn"));

            RunCurrentModel();

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            AssertPreviewValue("95c607e2-7f1b-4ec7-ba1c-a34f9dcf23bc", 14);

            Assert.AreEqual(6, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());
        }

        #endregion

        #region Dynamo Libraries Node Migration Tests

        [Test]
        [Category("Failure")]
        public void LibraryTestReferencePoint()
        {
            OpenModel(GetDynPath("LibraryTestReferencePoint.dyn"));
            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // check that all nodes and connectors are loaded
            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(5, workspace.Connectors.Count());

            // check that no nodes are migrated to dummy nodes
            Assert.AreEqual(0, workspace.Nodes.AsQueryable().Count(x => x is DummyNode));

            // check that the node is migrated to a DSFunction named "ReferencePoint.ByPoint"
            StringAssert.Contains("Reference", workspace.NodeFromWorkspace<DSFunction>(
                "d615cc73-d32d-4b1f-b519-0b8f9b903ebf").Name);
        }

        [Test]
        [Category("Failure")]
        public void LibraryTestCreateFamilyInstance()
        {
            OpenModel(GetDynPath("LibraryTestCreateFamilyInstance.dyn"));
            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // check that all nodes and connectors are loaded
            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(5, workspace.Connectors.Count());

            // check that no nodes are migrated to dummy nodes
            Assert.AreEqual(0, workspace.Nodes.AsQueryable().Count(x => x is DummyNode));

            // check that the node is migrated to a DSFunction named "FamilyInstance.ByPoint"
            StringAssert.Contains("Instance", workspace.NodeFromWorkspace<DSFunction>(
                "fc83b9b2-42c6-4a9f-8f60-a6ee29ef8a34").Name);
        }

        [Test]
        [Category("Failure")]
        public void LibraryTestModelCurve()
        {
            OpenModel(GetDynPath("LibraryTestModelCurve.dyn"));
            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // check that all nodes and connectors are loaded
            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(5, workspace.Connectors.Count());

            // check that no nodes are migrated to dummy nodes
            Assert.AreEqual(0, workspace.Nodes.AsQueryable().Count(x => x is DummyNode));

            // check that the node is migrated to a DSFunction named "ModelCurve.ByCurve"
            StringAssert.Contains("Model", workspace.NodeFromWorkspace<DSFunction>(
                "fdea006e-b127-4280-a407-4058b78b93a3").Name);
        }

        [Test]
        public void LibraryTestPythonScript()
        {
            OpenModel(GetDynPath("LibraryTestPythonScript.dyn"));
            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // check that all nodes and connectors are loaded
            Assert.AreEqual(5, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            // check that no nodes are migrated to dummy nodes
            Assert.AreEqual(0, workspace.Nodes.AsQueryable().Count(x => x is DummyNode));

            // check that the node is migrated to a PythonNode which retains the old script
            StringAssert.Contains("OUT = OUT", workspace.NodeFromWorkspace<PythonNode>(
                "caef9f81-c9a6-47aa-92c9-dc3b8fd6f7d7").Script);
        }

        [Test,Category("FailureNET6")]
        public void LibraryTestExcelRead()
        {
            OpenModel(GetDynPath("LibraryTestExcelRead.dyn"));
            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // check that all nodes and connectors are loaded
            Assert.AreEqual(7, workspace.Nodes.Count());
            Assert.AreEqual(6, workspace.Connectors.Count());

            // check that no nodes are migrated to dummy nodes
            Assert.AreEqual(0, workspace.Nodes.AsQueryable().Count(x => x is DummyNode));

            // check that some of the nodes are Excel nodes
            Assert.AreEqual(4, workspace.Nodes.AsQueryable().Count(x => x.Name.Contains("Excel")));
        }

        [Test]
        [Category("Failure")]
        public void TestSaveDontCorruptForUnresolvedNodes()
        {
            var model = CurrentDynamoModel;
            var exPath = Path.Combine(TestDirectory, @"core\migration");
            var oldPath = Path.Combine(exPath, @"TestSaveDontCorruptForUnresolvedNodes.dyn");
            OpenModel(oldPath);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            CurrentDynamoModel.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));

            XmlDocument docOld = new XmlDocument();
            docOld.Load(oldPath);
            XmlDocument docNew = new XmlDocument();
            docNew.Load(newPath);

            XmlNodeList oldNodes = docOld.GetElementsByTagName("DSRevitNodesUI.ElementTypes");
            XmlNodeList newNodes = docNew.GetElementsByTagName("DSRevitNodesUI.ElementTypes");
            if (!oldNodes[0].InnerXml.Equals(newNodes[0].InnerXml))
            {
                Assert.Fail("the content of the unresolved node has been changed after saving");
            }

            oldNodes = docOld.GetElementsByTagName("Dynamo.Graph.Nodes.ZeroTouch.DSFunction");
            newNodes = docNew.GetElementsByTagName("Dynamo.Graph.Nodes.ZeroTouch.DSFunction");
            if (!oldNodes[0].InnerXml.Equals(newNodes[0].InnerXml))
            {
                Assert.Fail("the content of the unresolved node has been changed after saving");
            }
            if (!oldNodes[1].InnerXml.Equals(newNodes[1].InnerXml))
            {
                Assert.Fail("the content of the unresolved node has been changed after saving");
            }
        }


        [Test]
        [Category("Failure")]
        public void TestSaveDontCorruptForDeprecatedNodes()
        {
            var model = CurrentDynamoModel;
            var exPath = Path.Combine(TestDirectory, @"core\migration");
            var oldPath = Path.Combine(exPath, @"TestSaveDontCorruptForDeprecatedNodes.dyn");
            OpenModel(oldPath);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            model.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));

            XmlDocument docOld = new XmlDocument();
            docOld.Load(oldPath);
            XmlDocument docNew = new XmlDocument();
            docNew.Load(newPath);

            XmlNodeList oldNodes = docOld.GetElementsByTagName("Dynamo.Graph.Nodes.Now");
            XmlNodeList newNodes = docNew.GetElementsByTagName("Dynamo.Graph.Nodes.Now");
            if (!oldNodes[0].InnerXml.Equals(newNodes[0].InnerXml))
            {
                Assert.Fail("the content of the deprecated node has been changed after saving");
            }

            oldNodes = docOld.GetElementsByTagName("Dynamo.Graph.Nodes.Future");
            newNodes = docNew.GetElementsByTagName("Dynamo.Graph.Nodes.Future");
            if (!oldNodes[0].InnerXml.Equals(newNodes[0].InnerXml))
            {
                Assert.Fail("the content of the deprecated node has been changed after saving");
            }
        }
        #endregion

        #region Dynamo Package Node Migration Tests
        [Test]
        [Category("UnitTests")]
        public void TestPackageNodeMigrationForJSONGraphs()
        {
            // Define package loading reference paths
            var dir = TestDirectory;
            var pkgDir = Path.Combine(dir, "pkgs\\MigrationTesting");
            var legacyGraph = Path.Combine(pkgDir, "extra\\LegacyPackageSampleGraph.dyn");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package which includes migrations
            loader.LoadPackages(new List<Package> {pkg});

            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "MigrationTesting");

            // Load the legacy graph, which contains 4 ZeroTouch/NodeModel test cases that use
            // old class names than were renamed in the version of the package we are loading.
            // Verify these nodes don't appear as dummy nodes and are successfully migrated.
            TestMigration(legacyGraph);

            // Verify all 4 nodes exist in the workspace and were properly loaded/opened from above
            Assert.AreEqual(4, this.CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void TestZTNodeMigrationJSON_WithDifferentMethodNameAndParams()
        {
            var legacyGraph = Path.Combine(TestDirectory, "core","migration","TestMigrationFFIClass.dyn");
            TestMigration(legacyGraph);

            Assert.AreEqual(8, this.CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            AssertPreviewValue("408bd6b17f7f43b28a29175bf12fb01f", "hello");
            AssertPreviewValue("013bd08a0f574e85b1d9676ba7004990", "migrated");
            AssertPreviewValue("df483e75085340b2a8c359a59dc8ea7a", "migrated");
        }
        #endregion

        #region Private Helper Methods

        private string GetDynPath(string sourceDynFile)
        {
            string sourceDynPath = TestDirectory;
            sourceDynPath = Path.Combine(sourceDynPath, @"core\migration\");
            return Path.Combine(sourceDynPath, sourceDynFile);
        }

        private CodeBlockNodeModel GetCodeBlockNode(string nodeGuid)
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            return workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                System.Guid.Parse(nodeGuid));
        }

        private void TestMigration(string filename)
        {
            OpenModel(GetDynPath(filename));
            Assert.DoesNotThrow(BeginRun);

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            int unresolvedNodeCount = 0;
            string str = "\n";

            foreach (var node in nodes.OfType<DummyNode>())
            {
                if (node.NodeNature == DummyNode.Nature.Unresolved)
                {
                    unresolvedNodeCount++;
                    str += node.Name;
                    str += "\n";
                }
            }

            if (unresolvedNodeCount >= 1)
            {
                Assert.Fail("Number of unresolved nodes found in TestCase: " + unresolvedNodeCount + str);
            }
        }

        #endregion
    }
}
