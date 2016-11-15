using System;
using NUnit.Framework;
using ProtoTestFx;

namespace ProtoTest.DebugTests
{
    [TestFixture]
    public class RunEqualityTests
    {

        [Test]
        [Category("Debugger")]
        public void DebugEQTestEqualityR1()
        {
            String src =
                @" a = 4;";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("Debugger")]
        public void DebugEQTestEqualityImpR1()
        {
            String src =
                @" a = 0; [Imperative] {
a = 4;
}
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("Debugger")]
        public void DebugEQTestEquality0001()
        {
            String src =
                @"[Imperative]
{
	f0 = 5 > 6;
    f1 = (5 > 6);
    f2 = 5 >= 6;
    f3 = (5 >= 6);    
    t0 = 5 == 5;
    t1 = (5 == 5);
    t2 = 5 < 6;
    t3 = (5 < 6);
    
    t4 = 5 <= 6;
    t5 = (5 <= 6);
    t6 = 5 <= 5;
    t7 = (5 <= 5);
}
";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        public void DebugEQBIM01_SomeNulls()
        {
            String code =
                @"
a = {null,20,30,null,{10,0},0,5,2};
b = {1,2,3};
e = {3,20,30,4,{null,0},0,5,2};
c = SomeNulls(a);
d = SomeNulls(b);
f = SomeNulls(e);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQBIM02_CountTrue()
        {
            String code =
                @"a = {true,true,true,false,{true,false},true,{false,false,{true,{false},true,true,false}}};
b = {true,true,true,false,true,true};
c = {true,true,true,true,true,true,true};
w = CountTrue(a);
x = CountTrue(b);
y = CountTrue(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQBIM03_CountFalse()
        {
            String code =
                @"a = {true,true,true,false,{true,false},true,{false,false,{true,{false},true,true,false}}};
b = {true,true,true,false,true,true};
c = {true,true,true,true,true,true,true};
e = CountFalse(a);
f = CountFalse(b);
g = CountFalse(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "AllFalse() & AllTrue()"
        public void DebugEQBIM04_AllFalse_AllTrue()
        {
            String code =
                @"
a = {true};
b = {false,false,{false,{false,{false,false,{false},false}}},false};
c = {true,true,true,true,{true,true},true,{true,true,{true, true,{true},true,true,true}}};
d = AllTrue(a);
e = AllTrue(b);
f = AllTrue(c);
g = AllFalse(a);
h = AllFalse(b);
i = AllFalse(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "IsHomogeneous()"
        public void DebugEQBIM05_IsHomogeneous()
        {
            String code =
                @"a = {1,2,3,4,5};
b = {false, true, false};
c = {{1},{1.0,2.0}};
d = {null,1,2,3};
e = {};
ca = IsHomogeneous(a);
cb = IsHomogeneous(b);
cc = IsHomogeneous(c);
cd = IsHomogeneous(d);
ce = IsHomogeneous(e);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Sum() & Average()"
        public void DebugEQBIM06_SumAverage()
        {
            String code =
                @"
b = {1,2,{3,4,{5,{6,{7},8,{9,10},11}},12,13,14,{15}},16};
c = {1.2,2.2,{3.2,4.2,{5.2,{6.2,{7.2},8.2,{9.2,10.2},11.2}},12.2,13.2,14.2,{15.2}},16.2};
x = Average(b);
y = Sum(b);
z = Average(c);
s = Sum(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "SomeTrue() & SomeFalse()"
        public void DebugEQBIM07_SomeTrue_SomeFalse()
        {
            String code =
                @"a = {true,true,true,{false,false,{true, true,{false},true,true,false}}};
b = {true,true,{true,true,true,{true,{true},true},true},true};
c = {true, false, false};
p = SomeTrue(a);
q = SomeTrue(b);
r = SomeTrue(c);
s = SomeFalse(a);
t = SomeFalse(b);
u = SomeFalse(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Remove() & RemoveDuplicate()"
        public void DebugEQBIM08_Remove_RemoveDuplicate()
        {
            String code =
                @"a = {null,20,30,null,20,15,true,true,5,false};
b = {1,2,3,4,9,4,2,5,6,7,8,7,1,0,2};
rda = RemoveDuplicates(a);
rdb = RemoveDuplicates(b);
ra = Remove(a,3);
rb = Remove(b,2);
p = rda[3];
q = rdb[4];
x = ra[3];
y = rb[2];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "RemoveNulls()"
        public void DebugEQBIM09_RemoveNulls()
        {
            String code =
                @"a = {1,{6,null,7,{null,null}},7,null,2};
b = {null,{null,{null,{null},null},null},null};
p = RemoveNulls(a);
q = RemoveNulls(b);
x = p[3];
y = p[1][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "RemoveIfNot()"
        public void DebugEQBIM10_RemoveIfNot()
        {
            String code =
                @"a = {""This is "",""a very complex "",""array"",1,2.0,3,false,4.0,5,6.0,true,{2,3.1415926},null,false,'c'};
b = RemoveIfNot(a, ""int"");
c = RemoveIfNot(a, ""double"");
d = RemoveIfNot(a, ""bool"");
e = RemoveIfNot(a, ""array"");
q = b[0];
r = c[0];
s = d[0];
t = e[0][0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Reverse()"
        public void DebugEQBIM11_Reverse()
        {
            String code =
                @"a = {1,{{1},{3.1415}},null,1.0,12.3};
b = {1,2,{3}};
p = Reverse(a);
q = Reverse(b);
x = p[0];
y = q[0][0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Contains()"
        public void DebugEQBIM12_Contains()
        {
            String code =
                @"a = {1,{{1},{3.1415}},null,1.0,12.3};
b = {1,2,{3}};
x = {{1},{3.1415}};
r = Contains(a, 3.0);
s = Contains(a, x);
t = Contains(a, null);
u = Contains(b, b);
v = Contains(b, {3});
w = Contains(b, 3);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "IndexOf()"
        public void DebugEQBIM13_IndexOf()
        {
            String code =
                @"a = {1,{{1},{3.1415}},null,1.0,12,3};
b = {1,2,{3}};
c = {1,2,{3}};
d = {{1},{3.1415}};
r = IndexOf(a, d);
s = IndexOf(a, 1);
t = IndexOf(a, null);
u = IndexOf(b, {3});
v = IndexOf(b, 3);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Sort()"
        public void DebugEQBIM14_Sort()
        {
            String code =
                @"a = {1,3,5,7,9,8,6,4,2,0};
b = {1.3,2,0.8,2,null,2,2.0,2,null};
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
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "SortIndexByValue()"
        public void DebugEQBIM15_SortIndexByValue()
        {
            String code =
                @"a = {1,3,5,7,9,8,6,4,2,0};
b = {1.3,2,0.8,2,null,2,2.0,2,null};
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
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Insert()"
        public void DebugEQBIM16_Insert()
        {
            String code =
                @"a = {false,2,3.1415926,null,{false}};
b = 1;
c = {1};
d = {};
e = {{1},2,3.0};
p = Insert(a,b,1);
q = Insert(a,c,1);
r = Insert(a,d,0);
s = Insert(a,e,5);
u = p[1];
v = q[1][0];
w = r[1][0];
x = s[5][0][0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "SetDifference(), SetUnion() & SetIntersection()"
        public void DebugEQBIM17_SetDifference_SetUnion_SetIntersection()
        {
            String code =
                @"a = {false,15,6.0,15,false,null,15.0};
b = {10,20,false,12,21,6.0,15,null,8.2};
c = SetDifference(a,b);
d = SetDifference(b,a);
e = SetIntersection(a,b);
f = SetUnion(a,b);
p = c[0];
q = d[1];
r = e[1];
s = f[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Reorder"
        public void DebugEQBIM18_Reorder()
        {
            String code =
                @"a = {1,4,3,8.0,2.0,0};
b = {2,1,0,3,4};
c = Reorder(a,b);
p = c[0];
q = c[1];
r = c[2];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "IsUniformDepth"
        public void DebugEQBIM19_IsUniformDepth()
        {
            String code =
                @"a = {};
b = {1,2,3};
c = {{1},{2,3}};
d = {1,{2},{{3}}};
p = IsUniformDepth(a);
q = IsUniformDepth(b);
r = IsUniformDepth(c);
s = IsUniformDepth(d);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "NormailizeDepth"
        public void DebugEQBIM20_NormalizeDepth()
        {
            String code =
                @"a = {{1,{2,3,4,{5}}}};
p = NormalizeDepth(a,1);
q = NormalizeDepth(a,2);
r = NormalizeDepth(a,4);
s = NormalizeDepth(a);
w = p[0];
x = q[0][0];
y = r[0][0][0][0];
z = s[0][0][0][0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Map & MapTo"
        public void DebugEQBIM21_Map_MapTo()
        {
            String code =
                @"a = Map(80.0, 120.0, 100.0);
b = MapTo(0.0, 100.0 ,25.0, 80.0, 90.0);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Transpose"
        public void DebugEQBIM22_Transpose()
        {
            String code =
                @"a = {{1,2,3},{1,2},{1,2,3,4,5,6,7}};
p = Transpose(a);
q = Transpose(p);
x = p[6][0];
y = q[0][6];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "LoadCSV"
        public void DebugEQBIM23_LoadCSV()
        {
            String code =
                @"a = ""CSVTestCase/test1.csv"";
b = LoadCSV(a);
c = LoadCSV(a, false);
d = LoadCSV(a, true);
x = b[0][2];
y = c[0][2];
z = d[0][2];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Count"
        public void DebugEQBIM24_Count()
        {
            String code =
                @"a = {1, 2, 3, 4};
b = { { 1, { 2, 3, 4, { 5 } } } };
c = { { 2, null }, 1, ""str"", { 2, { 3, 4 } } };
x = Count(a);
y = Count(b);
z = Count(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Rank"
        public void DebugEQBIM25_Rank()
        {
            String code =
                @"a = { { 1 }, 2, 3, 4 };
b = { ""good"", { { null } }, { 1, { 2, 3, 4, { 5, { ""good"" }, { null } } } } };
c = { { null }, { 2, ""good"" }, 1, null, { 2, { 3, 4 } } };
x = Rank(a);
y = Rank(b);
z = Rank(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "Flatten"
        public void DebugEQBIM26_Flatten()
        {
            String code =
                @"a = {1, 2, 3, 4};
b = { ""good"", { 1, { 2, 3, 4, { 5 } } } };
c = { null, { 2, ""good""}, 1, null, { 2, { 3, 4 } } };
q = Flatten(a);
p = Flatten(b);
r = Flatten(c);
x = q[0];
y = p[2];
z = r[4];
s = p[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        //Test "CountTrue/CountFalse/Average/Sum/RemoveDuplicate"
        public void DebugEQBIM27_Conversion_Resolution_Cases()
        {
            String code =
                @"a = {null,20,30,null,{10,0},true,{false,0,{true,{false},5,2,false}}};
b = {1,2,{3,4,9},4,2,5,{6,7,{8}},7,1,0,2};
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
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestMethodWithArrayInput2()
        {
            string code =
                @"
                            class A
                            {
                            }
                            class B extends A
                            {
                            }
                            def Test(arr : A[])
                            {
                                    return = 123;
                            }
                            a = {B.B(), A.A(), B.B()};
                            val = Test(a);
                            ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestAssignment01_002()
        {
            String code =
                @"[Associative]
{
	foo = 5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestNull01_002()
        {
            String code =
                @"
[Associative]
{
	x = null;
    y = x;
    a = null;
    b = a + 2;
    c = 2 + a * x;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestNull02_002()
        {
            String code =
                @"
[Associative]
{
    def foo : int ( a : int )
    {
        b = a + 1;
    }
	 
    c = foo(1);	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestFunctions01()
        {
            String code =
                @"[Associative]
{
    def mult : int( s : int )	
	{
		return = s * 2;
	}
    test = mult(5);
    test2 = mult(2);
    test3 = mult(mult(5));
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestFunctions02()
        {
            String code =
                @"        
[Associative]
{ 
    def test2 : int(b : int)
    {
        return = b;
    }
                
    def test : int(a : int)
    {
        return = a + test2(5);
    }
               
    temp = test(2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestFunctionsOverload01()
        {
            String code =
                @"[Associative]
{
    def m1 : int( s : int )	
	{
		return = s * 2;
	}
    def m1 : int( s: int, y : int )
    {
        return = s * y;
    }
    test1 = m1(5);
    test2 = m1(5, 10);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestFunctionsOverload02()
        {
            String code =
                @"[Associative]
{
    def f : int( p1 : int )
    {
	    x = p1 * 10;
	    return = x;
    }
    def f : int( p1 : int, p2 : int )
    {
	    return = p1 + p2;
    }
    a = 2;
    b = 20;
    // Pasing variables to function overloads
    i = f(a + 10);
    j = f(a, b);
}   
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArray001()
        {
            String code =
                @"[Associative]
{
	a = {1001,1002};
    x = a[0];
    y = a[1];
    a[0] = 23;
}";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArray002()
        {
            String code =
                @"
[Associative]
{ 
    def foo : int (a : int[])
    {           
        return = a[0];
    }
            
    arr = {100, 200};            
    b = foo(arr);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArray003()
        {
            String code =
                @"
a = {0,1,2};
t = {10,11,12};
a[0] = t[0];
t[1] = a[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArrayOverIndexing01_002()
        {
            string code =
                @"
[Imperative]
{
    arr1 = {true, false};
    arr2 = {1, 2, 3};
    arr3 = {false, true};
    t = arr2[1][0];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray001_002()
        {
            String code =
                @"
a = {10,20};
a[2] = 100;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray002()
        {
            String code =
                @"
t = {};
t[0] = 100;
t[1] = 200;
t[2] = 300;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray003()
        {
            String code =
                @"
t = {};
t[0][0] = 1;
t[0][1] = 2;
a = t[0][0];
b = t[0][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray004()
        {
            String code =
                @"
t = {};
t[0][0] = 1;
t[0][1] = 2;
t[1][0] = 10;
t[1][1] = 20;
a = t[1][0];
b = t[1][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray005()
        {
            String code =
                @"
t = {0,{20,30}};
t[1][1] = {40,50};
a = t[1][1][0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray006()
        {
            String code =
                @"
[Imperative]
{
    t = {};
    t[0][0] = 1;
    t[0][1] = 2;
    t[1][0] = 3;
    t[1][1] = 4;
    a = t[0][0];
    b = t[0][1];
    c = t[1][0];
    d = t[1][1];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray007()
        {
            String code = @"
a[3] = 3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray008()
        {
            String code = @"
a[0] = false;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray009()
        {
            String code = @"
a = false;
a[3] = 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray010()
        {
            String code = @"
a = false;
a[1][1] = {3};
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray011()
        {
            String code = @"
a[0] = 1;
a[0][1] = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray012()
        {
            String code = @"
a = 1;
a[-1] = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray013()
        {
            String code = @"
a = 1;
a[-3] = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray014()
        {
            String code = @"
a = {1, 2};
a[3] = 3;
a[-5] = 100;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray015()
        {
            String code = @"
a = 1;
a[-2][-1] = 3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestDynamicArray016()
        {
            String code = @"
a = {{1, 2}, {3, 4}};
a[-3][-1] = 5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArrayIndexReplication01()
        {
            string code = @"
a = 1;
a[1..2] = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArrayIndexReplication02()
        {
            string code = @"
a = {1, 2, 3};
b = a[1..2];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestTypeArrayAssign4()
        {
            string code = @"
a:int[] = {1, 2, 3};
a[0] = false;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestTypeArrayAssign5()
        {
            string code = @"
a = {false, 2, true};
b:int[] = a;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestTypeArrayAssign6()
        {
            string code = @"
a:int = 2;
a[1] = 3;;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQNestedBlocks001_002()
        {
            String code =
                @"[Associative]
{
    a = 4;
    b = a*2;
                
    [Imperative]
    {
        i=0;
        temp=1;
        //if(i<=a)
        //{
            //temp=temp+1;
        //}
    }
    a = 1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
      
        

        [Test]
        public void DebugEQLogicalOp001_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = true;
	                        b = false;
                            c = 1;
                            d = a && b;
                            e = c && d;
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQLogicalOp002_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = true;
	                        b = false;
                            c = 1;
                            d = a || b;
                            e = c || d;
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQLogicalOp003_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = true;
	                        b = false;
                            c = !(a || !b);
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQDoubleOp_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = 1 + 2;
                            b = 2.0;
                            b = a + b; 
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQRangeExpr001_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = 1..5;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3]; 
                            f = a[4];
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQRangeExpr002_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = 1.5..5..1.1;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];                             
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQRangeExpr003_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = 15..10..-1.5;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];                             
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQRangeExpr004_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = 0..15..#5;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3]; 
                            f = a[4];                            
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQRangeExpr005_002()
        {
            String code =
                @"
                        [Associative]
                        {
	                        a = 0..15..~4;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];  
                            f = a[4];                           
                        }
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQInlineCondition001_002()
        {
            String code =
                @"
	                        a = 10;
                            b = 20;
                            c = a < b ? a : b;
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQInlineCondition002_002()
        {
            String code =
                @"	
	                        a = 10;
			                b = 20;
                            c = a > b ? a : a == b ? 0 : b; 
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQInlineCondition003_002()
        {
            String code =
                @"
a = {11,12,10};
t = 10;
b = a > t ? 2 : 1;
x = b[0];
y = b[1];
z = b[2];
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQInlineCondition004()
        {
            String code =
                @"
def f(i : int)
{
    return = i + 1;
}
def g()
{
    return = 1;
}
a = {10,0,10};
t = 1;
b = a > t ? f(10) : g();
x = b[0];
y = b[1];
z = b[2];
                        ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
       

        [Test]
        public void DebugEQModulo001_002()
        {
            String code =
                @"
                    a = 10 % 4; // 2
                    b = 5 % a; // 1
                    c = b + 11 % a * 3 - 4; // 0
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQModulo002_002()
        {
            String code =
                @"
                    a = 10 % 4; // 2
                    b = 5 % a; // 1
                    c = 11 % a == 2 ? 11 % 2 : 11 % 3; // 2
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQNegativeIndexOnCollection001_002()
        {
            String code =
                @"
                    a = {1, 2, 3, 4};
                    b = a[-2]; // 3
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQNegativeIndexOnCollection002_002()
        {
            String code =
                @"
                    a = { { 1, 2 }, { 3, 4 } };
                    b = a[-1][-2]; // 3
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestUpdate01()
        {
            String code =
                @"
                    a = 1;
                    b = a;
                    a = 10;
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestUpdate03()
        {
            String code =
                @"
def f : int(p : int)
{
    a = 10;
    b = a;
    a = p;
    return = b;
}
x = 20;
y = f(x);
x = 40;
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArrayUpdateRedefinition01()
        {
            String code =
                @"
a = 1;
c = 2;
b = a + 1;
b = c + 1;
a = 3;
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArrayUpdateRedefinition02()
        {
            String code =
                @"
                    a = 1;
                    a = a + 1;
                    a = 10;
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestArrayUpdate01()
        {
            String code =
                @"
a = {10,11,12};
t = 0;
i = a[t];
t = 2;
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQTestXLangUpdate01()
        {
            String code =
                @"
[Associative]
{
    a = 1;
    b = a;
    [Imperative]
    {
        a = a + 1;
    }
}
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestXLangUpdate02()
        {
            String code =
                @"
[Associative]
{
    a = 1;
    b = a;
    a = 10;
    [Imperative]
    {
        a = a + 1;
    }
}
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestXLangUpdate03()
        {
            String code =
                @"
[Associative]
{
    a = 1;
    b = a;
    c = 100;
    d = c;
    [Imperative]
    {
        a = a + 1;
        c = 10;
    }
}
                ";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT001_Simple_Update()
        {
            string code = @"
a = 1;
b = a + 1;
a = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT001_SomeNulls_IfElse_01()
        {
            string code = @"
result =
[Imperative]
{
	arr1 = {1,null};
	arr2 = {1,2};
	if(SomeNulls(arr1))
	{
		arr2 = arr1;
	}
	return = SomeNulls(arr2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT001_SomeNulls_IfElse_02()
        {
            string code = @"
result =
[Imperative]
{
	arr1 = {};
	arr2 = {1,2};
	if(SomeNulls(arr1))
	{
		arr2 = arr1;
	}
	return = SomeNulls(arr2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT001_implicit_programming_Robert()
        {
            string code = @"
// no paradigm specified, so assume Associative
// some Associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some Imperative code ....
[Imperative]
{
    if (a>10) 	// implicit switch to Imperative paradigm
    {
        c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
        b=b/2;	// is executed before the statement on line 14 [as would be expected]
    }
    else
    {
        [Associative] 	// explicit switch to Associative paradigm [overrides the Imperative paradigm]
        {
            c = b;    	// c references the final state of b, therefore [because we are in an Associative paradigm] 
            b = b*2;	// the statement on line 21 is executed before the statement on line 20
        }
    }
}
// some more Associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a Imperative block is nested within an Associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the Associative graph before the Imperative block
			//			the Imperative block
			//			the part of the Associative graph after the Imperative block
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT001_implicit_programming_Robert_2()
        {
            string code = @"
// no paradigm specified, so assume Associative
// some Associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some Imperative code ....
[Imperative]
{
	if (a>10) 	// explicit switch to Imperative paradigm
	{
		c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
		b=b/2;	// is executed before the statement on line 14 [as would be expected]
	}
	else
	{
		[Associative] 	// explicit switch to Associative paradigm [overrides the Imperative paradigm]
		{
			c = b;    	// c references the final state of b, therefore [because we are in an Associative paradigm] 
			b = b*2;	// the statement on line 21 is executed before the statement on line 20
		}
	}
}
// some more Associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a Imperative block is nested within an Associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the Associative graph before the Imperative block
			//			the Imperative block
			//			the part of the Associative graph after the Imperative block
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT002_BasicImport_AbsoluteDirectory()
        {


            string code = @"
import (""..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


       

        [Test]
        public void DebugEQT002_SomeNulls_ForLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,3,5,7,{}};
	b = {null,null,true};
	c = {SomeNulls({1,null})};
	d = {a,b,c};
	j = 0;
	e = {};
	
	for(i in d)
	{
		
		e[j]= SomeNulls(i);
		j = j+1;
	}
	return  = e;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT002_Update_Collection()
        {
            string code = @"
a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT002_limits_to_replication_1_Robert()
        {
            string code = @"
a = 0..10..2; 
b = a>5? 0:1; 
[Imperative]
{
	c = a * 2; // replication within an Imperative block [OK?]
	d = a > 5 ? 0:1; // in-line conditional.. operates on a collection [inside an Imperative block, OK?]
	if( c[2] > 4 ) x = 10; // if statement evaluates a single term [OK]
	
	if( c > 4 ) // but... replication within a regular 'if..else' any support for this?
	{
		y = 1;
	}
	else
	{
		y = -1;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_Associative_Function_MultilineFunction()
        {
            string code = @"
[Associative]
{
	def Divide : int(a:int, b:int)
	{
		return = a/b;
	}
	d = Divide (1,3);
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_BasicImport_ParentPath()
        {
            string code = @"
import (""../basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_Inline_Using_Collection__2_()
        {
            string code = @"
	Passed = 1;
	Failed = 0;
	Einstein = 56;
	BenBarnes = 90;
	BenGoh = 5;
	Rameshwar = 80;
	Jun = 68;
	Roham = 50;
	Smartness = { BenBarnes, BenGoh, Jun, Rameshwar, Roham }; // { 1, 0, 1, 1, 0 }
	Results = Smartness > Einstein ? Passed : Failed;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_Inline_Using_Collection()
        {
            string code = @"
[Imperative]
{
	Passed = 1;
	Failed = 0;
	Einstein = 56;
	BenBarnes = 90;
	BenGoh = 5;
	Rameshwar = 80;
	Jun = 68;
	Roham = 50;
	Smartness = { BenBarnes, BenGoh, Jun, Rameshwar, Roham }; // { 1, 0, 1, 1, 0 }
	Results = Smartness > Einstein ? Passed : Failed;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_LanguageBlockScope_ImperativeNestedAssociative()
        {
            string code = @"
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Associative]	
	{
		a_inner = a;
		b_inner = b;
		c_inner = c;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_SomeNulls_WhileLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,3,5,7,{}};
	b = {null,null,true};
	c = {{}};
	
	d = {a,b,c};
	
	i = 0;
	j = 0;
	e = {};
	
	while(i<Count(d))
	{
	
		e[j]= SomeNulls(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT003_Update_In_Function_Call()
        {
            string code = @"
def foo1 ( a : int ) 
{
    return = a + 1;
}
def foo2 ( a : int[] ) 
{
    a[0] = a[1] + 1;
	return = a;
}
def foo3 ( a : int[] ) 
{
    b = a;
	b[0] = b[1] + 1;
	return = b;
}
a = 0..4..1;
b = a[0];
c = foo1(b);
d = foo2(a);
e1 = foo3(a);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT004_Associative_Function_SpecifyReturnType()
        {
            string code = @"
[Associative]
{
	def Divide : double (a:int, b:int)
	{
		return = a/b;
	}
	d = Divide (1,3);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT004_BasicImport_CurrentDirectoryWithDotAndSlash()
        {
            string code = @"
import ("".\basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT004_LanguageBlockScope_AssociativeNestedImperative()
        {
            string code = @"
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Imperative]	
	{
		a_inner = a;
		b_inner = b;
		c_inner = c;
	}
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT004_SomeNulls_Function()
        {
            string code = @"
def foo(x:var[]..[])
{
	a = {};
	i = 0;
	[Imperative]
	{
		for(j in x)
		{
			if(SomeNulls(j))
			{
				a[i] = j;
				i = i+1;
			}
		}
	}
	return  = Count(a);
}
b = {
{null},
{1,2,3,{}},
{0}
};
result = foo(b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT004_Update_In_Function_Call_2()
        {
            string code = @"
def foo1 ( a : int ) 
{
    return = a + 1;
}
def foo3 ( a : int[] ) 
{
    b = a;
	b[0] = b[1] + 1;
	return = b;
}
a = 0..4..1;
b = a[0];
c = foo1(b);
e1 = foo3(a);
a = 10..14..1;
a[1] = a[1] + 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT004_simple_order_1_Robert()
        {
            string code = @"
a1 = 10;        // =1
b1 = 20;        // =1
a2 = a1 + b1;   // =3
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5
a  = a2 + b;    // 6
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT005_Associative_Function_SpecifyArgumentType()
        {
            string code = @"
[Associative]
{
	def myFunction : int (a:int, b:int)
	{
		return = a + b;
	}
	d1 = 1.12;
	d2 = 0.5;
	
	result = myFunction (d1, d2);
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT005_BasicImport_RelativePath()
        {
            string code = @"
import ("".\\ExtraFolderToTestRelativePath\\basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT005_Inline_Using_2_Collections_In_Condition__2_()
        {
            string code = @"
	a1 	=  1..3..1; 
	b1 	=  4..6..1; 
	a2 	=  1..3..1; 
	b2 	=  4..7..1; 
	a3 	=  1..4..1; 
	b3 	=  4..6..1; 
	c1 = a1 > b1 ? true : false; // { false, false, false }
	c2 = a2 > b2 ? true : false; // { false, false, false }
	c3 = a3 > b3 ? true : false; // { false, false, false, null }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT005_Inline_Using_2_Collections_In_Condition()
        {
            string code = @"
[Imperative]
{
	a1 	=  1..3..1; 
	b1 	=  4..6..1; 
	a2 	=  1..3..1; 
	b2 	=  4..7..1; 
	a3 	=  1..4..1; 
	b3 	=  4..6..1; 
	c1 = a1 > b1 ? true : false; // { false, false, false }
	c2 = a2 > b2 ? true : false; // { false, false, false }
	c3 = a3 > b3 ? true : false; // { false, false, false, null }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT005_LanguageBlockScope_DeepNested_IAI()
        {
            string code = @"
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Associative]	
	{
		a_inner1 = a;
		b_inner1 = b;
		c_inner1 = c;
		
		
		[Imperative]
		{
			a_inner2 = a;
			b_inner2 = b;
			c_inner2 = c;
			
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT005_Update_In_collection()
        {
            string code = @"
a=1;
b=2;
c=4;
collection = {a,b,c};
collection[1] = collection[1] + 0.5;
d = collection[1];
d = d + 0.1; // updates the result of accessing the collection
b = b + 0.1; // updates the source member of the collection
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_Associative_Function_PassingNullAsArgument()
        {
            string code = @"
[Associative]
{
	def myFunction : double (a: double, b: double)
	{
		return = a + b;
	}
	d1 = null;
	d2 = 0.5;
	
	result = myFunction (d1, d2);
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_BasicImport_TestFunction()
        {
            string code = @"
import (""basicImport.ds"");
a = {1.1,2.2};
b = 2;
c = Scale(a,b);
d = Sin(30.0);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_Inline_Using_Different_Sized_1_Dim_Collections__2_()
        {
            string code = @"
	a = 10 ;
	b = ((a - a / 2 * 2) > 0)? a : a+1 ; //11
	c = 5; 
	d = ((c - c / 2 * 2) > 0)? c : c+1 ; //5 
	e1 = ((b>(d-b+d))) ? d : (d+1); //5
	//inline conditional, returning different sized collections
	c1 = {1,2,3};
	c2 = {1,2};
	a1 = {1, 2, 3, 4};
	b1 = a1>3?true:a1; // expected : {1, 2, 3, true}
	b2 = a1>3?true:c1; // expected : {1, 2, 3}
	b3 = a1>3?c1:c2;   // expected : {1, 2}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_Inline_Using_Different_Sized_1_Dim_Collections()
        {
            string code = @"
[Imperative]
{
	a = 10 ;
	b = ((a - a / 2 * 2) > 0)? a : a+1 ; //11
	c = 5; 
	d = ((c - c / 2 * 2) > 0)? c : c+1 ; //5 
	e1 = ((b>(d-b+d))) ? d : (d+1); //5
	//inline conditional, returning different sized collections
	c1 = {1,2,3};
	c2 = {1,2};
	a1 = {1, 2, 3, 4};
	b1 = a1>3?true:a1; // expected : {1, 2, 3, true}
	b2 = a1>3?true:c1; // expected : {1, 2, 3}
	b3 = a1>3?c1:c2;   // expected : {1, 2}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_LanguageBlockScope_DeepNested_AIA()
        {
            string code = @"
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Imperative]	
	{
		a_inner1 = a;
		b_inner1 = b;
		c_inner1 = c;
		
		
		[Associative]
		{
			a_inner2 = a;
			b_inner2 = b;
			c_inner2 = c;
			
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_SomeNulls_Inline()
        {
            string code = @"
[Imperative]
{
a = {null,1};
b = {};
c = {1,2,3};
result = SomeNulls(c)?SomeNulls(b):SomeNulls(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT006_grouped_1_Robert()
        {
            string code = @"
a1 = 10;        // =1
a2 = a1 + b1;   // =3
a  = a2 + b;    // 6    
    
b1 = 20;        // =1
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT007_Associative_Function_NestedFunction()
        {
            string code = @"
[Associative]
{
	def ChildFunction : double (r1 : double)
	{
	return = r1;
	
	}
	def ParentFunction : double (r1 : double)
	{
		return = ChildFunction (r1)*2;
	}
	d1 = 1.05;
	
	result = ParentFunction (d1);
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT007_Inline_Using_Collections_And_Replication_CollectionFunctionCall__2_()
        {
            string code = @"
	def even : int(a : int)
	{
		return = a * 2;
	}
	a =1..10..1 ; //{1,2,3,4,5,6,7,8,9,10}
	i = 1..5; 
	b = ((a[i] % 2) > 0)? even(a[i]) : a ;  // { 1, 6, 3, 10, 5 }	
	c = ((a[0] % 2) > 0)? even(a[i]) : a ; // { 4, 6, 8, 10, 12 }
	d = ((a[-2] % 2) == 0)? even(a[i]) : a ; // { 1, 2,..10}
	e1 = (a[-2] == d[9])? 9 : a[1..2]; // { 2, 3 }
";
            // defect : DNL-1467619 Regression : Replication + InlineCondition yields different output in release and debug mode
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT007_LanguageBlockScope_AssociativeParallelImperative()
        {
            string code = @"
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT007_SomeNulls_RangeExpression()
        {
            string code = @"
result =
[Imperative]
{
i = 0;
arr = {{1,1.2} , {null,0}, {true, false} };
a1 = 0;
a2 = 2;
d = 1;
a = a1..a2..d;
for(i in a)
{
	if(SomeNulls(arr[i])) 
	return = i;
	
}
return = -1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT008_Associative_Function_DeclareVariableBeforeFunctionDeclaration()
        {
            string code = @"
[Associative]
{
    a = 1;
	b = 10;
	def Sum : int(a : int, b : int)
	{
	
		return = a + b;
	}
	
	sum = Sum (a, b);
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT008_Inline_Returing_Different_Ranks__2_()
        {
            string code = @"
	a = { 0, 1, 2, 4};
	x = a > 1 ? 0 : {1,1}; // { 1, 1} ? 
	x_0 = x[0];
	x_1 = x[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT008_Inline_Returing_Different_Ranks()
        {
            string code = @"
[Imperative]
{
	a = { 0, 1, 2, 4};
	x = a > 1 ? 0 : {1,1}; // { 1, 1} ? 
	x_0 = x[0];
	x_1 = x[1];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT008_LanguageBlockScope_ImperativeParallelAssociative()
        {
            string code = @"
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT008_SomeNulls_Replication()
        {
            string code = @"
/*
[Imerative]
{
	a = 1..5;
	i = 0..3;
	x = a[i];
}
*/
a = {
{{null, 1},1},
{null},
{1,2,false}
};
i = 0..2;
j = 0;
[Imperative]
{
		if(SomeNulls(a[i]))
		{
			j = j+1;
		}
		
} 
//Note : the following works fine : 
/*
[Imperative]
{
	for ( x in  i) 
	{		
	    if(SomeNulls(a[x]))
	    {
                j = j+1;
	    }
	}
}
*/
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT009_Associative_Function_DeclareVariableInsideFunction()
        {
            string code = @"
[Associative]
{
	def Foo : int(input : int)
	{
		multiply = 5;
		divide = 10;
	
		return = {input*multiply, input/divide};
	}
	
	input = 20;
	sum = Foo (input);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT009_Inline_Using_Function_Call_And_Collection_And_Replication__2_()
        {
            string code = @"
	def even(a : int)
	{
		return = a * 2;
	}
	def odd(a : int ) 
	{
	return = a* 2 + 1;
	}
	x = 1..3;
	a = ((even(5) > odd(3)))? even(5) : even(3); //10
	b = ((even(x) > odd(x+1)))?odd(x+1):even(x) ; // {2,4,6}
	c = odd(even(3)); // 13
	d = ((a > c))?even(odd(c)) : odd(even(c)); //53
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT009_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_IA()
        {
            string code = @"
[Imperative]
{
	a = -10;
	b = false;
	c = -20.1;
	[Associative]	
	{
		a = 1.5;
		b = -4;
		c = false;
	}
	
	newA = a;
	newB = b;
	newC = c;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT009_Update_Of_Undefined_Variables()
        {
            string code = @"
u1 = u2;
u2 = 3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_Associative_Function_PassAndReturnBooleanValue()
        {
            string code = @"
[Associative]
{
	def Foo : bool (input : bool)
	{
		return = input;
	}
	
	input = false;
	result1 = Foo (input);
	result2 = Foo (true);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_Defect_1456751_execution_on_both_true_and_false_path_issue()
        {
            string code = @"
a = 0;
def foo ( )
{
    a = a + 1;
    return = a;
}
x = 1 > 2 ? foo() + 1 : foo() + 2;
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_Defect_1456751_replication_issue()
        {
            string code = @"
[Imperative]
{
	a = { 0, 1, 2};
	b = { 3, 11 };
	c = 5;
	d = { 6, 7, 8, 9};
	e = { 10 };
	xx = 1 < a ? a : 5;
        yy = 0;
	if( 1 < a )
	    yy = a;
	else
	    yy = 5;
	x1 = a < 5 ? b : 5;
	t1 = x1[0];
	t2 = x1[1];
	c1 = 0;
	for (i in x1)
	{
		c1 = c1 + 1;
	}
	x2 = 5 > b ? b : 5;
	t3 = x2[0];
	t4 = x2[1];
	c2 = 0;
	for (i in x2)
	{
		c2 = c2 + 1;
	}
	x3 = b < d ? b : e;
	t5 = x3[0];
	c3 = 0;
	for (i in x3)
	{
		c3 = c3 + 1;
	}
	x4 = b > e ? d : { 0, 1};
	t7 = x4[0]; 
	c4 = 0;
	for (i in x4)
	{
		c4 = c4 + 1;
	}
}
/*
Expected : 
result1 = { 5, 5, 2 };
thisTest.Verification(mirror, ""xx"", result1, 1);
thisTest.Verification(mirror, ""t1"", 3, 1);
thisTest.Verification(mirror, ""t2"", 11, 1);
thisTest.Verification(mirror, ""c1"", 2, 1);
thisTest.Verification(mirror, ""t3"", 3, 1);
thisTest.Verification(mirror, ""t4"", 5, 1);
thisTest.Verification(mirror, ""c2"", 2, 1);
thisTest.Verification(mirror, ""t5"", 3, 1); 
thisTest.Verification(mirror, ""c3"", 1, 1);
thisTest.Verification(mirror, ""t7"", 0, 1);
thisTest.Verification(mirror, ""c4"", 1, 1);*/
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_Inline_Using_Literal_Values()
        {
            string code = @"
[Imperative]
{
	a = 1 > 2.5 ? false: 1;
	b = 0.55 == 1 ? true : false;
	c = (( 1 + 0.5 ) / 2 ) <= (200/10) ? (8/2) : (6/3);
	d = true ? true : false;
	e = false ? true : false;
	f = true == true ? 1 : 0.5;
	g = (1/3.0) > 0 ? (1/3.0) : (4/3);
	h = (1/3.0) < 0 ? (1/3.0) : (4/3);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_AI()
        {
            string code = @"
[Associative]
{
	a = -10;
	b = false;
	c = -20.1;
	[Imperative]	
	{
		a = 1.5;
		b = -4;
		c = false;
	}
	
	newA = a;
	newB = b;
	newC = c;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_SomeNulls_AssociativeImperative_01()
        {
            string code = @"
[Imperative]
{
	a = {1,2,null};
	b = {null, null};
	
	[Associative]
	{
		a = {1};
		b = a;
		m = SomeNulls(b);
		a = {1,null,{}};
		n = m;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
        [Test]
        public void DebugEQT010_SomeNulls_AssociativeImperative_02()
        {
            string code = @"
[Imperative]
{
	a = {false};
	if(!SomeNulls(a))
	{
	[Associative]
	{
		
		b = a;
		a = {null};
		
		m = SomeNulls(b);//true,false
		[Imperative]
		{
			c = a;
			a = {2};
			n = SomeNulls(c);//true
		}
		
	}
	}else
	{
	m = false;
	n = false;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_SomeNulls_AssociativeImperative_03()
        {
            string code = @"
	
	a = {{}};
	b = a;
	
	m = SomeNulls(b);//false
	[Imperative]
	{
		c = a;
		a = {null,{}};
		m = SomeNulls(c);//false
	}
	a = {null};
	n = SomeNulls(b);//true;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_Update_Of_Singleton_To_Collection()
        {
            string code = @"
s1 = 3;
s2 = s1 -1;
s1 = { 3, 4 } ;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT010_imperative_if_inside_for_loop_1_Robert()
        {
            string code = @"
[Imperative]
{
	x = 0;
	
	for ( i in 1..10..2)
	{
		x = i;
		if(i>5) x = i*2; // tis is ignored
		// if(i<5) x = i*2; // this causes a crash
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT011_Associative_Function_FunctionWithoutArgument()
        {
            string code = @"
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	result1 = Foo1 ();
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT011_Defect_1467281_conditionals()
        {
            string code = @"
 x = 2 == { }; 
 y = {}==null;
 z = {{1}}=={1};
 z2 = {{1}}==1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT011_LanguageBlockScope_AssociativeParallelAssociative()
        {
            string code = @"
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT011_Update_Of_Variable_To_Null()
        {
            string code = @"
x = 1;
y = 2/x;
x = 0;
v1 = 2;
v2 = v1 * 3;
v1 = null;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT012_Associative_Function_MultipleFunctions()
        {
            string code = @"
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	
	def Foo2 : int ()
	{
		return = 6;
	}
	
	
	result1 = Foo1 ();
	result2 = Foo2 ();
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT012_BaseImportImperative()
        {
            string code = @"
import (""BaseImportImperative.ds"");
a = 1;
b = a;
[Associative]
{
	c = 3 * b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT012_CountTrue_IfElse()
        {
            string code = @"
result =
[Imperative]
{
	arr1 = {true,{{{{true}}}},null};
	arr2 = {{true},{false},null};
	if(CountTrue(arr1) > 1)
	{
		arr2 = arr1;
	}
	return = CountTrue(arr2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT012_LanguageBlockScope_ImperativeParallelImperative()
        {
            string code = @"
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT012_Update_Of_Variables_To_Bool()
        {
            string code = @"
p1 = 1;
p2 = p1 * 2;
p1 = false;
q1 = -3.5;
q2 = q1 * 2;
q1 = true;
s1 = 1.0;
s2 = s1 * 2;
s1 = false;
t1 = -1;
t2 = t1 * 2;
t1 = true;
r1 = 1;
r2 = r1 * 2;
r1 = true;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT013_Associative_Function_FunctionWithSameName_Negative()
        {
            string code = @"
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	
	
	def Foo1 : int ()
	{
		return = 6;
	}
	
	
	
	result2 = Foo2 ();
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT013_CountTrue_ForLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,3,5,7,{}};
	b = {null,null,{1,true}};
	c = {CountTrue({1,null})};
	
	d = {a,b,c};
	j = 0;
	e = {};
	
	for(i in d)
	{
		e[j]= CountTrue(i);
		j = j+1;
	}
	return  = e;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT013_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA()
        {
            string code = @"
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT014_CountTrue_WhileLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,3,5,7,{1}};//0
	b = {1,null,true};//1
	c = {{false}};//0
	
	d = {a,b,c};
	
	i = 0;
	j = 0;
	e = {};
	
	while(i<Count(d))
	{
		e[j]= CountTrue(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT014_Inline_Using_Collections()
        {
            string code = @"
[Imperative]
{
	a = { 0, 1, 2};
	b = { 3, 11 };
	c = 5;
	d = { 6, 7, 8, 9};
	e = { 10 };
	x1 = a < 5 ? b : 5;
	t1 = x1[0];
	t2 = x1[1];
	c1 = 0;
	for (i in x1)
	{
		c1 = c1 + 1;
	}
	
	x2 = 5 > b ? b : 5;
	t3 = x2[0];
	t4 = x2[1];
	c2 = 0;
	for (i in x2)
	{
		c2 = c2 + 1;
	}
	
	x3 = b < d ? b : e;
	t5 = x3[0];
	c3 = 0;
	for (i in x3)
	{
		c3 = c3 + 1;
	}
	
	x4 = b > e ? d : { 0, 1};
	t7 = x4[0];	
	c4 = 0;
	for (i in x4)
	{
		c4 = c4 + 1;
	}
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT014_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI()
        {
            string code = @"
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT015_Associative_Function_UnmatchFunctionArgument_Negative()
        {
            string code = @"
[Associative]
{
	def Foo : int (a : int)
	{
		return = 5;
	}
	
	result = Foo(1,2); 
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT015_CountTrue_Function()
        {
            string code = @"
def foo(x:var[]..[])
{
	a = {};
	i = 0;
	[Imperative]
	{
		for(j in x)
		{
			a[i] = CountTrue(j);
			i = i+1;
		}
	}
	return  = a;
}
b = {
{null},//0
{1,2,3,{true}},//1
{0},//0
{true, true,1,true, null},//3
{x, null}//0
};
result = foo(b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT015_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II()
        {
            string code = @"
[Associative]
{
	a = 10;
	
	[Imperative]	
	{
		aI1 = a;
	}
	aA1 = a;
	
	[Imperative]	
	{
		aI2 = a;
	}
	
	aA2 = a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT016_Associative_Function_ModifyArgumentInsideFunctionDoesNotAffectItsValue()
        {
            string code = @"
[Associative]
{
	def Foo : int (a : int)
	{
		a = a + 1;
		return = a;
	}
	input = 3;
	result = Foo(input); 
	originalInput = input;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT016_BaseImportAssociative()
        {
            string code = @"
import (""BaseImportAssociative.ds"");
a = 10;
b = 20;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT016_Inline_Using_Operators()
        {
            string code = @"
def foo (a:int )
{
	 return = a;   
}
a = 1+2 > 3*4 ? 5-9 : 10/2;
b = a > -a ? 1 : 0;
c = 2> 1 && 4>3 ? 1 : 0;
d = 1 == 1 || (1 == 0) ? 1 : 0;
e1 = a > b && c > d ? 1 : 0;
f = a <= b || c <= d ? 1 : 0;
g = foo({ 1, 2 }) > 3+ foo({4,5,6}) ?  1 : 3+ foo({4,5,6});
i = {1,3} > 2 ? 1: 0;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT016_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
            string code = @"
[Imperative]
{
	a = 10;
	[Associative]	
	{
		aA1 = a;
	}
	aI1 = a;
	
	[Associative]	
	{
		aA2 = a;
	}
	aI2 = a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT017_Associative_Function_CallingAFunctionBeforeItsDeclaration()
        {
            string code = @"
[Associative]
{
	def Level1 : int (a : int)
	{
		return = Level2(a+1);
	}
	
	def Level2 : int (a : int)
	{
		return = a + 1;
	}
	input = 3;
	result = Level1(input);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT017_BaseImportWithVariableClassInstance_Associativity()
        {
            string code = @"
import (""BaseImportWithVariableClassInstance.ds"");
c = a + b;
a = 10;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT017_Inline_In_Function_Scope()
        {
            string code = @"
def foo1 ( b )
{
	return = b == 0 ? b : b+1;
	
}
def foo2 ( x )
{
	y = [Imperative]
	{
	    if(x > 0)
		{
		   return = x >=foo1(x) ? x : foo1(x);
		}
		return = x >=2 ? x : 2;
	}
	x1 = y == 0 ? 0 : y;
	return = y + x1;
}
a1 = foo1(4);
a2 = foo2(3);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT018_CountTrue_RangeExpression_01()
        {
            string code = @"
result = 
[Imperative]
{
	a1 = {1,true, null};//1
	a2 = 8;
	a3 = {2,{true,{true,1}},{false,x, true}};//3
	a = CountTrue(a1)..a2..CountTrue(a3);//{1,4,7}
	
	return = CountTrue(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT018_CountTrue_RangeExpression_02()
        {
            string code = @"
result = 
[Imperative]
{
	a1 = {1,true, null};//1
	a2 = 8;
	a3 = {2,{true,{true,1}},{false,x, true}};//3
	a = CountTrue(a1)..a2..~CountTrue(a3);//{}
	return = CountTrue(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT018_CountTrue_RangeExpression_03()
        {
            string code = @"
result = 
[Imperative]
{
	a1 = {1,true, null};//1
	a2 = 8;
	a3 = {2,{true,{true,1}},{false,x, true}};//3
	a = {1.0,4.0,7.0};
	//a = CountTrue(a1)..a2..#CountTrue(a3);//{}
	return = CountTrue(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT019_CountTrue_Replication()
        {
            string code = @"
def foo(x:int)
{
	return = x +1;
}
a = {true,{true},1};//2
b = {null};
c = {{{true}}};//1
d = {{true},{false,{true,true}}};//3
arr = {CountTrue(a),CountTrue(b),CountTrue(c),CountTrue(d)};
result = foo(arr);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT019_Defect_1456758()
        {
            string code = @"
b = true;
a1 = b && true ? -1 : 1;
[Imperative]
{
	a2 = b && true ? -1 : 1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT019_Update_General()
        {
            string code = @"
X = 1;
Y = X + 1;
X = X + 1;
X = X + 1;
//Y = X + 1;
//X  = X + 1;
test = X + Y;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_Arithmatic_List_And_List_Different_Length()
        {
            string code = @"
list1 = { 1, 4, 7, 2};
list2 = { 5, 8, 3, 6, 7, 9 };
list3 = list1 + list2; // { 6, 12, 10, 8 }
list4 = list1 - list2; // { -4, -4, 4, -4}
list5 = list1 * list2; // { 5, 32, 21, 12 }
list6 = list2 / list1; // { 5, 2, 0, 3 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_BasicGlobalFunction()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = foo;
b = foo(3); //b=3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_Function_In_Assoc_Scope()
        {
            string code = @"
[Associative]
{
    def foo : int( a:int )
    {
	   return = a * 10;
	}
	
    a = foo( 2 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_SimpleRangeExpression()
        {
            string code = @"
[Imperative]
{
	a = 1..-6..-2;
	a1 = 2..6..~2.5; 
	a2 = 0.8..1..0.2; 
	a3 = 0.7..1..0.3; 
	a4 = 0.6..1..0.4; 
	a5 = 0.8..1..0.1; 
	a6 = 1..1.1..0.1; 
	a7 = 9..10..1; 
	a8 = 9..10..0.1;
	a9 = 0..1..0.1; 
	a10 = 0.1..1..0.1;
	a11 = 0.5..1..0.1;
	a12 = 0.4..1..0.1;
	a13 = 0.3..1..0.1;
	a14 = 0.2..1..0.1;
	a17 = (0.5)..(0.25)..(-0.25);
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_Simple_1D_Collection_Assignment()
        {
            string code = @"
[Imperative]
{
	a = { {1,2}, {3,4} };
	
	a[1] = {-1,-2,3};
	
	c = a[1][1];
	
	d = a[0];
	
	b = { 1, 2 };
	
	b[0] = {2,2};
	e = b[0];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_1()
        {
            string code = @"
a = ""word"";
b = ""word "";
result = 
[Imperative]
{
	if(a==b)
	{
		return = true;
	}
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_2()
        {
            string code = @"
a = ""w ord"";
b = ""word"";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_3()
        {
            string code = @"
a = "" "";
b = """";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_4()
        {
            string code = @"
a = ""a"";
b = ""a"";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_5()
        {
            string code = @"
a = ""  "";//3 whiteSpace
b = ""	"";//tab
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_6()
        {
            string code = @"
a = """";
b = "" "";
result = 
[Imperative]
{
	if (a ==null && b!=null) return = true;
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_String_IfElse_7()
        {
            string code = @"
a = ""a"";
result = 
[Imperative]
{
	if (a ==true||a == false) return = true;
	else return = false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_TestAllPassCondition()
        {
            string code = @"
[Imperative]
{
 a1 = 2 ;
 a2 = -1;
 a3 = 101;
 a4 = 0;
 
 b1 = 1.0;
 b2 = 0.0;
 b3 = 0.1;
 b4 = -101.99;
 b5 = 10.0009;
 
 c1 = { 0, 1, 2, 3};
 c2 = { 1, 0.2};
 c3 = { 0, 1.4, true };
 c4 = {{0,1}, {2,3 } };
 
 x = {0, 0, 0, 0};
 if(a1 == 2 ) // pass condition
 {
     x[0] = 1;
 }  
 if(a2 <= -1 )  // pass condition
 {
     x[1] = 1;
 }
 if(a3 >= 101 )  // pass condition
 {
     x[2] = 1;
 }
 if(a4 == 0 )  // pass condition
 {
     x[3] = 1;
 }
 
 
 y = {0, 0, 0, 0, 0};
 if(b1 == 1.0 ) // pass condition
 {
     y[0] = 1;
 }  
 if(b2 <= 0.0 )  // pass condition
 {
     y[1] = 1;
 }
 if(b3 >= 0.1 )  // pass condition
 {
     y[2] = 1;
 }
 if(b4 == -101.99 )  // pass condition
 {
     y[3] = 1;
 }
 if(b5 == 10.0009 )  // pass condition
 {
     y[4] = 1;
 }
 
 
 z = {0, 0, 0, 0};
 if(c1[0] == 0 ) // pass condition
 {
     z[0] = 1;
 }  
 if(c2[1] <= 0.2 )  // pass condition
 {
     z[1] = 1;
 }
 if(c3[2] == true )  // pass condition
 {
     z[2] = 1;
 }
  if(c4[0][0] == 0 )  // pass condition
 {
     z[3] = 1;
 }
 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

      
        [Test]
        public void DebugEQT01_Update_Variable_Across_Language_Scope()
        {
            string code = @"
[Associative]
{
    a = 0;
	d = a + 1;
    [Imperative]
    {
		b = 2 + a;
		a = 1.5;
		
    }
	c = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT01_WhileBreakContinue()
        {
            string code = @"
[Imperative]
{
    x = 0;
    y = 0;
    while (true) 
    {
        x = x + 1;
        if (x > 10)
            break;
        
        if ((x == 1) || (x == 3) || (x == 5) || (x == 7) || (x == 9))
            continue;
        
        y = y + 1;
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT020_LanguageBlockScope_AssociativeNestedImperative_Function()
        {
            string code = @"
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	[Imperative]	
	{
	x = 20;
	y = 10;
	z = foo (x, y);
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT020_MultipleImport_WithSameFunctionName()
        {
            string code = @"
import (""basicImport1.ds"");
import (""basicImport3.ds"");
arr = { 1.0, 2.0, 3.0 };
a1 = Scale( arr, 4.0 );
b = a * 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT020_Nested_And_With_Range_Expr()
        {
            string code = @"
a1 =  1 > 2 ? true : 2 > 1 ? 2 : 1;
a2 =  1 > 2 ? true : 0..3;
b = {0,1,2,3};
a3 = 1 > 2 ? true : b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT021_Defect_1457354()
        {
            string code = @"
import (""c:\\wrongPath\\test.ds"");
a = 1;
b = a * 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT021_Defect_1457354_2()
        {
            string code = @"
import (""basicImport"");
a = 1;
b = a * 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT021_Defect_1457354_3()
        {
            string code = @"
import (""basicImport12.ds"");
a = 1;
b = a * 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT021_Defect_1467166_array_comparison_issue()
        {
            string code = @"
[Imperative] 
{
    a = { 0, 1, 2}; 
    xx = a < 1 ? 1 : 0;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT022_Array_Marshal()
        {
            string code = @"
import (Dummy from ""FFITarget.dll"");
dummy = Dummy.Dummy();
arr = {0.0,1.0,2.0,3.0,4.0,5.0,6.0,7.0,8.0,9.0,10.0};
sum_1_10 = dummy.SumAll(arr);
twice_arr = dummy.Twice(arr);
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT022_CountTrue_ImperativeAssociative()
        {
            string code = @"
[Imperative]
{
	a1 = {true,0,1,1.0,null};
	a2 = {false, CountTrue(a1),0.0};
	a3 = a1;
	[Associative]
	{
		a1 = {true,{true}};
		a4 = a2;
		a2 = {true};
		b = CountTrue(a4);//1
	}
	
	c = CountTrue(a3);//1
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT022_Defect_1457740()
        {
            string code = @"
import (""basicImport1.ds"");
import (""basicImport3.ds"");
arr1 = { 1, 3, 5 };
temp = Scale( arr1, a );
a = a;
b = 2 * a;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT022_LanguageBlockScope_DeepNested_AIA_Function()
        {
            string code = @"
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	[Imperative]	
	{
		x_1 = 20;
		y_1 = 10;
		z_1 = foo (x_1, y_1);
	
	
	[Associative]
		{
			x_2 = 100;
			y_2 = 100;
			z_2 = foo (x_2, y_2);
			
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT023_CountFalse_IfElse()
        {
            string code = @"
result =
[Imperative]
{
	arr1 = {false,{{{{false}}}},null,0};
	arr2 = {{true},{false},null,null};
	if(CountFalse(arr1) > 1)
	{
		arr2 = arr1;
	}
	return = CountFalse(arr2);//2
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT023_LanguageBlockScope_AssociativeParallelImperative_Function()
        {
            string code = @"
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Imperative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	
}
";
            //Assert.Fail("");
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT024_CountFalse_ForLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,3,5,7,{}};
	b = {null,null,{0,false}};
	c = {CountFalse({{false},null})};
	
	d = {a,b,c};
	j = 0;
	e = {};
	
	for(i in d)
	{
		e[j]= CountFalse(i);
		j = j+1;
	}
	return  = e;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT024_Defect_1459470()
        {
            string code = @"
a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;
x = a;
					        
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT024_Defect_1459470_3()
        {
            string code = @"
def foo ()
{
	a = 0..4..1;
	b = a;
	c = b[2];
	a = 10..14..1;
	b[2] = b[2] + 1;
	a[2] = a[2] + 1;
	return = true;
	
}
a :int[];
b : int[];
c : int;
test = foo();
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT024_Defect_1459470_4()
        {
            string code = @"
a = {1,2,3,4};
b = a;
c = b[2];
d = a[2];
a[0..1] = {1, 2};
b[2..3] = 5;
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT025_CountFalse_WhileLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,3,5,7,{0}};//0
	b = {1,null,false};//1
	c = {{true}};//0
	
	d = {a,b,c};
	
	i = 0;
	j = 0;
	e = {};
	
	while(i<Count(d))
	{
		e[j]= CountFalse(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT025_Defect_1459704()
        {
            string code = @"
a = b;
b = 3;
c = a;
					        
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT025_LanguageBlockScope_AssociativeParallelAssociative_Function()
        {
            string code = @"
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Associative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT026_CountFalse_Function()
        {
            string code = @"
def foo(x:var[]..[])
{
	a = {};
	i = 0;
	[Imperative]
	{
		for(j in x)
		{
			a[i] = CountFalse(j);
			i = i+1;
		}
	}
	return  = a;
}
b = {
{null},//0
{1,2,3,{false}},//1
{0},//0
{false, false,0,false, null},//3
{x, null}//0
};
result = foo(b);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT027_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA_Function()
        {
            string code = @"
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Imperative]	
{
	x_1 = 20;
	y_1 = 0;
	z_1 = foo (x_1, y_1);
	
}
[Associative]
{
	x_2 = 20;
	y_2 = 0;
	z_2 = foo (x_2, y_2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT029_CountFalse_RangeExpression_01()
        {
            string code = @"
result = 
[Imperative]
{
	a1 = {0,false, null};//1
	a2 = 8;
	a3 = {2,{false,{false,1}},{false,x, true}};//3
	a = CountFalse(a1)..a2..CountFalse(a3);//{1,4,7}
	
	return = CountFalse(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT029_CountFalse_RangeExpression_02()
        {
            string code = @"
result = 
[Imperative]
{
	a1 = {1,false, null};//1
	a2 = 8;
	a3 = {2,{false,{false,1}},{false,x, true}};//3
	a = {1.0,4.0,7.0};
	//a = CountFalse(a1)..a2..#CountFalse(a3);//{}
	return = CountFalse(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT029_Defect_1460139_Update_In_Class()
        {
            string code = @"
X = 1;
Y = X + 1;
X = X + 1;
X = X + 1; // this line causing the problem
test = X + Y;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT029_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II_Function()
        {
            string code = @"
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	[Imperative]
	{
	x_I1 = 50;
	y_I1 = 50;
	z_I1 = foo (x_I1, y_I1);
	}
	
	x_A1 = 30;
	y_A1 = 12;
	z_A1 = foo (x_A1, y_A1);
	
	[Imperative]
	{
	x_I2 = 0;
	y_I2 = 12;
	z_I2 = foo (x_I2, y_I2);
	}
	
	x_A2 = 0;
	y_A2 = -10;
	z_A2 = foo (x_A2, y_A2);
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_Arithmatic_List_And_List_Same_Length()
        {
            string code = @"
list1 = { 1, 4, 7, 2};
list2 = { 5, 8, 3, 6 };
list3 = list1 + list2; // { 6, 12, 10, 8 }
list4 = list1 - list2; // { -4, -4, 4, -4}
list5 = list1 * list2; // { 5, 32, 21, 12 }
list6 = list2 / list1; // { 5, 2, 0, 3 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        

        [Test]
        public void DebugEQT02_Collection_Assignment_Associative()
        {
            string code = @"
[Associative]
{
	a = { {1,2}, {3,4} };
	
	a[1] = {-1,-2,3};
	
	c = a[1][1];
	
	d = a[0];
	
	b = { 1, 2 };
	
	b[0] = {2,2};
	e = b[0];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_GlobalFunctionWithDefaultArg()
        {
            string code = @"
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo;
b = foo(3); //b=5.0;
c = foo(2, 4.0); //c = 6.0
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_IfElseIf()
        {
            string code = @"
[Imperative]
{
 a1 = 7.5;
 
 temp1 = 10;
 
 if( a1>=10 )
 {
 temp1 = temp1 + 1;
 }
 
 elseif( a1<2 )
 {
 temp1 = temp1 + 2;
 }
 elseif(a1<10)
 {
 temp1 = temp1 + 3;
 }
 
  
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_SampleTestUsingCodeFromExternalFile__2_()
        {
            string code = @"
[Associative]
{
    variable = 5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_SampleTestUsingCodeFromExternalFile()
        {
            string code = @"
[Imperative]
{
    variable = 5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_SimpleRangeExpression()
        {
            string code = @"
[Imperative]
{
	a15 = 1/2..1/4..-1/4;
	a16 = (1/2)..(1/4)..(-1/4);
	a18 = 1.0/2.0..1.0/4.0..-1.0/4.0;
	a19 = (1.0/2.0)..(1.0/4.0)..(-1.0/4.0);
	a20 = 1..3*2; 
	//a21 = 1..-6;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_String_Not()
        {
            string code = @"
a = ""a"";
result = 
[Imperative]
{
	if(a)
	{
		return = false;
	}else if(!a)
	{
		return = false;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_TestAssocInsideImp()
        {
            string code = @"
[Imperative]
{
    x = 5.1;
    z = y;
    w = z * 2;
    [Associative]
    {
        y = 5;
        z = x;
        x = 35;
        i = 3;
    }
    f = i;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_Update_Function_Argument_Across_Language_Scope()
        {
            string code = @"
a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
[Imperative]
{
    a = 2.5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT02_WhileBreakContinue()
        {
            string code = @"
[Imperative]
{
    x = 0;
    sum = 0;
    while (x <= 10) 
    {
        x = x + 1;
        if (x >= 5)
            break;
        
        y = 0;
        while (true) 
        {
            y = y + 1;
            if (y >= 10)
                break;
        }
        // y == 10 
        sum = sum + y;
    }
    // sum == 40 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT030_CountFalse_Replication()
        {
            string code = @"
def foo(x:int)
{
	return = x +1;
}
a = {false,{false},0};//2
b = {CountFalse({a[2]})};
c = {{{false}}};//1
d = {{false},{false,{true,false,0}}};//3
arr = {CountFalse(a),CountFalse(b),CountFalse(c),CountFalse(d)};
result = foo(arr);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT032_Cross_Language_Variables()
        {
            string code = @"
a = 5;
b = 2 * a;
[Imperative] {
	sum = 0;
	arr = 0..b;
	for (i  in arr) {
		sum = sum + 1;
	}
}
a = 10;
// expected: sum = 21
// result: sum = 11
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT033_CountFalse_ImperativeAssociative()
        {
            string code = @"
[Imperative]
{
	a1 = {false,0,1,1.0,null};
	a2 = {true, CountFalse(a1),0.0};
	a3 = a1;
	[Associative]
	{
		a1 = {false,{false}};
		a4 = a2;
		a2 = {false};
		b = CountFalse(a4);//1
	}
	
	c = CountFalse(a3);//1
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        
        [Test]
        public void DebugEQT034_AllFalse_IfElse()
        {
            string code = @"
a = {false, false};//true
b = {{false}};//true
c = {false, 0};//false
result = {};
[Imperative]
{
	if(AllFalse(a)){
		a[2] = 0;
		result[0] = AllFalse(a);//false
	} 
	if(!AllFalse(b)){
		
		result[1] = AllFalse(b);//false
	}else
	{result[1]= null;}
	if(!AllFalse(c)){
		result[2] = AllFalse(c);
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT035_AllFalse_ForLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {false,false0,0,null,x};//false
	b = {false,false0,x};//false
	c = {};//false
	d = {{}};//false
	
	h = {
	{{0}},
	{false}
};
	e = {a,b ,c ,d,h};
	f = {};
	j = 0;
	for(i in e)
	{	
		if(AllFalse(i)!=true){
			f[j] = AllFalse(i);
			j = j+1;
		}
		
	}
return = f;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT036_1_Null_Check()
        {
            string code = @"
result = null;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT036_AllFalse_WhileLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {false,false0,0,null,x};//false
	b = {false,false0,x};//false
	c = {};//false
	d = {{}};//false
	e = {a,b ,c ,d};
	i = 0;
	f = {};
	j = 0;
	while(!AllFalse(e[i])&& i < Count(e))
	{	
		if(AllFalse(e[i])!=true){
			f[j] = AllFalse(e[i]);
			j = j+1;
		}
		i = i+1;
		
	}
return = f;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT037_AllFalse_Function()
        {
            string code = @"
def foo( x : bool)
{	
	return = !x;
}
a1 = {0};
a2 = {null};
a3 = {!true};
b = {a1,a2,a3};
result = {foo(AllFalse(a1)),foo(AllFalse(a2)),foo(AllFalse(a3))};//true,true,false
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT039_AllFalse_Inline()
        {
            string code = @"
a1 = {false,{false}};
a = AllFalse(a1);//true
b1 = {null,null};
b = AllFalse(b1);//false
c = AllFalse({b});//t
result = a? c:b;//t
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_Arithmatic_Mixed()
        {
            string code = @"
list1 = { 13, 23, 42, 65, 23 };
list2 = { 12, 8, 45, 64 };
list3 = 3 * 6 + 3 * (list1 + 10) - list2 + list1 * list2 / 3 + list1 / list2; // { 128, 172, 759, 1566 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_Assignment_Slicing_With_Collection()
        {
            string code = @"
def foo ( a:int[] )
{
	a[0] = 0;
	return = a;
}
	a = {1,2,3};
	c = foo ( a  );
	d = c[0];
	e = c[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_Collection_Assignment_Nested_Block()
        {
            string code = @"
[Associative]
{
	a = { {1,2,3},{4,5,6} };
	
	[Imperative]
	{
		c = a[0];
		d = a[1][2];
	}
	
	e = c;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_ForLoopBreakContinue()
        {
            string code = @"
[Imperative]
{
    sum = 0;
    for (x in {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13})
    {
        if (x >= 11)
            break;
        sum = sum + x;
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_GlobalFunctionInAssocBlk()
        {
            string code = @"
[Associative]
{
	def foo:double(x:int, y:double = 2.0)
	{
		return = x + y;
	}
	a = foo;
	b = foo(3); //b=5.0;
	c = foo(2, 4.0); //c = 6.0
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_MultipleIfStatement()
        {
            string code = @"
[Imperative]
{
 a=1;
 b=2;
 temp=1;
 
 if(a==1)
 {temp=temp+1;}
 
 if(b==2)  //this if statement is ignored
 {temp=4;}
 
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_SimpleRangeExpressionUsingCollection()
        {
            string code = @"
[Imperative]
{
	a = 3 ;
	b = 2 ;
	c = -1;
	w1 = a..b..-1 ; //correct  
	w2 = a..b..c; //correct 
	e1 = 1..2 ; //correct
	f = 3..4 ; //correct
	w3 = e1..f; //correct
	w4 = (3-2)..(w3[1][1])..(c+2) ; //correct
	w5 = (w3[1][1]-2)..(w3[1][1])..(w3[0][1]-1) ; //correct
}
/* expected results : 
    Updated variable a = 3
    Updated variable b = 2
    Updated variable c = -1
    Updated variable w1 = { 3, 2 }
    Updated variable w2 = { 3, 2 }
    Updated variable e1 = { 1, 2 }
    Updated variable f = { 3, 4 }
    Updated variable w3 = { { 1, 2, 3 }, { 2, 3, 4 } }
    Updated variable w4 = { 1, 2, 3 }
    Updated variable w5 = { 1, 2, 3 }
*/
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_TestAssignmentToUndefinedVariables_negative__2_()
        {
            string code = @"
[Associative]
{
    a = b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_TestAssignmentToUndefinedVariables_negative()
        {
            string code = @"
[Imperative]
{
    a = b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT03_TestImpInsideAssoc()
        {
            string code = @"
[Associative]
{
    x = 5.1;
    z = y;
    w = z * 2;
    [Imperative]
    {
        y = 5;
        z = x;
        x = 35;
        i = 3;
    }
    f = i;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

      

        [Test]
        public void DebugEQT03_Update_Function_Argument_Across_Language_Scope()
        {
            string code = @"
a = 1;
def foo ( a1 : int )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
[Imperative]
{
    a = 2.5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT040_AllFalse_Replication()
        {
            string code = @"
a = {
	{{0}},
	{false}
};
c = AllFalse(a);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT042_AllFalse_DynamicArray()
        {
            string code = @"
b = {};
a = {{true},{false},{false},
	{false,{true,false}}};
	
	i = 0;
	result2 = 
	[Imperative]
	{
		while(i<Count(a))
		{
			b[i] = AllFalse(a[i]);
			i = i+1;
		}
		return = b;
	}
	result = AllFalse(a);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT044_AllFalse_ImperativeAssociative()
        {
            string code = @"
[Imperative]
{
	a = {false||true};
	b = {""false""};
	c = a;
	a = {false};
	[Associative]
	{
		
		d = b;
		
		b = {false};
		
		m = AllFalse(c);//f
		n = AllFalse(d);//t
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT045_Defect_CountArray_1()
        {
            string code = @"
a = {0,1,null};
b = {m,{},a};
c={};
c[0] = 1;
c[1] = true;
c[2] = 0;
c[3] = 0;
a1 = Count(a);
b1 = Count(b);
c1 = Count(c);
result = {a1,b1,c1};
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT045_Defect_CountArray_2()
        {
            string code = @"
result=
[Imperative]
{
a = {};
b = a;
a[0] = b;
a[1] = ""true"";
c = Count(a);
a[2] = c;
return = Count(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT045_Defect_CountArray_3()
        {
            string code = @"
a = {};
b = {null,1+2};
a[0] = b;
a[1] = b[1];
result = Count(a);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT046_Sum_IfElse()
        {
            string code = @"
result = 
[Imperative]
{
	a = {1,2,3,4};
	b = {1.0,2.0,3.0,4.0};
	c = {1.0,2,3,4.0};
	d = {};
	e = {{1,2,3,4}};
	f = {true,1,2,3,4};
	g = {null};
	
	m= {-1,-1,-1,-1,-1,-1,-1};
	
	if(Sum(a)>=0) m[0] = Sum(a);	
	if(Sum(b)>=0) m[1] = Sum(b);
	if(Sum(c)>=0) m[2] = Sum(c);
	if(Sum(d)>=0) m[3] = Sum(d); 
	if(Sum(e)>=0) m[4] = Sum(e);
	if(Sum(f)>=0) m[5] = Sum(f);
	if(Sum(g)>=0) m[6] = Sum(g);
	
	return = m;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT047_Sum_ForLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {0,0.0};
	b = {{}};
	c = {m,Sum(a),b,10.0};
	
	d = {a,b,c};
	j = 0;
	
	for(i in d)
	{
		d[j] = Sum(i);
		j = j+1;
	}
	
	return = d; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT048_Sum_WhileLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {-2,0.0};
	b = {{}};
	c = {m,Sum(a),b,10.0};
	
	d = {a,b,c};
	j = 0;
	k = 0;
	e = {};
	
	while(j<Count(d))
	{
		if(Sum(d[j])!=0)
		{
			e[k] = Sum(d[j]);
			k = k+1;
		}
		j = j+1;
	}
	
	return = e; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT049_Sum_Function()
        {
            string code = @"
def foo(x:var[])
{
	return =
	[Imperative]
	{
		return = Sum(x);
	}
}
a = {-0.1,true,{},null,1};
b = {m+n,{{{1}}}};
c = {Sum(a),Sum(b)};
result = foo(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_Arithmatic_Single_List_And_Integer()
        {
            string code = @"
list1 = { 1, 2, 3, 4, 5 };
a = 5;
list2 = a + list1; // { 6, 7, 8, 9, 10 }
list3 = list1 + a; // { 6, 7, 8, 9, 10 }
list4 = a - list1; // { 4, 3, 2, 1, 0 }
list5 = list1 - a; // { -4, -3, -2, -1, 0 }
list6 = a * list1; // { 5, 10, 15, 20, 25 }
list7 = list1 * a; // { 5, 10, 15, 20, 25 }
list8 = a / list1; 
list9 = list1 / a; 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_Collection_Assignment_Using_Indexed_Values()
        {
            string code = @"
[Associative]
{
	a = { {1,2,3},{4,5,6} };
	
	b = { a[0], 4 };
	
	c = b[0];
	
	d = b[1];
	
	e = { a[0][0], a[0][1], a[1][0] };
	
}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_Defect_1454320_String()
        {
            string code = @"
[Associative]
{
str = ""hello world!"";
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_Defect_1467100_String()
        {
            string code = @"
def f(s : string)
{
    return = s;
}
x = f(""hello"");
//expected : x = ""hello""
//received : x = null
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_ForLoopBreakContinue()
        {
            string code = @"
[Imperative]
{
    sum = 0;
    for (x in {1, 2, 3, 4, 5, 6, 7, 8, 9, 10})
    {
        sum = sum + x;
        if (x <= 5)
            continue;
        sum = sum + 1;
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_Function_In_Nested_Scope()
        {
            string code = @"
[Associative]
{
    def foo : int( a:int, b : int )
    {
	   return = a * b;
	}
	a = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		a = foo( 2, 1 );
		return = a;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT04_IfStatementExpressions()
        {
            string code = @"
[Imperative]
{
 a=1;
 b=2;
 temp1=1;
 if((a/b)==0)
 {
  temp1=0;
  if((a*b)==2)
  { temp1=2;
  }
 } 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT051_Sum_Inline()
        {
            string code = @"
a = {1,{2,-3.00}};//0.0
sum = Sum(a);
b = Sum(a) -1;//-1.0
c = Sum({a,b,-1});//-2.0;
result = Sum(a)==0&& b==-1.00? b :c;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT052_Sum_RangeExpression()
        {
            string code = @"
result = 
[Imperative]
{
	a1 = {1,true, null};//1
	a2 = 8;
	a3 = {2,{true,{true,1.0}},{false,x, true}};//3.0
	a = Sum(a1)..a2..Sum(a3);//{1,4,7}
	
	return = Sum(a);//12.0
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT053_Sum_Replication()
        {
            string code = @"
a = {1,2,3};
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT054_Sum_DynamicArr()
        {
            string code = @"
a = {};
b = {1.0,2,3.0};
c = {null,m,""1""};
a[0]=Sum(b);//6.0
a[1] = Sum(c);//0
a[2] = Sum({a[0],a[1]});//6.0
result = Sum(a);//12.0
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        public void DebugEQT056_Sum_AssociativeImperative()
        {
            string code = @"
a = {1,0,0.0};
b = {2.0,0,true};
b1 = {b,1}
[Imperative]
{
	c = a[2];
	a[1] = 1;
	m = a;
	sum1 = Sum({c});//0.0
	[Associative]
	{
		 b[1] = 1;
		 sum2 = Sum( b1);////4.0
	}
	
	a[2]  =1;
	sum3 = Sum({c});//0.0
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT057_Average_DataType_01()
        {
            string code = @"
a = {};
b = {1,2,3};
c = {0.1,0.2,0.3,1};
d = {true, false, 1};
a1 = Average(a);
b1 = Average(b);
c1 = Average(c);
d1 = Average(d);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT059_Defect_Flatten_RangeExpression()
        {
            string code = @"
a = 0..10..5;
b = 20..30..2;
c = {a, b};
d = Flatten({a,b});
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT059_Defect_Flatten_RangeExpression_1()
        {
            string code = @"
a = {{null}};
b = {1,2,{3}};
c = {a,b};
d = Flatten(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT05_Function_outside_Any_Block()
        {
            string code = @"
def foo : int( a:int, b : int )
{
    return = a * b;
}
a = 3.5;
b = 3.5;
[Associative]
{
	a = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		a = foo( 2, 1 );
		return = a;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT05_Logic_List_And_List_Different_Value()
        {
            string code = @"
list1 = { 1, 8, 10, 4, 7 };
list2 = { 2, 6, 10, 3, 5, 20 };
list3 = list1 > list2; // { false, true, false, true, true }
list4 = list1 < list2;	// { true, false, false, false, false }
list5 = list1 >= list2; // { false, true, true, true, true }
list6 = list1 <= list2; // { true, false, true, false, false }
list9 = { true, false, true };
list7 = list9 && list5; // { false, false, true }
list8 = list9 || list6; // { true, false, true }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT05_RangeExpressionWithIncrement()
        {
            string code = @"
[Imperative]
{
	d = 0.9..1..0.1;
	e1 = -0.4..-0.5..-0.1;
	f = -0.4..-0.3..0.1;
	g = 0.4..1..0.2;
	h = 0.4..1..0.1;
	i = 0.4..1;
	j = 0.6..1..0.4;
	k = 0.09..0.1..0.01;
	l = 0.2..0.3..0.05;
	m = 0.05..0.1..0.04;
	n = 0.1..0.9..~0.3;
	k = 0.02..0.03..#3;
	l = 0.9..1..#5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT05_TestForLoopInsideNestedBlocks()
        {
            string code = @"
[Associative]
{
	a = { 4, 5 };
	[Imperative]
	{
		x = 0;
		b = { 2,3 };
		for( y in b )
		{
			x = y + x;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       

        [Test]
        public void DebugEQT05_TestRepeatedAssignment()
        {
            string code = @"
[Associative]
{
    b = a = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT05_TestRepeatedAssignment_negative__2_()
        {
            string code = @"
[Associative]
{
    b = a = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT05_TestRepeatedAssignment_negative()
        {
            string code = @"
[Imperative]
{
    b = a = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT060_Average_ForLoop()
        {
            string code = @"
result = 
[Imperative]
{
	a = {};
	b = {1,{2},{{2},1}};
	c = {true, false, null, 10};
	d = {a,b,c};
	
	e = {};
	j = 0;
	
	for(i in d)
	{
		e[j] = Average(i);
		 j = j+1;
		
	}
	return = e;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT061_Average_Function()
        {
            string code = @"
def foo : double (x :var[]..[])
{
	
	return = Average(x);
}
a = {1,2,2,1};
b = {1,{}};
c = Average(a);
result = {foo(a),foo(b)};
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT063_Average_Inline()
        {
            string code = @"
a = {1.0,2};
b = {{0},1.0,{2}};
result = Average(a)>Average(b)?true:false;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT064_Average_RangeExpression()
        {
            string code = @"
a = 0..6..3;//0,3,6
b = 0..10..~3;//0,3.3,6.6,10
m = Average(a);//3
n = Average(b);//5.0
c = Average({m})..Average({n});//3.0,4.0,5.0
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT066_Print_String()
        {
            string code = @"
r1 = Print(""Hello World"");
str = ""Hello World!!"";
r2 = Print(str);
a = 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT067_Print_Arr()
        {
            string code = @"
arr = { 0, 1 ,2};
r1 = Print(arr);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT06_Function_Imp_Inside_Assoc()
        {
            string code = @"
[Associative]
{
	def foo : int( a:int, b : int )
	{
		return = a * b;
	}
	a = 3.5;
	b = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		c = foo( 2, 1 );
		return = c;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT06_InsideNestedBlock()
        {
            string code = @"
[Associative]
{
	a = 4;
	b = a*2;
	temp = 0;
	[Imperative]
	{
		i=0;
		temp=1;
		while(i<=5)
		{
	      i=i+1;
		  temp=temp+1;
		}
    }
	a = temp;
      
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT06_Logic_List_And_List_Same_Length()
        {
            string code = @"
list1 = { 1, 8, 10, 4, 7 };
list2 = { 2, 6, 10, 3, 5 };
list3 = list1 > list2; // { false, true, false, true, true }
list4 = list1 < list2;	// { true, false, false, false, false }
list5 = list1 >= list2; // { false, true, true, true, true }
list6 = list1 <= list2; // { true, false, true, false, false }
list7 = list3 && list5; // { false, true, false, true, true }
list8 = list4 || list6; // { true, false, true, false, false }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT06_RangeExpressionWithIncrement()
        {
            string code = @"
[Imperative]
{
	a = 0.3..0.1..-0.1;
	b = 0.1..0.3..0.2;
	c = 0.1..0.3..0.1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT06_TestInsideNestedBlocksUsingCollectionFromAssociativeBlock()
        {
            string code = @"
[Associative]
{
	a = { 4,5 };
	b =[Imperative]
	{
	
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
		return = x;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT07_BreakStatement()
        {
            string code = @"
[Imperative]
{
		i=0;
		temp=0;
		while( i <= 5 )
		{ 
	      i = i + 1;
		  if ( i == 3 )
		      break;
		  temp=temp+1;
		}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_Collection_Assignment_In_Function_Scope()
        {
            string code = @"
def collection :int[] ( a :int[] , b:int , c:int )
{
	a[1] = b;
	a[2] = c;
	return= a;
}
	a = { 1,0,0 };
	[Imperative]
	{
		a = collection( a, 2, 3 );
	}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_Logic_Mixed()
        {
            string code = @"
list1 = { 1, 5, 8, 3, 6 };
list2 = { 4, 1, 6, 3 };
list3 = (list1 > 1) && (list2 > list1) || (list2 < 5); // { true, true, false , true }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_ScopeVariableInBlocks()
        {
            string code = @"
[Imperative]
{
	a = 4;
	b = a*2;
	temp = 0;
	if(b==8)
	{
		i=0;
		temp=1;
		if(i<=a)
		{
		  temp=temp+1;
		}
    }
	a = temp;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_String_Replication()
        {
            string code = @"
a = ""a"";
bcd = {""b"",""c"",""d""};
r = a +bcd;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_String_Replication_1()
        {
            string code = @"
a = {""a""};
bc = {""b"",""c""};
str = a + bc;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_String_Replication_2()
        {
            string code = @"
a = ""a"";
b = {{""b""},{""c""}};
str = a +b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT07_TestForLoopUsingLocalVariable()
        {
            string code = @"
[Imperative]
{
	a = { 1, 2, 3, 4, 5 };
	x = 0;
	for( y in a )
	{
		local_var = y + x;	
        x = local_var + y;		
	}
	z = local_var;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_TestOutsideBlock__2_()
        {
            string code = @"
b = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_TestOutsideBlock()
        {
            string code = @"
b = 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT07_Update_Array_Variable()
        {
            string code = @"
a = 1..3;
c = a;
b = [ Imperative ]
{
    count = 0;
	for ( i in a )
	{
	    a[count] = i + 1;
		count = count+1;
	}
	return = a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_Collection_Assignment_In_Function_Scope_2()
        {
            string code = @"
def foo ( a )
{
	return= a;
}
	a = { 1, foo( 2 ) , 3 };
	
	[Imperative]
	{
		b = { foo( 4 ), 5, 6 };
	}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_ContinueStatement()
        {
            string code = @"
[Imperative]
{
		i = 0;
		temp = 0;
		while ( i <= 5 )
		{
		  i = i + 1;
		  if( i <= 3 )
		  {
		      continue;
	      }
		  temp=temp+1;
		 
		}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_FunctionPointerUpdateTest()
        {
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
def foo2:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo1;
b = a(3);
a = foo2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_Logic_Single_List_And_Value()
        {
            string code = @"
list1 = { 1, 2, 3, 4, 5 };
a = 3;
list2 = a > list1; // { true, true, false, false, false }
list3 = list1 > a; // { false, false, false, true, true }
list4 = a >= list1; // { true, true, true, false, false }
list5 = list1 >= a; // { false, false, true, true, true }
list6 = a < list1; // { false, false, false, true, true }
list7 = list1 < a; // { true, true, false, false, false }
list8 = a <= list1; // { false, false, true, true, true }
list9 = list1 <= a; // { true, true, true, false, false }
list10 = list2 && true; // { true, true, false, false, false }
list11 = false && list2; // { false, false, false, false, false }
list12 = list2 || true; // { true, true, true, true, true }
list13 = false || list2; // { true, true, false, false, false }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_NestedBlocks()
        {
            string code = @"
[Associative]
{
	a = 4;
	
	[Imperative]
	{
		i=10;
		temp=1;
		if(i>=-2)
		{
		  temp=2;
		}
    }
	b=2*a;
	a=2;
              
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_String_Inline()
        {
            string code = @"
a = ""a"";
b = ""b"";
r = a>b? a:b;
r1 = a==b? ""Equal"":""!Equal"";
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_String_Inline_2()
        {
            string code = @"
a = ""a"";
b = ""b"";
r = a>b? a:b;
r1 = a==b? ""Equal"":""!Equal"";
b = ""a"";
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_TestCyclicReference__2_()
        {
            string code = @"
[Associative]
{
	a = 2;
        b = a *3;
        a = 6.5;
        a = b / 3; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_TestCyclicReference()
        {
            string code = @"
[Imperative]
{
	a = 2;
        b = a *3;
        a = 6.5;
        a = b / 3; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT08_Update_Array_Variable()
        {
            string code = @"
a = 1..3;
c = a;
b = [ Imperative ]
{
    count = 0;
	for ( i in a )
	{
	    if ( i > 0 )
		{
		    a[count] = i + 1;
		}
		count = count+1;
	}
	return = a;
}
d = [ Imperative ]
{
    count2 = 0;
	while (count2 <= 2 ) 
	{
	    if ( a[count2] > 0 )
		{
		    a[count2] = a[count2] + 1;
		}
		count2 = count2+1;
	}
	return = a;
}
e = b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_Defect_1449829()
        {
            string code = @"
[Associative]
{ 
 a = 2;
[Imperative]
{   
	b = 1;
    if(a == 2 )
	{
	b = 2;
    }
    else 
    {
	b = 4;
    }
}
}
  
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_NegativeTest_Non_FunctionPointer()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = 2;
b = a();
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_NestedIfElseInsideWhileStatement()
        {
            string code = @"
[Imperative]
{
		i=0;
		temp=0;
		while(i<=5)
		{ 
			i=i+1;
			if(i<=3)
			{
				temp=temp+1;
			}		  
			elseif(i==4)
			{
				temp = temp+1;
				if(temp==i) 
				{
					temp=temp+1;
				}			
			}
			else 
			{
				if (i==5)
				{ temp=temp+1;
				}
			}
		}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_NestedWhileStatement()
        {
            string code = @"
[Imperative]
{
	i = 1;
	a = 0;
	p = 0;
	
	temp = 0;
	
	while( i <= 5 )
	{
		a = 1;
		while( a <= 5 )
		{
			p = 1;
			while( p <= 5 )
			{
				temp = temp + 1;
				p = p + 1;
			}
			a = a + 1;
		}
		i = i + 1;
	}
}  
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_Replication_On_Operators_In_Range_Expr()
        {
            string code = @"
[Imperative]
{
	z5 = 4..1; // { 4, 3, 2, 1 }
	z2 = 1..8; // { 1, 2, 3, ... , 6, 7, 8 }
	z6 = z5 - z2 + 0.3;  // { 3.3, 1.3, -1.7, -2.7 }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_String_DynamicArr()
        {
            string code = @"
a[1] = foo(""1"" + 1);
a[2] = foo(""2"");
a[10] = foo(""10"");
a[ - 2] = foo("" - 2"");//smart formatting
r = 
[Imperative]
{
    i = 5;
    while(i < 7)
    {
        a[i] = foo(""whileLoop"");
        i = i + 1;
    }
    return = a;
}
def foo(x:var)
{
    return = x + ""!!"";
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_TestForLoopWithBreakStatement()
        {
            string code = @"
[Imperative]
{
	a = { 1,2,3 };
	x = 0;
	for( i in a )
	{
		x = x + 1;
		break;
	}	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_TestInNestedBlock__2_()
        {
            string code = @"
[Associative]
{
	a = 4;
	b = a + 2;
	[Imperative]
	{
		b = 0;
		c = 0;
		if ( a == 4 )
		{
			b = 4;
		}			
		else
		{
			c = 5;
		}
		d = b;
		e = c;	
        g2 = g1;	
	}
	f = a * 2;
    g1 = 3;
    g3 = g2;
      
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT09_TestInNestedBlock()
        {
            string code = @"
[Imperative]
{
	a = 4;
	b = a + 2;
    [Associative]
    {
        [Imperative]
        {
            b = 0;
            c = 0;
            if ( a == 4 )
            {
                b = 4;
            }			
            else
            {
                c = 5;
            }
            d = b;
            e = c;	
            g2 = g1;	
        }
    }
	f = a * 2;
    g1 = 3;
    g3 = g2;
      
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
        [Test]
        public void DebugEQT09_Update_Across_Multiple_Imperative_Blocks()
        {
            string code = @"
a = 1;
b = a;
c = [ Imperative ]
{
    a = 2;
	return = a;
}
d = [ Imperative ]
{
    a = 3;
	return = a;
}
e = c;
f = d;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT10_2D_Collection_Assignment_In_Function_Scope()
        {
            string code = @"
	def foo( a:int[] )
	{
		a[0][0] = 1;
		return= a;
	}
	b = { {0,2,3}, {4,5,6} };
	d = foo( b );
	c = d[0];
		
		
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT10_TestInFunctionScope__2_()
        {
            string code = @"
[Associative]
{
	 def add:double( n1:int, n2:double )
	 {
		  
		  return = n1 + n2;
	 }
	 test = add(2,2.5);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT10_TestNestedForLoops()
        {
            string code = @"
[Imperative]
{
	a = { 1,2,3 };
	x = 0;
	for ( i in a )
	{
		for ( j in a )
        {
			x = x + j;
		}
	}
}	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT10_TypeConversion()
        {
            string code = @"
[Imperative]
{
    temp = 0;
    a=4.0;
    if(a==4)
        temp=1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT10_Update_Array_Across_Multiple_Imperative_Blocks()
        {
            string code = @"
a = 1..3;
b = a;
c = [Imperative ]
{
    x = { 10, a[1], a[2] };
	a[0] = 10;
	return = x;
}
d = [ Imperative ]
{
    a[1] = 20;
	return = a;
}
e = c;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT11_2D_Collection_Assignment_Heterogeneous()
        {
            string code = @"
[Imperative]
{
	a = { {1,2,3}, {4}, {5,6} };
	b = a[1];
	a[1] = 2;
	a[1] = a[1] + 1;
	a[2] = {7,8};
	c = a[1];
	d = a[2][1];
}	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT11_String_Imperative()
        {
            string code = @"
r =
[Imperative]
{
    a = ""a"";
    b = a;
    
}
c = b;
b = ""b1"";
a = ""a1"";
m = ""m"";
n = m;
n = ""n"";
m = m+n;
//a =""a1""
//b = ""b1""
//c = ""b1"";
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT11_TestForLoopWithSingleton()
        {
            string code = @"
[Imperative]
{
	a = {1};
	b = 1;
	x = 0;
 
	for ( y in a )
	{
		x = x + 1;
	}
 
	for ( y in b )
	{
		x = x + 1;
	}
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT11_Update_Undefined_Variables()
        {
            string code = @"
b = a;
[Imperative]
{
    a = 3;
}
[Associative]
{
    a = 4;	
}
c = b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT11_WhilewithLogicalOperators()
        {
            string code = @"
[Imperative]
{
		i1 = 5;
		temp1 = 1;
		while( i >= 2) 
		{ 
	        i1=i1-1;
		    temp1=temp1+1;
		}
		
		i2 = 5;
		temp2 = 1;
		while ( i2 != 1 )
		{
		    i2 = i2 - 1;
		    temp2 = temp2 + 1;
		}
         
		temp3 = 2;
        while( i2 == 1 )
		{
		     temp3 = temp3 + 1;
		     i2 = i2 - 1;
		} 
		while( ( i2 == 1 ) && ( i1 == 1 ) )  
        {
             temp3=temp3+1;
		     i2=i2-1;
        }
		temp4 = 3;
		while( ( i2 == 1 ) || ( i1 == 5 ) )
        {
            i1 = i1 - 1;		
            temp4 = 4;
        }       
 
}		
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT12_Collection_Assignment_Block_Return_Statement()
        {
            string code = @"
a;b;c1;c2;
[Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = { 1,2,3 };
		if( c[1] <= 3 )
		return= c;
	}
	
	b[2] = 4;
	a = b;
	c1 = a[1];
	c2 = a[2];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT12_Function_From_Inside_Function()
        {
            string code = @"
def add_1 : double( a:double )
{
	return = a + 1;
}
[Associative]
{
	def add_2 : double( a:double )
	{
		return = add_1( a ) + 1;
	}
	
	a = 1.5;
	b = add_2 (a );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT12_RangeExpressionUsingNestedRangeExpressions()
        {
            string code = @"
[Imperative]
{
	x = 1..5..2; // {1,3,5}
	y = 0..6..2; // {0,2,4,6}
	a = (3..12..3)..(4..16..4); // {3,6,9,12} .. {4..8..12..16}
	b = 3..00.6..#5;      // {3.0,2.4,1.8,1.2,0.6}
	//c = b[0]..7..#1;    //This indexed case works
	c = 5..7..#1;         //Compile error here , 5
	d = 5.5..6..#3;       // {5.5,5.75,6.0}
	e1 = -6..-8..#3;      //{-6,-7,-8}
	f = 1..0.8..#2;       //{1,0.8}
	g = 1..-0.8..#3;      // {1.0,0.1,-0.8}
	h = 2.5..2.75..#4;    //{2.5,2.58,2.67,2.75}
	i = x[0]..y[3]..#10;//1..6..#10
	j = 1..0.9..#4;// {1.0, 0.96,.93,0.9}
	k= 1..3..#0;//null
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT12_TestForLoopWith2DCollection()
        {
            string code = @"
[Imperative]
{
	a = {{1},{2,3},{4,5,6}};
	x = 0;
	i = 0;
    for (y in a)
	{
		x = x + y[i];
	    i = i + 1;	
	}
	z = 0;
    for (i1 in a)
	{
		for(j1 in i1)
		{
		    z = z + j1;
		}
	}
			
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT12_TestUsingMathAndLogicalExpr()
        {
            string code = @"
[Imperative]
{
  e = 0;
  a = 1 + 2;
  b = 0.1 + 1.9;
  b = a + b;
  c = b - a - 1;
  d = a + b -c;
  if( c < a )
  {
     e = 1;
  }
  else
  {
    e = 2;
  }
  if( c < a || b > d)
  {
     e = 3;
  }
  else
  {
    e = 4;
  }
  if( c < a && b > d)
  {
     e = 3;
  }
  else
  {
    e = 4;
  }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT12_Update_Undefined_Variables()
        {
            string code = @"
b = a;
[Imperative]
{
    a = 3;
}
[Associative]
{
    a = 4;
    d = b + 1;	
}
c = b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT13_2D_Collection_Assignment_Block_Return_Statement()
        {
            string code = @"
a;
b;
c1;c2;
[Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = { { 1,2,3 } , { 4,5,6 } } ;
		return= c;
	}
	
	b[0][0] = 0;
	a = b;
	c1 = a[0];
	c2 = a[1][2];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT13_Defect_1450527()
        {
            string code = @"
[Associative]
{
	a = 1;
	temp=0;
	[Imperative]
	{
	    i = 0;
	    if(i <= a)
	    {
	        temp = temp + 1;
	    }
	}
	a = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT13_IfElseIf()
        {
            string code = @"
[Imperative]
{
 a1 = -7.5;
 
 temp1 = 10.5;
 
 if( a1>=10.5 )
 {
 temp1 = temp1 + 1;
 }
 
 elseif( a1<2 )
 {
 temp1 = temp1 + 2;
 }
 
  
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT13_TestForLoopWithNegativeAndDecimalCollection()
        {
            string code = @"
[Imperative]
{
	a = { -1,-3,-5 };
	b = { 2.5,3.5,4.2 };
	x = 0;
	y = 0;
    for ( i in a )
	{
		x = x + i;
	}
	
	for ( i in b )
	{
		y = y + i;
	}
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT13_TestUsingMathAndLogicalExpr__2_()
        {
            string code = @"
[Associative]
{
  a = 3.5;
  b = 1.5;
  c = a + b; 
  d = a - c;
  e = a * d;
  f = a / e; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT13_TestUsingMathAndLogicalExpr()
        {
            string code = @"
[Imperative]
{
  a = 3.5;
  b = 1.5;
  b = a + b; 
  b = a - b;
  b = a * b;
  b = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
        [Test]
        public void DebugEQT13_Update_Variables_Across_Blocks()
        {
            string code = @"
a = 3;
b = a * 3;
c = [Imperative]
{
    d = b + 3;
	a = 4;
	return = d;
}
f = c + 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_2D_Collection_Assignment_Using_For_Loop()
        {
            string code = @"
pts = {{0,1,2},{0,1,2}};
x = {1,2};
y = {1,2,3};
[Imperative]
{
    c1 = 0;
	for ( i in x )
	{
		c2 = 0;
		for ( j in y )
		{
		    pts[c1][c2] = i+j;
			c2 = c2+1;
		}
		c1 = c1 + 1;
	}
	
}
p1 = pts[1][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_IfElseStatementExpressions()
        {
            string code = @"
[Imperative]
{
 a=1;
 b=2;
 temp1=1;
 if((a/b)==1)
 {
  temp1=0;
 }
 elseif ((a*b)==2)
 { temp1=2;
 }
 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = foo + 2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_TestFactorialUsingWhileStmt()
        {
            string code = @"
[Imperative]
{
    a = 1;
	b = 1;
    while( a <= 5 )
	{
		a = a + 1;
		b = b * (a-1) ;		
	}
	factorial_a = b * a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_TestForLoopWithBooleanCollection()
        {
            string code = @"
[Imperative]
{ 
	a = { true, false, true, true };
	x = false;
	
	for( i in a )
	{
	    x = x + i;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_TestUsingMathAndLogicalExpr__2_()
        {
            string code = @"
[Associative]
{
  a = 3;
  b = -4;
  c = a + b; 
  d = a - c;
  e = a * d;
  f = a / e; 
  
  c1 = 1 && 2;
  c2 = 1 && 0;
  c3 = null && true;
  
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT14_TestUsingMathAndLogicalExpr()
        {
            string code = @"
[Imperative]
{
  a = 3;
  b = -4;
  b = a + b; 
  b = a - b;
  b = a * b;
  b = a / b; 
  
  c1 = 1 && 2;
  c2 = 1 && 0;
  c3 = null && true;
  
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_2D_Collection_Assignment_Using_While_Loop()
        {
            string code = @"
[Imperative]
{
	pts = {{0,1,2},{0,1,2}};
	x = {1,2,3};
	y = {1,2,3};
    i = 0;
	while ( i < 2 )
	{		
		j = 0;
		while ( j  < 3 )
		{
		    pts[i][j] = i+j;
			j = j + 1;
		}
		i = i + 1;
	}
	p1 = pts[1][1];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_Defect_1452044()
        {
            string code = @"
[Associative]
{
	a = 2;
	[Imperative]
	{
		b = 2 * a;
	}
		
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_Defect_1460935_3()
        {
            string code = @"
x = 1;
y = x;
x = true; //if x = false, the update mechanism works fine
yy = y;
x = false;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_SimpleRangeExpression_1()
        {
            string code = @"
[Imperative]
{
	a = 1..2.2..#3;
	b = 0.1..0.2..#4;
	c = 1..3..~0.2;
	d = (a[0]+1)..(c[2]+0.9)..0.1; 
	e1 = 6..0.5..~-0.3;
	f = 0.5..1..~0.3;
	g = 0.5..0.6..0.01;
	h = 0.51..0.52..0.01;
	i = 0.95..1..0.05;
	j = 0.8..0.99..#10;
	//k = 0.9..1..#1;
	l = 0.9..1..0.1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_TestEmptyIfStmt()
        {
            string code = @"
[Imperative]
{
 a = 0;
 b = 1;
 if(a == b);
 else a = 1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_TestForLoopWithMixedCollection()
        {
            string code = @"
[Imperative]
{
	a = { -2, 3, 4.5 };
	x = 1;
	for ( y in a )
	{
		x = x * y;       
    }
	
	a = { -2, 3, 4.5, true };
	y = 1;
	for ( i in a )
	{
		y = i * y;       
    }
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT15_TestWhileWithDecimalvalues()
        {
            string code = @"
[Imperative]
{
    a = 1.5;
	b = 1;
    while(a <= 5.5)
	{
		a = a + 1;
		b = b * (a-1) ;		
	}
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        

        [Test]
        public void DebugEQT16_Defect_1460623()
        {
            string code = @"
a2 = 1.0;
test2 = a2;
a2 = 3.0;
a2 = 3.3;
t2 = test2; // expected : 3.3; recieved : 3.0
a1 = { 1.0, 2.0};
test1 = a1[1]; 
a1[1] = 3.0;
a1[1] = 3.3;
t1 = test1; // expected : 3.3; recieved : 3.0
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT16_Defect_1460623_2()
        {
            string code = @"
def foo ( a )
{
    return = a;
}
x = 1;
y = foo (x );
x = 2;
x = 3;
[Imperative]
{
    x = 4;
}
z = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT16_Defect_1460623_3()
        {
            string code = @"
def foo ( a )
{
    x = a;
	y = x + 3;
	x = a + 1;
	x = a + 2;
	return = y;
}
x = 1;
y = foo (x );
[Imperative]
{
    x = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT16_SimpleRangeExpression_2()
        {
            string code = @"
[Imperative]
{
	a = 1.2..1.3..0.1;
	b = 2..3..0.1;
	c = 1.2..1.5..0.1;
	//d = 1.3..1.4..~0.5; //incorrect 
	d = 1.3..1.4..0.5; 
	e1 = 1.5..1.7..~0.2;
	f = 3..3.2..~0.2;
	g = 3.6..3.8..~0.2; 
	h = 3.8..4..~0.2; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT16_TestForLoopInsideIfElseStatement()
        {
            string code = @"
[Imperative]
{
	a = 1;
	b = { 2,3,4 };
	if( a == 1 )
	{
		for( y in b )
		{
			a = a + y;
		}
	}
	
	else if( a !=1)
	{
		for( y in b )
		{
			a = a + 1;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT16_TestIfConditionWithNegation_Negative()
        {
            string code = @"
[Imperative]
{
    a = 3;
    b = -3;
	if ( a == !b )
	{
	    a = 4;
	}
	
}
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT16_TestWhileWithLogicalOperators()
        {
            string code = @"
[Imperative]
{
    a = 1.5;
	b = 1;
    while(a <= 5.5 && b < 20)
	{
		a = a + 1;
		b = b * (a-1) ;		
	}	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT16__Defect_1452588()
        {
            string code = @"
[Imperative]
{
	a = { 1,2,3,4,5 };
	for( y in a )
	{
		x = 5;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_Assigning_Collection_And_Updating()
        {
            string code = @"
a = {1, 2, 3};
b = a;
b[0] = 100;
t = a[0];       // t = 100, as expected
      
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_Defect_1459759_2()
        {
            string code = @"
a1 = { 1, 2 };
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_Function_From_Parallel_Blocks()
        {
            string code = @"
[Associative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	
	
}
[Associative]
{
	a = 3;
	b = foo (a );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_PassFunctionPointerAsArg()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
def foo1:int(f:function, x:int)
{
	return = f(x);
}
a = foo1(foo, 2);
b = foo;
c = foo1(b, 3);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_SimpleRangeExpression_3()
        {
            string code = @"
[Imperative]
{
	a = 1..2.2..~0.2;
	b = 1..2..#3;
	c = 2.3..2..#3;
	d = 1.2..1.4..~0.2;
	e1 = 0.9..1..0.1;
	f = 0.9..0.99..~0.01;
	g = 0.8..0.9..~0.1;
	h = 0.8..0.9..0.1;
	i = 0.9..1.1..0.1;
	j = 1..0.9..-0.05;
	k = 1.2..1.3..~0.1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_TestForLoopInsideNestedIfElseStatement()
        {
            string code = @"
[Imperative]
{
	a = 1;
	b = { 2,3,4 };
	c = 1;
	if( a == 1 )
	{
		if(c ==1)
		{
			for( y in b )
			{
				a = a + y;
			}
		}	
	}
	
	else if( a !=1)
	{
		for( y in b )
		{
			a = a + 1;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }



        [Test]
        public void DebugEQT17_TestWhileWithBool()
        {
            string code = @"
[Imperative]
{
    a = 0;	
    while(a == false)
	{
		a = 1;	
	}	
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17_WhileInsideElse()
        {
            string code = @"
[Imperative]
{
	i=1;
	a=3;
    temp=0;
	if(a==4)             
	{
		 i = 4;
	}
	else
	{
		while(i<=4)
		 {
			  if(i>10) 
				temp=4;			  
			  else 
				i=i+1;
		 }
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT17__Defect_1452588_2()
        {
            string code = @"
[Imperative]
{
	a = 1;
	
	if( a == 1 )
	{
		if( a + 1 == 2)
			b = 2;
	}
	
	c = a;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_Assigning_Collection_In_Function_And_Updating()
        {
            string code = @"
def A (a: int [])
{
    return = a;
}
val = {1,2,3};
b = A(val);
t = b;
t[0] = 100;    // 
y = b[0];
z = val[0];    // val[0] is still 1
      
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_FunctionPointerAsReturnVal()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
def foo1:int(f : function, x:int)
{
	return = f(x);
}
def foo2:function()
{
	return = foo;
}
a = foo2();
b = a(2);
c = foo1(a, 3);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_SimpleRangeExpression_4()
        {
            string code = @"
[Imperative]
{
	a = 2.3..2.6..0.3;
	b = 4.3..4..-0.3;
	c= 3.7..4..0.3;
	d = 4..4.3..0.3;
	e1 = 3.2..3.3..0.3;
	f = 0.4..1..0.1;
	g = 0.4..0.45..0.05;
	h = 0.4..0.45..~0.05; 
	g = 0.4..0.6..~0.05;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_TestForLoopInsideWhileStatement()
        {
            string code = @"
[Imperative]
{
	a = 1;
	b = { 1,1,1 };
	x = 0;
	
	if( a == 1 )
	{
		while( a <= 5 )
		{
			for( i in b )
			{
				x = x + 1;
			}
			a = a + 1;
		}
	}
}
			
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_TestMethodCallInExpr__2_()
        {
            string code = @"
[Associative]
{
	def mul : double ( n1 : int, n2 : int )
    {
      	return = n1 * n2;
    }
    def add : double( n1 : int, n2 : double )
    {
       	return = n1 + n2;
    }
    test0 = add (-1 , 7.5 ) ;
    test1 = add (mul(1,2), 4.5 ) ;  
    test2 = add (mul(1,2.5), 4 ) ; 
    test3 = add (add(1.5,0.5), 4.5 ) ;  
    test4 = add (1+1, 4.5 ) ;
    test5 = add ( add(1,1) + add(1,0.5), 3.0 ) ;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_TestWhileWithNull()
        {
            string code = @"
[Imperative]
{
    a = null;
    c = null;
	
    while(a == 0)
	{
		a = 1;	
	}
    while(null == c)
	{
		c = 1;	
	}
    while(a == b)
	{
		a = 2;	
	}	
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
        [Test]
        public void DebugEQT18_Update_Variables_In_Inner_Assoc()
        {
            string code = @"
c = 2;
b = c * 2;
x = b;
[Imperative]
{
    c = 1;
	b = c + 1;
	d = b + 1;
	y = 1;
	[Associative]
	{
	  	b = c + 2;		
		c = 4;
		z = 1;
	}
}
b = c + 3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT18_WhileInsideIf()
        {
            string code = @"
[Imperative]
{
	i=1;
	a=3;
    temp=0;
	if(a==3)             //when the if statement is removed, while loop works fine, otherwise runs only once
	{
		 while(i<=4)
		 {
			  if(i>10) 
				temp=4;			  
			  else 
				i=i+1;
		 }
	}
}
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT19_Assigning_Collection_In_Function_And_Updating()
        {
            string code = @"
def A (a: int [])
{
    return = a;
}
val = {1,2,3};
b = A(val);
b[0] = 100;     
z = val[0];     
      
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT19_BasicIfElseTestingWithNumbers()
        {
            string code = @"
[Imperative]
{
    a = 0;
    b = 0;
    c = 0;
    d = 0;
    if(1)
	{
		a = 1;
	}
	else
	{
		a = 2;
	}
	
	
	if(0)
	{
		b = 1;
	}
	else
	{
		b = 2;
	}
	
	if(0)
	{
		c = 1;
	}
	elseif(1)
	{
		c = 3;
	}
	
	if(0)
	{
		d = 1;
	}
	elseif(0)
	{
		d = 2;
	}
	else
	{
		d = 4;
	}
		
} 
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT19_SimpleRangeExpression_5()
        {
            string code = @"
[Imperative]
{
	//a = 0.1..0.2..#1; //giving error
	b = 0.1..0.2..#2;
	c = 0.1..0.2..#3;
	d = 0.1..0.1..#4;
	e1 = 0.9..1..#5;
	f = 0.8..0.89..#3;
	g = 0.9..0.8..#3;
	h = 0.9..0.7..#5;
	i = 0.6..1..#4;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT19_TestAssignmentToCollection__2_()
        {
            string code = @"
[Associative]
{
	a = {{1,2},3.5};
	c = a[1];
	d = a[0][1];
        a[0][1] = 5;
       	b = a[0][1] + a[1];	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT19_TestAssignmentToCollection()
        {
            string code = @"
[Imperative]
{
	a = {{1,2},3.5};
	c = a[1];
	d = a[0][1];
        a[0][1] = 5;
       	b = a[0][1] + a[1];
        a = 2;		
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT19_TestForLoopInsideNestedWhileStatement()
        {
            string code = @"
[Imperative]
{
	i = 1;
	a = {1,2,3,4,5};
	x = 0;
	
	while( i <= 5 )
	{
		j = 1;
		while( j <= 5 )
		{
			for( y in a )
			{
			x = x + 1;
			}
			j = j + 1;
		}
		i = i + 1;
	}
}	
		
			
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT19_TestWhileWithIf()
        {
            string code = @"
[Imperative]
{
    a = 2;
	b = a;
	while ( a <= 4)
	{
		if(a < 4)
		{
			b = b + a;
		}
		else
		{
			b = b + 2*a;
		}
		a = a + 1;
	}
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_BasicIfElseTestingWithNumbers()
        {
            string code = @"
[Imperative]
{
    a = 0;
    b = 0;
    c = 0;
    d = 0;
    e = 0;
    f = 0;
    if(1.5)
	{
		a = 1;
	}
	else
	{
		a = 2;
	}
	
	
	if(-1)
	{
		b = 1;
	}
	else
	{
		b = 2;
	}
	
	if(0)
	{
		c = 1;
	}
	elseif(20)
	{
		c = 3;
	}
	
	if(0)
	{
		d = 1;
	}
	elseif(0)
	{
		d = 2;
	}
	else
	{
		d = 4;
	}
	
	if(true)
	{
		e = 5;
	}
	
	if(false)
	{
		f = 1;
	}
	else
	{
		f = 6;
	}
		
} 
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_Defect_1458567()
        {
            string code = @"
a = 1;
b = a[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_Defect_1461391()
        {
            string code = @"
a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
[Imperative]
{
    a = 2.5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_Defect_1461391_2()
        {
            string code = @"
a = 1;
def foo ( a1 : double[] )
{
    return = a1[0] + a1[1];
}
b = foo ( c ) ;
c = { a, a };
[Imperative]
{
    a = 2.5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_Defect_1461391_3()
        {
            string code = @"
a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( a ) ;
[Imperative]
{
   a = foo(2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_Defect_1461391_6()
        {
            string code = @"
def foo ( a : int) 
{
    return = a;
}
y1 = { 1, 2 };
y2 = foo ( y1);
[Imperative]
{ 
	count = 0;
	for ( i in y1)
	{
	    y1[count] = y1[count] + 1;	
        count = count + 1;		
	}
}
t1 = y2[0];
t2 = y2[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_Function_From_Imperative_If_Block()
        {
            string code = @"
[Associative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	[Imperative]
	{
	
		a = { 0, 1, 2, 3, 4, 5 };
		x = 0;
		for ( i in a )
		{
			x = x + foo ( i );
		}
		
		y = 0;
		j = 0;
		while ( a[j] <= 4 )
		{
			y = y + foo ( a[j] );
			j = j + 1;
		}
		
		z = 0;
		
		if( x == 55 )
		{
		    x = foo (x);
		}
		
		if ( x == 50 )
		{
		    x = 2;
		}
		elseif ( y == 30 )
		{
		    y = foo ( y );
		}
		
		if ( x == 50 )
		{
		    x = 2;
		}
		elseif ( y == 35 )
		{
		    x = 3; 
		}
		else
		{
		    z = foo (5);
		}
	}
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_TestForLoopWithoutBracket()
        {
            string code = @"
[Imperative]
{
	a = { 1, 2, 3 };
    x = 0;
	
	for( y in a )
	    x = y;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_TestInvalidSyntax__2_()
        {
            string code = @"
[Associative]
{
	a = 2;;;;;
    b = 3;
       			
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_TestInvalidSyntax()
        {
            string code = @"
[Imperative]
{
	a = 2;;;;;
    b = 3;
       			
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT20_TestWhileToCreate2DimArray()
        {
            string code = @"
def Create2DArray( col : int)
{
	result = [Imperative]
    {
		array = { 1, 2 };
		counter = 0;
		while( counter < col)
		{
			array[counter] = { 1, 2};
			counter = counter + 1;
		}
		return = array;
	}
    return = result;
}
x = Create2DArray( 2) ;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_Defect_1460891()
        {
            string code = @"
[Imperative]
{
    b = { };
    count = 0;
    a = 1..5..2;
    for ( i in a )
    {
        b[count] = i + 1;
        count = count + 1;
    }
	c = b ;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_Defect_1460891_2()
        {
            string code = @"
def CreateArray ( x : var[] , i )
{
    x[i] = i;
	return = x;
}
b = {0, 1};
count = 0..1;
b = CreateArray ( b, count );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_Defect_1461390()
        {
            string code = @"
[Associative]
{
    a = 0;
    d = a + 1;
    [Imperative]
    {
       b = 2 + a;
       a = 1.5;
              
    }
    c = a + 2; // fail : runtime assertion 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
        [Test]
        public void DebugEQT21_Defect_1461390_2()
        {
            string code = @"
a = 1;
b = a + 1;
[Imperative]
{
    a = 2;
    c = b + 1;
	b = a + 2;
    [Associative]
    {
       a = 1.5;
       d = c + 1;
       b = a + 3; 
       a = 2.5; 	   
    }
    b = a + 4;
    a = 3;	
}
f = a + b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_IfElseWithArray_negative()
        {
            string code = @"
[Imperative]
{
    a = { 0, 4, 2, 3 };
	b = 1;
    c = 0;
	if(a > b)
	{
		c = 0;
	}
	else
	{
		c = 1;
	}
} 
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_TestAssignmentToBool__2_()
        {
            string code = @"
[Associative]
{
	a = true;
    b = false;      			
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_TestAssignmentToBool()
        {
            string code = @"
[Imperative]
{
	a = true;
    b = false;      			
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_TestIfElseStatementInsideForLoop()
        {
            string code = @"
[Imperative]
{
	a = { 1,2,3,4,5 };
	x = 0;
	
	for ( i in a )
	{
		if( i >=4 )
			x = x + 3;
			
		else if ( i ==1 )
			x = x + 2;
		
		else
			x = x + 1;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT21_TestWhileToCallFunctionWithNoReturnType()
        {
            string code = @"
def foo ()
{
	return = 0;
}
def test ()
{
	temp = [Imperative]
	{
		t1 = foo();
		t2 = 2;
		while ( t2 > ( t1 + 1 ) )
		{
		    t1 = t1 + 1;
		}
		return = t1;	
	}
	return = temp;
}
x = test();
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_Create_Multi_Dim_Dynamic_Array()
        {
            string code = @"
[Imperative]
{
    d = {{}};
    r = c = 0;
    a = { 0, 1, 2 };
	b = { 3, 4, 5 };
    for ( i in a )
    {
        c = 0;
		for ( j in b)
		{
		    d[r][c] = i + j;
			c = c + 1;
        }
		r = r + 1;
    }
	test = d;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_Defect_1463683()
        {
            string code = @"
def foo ()
{
	return = 1;
}
def test ()
{
	temp = [Imperative]
	{
		t1 = foo();
		t2 = 3;
		if ( t2 < ( t1 + 1 ) )
		{
		    t1 = t1 + 2;
		}
		else
		{
		    t1 = t1 ;
		}
		return = t1;		
	}
	return = temp;
}
x = test();
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_Defect_1463683_4()
        {
            string code = @"
def foo ()
{
	return = 1;
}
def test (t2)
{
	temp = [Imperative]
	{
		t1 = foo();
		if ( (t2 > ( t1 + 1 )) && (t2 >=3)  )
		{
		    t1 = t1 + 2;
		}
		else
		{
		    t1 = t1 ;
		}
		return = t1;		
	}
	return = temp;
}
x1 = test(3);
x2 = test(0);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_IfElseWithArrayElements()
        {
            string code = @"
[Imperative]
{
    a = { 0, 4, 2, 3 };
	b = 1;
    c = 0;
	if(a[0] > b)
	{
		c = 0;
	}
	elseif( b  < a[1] )
	{
		c = 1;
	}
} 
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_TestAssignmentToNegativeNumbers__2_()
        {
            string code = @"
[Associative]
{
	a = -1;
	b = -111;
	c = -0.1;
	d = -1.99;
	e = 1.99;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_TestAssignmentToNegativeNumbers()
        {
            string code = @"
[Imperative]
{
	a = -1;
	b = -111;
	c = -0.1;
	d = -1.99;
	e = 1.99;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT22_TestWhileStatementInsideForLoop()
        {
            string code = @"
[Imperative]
{
	a = { 1,2,3 };
	x = 0;
	
	for( y in a )
	{
		i = 1;
		while( i <= 5 )
		{
			j = 1;
			while( j <= 5 )
			{
				x = x + 1;
				j = j + 1;
			}
		i = i + 1;
		}
	}
}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT23_Function_Call_As_Function_Call_Arguments()
        {
            string code = @"
[Associative]
{
	def foo : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	
	def foo2 : double ( a : double , b :double )
	{
		return = foo ( a , b ) + foo ( a, b );	
	}
	
	a1 = 2;
	b1 = 4;
	c1 = foo2( foo (a1, b1 ), foo ( a1, foo (a1, b1 ) ) );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT23_TestForLoopWithDummyCollection()
        {
            string code = @"
[Imperative]
{
	a = {0, 0, 0, 0, 0, 0};
	b = {5, 4, 3, 2, 1, 0, -1, -2};
	i = 5;
	for( x in b )
	{
		if(i >= 0)
		{
			a[i] = x;
			i = i - 1;
		}
	}
	a1 = a[0];
	a2 = a[1];
	a3 = a[2];
	a4 = a[3];
	a5 = a[4];
	a6 = a[5];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT23_TestUsingMathAndLogicalExpr__2_()
        {
            string code = @"
[Associative]
{
  a = -3.5;
  b = -4;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT23_TestUsingMathAndLogicalExpr()
        {
            string code = @"
[Imperative]
{
  a = -3.5;
  b = -4;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_Dynamic_Array_Argument_Function_1465802_1()
        {
            string code = @"
	def foo : int(i:int[])
	{
		return = 1;
	}
[Associative]
{
cy={};
cy[0]=10;
cy[1]=12;
b1=foo(cy);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_Dynamic_Array_Argument_Function_1465802_2()
        {
            string code = @"
	def foo : int(i:int[])
	{
		return = 1;
	}
[Associative]
{
cy={};
cy[0]=10;
cy[1]=null;
b1=foo(cy);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_Dynamic_Array_Imperative_Function_Scope()
        {
            string code = @"
def createArray( p : int[] )
{  
    a = [Imperative]  
    {    
        collection = {};	
	lineCnt = 0;
	while ( lineCnt < 2 )
	{
            collection [ lineCnt ] = p [ lineCnt ] * -1;
	    lineCnt = lineCnt + 1;      
	}
	return = collection;
    }
    return = a;
}
x = createArray ( { 1, 2, 3, 4 } );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_Dynamic_Array_Imperative_Scope()
        {
            string code = @"
t = [Imperative]
{
    d = { { } };
    r = c = 0;
    a = { 0, 1, 2 };
    b = { 3, 4, 5 };
    for ( i in a )
    {
        c = 0;
	for ( j in b)
	{
	    d[r][c] = i + j;
	    c = c + 1;
        }
	r = r + 1;
    }
    test = d;
    return = test;
}
// expected : test = { { 3, 4, 5 }, {4, 5, 6}, {5, 6, 7} }
// received : test = { { 3, 4, 5 }, , }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_Dynamic_Array_Inside_Function()
        {
            string code = @"
def foo ( d : var[] )
{
    [Imperative]
    {
	r = c = 0;
	a = { 0, 1, 2 };
	b1 = { 3, 4, 5 };
	for ( i in a )
	{
	    c = 0;
	    for ( j in b1)
	    {
		d[r][c] = i + j;
		c = c + 1;
	    }
	    r = r + 1;
	}	
    }
    return = d;
}
b = {};
b = foo ( b ) ;     
a = b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_Dynamic_Array_Inside_Function_2()
        {
            string code = @"
def foo ( d : var[]..[] )
{
    [Imperative]
    {
	r = c = 0;
	a = { 0, 1, 2 };
	b1 = { 3, 4, 5 };
	for ( i in a )
	{
	    c = 0;
	    for ( j in b1)
	    {
		d[r][c] = i + j;
		c = c + 1;
	    }
	    r = r + 1;
	}	
    }
    return = d;
}
b = { {} };
b = foo ( b ) ;     
a = b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_TestForLoopToModifyCollection()
        {
            string code = @"
[Imperative]
{
	a = {1,2,3,4,5,6,7};
	i = 0;
	for( x in a )
	{
	
		a[i] = a[i] + 1;
		i = i + 1;
		
	}
	a1 = a[0];
	a2 = a[1];
	a3 = a[2];
	a4 = a[3];
	a5 = a[4];
	a6 = a[5];
	a7 = a[6];
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_TestUsingMathematicalExpr__2_()
        {
            string code = @"
[Associative]
{
  a = 3;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT24_TestUsingMathematicalExpr()
        {
            string code = @"
[Imperative]
{
  a = 3;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Adding_Elements_To_Array()
        {
            string code = @"
a = 0..2;
a[3] = 3;
b = a;
x = { { 0, 0 } , { 1, 1 } };
x[1][2] = 1;
x[2] = {2,2,2,2};
y = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Adding_Elements_To_Array_Function()
        {
            string code = @"
def add ( x : var[]..[] ) 
{
    x[1][2] = 1;
    x[2] = { 2, 2, 2, 2 };
    return = x;
}
x = { { 0, 0 } , { 1, 1 } };
x = add(x);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Defect_1459759()
        {
            string code = @"
p1 = 2;
p2 = p1+2;
p1 = true;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Defect_1459759_2()
        {
            string code = @"
a1 = { 1, 2 };
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Defect_1459759_3()
        {
            string code = @"
a = { 2 , b ,3 };
b = 3;
c = a[1] + 2;
d = c + 1;
b = { 1,2 };
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Defect_1459759_4()
        {
            string code = @"
[Imperative]
{
	[Associative]
	{
		p1 = 2;
		p2 = p1+2;
		p1 = true;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Defect_1459759_5()
        {
            string code = @"
[Imperative]
{
	a = 2;
	x = [Associative]
	{
		b = { 2, 3 };
		c = b[1] + 1;
		b = 2;
		return = c;
	}
	a = x;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_Defect_1459759_6()
        {
            string code = @"
	def foo ( a, b )
	{
		a = b + 1;
		b = true;
		return = { a , b };
	}
[Imperative]
{
	c = 3;
	d = 4;
	e = foo( c , d );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_RangeExpression_WithDefaultDecrement()
        {
            string code = @"
a=5..1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_RangeExpression_WithDefaultDecrement_1467121()
        {
            string code = @"
a=5..1;
b=-5..-1;
c=1..0.5;
d=1..-0.5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_RangeExpression_WithDefaultDecrement_nested_1467121_2()
        {
            string code = @"
a=(5..1).. (1..5);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_TestForLoopEmptyCollection()
        {
            string code = @"
[Imperative]
{
	a = {};
	x = 0;
	for( i in a )
	{
		x = x + 1;
	}
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_TestUsingMathematicalExpr__2_()
        {
            string code = @"
[Associative]
{
  a = 3.0;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT25_TestUsingMathematicalExpr()
        {
            string code = @"
[Imperative]
{
  a = 3.0;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_Defct_DNL_1459616()
        {
            string code = @"
a=1;
a={a,2};
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_Defct_DNL_1459616_2()
        {
            string code = @"
a={1,2};
[Imperative]
{
    a={a,2};
}
b = a;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_Defct_DNL_1459616_3()
        {
            string code = @"
a={1,2};
[Imperative]
{
    a={a,2};
}
b = { 1, 2 };
def foo ( )
{
    b =  { b[1], b[1] };
    return = null;
}
dummy = foo ();
c = b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_IfElseWithNegatedCondition()
        {
            string code = @"
[Imperative]
{
    a = 1;
	b = 1;
    c = 0;
	if( !(a == b) )
	{
		c = 1;
	}
	else
	{
		c = 2;
	}
		
} 
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_Negative_TestPropertyAccessOnPrimitive()
        {
            string code = @"
x = 1;
y = x.a;
[Imperative]
{
    x1 = 1;
    y1 = x1.a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_RangeExpression_Function_tilda_associative_1457845_3()
        {
            string code = @"
[Associative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
}
[Imperative]
{
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_RangeExpression_Function_tilda_multilanguage_1457845_2()
        {
            string code = @"
[Associative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
[Imperative]
{
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
	}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_TestForLoopOnNullObject()
        {
            string code = @"
[Imperative]
{
	x = 0;
	
	for ( i in b )
	{
		x = x + 1;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT26_defect_1464429_DynamicArray()
        {
            string code = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT27_DynamicArray_Invalid_Index_1465614_1()
        {
            string code = @"
a={};
b=a[2];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT27_RangeExpression_Function_Associative_1463472()
        {
            string code = @"
[Associative]
{
	def twice : double( a : double )
	{
		return = 2 * a;
	}
	z1 = 1..twice(4)..twice(1);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT27_RangeExpression_Function_Associative_1463472_2()
        {
            string code = @"
[Associative]
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
d=1..4;
c=twice(4);
//	z1 = 1..twice(4)..twice(1);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT27_RangeExpression_Function_Associative_replication()
        {
            string code = @"
[Associative]
{
	def twice : int[]( a : int )
	{
		c=2*(1..a);
		return = c;
	}
    d={1,2,3,4};
	z1=twice(d);
//	z1 = 1..twice(4)..twice(1);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT27_RangeExpression_Function_return_1463472()
        {
            string code = @"
[Associative]
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
d=1..4;
c=twice(4);
//	z1 = 1..twice(4)..twice(1);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT27_defect_1464429_DynamicArray()
        {
            string code = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT27_defect_1464429_DynamicArray_update()
        {
            string code = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;
count = -2..-1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT28_Defect_1452966()
        {
            string code = @"
[Imperative]
{
	a = { 1,2,3 };
	x = 0;
	for ( i in a )
	{
		for ( j in a )
        {
			x = x + j;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT28_Update_With_Inline_Condition()
        {
            string code = @"
x = 3;
a1 = 1;
a2 = 2;
a = x > 2 ? a1: a2;
a1 = 3;
a2 = 4;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT28_Update_With_Inline_Condition_2()
        {
            string code = @"
x = 3;
a1 = { 1, 2};
a2 = 3;
a = x > 2 ? a2: a1;
a2 = 5;
x = 1;
a1[0] = 0;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT29_Defect_1452966_2()
        {
            string code = @"
[Imperative]
{
	a = {{6},{5,4},{3,2,1}};
	x = 0;
	
    for ( i in a )
	{
		for ( j in i )
		{
			x = x + j;
		}
	}		
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT29_DynamicArray_Using_Out_Of_Bound_Indices()
        {
            string code = @"
   
    basePoint = {  };
    
    basePoint [ 4 ] =3;
    test = basePoint;
    
    a = basePoint[0] + 1;
    b = basePoint[ 4] + 1;
    c = basePoint [ 8 ] + 1;
    
    d = { 0,1 };
    e1 = d [ 8] + 1;
    
    x = { };
    y = { };    
    t = [Imperative]
    {
        k = { };
	for ( i in 0..1 )
	{
	    x[i] = i;
	}
	k[0] = 0;
	for ( i in x )
	{
	    y[i] = x[i] + x[i+1];
	    k[i+1] = x[i] + x[i+1];
	
	}
	return = k;
    }
    z = y;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT30_ForLoopNull()
        {
            string code = @"
[Imperative]
{
	a = { 1,null,null };
	x = 1;
	
	for( i in a )
	{
		x = x + 1;
	}
}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT30_Update_Global_Variables_Imperative_Scope()
        {
            string code = @"
x  = {0,0,0,0};
count = 0;
i = 0;
sum  = 0;
test = sum;
[Imperative]
{
    for  ( i in x ) 
    {
       x[count] = count;
       count = count + 1;       
    }
    j = 0;
    while ( j < count )
    {
        sum = x[j]+ sum;
        j = j + 1;
    }
}
y = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT30_Update_Global_Variables_Imperative_Scope_2()
        {
            string code = @"
count = 0;
i = 0;
sum  = 0;
test = sum;
[Imperative]
{
    for  ( i in x ) 
    {
       x[count] = count;
       count = count + 1;       
    }
    j = 0;
    while ( j < count )
    {
        sum = x[j]+ sum;
        j = j + 1;
    }
}
x  = {0,0,0,0};
y = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT31_Defect_1449877__2_()
        {
            string code = @"
[Associative]
{
	a = -1;
	b = -2;
	c = -3;
	c = a * b * c;
	d = c * b - a;
	e = b + c / a;
	f = a * b + c;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT31_Defect_1449877()
        {
            string code = @"
[Imperative]
{
	a = -1;
	b = -2;
	c = -3;
	c = a * b * c;
	d = c * b - a;
	e = b + c / a;
	f = a * b + c;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT32_Defect_1449877_2__2_()
        {
            string code = @"
[Associative]
{
	def func:int(a:int,b:int)
	{
	return = b + a;
	}
	a = 3;
	b = -1;
	d = func(a,b);
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT32_Update_With_Range_Expr()
        {
            string code = @"
y = 1;
y1 = 0..y;
y = 2;
z1 = y1;                             
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT33_Defect_1450003__2_()
        {
            string code = @"
[Associative]
{
	def check:double( _a:double, _b:int )
	{
	_c = _a * _b;
	return = _c;
	} 
	_a_test = check(2.5,5);
	_b = 4.5;
	_c = true;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT33_ForLoopToReplaceReplicationGuides()
        {
            string code = @"
a = { 1, 2 };
b = { 3, 4 };
//c = a<1> + b <2>;
dummyArray = { { 0, 0 }, { 0, 0 } };
counter1 = 0;
counter2 = 0;
[Imperative]
{
	for ( i in a )
	{
		counter2 = 0;
		
		for ( j in b )
		{	    
			dummyArray[ counter1 ][ counter2 ] = i + j;
			
			counter2 = counter2 + 1;
		}
		counter1 = counter1 + 1;
	}
	
}
a1 = dummyArray[0][0];
a2 = dummyArray[0][1];
a3 = dummyArray[1][0];
a4 = dummyArray[1][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT34_Defect_1450727__2_()
        {
            string code = @"
[Associative]
{
	x = -5.5;
	y = -4.2;
 
	z = x + y;
 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT34_Defect_1450727()
        {
            string code = @"
[Imperative]
{
	x = -5.5;
	y = -4.2;
 
	z = x + y;
 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT34_Defect_1452966()
        {
            string code = @"
[Imperative]
{
	a = { 1, 2, 3, 4 };
	sum = 0;
	
	for(i in a )
	{
		for ( i in a )
		{
			sum = sum + i;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT34_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo1 : double ( a : double )
	 {
	    return = true;
	 }
	 
	 dummyArg = 1.5;
	
	b1 = foo1 ( dummyArg );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT35_Defect_1450727_2__2_()
        {
            string code = @"
[Associative]
{
	def neg_float:double(x:double,y:double)
	{
	a = x;
	b = y;
	return = a + b;
	}
	z = neg_float(-2.3,-5.8);
 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT35_Defect_1452966_2()
        {
            string code = @"
[Imperative]
{
	a = { {1, 2, 3}, {4}, {5,6} };
	sum = 0;
	
	for(i in a )
	{
		for (  j in i )
		{
			sum = sum + j;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT35_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo2 : double ( a : double )
	 {
	    return = 5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo2 ( dummyArg );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT35_IfElseWithEmptyBody()
        {
            string code = @"
[Imperative]
{
    c = 0;
    if(0)
	{
		
	}
	elseif (1) { c = 2; }
	else { }
	
	
		
} 
 
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       

        [Test]
        public void DebugEQT36_Defect_1450555()
        {
            string code = @"
[Imperative]
{
	a = true;
	b = 2;
	c = 2;
 
	if( a )
	b = false;
 
	if( b==0 )
	c = 1;
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT36_Defect_1452966_3()
        {
            string code = @"
[Associative]
{
	a = { {1, 2, 3}, {4}, {5,6} };
	
	def forloop :int ( a: int[]..[] )
	{
		sum = 0;
		sum = [Imperative]
		{
			for(i in a )
			{
				for (  j in i )
				{
					sum = sum + j;
				}
			}
			return = sum;
		}
		return = sum;
	}
	
	b =forloop(a);
	
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT36_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = 5.5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT37_Defect_1450920()
        {
            string code = @"
[Imperative]
{
    a = 0;
    b = 0;
    c = 0;
    d = 0;
    if(true)
	{
		a = 1;
	}
	
	if(false)
	{
		b = 1;
	}
	elseif(true)
	{
		b = 2;
	}
	
	if(false)
	{
		c = 1;
	}
	elseif(false)
	{
		c = 2;
	}
	else
	{
		c =  3;
	}		
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT37_Defect_1454517()
        {
            string code = @"
	a = { 4,5 };
	
	b =[Imperative]
	{
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
		
		return = x;
	}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT37_Modify_Collections_Referencing_Each_Other()
        {
            string code = @"
a = {1,2,3};
b = a;
c1 = a[0];
b[0] = 10;
c2 = a[0];
testArray = a;
testArrayMember1 = c1;
testArrayMember2 = c2;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT37_TestOperationOnNullAndBool__2_()
        {
            string code = @"
[Associative]
{
	a = true;
	b = a + 1;
	c = null + 2;
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT37_TestOperationOnNullAndBool()
        {
            string code = @"
[Imperative]
{
	a = true;
	b = a + 1;
	c = null + 2;
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT38_Defect_1449928__2_()
        {
            string code = @"
[Associative]
{
 a = 2.3;
 b = -6.9;
 c = a / b;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT38_Defect_1449928()
        {
            string code = @"
[Imperative]
{
 a = 2.3;
 b = -6.9;
 c = a / b;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT38_Defect_1454517_2()
        {
            string code = @"
	a = { 4,5 };
	x = 0;
	
	[Imperative]
	{
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
	}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT38_Defect_1454517_3()
        {
            string code = @"
def foo ( a : int [] )
{
    x = 0;
	x = [Imperative]
	{	
		for( y in a )
		{
			x = x + y;
		}
		return =x;
	}
	return = x;
}
a = { 4,5 };	
[Imperative]
{
	b = foo(a);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT38_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int[] ( a : double )
	 {
	    return = a;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT39_Defect_1449704__2_()
        {
            string code = @"
[Associative]
{
 a = b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT39_Defect_1449704()
        {
            string code = @"
[Imperative]
{
 a = b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT39_Defect_1450920_2()
        {
            string code = @"
[Imperative]
{
a=0;
b=0;
c=0;
d=0;
    if(0.4)
	{
		d = 4;
	}
	
	if(1.4)
	{
		a = 1;
	}
	
	if(0)
	{
		b = 1;
	}
	elseif(-1)
	{
		b = 2;
	}
	
	if(0)
	{
		c = 1;
	}
	elseif(0)
	{
		c = 2;
	}
	else
	{
		c =  3;
	}		
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT39_Defect_1452951()
        {
            string code = @"
[Associative]
{
	a = { 4,5 };
   
	[Imperative]
	{
	       //a = { 4,5 }; // works fine
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT39_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = {1, 2};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT40_Create_3_Dim_Collection_Using_For_Loop()
        {
            string code = @"
x = { { { 0, 0} , { 0, 0} }, { { 0, 0 }, { 0, 0} }};
a = { 0, 1 };
b = { 2, 3};
c = { 4, 5 };
y = [Imperative]
{
	c1 = 0;
	for ( i in a)
	{
	    c2 = 0;
		for ( j in b )
		{
		    c3 = 0;
			for ( k in c )
			{
			    x[c1][c2][c3] = i + j + k;
				c3 = c3 + 1;
			}
			c2 = c2+ 1;
		}
		c1 = c1 + 1;
	}
	
	return = x;
			
}
p1 = y[0][0][0];
p2 = y[0][0][1];
p3 = y[0][1][0];
p4 = y[0][1][1];
p5 = y[1][0][0];
p6 = y[1][0][1];
p7 = y[1][1][0];
p8 = y[1][1][1];
p9 = x [1][1][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT40_Defect_1450552__2_()
        {
            string code = @"
[Associative]
{
 a = b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT40_Defect_1450552()
        {
            string code = @"
[Imperative]
{
 a = b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT40_Defect_1450843()
        {
            string code = @"
[Imperative]
{
 a = null;
 b1 = 0;
 b2 = 0;
 b3 = 0;
 if(a!=1); 
 else 
   b1 = 2; 
   
 if(a==1); 
 else 
   b2 = 2;
   
 if(a==1); 
 elseif(a ==3);
 else b3 = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT40_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int[][] ( a : double )
	 {
	    return = { {2.5}, {3.5}};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT40_Index_usingFunction_1467064()
        {
            string code = @"
def foo()
{    
return = 0;
}
x = { 1, 2 };
x[foo()] = 3;
y = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT41_Create_3_Dim_Collection_Using_For_Loop_In_Func_Call()
        {
            string code = @"
def foo :int[]..[]( a : int[], b:int[], c :int[])
{
	y = [Imperative]
	{
		x = { { { 0, 0} , { 0, 0} }, { { 0, 0 }, { 0, 0} }};
		c1 = 0;
		for ( i in a)
		{
			c2 = 0;
			for ( j in b )
			{
				c3 = 0;
				for ( k in c )
				{
					x[c1][c2][c3] = i + j + k;
					c3 = c3 + 1;
				}
				c2 = c2+ 1;
			}
			c1 = c1 + 1;
		}		
		return = x;				
	}
	return = y;
}
a = { 0, 1 };
b = { 2, 3};
c = { 4, 5 };
y = foo ( a, b, c );
p1 = y[0][0][0];
p2 = y[0][0][1];
p3 = y[0][1][0];
p4 = y[0][1][1];
p5 = y[1][0][0];
p6 = y[1][0][1];
p7 = y[1][1][0];
p8 = y[1][1][1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT41_Defect_1450778()
        {
            string code = @"
[Imperative]
{
 a=1;
 b=2;
 c=2;
 d = 2;
 
 if(a==1)
 {
    c = 1;
 }
 
 if(b==2)  
 {
     d = 1;
 }
 
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT41_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int[][] ( a : double )
	 {
	    return = { {2.5}, 3};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT41_Pass_3x3_List_And_2x4_List()
        {
            string code = @"
def foo : int(a : int, b : int)
{
	return = a * b;
}
list1 = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
list2 = { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } };
list3 = foo(list1, list2); // { { 1, 4, 9 }, { 20, 30, 42 } }
x = list3[0];
y = list3[1];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT41__Defect_1452423__2_()
        {
            string code = @"
[Associative]
{
	b = true;
	c = 4.5;
	d = c * b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT41__Defect_1452423()
        {
            string code = @"
[Imperative]
{
	b = true;
	c = 4.5;
	d = c * b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT42_Defect_1449707()
        {
            string code = @"
[Imperative]
{
 a = 1;
 b = 1;
 c = 1;
 if( a < 1 )
	c = 6;
 
 else if( b >= 2 )
	c = 5;
 
 else
	c = 4;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT42_Defect_1466071_Cross_Update_2()
        {
            string code = @"
i = 5;
totalLength = 6;
[Associative]
{
	x = totalLength > i ? 1 : 0;
	
	[Imperative]
	{
		for (j in 0..3)
		{
			i = i + 1;
		}	
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT42_Defect_1466071_Cross_Update_3()
        {
            string code = @"
y = 1;
a = 2;
x = a > y ? 1 : 0;
y = [Imperative]
{
                while (y < 2) // create a simple outer loop
                {
                    y = y + 1;                              
                }
		return = y;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT42_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : bool[]..[] ( a : double )
	 {
	    return = { {2}, 3};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT42_Pass_3_List_Different_Length()
        {
            string code = @"
def foo : int(a : int, b : int, c : int)
{
	return = a * b - c;
}
list1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
list2 = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, -1, -2, -3 };
list3 = {1, 4, 7, 2, 5, 8, 3 };
list4 = foo(list1, list2, list3); // { 9, 14, 17, 26, 25, 22, 25 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQT42__Defect_1452423_2()
        {
            string code = @"
[Imperative]
{
	a = { -2,3,4.5,true };
	x = 1;
	for ( y in a )
	{
		x = x *y;       //incorrect result
    }
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT43_Defect_1450706()
        {
            string code = @"
[Imperative]
{
 a1 = 7.3;
 a2 = -6.5 ;
 
 temp1 = 10;
 temp2 = 10;
 
 if( a1 <= 7.5 )
	temp1 = temp1 + 2;
 
 if( a2 >= -9.5 )
	temp2 = temp2 + 2;
 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT43_Defect_1463498()
        {
            string code = @"
[Associative]
{
def foo : int ( a : int, b : int )
{
	a = a + b;
	b = 2 * b;
	return = a + b;
}
a = 1;
b = 2;
c = foo (a, b ); // expected 9, received -3
d = a + b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT43_Function_With_Matching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int[]..[] ( a : double )
	 {
	    return = { { 0, 2 }, { 1 } };
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg )[0][0];	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT43_Pass_3_List_Different_Length_2_Integers()
        {
            string code = @"
def foo : int(a : int, b : int, c : int, d : int, e : int)
{
	return = a * b - c / d + e;
}
list1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
list2 = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, -1, -2, -3 };
list3 = {1, 4, 7, 2, 5, 8, 3 };
list4 = foo(list1, list2, list3, 4, 23);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        

        [Test]
        public void DebugEQT43__Defect_1452423_3()
        {
            string code = @"
[Imperative]
{
	a = 0;
	while ( a == false )
	{
		a = 1;
	}
	
	b = a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT44_Pass_3_List_Same_Length()
        {
            string code = @"
def foo : int(a : int, b : int, c : int)
{
	return = a * b - c;
}
list1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
list2 = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
list3 = {1, 4, 7, 2, 5, 8, 3, 6, 9, 0 };
list4 = foo(list1, list2, list3); // { 9, 14, 17, 26, 25, 22, 25, 18, 9, 10 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT44_Use_Bracket_Around_Range_Expr_In_For_Loop()
        {
            string code = @"
[Imperative] {
s = 0;
for (i in (0..10)) {
	s = s + i;
}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT44__Defect_1452423_4__2_()
        {
            string code = @"
[Associative]
{
	y = true;
	x = 1 + y;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT44__Defect_1452423_4()
        {
            string code = @"
[Imperative]
{
	y = true;
	x = 1 + y;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT45_Defect_1450506()
        {
            string code = @"
[Imperative]
{
    i1 = 2;
    i2 = 3;
	i3 = 4.5;
	
    temp = 2;
    
	while(( i2 == 3 ) && ( i1 == 2 )) 
	{
	temp = temp + 1;
	i2 = i2 - 1;
    }
	
	if(( i2 == 3 ) || ( i3 == 4.5 )) 
	{
	temp = temp + 1;
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT45_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( 1 );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT45_Pass_3_List_Same_Length_2_Integers()
        {
            string code = @"
def foo : int(a : int, b : int, c : int, d : int, e : int)
{
	return = a * b - c * d + e;
}
list1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
list2 = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
list3 = {1, 4, 7, 2, 5, 8, 3, 6, 9, 0 };
list4 = foo(list1, list2, list3, 26, 43); // { 27, -43, -115, 19, -57, -135, -7, -89, -173, 53 }  
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT45__Defect_1452423_5__2_()
        {
            string code = @"
[Associative]
{
	a = 4 + true;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT45__Defect_1452423_5()
        {
            string code = @"
[Imperative]
{
	a = 4 + true;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT46_Pass_FunctionCall_Reutrn_List()
        {
            string code = @"
def foo : int(a : int)
{
	return = a * a;
}
list1 = { 1, 2, 3, 4, 5 };
list3 = foo(foo(foo(list1))); // { 1, 256, 6561, 65536, 390625 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT46_TestBooleanOperationOnNull__2_()
        {
            string code = @"
[Imperative]
{
	a = null;
	b = a * 2;
	c = a && 2;	
	d = 0;
	if ( a && 2 == 0)
	{
        d = 1;
	}
	else
	{
	    d = 2;
	}
	
	if( !a )
	{
	    d = d + 2;
	}
	else
	{
	    d = d + 1;
	}
	if( a )
	{
	    d = d + 3;
	}
	
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT46_TestBooleanOperationOnNull()
        {
            string code = @"
[Imperative]
{
	a = null;
	b = a * 2;
	c = a && 2;	
	d = 0;
	if ( a && 2 == 0)
	{
        d = 1;
	}
	else
	{
	    d = 2;
	}
	
	if( !a )
	{
	    d = d + 2;
	}
	if( a )
	{
	    d = d + 1;
	}
	
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT46_TestIfWithNull()
        {
            string code = @"
[Imperative]
{
    a = null;
    c = null;
	
    if(a == 0)
	{
		a = 1;	
	}
    if(null == c)
	{
		c = 1;	
	}
    if(a == b)
	{
		a = 2;	
	}	
	
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT47_Defect_1450858()
        {
            string code = @"
[Imperative]
{	
	i = 1;
	a = 3;
	if( a==3 )             	
	{		 
		while( i <= 4 )
		{
		if( i > 10 )
		temp = 4;
		else
		i = i + 1;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT47_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( true);	
	 c = 3;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT47_Pass_Single_3x3_List()
        {
            string code = @"
def foo : int(a : int)
{
	return = a * a;
}
list1 = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
list2 = foo(list1); // { { 1, 4, 9 }, { 16, 25, 36 }, { 49, 64, 81 } }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT47_TestBooleanOperationOnNull()
        {
            string code = @"
[Imperative]
{
	a = false;
        b = 0;
	d = 0;
	if( a == null)
	{
	    d = d + 1;
	}
    if( b == null)
	{
	    d = d + 1;
	}	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT48_Pass_Single_List()
        {
            string code = @"
def foo : int(num : int)
{
	return = num * num;
}
list1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
list2 = foo(list1);  // { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT49_Defect_1450783()
        {
            string code = @"
[Imperative]
{
	a = 4;
	if( a == 4 )
	{
	    i = 0;
	}
	a = i;
	b = i;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

      
        [Test]
        public void DebugEQT49_Pass_Single_List_2_Integers()
        {
            string code = @"
def foo : int(num : int, num2 : int, num3 : int)
{
	return = num * num2 - num3;
}
list1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
list2 = foo(list1, 34, 18); // { 16, 50, 84, 118, 152, 186, 220, 254, 288, 322 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT49_TestForStringObjectType()
        {
            string code = @"
[Associative]
{
    def foo : string (x : string )
	{
	   return = x; 		
	}
    a = ""sarmistha"";
    b = foo ( a );	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT50_1_of_3_Exprs_is_List()
        {
            string code = @"
list1 = { true, false, true, false, true };
list2 = list1 ? 1 : 0; // { 1, 0, 1, 0, 1 }
list3 = true ? 10 : list2; // { 10, 10, 10, 10, 10 }
list4 = false ? 10 : list2; // { 1, 0, 1, 0, 1 }
a = { 1, 2, 3, 4, 5 };
b = {5, 4, 3, 2, 1 };
c = { 4, 3, 2, 1 };
list5 = a > b ? 1 : 0; // { 0, 0, 0, 1, 1 }
list6 = c > a ? 1 : 0; // { 1, 1, 0, 0 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT50_Defect_1456713()
        {
            string code = @"
a = 2.3;
b = a * 3;
//Expected : b = 6.9;
//Recieved : b = 6.8999999999999995;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT50_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo : double ( a : int[] )
	 {
	    return = 1.5;
     }
	 aa = { };
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT51_2_of_3_Exprs_are_Lists_Different_Length()
        {
            string code = @"
list1 = { 1, 2, 3, 4, 5 };
list2 = { true, false, true, false };
list3 = list2 ? list1 : 0; // { 1, 0, 3, 0 }
list4 = list2 ? 0 : list1; // { 0, 2, 0, 4 }
list5 = { -1, -2, -3, -4, -5, -6 };
list6 = true ? list1 : list5; // { 1, 2, 3, 4, 5 }
list7 = false ? list1 : list5; // { -1, -2, -3, -4, -5 }  
a = { 1, 2, 3, 4 };
b = { 5, 4, 3, 2, 1 };
c = { 1, 4, 7 };
list8 = a >= b ? a + c : 10; // { 10, 10, 10 }
list9 = a < b ? 10 : a + c; // { 10, 10, 10 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT51_Assignment_Using_Negative_Index()
        {
            string code = @"
a = { 0, 1, 2, 3 };
c1 = a [-1];
c2 = a [-2];
c3 = a [-3];
c4 = a [-4];
c5 = a [-5];
c6 = a [-1.5];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT51_Defect_1452588()
        {
            string code = @"
[Imperative]
{
    a = 0;
    
    if ( a == 0 )
    {
	    b = 2;
    }
    c = a;
} 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT51_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
	 aa = {1, 2 };
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT52_2_of_3_Exprs_are_Lists_Same_Length()
        {
            string code = @"
list1 = { 1, 2, 3, 4, 5 };
list2 = { true, false, true, false, true };
list3 = list2 ? list1 : 0; // { 1, 0, 3, 0, 5 }
list4 = list2 ? 0 : list1; // { 0, 2, 0, 4, 0 }
list5 = true ? list3 : list4; // { 1, 0, 3, 0, 5 }
list6 = true ? list4 : list3; // {0, 2, 0, 4, 0 }
a = { 1, 2, 3, 4, 5 };
b = { 5, 4, 3, 2 };
list7 = a > b ? a + b : 10; // { 10, 10, 10, 6 }
list8 = a <= b ? 10 : a + b; // { 10, 10, 10, 6 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT52_Defect_1449889()
        {
            string code = @"
[Imperative]
{
	a = b;
    c = foo();
	d = 1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT52_Defect_1452588_2()
        {
            string code = @"
[Associative]
{ 
	[Imperative]
	{
            g2 = g1;	
	}	
	g1 = 3;      
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT52_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       
        [Test]
        public void DebugEQT53_3_of_3_Exprs_are_different_dimension_list()
        {
            string code = @"
a = { { 1, 2, 3 }, { 4, 5, 6 } };
b = { { 1, 2 },  { 3, 4 }, { 5, 6 } };
c = { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } };
list = a > b ? b + c : a + c; // { { 2, 4, }, { 8, 10 } } 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT53_Collection_Indexing_On_LHS_Using_Function_Call()
        {
            string code = @"
def foo()
{
    return = 0;
}
x = { 1, 2 };
x[foo()] = 3;
y = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT53_Function_Updating_Argument_Values()
        {
            string code = @"
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    a = 4.5;
		return = a;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT53_Undefined_Class_negative_1467107_10()
        {
            string code = @"
def foo(x:int)
{
return = x + 1;
}
//y1 = test.foo(2);
m=null;
y2 = m.foo(2);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT53_Undefined_Class_negative_associative_1467091_13()
        {
            string code = @"
def foo ( x : int)
{
return = x + 1;
}
y = test.foo (1);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT54_3_of_3_Exprs_are_Lists_Different_Length()
        {
            string code = @"
list1 = { true, false, true, true, false };
list2 = { 1, 2, 3, 4 };
list3 = { -1, -2, -3, -4, -5, -6 };
list4 = list1 ? list2 : list3; // { 1, -2, 3, 4 }
list5 = !list1 ? list2 : list4; // { 1, 2, 3, 4 }
list6 = { -1, -2, -3, -4, -5 };
list7 = list1 ? list2 : list6; // { 1, -2, 3, 4 }
a = { 3, 0, -1 };
b = { 2, 1, 0, 3 };
c = { -2, 4, 1, 2, 0 };
list8 = a < c ? b + c : a + c; // { 1, 4, 1 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT55_3_of_3_Exprs_are_Lists_Same_Length()
        {
            string code = @"
list1 = { true, false, false, true };
list2 = { 1, 2, 3, 4 };
list3 = { -1, -2, -3, -4 };
list4 = list1 ? list2 : list3; // { 1, -2, -3, 4 }
list5 = !list1 ? list2 : list3; // { -1, 2, 3, -4 }
a = { 1, 4, 7 };
b = { 2, 8, 5 };
c = { 6, 9, 3 };
list6 = a > b ? b + c : b - c; // { -4, -1, 8 }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT55_Associative_assign_If_condition_1467002()
        {
            string code = @"
[Associative]
{
	x = {} == null;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT55_Defect_1450506()
        {
            string code = @"
[Imperative]
{
    i1 = 1.5;
    i2 = 3;
    temp = 2;
    while( ( i2==3 ) && ( i1 <= 2.5 )) 
    {
        temp = temp + 1;
	    i2 = i2 - 1;
    }     
 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT56_Function_Updating_Argument_Values()
        {
            string code = @"
[Associative]
{ 
	 def foo : int[] ( a : int[] )
	 {
	    a[0] = 0;
		return = a;
     }
	 aa = { 1, 2 };
	 bb = foo ( aa );	
	 
	 c = 3;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT56_UnaryOperator()
        {
            string code = @"
list1 = { true, true, false, false, true, false };
list2 = !list1; // { false, false, true, true, false, true }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT57_Function_Using_Local_Var_As_Same_Name_As_Arg()
        {
            string code = @"
[Associative]
{ 
	 def foo : int ( a : int )
	 {
	    a = 3;
		b = a + 1;
		return = b;
     }
	 
	 aa = 1;
	 bb = foo ( aa );
     c = 3;	 
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT58_Defect_1450932_comparing_collection_with_singleton_Associative()
        {
            string code = @"
[Associative]
{
    a2 = { 0, 1 };
	b2 = 1;
	d2 = a2 > b2 ? true : { false, false};
    //f2 = a2 > b2;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT58_Defect_1450932_comparing_collection_with_singleton_Associative_2()
        {
            string code = @"
[Associative]
{
    a2 = { 0, 1 };
    b2 = 1;	
    f2 = a2 > b2;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT58_Defect_1450932_comparing_collection_with_singleton_Associative_3()
        {
            string code = @"
[Associative]
{
    a2 = { 0, 1 };
    b2 = 1;
    d2 = a2 > b2 ? true : { false, false};
    f2 = a2 > b2;	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT58_Defect_1450932_comparing_collection_with_singleton_Imperative()
        {
            string code = @"
[Imperative]
{
    a = { 0, 1 };
	b = 1;
	c = -1;
	if(a > b)
	{
		c = 0;
	}
	else
	{
		c = 1;
	}
    d = a > b ? true : { false, false};
    f = a > b;
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT58_Function_With_No_Argument()
        {
            string code = @"
     def foo : int (  )
	 {
	    return = 3;
     }
	 
	 [Associative]
	 { 
		c = foo();	
     }
	 
	 [Imperative]
	 { 
		c = foo();	
     }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT59_Defect_1453881()
        {
            string code = @"
d2 = ( null == 0 ) ? 1 : 0; 
[Imperative]
{
	a = false;
    b = 0.5;
	d = 0;
	if( a == null)
	{
	    d = d + 1;
	}
	else
	{
	   d = d + 2;
	}
    if( b == null)
	{
	    b = b + 1;
	}
	else
	{
	   b = b + 2;
	}
	
	if( b != null)
	{
	    b = b + 3;
	}
	else
	{
	    b = b + 4;
	}
	
	
}	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT59_Defect_1453881_2()
        {
            string code = @"
def foo ()
{
    c = 
	[Imperative]
	{
		a = false;
		b = 0.5;
		d = 0;
		if( a == null)
		{
			d = d + 1;
		}
		else
		{
		   d = d + 2;
		}
		if( b == null)
		{
			b = b + 1;
		}
		else
		{
		   b = b + 2;
		}
		
		if( b != null)
		{
			b = b + 3;
		}
		else
		{
			b = b + 4;
		}
        return = { b, d };		
	}	
	return = c;
}
test = foo();
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT59_Defect_1455590()
        {
            string code = @"
	a = b = 2;
	[Imperative]
	{
		c = d = e = 4+1;
	}
		
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT59_Function_With_No_Return_Stmt()
        {
            string code = @"
     def foo : int ( a : int )
	 {
	    b = a + 1;
     }
	 
	 [Associative]
	 { 
		c = foo(1);	
     }
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT60_Defect_1455590_2()
        {
            string code = @"
def foo ( a )
{
	b = c = a ;
	return = a + b + c;
}
x = foo ( 3 );
[Imperative]
{
	y = foo ( 4 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT60_Function_With_No_Return_Stmt()
        {
            string code = @"
     def foo : int ( a : int )
	 {
	    b = a + 1;
     }
	 
	 [Imperative]
	 { 
		c = foo(1);	
     }
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT61_TestBooleanOperationOnNull()
        {
            string code = @"
[Imperative]
{
    a = null;
	
	b1 = 0;
	b2 = 1;
	b3 = -1;
	
	if( a == b1 )
	{
	    b1 = 10;
	}
	if ( a < b2 )
	{
		b2 = 10;
	}
	if ( a > b3 )
	{
		b3 = 10;
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT62_Change_Avariable_To_Dynamic_Array_OnTheFly()
        {
            string code = @"
def func(a:int)
{
a=5;
return = a;
}
c=1;
b= func(c[0]);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT62_Condition_Not_Evaluate_ToBool()
        {
            string code = @"
[Imperative]
{
    A = 1;
    if (0)       
 	   A = 2; 
    else 
	  A= 3;
}
//expected A=1;//Received A=3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT62_Create_Dynamic_Array_OnTheFly()
        {
            string code = @"
z=[Imperative]
{
for (i in 0..7)
{
b[i] = i;
}
return=b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT62_Create_Dynamic_Array_OnTheFly_AsFunctionArgument()
        {
            string code = @"
def func(a:int)
{
a=5;
return = a;
}
b= func(c[0]);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT62_Create_Dynamic_Array_OnTheFly_passargument_function()
        {
            string code = @"
	
  def test(a:int[])
	{
	b=1;
	return=b;
	}
d[0]=5;
a=test(d);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT62_Defect_1456721()
        {
            string code = @"
b = true;
a = 2 * b;
c = 3;
b1 = null;
a1 = 2 * b1;
c1 = 3;
a2 = 1 + true;
b2 = 2 * true;
c2 = 3  - true;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT63_Create_MultiDimension_Dynamic_Array_OnTheFly()
        {
            string code = @"
def func(a:int[]..[])
{
a[0][1]=5;
a[2][3]=6;
return = a;
}
c=1;
b= func(c);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

      

        [Test]
        public void DebugEQT63_Dynamic_array_onthefly_1467066()
        {
            string code = @"
z=[Imperative]
{
b[5]=0;
for (i in 0..7)
{
b[i] = i;
}
return=b;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT63_Dynamic_array_onthefly_aliastoanother()
        {
            string code = @"
a=5;
b=a;
a[2]=3;
b[2]=-5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT63_Dynamic_array_onthefly_function_1467139()
        {
            string code = @"
def foo(a:int[])
{
}
x[0]=5;
a = foo(x);
c = {100};
t = x;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT63_Dynamic_array_onthefly_function_return()
        {
            string code = @"
def foo()
{
return =b[0]=5;
}
a = foo();
c = {100};
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT63_Dynamic_array_onthefly_update()
        {
            string code = @"
z=true;
b=z;
z[0]={1};
z=5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT64_Defect_1450715()
        {
            string code = @"
[Imperative]
{
    a = { 1, 0.5, null, {2,3 } ,{{0.4, 5}, true } };
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT64_Modify_itemInAnArray_1467093()
        {
            string code = @"
a = {1, 2, 3};
a[1] = a; 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT64_Modify_itemInAnArray_1467093_2()
        {
            string code = @"
[Imperative]
{
a = {};
b = a;
a[0] = b;
//hangs here
c = a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT65_Array_Alias_ByVal_1467165()
        {
            string code = @"
a = {0,1,2,3};
b=a;
a[0]=9;
b[0]=10;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT65_Array_Alias_ByVal_1467165_3()
        {
            string code = @"
a = {0,1,2,3};
b=a;
a[0]=9;
b[0]=false;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT65_Array_Alias_ByVal_1467165_6()
        {
            string code = @"
a = {0,1,2,3};
b=a;
a[0]=null;
b[0]=false;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT66_Array_CannotBeUsedToIndex1467069()
        {
            string code = @"
[Imperative]
{
    a = {3,1,2}; 
    x = {10,11,12,13,14,15}; 
    x[a] = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT66_Array_CannotBeUsedToIndex1467069_2()
        {
            string code = @"
[Imperative]
{
    a = {3,1,2}; 
    x = {10,11,12,13,14,15}; 
    x[a] = 2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT67_Array_Remove_Item()
        {
            string code = @"
a={1,2,3,4,5,6,7};
a=Remove(a,0);// expected :{2,3,4,5,6,7}
a=Remove(a,4);//expected {1,2,3,4,6,7}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT67_Array_Remove_Item_2()
        {
            string code = @"
a={1,2,3,4,5,6,7};
a=Remove(a,0);// expected :{2,3,4,5,6,7}
a=Insert(a,4,6);//expected {1,2,3,4,6,7}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT68_copy_byreference_1467105()
        {
            string code = @"
[Associative]
{
a = { 1, 2, 3};
b = a;
b[0] = 10;
test = a[0]; //= 10Ö i.e. a change in ëbí causes a change to ëaí
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT74_Function_With_Simple_Replication_Associative()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	x = { 1, 2, 3 };
	y = foo(x);
	
}
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT75_Function_With_Replication_In_Two_Args()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = { 1, 2, 3 };
	x2 = { 1, 2, 3 };
	
	y = foo ( x1, x2 );
	
}
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT76_Function_With_Replication_In_One_Arg()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = { 1, 2, 3 };
	x2 = 1;
	
	y = foo ( x1, x2 );
	
}
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT77_Defect_1460274_Class_Update()
        {
            string code = @"
b = 1;
a = b + 1;
b = a;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT77_Defect_1460274_Class_Update_4()
        {
            string code = @"
[Imperative]
{
	a = {};
	b = a;
	a[0] = b;
	c = Count(a);
}
[Associative]
{
	a1 = {0};
	b1 = a1;
	a1[0] = b1;
	c1 = Count(a1);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT77_Function_With_Simple_Replication_Guide()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = { 1, 2 };
	x2 = { 1, 2 };
	y = foo( x1<1> , x2<2> );
	a1 = y[0][0];
	a2 = y[0][1];
	a3 = y[1][0];
	a4 = y[1][1];
	
}
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT78_Function_call_By_Reference()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		a = a + b;
		b = 2;
		return  = a + b;
	}
	
	a = 1;
	b = 2;
	c = foo (a, b );
	d = a + b;
	
}
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT80_Function_call_By_Reference()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		c = [Imperative]
		{
            d = 1;
			return = d;	
		}
		a = a + c;
		b = b + c;
		return  = a + b;
	}
	
	a = 2;
	b = 1;
	c = foo (a, b );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT81_Function_Calling_Imp_From_Assoc()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > 1 )
			{
				d = 1;
			}
			return = d;	
		}
		return  = a + c;
	}
	
	a = 2;
	b = foo (a );	
}
	 
	 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT83_Function_With_Null_Arg()
        {
            string code = @"
def foo:int ( a : int )
{	
	return = a;
}
[Associative]
{
	b = foo( null );
}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT87_Function_Returning_From_Imp_Block_Inside_Assoc()
        {
            string code = @"
def foo : int( a : int)
{
  temp = [Imperative]
  {
      if ( a == 0 )
      {
          return = 0; 
      }
    return = a ;
  } 
  return = temp;
}
x = foo( 0 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT88_Function_With_Collection_Argument()
        {
            string code = @"
def foo : double (arr : double[])
{
    return = 0;
}
arr = {1,2,3,4};
sum = foo(arr);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT89_Function_With_Replication()
        {
            string code = @"
def foo : double (arr1 : double[], arr2 : double[] )
{
    return  = arr1[0] + arr2[0];
}
arr = {  {2.5,3}, {1.5,2} };
two = foo (arr, arr);
t1 = two[0];
t2 = two[1];
//arr1 = {2.5,3};
//arr2 = {1.5,2};
//two = foo(arr1, arr2);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT91_Function_With_Default_Arg()
        {
            string code = @"
def foo:double(x:int = 2, y:double = 5.0)
{
	return = x + y;
}
a = foo();
b = foo(1, 3.0);
c = foo();
[Imperative]
{
	d = foo();
	e = foo(1, 3.0);
	f = foo();
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQT92_Function_With_Default_Arg_Overload()
        {
            string code = @"
def foo:double()
{
	return = 0.0;
}
def foo:double(x : int = 1, y : double = 2.0)
{
	return = x + y;
}
a = foo();
b = foo(3);
c = foo(3.4); // not found, null
d = foo(3, 4.0);
e = foo(1, 2.0, 3); // not found, null
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA01_RangeExpressionWithIntegerIncrement()
        {
            string code = @"
[Imperative]
{
	a1 = 1..5..2;
	a2 = 12.5..20..2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA02_RangeExpressionWithDecimalIncrement()
        {
            string code = @"
[Imperative]
{
	a1 = 2..9..2.7;
	a2 = 10..11.5..0.3;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA03_RangeExpressionWithNegativeIncrement()
        {
            string code = @"
[Imperative]
{
	a = 10..-1..-2;
	b = -2..-10..-1;
	c = 10..3..-1.5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA04_RangeExpressionWithNullIncrement()
        {
            string code = @"
[Imperative]
{
	a = 1..5..null;
	b = 0..6..(null);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA05_RangeExpressionWithBooleanIncrement()
        {
            string code = @"
[Imperative]
{
	a = 2.5..6..(true);
	b = 3..7..false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA06_RangeExpressionWithIntegerTildeValue()
        {
            string code = @"
[Imperative]
{
	a = 1..10..~4;
	b = -2.5..10..~5;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA07_RangeExpressionWithDecimalTildeValue()
        {
            string code = @"
[Imperative]
{
	a = 0.2..0.3..~0.2; //divide by zero error
	b = 6..13..~1.3;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA08_RangeExpressionWithNegativeTildeValue()
        {
            string code = @"
[Imperative]
{
	a = 3..1..~-0.5;
	b = 18..13..~-1.3;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA09_RangeExpressionWithNullTildeValue()
        {
            string code = @"
[Imperative]
{
	a = 1..5..~null;
	b = 5..2..~(null);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA10_RangeExpressionWithBooleanTildeValue()
        {
            string code = @"
[Imperative]
{
	a = 1..3..(true);
	b = 2..2..false;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA11_RangeExpressionWithIntegerHashValue()
        {
            string code = @"
[Imperative]
{
	a = 1..3.3..#5;
	b = 3..3..#3;
	c = 3..3..#1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA12_RangeExpressionWithDecimalHashValue()
        {
            string code = @"
[Imperative]
{
	a = 1..7..#2.5;
	b = 2..10..#2.4;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA13_RangeExpressionWithNegativeHashValue()
        {
            string code = @"
[Imperative]
{
	a = 7.5..-2..#-9;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA14_RangeExpressionWithNullHashValue()
        {
            string code = @"
[Imperative]
{
	a = 2..10..#null;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA15_RangeExpressionWithBooleanHashValue()
        {
            string code = @"
[Imperative]
{
	b = 12..12..#false;
	a = 12..12..#(true);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA16_RangeExpressionWithIncorrectLogic_1()
        {
            string code = @"
[Imperative]
{
	a = 5..1..2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA17_RangeExpressionWithIncorrectLogic_2()
        {
            string code = @"
[Imperative]
{
	a = 5.5..10.7..-2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA18_RangeExpressionWithIncorrectLogic_3()
        {
            string code = @"
[Imperative]
{
	a = 7..7..5;
	b = 8..8..~3;
	c = 9..9..#1;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA19_RangeExpressionWithIncorrectLogic_4()
        {
            string code = @"
[Imperative]
{
	a = null..8..2;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQTA21_Defect_1454692()
        {
            string code = @"
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in b )
	{
		x = y + x;
	}
	
}	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA21_Defect_1454692_2()
        {
            string code = @"
def length : int (pts : double[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }
        
        return = counter;
    }
    return = numPts;
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
num = length(arr);
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA21_Defect_1454692_4()
        {
            string code = @"
def foo(i : int[])
{
    count = 0;
	count = [Imperative]
	{
	    for ( x in i )
		{
		    count = count + 1;
		}
		return = count;
	}
	return = count;
	
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in arr )
	{
		x = y + x;
	}
	x1 = 0..3;
	c = foo(x1);
	
}
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA22_Range_Expression_floating_point_conversion_1467127()
        {
            string code = @"
a = 1..6..#10;
b = 0.1..0.6..#10;
c = 0.01..-0.6..#10;
d= -0.1..0.06..#10;
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA22_Range_Expression_floating_point_conversion_1467127_2()
        {
            string code = @"
a = 0..7..~0.75;
b = 0.1..0.7..~0.075;
c = 0.01..-7..~0.75;
d= -0.1..7..~0.75; 
e = 1..-7..~1;
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA23_Defect_1466085_Update_In_Range_Expr()
        {
            string code = @"
y = 1;
y1 = 0..y;
y = 2;
z1 = y1; 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA23_Defect_1466085_Update_In_Range_Expr_2()
        {
            string code = @"
a = 0;
b = 10;
c = 2;
y1 = a..b..c;
a = 7;
b = 14;
c = 7;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTA23_Defect_1466085_Update_In_Range_Expr_3()
        {
            string code = @"
def foo ( x : int[] )
{
    return = Count(x);
}
a = 0;
b = 10;
c = 2;
y1 = a..b..c;
z1 = foo ( y1 );
z2 = Count( y1 );
c = 5;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV101_Indexing_IntoArray_InFunctionCall_1463234()
        {
            string code = @"
def foo()
{
return = {1,2};
}
t = foo()[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV101_Indexing_IntoNested_FunctionCall_1463234_5()
        {
            string code = @"
	def foo()
	{
		return = {foo2()[0],foo2()[1]};
	}
def foo2()
{
return = {1,2};
}
a=test.test()[0];
t = foo()[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV101_Indexing_Intoemptyarray_InFunctionCall_1463234_3()
        {
            string code = @"
def foo()
{
return = {};
}
t = foo()[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV101_Indexing_Intosingle_InFunctionCall_1463234_()
        {
            string code = @"
def foo()
{
return = {1,2};
}
t = foo()[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV101_Indexing_Intosingle_InFunctionCall_1463234_2()
        {
            string code = @"
def foo()
{
return = {1};
}
t = foo()[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV101_Indexing_Intovariablenotarray_InFunctionCall_1463234_4()
        {
            string code = @"
def foo()
{
return = 1;
}
t = foo()[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV12_Function_With_Argument_Update_Associative()
        {
            string code = @"
[Associative]
{
	def update:int( a:int, b:int )
	{
		a = a + 1;
		b = b + 1;
		return = a + b;
	}
	
	c = 5;
	d = 5;
	e = update(c,d);
	e = c;
	f = d;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV14_Empty_Functions_Associative()
        {
            string code = @"
[Associative]
{
	def foo:int ( a : int )
	{
	}
	
	b = foo( 1 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       

        [Test]
        public void DebugEQTV16_Function_With_No_Return_Statement_DS()
        {
            string code = @"
	def foo : int( a : int )
	{
		a = a + 1;
	}
	[Imperative]
	{
		b = foo( 1 );
	}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV17_Function_Access_Local_Variables_Outside()
        {
            string code = @"
	def foo: int ( a : int )
	{
		b = a + 1;
		c = a * 2;
		return = a;
	}
[Imperative]
{	
	d = foo( 1 );
	e = b;
	f = c;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV18_Function_Access_Global_Variables_Inside()
        {
            string code = @"
	global = 5;
	global2 = 6;
	
	def foo: int ( a : int )
	{
		b = a + global;
		c = a * 2 * global2;
		return = b + c;
	}
[Imperative]
{	
	d = foo( 1 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV19_Function_Modify_Global_Variables_Inside()
        {
            string code = @"
	global = 5;
	
	def foo: int ( a : int )
	{
		global = a + global;
		
		return = a;
	}
[Imperative]
{	
	d = foo( 1 );
	e = global;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       

        [Test]
        public void DebugEQTV23_Defect_1455152()
        {
            string code = @"
def foo : int ( a : int )
{
    b = a + 1;
}	 
[Associative]
{
     c = foo(1);
}
[Imperative]
{
     d = foo(1);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV24_Defect_1454958()
        {
            string code = @"
def foo: int( a : int , b : int)
{
	return = a + b;
}
[Associative]
{
	b = Foo( 1,2 );
}
[Imperative]
{
	c = foo( 1 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV26_Defect_1454923_2()
        {
            string code = @"
def function1: int ( a : int, b : int )
{
	return = -1 * (a * b );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV27_Defect_1454688()
        {
            string code = @"
[Associative]
{
	a = function1(1,2,3);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV28_Defect_1454688_2()
        {
            string code = @"
[Imperative]
{
	a = function(1,2,3);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV29_Overloading_Different_Number_Of_Parameters()
        {
            string code = @"
def foo:int ( a : int )
{
	return = a + 1;
}
def foo:int ( a : int, b : int, c: int )
{
	return = a + b + c ;
}
c = foo( 1 );
d = foo( 3, 2, 0 );
[Imperative]
{
	a = foo( 1, 2, 3 );
}	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV30_Overloading_Different_Parameter_Types()
        {
            string code = @"
def foo:int ( a : int )
{
	return = 2 * a;
}
def foo:int ( a : double )
{
	return = 2;
}
	b = foo( 2 );
	c = foo(3.4);
[Imperative]
{
	d = foo(-2.4);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV31_Overloading_Different_Return_Types()
        {
            string code = @"
def foo: int( a: int )
{
	return = 1;
}
// This is the same definition regardless of return type
def foo: double( a : int )
{
	return = 2.3;
}
b = foo ( 1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV32_Function_With_Default_Argument_Value()
        {
            string code = @"
def foo : int ( a = 5, b = 5 )
{
	return =  a +  b;
}
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1, 2 );
	c4 = foo ( 1, 2, 3 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV32_Function_With_Default_Argument_Value_2()
        {
            string code = @"
def foo  ( a : int = 5, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
def foo  ( a : double = 5, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV32_Function_With_Default_Argument_Value_3()
        {
            string code = @"
def foo  ( a : int, b : double = 5, c : bool = true)
{
	return = x = c == true ? a  : b;
}
def foo2  ( a , b = 5, c = true)
{
	return = x = c == true ? a  : b;
}
d1 = foo2 (  );
d2 = foo2 ( 1 );
d3 = foo2 ( 2, 3 );
d4 = foo2 ( 4, 5, false );
d5 = 
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 2, 3 );
	c4 = foo ( 4, 5, false );
	return = { c1, c2, c3, c4 };
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV33_Function_Overloading()
        {
            string code = @"
def foo  ( a : int = 5, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
def foo  ( a : double = 6, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV33_Function_Overloading_2()
        {
            string code = @"
def foo  ( a : int , b : double , c : bool  )
{
	return = a;
}
def foo  ( a : double, b : double , c : int  )
{
	return = b;
}
[Imperative]
{
	c4 = foo ( 1, 2, 0 );
	c5 = foo ( 1.5, 2.5, 0 );
	c6 = foo ( 1.5, 2.5, true );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV33_Overloading_Different_Order_Of_Parameters()
        {
            string code = @"
def foo : int ( a :int, b : double )
{
	return = 2;
}
def foo : int( c : double, d : int )
{
	return = 3;
}
c = foo( 1,2.5 );
d = foo ( 2.5, 1 );
//e = foo ( 2.5, 2.5 );
f = foo ( 1, 2 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV35_Implicit_Conversion_Int_To_Double()
        {
            string code = @"
def foo : double ( a: double )
{
	return = a + 2.5;
}
	b = foo( 2 );
[Imperative]
{
	c = foo( 3 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV36_Implicit_Conversion_Return_Type()
        {
            string code = @"
def foo: bool ( a : double, b : double )
{
	return = 0.5;
}
c = foo ( 2.3 , 3 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV37_Overloading_With_Type_Conversion()
        {
            string code = @"
def foo : int ( a : double, b : double )
{
	return = 1;
}
def foo : int ( a : int, b : int )
{
	return = 2;
}
def foo : int ( a : int, b : double )
{
	return = 3;
}
a = foo ( 1,2 );
b = foo ( 2,2 );
c = foo ( 1, 2.3 );
d = foo ( 2.3, 2 );
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV40_Defect_1449956_3()
        {
            string code = @"
[Associative]
{
	def recursion  : int( a : int)
	{
		temp = [Imperative]
		{
			if ( a ==0 || a < 0)
			{
				return = 0;	
			}
			return = a + recursion( a - 1 );
		}		 
		return = temp;
	}
	x = recursion( 4 );
	y = recursion( -1 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV42_Defect_1454959()
        {
            string code = @"
def Level1 : int (a : int)
{
    return = Level2(a+1);
}
 
def Level2 : int (a : int)
{
    return = a + 1;
}
input = 3;
result = Level1(input); 
[Associative]
{
    a = Level1(4);
	b = foo (a);
	c = [Imperative]
	{
	    return = foo2( foo (a ) );
	}
}
def foo ( a )
{
    return = a + foo2(a);
}
def foo2 ( a ) 
{
    return = a;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV47_Defect_1456087()
        {
            string code = @"
def foo : int ( a : double, b : double )
{
	return = 2;
}
def foo : int ( a : int, b : int )
{
	return = 1;
}
def foo : int ( a : int, b : double )
{
	return = 3;
}
[Imperative]
{
	a = foo ( 1, 2 );
	b = foo ( 2.5, 2.5 );
	c = foo ( 1, 2.3 );
	d = foo ( 2.3, 2 );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV48_Defect_1456110()
        {
            string code = @"
def foo : int( a : int)
{
  temp = [Imperative]
  {
      if ( a == 0 )
      {
          return = 0; 
      }
      return = a ;
  } 
  return = temp;
}
x = foo( 0 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }
        [Test]
        public void DebugEQTV49_Defect_1456110()
        {
            string code = @"
def recursion : int(a : int)
{
    loc = [Imperative]
    {
        if (a <= 0)
        {
            return = 0; 
        }
        return = a + recursion(a - 1);
    }
    return = loc;
}
a = 10;
[Imperative]
{
	x = recursion(a); 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQTV49_Defect_1456110_2()
        {
            string code = @"
def recursion : int(a : int)
{
    loc = [Imperative]
    {
        if (a <= 0)
        {
            return = 0; 
        }
        return = a + recursion(a - 1);
    }
    return = loc;
}
a = 10;
[Imperative]
{
	x = recursion(a); 
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV55_Defect_1456571()
        {
            string code = @"
def foo(arr)
{
    retArr = 3;	
	[Imperative]
    {
		retArr = 5;
	}
    return = retArr;
}
	x = 0.5;
	x = foo(x);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        

        [Test]
        public void DebugEQTV58_Defect_1455090()
        {
            string code = @"
	def foo( a:int[] )
	{
		a[0][0] = 1;
		return = a;
	}
	b = { {0,2,3}, {4,5,6} };
	d = foo( b );
	c = d[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV59_Defect_1455278_2()
        {
            string code = @"
def multiply : double[] (a : double[])
{    
	temp = [Imperative]
    { 
		b = {0, 10};
		counter = 0; 
		
		for( y in a ) 
		{              
			b[counter] = y * y;   
			counter = counter + 1;           
		}                
        
		return = b;    
	}   
	return = temp;
}
	
	x = {2.5,10.0};
	x_squared = multiply( x );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV60_Defect_1455278_2()
        {
            string code = @"
def multiply : double[] (a : double[])
{    
	temp = [Imperative]
    { 
		b = {0, 10};
		counter = 0; 
		
		for( y in a ) 
		{              
			b[counter] = y * y;   
			counter = counter + 1;           
		}                
        
		return = b;    
	}   
	return = temp;
}
	
	x = {2.5,10};
	x_squared = multiply( x );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV60_Defect_1455278_3()
        {
            string code = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			while(counter < exponent )            
			{
				result = result * num;                
				counter =  counter + 1;            
			}            
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV61_Defect_1455278_3()
        {
            string code = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			while(counter < exponent )            
			{
				result = result * num;                
				counter =  counter + 1;            
			}            
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV61_Defect_1455278_4()
        {
            string code = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			if(num > exponent)
			{
				while(counter < exponent )            
				{
					result = result * num;                
					counter =  counter + 1;            
				}  
            }				
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV62_Defect_1455090()
        {
            string code = @"
	def foo:int[]..[] ( a:int[]..[] )
	{
		a[0][0] = 1;
		return = a;
	}
	b = { {0,2,3}, {4,5,6} };
	d = foo( b );
	c = d[0];
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV63_Defect_1455090_2()
        {
            string code = @"
	def foo ( a : double[]..[] )
	{
		a[0][0] = 2.5;
		return = a;
	}
	
	a = { {2.3,3.5},{4.5,5.5} };
	
	a = foo( a );
	c = a[0];
	
	[Imperative]
	{
		b = { {2.3}, {2.5} };
		b = foo( b );
		d = b[0];
	}
	
	
	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV71_Defect_1456108_2()
        {
            string code = @"
        def collectioninc: int[]( a : int[] )
	{
		j = 0;
		a = [Imperative]
		{
			for( i in a )
			{
				a[j] = a[j] + 1;
				j = j + 1;
			}
			return = a;
		}
		return = a;
	}
	d = { 1,2,3 };
	c = collectioninc( d );
    b;
        [Imperative]
	{
		b = collectioninc( d );
	}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV73_Defect_1451831()
        {
            string code = @"
[Associative]
{
  
	a = 1;
	b = 1;
	
	def test:int( a:int, b:int, c : int, d : int )
	{
		 
	    y = [Imperative]
		{
			if( a == b ) 
			{
				return = 1;
			}		
			else
			{
				return = 0;
			}
		}
		
		return = y + c + d;
	}
	
	c = 1;
	d = 1;
	
	y = test ( a , b, c, d);
	
		
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV75_Defect_1456870()
        {
            string code = @"
def foo1 ( b )
{
	return = b == 0 ? 0 : b+1;
	
}
def foo2 ( x )
{
	y = [Imperative]
	{
	    if(x > 0)
		{
		   return = x >=foo1(x) ? x : foo1(x);
		}
		return = x >=2 ? x : 2;
	}
	x1 = y == 0 ? 0 : y;
	return = x1 + y;
}
a1 = foo1(4);
a2 = foo2(3);
//thisTest.Verification(mirror, ""a1"", 5, 0); // fails
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV76_Defect_1456112()
        {
            string code = @"
def foo1 : double (arr : double[])
{
    return = 0;
}
arr = {1,2,3,4};
sum = foo1(arr);
def foo2 : double (arr : double)
{
    return = 0;
}
arr1 = {1.0,2.0,3.0,4.0};
sum1 = foo2(arr1);
sum2 = foo1(arr);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV76_Defect_1456112_2()
        {
            string code = @"
def foo1 : double (arr : double[][])
{
    return = arr[0][0];
}
[Imperative]
{
	arr1 = { {1, 2.0}, {true, 4} };
	sum1 = foo1(arr);
	x = 1;
	arr2 = { {1, 2.0}, {x, 4} };
	sum2 = foo1(arr2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV81_Defect_1458187()
        {
            string code = @"
def foo ( a )
{
	x = [Imperative]
	{
		if ( a == 0 )
		return = a;
		else
		return = a + 1;
	}
	return = x;
}
a = foo( 2 );
b = foo(false);
c = foo(true);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV81_Defect_1458187_2()
        {
            string code = @"
def foo ( a )
{
	x = (a == 1)?a:0;
	return = x + a;
}
a = foo( 2 );
b = foo(false);
c = foo(true);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV82_Defect_1460892()
        {
            string code = @"
def foo ( a : int )
{
    return  = a + 1;
}
def foo2 ( b : int, f1 : function )
{
    return = f1( b );
}
a = foo2 ( 10, foo );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV83_Function_Pointer()
        {
            string code = @"
def foo ( a : bool )
{
    return  = a ;
}
def foo2 ( b : int, f1 : function )
{
    return = f1( b );
}
a = foo2 ( 0, foo );
def poo ( a : int )
{
    return  = a ;
}
def poo2 ( b : bool, f1 : function )
{
    return = f1( b );
}
a2 = poo2 ( false, poo );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV83_Function_Pointer_Collection()
        {
            string code = @"
def count ( a : bool[]..[] )
{
    c = 0;
	c = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = c ;
}
def foo ( b : bool[]..[], f1 : function )
{
    return = count( b );
}
a = foo ( { true, false, { true, true } },  count );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV84_Function_Pointer_Implicit_Conversion()
        {
            string code = @"
def count ( a : int[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : double[], f1 : function )
{
    return = count( b );
}
a = foo ( { 1.0, 2.6 },  count );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV84_Function_Pointer_Implicit_Conversion_2()
        {
            string code = @"
def count ( a : double[]..[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[]..[], f1 : function )
{
    return = count( b );
}
a = foo ( { 1, 2 , {3, 4} },  count );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV84_Function_Pointer_Implicit_Conversion_3()
        {
            string code = @"
def count ( a : double[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[], f1 : function )
{
    return = count( b );
}
a = foo ( { 1, 2,  { 3, 4 } },  count );
d = foo ( { 2, 2.5, { 1, 1.5 }, 1 , false},  count );
// boolean can't be converted to double, so the following statement
// will generate a method resultion fail exception
// b = foo ( { true, false },  count );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV84_Function_Pointer_Implicit_Conversion_4()
        {
            string code = @"
def count ( a : double[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[], f1 : function )
{
    return = count( b );
}
[Imperative]
{
	a = foo ( { 1, 2,  { 3, 4 } },  count );
	d = foo ( { 2, 2.5, { 1, 1.5 }, 1 , false},  count );
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV84_Function_Pointer_Negative()
        {
            string code = @"
def greater ( a , b )
{
    c = [Imperative]
	{
	    if ( a > b )
		    return = a;
		else
		   return = b;
	}
	return  = c ;
}
def greatest ( a : double[], f : function )
{
    c = a[0];
	[Imperative]
	{
	    for ( i in a )
		{
		    c = f( i, c );
		}	
	}
	return  = c ;
}
[Imperative]
{
	a = greatest ( { 1.5, 6, 3, -1, 0 }, greater2 );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV84_Function_Pointer_Using_2_Functions()
        {
            string code = @"
def greater ( a , b )
{
    c = [Imperative]
	{
	    if ( a > b )
		    return = a;
		else
		   return = b;
	}
	return  = c ;
}
def greatest ( a : double[], greater : function )
{
    c = a[0];
	[Imperative]
	{
	    for ( i in a )
		{
		    c = greater( i, c );
		}	
	}
	return  = c ;
}
def foo ( a : double[], greatest : function , greater : function)
{
    return  = greatest ( a, greater );
}
[Imperative]
{
	a = foo ( { 1.5, 6, 3, -1, 0 }, greatest, greater );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV86_Defect_1456728()
        {
            string code = @"
def f1 (arr :  double[] )
{
    return = arr;
}
def f2 (arr :  double[] )
{
    return = { arr[0], arr[1] };
}
a = f1( { null, null } );
b = f2( { null, null } );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV88_Defect_1463489()
        {
            string code = @"
def foo: bool (  )
{
	return = 0.24;
}
c = foo ( ); //expected true, received 
d = [Imperative]
{
    return = foo();
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV88_Defect_1463489_2()
        {
            string code = @"
def foo: bool ( x : bool )
{
	return = x && true;
}
c = foo ( 0.6 ); 
c1 = foo ( 0.0 ); 
d = [Imperative]
{
    return = foo(-3.5);
}
d1 = [Imperative]
{
    return = foo(0.0);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060()
        {
            string code = @"
def foo ( x : double[])
{
    return = x;
}
a2 = { 2, 4, 3.5 };
b2 = foo (a2);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_2()
        {
            string code = @"
def foo ( x : double[])
{
    return = x;
}
a2 = { 2, 4, 3};
b2 = foo ( a2 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_3()
        {
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { 2, 4.1, 3.5};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_4()
        {
            string code = @"
def foo ( x : int)
{
    return = x;
}
a1 = { 2, 4.1, false};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_5()
        {
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { 2, 4.1, {1,2}};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_6()
        {
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { null, 5, 6.0};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_7()
        {
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { null, null, null};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_8()
        {
            string code = @"
def foo:int[]( x : int[])
{
    return = x;
}
a1 = {1.1,2.0,3};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV89_typeConversion_FunctionArguments_1467060_9()
        {
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { 1, null, 6.0};
b1 = foo ( a1 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV90_Defect_1463474_2()
        {
            string code = @"
a = 3;
def foo : void  ( )
{
	a = 2;		
}
foo();
b1 = a;	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV90_Defect_1463474_3()
        {
            string code = @"
a = 3;
def foo : void  (  )
{
	a = 2;
    return = -3;	
}
c1 = foo();
b1 = a;	
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV97_Heterogenous_Objects_As_Function_Arguments_With_Replication()
        {
            string code = @"
def foo ( x : double)
{
    return = x;
}
a1 = { 2.5, 4 };
b1 = foo ( a1 );
a2 = { 3, 4, 2.5 };
b2 = foo ( a2 );
a3 = { 3, 4, 2 };
b3 = foo ( a3 );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV98_Method_Overload_Over_Rank_Of_Array()
        {
            string code = @"
def foo(x : int[])
{ 
    return = 1;
}
def foo(x : int[]..[])
{ 
    return = 2;
}
def foo(x : int[][])
{ 
    return = 0;
}
    
x = foo ( { { 0,1}, {2, 3} } );
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTV99_Defect_1463456_Array_By_Reference_Issue_2()
        {
            string code = @"
def A (a: int [])
{
return = a;
}
val = {1,2,3};
b = A(val);
b[0] = 100; 
t = val[0]; //expected 100, received 1
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestBasicArrayMethods()
        {
            string code = @"
a = { 1, 2, { 3, 4, 5, { 6, 7, { 8, 9, { { 11 } } } } }, { 12, 13 } };
c = Count(a);
r = Rank(a);
a2 = Flatten(a);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestHostentityType()
        {
            string code = @"
[Associative]
{
	def factorial_local : hostentityid()
    {
        return = 11;
    }	
	x = factorial_local();
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestImport()
        {
            string code = @"
def dc_sqrt : double (val : double )
{
    return = val/2.0;
}
def dc_factorial : int (val : int )
{
    return = val * val ;
}
def dc_sin : double (val : double)
{
    return = val + val;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQTestStringConcatenation01__2_()
        {
            string code = @"
s1='a';
s2=""bcd"";
s3=s1+s2;
s4=""abc"";
s5='d';
s6=s4+s5;
s7=""ab"";
s8=""cd"";
s9=s7+s8;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestStringConcatenation01()
        {
            string code = @"
[Imperative]
{
	s1='a';
	s2=""bcd"";
	s3=s1+s2;
	s4=""abc"";
	s5='d';
	s6=s4+s5;
	s7=""ab"";
	s8=""cd"";
	s9=s7+s8;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestStringOperations()
        {
            string code = @"
[Imperative]
{
	s = ""ab"";
	r1 = s + 3;
	r2 = s + false;
	r3 = s + null;
	r4 = !s;
	r5 = s == ""ab"";
	r6 = s == s;
	r7 = ""ab"" == ""ab"";
	ns = s;
	ns[0] = 1;
	r8 = ns == {1, 'b'};
	//r9 = """" == """";
	//r10 = ("""" == null);
    r9 = s != ""ab"";
    ss = ""abc"";
    ss[0] = 'x';
    r10 = """" == null;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTestStringTypeConversion__2_()
        {
            string code = @"
def foo:bool(x:bool)
{
    return=x;
}
r1 = foo('h');
r2 = 'h' && true;
r3 = 'h' + 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTest_4_10_contains()
        {
            string code = @"
a = 5;
b = 7;
c = 9;
d = {a, b};
f = Contains(d, a); // true
g = Contains(d, c); // false
h = Contains({10,11},11); // true collection built ëon the flyí
				  // with ëliteralí values
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTest_4_11_indexOf()
        {
            string code = @"
a = 5;
b = 7;
c = 9;
d = {a, b, c};
f = IndexOf(d, b); // 1
g = d[f+1]; // c
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTest_4_13_Transpose()
        {
            string code = @"
a ={{1,2},{3,4}};
b = a[0][0]; // b = 1
c = a [0][1]; // c = 2
a = Transpose(a); // b = 1; c =3
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTest_4_14_isUniformDepth()
        {
            string code = @"
myNonUniformDepth2Dcollection = {{1, 2, 3}, {4, 5}, 6};
individualMemberB = myNonUniformDepth2Dcollection [0][1]; // OK, = B
individualMemberD = myNonUniformDepth2Dcollection [2][0]; // would fail
individualMemberE = myNonUniformDepth2Dcollection [2];    // OK, = 6
// Various collection manipulation functions are provided to assist with these issues, one of these functions is:
testDepthUniform = IsUniformDepth(myNonUniformDepth2Dcollection); // = false
testForDeepestDepth  = Rank(myNonUniformDepth2Dcollection); // = 2; current limitation :  1
 
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQTest_4_9_count()
        {
            string code = @"
count_test1=Count({1,2});   // 2 .. count of collection
a = {{1,2},3};		   // define a nested/ragged collection
count_test2=Count(a);       // 2 .. count of collection
count_test3=Count(a[0]);    // 2 .. count of sub collection
count_test4=Count(a[0][0]); // 0 .. count of single member
count_test5=Count(a[1]);    // 0 .. count of single member
count_test6=Count({}); 	   // 0 .. count of an empty collection
count_test7=Count(3); 	   // 0 .. count of single value
count_test8=Count(null);    // 0 .. count of null
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQUse_Keyword_Array_1463672()
        {
            string code = @"
def Create2DArray( col : int)
{
	result = [Imperative]
    {
		array = { 1, 2 };
		counter = 0;
		while( counter < col)
		{
			array[counter] = { 1, 2};
			counter = counter + 1;
		}
		return = array;
	}
    return = result;
}
x = Create2DArray( 2) ;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQZ001_Associative_Function_Regress_1454696()
        {
            string code = @"
    def Twice : double(array : double[])
    {
        return = array[0];
    }
    
    arr = {1.0,2.0,3.0};
    arr2 = Twice(arr);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQZ001_LanguageBlockScope_Defect_1453539()
        {
            string code = @"
[Associative]
{	
	a = 10;	
	b = true;	
	c = 20.1;	
}
// [Imperative]	
// {	
// aI = a;	
// bI = a;	
// cI = a;	
// }
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQZ003_Defect_1456728()
        {
            string code = @"
def function1 (arr :  double[] )
{
    return = { arr[0], arr [1] };
}
a = function1({null,null});
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQZ017_Defect_1456898_2()
        {
            string code = @"
def foo ( a : int[] )
{
	y = [Associative]
    {
		return = a[0];
    }
	x = [Imperative]
    {
		if ( a[0] == 0 ) 
		{
		    return = 0;
		}
		else
		{
			return = a[0];
		}
    }
    return = x + y;
}
[Imperative]
{
    a1 = foo( { 0, 1 } );
    b1 = foo( { 1, 2 } );
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQZ021_Defect_1458785_3()
        {
            string code = @"
def foo ( i:int[])
{
return = i;
}
x =  1;
a1 = foo(x);
a2 = 3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

       

       
        [Test]
        public void DebugEQarray__2_()
        {
            string code = @"
x :int = 0;
y :int = 0;
xSize :int = 2;
ySize :int = 3;
result :int = 0;
somelist : int[] = {11,102,1003,1004};
somelist2 : int[] = {x, y, xSize * 4, 1004 * ySize};
// Populate a multi-dimensional array
list2d[2][3];
list2d[0][0] = 10;
list2d[0][1] = 20;
list2d[0][2] = 30;
list2d[1][0] = 40;
list2d[1][1] = 50;
list2d[1][2] = 60;
// do somthing with those values
[Imperative]
{
	while( x < xSize )
	{
    	while( y < ySize )
	    {
	
    	    result = result + list2d[x][y];
        	y = y + 1;
    	}
    	x = x + 1;
	    y = 0;
	}
}
result = result * 10;
// Populate an array of integers and compute its average
// Populate an array of ints
list[5];
list[0] = 10;
list[1] = 20;
list[2] = 30;
list[3] = 40;
list[4] = 50;
// Declare counters and result storage
n :int = 0;
size :int = 5;
result = 0;
// Summation of elements in 'list' and storing them in 'result'
[Imperative]
{
	while( n < size )
	{
    	result = result + list[n];
    	n = n + 1;
	}
}
// Get the average
result = result / size;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQarray()
        {
            string code = @"
[Associative]
{
/*
	a = {1001,1002};
	a[0] = 1234;
	a[1] = 5678;
	x = a[1];
	y = a[0];
	
	
	b = {101, 102, {103, 104}, 105};
	b[2][1] = 100001;
	
	c = {
			101,    
			102, 
			{103, 104}, 
			{{1001, 2002}, 1},
			5
		};
	c[2][1]		= 111111;
	c[3][0][1]	= 222222;
	c[3][0][0]	= 333333;
	
	d = {
			{1, 0, 0, 0}, 
			{0, 1, 0, 0}, 
			{0, 0, 1, 0},
			{0, 0, 0, 1}
		};
	d[0][0] = c[2][1];
	d[1][1] = 2;
	d[2][2] = 2;
	d[3][3] = x;
	*/
	e = {10,{20,30}};
	e[1][1] = 40;
	dd = e[0];
	dd = e[1][0];
	dd = e[1][1];
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQarrayargs()
        {
            string code = @"
[Associative]
{
	//def inc : int( s : int )	
	//{
	//	return = s + 1;
	//}
	def scale2 : int( s : int )	
	{
		i = 2;
		return = s * i;
	}
	a = scale2(20);
	//b = scale2(20) + inc(2);
	//c = scale2(20) + inc(inc(2));
	//d = scale2(20) + inc(inc(inc(inc(inc(inc(inc(inc(inc(inc(inc(inc(2))))))))))));
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQbasicImport__2_()
        {
            string code = @"
def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQbasicImport1()
        {
            string code = @"
def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr;
}
a = 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQbasicImport3()
        {
            string code = @"
def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr * 2;
}
a = 3;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQblockassign_associative()
        {
            string code = @"
[Associative]
{
    def DoSomthing : int(p : int)
    {
        ret = p;       
        d = [Imperative]
        {
            loc = 20;
            return = loc;
        }
        return = ret * 100 + d;
    }
    a = DoSomthing(10);   
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQblockassign_imperative()
        {
            string code = @"
[Imperative]
{ 
    d = [Associative]
    {
        aa = 20;
        return = aa;
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQbranching()
        {
            string code = @"
def branchw()
{
    y : int;
	y = 0;
    [Imperative]{
        while(y < 10)
		{
			y = y + 2;	
    	}
    }
}
def branch1()
{
    x : int;
    y : int;
	x = 2;
	y = 20;
    [Imperative]
    {
        if (x > y)
		{
			y = 2;	
    	}
    }
    return = y;
}
def branch2()
{
    x : int;
    y : int;
	x = 1600;
	y = 201;
    [Imperative]
    {
		if( x > 2 )
        {
            y = 2;
            if (y > 200)
            {
                y = 200;
            }
        }
	}
}
def branch3()
{
	x : int;
    y : int;
	x = 64;
	y = 20;
    
    [Imperative]
    {
		if( x == 4 )
		{
			y = 2;	
        	if( y > 200 )
		    {
			    y = 200;
    		}
	    }
	
    	elseif( x > 20 )
		{
	        x = 20000;
    	}
    }
}
def branch4()
{
	x : int;
    y : int;
    x = 2;
    
	[Imperative]
    {
		if( x > 2 )
		{
			y = 2;
		}
	    else
	    {
	        x = 100;
		}
	    y = 2000;
	    if( x > 2 )
		{
			y = 2;
		}
    }
}
def branch5 (p : int)
{
    x : int;
    y : int;
    a = 4000;
	x = a;
	[Imperative]
    {
        if ( x > p )
		{
			x = 0;
		}
	    elseif( x > a )
	    {
	        x = p;
	    }
	    else
    	{
	        x = 20;
    	}
    }
    x = a;
}    
def branch6()
{
    x : int;
    y : int;
	x = 100;
	y = 200;
	[Imperative]
    {
    	if( y > 0 )
		{	
			y = 0;	
		}	
	    elseif( y > x )	
	    {	
	        y = 10;	
	    }	
	    elseif( y > 20 )	
	    {	
	        y = 20;	
	    }	
	    else	
	    {	
	        y = 3000;	
	        if( x > y )	
		    {	
			    y = 2000;	
		    }	
	        else	
	        {	
	            x = 2000;	
         	}
     	}
    }
    y = 40;
}
def branch7(p:int)
{
    x : int;
    y : int;
	x = 100;
	y = 200;
    
    [Imperative]
    {
		if( y > 0 )	
		{	
			y = 0;	
		}	
	    elseif( y > x )	
	    {	
	        if( x > y )	
		    {	
			    y = 2000;	
		    }	
	        elseif( y > p )	
	        {	
	            y = 20;	
	            if( p == y * x )	
		        {	
			        y = 2000 * 3 / p + 2;	
		        }	
	        }	
	    }	
	    elseif( y > 20 )	
	    {	
	        y = 20;	
	    }	
	    else	
	    {	
	        y = 3000;	
	        if( x > y )	
		    {	
			    y = 2000;	
		    }	
	        else	
	        {	
	            x = 2000;	
         	}
     	}
	}
    y = 40;
}
def branch55(p:int)
{
    x : int;
    a : int;
    a = 16;
	x = a;
	[Imperative]
	{
	    if( x > 64 )
		{
			x = 64;
		}
	    elseif( x > 32 )
	    {
	        x = 32;
	    }
	    elseif( x > 16 )
	    {
	        x = 16;
	    }
	    else
	    {
	        x = 12345;
     	}
    }
    a = x + p;
}
n : int;
n = branch1();
n = branch2();
n = branch3();
n = branch4();
n = branch5(100);
n = branch6();
n = branch7(10);
n = branch55(100);
count : int;
count = 10;
[Imperative]
{
	while(count > 2)
	{
	    count = count - 1;
	    branchw();
	}
}";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQdemo()
        {
            string code = @"
[Imperative]
{
    a = 10;
    b = 20;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQforloop()
        {
            string code = @"
[Imperative]
{
    a = {10,20,30,40};
    x = 0;
    for (val in a)
    {
        x = x + val;
    }
    x = 0;
    for (val in {100,200,300,400})
    {
        x = x + val;
    }
    x = 0;
    for (val in {{100,101},{200,201},{300,301},{400,401}})
    {
        x = x + val[1];
    }
    x = 0;
    for (val in 10)
    {
        x = x + val;
    }
    
    y = 0;
    b = 11;
    for (val in b)
    {
        y = y + val;
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQfunctionoverload()
        {
            string code = @"
[Associative]
{
    def f : int( p1 : int )
    {
	    x = p1 * 10;
	    return = x;
    }
    def f : int( p1 : int, p2 : int )
    {
	    return = p1 + p2;
    }
    a = 2;
    b = 20;
    i = f(a + 10);
    j = f(a, b);
}   
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQfusionarray()
        {
            string code = @"
[Imperative]
{
	x = 0;
	y = 0;
	xSize = 2;
	ySize = 3;
	result = 0;
    
	somelist = {11,102,1003,1004};
	somelist2 = {x, y, xSize * 4, 1004 * ySize};
	// Populate a multi-dimensional array
	list2d = {{10,20,30},{40,50,60}};
	// do somthing with those values
	while( x < xSize )
	{
		while( y < ySize )
		{
			result = result + list2d[x][y];
			y = y + 1;
		}
		x = x + 1;
		y = 0;
	}
	result = result * 10;
    
	// Populate an array of ints
	list = {10, 20, 30, 40, 50};
    
	// Declare counters and result storage
	n = 0;
	size = 5;
	result = 0;
    
    
	// Summation of elements in 'list' and storing them in 'result'
	while( n < size )
	{
		result = result + list[n];
		n = n + 1;
	}
	// Get the average
	result = result / size;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQheader1()
        {
            string code = @"
// import other module
import (""./include/header2.ds"");
x = 100;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQimport001()
        {
            string code = @"
def add : int(i : int, j : int)
{
	return = i + j;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQimport002()
        {
            string code = @"
def mul : int(i : int, j : int)
{
	return = i * j;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQimporttest()
        {
            string code = @"
import (""./header1.ds"");
import (""./include/header2.ds"");
a = 1;
b = 2;
[Associative]
{
    c = 3;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQmultilang()
        {
            string code = @"
[Associative]
{
	a = 100;
	b = 200;
	[Imperative] 
	{
		n = 300;
		n = n + 2;
	} 
	c = b + b;
}
[Imperative]
{
	n = 32;
	b = 64;
	if( n < b )
	{
		n = 1; 		
	}
	n = n + b;
} 
[Associative]
{
	a = 80;
	b = 160;
	
	[Imperative] 
	{
		n = 320;
		z = 640;
		if( n < z )
		{
			n = 1; 		
		}
		[Associative]
		{
			x = 10;
			y = 20;
			z = x + y * 2;
		}
		n = 20000;
	} 
	c = b + 2;
} 
[Associative]
{
	a = 80;
	b = 160;
	
	[Imperative] 
	{
		n = 320;
		z = 640;
		if( n < z )
		{
			n = 1; 		
		}
		[Associative]
		{
			xx = 1010;
			yy = xx + 2;
			[Imperative] 
			{
				n = 3200;
				z = 6400;
				if( n < z )
				{
					n = 1000000; 		
				}
				[Associative]
				{
					x = 1111;
					y = 2222;
					z = x + y * 2;
				}
				n = 12345;
			} 
		}
		n = n + 1;
	} 
	c = b + 2;
} 

";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQnesting()
        {
            string code = @"
[Imperative]
{
	a = 10;
	if(a >= 10)
	{
		x = a * 2;
		[Associative]
		{
			loc = x * a;
		}
	}
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQnull()
        {
            string code = @"
[Associative]
{
	x = 1;
    a1 = null;
    b1 = a1 + 2;
    c1 = 2 + a1 * x;
    [Imperative]
    {
        a = 2;
        b = null + 2;
        c = b * 3; 
    }
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQreplication()
        {
            string code = @"
[Associative]
{
	def sum : int(p1 : int, p2 : int)
	{
		return = p1 + p2;
	}
	//a = {1,2,3};
	//b = {4,5,6};
	//c = sum(a<1>, b<2>);
    c = sum(5, 2);
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQsimple()
        {
            string code = @"
[Imperative]
{
    b = 2 + 10.12345;
	c = 2 + 10;
	d = 2.12 + 10.12345 * 2;
    
	e = 2.000001 == 2;
	f = 2 == 2.000001;
	g = 2.000001 == 2.000001;
	h = 2.000001 != 2;
	i = 2 != 2.000001;
	j = 2.000001 != 2.000001;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQsimple2()
        {
            string code = @"
[Imperative]
{
	a = 2.12 + 100;
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }


        [Test]
        public void DebugEQtemp()
        {
            string code = @"
[Associative]
{
	def Level1 : int (a : int)
	{
		return = Level2(a+1);
	}
	
	def Level2 : int (a : int)
	{
		return = a + 1;
	}
	input = 3;
	result = Level1(input); 
	
}
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQtest__2_()
        {
            string code = @"
def length : int (pts : int[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = numPts;
}
z=length({1,2});
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQtest__4_()
        {
            string code = @"
a = {};
b = Average(a);
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQtest__5_()
        {
            string code = @"
a = 1;
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

        [Test]
        public void DebugEQupdate()
        {
            string code = @"
/*
a = 1;
b = a;
a = 10;
*/
/*
a = 1;
a = a + 1;
a = 10;
*/
/*
a = 1;
a = 10 + 20 * a;
a = 10;
*/
/*
a = 2;
x = 20;
y = 30;
a = x + y * a;
*/
/*
a = 2;
x = 20;
y = 30;
a = x * y + a;
*/
/*
def f : int(p : int)
{
    return = p + 1;
}
a = 10;
b = f(a);
*/
/*
def doo : int()
{
    d = 12;
    return = d;
}
def f : int(p : int)
{
    a = 10;
    b = a;
    a = p;
    return = b;
}
x = 20;
y = f(x);
x = 40;
*/
/*
a = 10;
b = 20;
c = a < b ? a : b;
*/
/*
a = 5;
b = ++a;
*/
";
            DebugTestFx.CompareDebugAndRunResults(code);
        }

    }

    [TestFixture]
    public class DebugUseCaseTesting
    {
        [Test]
        [Category("Debugger")]
        public void DebugEQtest_Copy_and_modiy_collection_1()
        {
            String src =
                @" 
a = 0..10;
b = a;
b[2] = 100; // modifying a member of a  copy of a collections
c = a;
d = b[0..(Count(b) - 1)..2]; // rnage expression used for indexing into a collection
 ";
            DebugTestFx.CompareDebugAndRunResults(src);
        }


        [Test]
        [Category("Debugger")]
        public void DebugEQtest_Simple_numeric_associative_2()
        {
            String src =
                @" 
a : int;
b : int;
[Associative]
{
    a = 10;
    b = 2 * a;
    a = a + 1;
}
 ";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("Debugger")]
        public void DebugEQtest_Simple_numeric_imperative_2()
        {
            String src =
                @" 
import(""ProtoGeometry.dll"");
a : int;
b : int;
[Imperative]
{
	a = 10;
	b = 2 * a;
	a = a + 1;
}
 ";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("Debugger")]
        public void DebugEQtest_2D_array_from_imperative_associative_code_1()
        {
            String src =
                @" 
results = { { } };
numCycles = 4;
[Imperative]
{
    for(i in(0..(numCycles-1)))
    {
        s = Print(""i = "" + i);
        
        [Associative]
        {
            results[i] = (0..5) * i;
           
            s = Print(results[i]);
        } 
    }
}
 ";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("Debugger")]
        public void DebugEQtest_2D_array_from_imperative_imperative_code_1()
        {
            String src =
                @" 
results = { { } };
numCycles = 4;
[Imperative]
{
    for(i in (0..(numCycles)))
	{
    	results[i] = { };
    
		for(j in(0..(numCycles-1)))
		{
    		results[i][j] = i * j;
	         
        	s = Print(results[i][j]);
		}
		s = Print(results[i]);
	} 
}
s = Print(results);
 ";
            DebugTestFx.CompareDebugAndRunResults(src);
        }

        [Test]
        [Category("Debugger")]
        public void DebugEQtest_Set_operation_functions_test_1()
        {
            String src =
                @" 
set = { true, { false, true } };
allFalseSet = AllFalse(set);
someFalseSet = SomeFalse(set);
someTrueSet = SomeTrue(set);
someNullsSet = SomeNulls(set);
setInsert = Insert(set, null, -1);
allFalseSetInsert = AllFalse(setInsert);
someFalseSetInsert = SomeFalse(setInsert);
someTrueSetInsert = SomeTrue(setInsert);
someNullsSetInsert = SomeNulls(setInsert);
countFalse = CountFalse(setInsert);
countTrue = CountTrue(setInsert);
containsNull = Contains(setInsert, null);
removeSetInsert = Remove(setInsert, 2);
removeNullsSetInsert = RemoveNulls(setInsert);
removeDuplicatesSetInsert = RemoveDuplicates(setInsert);
flattenSetInsert = Flatten(setInsert);
removeDuplicatesSetInsertFalttened = RemoveDuplicates(flattenSetInsert);
removeIfNotSetInsert = RemoveIfNot(flattenSetInsert, ""bool""); // (={})... this looks incorrect
one1Dcollection = { 3, 1 };
other1Dcollection = { 0, 1, 2, 3, 4 };
setDifferenceA = SetDifference(one1Dcollection, other1Dcollection);
setDifferenceB = SetDifference(other1Dcollection, one1Dcollection);
setIntersection = SetIntersection(other1Dcollection, one1Dcollection);
setUnion = SetUnion(other1Dcollection, one1Dcollection); 
 ";
            DebugTestFx.CompareDebugAndRunResults(src);
        }
    }
}

