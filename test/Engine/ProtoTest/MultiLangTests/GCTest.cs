using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.MultiLangTests
{
    class GCTest : ProtoTestBase
    {
        string testCasePath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        public void T01_TestGCArray()
        {
            string code = @"
import(""DisposeVerify.ds"");
v1;
v2;
v3;
[Imperative]
{
DisposeVerify.x = 1;
arr = { A.A(), A.A(), A.A() };
arr = 3;
v1 = DisposeVerify.x; // 4
a1 = A.A();
arr = { a1, A.A() };
arr = 3;
v2 = DisposeVerify.x; // 5
def foo : int(a : A[])
{
    return = 10;
}
a2 = A.A();
a = foo( { a1, a2 });
a2 = A.A();
v3 = DisposeVerify.x; // 6
}
v4 = DisposeVerify.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v4", 8);
            }
            else
            {
                thisTest.Verify("v1", 4);
                thisTest.Verify("v2", 5);
                thisTest.Verify("v3", 6);
            }
        }

        [Test]
        public void T02_TestGCEndofIfBlk()
        {
            string code = @"
import(""DisposeVerify.ds"");
DisposeVerify.x = 1;
a1 = A.A();
[Imperative]
{
    m = 10;
    if (m > 10)
        a2 = A.A();
    else
        a3 = A.A();
    a4 = A.A();
}
v = DisposeVerify.x; // 3";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v", 3);
        }

        [Test]
        public void T03_TestGCEndofLangBlk()
        {
            string code = @"
import(""DisposeVerify.ds"");
DisposeVerify.x = 1;
a1 = A.A();
v1;
[Imperative]
{
    a2 = A.A();
    [Associative]
    {
        a3 = a2;
        a4 = A.A();
    }
	a5 = a1;
	v1 = DisposeVerify.x; // 2
}
v2 = DisposeVerify.x; // 3";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);

            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kReferenceCounting)
            {
                thisTest.Verify("v1", 2);
            }
            thisTest.Verify("v2", 3);
        }

        [Test]
        public void T04_TestGCReturnFromLangBlk()
        {
            string code = @"
import(""DisposeVerify.ds"");
DisposeVerify.x = 1;
v1;
[Imperative]
{
	// %tempLangBlk = 
    [Associative]
    {
        a1 = A.A();
        return = a1; // a1 is not gced here because it is been returned
    }
	v1 = DisposeVerify.x; // 1
	// %tempLangBlk, same value as a1, is gcced here, this is also to test after assign the return value from the language 
	// block, the ref count of that value is still 1
}
v2 = DisposeVerify.x; // 2";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 1);
            thisTest.Verify("v2", 2);
        }

        [Test]
        public void T05_TestGCReturnFromFunction()
        {
            string code = @"
import(""DisposeVerify.ds"");
DisposeVerify.x = 1;
v1;
v2;
def foo : A()
{
	arr = { A.A(), A.A(), A.A() };
	return = arr[1]; // only the second element in arr is not gced, ref count of arr[1] is incremented
}
[Imperative]
{
  
m = foo();
v1 = DisposeVerify.x; // 3
m = 10;
// test after assign the return value from foo, the ref count of that value is 1
v2 = DisposeVerify.x; // 4
}
v3 = DisposeVerify.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v3", 4);
            }
            else
            {
                thisTest.Verify("v1", 3);
                thisTest.Verify("v2", 4);
            }
        }

        [Test]
        public void T06_TestGCEndofWhileBlk()
        {
            string code = @"
import(""DisposeVerify.ds"");
v1;
v2;
v3;
[Imperative]
{
  
DisposeVerify.x = 1;
arr = { A.A(), A.A(), A.A() };
[Associative]
{
    [Imperative]
    {
        a = 3;
        while (a > 0)
        {
            mm = A.A();
            a = a - 1;
        }
        v1 = DisposeVerify.x; // 3
    }
}
v2 = DisposeVerify.x; // 4
arr = null;
v3 = DisposeVerify.x; // 7
}
v4 = DisposeVerify.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);

            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v4", 7);
            }
            else
            {
                thisTest.Verify("v1", 4);
                thisTest.Verify("v2", 4);
                thisTest.Verify("v3", 7);
            }
        }

        [Test]
        public void T07_TestGCEndofForBlk()
        {
            string code = @"
import(""DisposeVerify.ds"");
v1;
v2;
v3;
[Imperative]
{
  
DisposeVerify.x = 1;
arr = { A.A(), A.A(), A.A() };
[Associative]
{
    [Imperative]
    {
        for(i in arr)
        {
            mm = i;
            mm2 = A.A();
        }
        v1 = DisposeVerify.x; // 3
    }
}
v2 = DisposeVerify.x; // 4
arr = null;
v3 = DisposeVerify.x; // 7
}
v4 = DisposeVerify.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v4", 7);
            }
            else
            {
                thisTest.Verify("v1", 4);
                thisTest.Verify("v2", 4);
                thisTest.Verify("v3", 7);
            }
        }

        [Test]
        public void T08_TestGCArray02()
        {
            string code = @"
import(""DisposeVerify.ds"");
v1;
v2;
v3;
v4;
v5;
v6;
v7;
[Imperative]
{
  
DisposeVerify.x = 1;
arr = { A.A(), A.A(), A.A() };
b = arr;
b = null;
v1 = DisposeVerify.x; // 1
arr = null;
v2 = DisposeVerify.x; // 4
a1 = A.A();
a2 = A.A();
a3 = A.A();
arr2 = { a1, a2, a3 };
b2 = arr2;
b2 = null;
v3 = DisposeVerify.x; // 4
arr2 = null; 
v4 = DisposeVerify.x; // 4
a1 = null; 
v5 = DisposeVerify.x; // 5
a2 = null;
v6 = DisposeVerify.x; // 6
a3 = null; 
v7 = DisposeVerify.x; // 7
}

v8 = DisposeVerify.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v8", 7);
            }
            else
            {
                thisTest.Verify("v1", 1);
                thisTest.Verify("v2", 4);
                thisTest.Verify("v3", 4);
                thisTest.Verify("v4", 4);
                thisTest.Verify("v5", 5);
                thisTest.Verify("v6", 6);
                thisTest.Verify("v7", 7);
            }
        }

        [Test]
        public void T09_TestGCPassingArguments()
        {
            string code = @"
import(""DisposeVerify.ds"");
DisposeVerify.x = 1;
def foo : int(p : A[])
{
	return = 10;
}
def foo2 : int(p : A)
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
a1 = A.A();
a2 = { A.A(), A.A(), A.A() };
x = foo2(a1);
y = foo(a2);
v1 = DisposeVerify.x; // 1
v2 = DisposeVerify.x; // 1
a1 = null;
v3 = DisposeVerify.x; // 2
a2 = null;
v4 = DisposeVerify.x; // 5
b = foo2(A.A());
v5 = DisposeVerify.x; // 6
c = foo( { A.A(), A.A(), A.A() } );
}
v6 = DisposeVerify.x; // 9";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);

            // Fails to GC temporary object 'A'after calling b = foo2(A.A());
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4004

            // SSA'd transforms will not GC the temps until end of block
            // However, they must be GC's after every line when in debug setp over
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v6", 9);
            }
            else
            {
                thisTest.Verify("v1", 1);
                thisTest.Verify("v2", 1);
                thisTest.Verify("v3", 2);
                thisTest.Verify("v4", 5);
                thisTest.Verify("v5", 6);
                thisTest.Verify("v6", 9);
            }
        }

        [Test]
        public void T10_TestGCReturnArguments()
        {
            string code = @"
import(""DisposeVerify.ds"");
DisposeVerify.x = 1;
def foo : A(p : A[])
{
	return = p[0];
}
v1;
v2;
[Imperative]
{
  
m = foo( { A.A(), A.A(), A.A() } );
v1 = DisposeVerify.x; // 3
m = null; 
v2 = DisposeVerify.x; // 4
}
v3 = DisposeVerify.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kMarkAndSweep)
            {
                thisTest.Verify("v3", 4);
            }
            else
            {
                thisTest.Verify("v1", 3);
                thisTest.Verify("v2", 4);
            }
        }

        [Test]
        public void T11_TestGCLangBlkInFunction()
        {
            string code = @"
import(""DisposeVerify.ds"");
def foo : A(a : A)
{
	aaa = A.A();
	[Imperative]
	{
		aaaa = aaa;
		c = a;
	}
	return = aaa;
}
DisposeVerify.x = 1;
aa = A.A();
bb = foo(aa);
v1 = DisposeVerify.x; // 2
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 1);
        }

        [Test]
        public void T12_TestGCIfElseInFunction()
        {
            string code = @"
import(""DisposeVerify.ds"");
v1;
[Imperative]
{
	def foo : int(a : A)
	{
		a1 = A.A();
		if (1 == 1)
		{
			a2 = A.A();
		}
		
		return = 10;
	}
	DisposeVerify.x = 1;
	aaaa = [Associative]
	{
		aaaaaaa = A.A();
		return = A.A();
	}
	if (1 == 1)
		aaaaa = A.A();
	aa = A.A();
	cc = foo(aa);
	v1 = DisposeVerify.x;
}
v2 = DisposeVerify.x; // 4";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            if (thisTest.GetTestCore().Heap.GCStrategy == ProtoCore.DSASM.Heap.GCStrategies.kReferenceCounting)
            {
                thisTest.Verify("v1", 5);
            }
            thisTest.Verify("v2", 7);
        }

        [Test]
        public void T13_GCTestComplexCase()
        {
            string code = @"
import(""DisposeVerify.ds"");
def flatten(arr : A[][])
{
	solids = {};
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
DisposeVerify.x = 1;
arrr = { { A.A(), A.A(), A.A() }, { A.A(), A.A(), A.A() }, { A.A(), A.A(), A.A() } };
arrr2 = flatten(arrr);
v1 = DisposeVerify.x; // 1
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 1);
        }

        [Test]
        public void T14_TestGCPointer_AssociativeScope()
        {
            string code = @"
import(""DisposeVerify.ds"");
[Associative]
{
    DisposeVerify.x = 1;
    arr = A.A();
    arr = 1;                // Dispose A.A() 
}
    v1 = DisposeVerify.x;   // Reflect the disposed object

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 2);
        }

        [Test]
        public void T15_TestGCArray_AssociativeScope()
        {
            string code = @"
import(""DisposeVerify.ds"");

[Associative]
{
    DisposeVerify.x = 1;
    arr = {A.A()};
    arr = 1;                // Dispose A.A() 
}
    v1 = DisposeVerify.x;   // Reflect the disposed object

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testCasePath);
            thisTest.Verify("v1", 2);
        }

    }
}
