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
