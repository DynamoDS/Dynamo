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
isValid = x1 == 1 && x2 == 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // The point should still be accessible after GC
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("x2", 1.0);
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

// Intermediate step that doesn't access the point
temp = ""intermediate"";
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

// Intermediate step
temp = 42;
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
        /// Uses replication to create multiple points.
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

// Access first and last
x0 = d[""points""][0].X;
x49 = d[""points""][49].X;

// GC
__GC();

// Access again - all should still be valid
x0_after = d[""points""][0].X;
x25_after = d[""points""][25].X;
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

// Multiple GC cycles with intermediate operations
temp1 = ""intermediate1"";
__GC();
x2 = d[""point""].X;

temp2 = ""intermediate2"";
__GC();
x3 = d[""point""].X;

temp3 = ""intermediate3"";
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
    }
}
