using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class DSCoreDataTests : DynamoModelTestBase
    {
        // Preload required libraries
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSCPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void LoadJSONFromFile()
        {
            // Load JSON file 
            string jsonFilePath = Path.Combine(TestDirectory, @"core\json\JSONFile.json");
            JObject jsonObject = JObject.Parse(File.ReadAllText(jsonFilePath));

            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);
            AssertNoDummyNodes();

            // Get node data
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var engine = CurrentDynamoModel.EngineController;
            // Get Dictionary Components node
            Guid testNodeGuid = Guid.Parse("aa367b7b-22c5-492e-be30-9690c8a45960");
            NodeModel testNode = workspace.NodeFromWorkspace(testNodeGuid);

            // Get test node data
            var rawVal = testNode.GetValue(0, engine).Data;

            // Verify the test node is returning a valid dictionary
            Assert.AreEqual(typeof(DesignScript.Builtin.Dictionary).FullName, rawVal.GetType().FullName);

            // Compare the keys and values of the node result against the test loaded result
            var dictionary = (rawVal as DesignScript.Builtin.Dictionary);

            var nodeKeys = dictionary.Keys.ToList();
            var nodeValues = dictionary.Values.ToList();
            var localKeys = jsonObject.Properties().Select(p => p.Name).ToList();
            var localValues = jsonObject.Properties().Select(p => p.Value).ToList();

            // Sort
            nodeKeys.Sort();
            localKeys.Sort();

            // Verify list lengths
            Assert.AreEqual(nodeKeys.Count, localKeys.Count);
            Assert.AreEqual(nodeValues.Count, nodeValues.Count);

            // Verify keys
            Assert.AreEqual(nodeKeys, localKeys);
        }

        [Test]
        [Category("UnitTests")]
        public void CompareDictionaryAndJSON()
        {
            // Load JSON file 
            string jsonFilePath = Path.Combine(TestDirectory, @"core\json\JSONFile.json");
            JObject jsonObject = JObject.Parse(File.ReadAllText(jsonFilePath));

            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);

            // Assert all keys match between Dynamo Dictionary and parsed JSON
            AssertPreviewValue("5b0b1aba-ee6b-420e-aa12-e54270c00718", true);

            // Assert all values match between Dynamo Dictionary and parsed JSON
            AssertPreviewValue("5b0b1aba-ee6b-420e-aa12-e54270c00718", true);
        }

        [Test]
        [Category("UnitTests")]
        public void ParseJSON()
        {
            // Open the test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);

            // Get node data
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            Guid testNodeGuid = Guid.Parse("9dca6adc-dcf2-436a-9317-43a0af5195bc");
            NodeModel testNode = workspace.NodeFromWorkspace(testNodeGuid);

            // Expected parsed types
            string[] expectedOutputs = new string[]
            {
                    typeof(System.Boolean).FullName,
                    typeof(System.Boolean).FullName,
                    typeof(DesignScript.Builtin.Dictionary).FullName,
                    typeof(System.String).FullName,
                    typeof(System.Int64).FullName,
                    typeof(System.DateTime).FullName,
                    typeof(System.Double).FullName,
                    typeof(DesignScript.Builtin.Dictionary).FullName
            };

            // Verify node output types match expected output
            AssertPreviewValue(testNode.GUID.ToString(), expectedOutputs);
        }

        [Test]
        [Category("UnitTests")]
        public void StringifyJSON()
        {
            // Open the test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);

            // Get node data
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            Guid testNodeGuid = Guid.Parse("31f973f9-0764-47e1-b770-7b6b97d8803e");
            NodeModel testNode = workspace.NodeFromWorkspace(testNodeGuid);

            // Expected parsed types
            var expectedString = typeof(System.String).FullName;
            var expectedChar = typeof(System.Char).FullName;

            // Verify node output types match expected output
            AssertPreviewValue(testNode.GUID.ToString(), new[]
            {
                expectedString, expectedString, expectedChar, expectedString, expectedString, expectedString,
                expectedString, expectedString

            });
        }

        [Test]
        [Category("UnitTests")]
        public void VerifyStringifyReplication()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Replication.dyn");
            OpenModel(path);

            // Verify applying replication produces different result
            AssertPreviewValue("b69f2b6e-a328-43cf-a7a3-db42576ce814", true);

            // Verify DesignScript and node stringify functionality produces the same results - no replication.
            AssertPreviewValue("e81d053f-dbc5-4b1e-86df-075c9b279aa3", true);

            // Verify DesignScript and node stringify functionality produces the same results - with replication.
            AssertPreviewValue("a0af3136-aaaf-40c2-b2ef-cee522f9ea45", true);
        }

        [Test]
        [Category("UnitTests")]
        public void VerifyDesignScriptMatchesNodes()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_DesignScript.dyn");
            OpenModel(path);
            AssertNoDummyNodes();

            // Verify DesignScript usage of ParseJSON and StringifyJSON results match nodes
            AssertPreviewValue("5df5ed6d-6270-4018-a479-4f7cefcf7fe8", true);
        }

        [Test]
        [Category("UnitTests")]
        public void ParsingJSONInPythonReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_PythonJSONParsing.dyn");
            OpenModel(path);

            // Verify keys match when parsing JSON via Python
            AssertPreviewValue("e4e600d9-12a6-400e-adb3-02c1ad26cddf", true);

            // Verify values match when parsing JSON via Python
            AssertPreviewValue("cdad5bf1-f5f7-47f4-a119-ad42e5084cfa", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForBoundingBoxReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Abstract_BoundingBox_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("abb39e07-db08-45cf-9438-478defffbf68", true);

            // Verify current failure cases are all false
            AssertPreviewValue("eb9130a1-309c-492a-9679-28ad5ef8fddf", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForCoordinateSystemReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Abstract_CoordinateSystem_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("07366adaf0954529b1ed39b240192c96", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForPlaneReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Abstract_Plane_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("9754cbd66d4842419a6899f372a80aee", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForVectorReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Abstract_Vector_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("71efc8c5c0c74189901707c30e6d5903", true);
        }

        [Test]
        [Category("Failure")]
        public void RoundTripForArcReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Arc_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("71efc8c5c0c74189901707c30e6d5903", true);

            // Verify current failure cases are all false
            AssertPreviewValue("82304dd5025948f8a5644a84a32d58d4", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForCircleReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Circle_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("54d56712f1fa41948a5262aaf4eba5ba", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForEllipseReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Ellipse_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("a29aa179c7ae4069a6d9c6d2055ab845", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForEllipseArcReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_EllipseArc_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("a29aa179c7ae4069a6d9c6d2055ab845", true);

            // Verify current failure cases are all false
            AssertPreviewValue("a73925f57d2c44d7994a2c4d77bf8581", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForHelixReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Helix_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("b6a4919b3dd94eb79a7f0435d941d235", true);

            // Verify current failure cases are all false
            AssertPreviewValue("1bbd147b429c43ab8fe46a00d691a024", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForLineReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Line_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("71efc8c5c0c74189901707c30e6d5903", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForNurbsCurveReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_NurbsCurve_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("423356e2c8f84e00aa6c50e9bdb72c98", true);
        }

        [Test]
        [Category("Failure")]
        public void RoundTripForPolyCurveReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_PolyCurve_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("423356e2c8f84e00aa6c50e9bdb72c98", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForPolygonReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Polygon_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("015f80f917374031b345b46b5a8d54ca", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForRectangleReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Rectangle_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("9754cbd66d4842419a6899f372a80aee", true);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForPointReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Point_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("71efc8c5c0c74189901707c30e6d5903", true);
        }

        [Test]
        [Category("Failure")]
        public void RoundTripForCylinderReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Solid_Cylinder_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("07366adaf0954529b1ed39b240192c96", true);
        }

        [Test]
        [Category("UnitTests")]
        public void CanObjectBeCachedRejectsNull()
        {
            var canCacheNull = DSCore.Data.CanObjectBeCached(null);
            Assert.IsFalse(canCacheNull);
        }

        [Test]
        [Category("UnitTests")]
        public void CanObjectBeCachedRejectsNullString()
        {
            var canCacheStringNull = DSCore.Data.CanObjectBeCached("null");
            Assert.IsFalse(canCacheStringNull);
        }

        [Test]
        [Category("UnitTests")]
        public void CanObjectBeCachedRejectsEmptyString()
        {
            var canCacheEmptyList = DSCore.Data.CanObjectBeCached(new ArrayList() { });
            Assert.IsFalse(canCacheEmptyList);
        }

        [Test]
        [Category("UnitTests")]
        public void RememberRestoresFromCacheWhenPassedUnsupportedInputAndValidCacheJson()
        {
            var validCachedJson = "2";
            object unsupportedInput = null;
            var dict = DSCore.Data.Remember(unsupportedInput, validCachedJson);

            var returnObject = dict[">"];
            var returnCacheJson = dict["Cache"];

            Assert.AreEqual(2, returnObject);
            Assert.AreEqual(validCachedJson, returnCacheJson);
        }

        [Test]
        [Category("UnitTests")]
        public void RememberReturnUnsupportedInputWhenPassedUnsupportedInputAndEmptyCacheJson()
        {
            var emptyCachedJson = "";
            object unsupportedInput = null;
            var dict = DSCore.Data.Remember(unsupportedInput, emptyCachedJson);

            var returnObject = dict[">"];
            var returnCacheJson = dict["Cache"];

            Assert.AreEqual(unsupportedInput, returnObject);
            Assert.AreEqual("", returnCacheJson);
        }

        [Test]
        [Category("UnitTests")]
        public void RememberWillUpdateCacheWhenPassedSupportedInputAndValidCacheJson()
        {
            var validCachedJson = "2";
            var newInputObject = true;
            var dict = DSCore.Data.Remember(newInputObject, validCachedJson);

            var returnObject = dict[">"];
            var returnCacheJson = dict["Cache"];

            Assert.AreEqual(returnObject.GetType(), typeof(Boolean));
            Assert.AreEqual(newInputObject, returnObject);
            Assert.AreEqual("true", returnCacheJson);
        }

        [Test]
        [Category("UnitTests")]
        public void RememberWillUpdateCacheWhenPassedSupportedInputAndEmptyCacheJson()
        {
            var emptyCachedJson = "";
            var newInputObject = true;
            var dict = DSCore.Data.Remember(newInputObject, emptyCachedJson);

            var returnObject = dict[">"];
            var returnCacheJson = dict["Cache"];

            Assert.AreEqual(returnObject.GetType(), typeof(Boolean));
            Assert.AreEqual(newInputObject, returnObject);
            Assert.AreEqual("true", returnCacheJson);
        }
    }
}
