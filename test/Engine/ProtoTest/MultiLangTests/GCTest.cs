using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.MultiLangTests
{
    class GCTest : ProtoTestBase
    {
        string testCasePath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";
        
        public override void Setup()
        {
            base.Setup();
            FFITarget.DisposeCounter.Reset(0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T01_TestGCArray()
        {
            string code = @"
import(""FFITarget.dll"");
v1;
v2;
v3;
def foo : int(a : DisposeCounterTest[])
{
    return = 10;
}
[Imperative]
{
DisposeCounter.Reset(1);
arr = [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ];
arr = 3;
v1 = DisposeCounter.x; // 4
a1 = DisposeCounterTest.DisposeCounterTest();
arr = [ a1, DisposeCounterTest.DisposeCounterTest() ];
arr = 3;
v2 = DisposeCounter.x; // 5
a2 = DisposeCounterTest.DisposeCounterTest();
a = foo( [ a1, a2 ]);
a2 = DisposeCounterTest.DisposeCounterTest();
v3 = DisposeCounter.x; // 6
}
__GC();
v4 = DisposeCounter.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v4", 8);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T02_TestGCEndofIfBlk()
        {
            string code = @"
import(""FFITarget.dll"");
DisposeCounter.Reset(1);
a1 = DisposeCounterTest.DisposeCounterTest();
[Imperative]
{
    m = 10;
    if (m > 10)
        a2 = DisposeCounterTest.DisposeCounterTest();
    else
        a3 = DisposeCounterTest.DisposeCounterTest();
    a4 = DisposeCounterTest.DisposeCounterTest();
}
__GC();
v = DisposeCounter.x; // 3";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T03_TestGCEndofLangBlk()
        {
            string code = @"
import(""FFITarget.dll"");
DisposeCounter.Reset(1);
a1 = DisposeCounterTest.DisposeCounterTest();
v1;
[Imperative]
{
    a2 = DisposeCounterTest.DisposeCounterTest();
    [Associative]
    {
        a3 = a2;
        a4 = DisposeCounterTest.DisposeCounterTest();
    }
	a5 = a1;
	v1 = DisposeCounter.x; // 2
}
__GC();
v2 = DisposeCounter.x; // 3";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);

            thisTest.Verify("v2", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T04_TestGCReturnFromLangBlk()
        {
            string code = @"
import(""FFITarget.dll"");
DisposeCounter.Reset(1);
v1;
[Imperative]
{
	// %tempLangBlk = 
    [Associative]
    {
        a1 = DisposeCounterTest.DisposeCounterTest();
        return = a1; // a1 is not gced here because it is been returned
    }
	v1 = DisposeCounter.x; // 1
	// %tempLangBlk, same value as a1, is gcced here, this is also to test after assign the return value from the language 
	// block, the ref count of that value is still 1
}
__GC();
v2 = DisposeCounter.x; // 2";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", null);
            thisTest.Verify("v2", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T05_TestGCReturnFromFunction()
        {
            string code = @"
import(""FFITarget.dll"");
DisposeCounter.Reset(1);
v1;
v2;
def foo : DisposeCounterTest()
{
	arr = [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ];
	return = arr[1]; // only the second element in arr is not gced, ref count of arr[1] is incremented
}
[Imperative]
{
  
m = foo();
v1 = DisposeCounter.x; // 3
m = 10;
// test after assign the return value from foo, the ref count of that value is 1
v2 = DisposeCounter.x; // 4
}
__GC();
v3 = DisposeCounter.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v3", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T06_TestGCEndofWhileBlk()
        {
            string code = @"
import(""FFITarget.dll"");
v1;
v2;
v3;
[Imperative]
{
  
DisposeCounter.Reset(1);
arr = [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ];
[Associative]
{
    [Imperative]
    {
        a = 3;
        while (a > 0)
        {
            mm = DisposeCounterTest.DisposeCounterTest();
            a = a - 1;
        }
        v1 = DisposeCounter.x; // 3
    }
}
v2 = DisposeCounter.x; // 4
arr = null;
v3 = DisposeCounter.x; // 7
}
__GC();
v4 = DisposeCounter.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
                thisTest.Verify("v4", 7);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T07_TestGCEndofForBlk()
        {
            string code = @"
import(""FFITarget.dll"");
v1;
v2;
v3;
[Imperative]
{
  
DisposeCounter.Reset(1);
arr = [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ];
[Associative]
{
    [Imperative]
    {
        for(i in arr)
        {
            mm = i;
            mm2 = DisposeCounterTest.DisposeCounterTest();
        }
        v1 = DisposeCounter.x; // 3
    }
}
v2 = DisposeCounter.x; // 4
arr = null;
v3 = DisposeCounter.x; // 7
}
__GC();
v4 = DisposeCounter.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
                thisTest.Verify("v4", 7);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T08_TestGCArray02()
        {
            string code = @"
import(""FFITarget.dll"");
v1;
v2;
v3;
v4;
v5;
v6;
v7;
[Imperative]
{
  
DisposeCounter.Reset(1);
arr = [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ];
b = arr;
b = null;
v1 = DisposeCounter.x; // 1
arr = null;
v2 = DisposeCounter.x; // 4
a1 = DisposeCounterTest.DisposeCounterTest();
a2 = DisposeCounterTest.DisposeCounterTest();
a3 = DisposeCounterTest.DisposeCounterTest();
arr2 = [ a1, a2, a3 ];
b2 = arr2;
b2 = null;
v3 = DisposeCounter.x; // 4
arr2 = null; 
v4 = DisposeCounter.x; // 4
a1 = null; 
v5 = DisposeCounter.x; // 5
a2 = null;
v6 = DisposeCounter.x; // 6
a3 = null; 
v7 = DisposeCounter.x; // 7
}

__GC();
v8 = DisposeCounter.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
                thisTest.Verify("v8", 7);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T09_TestGCPassingArguments()
        {
            string code = @"
import(""FFITarget.dll"");
DisposeCounter.Reset(1);
def foo : int(p : DisposeCounterTest[])
{
	return = 10;
}
def foo2 : int(p : DisposeCounterTest)
{
	return = 10;
}

v1;
v2;
v3;
v4;
v5;
v6;
[Imperative]
{
a1 = DisposeCounterTest.DisposeCounterTest();
a2 = [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ];
x = foo2(a1);
y = foo(a2);
v1 = DisposeCounter.x; // 1
v2 = DisposeCounter.x; // 1
a1 = null;
v3 = DisposeCounter.x; // 2
a2 = null;
v4 = DisposeCounter.x; // 5
b = foo2(DisposeCounterTest.DisposeCounterTest());
v5 = DisposeCounter.x; // 6
c = foo( [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ] );
}
__GC();
v6 = DisposeCounter.x; // 9";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);

            // Fails to GC temporary object 'A'after calling b = foo2(DisposeCounterTest.DisposeCounterTest());
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4004

            // SSA'd transforms will not GC the temps until end of block
            // However, they must be GC's after every line when in debug setp over
                thisTest.Verify("v6", 9);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T10_TestGCReturnArguments()
        {
            string code = @"
import(""FFITarget.dll"");
DisposeCounter.Reset(1);
def foo : DisposeCounterTest(p : DisposeCounterTest[])
{
	return = p[0];
}
v1;
v2;
[Imperative]
{
  
m = foo( [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ] );
v1 = DisposeCounter.x; // 3
m = null; 
v2 = DisposeCounter.x; // 4
}
__GC();
v3 = DisposeCounter.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
                thisTest.Verify("v3", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T11_TestGCLangBlkInFunction()
        {
            string code = @"
import(""FFITarget.dll"");
def foo : DisposeCounterTest(a : DisposeCounterTest)
{
	aaa = DisposeCounterTest.DisposeCounterTest();
	[Imperative]
	{
		aaaa = aaa;
		c = a;
	}
	return = aaa;
}
DisposeCounter.Reset(1);
aa = DisposeCounterTest.DisposeCounterTest();
bb = foo(aa);
v1 = DisposeCounter.x; // 2
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T13_GCTestComplexCase()
        {
            string code = @"
import(""FFITarget.dll"");
def flatten(arr : DisposeCounterTest[][])
{
	solids = [];
	i = 0;
	[Imperative]
	{
		for(obj in arr)
		{
			for(solid in obj)
			{
				solids[i] = solid;
				i = i + 1;
			}
		}
	}
	return = solids;
}
DisposeCounter.Reset(1);
arrr = [ [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ], [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ], [ DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest(), DisposeCounterTest.DisposeCounterTest() ] ];
arrr2 = flatten(arrr);
v1 = DisposeCounter.x; // 1
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T14_TestGCPointer_AssociativeScope()
        {
            string code = @"
import(""FFITarget.dll"");
[Associative]
{
    DisposeCounter.Reset(1);
    arr = DisposeCounterTest.DisposeCounterTest();
    arr = 1;                // Dispose DisposeCounterTest.DisposeCounterTest() 
}
__GC();
    v1 = DisposeCounter.x;   // Reflect the disposed object

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T15_TestGCArray_AssociativeScope()
        {
            string code = @"
import(""FFITarget.dll"");

[Associative]
{
    DisposeCounter.Reset(1);
    arr = [DisposeCounterTest.DisposeCounterTest()];
    arr = 1;                // Dispose DisposeCounterTest.DisposeCounterTest() 
}
__GC();
    v1 = DisposeCounter.x;   // Reflect the disposed object

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 2);
        }

    }
}
