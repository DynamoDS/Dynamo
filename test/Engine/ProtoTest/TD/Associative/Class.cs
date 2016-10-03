using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Associative
{
    class Class : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T001_Associative_Class_Property_Int()
        {
            string code = @"
import(""FFITarget.dll"");
	
	newPoint = DummyPoint.ByCoordinates(1,2,3);
	
	xPoint = newPoint.X;
    yPoint = newPoint.Y;            
    zPoint = newPoint.Z;               
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint", 1.0);
            thisTest.Verify("yPoint", 2.0);
            thisTest.Verify("zPoint", 3.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T002_Associative_Class_Property_Double()
        {
            string code = @"
import(""FFITarget.dll"");
	newPoint = DummyPoint.ByCoordinates(1.1,2.2,3.3);
	
	xPoint = newPoint.X;
    yPoint = newPoint.Y;            
    zPoint = newPoint.Z;               
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint", 1.1);
            thisTest.Verify("yPoint", 2.2);
            thisTest.Verify("zPoint", 3.3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T003_Associative_Class_Property_Bool()
        {
            string code = @"
import(""FFITarget.dll"");
	newPoint1 = BooleanMember.BooleanMember(true);
	newPoint2 = BooleanMember.BooleanMember(false);
	propPoint1 = newPoint1.a;
    propPoint2 = newPoint2.a;            
              
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("propPoint1", true);
            thisTest.Verify("propPoint2", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_FFI_Class_Property_CallFromFunctionOutsideClass()
        {
            string code = @"
import(""FFITarget.dll"");
	def GetPointValue : double (pt : DummyPoint)
	{
		return = pt.X + pt.Y + pt.Z; 
	}
	
	myNewPoint = DummyPoint.ByCoordinates(1.0, 10, 200);
	myPointValue = GetPointValue(myNewPoint);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("myPointValue", 211.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_Associative_Class_Constructor_Overloads()
        {
            string code = @"
import(""FFITarget.dll"");
    pt1 = TestOverloadConstructor.TestOverloadConstructor(10);
	pt2 = TestOverloadConstructor.TestOverloadConstructor(10, 200);
	pt3 = TestOverloadConstructor.TestOverloadConstructor(10, 200, 300);
	a = pt1.a;
	b = pt2.b;
	c = pt3.c;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 10);
            thisTest.Verify("b", 200);
            thisTest.Verify("c", 300);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T014_Associative_Class_Property_GetUsingMultipleReferencingWithSameName()
        {
            string code = @"
import(""FFITarget.dll"");
	p1 = TestObjectA.TestObjectA(10);
	p2 = TestSamePropertyName.TestSamePropertyName(p1);
    x = p2.a.a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T015_Associative_Class_Property_SetInExternalFunction()
        {
            string code = @"
import(""FFITarget.dll"");  
def Modify : TestPoint(old : DummyPoint)
{
        old.X = 10;
        old.Y = 20;
        return = old;
}  
pt1 = DummyPoint.ByCoordinates(1, 2, 3);
pt2 = Modify(pt1);
testX1 = pt1.X;
testY1 = pt1.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T019_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumentNumber()
        {
            string code = @"
import(""FFITarget.dll"");
    test1 = TestOverloadConstructor.TestOverloadConstructor(1, 2);
	test2 = TestOverloadConstructor.TestOverloadConstructor(10, 20, 30);
	
	
	x1 = test1.a;
	x2 = test1.b;
	y1 = test2.a;
	y2 = test2.b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 2);
            thisTest.Verify("y1", 10);
            thisTest.Verify("y2", 20);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestDefaultArgInConstructor01()
        {
            string code = @"
import(""FFITarget.dll"");p = TestDefaultArgument.TestDefaultArgument(2); i = p.a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestDefaultArgInConstructor02()
        {
            string code = @"
import(""FFITarget.dll"");p = TestDefaultArgument.TestDefaultArgument(1, 2); i = p.b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z005_Associative_Class_Property_Regress_1454178()
        {
            string code = @"
import(""FFITarget.dll"");
sum = [Associative]
{
    t1 = DummyTuple4.XYZH(1,1,1,1);
    t2 = DummyTuple4.XYZ(1,1,1);
    return = t1.Multiply(t2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 4.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z007_Associative_Class_Property_Regress_1454172()
        {
            string code = @"
import(""FFITarget.dll"");
sum = [Associative]
{
    t1 = DummyTuple4.XYZH(1,1,1,1);
    return = t1.Coordinates3();  
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedA = { 1.0, 1.0, 1.0 };
            thisTest.Verify("sum", expectedA);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z008_Associative_Class_Property_Regress_1454161()
        {
            string code = @"
import(""FFITarget.dll"");
x3;
y3;
z3;
h3;
    [Associative]
    {
    t3 = DummyTuple4.ByCoordinates3({1.0,1.0,1.0});
    x3 = t3.X;
    y3 = t3.Y;
    z3 = t3.Z;
    h3 = t3.H;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x3", 1.0);
            thisTest.Verify("y3", 1.0);
            thisTest.Verify("z3", 1.0);
            thisTest.Verify("h3", 1.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z009_Associative_Class_Property_Regress_1453891()
        {
            string code = @"
import(""FFITarget.dll"");
pt1 = DummyPoint.ByCoordinates(3.1,2.1,1.1);
pt2 = DummyPoint.ByCoordinates(31.1,21.1,11.1);
l = DummyLine.ByStartPointEndPoint(pt1, pt2);               
l_startPoint_X = l.Start.X;
  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("l_startPoint_X", 3.1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z011_Associative_Class_Property_Regress_1454162()
        {
            string code = @"
import(""FFITarget.dll"");
    t1 = DummyTuple4.XYZH(1.0,1.0,1.0,1.0);
    x1 = t1.X;
    y1 = t1.Y;
    z1 = t1.Z;
    h1 = t1.H;
    result1 = {x1, y1, z1, h1};
    
    
    
    t2 = DummyTuple4.ByCoordinates3({1.0,1.0,1.0});
    x2 = t2.X;
    y2 = t2.Y;
    z2 = t2.Z;
    h2 = t2.H;
    result2 = {x2, y2, z2, h2};
    
   ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1.0, 1.0, 1.0, 1.0 };
            object[] expectedResult2 = { 1.0, 1.0, 1.0, 1.0 };
            thisTest.Verify("result1", expectedResult1, 0);
            thisTest.Verify("result2", expectedResult2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Z015_Defect_1457029()
        {
            //Assert.Fail("1457029 - Sprint25: Rev 3369 : Passing a collection value using index as argument to a class method is failing");

            string code = @"
import(""FFITarget.dll"");
c1 = { 1.0, 2.0, 3.0 };
c1 = DummyVector.ByCoordinates( c1[0], 20, 30 );
x = c1.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Z015_Defect_1457029_2()
        {
            //Assert.Fail("1457029 - Sprint25: Rev 3369 : Passing a collection value using index as argument to a class method is failing");

            string code = @"
import(""FFITarget.dll"");
c = { {1.0, 2.0}, {3.0} };
c = ArrayMember.Ctor( c[0] );
x = c.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] ExpectedResult = { 1.0, 2.0 };

            thisTest.Verify("x", ExpectedResult, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void Z017_Defect_1456898_2()
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
a1;
b1;
[Imperative]
{
    a1 = foo( { 0, 1 } );
    b1 = foo( { 1, 2 } );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification           
            thisTest.Verify("a1", 0);
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z018_Defect_1456798()
        {
            string code = @"
import(""FFITarget.dll"");
x = { 0, 1, 2 };
a1 = ArrayMember.Ctor(x);
b1 = a1.X[0] + a1.X[1] + a1.X[2];
b2 = a1.Add();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification            
            thisTest.Verify("b1", 3);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z021_Defect_1458785_3()
        {
            string code = @"def foo ( i:int[]){return = i;}x =  1;a1 = foo(x);a2 = 3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a1", new object[] { 1 });
            thisTest.Verify("a2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z029_Calling_Class_Constructor_From_Instance_Negative()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
b1 = a1.ClassFunctionality();
c1 = b1.IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v2 = null;
            //Verification 
            thisTest.Verify("b1", v2);
            thisTest.Verify("c1", v2);

        }

        [Test]
        [Category("SmokeTest")]
        public void Z030_Class_Instance_Name_Same_As_Class_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
import(""FFITarget.dll"");
ClassFunctionality = ClassFunctionality.ClassFunctionality();
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void Z030_Class_Instance_Name_Same_As_Class_Negative_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
import(""FFITarget.dll"");
ClassFunctionality = ClassFunctionality.ClassFunctionality();
t = ClassFunctionality.IntaVal;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }
    }
}
