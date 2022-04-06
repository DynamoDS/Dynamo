using System;
using NUnit.Framework;
namespace ProtoTest.Associative
{
    class MethodResolution : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        public void SimpleAssignment()
        {
            String code =
@"
	fx : var;
	fy : var;
	fx = 123;
	fy = 345;
	
	x = fx;
	y = fy;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 123);
            thisTest.Verify("y", 345);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void SimpleMethodOverload1()
        {
            String code =
@"
	def f()
	{
		return = 123;
	}
    def f(a : int)
    {
        return = a;
	}

    x = f();
    y = f(345);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 123);
            thisTest.Verify("y", 345);
        }

        [Test]
        public void SimpleCtorResolution04()
        {
            String code =
@"
class Sample
{
    mx : var;
    constructor Create()
    {}
        
    constructor Create(intval : int)
    {}
        
    constructor Create(doubleval : double)
    {
        mx = doubleval;
    }
        
    constructor Create(intval : int, doubleval : double)
    {}
}
    
//    default ctor
s1 = Sample.Create();
    
//    ctor with int
s2 = Sample.Create(1);
    
//    ctor with double
s3 = Sample.Create(1.0);
    
//    ctor with int and double
s4 = Sample.Create(1, 1.0);
d = s3.mx;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", 1);
        }

        [Test]
        public void TestMethodOverload4()
        {
            string code =
                @"
import (""FFITarget.dll"");
c = TestOverloadC.TestOverloadC();
val = c.execute(c);
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 1);
        }

        [Test]
        public void TestMethodResolutionOverInheritance()
        {
            string code =
                @"
import (""FFITarget.dll"");
c = TestOverloadC.TestOverloadC();
val = c.unique(c);
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 4);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput2()
        {
            string code =
                @"
import (""FFITarget.dll"");
b = TestOverloadB.TestOverloadB();
arr = [TestOverloadC.TestOverloadC(), TestOverloadB.TestOverloadB(), TestOverloadB.TestOverloadB()];
val = b.execute(arr);
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val", 3);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestDispatchArray()
        {
            //Recorded as defect: DNL-1467146
            string code =
                @"def execute(b : var)
{
	return = 100; 
}
arr = [3];
v = execute(arr);
val = v[0];
                ";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("val", 100);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("DSDefinedClass_Ported")]
        public void TestDispatchEmptyArray()
        {
            string code =
                @"def execute(b : var)
{
    return = 100; 
}
arr = [];
v = execute(arr);
val = v[0];
                ";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestMethodWithArrayInput()
        {
            string code = @"
                            def Test(arr : var[])
                            {
                                return = 123;
                            }
                            a = [3, 4, 5];
                            val = Test(a);
                            ";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("val", 123);
        }

        [Test]
        public void TestMethodWithArrayInput2()
        {
            string code = @"
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
                            a = [B.B(), A.A(), B.B()];
                            val = Test(a);
                            ";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("val", 123);
        }

        [Test]
        public void TestMethodResolutionForArray()
        {
            string code = @"
def foo(val : bool)
{
    return = 1;
}
def foo(val : var)
{
    return = 2;
}
arr1 = [ true, null, false ];
r1 = foo(arr1);   // r1 = {1, 1, 1}
arr2 = [ null, true, false ];
r2 = foo(arr2);   // r2 = {2, 2, 2}";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", new object[] { 1, 1, 1 });
            thisTest.Verify("r2", new object[] { 2, 2, 2 });
        }

        [Test]
        [Category("Failure")]
        public void TestMethodResolutionForInforLoss()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4114
            string code = @"
def foo(x1 : int, x2: int, x3: int, x4:int)
{
    return = x1 + x2 + x3 + x4;
}
def foo(x1 : double, x2:double, x3:double, x4:double)
{
    return = x1 * x2 * x3 * x4;
}
r = foo(3, 4, 5, 4.7);   
";
            string error = "MAGN-4114 Method resolution issue - type promotion should be preferred to type demotion";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 282);

        }

        [Test]
        public void TestMethodResultionForNonExistType()
        {
            string code = @"
def foo(x : NonExistClass[][])
{
    return = x;
}
z = foo([ 1, 2 ]);  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", null);
        }


        [Test]
        public void TestArraySubtypeDispatch1D()
        {
            string code = @"
def foo(val : int)
{
    return = true;
}
def foo(val : double)
{
    return = false;
}

r1 = foo([1, 2]);   // r1 = {true, true}
r2 = foo([1.0, 2.0]);   // r1 = {false, false}
r3 = foo([1, 2.0]);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", new object[] { true, true });
            thisTest.Verify("r2", new object[] { false, false });
            thisTest.Verify("r3", new object[] { true, false });
        }

        [Test]
        public void TestArraySubtypeDispatch1D_R1()
        {
            string code = @"
def foo(val : int)
{
    return = true;
}
def foo(val : double)
{
    return = false;
}

r1 = foo([1, 2]<1>);   // r1 = {true, true}
r2 = foo([1.0, 2.0]<1>);   // r1 = {false, false}
r3 = foo([1, 2.0]<1>);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", new object[] { true, true });
            thisTest.Verify("r2", new object[] { false, false });
            thisTest.Verify("r3", new object[] { true, false });
        }

        [Test]
        public void MAGN5729_Repro()
        {
            string code = @"
def foo(val : int)
{
    return = true;
}
def foo(val : double)
{
    return = false;
}

r3 = foo([1, 2.0]<1>);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r3", new object[] { true, false });
        }


        [Test]
        public void MAGN5729_Repro_Simple()
        {
            string code = @"
def foo(val : int)
{
    return = true;
}
def foo(val : double)
{
    return = false;
}

r3 = foo([1, 2.0]);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r3", new object[] { true, false });
        }

        [Test]
        public void MAGN7807_ParameterNameSameAsFunctionName()
        {
            string code =
@"
def foo(foo : int)
{
    return = foo;
}

def bar(bar: int)
{
return = [Imperative]
{
    return = bar;
}
}

def qux1()
{
    qux1 = 21;
    return = qux1 + 1;
}

def qux2()
{
    return = [Imperative] {
       qux2 = 21;
       return = qux2 + 1;
    }
}

x = foo([1,2,3]);
y = bar([4,5,6]);
z1 = qux1();
z2 = qux2();
";
            thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("x", new object[] { 1, 2, 3 });
            thisTest.Verify("y", new object[] { 4, 5, 6 });
            thisTest.Verify("z1", 22);
            thisTest.Verify("z2", 22);
        }
    }
}
