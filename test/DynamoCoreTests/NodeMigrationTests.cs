using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Xml;
using CoreNodeModels;
using CoreNodeModels.Input;
using CoreNodeModels.Logic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using PythonNodeModels;

namespace Dynamo.Tests
{
    public class NodeMigrationTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
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

        [Test]
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

        [Test]
        public void TestMigration_InputOutput_Excel()
        {
            TestMigration("TestMigration_InputOutput_Excel.dyn");
        }

        [Test]
        public void TestMigration_InputOutput_File()
        {
            TestMigration("TestMigration_InputOutput_File.dyn");
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
            var numberRange = workspace.NodeFromWorkspace<DoubleInput>(
                "eb6aa95d-8be4-4ca7-95a6-e696904a71fa");
            var numberStep = workspace.NodeFromWorkspace<DoubleInput>(
                "5fde015f-8a95-4b46-ba64-29de06850938");
            var numberCount = workspace.NodeFromWorkspace<DoubleInput>(
                "ace69b4e-5092-42cf-9fba-9aee6729509d");
            var numberApprox = workspace.NodeFromWorkspace<DoubleInput>(
                "30e9b7fd-fc09-4243-a34a-146ad841868a");
            var numberIncr = workspace.NodeFromWorkspace<DoubleInput>(
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

            // check that the node is migrated to a DSFunction nicknamed "ReferencePoint.ByPoint"
            StringAssert.Contains("Reference", workspace.NodeFromWorkspace<DSFunction>(
                "d615cc73-d32d-4b1f-b519-0b8f9b903ebf").NickName);
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

            // check that the node is migrated to a DSFunction nicknamed "FamilyInstance.ByPoint"
            StringAssert.Contains("Instance", workspace.NodeFromWorkspace<DSFunction>(
                "fc83b9b2-42c6-4a9f-8f60-a6ee29ef8a34").NickName);
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

            // check that the node is migrated to a DSFunction nicknamed "ModelCurve.ByCurve"
            StringAssert.Contains("Model", workspace.NodeFromWorkspace<DSFunction>(
                "fdea006e-b127-4280-a407-4058b78b93a3").NickName);
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

        [Test]
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
            Assert.AreEqual(4, workspace.Nodes.AsQueryable().Count(x => x.NickName.Contains("Excel")));
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
            var res = CurrentDynamoModel.CurrentWorkspace.SaveAs(newPath, model.EngineController.LiveRunnerRuntimeCore);

            Assert.IsTrue(res);
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
            var res = CurrentDynamoModel.CurrentWorkspace.SaveAs(newPath, model.EngineController.LiveRunnerRuntimeCore);

            Assert.IsTrue(res);
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
                    str += node.NickName;
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
