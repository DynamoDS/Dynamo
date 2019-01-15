using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class BuiltinMethodsTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        //Test "SomeNulls()"
        public void BIM01_SomeNulls()
        {
            String code =
@"
a = [null,20,30,null,[10,0],0,5,2];
b = [1,2,3];
e = [3,20,30,4,[null,0],0,5,2];
c = SomeNulls(a);
d = SomeNulls(b);
f = SomeNulls(e);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", true);
            thisTest.Verify("d", false);
            thisTest.Verify("f", true);
        }

        [Test]
        //Test "CountTrue()"
        public void BIM02_CountTrue()
        {
            String code =
@"a = [true,true,true,false,[true,false],true,[false,false,[true,[false],true,true,false]]];
b = [true,true,true,false,true,true];
c = [true,true,true,true,true,true,true];
w = CountTrue(a);
x = CountTrue(b);
y = CountTrue(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("w",8);
            thisTest.Verify("x",5);
            thisTest.Verify("y",7);
        }

        [Test]
        //Test "CountFalse()"
        public void BIM03_CountFalse()
        {
            String code =
@"a = [true,true,true,false,[true,false],true,[false,false,[true,[false],true,true,false]]];
b = [true,true,true,false,true,true];
c = [true,true,true,true,true,true,true];
e = CountFalse(a);
f = CountFalse(b);
g = CountFalse(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("e",6);
            thisTest.Verify("f",1);
            thisTest.Verify("g",0);
        }

        [Test]
        //Test "AllFalse() & AllTrue()"
        public void BIM04_AllFalse_AllTrue()
        {
            String code =
@"
a = [true];
b = [false,false,[false,[false,[false,false,[false],false]]],false];
c = [true,true,true,true,[true,true],true,[true,true,[true, true,[true],true,true,true]]];
d = AllTrue(a);
e = AllTrue(b);
f = AllTrue(c);
g = AllFalse(a);
h = AllFalse(b);
i = AllFalse(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", true);
            thisTest.Verify("e", false);
            thisTest.Verify("f", true);
            thisTest.Verify("g", false);
            thisTest.Verify("h", true);
            thisTest.Verify("i", false);
        }

        [Test]
        //Test "IsHomogeneous()"
        public void BIM05_IsHomogeneous()
        {
            String code =
@"a = [1,2,3,4,5];
b = [false, true, false];
c = [[1],[1.0,2.0]];
d = [null,1,2,3];
e = [];
ca = IsHomogeneous(a);
cb = IsHomogeneous(b);
cc = IsHomogeneous(c);
cd = IsHomogeneous(d);
ce = IsHomogeneous(e);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ca", true);
            thisTest.Verify("cb", true);
            thisTest.Verify("cc", true);
            thisTest.Verify("cd", false);
            thisTest.Verify("ce", true);
        }

        [Test]
        //Test "Sum() & Average()"
        public void BIM06_SumAverage()
        {
            String code =
@"
b = [1,2,[3,4,[5,[6,[7],8,[9,10],11]],12,13,14,[15]],16];
c = [1.2,2.2,[3.2,4.2,[5.2,[6.2,[7.2],8.2,[9.2,10.2],11.2]],12.2,13.2,14.2,[15.2]],16.2];
x = Average(b);
y = Sum(b);
z = Average(c);
s = Sum(c);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 8.5);
            thisTest.Verify("y", 136);
            thisTest.Verify("z", 8.7);
            thisTest.Verify("s", 139.2);

        }

        [Test]
        //Test "SomeTrue() & SomeFalse()"
        public void BIM07_SomeTrue_SomeFalse()
        {
            String code =
@"a = [true,true,true,[false,false,[true, true,[false],true,true,false]]];
b = [true,true,[true,true,true,[true,[true],true],true],true];
c = [true, false, false];
p = SomeTrue(a);
q = SomeTrue(b);
r = SomeTrue(c);
s = SomeFalse(a);
t = SomeFalse(b);
u = SomeFalse(c);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("p", true);
            thisTest.Verify("q", true);
            thisTest.Verify("r", true);
            thisTest.Verify("s", true);
            thisTest.Verify("t", false);
            thisTest.Verify("u", true);
        }

        [Test]
        //Test "Remove() & RemoveDuplicate()"
        public void BIM08_Remove_RemoveDuplicate()
        {
            String code =
@"
a = [null,20,30,null,20,15,true,true,5,false];
b = [1,2,3,4,9,4,2,5,6,7,8,7,1,0,2];
rda = RemoveDuplicates(a);
rdb = RemoveDuplicates(b);
ra = Remove(a,3);
rb = Remove(b,2);
p = rda[3];
q = rdb[4];
x = ra[3];
y = rb[2];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("p",15);
            thisTest.Verify("q",9);
            thisTest.Verify("x",20);
            thisTest.Verify("y",4);
        }

        [Test]
        //Test "RemoveNulls()"
        public void BIM09_RemoveNulls()
        {
            String code =
@"a = [1,[6,null,7,[null,null]],7,null,2];
b = [null,[null,[null,[null],null],null],null];
p = RemoveNulls(a);
q = RemoveNulls(b);
x = p[3];
y = p[1][1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x",2);
            thisTest.Verify("y",7);
        }

        [Test]
        //Test "RemoveIfNot()"
        public void BIM10_RemoveIfNot()
        {
            String code =
@"import(""BuiltIn.ds"");
a = [""This is "",""a very complex "",""array"",1,2.0,3,false,4.0,5,6.0,true,[2,3.1415926],null,false,'c'];
b = List.RemoveIfNot(a, ""int"");
c = List.RemoveIfNot(a, ""double"");
d = List.RemoveIfNot(a, ""bool"");
e = List.RemoveIfNot(a, ""array"");
q = b[0];
r = c[0];
s = d[0];
t = e[0][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("q", 1);
            thisTest.Verify("r", 2.0);
            thisTest.Verify("s", false);
            thisTest.Verify("t", 2);
        }

        [Test]
        //Test "Reverse()"
        public void BIM11_Reverse()
        {
            String code =
@"a = [1,[[1],[3.1415]],null,1.0,12.3];
b = [1,2,[3]];
p = Reverse(a);
q = Reverse(b);
x = p[0];
y = q[0][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x",12.3000);
            thisTest.Verify("y",3);
        }

        [Test]
        //Test "Contains()"
        public void BIM12_Contains()
        {
            String code =
@"a = [1,[[1],[3.1415]],null,1.0,12.3];
b = [1,2,[3]];
x = [[1],[3.1415]];
r = Contains(a, 3.0);
s = Contains(a, x);
t = Contains(a, null);
u = Contains(b, b);
v = Contains(b, [3]);
w = Contains(b, 3);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", false);
            thisTest.Verify("s", true);
            thisTest.Verify("t", true);
            thisTest.Verify("u", true);
            thisTest.Verify("v", true);
            thisTest.Verify("w", true);
        }


        [Test]
        //Test "IndexOf()"
        public void BIM13_IndexOf()
        {
            String code =
@"a = [1,[[1],[3.1415]],null,1.0,12,3];
b = [1,2,[3]];
c = [1,2,[3]];
d = [[1],[3.1415]];
r = IndexOf(a, d);
s = IndexOf(a, 1);
t = IndexOf(a, null);
u = IndexOf(b, [3]);
v = IndexOf(b, 3);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r",1);
            thisTest.Verify("s",0);
            thisTest.Verify("t",2);
            thisTest.Verify("u",2);
            thisTest.Verify("v",-1);
        }

        [Test]
        //Test "Sort()"
        public void BIM14_Sort()
        {
            String code =
@"a = [1,3,5,7,9,8,6,4,2,0];
b = [1.3,2,0.8,2,null,2,2.0,2,null];
x = Sort(a);
x1 = Sort(a,true);
x2 = Sort(a,false);
y = Sort(b);
p = x[0];
p1 = x1[0];
p2 = x2[0];
q = x[9];
s = y[0];
t = y[7];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p", 0);
            thisTest.Verify("p1", 0);
            thisTest.Verify("p2", 9);
            thisTest.Verify("q", 9);
            thisTest.Verify("s", null);
            thisTest.Verify("t", 2.0);
        }

        [Test]
        //Test "SortIndexByValue()"
        public void BIM15_SortIndexByValue()
        {
            String code =
@"a = [1,3,5,7,9,8,6,4,2,0];
b = [1.3,2,0.8,2,null,2,2.0,2,null];
x = SortIndexByValue(a);
x1 = SortIndexByValue(a,true);
x2 = SortIndexByValue(a,false);
y = SortIndexByValue(b);
p = x[0];
p1 = x1[0];
p2 = x2[0];
q = x[9];
s = y[0];
t = y[7];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p", 9);
            thisTest.Verify("p1", 9);
            thisTest.Verify("p2", 4);
            thisTest.Verify("q", 4);
            // thisTest.Verify("s", 4); s could be 4 or 8 (for null value)
            // thisTest.Verify("t", 3); t could be multiple choices (for 2).
        }

        [Test]
        //Test "Insert()"
        public void BIM16_Insert()
        {
            String code =
@"a = [false,2,3.1415926,null,[false]];
b = 1;
c = [1];
d = [];
e = [[1],2,3.0];
p = Insert(a,b,1);
q = Insert(a,c,1);
r = Insert(a,d,0);
s = Insert(a,e,5);
u = p[1];
v = q[1][0];
w = r[1];
x = s[5][0][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("u", 1);
            thisTest.Verify("v", 1);
            thisTest.Verify("w", false);
            thisTest.Verify("x", 1);
        }

        [Test]
        //Test "SetDifference(), SetUnion() & SetIntersection()"
        public void BIM17_SetDifference_SetUnion_SetIntersection()
        {
            String code =
@"a = [false,15,6.0,15,false,null,15.0];
b = [10,20,false,12,21,6.0,15,null,8.2];
c = SetDifference(a,b);
d = SetDifference(b,a);
e = SetIntersection(a,b);
f = SetUnion(a,b);
p = c[0];
q = d[1];
r = e[1];
s = f[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p",15.0000);
            thisTest.Verify("q",20);
            thisTest.Verify("r",15);
            thisTest.Verify("s",15);
        }

        [Test]
        //Test "Reorder"
        public void BIM18_Reorder()
        {
            String code =
@"a = [1,4,3,8.0,2.0,0];
b = [2,1,0,3,4];
c = Reorder(a,b);
p = c[0];
q = c[1];
r = c[2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p",3);
            thisTest.Verify("q",4);
            thisTest.Verify("r",1);
        }

        [Test]
        public void Reorder2()
        {
            String code =
@"
x = [1, 2, 3];
y = [-1];
z = Reorder(x,y);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", null);
        }

        [Test]
        //Test "IsUniformDepth"
        public void BIM19_IsUniformDepth()
        {
            String code =
@"a = [];
b = [1,2,3];
c = [[1],[2,3]];
d = [1,[2],[[3]]];
p = IsUniformDepth(a);
q = IsUniformDepth(b);
r = IsUniformDepth(c);
s = IsUniformDepth(d);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p", true);
            thisTest.Verify("q", true);
            thisTest.Verify("r", true);
            thisTest.Verify("s", false);
        }

        [Test]
        //Test "NormailizeDepth"
        public void BIM20_NormalizeDepth()
        {
            String code =
@"a = [[1,[2,3,4,[5]]]];
p = NormalizeDepth(a,1);
q = NormalizeDepth(a,2);
r = NormalizeDepth(a,4);
s = NormalizeDepth(a);
w = p[0];
x = q[0][0];
y = r[0][0][0][0];
z = s[0][0][0][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("w",1);
            thisTest.Verify("x",1);
            thisTest.Verify("y",1);
            thisTest.Verify("z",1);
        }

        [Test]
        //Test "Map & MapTo"
        public void BIM21_Map_MapTo()
        {
            String code =
@"a = Map(80.0, 120.0, 100.0);
b = MapTo(0.0, 100.0 ,25.0, 80.0, 90.0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a",0.5000);
            thisTest.Verify("b",82.5000);
        }

        [Test]
        //Test "Transpose"
        public void BIM22_Transpose()
        {
            String code =
@"a = [[1,2,3],[1,2],[1,2,3,4,5,6,7]];
p = Transpose(a);
q = Transpose(p);
x = p[6][0];
y = q[2][6];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
            thisTest.Verify("y", 7);
        }

        [Test]
        public void TransposeEmpty2DArray()
        {
            string code = @"x = [[]]; y = Transpose(x);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { });
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV()
        {
            String code =
@"a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set1/test1.csv"";
b = ImportFromCSV(a);
c = ImportFromCSV(a, false);
d = ImportFromCSV(a, true);
x = b[0][2];
y = c[0][2];
z = d[0][2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3.0);
            thisTest.Verify("y", 3.0);
            thisTest.Verify("z", 3.0);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM24_LoadCSV()
        {
            // ensure that white space is trimmed from the path
            String code =
@"a = ""\n \r\t../../../test/Engine/ProtoTest/ImportFiles/CSV/Set1/test1.csv\r\r\n "";
b = ImportFromCSV(a);
x = b[0][2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3.0);
        }

        [Test]
        //Test "Count"
        public void BIM24_Count()
        {
            String code =
@"a = [1, 2, 3, 4];
b = [ [ 1, [ 2, 3, 4, [ 5 ] ] ] ];
c = [ [ 2, null ], 1, ""str"", [ 2, [ 3, 4 ] ] ];
x = Count(a);
y = Count(b);
z = Count(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 4);
            thisTest.Verify("y", 1);
            thisTest.Verify("z", 4);
        }

        [Test]
        //Test "Rank"
        public void BIM25_Rank()
        {
            String code =
@"
import(""BuiltIn.ds"");
a = [ [ 1 ], 2, 3, 4 ];
b = [ ""good"", [ [ null ] ], [ 1, [ 2, 3, 4, [ 5, [ ""good"" ], [ null ] ] ] ] ];
c = [ [ null ], [ 2, ""good"" ], 1, null, [ 2, [ 3, 4 ] ] ];
x = List.Rank(a);
y = List.Rank(b);
z = List.Rank(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2);
            thisTest.Verify("y", 5);
            thisTest.Verify("z", 3);
        }

        [Test]
        //Test "Flatten"
        public void BIM26_Flatten()
        {
            String code =
@"a = [1, 2, 3, 4];
b = [ ""good"", [ 1, [ 2, 3, 4, [ 5 ] ] ] ];
c = [ null, [ 2, ""good""], 1, null, [ 2, [ 3, 4 ] ] ];
q = Flatten(a);
p = Flatten(b);
r = Flatten(c);
x = q[0];
y = p[2];
z = r[4];
s = p[0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
            thisTest.Verify("z", null);
            thisTest.Verify("s", "good");
        }

        [Test]
        //Test "CountTrue/CountFalse/Average/Sum/RemoveDuplicate"
        public void BIM27_Conversion_Resolution_Cases()
        {
            String code =
@"a = [null,20,30,null,[10,0],true,[false,0,[true,[false],5,2,false]]];
b = [1,2,[3,4,9],4,2,5,[6,7,[8]],7,1,0,2];
x = CountTrue(a);
y = CountFalse(a);
z = AllTrue(a);
w = AllFalse(a);
p = SomeTrue(a);
q = SomeTrue(a);
r = Sum(true);
s = Sum(null);
t = RemoveDuplicates(b);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Theses are invalid cases not following the parameter requirements
            //But need error messages when executing.
        }

        [Test]
        //Test "IsRectangular"
        public void BIM28_IsRectangular()
        {
            String code =
@"a = [[1,[2,3]],[4, 5, 6]];
b= [[1, 2, 3, 4], [5, 6, 7, 8]];
c= [];
x = IsRectangular(a);
y = IsRectangular(b);
z = IsRectangular(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", false);
            thisTest.Verify("y", true);
            thisTest.Verify("z", false);
        }

        [Test]
        public void BIM29_RemoveIfNot()
        {
            string code = @"
import(""BuiltIn.ds"");
a = [ true,null,false,true];
b = List.RemoveIfNot(a, ""bool""); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void BIM30_RemoveDuplicate()
        {
            String code =
@"
import(""FFITarget.dll"");
a = [null,20,30,null,20,15,true,true,5,false];
b = [1,2,3,4,9,4,2,5,6,7,8,7,1,0,2];
rda = RemoveDuplicates(a);
rdb = RemoveDuplicates(b);
rdd = RemoveDuplicates([[1,2,[5,[6]]], [1,2,[5,6]], [1,2,[5,[6]]]]);
rde = RemoveDuplicates([""hello2"", ""hello"", 'r', ""hello2"", 's', 's', ""hello"", ' ']);
rdf = RemoveDuplicates([]);
p = rda[3];
q = rdb[4];
m2 = rdd[0][2][0];
m3 = rde[4];
res1 = Count(rda);
res2 = Count(rdb);
res4 = Count(rdd);
res5 = Count(rde);
res6 = Count(rdf);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p",15);
            thisTest.Verify("q",9);
            thisTest.Verify("m2", 5);
            thisTest.Verify("m3", ' ');
            thisTest.Verify("res1", 7);
            thisTest.Verify("res2", 10);
            thisTest.Verify("res4", 2);
            thisTest.Verify("res5", 5);
            thisTest.Verify("res6", 0);
        }

        [Test]
        //Test "Count"
        public void BIM31_Sort()
        {
            String code =
@"a = [ 3, 1, 2 ];
def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
sort = Sort(sorterFunction, a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        //Test "Count"
        public void BIM31_Sort_null()
        {
            String code =
@"c = [ 3, 1, 2,null ];
def sorterFunction(a : int, b : int)
{
    return = [Imperative]
    {
        if (a == null)
            return = -1;
        if (b == null)
            return = 1;
        return = a > b ? 10 : -10;
    }
}
sort = Sort(sorterFunction, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { null, 1, 2, 3 });
        }

        [Test]
        //Test "Count"
        public void BIM31_Sort_duplicate()
        {
            String code =
@"c = [ 3, 1, 2, 2,null ];
def sorterFunction(a : int, b : int)
{
    return = [Imperative]
    {
        if (a == null)
            return = -1;
        if (b == null)
            return = 1;
        return = a - b;
    }
}
sort = Sort(sorterFunction, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { null, 1, 2, 2, 3 });
        }

        [Test]
        public void Test_EvaluateFunctionPointer01()
        {
            string code =
@"
def foo(x, y, z)
{
    return = x + y + z;
}

param = [ 2, 3, 4 ];
x = Evaluate(foo, param, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 9);
        }

        [Test]
        public void Test_EvaluateFunctionPointer02()
        {
            string code =
@"
def foo(x, y, z)
{
    return = x + y + z;
}

def foo(x, y)
{
    return = x * y;
}

param = [ 2, 3, 4 ];
x = Evaluate(foo, param, true);
param = [ 5, 6 ];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 30);
        }

        [Test]
        public void Test_EvaluateFunctionPointer03()
        {
            string code =
@"
def foo(x, y, z)
{
    return = x + y + z;
}

def bar(x, y, z)
{
    return = x * y * z;
}

t = foo;
param = [ 2, 3, 4 ];
x = Evaluate(t, param, true);
t = bar;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 24);
        }

        [Test]
        public void Test_EvaluateFunctionPointer04()
        {
            // replicated on function pointer
            string code =
@"
def foo(x, y, z)
{
    return = x + y + z;
}

def bar(x, y, z)
{
    return = x * y * z;
}

param = [2, 3, 4 ];
x = Evaluate([ foo, bar ], param, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 9, 24 });
        }

        [Test]
        public void Test_EvaluateFunctionPointer05()
        {
            // replicated on function pointer
            string code =
@"
def foo(x, y, z)
{
    return = x + y + z;
}

param = [[2, 3, 4], [5,6,7], [8, 9, 10] ];
// e.q. 
// foo({2,3,4}, {5,6,7}, {8, 9, 10});
x = Evaluate(foo, param, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 15, 18, 21 });
        }

        [Test]
        public void Test_EvaluateFunctionPointer06()
        {
            // replicated on function pointer
            string code =
@"
def bar(x, y)
{
    return = x * y;
}

def foo(f : function, x, y)
{
    return = f(x, y);
}

x = Evaluate(foo, [ bar, 2, 3 ], true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 6);
        }

        [Test]
        public void Test_EvaluateFunctionPointer07()
        {
            // Nested call
            string code =
@"
def multiplyBy2(z)
{
    return = 2 * z;
}

def bar(y, z)
{
    return = y * Evaluate(multiplyBy2, [ z ], true);
}

def foo(x, y, z)
{
    return = x + Evaluate(bar, [ y, z ], true);
}

x = Evaluate(foo, [ 2, 3, 4 ], true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 26);
        }

        [Test]
        public void Test_EvaluateFunctionPointer08()
        {
            // Nested call
            string code =
@"
def f1(x)
{
    return = 2 * x;
}

def f2(x)
{
    return = 3 * x;
}

def foo(evalFunction : function, fptr : function, param : var[])
{
    return = evalFunction(fptr, param, true);
}

x = foo([ Evaluate, Evaluate ], [ f1, f2 ], [ [ 41 ], [ 42 ] ]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 82, 126 });
        }

        [Test]
        public void Test_EvaluateFunctionPointer10()
        {
            // Recursive call
            string code =
@"
def foo(x)
{
    Print(x);
    return = [Imperative]
    {
        if (x == 0)
        {
            return=1;
        }
        else
        {
            return = x * Evaluate(foo, [ x - 1 ], true);
        }
    }
}

x = foo(5);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 120);

            // This case crashes nunit 
            //Assert.Fail("This test case crashes Nunit");
        }

       [Test]
       public void TestTryGetValueFromNestedDictionaries01()
       {
           string code = @"
a = { ""in"" : 42 };
r = Dictionary.ValueAtKey(a, ""in"");
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r", 42);
       }

       [Test]
       public void TestTryGetValuesFromDictionary02()
       {
           string code = @"
a = { ""in"" : 42 };
key = ""in"";
r = Dictionary.ValueAtKey(a, key);
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r", 42);
       }

       [Test]
       public void TestTryGetValuesFromDictionary03()
       {
           string code = @"
a = {""in"": 42, ""out"" : 24};

b = { ""in"": 24, ""out"" : 42};

c = [ a, b];

r1 = Dictionary.ValueAtKey(c, ""in"");
r2 = Dictionary.ValueAtKey(c, ""out"");
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r1", new object[] { 42, 24 });
           thisTest.Verify("r2", new object[] { 24, 42 });
       }

       [Test]
       public void TestTryGetValuesFromDictionary04()
       {
           string code = @"
a = { ""in"" : 42, ""out"" : 24 };
b = { ""in"" : 24, ""out"" : 42 };
c = [[a], [b]];
r1 = Dictionary.ValueAtKey(c, ""in"");
r2 = Dictionary.ValueAtKey(c, ""out"");
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r1", new object[] { new object[] { 42 }, new object[] { 24 } });
           thisTest.Verify("r2", new object[] { new object[] { 24 }, new object[] { 42 } });
       }

       [Test]
       public void TestTryGetValuesFromDictionary05()
       {
           string code = @"
a = { ""in"" : 42, ""out"" : 24 };
b = { ""in"" : 24, ""out"" : 42 };
c = [a, [b]];
r1 = Dictionary.ValueAtKey(c, ""in"");
r2 = Dictionary.ValueAtKey(c, ""out"");
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r1", new object[] { 42, new object[] { 24 } });
           thisTest.Verify("r2", new object[] { 24, new object[] { 42 } });
       }

       [Test]
       public void TestTryGetValuesFromDictionary06()
       {
           string code = @"
a = { ""in"" : 42, ""out"" : 24 };
b = { ""in"" : 24, ""out"" : 42 };
c = [a, [b]];
r = Dictionary.ValueAtKey(c, ""nonexist"");
";
           var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { null, new object[] { null } });
       }

       [Test]
       public void TestTryGetValuesFromDictionary07()
       {
           string code = @"
a = { ""in"" : 42, ""out"" : 24 };
r = Dictionary.ValueAtKey(a, ""nonexist"");
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r", null);
       }

       [Test]
       public void TestTryGetValuesFromDictionary08()
       {
           string code = @"
a = 42;
r = Dictionary.ValueAtKey(a, ""nonexist"");
";
           var mirror = thisTest.RunScriptSource(code);
           thisTest.Verify("r", null);
       }

        [Test]
        public void TestTryGetValuesFromDictionary09()
        {
            string code = @"
a = { ""in"" : 42, ""out"" : 24 };
b = { ""in"" : 24, ""out"" : 42 };
c = [[a],[[b]]];
r1 = Dictionary.ValueAtKey(c, ""in"");
r2 = Dictionary.ValueAtKey(c, ""out"");
";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { 42 } , new object[] { new object[] { 24 } } });
            thisTest.Verify("r2", new object[] { new object[] { 24 } , new object[] { new object[] { 42 } } });
        }

        [Test]
        public void TestTryGetValuesFromDictionary10()
        {
            string code = @"
a = { ""in"" : 42, ""out"" : 24 };
b = { ""in"" : 24, ""out"" : 42 };
c = [[[a]],b];
r1 = Dictionary.ValueAtKey(c, ""in"");
r2 = Dictionary.ValueAtKey(c, ""out"");
";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { new object[] { 42 } }, 24 });
            thisTest.Verify("r2", new object[] { new object[] { new object[] { 24 } }, 42 });
        }

        [Test]
        public void TestTryGetValuesFromDictionary11()
        {
            string code = @"
a = [ ""in"" , 42, ""out"" , 24 ];
b = { ""in"" : 24, ""out"" : 42 };
c = [a,[b]];
r1 = Dictionary.ValueAtKey(c, ""in"");
r2 = Dictionary.ValueAtKey(c, ""out"");
";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { null, null, null, null }, new object[] { 24 } });
            thisTest.Verify("r2", new object[] { new object[] { null, null, null, null }, new object[] { 42 } });
        }
      
        [Test]
        public void TestGetKeysFromNonArray()
        {
            string code = @"
x = 1;
k = GetKeys(x);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("k", null);
        }
    }
}
