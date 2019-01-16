using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class Old_Language_Manual_Examples : ProtoTestBase
    {
        string importPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        [Category("SmokeTest")]
        public void Test_4_9_count()
        {
            string errmsg = "";
            string code = @"count_test1=Count([1,2]);   // 2 .. count of collection
a = [[1,2],3];		   // define a nested/ragged collection
count_test2=Count(a);       // 2 .. count of collection
count_test3=Count(a[0]);    // 2 .. count of sub collection
count_test4=Count(a[0][0]); // 0 .. count of single member
m = a[0][0];
count_test5=Count(a[1]);    // 0 .. count of single member
count_test6=Count([]); 	   // 0 .. count of an empty collection
count_test7=Count(3); 	   // 0 .. count of single value
count_test8=Count(null);    // null .. count of null
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("count_test1", 2);
            thisTest.Verify("count_test2", 2);
            thisTest.Verify("count_test3", 2);
            thisTest.Verify("count_test4", 1);
            thisTest.Verify("count_test5", 1);
            thisTest.Verify("count_test6", 0);
            thisTest.Verify("count_test7", 1);
            thisTest.Verify("count_test8", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_10_contains()
        {
            string errmsg = "";
            string code = @"a = 5;
b = 7;
c = 9;
d = [a, b];
f = Contains(d, a); // true
g = Contains(d, c); // false
h = Contains([10,11],11); // true collection built �on the fly�
				  // with �literal� values
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("f", true);
            thisTest.Verify("g", false);
            thisTest.Verify("h", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_11_indexOf()
        {
            string errmsg = "";
            string code = @"a = 5;
b = 7;
c = 9;
d = [a, b, c];
f = IndexOf(d, b); // 1
g = d[f+1]; // c
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("f", 1);
            thisTest.Verify("g", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_13_Transpose()
        {
            string errmsg = "";
            string code = @"a =[[1,2],[3,4]];
b = a[0][0]; // b = 1
c = a [0][1]; // c = 2
a = Transpose(a); // b = 1; c =3
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 3);
        }

        [Test, Category("Failure")]
        [Category("SmokeTest")]
        public void Test_4_14_isUniformDepth()
        {
            // TODO pratapa: Failure with Rank; this works fine in a code block??
            string errmsg = "";
            string code = @"myNonUniformDepth2Dcollection = [[1, 2, 3], [4, 5], 6];
individualMemberB = myNonUniformDepth2Dcollection [0][1]; // OK, = B
individualMemberD = myNonUniformDepth2Dcollection [2][0]; // would fail
individualMemberE = myNonUniformDepth2Dcollection [2];    // OK, = 6
// Various collection manipulation functions are provided to assist with these issues, one of these functions is:
testDepthUniform = IsUniformDepth(myNonUniformDepth2Dcollection); // = false
testForDeepestDepth  = Rank(myNonUniformDepth2Dcollection); // = 2; current limitation :  1
 
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("individualMemberB", 2);
            thisTest.Verify("individualMemberD", n1);
            thisTest.Verify("individualMemberE", 6);
            thisTest.Verify("testForDeepestDepth", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_15_someNulls()
        {
            string errmsg = "";
            string code = @"a = [ 1, null, 2, 3 ];
b = Count(a); // 4 after updating a @ line 9 this value become 3.
c = SomeNulls(a); // true after updating a @ line 9 this value become false.
d = a[-2]; // d = 2 note: use of fixed index [-2] 
a = RemoveNulls(a); // {1, 2, 3}... d = 2
f = Count(d); // 2
g = SomeNulls(a); // false
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] { 1, 2, 3 });
            thisTest.Verify("b", 3);
            thisTest.Verify("c", false);
            thisTest.Verify("d", 2);
            thisTest.Verify("f", 1);
            thisTest.Verify("g", false);
        }

        [Test, Category("Failure")]
        [Category("SmokeTest")]
        public void Test_4_17_arrayAssignment()
        {
            string errmsg = "";
            string code = @"a = 0..5;
a[1] = -1; // replace a member of a collection
a[2] = a[2] + 0.5; // modify a member of a collection
a[3] = null; // make a member of a collection = null
a[4] = [ 3.4, 4.5 ]; // allowed, but not advised: subsequently altering the structure of the collection
c = a;
b = [ 0, -1, 2.5, null, [ 3.4, 4.5 ], 5 ]; // however a collection of non-uniform depth and irregular structure can be defined
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", new object[] { 0, -1, 3, null, new object[] { 3.400000, 4.500000 }, 5 });
            thisTest.Verify("b", new object[] { 0, -1, 2.500000, null, new object[] { 3.400000, 4.500000 }, 5 });
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_18_removeByIndex()
        {
            string errmsg = "1467371 Sprint28:Rev:4088: Negative index value is throwing error Index out of Range while using in Remove function.";
            string code = @"a = 1;
b = 2;
c = 3;
d = 4;
x = [ a, b, c, d ];
u = Remove(x, 0); // remove by content.. u = {b, c, d};
v = Remove(x, -1); // remove by index.. x = {a, b, c};
w = Insert(x, d, 0); // insert after defined index.. x = {d,a,b,c,d};";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
            thisTest.Verify("d", 4);
            thisTest.Verify("x", new object[] { 1, 2, 3, 4 });
            thisTest.Verify("u", new object[] { 2, 3, 4 });
            thisTest.Verify("v", new object[] { 1, 2, 3 });
            thisTest.Verify("w", new object[] { 4, 1, 2, 3, 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_20_zipped_collection()
        {
            string errmsg = "";
            string code = @"// Current limitation : 
a = [3, 4, 5];
b = [2, 6];
c = a + b ; // { 5, 10, null}; // Here the length of the resulting variable [c] will be based on the length of the first
//collection encountered [in this case a]
d = b + a; // { 5, 10}; // Here the length of the resulting variable [d] will be based on the length of the first
// collection encountered [in this case b]
// Workaround :
//def sum(a, b)
//{
  //return = a + b;
//}
//d = sum(a, b); // {5, 10}";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] { 3, 4, 5 });
            thisTest.Verify("b", new object[] { 2, 6 });
            thisTest.Verify("c", new object[] { 5, 10 });
            thisTest.Verify("c", new object[] { 5, 10 });

        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void Test_4_22_replication_guide_with_ragged_collection()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1678
            string errmsg = "MAGN-1678 Sprint 28:Rev:4088: DS throws Type conversion.. & Index out of range.... error while adding two array jagged array.";
            string code = @"// The use of replication guides with ragged collections can be unpredictable results, as follows:
a = [ 1, [ 3, 4 ] ]; // initial ragged collections
b = [ [ 5, 6 ], 7 ];
c = a + b; // c = { { 6, 7 }, { 10, 11 } }
//d = a<1> + b<2>; // unpredictable
/*
// Woraround : 'flattening' ragged collections and then applying replication give far more predictable results:
f = Flatten(a); // e = { 1, 3, 4 }
g = Flatten(b); // f = { 5, 6, 7 }
h = g + f; // h = { 6, 9, 11 }
i = f<1> + g<2>; // i = { { 6, 7, 8 }, { 8, 9, 10 }, { 9, 10, 11 } }
// Normalising the depth of collections has limited value, if the resulting sub collections are of different length
i = NormalizeDepth(a); // i = {{1},{3,4}};
j = NormalizeDepth(b); // j = {{5,6},{7}};
k = i + j; // unpredictable
l = i<1> + j<2>; // unpredictable*/";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] { 1, new object[] { 3, 4 } });
            thisTest.Verify("b", new object[] { new object[] { 5, 6 }, 7 });
            thisTest.Verify("c", new object[] { new object[] { 6, 7 }, new object[] { 10, 11 } });
        }
    }
}
