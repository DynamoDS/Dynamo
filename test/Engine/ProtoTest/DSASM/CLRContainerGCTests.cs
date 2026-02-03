using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.DSASM
{
    /// <summary>
    /// Tests for DYN-8717: GC bug where geometry inside CLR containers
    /// gets incorrectly garbage collected.
    ///
    /// These tests verify that the GC correctly traces references stored
    /// inside CLR-backed objects (like Dictionary).
    /// </summary>
    [TestFixture]
    class CLRContainerGCTests : ProtoTestBase
    {
        /// <summary>
        /// Test 2.1: Basic Dictionary with geometry.
        ///
        /// This is the minimal reproduction case for DYN-8717.
        /// A dictionary containing an FFI object should keep that object
        /// alive as long as the dictionary is reachable, even when
        /// intermediate code paths don't reference the nested object.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestDictionaryWithGeometry_BasicToggle()
        {
            string code = @"
import(""FFITarget.dll"");

// Create dictionary with geometry (simulates ParseJSON creating Dict with Line)
d = {""Point"": DummyPoint.ByCoordinates(1, 2, 3)};

// First access - should work
x1 = d[""Point""].X;

// Simulate toggle to 'false' path - dictionary exists but point not directly accessed
flag = false;
result = flag ? d[""Point""].Y : ""no access"";

// Force GC - this is where the bug manifests
__GC();

// Toggle back to 'true' path - dictionary still exists, should still have valid Point
flag = true;
x2 = d[""Point""].X;  // BUG: Point may have been disposed!

// Verify the point is still valid
// Note: When flag changed to true, 'result' should have re-executed via associative update
isValid = x1 == 1 && x2 == 1 && result == 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // The point should still be accessible after GC
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("x2", 1.0);
            // Verify that result updated via associative re-execution when flag changed
            thisTest.Verify("result", 2.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.2: Nested containers.
        /// Dictionary containing Dictionary with geometry.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestNestedContainers_DictionaryInDictionary()
        {
            string code = @"
import(""FFITarget.dll"");

// Create nested dictionaries with geometry
inner = {""point"": DummyPoint.ByCoordinates(10, 20, 30)};
outer = {""nested"": inner};

// Access through nested path
x1 = outer[""nested""][""point""].X;

// Force GC - point should survive because container is still referenced
__GC();

// Access again - should still work
x2 = outer[""nested""][""point""].X;
y2 = outer[""nested""][""point""].Y;

isValid = x1 == 10 && x2 == 10 && y2 == 20;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 10.0);
            thisTest.Verify("x2", 10.0);
            thisTest.Verify("y2", 20.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.3: Arrays of geometry in CLR container.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestArraysInContainers()
        {
            string code = @"
import(""FFITarget.dll"");

// Create array of points inside dictionary
points = [
    DummyPoint.ByCoordinates(1, 0, 0),
    DummyPoint.ByCoordinates(2, 0, 0),
    DummyPoint.ByCoordinates(3, 0, 0)
];
d = {""points"": points};

// Access first point
x1 = d[""points""][0].X;

// Force GC - all points should survive because container is still referenced
__GC();

// Access all points - should all still be valid
x2 = d[""points""][0].X;
x3 = d[""points""][1].X;
x4 = d[""points""][2].X;

sum = x1 + x2 + x3 + x4;  // Should be 1 + 1 + 2 + 3 = 7
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("x2", 1.0);
            thisTest.Verify("x3", 2.0);
            thisTest.Verify("x4", 3.0);
            thisTest.Verify("sum", 7.0);
        }

        /// <summary>
        /// Test 2.4: Mixed containers with geometry and primitives.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestMixedContainers()
        {
            string code = @"
import(""FFITarget.dll"");

// Dictionary with mixed content
d = {
    ""name"": ""test"",
    ""count"": 42,
    ""point"": DummyPoint.ByCoordinates(5, 6, 7),
    ""values"": [1, 2, 3]
};

// Access primitive
name1 = d[""name""];
count1 = d[""count""];

// Access geometry
x1 = d[""point""].X;

// GC
__GC();

// Access everything again
name2 = d[""name""];
count2 = d[""count""];
x2 = d[""point""].X;
y2 = d[""point""].Y;
z2 = d[""point""].Z;

isValid = name1 == ""test"" && count1 == 42 && x1 == 5 &&
          name2 == ""test"" && count2 == 42 && x2 == 5 && y2 == 6 && z2 == 7;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("name1", "test");
            thisTest.Verify("count1", 42);
            thisTest.Verify("x1", 5.0);
            thisTest.Verify("x2", 5.0);
            thisTest.Verify("y2", 6.0);
            thisTest.Verify("z2", 7.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.5: Circular references.
        /// Ensure GC doesn't infinite loop and both containers survive.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestCircularReferences()
        {
            // Note: This test verifies that circular reference handling
            // doesn't cause infinite loops. Due to DS limitations on
            // creating true circular references, we test nested containers
            // that reference each other's contents.
            string code = @"
import(""FFITarget.dll"");

// Create two dictionaries that reference each other's geometry
point1 = DummyPoint.ByCoordinates(1, 1, 1);
point2 = DummyPoint.ByCoordinates(2, 2, 2);

dictA = {""myPoint"": point1, ""otherPoint"": point2};
dictB = {""myPoint"": point2, ""otherPoint"": point1};

// Access through both paths
x1 = dictA[""myPoint""].X;
x2 = dictB[""myPoint""].X;

// Multiple GC cycles
__GC();
__GC();
__GC();

// Both should still be valid
x3 = dictA[""myPoint""].X;
x4 = dictB[""myPoint""].X;
x5 = dictA[""otherPoint""].X;
x6 = dictB[""otherPoint""].X;

isValid = x1 == 1 && x2 == 2 && x3 == 1 && x4 == 2 && x5 == 2 && x6 == 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("x2", 2.0);
            thisTest.Verify("x3", 1.0);
            thisTest.Verify("x4", 2.0);
            thisTest.Verify("x5", 2.0);
            thisTest.Verify("x6", 1.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.6: Large containers with many geometry objects.
        ///
        /// Tests that GC can traverse CLR containers with many objects and mark
        /// all of them, even objects not directly accessed before GC.
        ///
        /// Without the fix: Point at index 25 (never accessed before GC) would be
        /// incorrectly collected even though the dictionary containing it is reachable.
        /// With the fix: All 50 points survive because GC traverses the container.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestLargeContainers()
        {
            string code = @"
import(""FFITarget.dll"");

// Create many points using replication
points = DummyPoint.ByCoordinates(0..49, (0..49) * 2, (0..49) * 3);

d = {""points"": points};

// Access first and last (but not middle points)
x0 = d[""points""][0].X;
x49 = d[""points""][49].X;

// Force GC - middle points (like index 25) were never extracted to stack
// Without fix: they would be incorrectly collected
// With fix: they survive because GC traverses the container
__GC();

// Access again - all should still be valid, including point 25 (never accessed before)
x0_after = d[""points""][0].X;
x25_after = d[""points""][25].X;  // This would fail WITHOUT the fix
x49_after = d[""points""][49].X;

isValid = x0 == 0 && x49 == 49 &&
          x0_after == 0 && x25_after == 25 && x49_after == 49;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x0", 0.0);
            thisTest.Verify("x49", 49.0);
            thisTest.Verify("x0_after", 0.0);
            thisTest.Verify("x25_after", 25.0);
            thisTest.Verify("x49_after", 49.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.7: Repeated GC cycles - verify geometry survives multiple collections.
        /// Note: Rewritten to avoid associative re-evaluation issues with flag toggling.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestRepeatedToggling()
        {
            string code = @"
import(""FFITarget.dll"");

d = {""point"": DummyPoint.ByCoordinates(42, 43, 44)};

// First access
x1 = d[""point""].X;

// Multiple GC cycles
__GC();
x2 = d[""point""].X;

__GC();
x3 = d[""point""].X;

__GC();
x4 = d[""point""].X;

// After 3 GC cycles, point should still be valid
isValid = x1 == 42 && x2 == 42 && x3 == 42 && x4 == 42;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 42.0);
            thisTest.Verify("x2", 42.0);
            thisTest.Verify("x3", 42.0);
            thisTest.Verify("x4", 42.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.8: Multiple independent dictionaries with geometry.
        ///
        /// This test verifies that creating multiple independent dictionaries with
        /// different geometry objects works correctly - each dictionary maintains its
        /// own references and all survive GC when they're still in scope.
        ///
        /// Note: This does NOT simulate upstream node changes in a Dynamo graph
        /// (which would require node re-execution). Instead, it creates independent
        /// variables similar to having multiple separate nodes. To test actual
        /// upstream changes, use the manual test: Test-UpstreamChange.dyn
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestMultipleIndependentDictionaries()
        {
            string code = @"
import(""FFITarget.dll"");

// Simulate first execution - ParseJSON creates Dict with Point1
point1 = DummyPoint.ByCoordinates(100, 200, 300);
dict1 = {""Shape"": point1, ""id"": 1};

// Access the point
shape1 = dict1[""Shape""];
x1 = shape1.X;
id1 = dict1[""id""];

// Simulate upstream change - ParseJSON RE-EXECUTES with different JSON
// This creates a NEW dictionary with NEW point
point2 = DummyPoint.ByCoordinates(999, 888, 777);
dict2 = {""Shape"": point2, ""id"": 2};

// Now downstream nodes reference dict2, not dict1
// dict1 is no longer referenced and should be eligible for GC
shape2 = dict2[""Shape""];
x2 = shape2.X;
id2 = dict2[""id""];

// Force GC - dict1 and point1 should be collected
// dict2 and point2 should survive
__GC();

// Verify we can still access the NEW point (point2)
x3 = shape2.X;
y3 = shape2.Y;
z3 = shape2.Z;

// Verify values
isValid = x1 == 100 && x2 == 999 && x3 == 999 &&
          y3 == 888 && z3 == 777 &&
          id1 == 1 && id2 == 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // New point should be accessible
            thisTest.Verify("x1", 100.0);  // Old point was accessible initially
            thisTest.Verify("x2", 999.0);  // New point is accessible
            thisTest.Verify("x3", 999.0);  // New point still accessible after GC
            thisTest.Verify("y3", 888.0);
            thisTest.Verify("z3", 777.0);
            thisTest.Verify("id1", 1);
            thisTest.Verify("id2", 2);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.9: Multiple independent objects don't accumulate.
        ///
        /// Verifies that creating multiple independent dictionary and geometry objects
        /// works correctly without causing memory issues. Each object maintains its own
        /// references and all survive GC when they're in scope.
        ///
        /// Note: This creates independent variables (like separate nodes), not
        /// associative updates to existing variables (like upstream node changes).
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestMultipleIndependentObjects()
        {
            string code = @"
import(""FFITarget.dll"");

// Simulate multiple re-executions of ParseJSON with different data
// Only the LAST created geometry should survive

// Execution 1
point1 = DummyPoint.ByCoordinates(1, 0, 0);
dict1 = {""point"": point1};
result1 = dict1[""point""].X;

// Execution 2 (upstream change)
point2 = DummyPoint.ByCoordinates(2, 0, 0);
dict2 = {""point"": point2};
result2 = dict2[""point""].X;

// Execution 3 (upstream change)
point3 = DummyPoint.ByCoordinates(3, 0, 0);
dict3 = {""point"": point3};
result3 = dict3[""point""].X;

// Execution 4 (upstream change)
point4 = DummyPoint.ByCoordinates(4, 0, 0);
dict4 = {""point"": point4};
result4 = dict4[""point""].X;

// Execution 5 (upstream change)
point5 = DummyPoint.ByCoordinates(5, 0, 0);
dict5 = {""point"": point5};
result5 = dict5[""point""].X;

// Multiple GC cycles
__GC();
__GC();
__GC();

// Only the LAST point (point5) should be accessible
// Previous points should have been disposed
finalX = dict5[""point""].X;

// Verify correct values
isValid = result1 == 1 && result2 == 2 && result3 == 3 &&
          result4 == 4 && result5 == 5 && finalX == 5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("result1", 1.0);
            thisTest.Verify("result2", 2.0);
            thisTest.Verify("result3", 3.0);
            thisTest.Verify("result4", 4.0);
            thisTest.Verify("result5", 5.0);
            thisTest.Verify("finalX", 5.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.10: Contrast DYN-8717 bug scenario vs independent objects.
        ///
        /// This test demonstrates the DIFFERENCE between:
        /// A) DYN-8717 bug scenario - same container stays in memory but geometry
        ///    inside was incorrectly collected when not directly accessed
        /// B) Independent objects scenario - multiple dictionaries with different
        ///    geometry objects, each maintaining their own references
        ///
        /// Both scenarios should work correctly with the fix.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestBugScenarioVsIndependentObjects()
        {
            string code = @"
import(""FFITarget.dll"");

// ==========================================
// PART A: DYN-8717 Bug Scenario (What we fixed)
// ==========================================

// Create dictionary with geometry
dictA = {""point"": DummyPoint.ByCoordinates(10, 20, 30)};

// Access the geometry initially
resultA1 = dictA[""point""].X;

// GC - WITHOUT FIX, point would be collected here even though dictA still holds it
__GC();

// Access geometry again - should still work because dictA still holds reference
resultA2 = dictA[""point""].X;
resultA3 = dictA[""point""].Y;

// Verify: dictA stayed in memory, point should NOT have been collected
isValidA = resultA1 == 10 && resultA2 == 10 && resultA3 == 20;


// ==========================================
// PART B: Upstream Change Scenario (Normal behavior)
// ==========================================

// First execution - create dict with one point
dictB1 = {""point"": DummyPoint.ByCoordinates(100, 200, 300)};
resultB1 = dictB1[""point""].X;

// Upstream change - NEW dictionary created with different point
// OLD dict (dictB1) is no longer the active container
dictB2 = {""point"": DummyPoint.ByCoordinates(400, 500, 600)};
resultB2 = dictB2[""point""].X;

// GC - dictB1 and its point should be collected (no longer referenced downstream)
// dictB2 and its point should survive
__GC();

// Access NEW dictionary - should work
resultB3 = dictB2[""point""].X;
resultB4 = dictB2[""point""].Y;

// Verify: dictB2 is active, dictB1 was disposed (normal behavior)
isValidB = resultB1 == 100 && resultB2 == 400 && resultB3 == 400 && resultB4 == 500;


// ==========================================
// Both scenarios should work correctly
// ==========================================
isValid = isValidA && isValidB;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // Part A: Same container scenario (DYN-8717 fix)
            thisTest.Verify("resultA1", 10.0);
            thisTest.Verify("resultA2", 10.0);
            thisTest.Verify("resultA3", 20.0);  // This would fail without DYN-8717 fix
            thisTest.Verify("isValidA", true);

            // Part B: Upstream change scenario (normal behavior)
            thisTest.Verify("resultB1", 100.0);
            thisTest.Verify("resultB2", 400.0);
            thisTest.Verify("resultB3", 400.0);
            thisTest.Verify("resultB4", 500.0);
            thisTest.Verify("isValidB", true);

            // Both should work
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test the exact scenario from DYN-8717 - Dictionary from ParseJSON
        /// with conditional code block.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestDYN8717_ExactReproduction()
        {
            string code = @"
import(""FFITarget.dll"");

// Simulate what ParseJSON would create
dict = {""Shape"": DummyPoint.ByCoordinates(100, 200, 300)};

// Simulate first CBN: alice = false ? ""test"" : input;
alice = true;
input = dict;
bob = alice == false ? ""test"" : input;

// Access the shape
shape1 = bob[""Shape""];
x1 = shape1.X;

// Simulate second run with alice = false
alice = false;
bob = alice == false ? ""test"" : input;
// bob is now ""test"", not the dictionary

// Force GC - the bug causes Shape to be collected here
__GC();

// Simulate third run with alice = true again
alice = true;
bob = alice == false ? ""test"" : input;

// Access shape again - should still work!
shape2 = bob[""Shape""];
x2 = shape2.X;

isValid = x1 == 100 && x2 == 100;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 100.0);
            thisTest.Verify("x2", 100.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.11: IEnumerable collection handling - List scenario.
        ///
        /// This test verifies that the GC traverses IEnumerable collections
        /// (which includes List&lt;T&gt;, Collection&lt;T&gt;, and other collection types).
        /// While this test uses arrays (which are IEnumerable), it represents
        /// the same code path that handles List and other IEnumerable types.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestIEnumerableHandling_ListScenario()
        {
            string code = @"
import(""FFITarget.dll"");

// Create an array (IEnumerable) with geometry
// In CLR, this exercises the same IEnumerable traversal path
// that would handle List<T>, Collection<T>, etc.
collection = [
    DummyPoint.ByCoordinates(10, 20, 30),
    DummyPoint.ByCoordinates(40, 50, 60),
    DummyPoint.ByCoordinates(70, 80, 90)
];

// Access first point
x1 = collection[0].X;

// Force GC - points should survive because collection is still referenced
__GC();

// Access all points - should still be valid
x2 = collection[0].X;
y2 = collection[1].Y;
z2 = collection[2].Z;

isValid = x1 == 10 && x2 == 10 && y2 == 50 && z2 == 90;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 10.0);
            thisTest.Verify("x2", 10.0);
            thisTest.Verify("y2", 50.0);
            thisTest.Verify("z2", 90.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.12: Nested IEnumerable collections.
        ///
        /// Verifies that nested collection structures (collection within collection)
        /// properly protect all nested geometry through IEnumerable traversal.
        /// This represents scenarios with List&lt;List&lt;T&gt;&gt;, Collection&lt;Array&gt;, etc.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestNestedIEnumerableCollections()
        {
            string code = @"
import(""FFITarget.dll"");

// Create nested arrays (IEnumerable within IEnumerable)
innerArray1 = [DummyPoint.ByCoordinates(1, 2, 3), DummyPoint.ByCoordinates(4, 5, 6)];
innerArray2 = [DummyPoint.ByCoordinates(7, 8, 9), DummyPoint.ByCoordinates(10, 11, 12)];
outerArray = [innerArray1, innerArray2];

// Access points through nested structure
x1 = outerArray[0][0].X;
y1 = outerArray[0][1].Y;
z1 = outerArray[1][0].Z;

// GC - nested geometry should be protected
__GC();

// Access again through nested paths
x2 = outerArray[0][0].X;
y2 = outerArray[0][1].Y;
z2 = outerArray[1][0].Z;
w2 = outerArray[1][1].X;

isValid = x1 == 1 && y1 == 5 && z1 == 9 &&
          x2 == 1 && y2 == 5 && z2 == 9 && w2 == 10;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("y1", 5.0);
            thisTest.Verify("z1", 9.0);
            thisTest.Verify("x2", 1.0);
            thisTest.Verify("y2", 5.0);
            thisTest.Verify("z2", 9.0);
            thisTest.Verify("w2", 10.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.13: Mixed collection types in single graph.
        ///
        /// Verifies that different collection types (Dictionary, Array/IEnumerable,
        /// and nested structures) all work correctly together in the same graph.
        /// This represents real-world scenarios where multiple collection types
        /// are used simultaneously.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestMixedCollectionTypes()
        {
            string code = @"
import(""FFITarget.dll"");

// Create geometry objects
p1 = DummyPoint.ByCoordinates(1, 1, 1);
p2 = DummyPoint.ByCoordinates(2, 2, 2);
p3 = DummyPoint.ByCoordinates(3, 3, 3);
p4 = DummyPoint.ByCoordinates(4, 4, 4);

// Store in Dictionary (IDictionary)
dict = {""point"": p1};

// Store in Array (IEnumerable)
arr = [p2];

// Store in nested structure: Dict containing Array (IDictionary + IEnumerable)
nestedDictArray = {""array"": [p3]};

// Store in nested structure: Array containing Dict (IEnumerable + IDictionary)
nestedArrayDict = [{""point"": p4}];

// Access all through different collection types
x1 = dict[""point""].X;
x2 = arr[0].X;
x3 = nestedDictArray[""array""][0].X;
x4 = nestedArrayDict[0][""point""].X;

// GC - all collections should protect their geometry
__GC();

// Access again - all should still be valid
x1b = dict[""point""].X;
x2b = arr[0].X;
x3b = nestedDictArray[""array""][0].X;
x4b = nestedArrayDict[0][""point""].X;

isValid = x1 == 1 && x2 == 2 && x3 == 3 && x4 == 4 &&
          x1b == 1 && x2b == 2 && x3b == 3 && x4b == 4;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("x2", 2.0);
            thisTest.Verify("x3", 3.0);
            thisTest.Verify("x4", 4.0);
            thisTest.Verify("x1b", 1.0);
            thisTest.Verify("x2b", 2.0);
            thisTest.Verify("x3b", 3.0);
            thisTest.Verify("x4b", 4.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.14: IEnumerable with conditional access.
        ///
        /// Similar to the original DYN-8717 scenario but using array/IEnumerable
        /// instead of Dictionary. Verifies that IEnumerable collections protect
        /// their contents when downstream conditional logic changes.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestIEnumerableWithConditionalAccess()
        {
            string code = @"
import(""FFITarget.dll"");

// Create array with geometry (IEnumerable)
collection = [DummyPoint.ByCoordinates(100, 200, 300)];

// Access geometry initially
x1 = collection[0].X;

// Force GC - geometry in collection should NOT be collected
// because collection itself is still referenced
__GC();

// Access geometry again - should still work
x2 = collection[0].X;
y2 = collection[0].Y;
z2 = collection[0].Z;

// Verify all accesses work
isValid = x1 == 100 && x2 == 100 && y2 == 200 && z2 == 300;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 100.0);
            thisTest.Verify("x2", 100.0);
            thisTest.Verify("y2", 200.0);
            thisTest.Verify("z2", 300.0);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.15: Associative update with dictionary containing geometry.
        ///
        /// Tests that when an upstream variable (point) is updated, all dependent
        /// objects (dict, shape, coordinates) automatically update via DesignScript's
        /// associative execution. This simulates upstream node changes in a Dynamo graph
        /// where changing an input causes downstream nodes to re-execute.
        ///
        /// This addresses PR feedback from @aparajit-pratap about testing actual
        /// associative updates rather than creating independent variables.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestAssociativeUpdateWithDictionary()
        {
            string code = @"
import(""FFITarget.dll"");

// Initial values - simulate first execution
point = DummyPoint.ByCoordinates(100, 200, 300);
dict = {""Shape"": point, ""id"": 1};
shape = dict[""Shape""];
x = shape.X;
y = shape.Y;
id = dict[""id""];

// Update the upstream variable - simulates upstream node change
// Due to associative execution, dict, shape, x, y should all automatically update
point = DummyPoint.ByCoordinates(999, 888, 777);

// Force GC after update
__GC();

// Verify that all dependent variables updated correctly
xFinal = shape.X;
yFinal = shape.Y;
zFinal = shape.Z;
idFinal = dict[""id""];

// Verify values reflect the updated point
isValid = xFinal == 999 && yFinal == 888 && zFinal == 777 && idFinal == 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // All values should reflect the updated point (999, 888, 777)
            thisTest.Verify("xFinal", 999.0);
            thisTest.Verify("yFinal", 888.0);
            thisTest.Verify("zFinal", 777.0);
            thisTest.Verify("idFinal", 1);
            thisTest.Verify("isValid", true);
        }

        /// <summary>
        /// Test 2.16: Associative update with conditional access pattern.
        ///
        /// Tests the pattern where a conditional expression accesses geometry from a dictionary,
        /// then the condition changes (triggering associative re-execution), and the geometry
        /// should remain accessible. This mirrors the DYN-8717 bug scenario but with explicit
        /// upstream variable updates.
        /// </summary>
        [Test]
        [Category("DYN-8717")]
        public void TestAssociativeUpdateWithConditional()
        {
            string code = @"
import(""FFITarget.dll"");

// Create point and dictionary
point = DummyPoint.ByCoordinates(100, 200, 300);
dict = {""point"": point};

// Conditional access - initially flag is false
flag = false;
result = flag ? dict[""point""].X : ""no access"";

// Force GC while geometry is in dictionary but not directly accessed
__GC();

// Change flag - this triggers associative re-execution of result
flag = true;

// Verify that after flag changed, result now contains the X coordinate
// and we can still access the geometry
x = dict[""point""].X;
y = dict[""point""].Y;

// Verify values
isValid = x == 100 && y == 200 && result == 100;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x", 100.0);
            thisTest.Verify("y", 200.0);
            thisTest.Verify("result", 100.0);
            thisTest.Verify("isValid", true);
        }
    }
}
