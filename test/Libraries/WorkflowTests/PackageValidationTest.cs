using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Path = System.IO.Path;

namespace Dynamo.Tests
{
    public class PackageValidationTest : DynamoModelTestBase
    {
        protected override string GetUserUserDataRootFolder()
        {
            string userDataDirectory = Path.Combine(TestDirectory, @"core\userdata");
            return userDataDirectory;
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }
        
        [Test]
        public void TestLoadPackageFromOtherLocation()
        {
            var testFilePath = Path.Combine(TestDirectory, @"core\userdata\packageTest.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("5e44d891-d2bb-4188-9f20-6ced9b430b4f", new object[] { 1,2,3});
        }

        #region Dynamo Text Package Tests

        [Test]
        public void TestTextFromDynamoText()
        {
            var testFilePath = Path.Combine(TestDirectory, @"core\userdata\BasicTextTest.dyn");
            RunModel(testFilePath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());


            var textNodeID = "343723bb-fdac-49c7-ace8-c2cc91a2fae2";
            AssertPreviewCount(textNodeID, 268);

            // output will be 268 Lines, so putting verification for all Lines creation
            for (int i = 0; i < 267; i++)
            {
                var linesFromText = GetPreviewValueAtIndex
                                        (textNodeID, i) as Autodesk.DesignScript.Geometry.Line;
                Assert.IsNotNull(linesFromText);
            }

            var thickenNodeID = "c44bf1a4-2b18-480d-af18-1640ec5cc4fe";
            AssertPreviewCount(thickenNodeID, 268);

             // output will be 268 Solids, so putting verification for all Solid creation
            for (int i = 0; i < 267; i++)
            {
                var solids = GetPreviewValueAtIndex
                                        (thickenNodeID, i) as Autodesk.DesignScript.Geometry.Solid;
                Assert.IsNotNull(solids);
            }
        }

        #endregion

    }
}
