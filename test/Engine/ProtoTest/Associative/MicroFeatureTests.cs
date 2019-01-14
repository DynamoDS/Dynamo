using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class MicroFeatureTests : ProtoTestBase
    {
        readonly string testCasePath = Path.GetFullPath(@"..\..\..\Scripts\Associative\MicroFeatureTests\");

        [Test]
        public void TestAssignment01()
        {

            String code =
@"
foo;
[Associative]
{
	foo = 5;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", 5);
        }

        [Test]
        public void TestAssignment02()
        {
            String code =
@"
boo;
moo;
scoo;
[Associative]
{
	boo = 5;
    moo = 7.2;
    scoo = 11;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("boo",5);
            thisTest.Verify("moo",7.2);
            thisTest.Verify("scoo",11);
        }

        [Test]
        public void TestNull01()
        {
            String code =
@"
x;
y;
a;
b;
c;
[Associative]
{
	x = null;
    y = x;
    a = null;
    b = a + 2;
    c = 2 + a * x;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
            thisTest.Verify("y", null);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
        }

        [Test]
        public void TestNull02()
        {
            String code =
@"
c;
[Associative]
{
    def foo : int ( a : int )
    {
        b = a + 1;
    }
	 
    c = foo(1);	
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
        }

        [Test]
        public void TestFunctions01()
        {
            String code =
@"
test;
test2;
test3;
def mult : int( s : int )	
{
    return = s * 2;
}
[Associative]
{
    test = mult(5);
    test2 = mult(2);
    test3 = mult(mult(5));
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("test",10);
            thisTest.Verify("test2",4);
            thisTest.Verify("test3",20);
        }

        [Test]
        public void TestFunctions02()
        {
            String code =
@"        
temp;
def test2 : int(b : int)
{
    return = b;
}
                
def test : int(a : int)
{
    return = a + test2(5);
}
[Associative]
{ 
    temp = test(2);
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("temp",7);
        }

        [Test]
        [Category("Failure")]
        public void TestDuplicateFunctionParams()
        {
            const string code = @"
def test : int(a : int, a : int)
{
    return = a + a;
}

temp = test(1, 2);
";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(1);
            thisTest.Verify("temp", null);
        }

        [Test]
        public void TestFunctionsOverload02()
        {
            String code =
@"
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
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i",120);
            thisTest.Verify("j",22);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestFunctionsOverload03()
        {
            String code =
            @"
            def foo(a : int)
            {
	            return = a;
            }
            def foo(a : int[])
            {
	            return = a;
            }
            c = [1,2,3,4];
            d = foo(c);
            e = foo([5,6,7,8]);
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", new [] {1,2,3,4});
            thisTest.Verify("e", new [] {5, 6, 7, 8});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestDynamicDispatch01()
        {
            String code =
@"
    def foo(x:int)
    {
        return = x;
    }
    def ding(x:int)
    {
        return = (x < 0) ? 2 * x : x;
    }
    t1 = foo(ding(-1));
    t2 = foo(ding(2));
    arr = [1, 2];
    arr[1] = 100;
    t3 = foo(arr[1]);    
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t1",-2);
            thisTest.Verify("t2",2);
            thisTest.Verify("t3",100);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestClasses01()
        {
            String code =
@"
import(""FFITarget.dll"");
p = DummyPoint.ByCoordinates(123.0, 345.0, 567.0);
a = p.X;
b = p.Y;
c = p.Z;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",123.0);
            thisTest.Verify("b",345.0);
            thisTest.Verify("c",567.0);
        }

        [Test]
        public void TestClasses06()
        {
            String code =
@"  
import(""FFITarget.dll"");
p1 = DummyPoint.ByCoordinates(1,1,1);
p2 = DummyPoint.ByCoordinates(10,10,10);
line = DummyLine.ByStartPointEndPoint(p1, p2);
a = line.Start.X;
b = line.Start.Y;
c = line.Start.Z;
x = line.End.X;
y = line.End.Y;
z = line.End.Z;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",1.0);
            thisTest.Verify("b",1.0);
            thisTest.Verify("c",1.0);
            thisTest.Verify("x",10.0);
            thisTest.Verify("y",10.0);
            thisTest.Verify("z",10.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestClasses07()
        {
            String code =
@"  
mx : var[];
def vector2D()
{
	mx = [10,20]; 
}
def ModifyMe : int()
{
    mx[1] = 64;
    return = mx[1];
}
vector2D();
x = ModifyMe();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",64);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestClassFunction02()
        {
            String code =
@"
    val : var;
	def sum : int (p : int)
    {
        return = p + 10;
    }
    val = sum(2);
    x = val;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",12);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestClassProperty()
        {
            String code =
@"  
import(""FFITarget.dll"");
p1 = DummyPoint.ByCoordinates(1.0,1.0,1.0);
p2 = DummyPoint.ByCoordinates(10.0,10.0,10.0);
line = DummyLine.ByStartPointEndPoint(p1, p2);
a;b;c;x;y;z;
[Associative]
{
    a = line.Start.X;
    b = line.Start.Y;
    c = line.Start.Z;
    x = line.End.X;
    y = line.End.Y;
    z = line.End.Z;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",1.0);
            thisTest.Verify("b",1.0);
            thisTest.Verify("c",1.0);
            thisTest.Verify("x",10.0);
            thisTest.Verify("y",10.0);
            thisTest.Verify("z",10.0);
        }

        [Test]
        public void TestClassFunction10()
        {
            String code =
@"
class A  
{   
    x : int;
    constructor Create(p : B)
    {
       x = p.a; 
    }
}
    
class B
{
    a : int;
    constructor Create(p : A)
    {
        a = p.x;
    }
}
    
aa = 2;
";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void TestClassFunction11()
        {
            String code =
@"
class Point
{
    context : var;
    x : var;
    constructor Create(cs : CoordinateSystem, xx : double)
    {
        context= cs;
        x = xx;
    }
}
class CoordinateSystem
{
    origin : var;
    constructor Create(orig : Point)
    {
        origin = orig;
    }
}
cs = null;
p = Point.Create(cs, 10.0);
cs2 = CoordinateSystem.Create(p);
xval = cs2.origin.x;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("xval",10);
        }

        [Test]
        public void TestStaticUpdate01()
        {
            string code = @"
class Base
{
    public static x;
}
Base.x = 1;
t = Base.x;
Base.x = 10; 
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t",10);
        }

        [Test]
        public void TestStaticUpdate02()
        {
            string code = @"
class Base
{
    static x : int[];
}
t = Base.x;
Base.x = [ 1, 2 ];   
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 1, 2 });
        }

        [Test]
        public void TestStaticProperty01()
        {
            string code = @"
class A
{
    static x:int;
}
a = A.A();
a.x = 3;
t1 = a.x;
b = A.A();
t2 = b.x;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t1",3);
            thisTest.Verify("t2",3);
        }

        [Test]
        public void TestStaticProperty02()
        {
            string code = @"
class S
{
	public static a : int;
}
class C
{
    public x : int;
    constructor C()
    {
        S.a = 2;
    }
}
p = C.C();
b = S.a;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",2);
        }

        [Test]
        public void TestTemporaryArrayIndexing01()
        {
            string code = @"t = [1,2,3,4][3]; ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing02()
        {
            string code = @"
    t = [[1,2], [3,4]][1][1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing03()
        {
            string code = @"
    t = ([[1,2], [3,4]])[1][1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing04()
        {
            string code = @"
    t = ([[1,2], [3,4]][1])[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing05()
        {
            string code = @"
    t = [1,2,3,4,5][1..3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3, 4 });
        }

        [Test]
        public void TestTemporaryArrayIndexing06()
        {
            string code = @"
    t = (1..5)[1..3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3, 4 });
        }

        [Test]
        public void TestTemporaryArrayIndexing07()
        {
            string code = @"
    t = ([1,2,3] + [4, 5, 6])[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 7);
        }

        [Test]
        public void TestArray001()
        {
            String code =
@"
x;
y;
a;
[Associative]
{
	a = [1001,1002];
    x = a[0];
    y = a[1];
    a[0] = 23;
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",23);
            thisTest.Verify("y",1002);
            thisTest.Verify("a", new object[] { 23, 1002});
        }

        [Test]
        public void TestArray002()
        {
            String code =
@"
    def foo : int (a : int[])
    {           
        return = a[0];
    }
            
    arr = [100, 200];            
    b = foo(arr);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",100);
        }

        [Test]
        public void TestArray003()
        {
            String code =
@"
a = [0,1,2];
t = [10,11,12];
a[0] = t[0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 10, 1, 2});
        }

        [Test]
        public void TestIndexingIntoArray01()
        {
            String code =
@"
class A
{
    fx :var;
    constructor A(x : var)
    {
        fx = x;
    }
}
fa = A.A(10..12);
r1 = fa.fx;
r2 = fa[0].fx; // 10
r3 = fa.fx[0]; // 10
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r2", 10);
            thisTest.Verify("r3", 10);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestIndexingIntoArray02()
        {
            String code =
@"
x=[Imperative]
{
    return = ding()[1][1];
}
def ding()
{
    return = [[1,2,3], [4,5,6]];
}
def foo()
{
    return = [1, 2, 3];
}
y = foo()[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);
            thisTest.Verify("y", 2);
        }

        [Test]
        public void TestArrayOverIndexing01()
        {
            string code = @"
[Imperative]
{
    arr1 = [true, false];
    arr2 = [1, 2, 3];
    arr3 = [false, true];
    t = arr2[1][0];
}
";
            thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.MethodResolutionFailure);
        }

        [Test]
        public void TestDynamicArray001()
        {
            String code =
@"
a = [10,20];
a[2] = 100;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] {10, 20, 100});
        }

        [Test]
        public void TestDynamicArray002()
        {
            String code =
@"
t = [];
t[0] = 100;
t[1] = 200;
t[2] = 300;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] {100, 200, 300});
        }

        [Test]
        public void TestDynamicArray003()
        {
            String code =
@"
t = [];
t[0][0] = 1;
t[0][1] = 2;
a = t[0][0];
b = t[0][1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",1);
            thisTest.Verify("b",2);
        }

        [Test]
        public void TestDynamicArray004()
        {
            String code =
@"
t = [];
t[0][0] = 1;
t[0][1] = 2;
t[1][0] = 10;
t[1][1] = 20;
a = t[1][0];
b = t[1][1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",10);
            thisTest.Verify("b",20);
        }

        [Test]
        public void TestDynamicArray005()
        {
            String code =
@"
t = [0,[20,30]];
t[1][1] = [40,50];
a = t[1][1][0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",40);
        }

        [Test]
        public void TestDynamicArray006()
        {
            String code =
@"

i = [Imperative]
{
    t = [];
    t[0][0] = 1;
    t[0][1] = 2;
    t[1][0] = 3;
    t[1][1] = 4;
    a = t[0][0];
    b = t[0][1];
    c = t[1][0];
    d = t[1][1];
    return [a, b, c, d];
}
a = i[0];
b = i[1];
c = i[2];
d = i[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",1);
            thisTest.Verify("b",2);
            thisTest.Verify("c",3);
            thisTest.Verify("d",4);
        }

        [Test]
        public void TestDynamicArray007()
        {
            String code = @"
a[3] = 3;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { null, null, null, 3 });
        }

        [Test]
        public void TestDynamicArray008()
        {
            String code = @"
a[0] = false;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { false });
        }

        [Test]
        public void TestDynamicArray009()
        {
            String code = @"
a = false;
a[3] = 1;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { false, null, null, 1 });
        }

        [Test]
        public void TestDynamicArray010()
        {
            String code = @"
a = false;
a[1][1] = [3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { false, new object[] { null, new object[] { 3 } } });
        }

        [Test]
        public void TestDynamicArray011()
        {
            String code = @"
a[0] = 1;
a[0][1] = 2;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 1, 2 } });
        }

        [Test]
        public void TestDynamicArray012()
        {
            String code = @"
a = 1;
a[-1] = 2;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 2 });
        }

        [Test]
        public void TestDynamicArray013()
        {
            String code = @"
a = 1;
a[-3] = 2;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 2, null, 1 });
        }

        [Test]
        public void TestDynamicArray014()
        {
            String code = @"
a = [1, 2];
a[3] = 3;
a[-5] = 100;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 100, 1, 2, null, 3 });
        }

        [Test]
        public void TestDynamicArray015()
        {
            String code = @"
a = 1;
a[-2][-1] = 3;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 3 }, 1 });
        }

        [Test]
        public void TestDictionary01()
        {
            // Using string as a key
            String code = @"
a = {""x"" : 42};
r = a [""x""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDecimalArrayIndex()
        {
            // Double value can't be used as a key
            String code = @"
a = [1, 2, 3];
r = a [1.3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 2);
        }

        [Test]
        public void TestExpressionArrayIndex()
        {
            // Using boolean value as a key
            String code = @"
a = [];
a[0] = 42;
a[1] = 41;
r1 = a [1 == 1 ? 0 : 1];
r2 = a [0 == 1 ? 0 : 1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 42);
            thisTest.Verify("r2", 41);
        }

        [Test]
        public void TestDictionary04()
        {
            // Using character value as a key
            String code = @"
a = {""x"" : 42};
r1 = a[""x""];
r2 = a[""y""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 42);
            thisTest.Verify("r2", null);
        }

        [Test]
        public void TestDictionary05()
        {
            // Using class instance as a key 
            String code = @"

arr = {""a"" : 41};
arr = arr.SetValueAtKeys(""a"", 42);
r = arr[""a""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary06()
        {
            // Test replication on array indexing on LHS
            // using character as a key
            String code = @"
strs = [""x"", 'c', 'b'];
arr = [ 11, 13, 17];
dict = Dictionary.ByKeysValues(strs, arr);
r1 = dict[""x""];
r2 = dict['c'];
r3 = dict['b'];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 13);
            thisTest.Verify("r3", 17);
        }

        [Test]
        public void TestDictionary07()
        {
            // Test replication on array indexing on RHS
            String code = @"
strs = [""x"", 'c', 'b'];
arr = [ 11, 13, 17];
            dict = Dictionary.ByKeysValues(strs, arr);
            values = dict[strs];
            r1 = values[0];
            r2 = values[1];
            r3 = values[2];
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 13);
            thisTest.Verify("r3", 17);
        }

        [Test]
        public void TestDictionary08()
        {
            // Test for 2D array
            String code = @"
arr = [[1, 2], [3, 4]];
arr[1] = {""xyz"" : 42};
r1 = arr[1][""xyz""];
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 42);
        }

        [Test]
        public void TestDictionary09()
        {
            // Copy array should also copy key-value pairs
            String code = @"
a = {""xyz"" : 42};
b = a;
r = b[""xyz""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary10()
        {
            // Key-value shouldn't be disposed after scope
            String code = @"
a = [Imperative]
{
    b = {""xyz"" : 42};
    return = b;
}

r = a[""xyz""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }


        [Test]
        public void TestDictionary14()
        {
            // Copy array should also copy key-value pairs
            String code = @"
a = {""true"" : 42};
def foo(x: var[]..[])
{
    return = x[""true""];
}
r = foo(a);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestMixedArray()
        {
            // Type conversion applied to values as well
            String code = @"
a:int[] = [1.1, 2.2, 3.3];
a[1] = 42.4;
r = a;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new[] { 1, 42.4, 3 });
        }

        [Test]
        public void TestTypedArrayAssignment()
        {
            // Type conversion applied to values as well
            String code = @"
a = [1.1, 2.2, 3.3];
a[1] = 42.4;
b:int[] = a;
r = b[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestImperativeArray()
        {
            // Test for-loop to get values
            String code = @"
a = [1, 2, 3];
a[2] = 42;
r = [Imperative]
{
    x = null;
    for (v in a)
    {
        x = v;
    }
    return = x;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestFunctionArray()
        {
            // Test replication for function call
            String code = @"
a = [1, 2, 3];
a[3] = 42;

def foo(x) { return = x;};
r1 = foo(a);
r2 = r1[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r2", 42);
        }

        [Test]
        public void TestFunctionArrays()
        {
            // Test replication for function call
            String code = @"
a = [1, 2, 3];
a[3] = 21;
b = [1, 2, 3];
b[3] = 21;

def foo(x, y) { return = x + y;
 };
sum = foo(a, b);
r = sum[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestArrayIndexingAssignment()
        {
            // Test replication for array indexing
            String code = @"
a = [1, 2, 3];
a[3] = 42;
b = [];
b[a] = 1;
b[42] = 42;
c = b[a];
r = c[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary19()
        {
            // Test builtin functions GetKeys() for array
            String code = @"
import(""BuiltIn.ds"");
a = {""x"" : ""foo""};
r = List.Count(a.Keys);
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 1);
        }

        [Test]
        public void TestDictionary20()
        {
            // Test builtin functions GetValues() for array
            String code = @"
a = {""x"" : ""foo""};
r = a.Values;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new[] { "foo" });
        }

        [Test]
        public void TestDictionary21()
        {
            // Test builtin functions ContainsKey() for array
            String code = @"
a = {""x"" : ""foo""};
r = Dictionary.ValueAtKey(a, ""x"");
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "foo");
        }

        [Test]
        public void TestDictionary22()
        {
            // Test builtin functions RemoveKey() for array
            String code = @"
a = {""x"" : ""foo""};
r1 = Dictionary.RemoveKeys(a, ""x"");
r2 = Dictionary.ValueAtKey(r1, ""x"");
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r2", null);
        }

        [Test]
        public void TestForArray()
        {
            // Test for-loop
            String code = @"
r = [Imperative]
{
    a = [1, 5, 7];
    x = 0; 
    for (v in a) 
    {
        x = x + v;
    }
    return = x;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 13);
        }

        [Test]
        public void TestForEmptyArray()
        {
            // Test for-loop
            String code = @"
r = [Imperative]
{
    a = [];
    x = 0;
    for (v in a) 
    {
        x = x + v;
    }
    return = x;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 0);
        }

        [Test]
        public void TestDictionaryRegressMAGN337()
        {
            string code = @"
     a = [ 1, 2, 3 ];
    b = [""x"",""y""];
                
def foo(a1 : var[], b1 : var[])
{

    a1 = Dictionary.ByKeysValues(b1, a1);
    return =a1;
}
z1 = foo(a, b);
r1=z1[""x""];
r2=z1[""y""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
        }


        [Test]
        public void TestDictionaryRegressMAGN619()
        {
            string code = @"
a = {""null"" : 5};
c = a.Count;
r = a[""null""];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("r", 5);
        }

        [Test]
        public void TestArrayCopyAssignment01()
        {
            String code = @"
a = [1, 2, 3];
b[1] = a;
b[1][1] = 100;
z = a[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2);
        }

        [Test]
        public void TestArrayCopyAssignment02()
        {
            String code = @"
a = [1, 2, 3];
b = [1, 2, 3];
b[1] = a;
b[1][1] = 100;
z = a[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2);
        }

        [Test]
        public void TestDynamicArray016()
        {
            String code = @"
a = [[1, 2], [3, 4]];
a[-3][-1] = 5;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 5 }, new object[] { 1, 2 }, new object[] { 3, 4 } });
        }

        [Test]
        public void TestArrayIndexReplication01()
        {
            string code = @"
a = 1;
a[1..2] = 2;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, 2, 2 });
        }

        [Test]
        public void TestArrayIndexReplication02()
        {
            string code = @"
a = [1, 2, 3];
b = a[1..2];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 2, 3 });
        }

        [Test]
        public void TestDynamicArrayNegative01()
        {
            string code = @"
x = (null)[0];
";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("x", null);
        }

        [Test]
        public void TestDynamicArrayNegative02()
        {
            string code = @"
x;
[Imperative]
{
x = (null)[0];
}
";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("x", null);
        }

        [Test]
        public void TestDynamicArrayNegative03()
        {
            // Test calling a function on an empty array
            string code = @"
p = [];
x = p.f();
";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("x", new object[] { });
        }


        [Test]
        public void TestReplicationGuidesOnFunctions01()
        {
            string code = @"
def f()
{
    return = [ 1, 2 ];
}
def g()
{
    return = [ 3, 4 ];
}
x = f()<1> + g()<2>;
a = x[0];
b = x[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 4, 5 });
            thisTest.Verify("b", new object[] { 5, 6 });
        }



        [Test]
        public void TestReplicationGuideWithLongestProperty01()
        {
            String code =
@"

def f(i:int, j:int)
{
    return = i + j;
}

a = 1..3;
b = 2..3;
c = f(a<1L>,b<2>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new object[] { 3, 4 }, new object[] { 4, 5 }, new object[] { 5, 6 } });
        }

        [Test]
        public void TestReplicationGuidesOnProperties()
        {
            string code = @"
                class A
                {
                    X : int[];
	                constructor A(a : int[])
                    {
                        X = a;
                    }
                }
                a = [A.A(0..2), A.A(3..5)];
                z = a.X<1> + a.X<2>;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", new object[]
            {
                new object[]
                {
                    new[] {0, 2, 4}, new[] {3, 5, 7}
                },
                new object[]
                {
                    new[] {3, 5, 7}, new[] {6, 8, 10}
                }
            });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReplicationGuidesOnDotOps01()
        {
            string code = @"
v = [1,2];
c = v<1> + v<2>;
x = c[0];
y = c[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReplicationGuidesOnDotOps02()
        {
            string code = @"
def f(a:int,b:int)
{
    return = 1;
}
x = [1,2];
y = [3,4];
a = f(x<1>, y<2>);
b = a[0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1, 1 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestReplicationGuidesOnDotOps03()
        {
            string code = @"
def f(a:int,b:int)
{
    return = 1;
}
a = f([1,2]<1>, [3,4]<2>);
b = a[0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1, 1 });
        }

        [Test]
        public void TestTypeArrayAssign4()
        {
            string code = @"
a:int[] = [false, 2, 3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { null, 2, 3 });
        }

        [Test]
        public void TestTypeArrayAssign5()
        {
            string code = @"
a = [false, 2, true];
b:int[] = a;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { null, 2, null });
        }

        [Test]
        public void TestTypeArrayAssign6()
        {
            string code = @"
a:int = 2;
a[1] = 3;;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 2, 3 });
        }

        [Test]
        public void TestTypeArrayAssign_1467462()
        {
            string code = @"
x:int[] = 1..4;
x[[2,3]] = [1, 2];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void NestedBlocks001()
        {
            String code =
@"
a;
[Associative]
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",1);
        }

        [Test]
        public void LogicalOp001()
        {
            String code =
                        @"
e;
                        [Associative]
                        {
	                        a = true;
	                        b = false;
                            c = 1;
                            d = a && b;
                            e = c && d;
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("e", false);
        }

        [Test]
        public void LogicalOp002()
        {
            String code =
                        @"
e;
                        [Associative]
                        {
	                        a = true;
	                        b = false;
                            c = 1;
                            d = a || b;
                            e = c || d;
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("e", true);
        }

        [Test]
        public void LogicalOp003()
        {
            String code =
                        @"
c;
                        [Associative]
                        {
	                        a = true;
	                        b = false;
                            c = !(a || !b);
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", false);
        }

        [Test]
        public void DoubleOp()
        {
            String code =
                        @"
b;
                        [Associative]
                        {
	                        a = 1 + 2;
                            b = 2.0;
                            b = a + b; 
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",5.0);
        }

        [Test]
        public void TestEq()
        {
            string code= @"
class A {}
a = A();
b = 42;
c = a == b;
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("c", false);
        }

        [Test]
        public void RangeExpr001()
        {
            String code =
                        @"
b;
c;
d;
e;
f;
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1);
            thisTest.Verify("c",2);
            thisTest.Verify("d",3);
            thisTest.Verify("e",4);
            thisTest.Verify("f",5);
        }

        [Test]
        public void RangeExpr002()
        {
            String code =
                        @"
b;
c;
d;
e;
                        [Associative]
                        {
	                        a = 1.5..5..1.1;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];                             
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1.5);
            thisTest.Verify("c",2.6);
            thisTest.Verify("d",3.7);
            thisTest.Verify("e",4.8);
        }

        [Test]
        public void RangeExpr003()
        {
            String code =
                        @"
b;
c;
d;
e;
                        [Associative]
                        {
	                        a = 15..10..-1.5;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];                             
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",15.0);
            thisTest.Verify("c",13.5);
            thisTest.Verify("d",12.0);
            thisTest.Verify("e",10.5);
        }

        [Test]
        public void RangeExpr004()
        {
            String code =
                        @"
b;
c;
d;
e;
f;
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",0);
            thisTest.Verify("c",3.75);
            thisTest.Verify("d",7.5);
            thisTest.Verify("e",11.25);
            thisTest.Verify("f",15);
        }

        [Test]
        public void RangeExpr005()
        {
            String code =
                        @"
b;
c;
d;
e;
f;
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",0);
            thisTest.Verify("c",3.75);
            thisTest.Verify("d",7.5);
            thisTest.Verify("e",11.25);
            thisTest.Verify("f",15);
        }

        [Test]
        public void RangeExpr06()
        {
            string code = @"
x1 = 0..#(-1)..5;
x2 = 0..#0..5;
x3 = 0..#1..10;
x4 = 0..#5..10;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", new object[] { });
            thisTest.Verify("x3", new object[] { 0 });
            thisTest.Verify("x4", new object[] { 0, 10, 20, 30, 40 });
        }

        [Test]
        public void InlineCondition001()
        {
            String code =
                        @"
	                        a = 10;
                            b = 20;
                            c = a < b ? a : b;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",10);
        }

        [Test]
        public void InlineCondition002()
        {
            String code =
                        @"	
	                        a = 10;
			                b = 20;
                            c = a > b ? a : a == b ? 0 : b; 
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",20);
        }

        [Test]
        public void InlineCondition003()
        {
            String code =
                        @"
a = [11,12,10];
t = 10;
b = a > t ? 2 : 1;
x = b[0];
y = b[1];
z = b[2];
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",2);
            thisTest.Verify("y",2);
            thisTest.Verify("z",1);
        }

        [Test]
        public void InlineCondition004()
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
a = [10,0,10];
t = 1;
b = a > t ? f(10) : g();
x = b[0];
y = b[1];
z = b[2];
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",11);
            thisTest.Verify("y",1);
            thisTest.Verify("z",11);
        }

        [Test]
        public void Modulo001()
        {
            String code =
                @"
                    a = 10 % 4; // 2
                    b = 5 % a; // 1
                    c = b + 11 % a * 3 - 4; // 0
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",0);
        }

        [Test]
        public void Modulo002()
        {
            String code =
               @"
                    a = 10 % 4; // 2
                    b = 5 % a; // 1
                    c = 11 % a == 2 ? 11 % 2 : 11 % 3; // 2
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",2);
        }

        [Test]
        public void Modulo003()
        {
            string code = @"a = 5.2 % 2.3;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 0.6);
        }

        [Test]
        public void ModuloByZero()
        {
            string code =
@"
a = 2 % 0;
b = 2.1 % 0;
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", double.NaN);

            var warnings = thisTest.GetTestRuntimeCore().RuntimeStatus.Warnings;
            Assert.IsTrue(warnings.Any(w => w.ID == ProtoCore.Runtime.WarningID.ModuloByZero));
        }

        [Test]
        public void NegativeIndexOnCollection001()
        {
            String code =
                @"
                    a = [1, 2, 3, 4];
                    b = a[-2]; // 3
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",3);
        }

        [Test]
        public void NegativeIndexOnCollection002()
        {
            String code =
                @"
                    a = [ [ 1, 2 ], [ 3, 4 ] ];
                    b = a[-1][-2]; // 3
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",3);
        }

        [Test]
        public void NegativeIndexOnCollection003()
        {
            String code =
                @"
                    class A
                    {
	                    x : var[];
	
	                    constructor A()
	                    {
		                    x = [ B.B(), B.B(), B.B() ];
	                    }
                    }
                    class B
                    {
	                    x : var[]..[];
	
	                    constructor B()
	                    {
		                    x = [ [ 1, 2 ], [ 3, 4 ],  [ 5, 6 ] ];		
	                    }
                    }
                    a = [ A.A(), A.A(), A.A() ];
                    b = a[-2].x[-3].x[-2][-1]; // 4
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void PopListWithDimension()
        {
            String code =
                @"
y : var;
z : var[]..[];

def A()
{
	y = 10;
    z = [ [ 40, 50 ], [ 60, 70 ] ];
}

z[1] = [ 1, 2 ];
watch1 = z[1][0];
watch2 = z[1][1];
z[1][0] = 10;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("watch1", 10);
            thisTest.Verify("watch2", 2);
        }

        [Test]
        public void TestUpdate01()
        {
            String code =
                @"
                    a = 1;
                    b = a;
                    a = 10;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",10);
            thisTest.Verify("b",10);
        }

        [Test]
        public void TestUpdate02()
        {
            String code =
                @"
                    a = 1;
                    b = b + a;
                    b = 2;
                    a = 10;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",2);
        }

        [Test]
        public void TestUpdate03()
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",40);
            thisTest.Verify("y",40);
        }

        [Test]
        public void TestUpdateRedefinition01()
        {
            String code =
                @"
a = 1;
c = 2;
b = a + 1;
b = c + 1;
a = 3;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",3);
        }

        [Test]
        public void TestUpdateRedefinition02()
        {
            String code =
                @"
                    a = 1;
                    a = a + 1;
                    a = 10;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",10);
        }

        [Test]
        public void TestArrayUpdate01()
        {
            String code =
                @"
a = [10,11,12];
t = 0;
i = a[t];
t = 2;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i",12);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestNoUpdate01()
        {
            String code =
                @"
def Trim(x)
{
    return = x - 1;
}
myline = 10;
myline = Trim(myline);
myline = Trim(myline);
length = myline;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("length",8);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestPropertyUpdate01()
        {
            String code =
                @"
p = 0;
a = p;
p = 2; 
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",2);
        }
        // Comment Jun: Investigate how replicating setters have affected this update

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestPropertyUpdate02()
        {
            String code =
                @"
p = 0;
b = 2;
p = b;
b = 10;
t = p;
                ";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467249 - Sprint25: rev 3468 : REGRESSION: class property update is not propagating");
            thisTest.Verify("t",10);
        }

        [Test]
        public void TestPropertyUpdate03()
        {
            String code =
                @"
class A
{
    x : int;	
    constructor A()
    {
        x = 1;
    }
}
class B
{
    m : var;	
    constructor B()
    {
        m = A.A();
    }
}
p = B.B();
a = p.m.x;
p.m.x = 2;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",2);
        }

        [Test]
        public void TestPropertyUpdate04()
        {
            String code =
                @"
class A
{
    x : int;	
    constructor A()
    {
        x = 1;
    }
}
class B
{
    m : var;	
    constructor B()
    {
        m = A.A();
    }
}
p = B.B();
b = 2;
p.m.x = b;
b = 10;
t = p.m.x;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t",10);
        }

        [Test]
        public void TestPropertyUpdate07()
        {
            String code =
                @"
class C
{
    x : int;
    constructor C(i:int)
    {
        x = i;
    }
}
i = 10;
a = C.C(i);
a.x = 15;
v = a.x;
i = 7;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v", 15);
        }

        [Test]
        [Category("Failure")]
        public void TestLHSUpdate01()
        {
            String code =
                @"
class C
{
    x : var;
    constructor C(i : int)
    {
        x = D.D(i);
    }
}
class D
{
    y : int = 0;
    constructor D(i : int)
    {
        y = i;
    }
}
a = C.C();
// must reexecute a.x.y = 10 because a.x was modified
a.x.y = 10;
i = a.x.y;
a.x = D.D(2);
j = a.x.y;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 10);
            thisTest.Verify("j", 10);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestLHSUpdate02()
        {
            String code =
                @"
a = 1;
b = a; // Should only update once
a = 10;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 10);
        }

        [Test]
        public void TestXLangUpdate01()
        {
            String code =
                @"
a;
b;
c = [Associative]
{
    a = 1;
    b = a;
    [Imperative]
    {
        a = a + 1;
    }
    return [a, b];
}
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new[] {1, 1});
        }

        [Test]
        public void TestXLangUpdate02()
        {
            String code =
                @"
a;
b;
[Associative]
{
    a = 1;
    b = a;
    a = 10;
    a = [Imperative]
    {
        return a + 1;
    }
}
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",11);
            thisTest.Verify("b",11);
        }

        [Test]
        public void TestXLangUpdate03()
        {
            String code =
                @"
a;b;c;d;
[Associative]
{
    a = 1;
    b = a;
    c = 10;
    d = c;
    a = [Imperative]
    {
        a = a + 1;
        c = c + a;
        return c;
    }
}
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",12);
            thisTest.Verify("b",12);
            thisTest.Verify("c",10);
            thisTest.Verify("d",10);
        }

        [Test]
        public void TestXLangUpdate04()
        {
            String code =
                @"
def f(p : int)
{
    return = p;
}
a = 1;
i = [Imperative]
{
    return = f(a);
}
a = 10;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i",10);
        }

        [Test]
        public void TestXLangUpdate_AssociativeTriggersAssociative01()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4585
            String code =
                @"
a = 1;
x = [Associative]
{
    return = a + 10;
}
a = 2;
                ";
            thisTest.RunScriptSource(code);
            string err = "MAGN-4585: Failure to trigger update in an inner associative block";
            thisTest.Verify("x",12);
        }

        [Test]
        public void TestXLangUpdate_AssociativeTriggersAssociative02()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4585
            String code =
                @"
a = 1;
x = [Associative]
{
    return = a + 100;
}


y = [Associative]
{
    return = a + 200;
}
a = 10;

                ";
            thisTest.RunScriptSource(code);
            string err = "MAGN-4585: Failure to trigger update in an inner associative block";
            thisTest.Verify("x",110);
            thisTest.Verify("y",210);
        }


        [Test]
        public void TestGCRefCount002()
        {
            String code =
                @"
import (""FFITarget.dll"");
def CreatePoint : Point(x : int, y : int, z : int)
{
	return = DummyPoint.ByCoordinates(x, y, z);
}
def getx : double(p : DummyPoint)
{
	return = p.X;
}
p = CreatePoint(5, 6, 7);
x = getx(p);
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5.0);
        }

        [Test]
        public void TestGlobalVariable()
        {
            String code =
                @"
                    gx = 100;
                    def f : int()
                    {
                        return = gx;
                    }
                    i = f();
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("gx", 100);
        }

        [Test]
        public void TestBasicArrayMethods()
        {
            string src = @"
import(""BuiltIn.ds"");
a = [ 1, 2, [ 3, 4, 5, [ 6, 7, [ 8, 9, [ [ 11 ] ] ] ] ], [ 12, 13 ] ];
c = List.Count(a);
r = List.Rank(a);
a2 = List.Flatten(a);";
            thisTest.RunScriptSource(src);
            thisTest.Verify("c",4);
            thisTest.Verify("r",6);
            thisTest.Verify("a2", new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13 });
        }

        [Test]
        public void TestStringConcatenation01()
        {
            string src = @"s1='a';
s2=""bcd"";
s3=s1+s2;

s4=""abc"";
s5='d';
s6=s4+s5;

s7=""ab"";
s8=""cd"";
s9=s7+s8;";
            thisTest.RunScriptSource(src);
            thisTest.Verify("s3", "abcd");
            thisTest.Verify("s6", "abcd");
            thisTest.Verify("s9", "abcd");
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestStringOperations()
        {
            string src = @"
s = ""ab"";
r1 = s + 3;
r2 = s + false;
r4 = !s;
r444 = !1;
r5 = s == ""ab"";
r6 = s == s;
r7 = ""ab"" == ""ab"";
ns = s;
ns[0] = 1;
r9 = s != ""ab"";
ss = ""abc"";
m = ss;
";
            thisTest.RunScriptSource(src);
            thisTest.Verify("r1", "ab3");
            thisTest.Verify("r2", "abfalse");
            thisTest.Verify("r4", false);
            thisTest.Verify("r444", false);
            thisTest.Verify("r5", true);
            thisTest.Verify("r6", true);
            thisTest.Verify("r7", true);
            thisTest.Verify("r9", false);
            thisTest.Verify("ss", "abc");
        }

        [Test]
        public void TestStringTypeConversion()
        {
            //Assert.Fail("DNL-1467239 Sprint 26 - Rev 3425 type conversion - string to bool conversion failing");
            string src = @"def foo:bool(x:bool)
{
    return=x;
}
r1 = foo('h');
r2 = 'h' && true;
r3 = 'h' + 1;";
            thisTest.RunScriptSource(src);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
            thisTest.Verify("r3", null);
        }

        [Test]
        public void TestTypeArrayAssign()
        {
            String code =
@"
t:int[] = [1,2,3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 1, 2, 3 });
        }

        [Test]
        public void TestTypeArrayAssign2()
        {
            String code =
@"
t:int[];
t = [1,2,3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 1, 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestTypeArrayAssign3()
        {
            String code =
@"
def foo() {
    t = [1,2,3];
    return = t;
}
b = foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new Object[] { 1, 2, 3 });
        }

        [Test]
        public void TestTypedAssignment02()
        {
            string code =
@" t1:int = 1;
t1 = 3.5;
t2:var = 2;
t2 = 4.3;
t3 = false;
t3 = 4.9;
t4 = 1;
t4:int = 3.9;
t4:var = 5.1;
t4 = 6.1;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", 4.3);
            thisTest.Verify("t3", 4.9);
            thisTest.Verify("t4", 6.1);
        }

        [Test]
        public void TestTypedAssignment03()
        {
            string code =
@" 
i = [Imperative]
{
    t1:int = 1;
    t1 = 3.5;
    t2:var = 2;
    t2 = 4.3;
    t3 = false;
    t3 = 4.9;
    t4 = 1;
    t4:int = 3.9;
    t4:var = 5.1;
    t4 = 6.1;
    return [t1, t2, t3, t4];
}
t1 = i[0];
t2 = i[1];
t3 = i[2];
t4 = i[3];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 3.5);
            thisTest.Verify("t2", 4.3);
            thisTest.Verify("t3", 4.9);
            thisTest.Verify("t4", 6.1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestTypedAssignment05()
        {
            string code =
@"
x:int = 1;
def foo()
{
    p:int = false;
    return = p;
}
r = foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        public void TestTypedAssignment06()
        {
            string code =
@"
x:int = 3.5;
x:bool;
y:int = 0;
y:bool;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", true);
            thisTest.Verify("y", false);
        }

        [Test]
        [Category("ToFixJun")]
        public void TestPropAssignWithReplication()
        {
            //Assert.Fail("DNL-1467241 Sprint25: rev 3420 : Property assignments using replication is not working");
            string code =
@"class A
{
    x : int;
    t : int;
    constructor A( y)
    {
        x = y;
    }
}
 
a1 = [ A.A(1), A.A(2) ];
a1.t = 5;
testx = a1.x;
test = a1.t;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("testx", new Object[] { 1, 2 });
            thisTest.Verify("test", new Object[] { 5, 5 });
        }

        [Test]
        public void TestPropAssignWithReplication02()
        {
            string code =
@"class A 
{
    x : int;
    constructor A(i : int)
    {
        x = i;
    }
}
a = [A.A(10), A.A(20)];
a.x = 5;
t = a.x;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 5, 5 });
        }

        [Test]
        public void TestGlobalFunctionRecursion100()
        {
            string code =
@"
def f(i : int)
{
    loc = [Imperative]
    {
        a = 0;
        if (i > 1)
        {
            return = f(i - 1) + i + a;
        }
        return = i;
    }
    return = loc;
}
x = 100;
y = f(x);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", 5050);
        }

        [Test]
        public void TestGlobalFunctionRecursion100_GlobalIncrement()
        {
            string code =
@"
def f(i : int)
{
    loc = [Imperative]
    {
        a = 0;
        if (i > 1)
        {
            return = f(i - 1) + i + a;
        }
        return = i;
    }
    return = loc;
}
x = 100;
y = f(x);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", 5050);
        }

        [Test]
        public void TestGlobalFunctionRecursionReplication()
        {
            string code =
@"
def f(i : int)
{
    loc = [Imperative]
    {
        xx = 0;
        if (i > 1)
        {
            return = f(i - 1) + i + xx;
        }
        return = i;
    }
    return = loc;
}
x = [100,200,300];
y = f(x);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", new Object[] { 5050, 20100, 45150 });
        }

        [Test]
        public void TestrecusionWithNestedFunction01()
        {
            string code =
@"
def if_1(x)
{
    return = 1;
}

def if_2(x)
{
    v1 = x - 1;
    v2 = foo(v1);
    v3 = x * v2;
    return = v3;
}

def foo(x)
{
    c = x <= 1;

    v = [Imperative]
    {
        if (c)
        {
            return = if_1(x);
        }
        return = if_2(x);
    }

    return = v;
}

r = foo(3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 6);
        }

        [Test]
        public void TestContextInject01()
        {
            ProtoScript.Runners.ProtoRunner runner = new ProtoScript.Runners.ProtoRunner();
            string code =
@"
[Associative]
{
	a = x; 
    b = y;
    c = a + b;
    
    x = 10;
    y = 20;
}
";
            // TODO Jun: Move this test to the existing location context inject testcases
            // Add state verification
            Dictionary<string, Object> context = new Dictionary<string, object>();
            context.Add("x", 1);
            context.Add("y", 2);
            runner.PreStart(code, context);
            runner.RunToNextBreakpoint();
        }

        [Test]
        public void TestBasicFFIReplicate()
        {
            string code =
@"
a = [25, 36, 49];
r = Minimal.Sqrt(a);
";
            code = string.Format("{0}\n{1}", "import(\"FFITarget.dll\");", code);
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new Object[] { 5.0, 6.0, 7.0 });
        }

        [Test]
        public void Test_Compare_Node_01()
        {
            string s1 = "a = 1;";
            string s2 = "a=(1);";

            ProtoCore.AST.Node s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            ProtoCore.AST.Node s2Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            bool areEqual = s1Root.Equals(s2Root);
            Assert.AreEqual(areEqual, true);
        }

        [Test]
        public void Test_Compare_Node_02()
        {
            string s1 = "a = 1; b=2;";
            string s2 = "a=(1) ; b = (2);";
            ProtoCore.AST.Node s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            ProtoCore.AST.Node s2Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            bool areEqual = s1Root.Equals(s2Root);
            Assert.AreEqual(areEqual, true);
        }

        [Test]
        public void Test_Compare_Node_03()
        {
            string s1 = "a     =   1;  c = a+1;";
            string s2 = "a = 1; c=a +    1;";
            ProtoCore.AST.Node s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            ProtoCore.AST.Node s2Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            bool areEqual = s1Root.Equals(s2Root);
            Assert.AreEqual(areEqual, true);
        }

        [Test]
        public void ParseTypedIdentifier_AstNode()
        {
            string s1 = "a : A;";
            string s2 = "a : A = null;";
            string s3 = "a : A.B.C;";
            string s4 = "a : A.B.C = null;";
            
            var s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            Assert.IsNotNull(s1Root);
            var typedNode = s1Root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);

            s1Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            Assert.IsNotNull(s1Root);
            var ben = s1Root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);

            s1Root = ProtoCore.Utils.ParserUtils.Parse(s3);
            Assert.IsNotNull(s1Root);
            typedNode = s1Root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);

            s1Root = ProtoCore.Utils.ParserUtils.Parse(s4);
            Assert.IsNotNull(s1Root);
            ben = s1Root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
        }

        [Test]
        public void ParseTypedIdentifierWithRank_AstNode()
        {
            string s1 = "a : A[];";
            string s2 = "a : A[] = null;";
            string s3 = "a : A.B.C[];";
            string s4 = "a : A.B.C[] = null;";

            string s5 = "a : A[][];";
            string s6 = "a : A[][] = null;";
            string s7 = "a : A.B.C[][];";
            string s8 = "a : A.B.C[][] = null;";

            var root = ProtoCore.Utils.ParserUtils.Parse(s1);
            Assert.IsNotNull(root);
            var typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s2);
            Assert.IsNotNull(root);
            var ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s3);
            Assert.IsNotNull(root);
            typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s4);
            Assert.IsNotNull(root);
            ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s5);
            Assert.IsNotNull(root);
            typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s6);
            Assert.IsNotNull(root);
            ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s7);
            Assert.IsNotNull(root);
            typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s8);
            Assert.IsNotNull(root);
            ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);
        }


        [Test]
        public void TestLHSUndefinedArrayIndex01()
        {
            string code = 
@"
a[i] = 10;     
b = a[0];       
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", null);
        }

        [Test]
        public void TestLHSUndefinedArrayIndex02()
        {
            string code =
@"
a = [1,2,3];
a[i] = 10;     
b = a[0];       
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration01()
        {
            string code =
@"
a : local = 1;       
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }


        [Test]
        public void TestLocalKeywordDeclaration02()
        {
            string code =
@"
a : local = 1;       
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration03()
        {
            string code =
@"
a : local int = 1;       
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration04()
        {
            string code =
@"
a : local = 1;      
b : local int = 2;       
c = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestLocalKeywordDeclaration05()
        {
            string code =
@"
a : local int = 1;      
b : local int = 2;       
c = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestLocalKeywordDeclaration06()
        {
            string code =
@"
a : local int = 1;      
b : local int = 2;       
c : local int = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestLocalKeywordDeclaration07()
        {
            string code =
@"
i = [Associative]
{
    a : local int = 1;      
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration08()
        {
            string code =
@"
i = [Associative]
{
    a : local int = 1;      
    b : local int = 2;       
    return = a + b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 3);
        }

        [Test]
        public void TestLocalKeywordDeclarationNegativ01()
        {
            String code =
@"  
a : int local = 1;   // 'local' should come before any type specifier
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }


        [Test]
        public void TestLocalKeywordDeclarationNegativ02()
        {
            String code =
@"  
local = 1;   // 'local' is a reserved keyword
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestLocalKeywordDeclarationNegativ03()
        {
            String code =
@"  
local : int = 1;   // 'local' is a reserved keyword
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestLocalKeywordDeclarationNegativ04()
        {
            String code =
@"  
local = [Imperative] { return = 1; };   // 'local' is a reserved keyword
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }


        [Test]
        public void TestLocalKeywordFromLanguageBlock01()
        {
            string code =
@"
a = 1;
b = [Associative]
{
    a : local = 2;
    return = a;
}

c = a;
d = b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestLocalKeywordFromLanguageBlock02()
        {
            string code =
@"
a = 1;
b = [Associative]
{
    a : local = 2;
    x : local = a;
    return = x;
}

c = a;
d = b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestLocalKeywordFromFunction01()
        {
            string code =
@"
a = 1;
def f()
{
    a : local = 2;
    return = a;
}

p = f();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("p", 2);
        }

        [Test]
        public void TestLocalKeywordFromFunction02()
        {
            string code =
@"
a = 1;
def f()
{
    a : local = 2;
    x : local = a;
    return = x;
}

p = f();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("p", 2);
        }


        [Test]
        public void TestLocalVariableUpdate01()
        {
            string code =
@"
i = [Associative]
{
    a : local = 1; 
    b : local = a;
    a = 2;
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestLocalVariableUpdate02()
        {
            string code =
@"
i = [Associative]
{
    a : local = 1; 
    b : local = a + 10;
    a = 2;
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 12);
        }

        [Test]
        public void TestLocalVariableUpdate03()
        {
            string code =
@"
a = 10;
b = a;
i = [Associative]
{
    a : local = 1; 
    b : local = a;
    a = 2;          // Update local 'a'
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 10);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestLocalVariableUpdate04()
        {
            string code =
@"
a = 10;
b = a;
i = [Associative]
{
    return = [Imperative]
    {
        return = [Associative]
        {
            a : local = 1; 
            b : local = a;
            a = 2;          // Update local 'a'
            return = b;
        }
    }
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 10);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestLocalVariableNoUpdate01()
        {
            string code =
@"
a = 1;
b = a;
c = [Associative]
{
    a : local = 2; // Updating local 'a' should not update global 'a'
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
        }

        [Test]
        public void TestLocalVariableNoUpdate02()
        {
            string code =
@"
a = 1;
b = a;
c = [Associative]
{
    a : local; 
    a = 2;      // Updating local 'a' should not update global 'a'
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
        }


        [Test]
        public void TestLocalVariableNoUpdate03()
        {
            string code =
@"
a : local = 1;      // Tagging a variable local at the global scope has no semantic effect
b : local = a;
c = [Associative]
{
    a : local; 
    a = 2;      // Updating local 'a' should not update global 'a'
    return = 1;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
        }


        [Test]
        public void TestNullsOnExpression01()
        {
            string code =
@"
        a = 1 + null;
        b = """" == null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", false);
        }

        [Test]
        public void TestAddNullToString()
        {
            string code =
@"
a = ""hello"" + null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }
        
        [Test]
        public void TestUndefinedTypedIdentifier()
        {
            string code =
@"
        a : UndefinedType;
        b : UndefinedType2 = 2;
        c : UndefinedType3 = null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);

            thisTest.VerifyRuntimeWarningCount(3);
        }

        [Test]
        public void TestCallingOverloadedMemberFunction01()
        {
            string code = @"
import (TestThisOverload from ""FFITarget.dll"");
obj = TestThisOverload.TestThisOverload(100);
a = obj.Add(21);
b = TestThisOverload.Add(obj, 21); 
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 121);
            thisTest.Verify("b", 121);
        }

        [Test]
        public void TestCallingOverloadedMemberFunction02()
        {
            string code = @"
import (TestThisOverload from ""FFITarget.dll"");
obj = TestThisOverload.TestThisOverload(3);
a = obj.Mul(4);
b = TestThisOverload.Mul(obj, 4);            
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 12);
            // TestThisOverload.Mul() is a defined static function. 
            // Here we shouldn't generate an overloaded one.
            thisTest.Verify("b", 24);
        }

        [Test]
        public void TestReturnStatement01()
        {
            string code = @"
x = [Imperative] {
    return 6;
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("x", 6);
        }

        [Test]
        public void TestReturnStatement02()
        {
            string code = @"
x = [Imperative] {
    return [2, 3, 5];
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("x", new object[] { 2, 3, 5 } );
        }

        [Test]
        public void TestReturnStatement03()
        {
            string code = @"
def foo(x) {
    return x * 2;
}

x = [Imperative] {
    return foo(3);
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("x", 6);
        }

        [Test]
        public void TestReturnStatement04()
        {
            string code = @"
[Imperative] {
    return 6;
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement05()
        {
            string code = @"
[Imperative] {
    return [2, 3, 5];
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement06()
        {
            string code = @"
def foo(x) {
    return x * 2;
}

[Imperative] {
    return foo(3);
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement07()
        {
            string code = @"
def foo(x) {
    return [Imperative] {
        return x * 2;
    }
}

r = foo(3);
";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("r", 6);
        }

        [Test]
        public void TestReturnStatement08()
        {
            string code = @"
x = [Associative] {
    return 6;
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("x", 6);
        }

        [Test]
        public void TestReturnStatement09()
        {
            string code = @"
x = [Associative] {
    return [2, 3, 5];
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("x", new object[] { 2, 3, 5 });
        }

        [Test]
        public void TestReturnStatement10()
        {
            string code = @"
def foo(x) {
    return x * 2;
}

x = [Associative] {
    return foo(3);
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("x", 6);
        }

        [Test]
        public void TestReturnStatement11()
        {
            string code = @"
[Associative] {
    return 6;
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement12()
        {
            string code = @"
[Associative] {
    return [2, 3, 5];
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement13()
        {
            string code = @"
def foo(x) {
    return x * 2;
}

[Associative] {
    return foo(3);
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement14()
        {
            string code = @"
def foo(x) {
    return [Associative] {
        return x * 2;
    }
}

r = foo(3);
";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("r", 6);
        }

        [Test]
        public void TestReturnStatement15()
        {
            string code = @"
[Imperative] {
    return 6;
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestReturnStatement16()
        {
            string code = @"
[Associative] {
    return 6;
}";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }
    }
}
