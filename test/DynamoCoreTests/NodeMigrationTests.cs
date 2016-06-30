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
        public void TestMigration_DSCoreNodesUI_to_CoreNodeModels()
        {
            TestMigration("TestMigration_DSCoreNodesUI_to_CoreNodeModels.dyn");
        }

        [Test]
        [Category("Failure")]
        public void TestMigration_Core_Time()
        {
            TestMigration("TestMigration_Core_Time.dyn");
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
