using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using DynamoUnits;
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
        public void SerializingObjectOverMaximumDepthFailes()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Serialization_Depth_Fail.dyn");
            OpenModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("cc45bec3172e40dab4d967e9dd81cbdd"));

            var expectedWarning = "Exceeds MaxDepth";

            Assert.AreEqual(node.State, ElementState.Warning);
            AssertPreviewValue("cc45bec3172e40dab4d967e9dd81cbdd", null);
            Assert.AreEqual(node.Infos.Count, 1);
            Assert.IsTrue(node.Infos.Any(x => x.Message.Contains(expectedWarning) && x.State == ElementState.Warning));
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

            // Currently we do not support oriented BB.
            // This test will verify current unsupported cases
            AssertPreviewValue("9d611e10bea84fbc93648516e9f677f7", true);
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
        [Category("UnitTests")]
        public void RoundTripForArcReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\Curve_Arc_JSONParsing.dyn");
            OpenModel(path);

            // Verify objects match when serializing / de-serializing geometry type
            AssertPreviewValue("71efc8c5c0c74189901707c30e6d5903", true);

            // A known issue is that Arcs do not deserialize with the same start angle value.
            // It is always zero although the curve topology is identical.
            // This will verify the current known edge case.
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

            // A known issue is that EllipseArcs do not deserialize with the same start angle value.
            // It is always zero although the curve topology is identical.
            // This will verify the current known edge case.
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

            // A known issue is that Helix do not deserialize with the same type.
            // It is always converted to nurbscurve (Same as SAB serialization).
            // When the spiral GeoemtrySchema type is finalized we use it to support helix. 
            // This will verify the current known unsupported case.
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        public void RoundTripForColorReturnsSameResult()
        {
            var color = DSCore.Color.ByARGB(25, 30, 35, 40);
            var json = DSCore.Data.StringifyJSON(color);
            var color2 = (DSCore.Color)DSCore.Data.ParseJSON(json);

            Assert.AreEqual(color.Red, color2.Red);
            Assert.AreEqual(color.Green, color2.Green);
            Assert.AreEqual(color.Blue, color2.Blue);
            Assert.AreEqual(color.Alpha, color2.Alpha);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForLocationReturnsSameResult()
        {
            var location = DynamoUnits.Location.ByLatitudeAndLongitude(43.6606, 73.0357, "Dynamo");
            var json = DSCore.Data.StringifyJSON(location);
            var location2 = (DynamoUnits.Location)DSCore.Data.ParseJSON(json);

            Assert.AreEqual(location.Latitude, location2.Latitude);
            Assert.AreEqual(location.Longitude, location2.Longitude);
            Assert.AreEqual(location.Name, location2.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void RoundTripForImageReturnsSameResult()
        {
            string path = Path.Combine(TestDirectory, @"core\json\TestColor.bmp");
            Bitmap bitmap1 = new Bitmap(path);
            var json = DSCore.Data.StringifyJSON(bitmap1);
            var bitmap2 = (Bitmap)DSCore.Data.ParseJSON(json);

            Assert.AreEqual(bitmap1.Width, bitmap2.Width);
            Assert.AreEqual(bitmap1.Height, bitmap2.Height);
            Assert.AreEqual(bitmap1.GetPixel(5, 5), bitmap2.GetPixel(5, 5));
            Assert.AreEqual(bitmap1.GetPixel(195, 5), bitmap2.GetPixel(195, 5));
            Assert.AreEqual(bitmap1.GetPixel(195, 95), bitmap2.GetPixel(195, 95));
            Assert.AreEqual(bitmap1.GetPixel(5, 95), bitmap2.GetPixel(5, 95));
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
        public void RememberRestoresFromCacheWhenPassedUnsupportedInputAndvCacheJson()
        {
            var vCachedJson = "2";
            object unsupportedInput = null;
            var dict = DSCore.Data.Remember(unsupportedInput, vCachedJson);

            var returnObject = dict[">"];
            var returnCacheJson = dict["Cache"];

            Assert.AreEqual(2, returnObject);
            Assert.AreEqual(vCachedJson, returnCacheJson);
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
        public void RememberWillUpdateCacheWhenPassedSupportedInputAndvCacheJson()
        {
            var vCachedJson = "2";
            var newInputObject = true;
            var dict = DSCore.Data.Remember(newInputObject, vCachedJson);

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

        [Test]
        [Category("UnitTests")]
        public void ThrowsWhenPassedUnsupportedInputAndInvCacheJson()
        {
            var invCachedJson = "{\"one: 2}";
            object unsupportedInput = null;

            Assert.That(() => DSCore.Data.Remember(unsupportedInput, invCachedJson), Throws.Exception);
        }

        [Test]
        [Category("UnitTests")]
        public void ThrowsWhenPassedAnObjectThatCanNotSerialize()
        {
            var vCachedJson = "";
            object unsupportedInput = new FileInfo(Path.Combine(TestDirectory, @"core\json\Solid_Cylinder_JSONParsing.dyn"));

            Assert.That(() => DSCore.Data.Remember(unsupportedInput, vCachedJson), Throws.Exception);
        }


        [Test]
        [Category("UnitTests")]
        public void IsNotSupportedNullInput()
        {
            object nullInput = null;
            var vType = typeof(String);
            var invType = typeof(Nullable);

            var vString = "input string";

            // Assert - check for nulls - fail
            var validate = DSCore.Data.IsSupportedDataNodeDynamoType(nullInput, vType, false);
            Assert.AreEqual(false, validate, "Null input should not be supported.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vString, invType, false);
            Assert.AreEqual(false, validate, "Unexisting enum type should not be supported.");
        }

        [Test]
        [Category("UnitTests")]
        public void IsSupportedPrimitiveDataType()
        {
            var vString = "input string";
            var vInt = 5;
            var vDouble = 3.14;
            var vDateTime = DSCore.DateTime.ByDate(2001, 1, 1);
            var vLocation = Location.ByLatitudeAndLongitude(50, 50);
            var vTimeSpan = DSCore.TimeSpan.ByDateDifference(vDateTime, DSCore.DateTime.ByDate(2000, 1, 1));

            var invStringList = new ArrayList() { vString, vInt, vDouble };
            var vStringList = new ArrayList() { vString };
            var vIntList = new ArrayList() { vInt };
            var vDoubleList = new ArrayList() { vDouble };
            var vDateTimeList = new ArrayList() { vDateTime };
            var vLocationList = new ArrayList() { vLocation };
            var vTimeSpanList = new ArrayList() { vTimeSpan };

            // Assert - check list - succeed
            var singleChecks = new Dictionary<Type, object>();
            singleChecks[typeof(string)] = vString;
            singleChecks[typeof(int)] = vInt;
            singleChecks[typeof(double)] = vDouble;
            singleChecks[typeof(DateTime)] = vDateTime;
            singleChecks[typeof(Location)] = vLocation;
            singleChecks[typeof(TimeSpan)] = vTimeSpan;

            foreach (var kv in singleChecks)
            {
                Assert.AreEqual(
                    true,
                    DSCore.Data.IsSupportedDataNodeDynamoType(kv.Value, kv.Key, false),
                    String.Format($"Couldn't validate {kv.Key} input."));
            }

            // Assert - check list - fail
            var validate = DSCore.Data.IsSupportedDataNodeDynamoType(vStringList, typeof(string), false);
            Assert.AreEqual(false, validate, "Shouldn't validate list values with list flag off.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vString, typeof(string), true);
            Assert.AreEqual(false, validate, "Shouldn't validate single values with list flag on.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(invStringList, typeof(string), true);
            Assert.AreEqual(false, validate, "Shouldn't validate heterogenous list input.");

            // Assert - check homogenous list values - succeed
            var listChecks = new Dictionary<Type, object>();
            listChecks[typeof(string)] = vStringList;
            listChecks[typeof(int)] = vIntList;
            listChecks[typeof(double)] = vDoubleList;
            listChecks[typeof(DateTime)] = vDateTimeList;
            listChecks[typeof(Location)] = vLocationList;
            listChecks[typeof(TimeSpan)] = vTimeSpanList;

            foreach (var kv in listChecks)
            {
                Assert.AreEqual(
                    true,
                    DSCore.Data.IsSupportedDataNodeDynamoType(kv.Value, kv.Key, true),
                    String.Format($"Couldn't validate {kv.Key} list input."));
            }
        }


        [Test]
        [Category("UnitTests")]
        public void IsSupportedGeometryDataType()
        {
            // Single values and primitive composites
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(1, 1, 1);
            var point2 = Autodesk.DesignScript.Geometry.Point.ByCoordinates(2, 2, 1);
            var point3 = Autodesk.DesignScript.Geometry.Point.ByCoordinates(3, 3, 3);
            var vector = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByBestFitThroughPoints([point, point2, point3]);
            var vBoundingBox = BoundingBox.ByCorners(point, point3);
            var vCoordinateSystem = CoordinateSystem.ByOrigin(0, 0);
            var vSurface = Surface.ByPerimeterPoints([point, point2, point3]);
            var vUV = UV.ByCoordinates(0, 0);
            var vCurve = Curve.ByParameterLineOnSurface(vSurface, vUV, vUV);
            var vArc = Arc.ByThreePoints(point, point2, point3);
            var vCircle = Circle.ByBestFitThroughPoints([point, point2, point3]);
            var vEllipse = Ellipse.ByOriginRadii(point, 5, 5);
            var vEllipseArc = EllipseArc.ByPlaneRadiiAngles(plane, 2, 2, 0, 180);
            var vHelix = Helix.ByAxis(point, vector, point3, 1, 360);
            var vLine = Line.ByBestFitThroughPoints([point, point2, point3]);
            var vNurbsCurve = NurbsCurve.ByControlPoints([point, point2, point3, point]);
            var vPolyCurve = PolyCurve.ByJoinedCurves([vCurve, vCurve], 0.001);
            var vPolygon = Polygon.ByPoints([point, point2, point3, point]);
            var vRectangle = Autodesk.DesignScript.Geometry.Rectangle.ByWidthLength(5, 10);
            var indexGroup = IndexGroup.ByIndices(0, 1, 2);
            var vMesh = Mesh.ByPointsFaceIndices([point, point2, point3], [indexGroup, indexGroup, indexGroup]);
            var vSolid = Solid.ByJoinedSurfaces([vSurface, vSurface, vSurface, vSurface, vSurface, vSurface]);
            var vCone = Cone.ByCoordinateSystemHeightRadii(vCoordinateSystem, 1, 1, 1);
            var vCylinder = Cylinder.ByPointsRadius(point, point3, 1);
            var vCuboid = Cuboid.ByCorners(point, point3);
            var vSphere = Sphere.ByCenterPointRadius(point, 1);
            var vNurbsSurface = NurbsSurface.ByControlPoints([[point, point2, point3, point],
                [point, point3, point2, point],
                [point2, point3, point, point2]], 2, 2);
            var vPolySurface = PolySurface.ByJoinedSurfaces([vSurface, vSurface]);

            // Lists
            var invBoundingBoxList = new ArrayList() { point, vBoundingBox };
            var vBoundingBoxList = new ArrayList() { vBoundingBox, vBoundingBox };
            var vCoordinateSystemList = new ArrayList() { vCoordinateSystem, vCoordinateSystem };
            var vPointList = new ArrayList() { point, point };
            var vVectorList = new ArrayList() { vector, vector };
            var vPlaneList = new ArrayList() { plane, plane };
            var vSurfaceList = new ArrayList() { vSurface, vSurface };
            var vUVList = new ArrayList() { vUV, vUV };
            var vCurveList = new ArrayList() { vCurve, vCurve };
            var vArcList = new ArrayList() { vArc, vArc };
            var vCircleList = new ArrayList() { vCircle, vCircle };
            var vEllipseList = new ArrayList() { vEllipse, vEllipse };
            var vEllipseArcList = new ArrayList() { vEllipseArc, vEllipseArc };
            var vHelixList = new ArrayList() { vHelix, vHelix };
            var vLineList = new ArrayList() { vLine, vLine };
            var vNurbsCurveList = new ArrayList() { vNurbsCurve, vNurbsCurve };
            var vPolyCurveList = new ArrayList() { vPolyCurve, vPolyCurve };
            var vPolygonList = new ArrayList() { vPolygon, vPolygon };
            var vRectangleList = new ArrayList() { vRectangle, vRectangle };
            var vMeshList = new ArrayList() { vMesh, vMesh };
            var vSolidList = new ArrayList() { vSolid, vSolid };
            var vConeList = new ArrayList() { vCone, vCone };
            var vCylinderList = new ArrayList() { vCylinder, vCylinder };
            var vCuboidList = new ArrayList() { vCuboid, vCuboid };
            var vSphereList = new ArrayList() { vSphere, vSphere };
            var vNurbsSurfaceList = new ArrayList() { vNurbsSurface, vNurbsSurface };
            var vPolySurfaceList = new ArrayList() { vPolySurface, vPolySurface };

            // Assert - check signle values - succeed
            var singleChecks = new Dictionary<Type, object>();
            singleChecks[typeof(BoundingBox)] = vBoundingBox;
            singleChecks[typeof(CoordinateSystem)] = vCoordinateSystem;
            singleChecks[typeof(Autodesk.DesignScript.Geometry.Point)] = point;
            singleChecks[typeof(Vector)] = vector;
            singleChecks[typeof(Plane)] = plane;
            singleChecks[typeof(Surface)] = vSurface;
            singleChecks[typeof(UV)] = vUV;
            singleChecks[typeof(Curve)] = vCurve;
            singleChecks[typeof(Arc)] = vArc;
            singleChecks[typeof(Circle)] = vCircle;
            singleChecks[typeof(Ellipse)] = vEllipse;
            singleChecks[typeof(EllipseArc)] = vEllipseArc;
            singleChecks[typeof(Helix)] = vHelix;
            singleChecks[typeof(Line)] = vLine;
            singleChecks[typeof(NurbsCurve)] = vNurbsCurve;
            singleChecks[typeof(PolyCurve)] = vPolyCurve;
            singleChecks[typeof(Polygon)] = vPolygon;
            singleChecks[typeof(Autodesk.DesignScript.Geometry.Rectangle)] = vRectangle;
            singleChecks[typeof(Mesh)] = vMesh;
            singleChecks[typeof(Solid)] = vSolid;
            singleChecks[typeof(Cone)] = vCone;
            singleChecks[typeof(Cylinder)] = vCylinder;
            singleChecks[typeof(Cuboid)] = vCuboid;
            singleChecks[typeof(Sphere)] = vSphere;
            singleChecks[typeof(NurbsSurface)] = vNurbsSurface;
            singleChecks[typeof(PolySurface)] = vPolySurface;

            foreach (var kv in singleChecks)
            {
                Assert.AreEqual(
                    true,
                    DSCore.Data.IsSupportedDataNodeDynamoType(kv.Value, kv.Key, false),
                    String.Format($"Couldn't validate {kv.Key} input."));
            }

            // Assert - check list - fail
            var validate = DSCore.Data.IsSupportedDataNodeDynamoType(invBoundingBoxList, typeof(BoundingBox), true);
            Assert.AreEqual(false, validate, "Shouldn't validate heterogenous list input.");

            // Assert - check homogenous list values - succeed
            var listChecks = new Dictionary<Type, object>();
            listChecks[typeof(BoundingBox)] = vBoundingBoxList;
            listChecks[typeof(CoordinateSystem)] = vCoordinateSystemList;
            listChecks[typeof(Autodesk.DesignScript.Geometry.Point)] = vPointList;
            listChecks[typeof(Vector)] = vVectorList;
            listChecks[typeof(Plane)] = vPlaneList;
            listChecks[typeof(Surface)] = vSurfaceList;
            listChecks[typeof(UV)] = vUVList;
            listChecks[typeof(Curve)] = vCurveList;
            listChecks[typeof(Arc)] = vArcList;
            listChecks[typeof(Circle)] = vCircleList;
            listChecks[typeof(Ellipse)] = vEllipseList;
            listChecks[typeof(EllipseArc)] = vEllipseArcList;
            listChecks[typeof(Helix)] = vHelixList;
            listChecks[typeof(Line)] = vLineList;
            listChecks[typeof(NurbsCurve)] = vNurbsCurveList;
            listChecks[typeof(PolyCurve)] = vPolyCurveList;
            listChecks[typeof(Polygon)] = vPolygonList;
            listChecks[typeof(Autodesk.DesignScript.Geometry.Rectangle)] = vRectangleList;
            listChecks[typeof(Mesh)] = vMeshList;
            listChecks[typeof(Solid)] = vSolidList;
            listChecks[typeof(Cone)] = vConeList;
            listChecks[typeof(Cylinder)] = vCylinderList;
            listChecks[typeof(Cuboid)] = vCuboidList;
            listChecks[typeof(Sphere)] = vSphereList;
            listChecks[typeof(NurbsSurface)] = vNurbsSurfaceList;
            listChecks[typeof(PolySurface)] = vPolySurfaceList;

            foreach (var kv in listChecks)
            {
                Assert.AreEqual(
                    true,
                    DSCore.Data.IsSupportedDataNodeDynamoType(kv.Value, kv.Key, true),
                    String.Format($"Couldn't validate {kv.Key} list input."));
            }
        }

        [Test]
        [Category("UnitTests")]
        public void IsSupportedInheritanceDataType()
        {
            // Primitives
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(1, 1, 1);
            var point2 = Autodesk.DesignScript.Geometry.Point.ByCoordinates(2, 2, 1);
            var point3 = Autodesk.DesignScript.Geometry.Point.ByCoordinates(3, 3, 3);
            var vector = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByBestFitThroughPoints([point, point2, point3]);
            var vCoordinateSystem = CoordinateSystem.ByOrigin(0, 0);
            var vSurface = Surface.ByPerimeterPoints([point, point2, point3]);

            // Curve
            var vUV = UV.ByCoordinates(0, 0);
            var vCurve = Curve.ByParameterLineOnSurface(vSurface, vUV, vUV);
            var vArc = Arc.ByThreePoints(point, point2, point3);
            var vCircle = Circle.ByBestFitThroughPoints([point, point2, point3]);
            var vEllipse = Ellipse.ByOriginRadii(point, 5, 5);
            var vEllipseArc = EllipseArc.ByPlaneRadiiAngles(plane, 2, 2, 0, 180);
            var vHelix = Helix.ByAxis(point, vector, point3, 1, 360);
            var vLine = Line.ByBestFitThroughPoints([point, point2, point3]);
            var vNurbsCurve = NurbsCurve.ByControlPoints([point, point2, point3, point]);
            var vPolyCurve = PolyCurve.ByJoinedCurves([vCurve, vCurve], 0.001);
            var vPolygon = Polygon.ByPoints([point, point2, point3, point]);
            var vRectangle = Autodesk.DesignScript.Geometry.Rectangle.ByWidthLength(5, 10);


            var indexGroup = IndexGroup.ByIndices(0, 1, 2);
            var vMesh = Mesh.ByPointsFaceIndices([point, point2, point3], [indexGroup, indexGroup, indexGroup]);

            // Solid
            var vSolid = Solid.ByJoinedSurfaces([vSurface, vSurface, vSurface, vSurface, vSurface, vSurface]);
            var vCone = Cone.ByCoordinateSystemHeightRadii(vCoordinateSystem, 1, 1, 1);
            var vCylinder = Cylinder.ByPointsRadius(point, point3, 1);
            var vCuboid = Cuboid.ByCorners(point, point3);
            var vSphere = Sphere.ByCenterPointRadius(point, 1);

            // Surface
            var vNurbsSurface = NurbsSurface.ByControlPoints([[point, point2, point3, point],
                [point, point3, point2, point],
                [point2, point3, point, point2]], 2, 2);
            var vPolySurface = PolySurface.ByJoinedSurfaces([vSurface, vSurface]);

            // Heterogeneous lists
            var vCurveInheritanceList = new ArrayList() { vRectangle, vPolygon, vNurbsCurve, vEllipse };
            var vPolyCurveInheritanceList = new ArrayList() { vRectangle, vPolygon, vPolyCurve };
            var vSolidInheritanceList = new ArrayList() { vSolid, vCone, vCylinder, vCuboid, vSphere };
            var vSurfaceInheritanceList = new ArrayList() { vNurbsSurface, vPolySurface };

            var ivCurveInheritanceList = new ArrayList() { vRectangle, vPolygon, vNurbsCurve, vEllipse, vSphere };
            var ivPolyCurveInheritanceList = new ArrayList() { vRectangle, vPolygon, vPolyCurve, vCurve };
            var ivSolidInheritanceList = new ArrayList() { vPolygon, vCone, vCylinder, vCuboid, vSphere };
            var ivSurfaceInheritanceList = new ArrayList() { vNurbsSurface, vPolySurface, vCylinder, vCuboid, vSphere };

            // Assert - check single upward inheritance - succeed
            var validate = DSCore.Data.IsSupportedDataNodeDynamoType(vRectangle, typeof(Curve), false);
            Assert.AreEqual(true, validate, "Couldn't validate Rectangle inheritance from Curve.");

            // Assert - check single downward inheritance - fail 
            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vCurve, typeof(Autodesk.DesignScript.Geometry.Rectangle), false);
            Assert.AreEqual(false, validate, "Shouldn't validate Curve inheritance from Rectangle.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vCone, typeof(Curve), false);
            Assert.AreEqual(false, validate, "Shouldn't validate Cone inheritance from Curve.");

            // Assert - check heterogeneous list values - succeed
            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vCurveInheritanceList, typeof(Curve), true);
            Assert.AreEqual(true, validate, "Couldn't validate DataTypes inheriting from Curve in a heterogeneous list input.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vPolyCurveInheritanceList, typeof(PolyCurve), true);
            Assert.AreEqual(true, validate, "Couldn't validate DataTypes inheriting from PolyCurve in a heterogeneous list input.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vSolidInheritanceList, typeof(Solid), true);
            Assert.AreEqual(true, validate, "Couldn't validate DataTypes inheriting from Solid in a heterogeneous list input.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(vSurfaceInheritanceList, typeof(Surface), true);
            Assert.AreEqual(true, validate, "Couldn't validate DataTypes inheriting from Surface in a heterogeneous list input.");


            // Assert - check invalid heterogeneous list values - fail
            validate = DSCore.Data.IsSupportedDataNodeDynamoType(ivCurveInheritanceList, typeof(Curve), true);
            Assert.AreEqual(false, validate, "Shouldn't validate DataTypes inheriting from Curve if there is a Sphere in the list input.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(ivPolyCurveInheritanceList, typeof(PolyCurve), true);
            Assert.AreEqual(false, validate, "Shouldn't validate DataTypes - Curve does not inherit from PolyCurve.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(ivSolidInheritanceList, typeof(Solid), true);
            Assert.AreEqual(false, validate, "Shouldn't validate DataTypes inheriting from Solid if there is a Polygone in the list input.");

            validate = DSCore.Data.IsSupportedDataNodeDynamoType(ivSurfaceInheritanceList, typeof(Surface), true);
            Assert.AreEqual(false, validate, "Shouldn't validate DataTypes inheriting from Surface with Cylindar, Cuboid and Sphere in the list.");
        }
    }
}
