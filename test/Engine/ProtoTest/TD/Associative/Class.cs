using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Associative
{
    class Class : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T001_Associative_Class_Property_Int()
        {
            string code = @"
	class Point
	{
        _x : int;
        _y : int;
        _z : int;
                                
        constructor Point(xx : int, yy : int, zz : int)
        {
			_x = xx;
            _y = yy;
            _z = zz;
        }
                                
        def get_X : int () 
        {
            return = _x;
        }
    }
	
	newPoint = Point.Point(1,2,3);
	
	xPoint = newPoint._x;
    yPoint = newPoint._y;            
    zPoint = newPoint._z;               
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint", 1);
            thisTest.Verify("yPoint", 2);
            thisTest.Verify("zPoint", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T002_Associative_Class_Property_Double()
        {
            string code = @"
	class Point
	{
        _x : double;
        _y : int;
        _z : int;
                                
        constructor Point(xx : double, yy : double, zz : double)
        {
			_x = xx;
            _y = yy;
            _z = zz;
        }
                                
        def get_X : double () 
        {
            return = _x;
        }
    }
	
	newPoint = Point.Point(1.1,2.2,3.3);
	
	xPoint = newPoint._x;
    yPoint = newPoint._y;            
    zPoint = newPoint._z;               
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint", 1.1);
            thisTest.Verify("yPoint", 2);
            thisTest.Verify("zPoint", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T003_Associative_Class_Property_Bool()
        {
            string code = @"
	class Point
	{
        _x : bool;
                                
        constructor Point(xx : bool)
        {
			_x = xx;
        }
                                
    }
	newPoint1 = Point.Point(true);
	newPoint2 = Point.Point(false);
	propPoint1 = newPoint1._x;
    propPoint2 = newPoint2._x;            
              
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("propPoint1", true);
            thisTest.Verify("propPoint2", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_Associative_Class_Property_DefaultInitialization()
        {
            string code = @"
	class TestClass
	{
        _var : var;
		_int : int;
		_double : double;
		_bool : bool;
                                
        constructor TestClass ()
        {
        }       
    }
	newClass = TestClass.TestClass();
	defaultVar = newClass._var;
	defaultInt = newClass._int;
	defaultDouble = newClass._double;
	defaultBool = newClass._bool;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object defaultVar = null;
            thisTest.Verify("defaultVar", defaultVar);
            thisTest.Verify("defaultInt", 0);
            thisTest.Verify("defaultDouble", 0.0);
            thisTest.Verify("defaultBool", false);
            //"1453912"//
        }

        [Test]
        [Category("SmokeTest")]
        public void T005_Associative_Class_Property_Get_InternalClassFunction()
        {
            string code = @"
	class MyPoint
	{
		X : double;
		Y : double;
		Z : double;
                                
        constructor MyPoint (x : double, y : double, z : double)
        {
			X = x;
			Y = y;
			Z = z;
        } 
		
		def Get_X : double()
		{
			return = X;
		}
		
		def Get_Y : double()
		{
			return = Y;	
		}
		
		def Get_Z : double ()
		{
			return = Z;
		}
		
		def Sum : double ()
		{
			return = Get_X() + Get_Y() + Get_Z();
		}
    }
	
	myNewPoint = MyPoint.MyPoint (0.0, 1.2, 3.5);
	val = myNewPoint.Sum();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 4.7);
            //"1453886"//
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_Associative_Class_Property_UseInsideInternalClassFunction()
        {
            string code = @"
	class MyPoint
	{
		X : double;
		Y : double;
		Z : double;
                                
        constructor MyPoint (x : double, y : double, z : double)
        {
			X = x;
			Y = y;
			Z = z;
        } 
		
		def Sum : double ()
		{
			return = X + Y + Z;
		}
    }
	
	myNewPoint = MyPoint.MyPoint (0.0, 1.2, 3.5);
	val = myNewPoint.Sum();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 4.7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_Associative_Class_Property_CallFromFunctionOutsideClass()
        {
            string code = @"
	class MyPoint
	{
		X : double;
		Y : double;
		Z : double;
                                
        constructor MyPoint (x : double, y : double, z : double)
        {
			X = x;
			Y = y;
			Z = z;
        }
		
		
		def Get_X : double()
		{
			return = X;
		}
		
		def Get_Y : double()
		{
			return = Y;
		}
		def Get_Z : double()
		{
			return = Z;
		}
    }
	
	def GetPointValue : double (pt : MyPoint)
	{
		return = pt.Get_X() + pt.Get_Y() + pt.Get_Z(); 
	}
	
	myNewPoint = MyPoint.MyPoint (1.0, 10.1, 200.2);
	myPointValue = GetPointValue(myNewPoint);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("myPointValue", 211.3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_Associative_Class_Property_CallFromAnotherExternalClass()
        {
            string code = @"
class MyVector
{
	X : double;
	Y : double;
						
	constructor MyVector (x : double, y : double)
	{
		X = x;
		Y = y;
	}
}
class MyPoint
{
	X : double;
	Y : double;
	Z : double;
	
	constructor MyPoint (direction : MyVector, z : double)
	{
		X = direction.X;
		Y = direction.Y;
		Z = z;
	}
}
XYDirection = MyVector.MyVector (1.3,20.5);
myPoint = MyPoint.MyPoint(XYDirection, 300.8);
xPosition = myPoint.X;
yPosition = myPoint.Y;
zPosition = myPoint.Z;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPosition", 1.3);
            thisTest.Verify("yPosition", 20.5);
            thisTest.Verify("zPosition", 300.8);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_Associative_Class_Property_AssignInDifferentNamedConstructors()
        {
            string code = @"
	class MyPoint
	{
		X : double;
		Y : double;
		Z : double;
                            
        constructor ByXY (x : double, y : double)
        {
			X = x;
			Y = y;
			Z = 0.0;
        }
		
		constructor ByXZ (x : double, z : double)
        {
			X = x;
			Y = 0.0;
			Z = z;
        }
		
		constructor ByYZ (y : double, z : double)
        {
			X = 0.0;
			Y = y;
			Z = z;
        }
    }
    pt1 = MyPoint.ByXY (10.1, 200.2);
	pt2 = MyPoint.ByXZ (10.1, 3000.3);	
	pt3 = MyPoint.ByYZ (200.2,3000.3);
	
	xPt1 = pt1.X;
	yPt1 = pt1.Y;	
	zPt1 = pt1.Z;
	
	xPt2 = pt2.X;	
	yPt2 = pt2.Y;	
	zPt2 = pt2.Z;
	
	xPt3 = pt3.X;	
	yPt3 = pt3.Y;	
	zPt3 = pt3.Z;		
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPt1", 10.1);
            thisTest.Verify("yPt1", 200.2);
            thisTest.Verify("zPt1", 0.0);

            thisTest.Verify("xPt2", 10.1);
            thisTest.Verify("yPt2", 0.0);
            thisTest.Verify("zPt2", 3000.3);

            thisTest.Verify("xPt3", 0.0);
            thisTest.Verify("yPt3", 200.2);
            thisTest.Verify("zPt3", 3000.3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T010_Associative_Class_Constructor_Overloads()
        {
            string code = @"
	class MyPoint
	{
		X : double;
		Y : double;
		Z : double;
                            
        constructor Create (x : double, y : double, flag: bool)
        {
			X = x;
			Y = y;
			Z = 3000.1;
        }
		
		constructor Create (x : double, y : double)
        {
			X = x;
			Y = y;
			Z = 0.1;
        }
		
    }
    pt1 = MyPoint.Create (10.0,200.0);
	pt2 = MyPoint.Create (10.0,200.0, true);
	pt3 = MyPoint.Create (10.0,200.0, false);
	zPt1 = pt1.Z;
	zPt2 = pt2.Z;
	zPt3 = pt3.Z;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("zPt1", 0.1);
            thisTest.Verify("zPt2", 3000.1);
            thisTest.Verify("zPt3", 3000.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T011_Associative_Class_Property_ExtendedClass()
        {
            string code = @"
class MyPoint
	{
		X : double;
		Y : double;
		Z : double;
                            
        constructor ByXYZ (x : double, y : double, z: double)
        {
			X = x;
			Y = y;
			Z = z;
        }
	
    }
	
	class MyExtendedPoint extends MyPoint
	{
		constructor ByX (x : double)	
		{
			X = x;
			Y = 20.2;
			Z = 300.3;
		}
	}
xPt1;
yPt1;
zPt1;
[Associative]
{
   
    pt1 = MyExtendedPoint.ByX (10.1);
	xPt1 = pt1.X;
	yPt1 = pt1.Y;
	zPt1 = pt1.Z;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPt1", 10.1);
            thisTest.Verify("yPt1", 20.2);
            thisTest.Verify("zPt1", 300.3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_Associative_Class_Property_Var()
        {
            string code = @"
	class Point
	{
        _x : var;
        _y : var;
        _z : var;
                                
        constructor Create(xx : int, yy : double, zz : bool)
        {
			_x = xx;
            _y = yy;
            _z = zz;
        }
                                
	}
	newPoint = Point.Create(1, 2.0, true);
	
	xPoint = newPoint._x;
    yPoint = newPoint._y;            
    zPoint = newPoint._z;               
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint", 1);
            thisTest.Verify("yPoint", 2.0);
            thisTest.Verify("zPoint", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_Associative_Class_Property_GetFromAnotherConstructorInSameClass()
        {
            string code = @"
	class TestPoint
	{
        _x : var;
        _y : var;
        _z : var;
                                
        constructor Create(xx : int, yy : int, zz : int)
        {
			_x = xx; 
            _y = yy;
            _z = zz;
        }
		
		constructor Modify(oldPoint : TestPoint)
		
		{
		
		    _x = oldPoint._x +1;
			_y = oldPoint._y +1;
			_z = oldPoint._z +1;
		
		
		}
                                
	}
	oldPoint = TestPoint.Create(1, 2, 3);
	newPoint = TestPoint.Modify(oldPoint);
	xPoint = newPoint._x;
    yPoint = newPoint._y;            
    zPoint = newPoint._z;               
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint", 2);
            thisTest.Verify("yPoint", 3);
            thisTest.Verify("zPoint", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T014_Associative_Class_Property_GetUsingMultipleReferencingWithSameName()
        {
            string code = @"
	class TestPoint
	{
        X : var;
        Y : var;
             
        constructor Create(xx : int, yy : int)
        {
			X = xx; 
            Y = yy;
        }                            
	}
	
	class TestLine
	{
        X : TestPoint;
        Y : TestPoint;
             
        constructor Create(startPt : TestPoint, endPoint : TestPoint)
        {
			X = startPt; 
            Y = endPoint;
        }                            
	}
	
	
	
	pt1 = TestPoint.Create(1, 2);
	pt2 = TestPoint.Create(3, 4);
	line1 = TestLine.Create(pt1,pt2);
    test1 = line1.X.X;
    test2 = line1.X.Y;    
    test3 = line1.Y.X;
    test4 = line1.Y.Y; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test1", 1);
            thisTest.Verify("test2", 2);
            thisTest.Verify("test3", 3);
            thisTest.Verify("test4", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_Associative_Class_Property_SetInExternalFunction()
        {
            string code = @"
	class TestPoint
	{
        X : var;
        Y : var;
             
        constructor Create(xx : int, yy : int)
        {
			X = xx; 
            Y = yy;
        }            
       
	}
	
    def Modify : TestPoint(old : TestPoint)
    {
            old.X = 10;
            old.Y = 20;
            return = old;
    }     
	
	pt1 = TestPoint.Create(1, 2);
	pt2 = Modify(pt1);
	
	testX1 = pt1.X;
	testY1 = pt1.Y;
	
	
	
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void T016_Associative_Class_Property_SetInClassMethod()
        {
            string code = @"
	class TestPoint
	{
        X : var;
        Y : var;
             
        constructor Create(xx : int, yy : int)
        {
			X = xx; 
            Y = yy;
        }       
    def Modify : int()
    {
            X = 10;
            Y = 20;
			return = 10;
    }    		
       
	}
	pt1 = TestPoint.Create(1, 2);
	pt2 = pt1.Modify();
	
	testX1 = pt1.X;
	testY1 = pt1.Y;
	
	
	
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void T017_Associative_Class_Property_SetInExternalClassMethod()
        {
            string code = @"
	class TestPoint
	{
        X : var;
        Y : var;
             
        constructor Create(xx : int, yy : int)
        {
			X = xx; 
            Y = yy;
        }       
    def Modify : int()
    {
            X = 10;
            Y = 20;
			return = 10;
    }    		
       
	}
	
	class ExternalClass
	{
	
		constructor Create()
		
		{
			test = 1;
		}
		
		def Modify : int (origin : TestPoint)
		{
		
		origin.X = 10;
		origin.Y = 20;
		return = 5;
		
		}
	}
	pt1 = TestPoint.Create(1, 2);
	dummy = ExternalClass.Create();
	
	result = dummy.Modify(pt1);
	
	testX1 = pt1.X;
	testY1 = pt1.Y;
	
	
	
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void T018_Associative_Class_Constructor_WithSameNameAndArgument_Negative()
        {
            string code = @"
class TestClass
    {
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = x;
        Y = y;
        }
    constructor Create(x : double, y : double)
        {
        X = x;
        Y = y;
        }
    }
    test = TestClass.Create (10.0, 11.0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumentNumber()
        {
            string code = @"
class TestClass
    {
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = x + 10;
        Y = y + 10;
        }
    constructor Create(x : double, y : double, z: double)
        {
        X = x + 100;
        Y = y + 100;
        }
    }
    test1 = TestClass.Create (1.0, 2.0);
	test2 = TestClass.Create (1.0, 2.0,3.0);
	
	
	x1 = test1.X;
	y1 = test1.Y;
	x2 = test2.X;
	y2 = test2.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 11.0);
            thisTest.Verify("x2", 101.0);
            thisTest.Verify("y1", 12.0);
            thisTest.Verify("y2", 102.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T020_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumenType()
        {
            string code = @"
class TestClass
    {
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = x + 10;
        Y = y + 10;
        }
    constructor Create(x : bool, y : bool)
        {
        X = 100;
        Y = 100;
        }
    }
    test1 = TestClass.Create (1.0, 2.0);
	test2 = TestClass.Create (true, false);
	
	
	x1 = test1.X;
	y1 = test1.Y;
	x2 = test2.X;
	y2 = test2.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 11.0);
            thisTest.Verify("x2", 100);
            thisTest.Verify("y1", 12.0);
            thisTest.Verify("y2", 100);
        }

        [Test]
        [Category("SmokeTest")]
        public void T021_Associative_Class_Constructor_UsingUserDefinedClassAsArgument()
        {
            string code = @"
class MyClass
    
{     
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = x + 10;
        Y = y + 10;
        }  
}
class TestClass
    {
    X: var;
    Y: var;
    constructor Create(test : MyClass)
        {
        X = test.X + 10;
        Y = test.Y + 10;
        }
    }
    test1 = MyClass.Create (1.0, 2.0);
	test2 = TestClass.Create (test1);
	
	
	x2 = test2.X;
	y2 = test2.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x2", 21.0);
            thisTest.Verify("y2", 22.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Associative_Class_Constructor_AssignUserDefineProperties()
        {
            string code = @"
class MyPoint
    
{     
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = x;
        Y = y;
        }  
}
class MyLine
    {
    Start: MyPoint;
    End: MyPoint;
    constructor Create(start : MyPoint, end : MyPoint)
        {
        Start = start;
        End = end;
        }
    }
    p1 = MyPoint.Create(10.2,10.1);
	p2 = MyPoint.Create(-10.2, -10.1);
	l = MyLine.Create(p1,p2);
	
	lsX = l.Start.X;
	lsY = l.Start.Y;
	leX = l.End.X;
	leY = l.End.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("lsX", 10.2);
            thisTest.Verify("lsY", 10.1);
            thisTest.Verify("leX", -10.2);
            thisTest.Verify("leY", -10.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_Associative_Class_Constructor_CallingAFunction()
        {
            string code = @"
def Divide : double (a : double, b : double)
{
	return = (a+b)/2;
}
class MyPoint
    
{     
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = Divide(x+y, x+y);
        Y = Divide(x-y, x-y);
        }  
}
    p = MyPoint.Create(10.2,10.1);
	pX = p.X;
	pY = p.Y;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("pX", 20.3);
            thisTest.Verify("pY", 0.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T024_Associative_Class_Constructor_CallingAnImperativeFunction()
        {
            string code = @"
[Imperative]
{
	def Greater : double (a : double, b : double)
	{
		if (a > b)
			return = a;
		//else
			return = b;
	}
}
class MyPoint    
{     
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
    {
        X = Greater(x,y);
        Y = Greater(x,y);
    }  
}
p1 = MyPoint.Create(20.0,30.0);
p2 = MyPoint.Create(-20.0,-30.0);
p1X = p1.X;
p1Y = p1.Y;
p2X = p2.X;
p2Y = p2.Y;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;

            thisTest.Verify("p1X", v1);
            thisTest.Verify("p1Y", v1);
            thisTest.Verify("p2X", v1);
            thisTest.Verify("p2Y", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T025_Associative_Class_Constructor_CallingAnotherConstructor()
        {
            string code = @"
class MyPoint
    
{     
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
        {
        X = x;
        Y = y;
        }  
}
class MyLine
    {
    Start: MyPoint;
    End: MyPoint;
    constructor Create(x1: double, y1 : double, x2: double, y2: double)
        {
		p1 = MyPoint.Create(x1,y1);
		p2 = MyPoint.Create(x2,y2);
        Start = p1;
        End = p2;
        }
    }
	l = MyLine.Create(1.0, 2.0, -1.0, -2.0);
	
	lsX = l.Start.X;
	lsY = l.Start.Y;
	leX = l.End.X;
	leY = l.End.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("lsX", 1.0);
            thisTest.Verify("lsY", 2.0);
            thisTest.Verify("leX", -1.0);
            thisTest.Verify("leY", -2.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_Associative_Class_Constructor_BaseConstructorAssignProperties()
        {
            string code = @"
class MyPoint
    
    {
    
    X: double;
    Y: double;
    
    constructor CreateByXY(x : double, y : double)
        
        
        {
        X = x;
        Y = y;
        
        }   
    }
    
class MyNewPoint extends MyPoint
    {
    
    Z : double;
    
    constructor Create (x: double, y: double, z : double) : base.CreateByXY(x, y)
        {
         Z = z;
        }
    }
   
test = MyNewPoint.Create (10, 20, 30);
x = test.X;
y = test.Y;
z = test.Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10.0);
            thisTest.Verify("y", 20.0);
            thisTest.Verify("z", 30.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T027_Associative_Class_Constructor_BaseConstructorWithSameName()
        {
            string code = @"
class MyPoint
    
    {
    
    X: double;
    Y: double;
    
    constructor Create(x : double, y : double)
        
        
        {
        X = x;
        Y = y;
        
        }   
    }
    
class MyNewPoint extends MyPoint
    {
    
    Z : double;
    
    constructor Create (x: double, y: double, z : double) : base.Create(x, y)
        {
         Z = z;
        }
    }
   
test = MyNewPoint.Create (10.1, 20.2, 30.3);
z = test.Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 30.3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T028_Associative_Class_Property_DefaultAssignment()
        {
            string code = @"
	class MyPoint
	{
		X : double = 0;
		Y : double = 0;
		Z : double = 0;
                            
        constructor ByXY (x : double, y : double)
        {
			X = x;
			Y = y;
        }
	
    }
	
    pt1 = MyPoint.ByXY (1,2);
	xPt1 = pt1.X;
	yPt1 = pt1.Y;
	zPt1 = pt1.Z;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object xPt1 = 1.0;
            thisTest.Verify("xPt1", xPt1, 0);
            object yPt1 = 2.0;
            thisTest.Verify("yPt1", yPt1, 0);
            object zPt1 = 0.0;
            thisTest.Verify("zPt1", zPt1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T029_Associative_Class_Property_AccessModifier()
        {
            string src = @"class A
{
    private x:int;
    constructor A()
    {
        x = 3;
    }
}
a = A.A();
// compile error!
t = a.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kAccessViolation);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T030_Associative_Class_Property_AccessModifier()
        {
            string src = @"class A
{
    private x:int;
    constructor A()
    {
        x = 3;
    }
}
class B extends A
{
    def foo()
    {
        return = x;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kAccessViolation);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T031_Associative_Class_Property_AccessModifier()
        {
            string src = @"class A
{
    protected x:int;
    constructor A()
    {
        x = 3;
    }
}
class B extends A
{
    constructor B()
    {
        x = 4;
    }
    def foo:int()
    {
        return = x;
    }
}
b = B.B();
x = b.foo();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Inherit_defaultconstructor()
        {
            string code = @"
// default constructor
class baseClass
{ 
	val1 : int =2 ;
}
class derivedClass extends  baseClass
{ 
	val2 : int =1;
	val3 : double=1;
	
	
}
instance = derivedClass.derivedClass();
result1 = instance.val2;//1
result2 = instance.val3;//1
result3 = instance.val1;//2";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result1", 1);
            thisTest.Verify("result2", 1.0);
            thisTest.Verify("result3", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T052_Inherit_defaultproperty()
        {
            string code = @"
// access  property from inherited class and test default values are retained
class A
{ 
	x : var ;
	y : int;
	z : bool;
	u : double;
	v : B;
	w1 : int[];
	w2 : double[];
	w3 : bool[];
	w4 : B[][];
	
	constructor A ()
	{
		      	
	}	
}
class B extends A
{
	
}
a1 = B.B();
x1 = a1.x; // null
x2 = a1.y;//0
x3 = a1.z;//false
x4 = a1.u;//0.0
x5 = a1.v;//null
x6 = a1.w1;//0
x7 = a1.w2;//0.0
x8 = a1.w3;//false
x9 = a1.w4;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("x1", a);
            TestFrameWork.Verify(mirror, "x2", 0);
            TestFrameWork.Verify(mirror, "x3", false);
            TestFrameWork.Verify(mirror, "x4", 0.0);
            thisTest.Verify("x5", a);
            a = new object[] { };
            thisTest.Verify("x6", a);
            thisTest.Verify("x8", a);
            thisTest.Verify("x9", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T053_Inherit_changevalue()
        {
            string code = @"
// set variablse in abse class to default value and modify them in inherited class 
class A
{ 
	x : var ;
	y : int;
	z : bool;
	u : double;
	v : B;
	w1 : int[];
	w2 : double[];
	w3 : bool[];
	w4 : B[][];
	
	constructor A ()
	{
		      	
	}	
}
class B extends A
{
	constructor B()
	{
	y=1;
	z=true;
	u=0.5;
	w1={2,2};
	w2 ={0.5,0.5};
	w3 ={true,false};
	w4 ={this.x,this.x};
	}
}
a1 = B.B();
x1 = a1.x;//null
x2 = a1.y;//1
x3 = a1.z;//true
x4 = a1.u;//0.5
x5 = a1.v;//null
x6 = a1.w1;//{2,2}
x7 = a1.w2;//{0.5,0.5}
x8 = a1.w3;//{true,false}
x9 = a1.w4;//{null,null}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467246 - sprint25: rev 3445 : REGRESSION : Property setter/getter not working as expected for derived classes");
            thisTest.SetErrorMessage("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            object a = null;
            thisTest.Verify("x1", a);
            thisTest.Verify("x2", 1);
            thisTest.Verify("x3", true);
            thisTest.Verify("x4", 0.5);
            thisTest.Verify("x5", a);
            object[] arr = new object[] { 2, 2 };
            thisTest.Verify("x6", arr);
            object[] arr1 = new object[] { 0.5, 0.5 };
            thisTest.Verify("x7", arr1);
            object[] arr2 = new object[] { true, false };
            thisTest.Verify("x8", arr2);
            thisTest.Verify("x9", new object[] { null, null });
        }

        [Test]
        [Category("SmokeTest")]
        public void T056_Inherit_private()
        {
            string code = @"
// do not modify in extended class
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A ()
	{
		   	
	}
	public def foo1 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	def testprivate ()
       {
          return=foo2(1);
       }
}
class B extends A
{
   
	
}
a = B.B();
a1 = a.foo1(1);
a2 = a.foo2(1);//not accessible
a3=a.testprivate();//3
a.x = 4;
//a.y = 5;
a4 = a.x;//1
a5 = a.y;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object result = null;
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", result);
            thisTest.Verify("a3", 3);
            thisTest.Verify("a4", 4);
            thisTest.Verify("a5", result);

        }

        [Test]
        [Category("Method Resolution")]
        public void T057_Inherit_private_modify()
        {
            string code = @"
//modify in extended class 
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A ()
	{
		   	
	}
	public def foo1 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	def testprivate ()
        {
          return=foo2(1);
        }
}
class B extends A
{
        private def foo2 (a)
	{
	    x = a;
	    return = x ; 
        }
	def testextended ()
        {
          return=foo2(10);
        }
		
}
b = B.B();
b1 = b.foo1(1);
b2 = b.foo2(1);//private
b3=b.testprivate();//3
b4=b.testextended();//10  from the extended class 
b.x = 4;
//a.y = 5;
b5 = b.x;
b6 = b.y;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed");
            object result = null;
            thisTest.Verify("b1", 3);
            thisTest.Verify("b2", result);
            thisTest.Verify("b3", 1);
            thisTest.Verify("b4", 10);
            thisTest.Verify("b5", 4);
            thisTest.Verify("b6", result);
        }

        [Test]
        [Category("Method Resolution")]
        public void T058_Inherit_private_notmodify()
        {
            string code = @"
// do not modify private member
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A ()
	{
		   	
	}
	public def foo1 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	def testprivate ()
        {
          return=foo2(1);
        }
}
class B extends A
{
	def testextended ()
        {
          return=foo2(10);
        }
		
}
a = B.B();
a1 = a.foo1(1);
a2 = a.foo2(1);//private function
a3=a.testprivate();//3
a4=a.testextended();// private function null
a.x = 4;
//a.y = 5;
a5 = a.x;
a6 = a.y;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed"); 
            object result = null;
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", result);
            thisTest.Verify("a3", 3);
            thisTest.Verify("a4", result);
            thisTest.Verify("a5", 4);
            thisTest.Verify("a6", result);
        }

        [Test]
        [Category("Method Resolution")]
        public void T059_Inherit_access_privatemember()
        {
            string code = @"
//access (in extended class) the private property created in base class 
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A ()
	{
		   	
	}
	public def foo1 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	def testprivate ()
        {
          return=foo2(1);
        }
}
class B extends A
{
        private def foo2 (a)
	{
	    x = y;
	    return = x ; 
        }
	def testextended ()
        {
          return=foo2(10);
        }
		
}
a = B.B();
a1 = a.foo1(1);
a2 = a.foo2(1);// access private property from base class 
a3=a.testprivate();//3
a4=a.testextended();//10  from the extended class 
a.x = 4;
//a.y = 5;
a5 = a.x;
a6 = a.y;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed");

            object result = null;
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", result);
            thisTest.Verify("a3", null);
            thisTest.Verify("a4", result);
            thisTest.Verify("a5", 4);
            thisTest.Verify("a6", result);
        }

        [Test]
        [Category("SmokeTest")]
        public void T061_Inherit_Property()
        {
            string code = @"
class TestPoint
{
    A : var;
           
    constructor Create( xx : int )
    {
	    A = xx;          
    }
	
	def Modify( )		
	{	
	    A = A + 1;
	    return = A;
	}
                                
}
class derived extends TestPoint
{
	D : var;
           
    constructor Create( xx : int ): base.Create( xx )
    {
	    D = xx; 
    }
	
	def Modify( val:int )
	{
	    D = A + val;
	    return = D;
	}   
}
oldPoint = TestPoint.Create(1);
derivedpoint=derived.Create(7);
basePoint=oldPoint.Modify();
callbase=derivedpoint.Modify();
derivedPoint2 = derivedpoint.Modify(2);
xPoint1 = oldPoint.A;
xPoint2 = derivedpoint.A;
xPoint3 = derivedpoint.D;
    
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("basePoint", 2);
            thisTest.Verify("callbase", 8);
            thisTest.Verify("derivedPoint2", 10);
            thisTest.Verify("xPoint1", 2);
            thisTest.Verify("xPoint2", 8);
            thisTest.Verify("xPoint3", 10);

        }

        [Test]
        [Category("SmokeTest")]
        public void T062_Inherit_classAsArgument()
        {
            string code = @"
class TestPoint
{
            A : var;
            B : var;
            C : var;
                                
        constructor Create(xx : int, yy : int, zz : int)
        {
	    A = xx; 
            B = yy;
            C = zz;
        }
	def Modify(oldPoint : TestPoint)
	{
	    return=TestPoint.Create(oldPoint.A + 1, oldPoint.B + 1, oldPoint.C + 1);	
	}
                                
}
class derived extends TestPoint
{
     constructor Create(xx : int, yy : int, zz : int)
        {
	    A = xx; 
            B = yy;
            C = zz;
     }
	def Modify(oldPoint : TestPoint, val:int)
		
	{
        return = derived.Create(oldPoint.A + val, oldPoint.B + val, oldPoint.C + val);
		
	}   
}
	oldPoint = TestPoint.Create(1, 2, 3);
        derivedpoint=derived.Create(7,8,9);
	basePoint=oldPoint.Modify(derivedpoint);
	derivedPoint2 =derivedpoint.Modify(derivedpoint, 2);
	xPoint1 = oldPoint.A; //1
        yPoint1 = oldPoint.B;    //2        
        zPoint1 = oldPoint.C;       //3        
        xPoint2 = derivedpoint.A; //9
        yPoint2 = derivedpoint.B;    //10 
        zPoint2 = derivedpoint.C;//11
        xPoint3 = basePoint.A;//8
        yPoint3 = basePoint.B;   //9         
        zPoint3 = basePoint.C;//10
        xPoint4 = derivedPoint2.A;//9
        yPoint4 = derivedPoint2.B;//10            
        zPoint4 = derivedPoint2.C; //11            
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xPoint1", 1);
            thisTest.Verify("yPoint1", 2);
            thisTest.Verify("zPoint1", 3);
            thisTest.Verify("xPoint2", 7);
            thisTest.Verify("yPoint2", 8);
            thisTest.Verify("zPoint2", 9);
            thisTest.Verify("xPoint3", 8);
            thisTest.Verify("yPoint3", 9);
            thisTest.Verify("zPoint3", 10);
            thisTest.Verify("xPoint4", 9);
            thisTest.Verify("yPoint4", 10);
            thisTest.Verify("zPoint4", 11);

        }

        [Test]
        [Category("SmokeTest")]
        public void T063_Inherit_classAsArgument_callinfunction()
        {
            string code = @"
class TestPoint
{
            A : var;
            B : var;
            C : var;
                                
        constructor Create(xx : int, yy : int, zz : int)
        {
	    A = xx; 
            B = yy;
            C = zz;
        }
                                
}
class derived extends TestPoint
{
    
           
     constructor Create(xx : int, yy : int, zz : int)
        {
	        A = xx; 
            B = yy;
            C = zz;
     }
	   
}
def modify(oldPoint : TestPoint)
		
        {
	
	    A1 = oldPoint.A +1;
	  
	    return=A1;
		
        }
        oldPoint = TestPoint.Create(1, 2, 3);
        derivedpoint=derived.Create(7,8,9);
	basePoint=modify(oldPoint);//2
	derivedPoint2 = modify(derivedpoint);//8
	xPoint1 =  oldPoint.A; //1
        yPoint1 =  oldPoint.B;    //2        
        zPoint1 =  oldPoint.C;       //3        
        xPoint2 = derivedpoint.A; //7
        yPoint2 = derivedpoint.B;    //8        
        zPoint2 = derivedpoint.C;//9           
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("basePoint", 2);
            thisTest.Verify("derivedPoint2", 8);
            thisTest.Verify("xPoint1", 1);
            thisTest.Verify("yPoint1", 2);
            thisTest.Verify("zPoint1", 3);
            thisTest.Verify("xPoint2", 7);
            thisTest.Verify("yPoint2", 8);
            thisTest.Verify("zPoint2", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T064_Inherit_classAsArgument_callinfunction()
        {
            string code = @"
// do not extend and try calling
class TestPoint
{
            A : var;
            B : var;
            C : var;
                                
        constructor Create(xx : int, yy : int, zz : int)
        {
	    A = xx; 
            B = yy;
            C = zz;
        }
                                
}
class derived 
{
            A : var;
            B : var;
            C : var;
           
     constructor Create(xx : int, yy : int, zz : int)
        {
	        A = xx; 
            B = yy;
            C = zz;
     }
	   
}
def modify(oldPoint : TestPoint)
		
        {
	
	    A1 = oldPoint.A +1;
	  
	    return=A1;
		
        }
        oldPoint = TestPoint.Create(1, 2, 3);
        derivedpoint=derived.Create(7,8,9);
	basePoint=modify(oldPoint);//2
	derivedPoint2 = modify(derivedpoint);//8
	xPoint1 =  oldPoint.A; //1
        yPoint1 =  oldPoint.B;    //2        
        zPoint1 =  oldPoint.C;       //3        
        xPoint2 = derivedpoint.A; //7
        yPoint2 = derivedpoint.B;    //8        
        zPoint2 = derivedpoint.C;//9           
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("basePoint", 2);
            object a = null;
            thisTest.Verify("derivedPoint2", a);
            thisTest.Verify("xPoint1", 1);
            thisTest.Verify("yPoint1", 2);
            thisTest.Verify("zPoint1", 3);
            thisTest.Verify("xPoint2", 7);
            thisTest.Verify("yPoint2", 8);
            thisTest.Verify("zPoint2", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T065_Inherit_constructor_withoutbase()
        {
            string code = @"
class TestPoint
{
    A : var;
           
    constructor Create(xx : int)
    {
	    A = xx; 
    }
	
	def Modify()
	{
	    A = A +1;
	    return = A;
	}
                                
}
class derived extends TestPoint
{
	    D : var;
           
        constructor Create(xx : int)
        {
	        D = xx;           
        }
	
	def Modify(val:int)		
	{
	    D = A +val;
	    return = D;		
	}   
}
	
oldPoint = TestPoint.Create(1);
derivedpoint=derived.Create(7);
basePoint=oldPoint.Modify();
xPoint1 = oldPoint.A;//2
xPoint2 = derivedpoint.A;//1
xPoint3 = derivedpoint.D;//7";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("basePoint", 2);
            object a = null;
            thisTest.Verify("xPoint1", 2);
            thisTest.Verify("xPoint2", a);
            thisTest.Verify("xPoint3", 7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T066_Inherit_constructor_failing_witbase()
        {
            string code = @"
// failing constructor in base
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		 x = w;     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B ():base.A()
	{
		 y = 10;     	
	}	
}
a1 = A.A();
b1=B.B();
a2 = a1.x;//null
b2 = b1.y;//10
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", 10);


        }

        [Test]
        [Category("SmokeTest")]
        public void T067_Inherit_propertynotassignedinbase()
        {
            string code = @"
// no value in base
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B ():base.A()
	{
		 y = 10;     	
	}	
}
a1 = A.A();
b1=B.B();
a2 = a1.x;//null
b2 = b1.y;//10
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", 10);


        }

        [Test]
        [Category("SmokeTest")]
        public void T068_Inherit_propertyassignedinherited()
        {
            string code = @"
// failing constructor in base reassigned in inherited
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		x=w;     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B ():base.A()
	{
		 x = 10;     	
	}	
}
a1 = A.A();
b1=B.B();
a2 = a1.x;//null
b2 = b1.y;//null
b3 = b1.x;//10
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", a);
            thisTest.Verify("b3", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T069_Inherit_constructor_failing_both()
        {
            string code = @"
// failing constructor in both
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		 x = w;     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B ():base.A()
	{
		 y = w;     	
	}	
}
a1 = A.A();
b1=B.B();
a2 = a1.x;//null
b2 = b1.y;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", a);

        }

        [Test]
        [Category("SmokeTest")]
        public void T070_Inherit_constructor_failing_inherited()
        {
            string code = @"
// failing constructor in inherited
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		 x = 10;     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B ():base.A()
	{
		 y = w;     	
	}	
}
a1 = A.A();
b1=B.B();
a2 = a1.x;//10
b2 = b1.y;//null
b3=b1.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a2", 10);
            thisTest.Verify("b2", a);
            thisTest.Verify("b3", 10);
        }


        [Test]
        [Category("SmokeTest")]
        public void T071_Inherit_constructor_failing_inherited_sameproperty()
        {
            string code = @"
// failing constructor in inherited same property failing in inherited
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		 x = 10;     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B ():base.A()
	{
		 x = w;     	
	}	
}
a1 = A.A();
b1=B.B();
a2 = a1.x;//10
b2 = b1.y;//null
b3=b1.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a2", 10);
            thisTest.Verify("b2", a);
            thisTest.Verify("b3", a);
        }

        [Test]
        [Category("Type System")]
        public void T072_inherit_Class_Constructor_CallingAFunction()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
def Divide : double (a : double, b : double)
{
	return = (a+b)/2;
}
class MyPoint
    
{     
    X: var;
    Y: var;
    constructor Create(x : double, y : double)
    {
        X = Divide(x+y, x+y);
        Y = Divide(x-y, x-y);
    }  
}
class testPoint extends MyPoint
    
{     
    
    constructor Create(x : double, y : double)
    {
        X = Divide(x*y, x/y);
        Y = Divide(x/y, x*y);
    }  
}
p = testPoint.Create(10,10);
pX = p.X;
pY = p.Y;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("pX", 50.5);
            thisTest.Verify("pY", 50.5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T073inherit_Constructor_WithSameNameAndDifferentArgumenType()
        {
            string code = @"
class TestClass
    {
    X: var;
    Y: var;
    public constructor Create(x : double, y : double)
        {
        X =  x;
        Y =  y;
        }
    constructor Create(x : bool, y : bool)
        {
        X = x;
        Y = y;
        }
    }
class myClass extends TestClass
{
 public constructor Create(x : double, y : double):base.Create(x,y)
        {
        X =  x;
        Y =  y;
        }
    constructor Create(x : bool, y : bool):base.Create(x,y)
        {
        X = x;
        Y = y;
        }
}
test1 = myClass.Create(1.0,2.0);
test2 = myClass.Create (true, false);
	
	x1 = test1.X;
	y1 = test1.Y;
	
	x2 = test2.X;
	y2 = test2.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("y1", 2.0);
            thisTest.Verify("x2", true);
            thisTest.Verify("y2", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T074_Inherit_property_array()
        {
            string code = @"
// array assign value in inherited 
class A
{ 
	y : int[];
	
	
	constructor A ()
	{
		      	
	}	
}
class B extends A
{
	constructor B()
	{
	y={1,2};
	}
}
a1 = B.B();
x1 = a1.y;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] a = new object[] { 1, 2 };
            thisTest.Verify("x1", a);

        }

        [Test]
        [Category("SmokeTest")]
        public void T075_Inherit_property_array_modify()
        {
            string code = @"
// array assign value in inherited 
class A
{ 
	y : int[];
	
	
	constructor A ()
	{
	y={1,2};	      	
	}	
}
class B extends A
{
	constructor B()
	{
	y={3,4};	
	}
}
a1 = B.B();
x1 = a1.y;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] a = new object[] { 3, 4 };
            thisTest.Verify("x1", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T076_Inherit_property_array_modifyanitem()
        {
            string code = @"
// array assign value in inherited 
class A
{ 
	y : int[];
	
	
	constructor A ()
	{
	y={1,2};	      	
	}	
}
class B extends A
{
	constructor B()
	{
	
         y[0]=-1; 
	}
}
a1 = B.B();
x1 = a1.y;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] a = new object[] { -1, 2 };
            thisTest.Verify("x1", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T077_Inherit_property_thatdoesnotexist()
        {
            string code = @"
// array assign value in inherited 
class A
{ 
	y : int[];
	
	
	constructor A ()
	{
	y={1,2};	      	
	}	
}
class B extends A
{
	constructor B()
	{
	
         y[0]=-1; 
	}
}
a1 = B.B();
x1 = a1.z;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("x1", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T078_Inherit_property_singletonconvertedtoarray()
        {
            string code = @"
class A
{ 
	y : int;
	
	
	constructor A ()
	{
    	y=1;	      	
	}	
}
class B extends A
{
	constructor B()
	{
	y={3,4};	
	}
}
a1 = B.B();
x1 = a1.y;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null);
        }


        [Test]
        [Category("SmokeTest")]
        public void Z001_Associative_Class_Property_Regress001()
        {
            string code = @"
class Point
    {
        id : var;
        x : var;
        y : var;
        z : var;
        
        constructor ByCoordinates(xx : double, yy : double, zz : double)
        {
            id = 0;
            
            x = xx;
            y = yy;
            z = zz;
        }
    }
    
    class Line
    {
        id : var;
        startPoint : var;
        endPoint : var;
        
        constructor ByStartPointEndPoint(sp : Point, ep : Point)
        {
            id = 2;
            
            startPoint = sp;
            endPoint = ep;
        }
    }
sp_x;  
[Associative]
{   
    sp = Point.ByCoordinates(1,2,3);
    ep = Point.ByCoordinates(11,12,13);
    
    l = Line.ByStartPointEndPoint(sp, ep);
    
    sp_x = l.startPoint.x; // this line causes failure
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sp_x", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z002_Associative_Class_Property_Regress_1454056()
        {
            string src = @"class MyPoint
{
	X : double;
	Y : double;
	Z : double;
							
	constructor MyPoint (x : double, y : double, z : double)
	{
		X = x;
		Y = y;
		Z = z;
	} 
	
	def Sum : double ()	{ return = X + Y + Z; }
}
val;
[Associative]
{	
	myNewPoint = MyPoint.MyPoint(0.0, 1.2, 3.5);
	val = myNewPoint.Sum();
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("val", 4.7);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z003_Associative_Class_Property_Regress_1454164()
        {
            string code = @"
class Sample
    {
        constructor Create(val : int)
        {
            
        }
    }
[Associative]
{
 
    s = Sample.Create(20);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //No value verification needed. Only need to safeguard unexpected assert. 
        }

        [Test]
        [Category("SmokeTest")]
        public void Z004_Associative_Class_Property_Regress_1454138()
        {
            string code = @"
class Sample
    {
		X : var;
		
        constructor Create()
        { X = 0.1;        }
        
        constructor Create(intval : int)
        { X = 1.1;   }
        
        constructor Create(doubleval : double)
        { X = 2.1;   }
        
        constructor Create(intval : int, doubleval : double)
        { X = 3.1;  }
        
        def GetX : double () {return = X;}
    }
test1;
test2;
test3;
test4;
[Associative]
{
    
    
    //    default ctor
    s1 = Sample.Create();
    test1 = s1.GetX();
    
    //    ctor with int
    s2 = Sample.Create(1);
    test2 = s2.GetX();
    
    //    ctor with double
    s3 = Sample.Create(1.0);
    test3 = s3.GetX();
    //    ctor with int and double
    s4 = Sample.Create(1, 1.0);
    test4 = s4.GetX();
    
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test1", 0.1);
            thisTest.Verify("test2", 1.1);
            thisTest.Verify("test3", 2.1);
            thisTest.Verify("test4", 3.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z005_Associative_Class_Property_Regress_1454178()
        {
            string code = @"
 class Tuple4
    {
        X : var;
        Y : var;
        Z : var;
        H : var;
        
        constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
        {
            X = xValue;
            Y = yValue;
            Z = zValue;
            H = hValue;        
        }
        
        constructor XYZ(xValue : double, yValue : double, zValue : double)
        {
            X = xValue;
            Y = yValue;
            Z = zValue;
            H = 1.0;        
        }
        
        def Multiply : double (other : Tuple4)
        {
            return = X * other.X + Y * other.Y + Z * other.Z + H * other.H;
        }
        
        
    }
sum;
[Associative]
{
   
    
    
    t1 = Tuple4.XYZH(1,1,1,1);
 
    
    t2 = Tuple4.XYZ(1,1,1);
    
    
    sum = t1.Multiply(t2);
    
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 4.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z006_Associative_Class_Property_Regress_1453886()
        {
            string code = @"
 class Sample
    {
        _val : var;
        
        constructor Sample()
        {
            _val = 1.1;
        }
        def get_Val : double ()
        {
            return = _val;
        }
    }
    
    class Sample2
    {
        constructor Sample2()
        {}
        
        def function1 : double (s : Sample )
        {
            return = s.get_Val();
            //return = 1;
        }
    }
one;
[Associative]
{
   
    
     s1 = Sample.Sample();
     s = Sample2.Sample2();
     one = s.function1(s1);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("one", 1.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z007_Associative_Class_Property_Regress_1454172()
        {
            string code = @"
 class Tuple4
    {
        X : var;
        Y : var;
        Z : var;
        H : var;
        
        constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
        {
            X = xValue;
            Y = yValue;
            Z = zValue;
            H = hValue;        
        }
        
        
        def Coordinates3 : double[] ()
        {
            return = { X, Y, Z };
        }
        
    }
sum;
[Associative]
{
   
    
    
    t1 = Tuple4.XYZH(1,1,1,1);
    sum = t1.Coordinates3();
    
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedA = { 1.0, 1.0, 1.0 };
            thisTest.Verify("sum", expectedA);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z008_Associative_Class_Property_Regress_1454161()
        {
            string code = @"
    class Tuple4
    {
        X : var;
        Y : var;
        Z : var;
        H : var;
        
        constructor ByCoordinates3(coordinates : double[] )
        {
            X = coordinates[0];
            Y = coordinates[1];
            Z = coordinates[2];
            H = 1.0;        
        }
        
    }
x3;
y3;
z3;
h3;
    [Associative]
    {
    t3 = Tuple4.ByCoordinates3({1.0,1.0,1.0});
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
        [Category("SmokeTest")]
        public void Z009_Associative_Class_Property_Regress_1453891()
        {
            string code = @"
class Point
{
    _x : var;
    _y : var;
    _z : var;
                                
    constructor Point(xx : double, yy : double, zz : double)
    {
        _x = xx;
        _y = yy;
        _z = zz;
    }
                                
    def get_X : double () 
    {
        return = _x;
    }
    def get_Y : double () 
    {
        return = _y;
    }
    def get_Z : double () 
    {
        return = _z;
    }
}            
    
class Line        
{
    _sp : var;
    _ep : var;
                    
    constructor Line(startPoint : Point, endPoint : Point)
    {
        _sp = startPoint; 
        _ep = endPoint;
                    
    }
    def get_StartPoint : Point ()
    {                              
        return = _sp;
    }
                                                
    def get_EndPoint : Point () 
    {
        return = _ep;
    }         
}
                
pt1 = Point.Point(3.1,2.1,1.1);
pt2 = Point.Point(31.1,21.1,11.1);
l = Line.Line(pt1, pt2);               
l_startPoint_X = l.get_StartPoint().get_X();
  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("l_startPoint_X", 3.1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z010_Associative_Class_Property_Regress_1454658()
        {
            string code = @"
class Tuple4
{
	X : var;
	Y : var;
	Z : var;
	H : var;
	constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
	{
		X = xValue;
		Y = yValue;
		Z = zValue;
		H = hValue;		
	}
	
}
class Transform
{
	C0 : var;
    C1 : var; 
    C2 : var; 
    C3 : var; 
    
    constructor ByTuples(C0Value : Tuple4, C1Value : Tuple4, 
                         C2Value : Tuple4, C3Value : Tuple4)
    {
    	C0 = C0Value;
        C1 = C1Value;
        C2 = C2Value;
        C3 = C3Value;
    }
    
    def ApplyTransform : Tuple4 (t : Tuple4)
    {
        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X); // This is what causes the problem
        return = t; 
    }
    
}
[Associative]
{
	t1 = Tuple4.XYZH(1.0,0.0,0.0,0.0);
	t2 = Tuple4.XYZH(0.0,1.0,0.0,0.0);
	t3 = Tuple4.XYZH(0.0,0.0,1.0,0.0);
	t4 = Tuple4.XYZH(0.0,0.0,0.0,1.0);
	
	xform = Transform.ByTuples(t1, t2, t3, t4);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //No specific verification needed, prevent compilation failure only.
        }

        [Test]
        [Category("SmokeTest")]
        public void Z011_Associative_Class_Property_Regress_1454162()
        {
            string code = @"
    class Tuple4
    {
        X : var;
        Y : var;
        Z : var;
        H : var;
        
        constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)
        {
            X = xValue;
            Y = yValue;
            Z = zValue;
            H = hValue;        
        }
        
        constructor ByCoordinates3(coordinates : double[] )
        {
            X = coordinates[0];
            Y = coordinates[1];
            Z = coordinates[2];
            H = 1.0;        
        }
        
    }
    
    
    t1 = Tuple4.XYZH(1.0,1.0,1.0,1.0);
    x1 = t1.X;
    y1 = t1.Y;
    z1 = t1.Z;
    h1 = t1.H;
    result1 = {x1, y1, z1, h1};
    
    
    
    t2 = Tuple4.ByCoordinates3({1.0,1.0,1.0});
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
        [Category("SmokeTest")]
        public void Z012_Access_Class_Property_From_If_Block_In_Class_Method_Regress_1456397()
        {
            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1 ;
	}
	
	def CreateNewVal ( ) 
    {
		y = [Imperative]
		{
			if ( a  < 10 )
			{
				return = a + 10;
			}
			return = a; 
		}
		
		return = y + a;     
    }
	
	
}
b1;
[Associative]
{
    a1 = A.CreateA(1);
	b1 = a1.CreateNewVal();
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 12);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z013_Defect_1457038()
        {
            string code = @"
class collection{
                                
	a : int[];                                
	constructor create( b : int[])                
	{
		a = b;                
	}                                
	def ret_col ( )                
	{
		return = a[0];                
	}
}
class A {
    a: int[];
    constructor create() 
    {
        a= {1, 2, 3};
    }
    def foo:int()
    {
         b = { 1, 2, 3 };
		 //a[2] = a[0] + a[1];
		 return = b[2];
    }
}
x = A.create();
y = x.foo();
d;
[Associative]
{                
    //c = { 3, 3 };
	c1 = collection.create( { 4, 3 } );                
	d = c1.ret_col();
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 4);
            thisTest.Verify("y", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z014_Defect_1457057()
        {
            string code = @"
class Point
{
    X : var;
    Y : var;
    Z : var;
    id : var;
    
    constructor ByCoordinates(x : double, y : double, z : double, i : int)
    {
        X = x;
        Y = y;
        Z = z;
		id = i;
    }
}
def length  (pts : Point[])
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
class BSplineCurve
{
    //id : var;
    numPts : var;
    ids : var;
        
    private def getIds : int[] (pts : Point[])
    {
        ids = [Imperative]
        {
            len = length(pts);
            arr = 0.0..len;
            
            // problem with this for loop!
            
            counter = 0;
            for(pt in pts)
            {
                arr[counter] = pt.id;
                counter = counter + 1;
            }
            
            return = arr;
        }
        
        return = ids;
    }
    
    constructor ByPoints(ptsOnCurve : Point[])
    {
        numPts = length(ptsOnCurve);
        ids = null;
		//ids = getIds(ptsOnCurve);
    }
}
pt1 = Point.ByCoordinates(0,0,0,0);
pt2 = Point.ByCoordinates(5,0,0,1);
pts = {pt1, pt2};
bcurve = BSplineCurve.ByPoints(pts);
numpts = bcurve.numPts;
//ids = bcurve.ids;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 2, 0);
        }

        [Test]
        public void Z015_Defect_1457029()
        {
            //Assert.Fail("1457029 - Sprint25: Rev 3369 : Passing a collection value using index as argument to a class method is failing");

            string code = @"
class A
{
	Pt : double;
	constructor A (pt : double)
	{
		Pt = pt;
	}
	
	
}
	
c1 = { 1.0, 2.0, 3.0 };
c1 = A.A( c1[0] );
x = c1.Pt;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0, 0);
        }

        [Test]
        public void Z015_Defect_1457029_2()
        {
            //Assert.Fail("1457029 - Sprint25: Rev 3369 : Passing a collection value using index as argument to a class method is failing");

            string code = @"
class A
{
	Pt : double[];
	constructor A (pt : double[])
	{
		Pt = pt;
	}
	
	
}
	
c = { {1.0, 2.0}, {3.0} };
c = A.A( c[0] );
x = c.Pt;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] ExpectedResult = { 1.0, 2.0 };

            thisTest.Verify("x", ExpectedResult, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void Z016_Defect_1456771()
        {
            string code = @"
class A
{
    a : int;
	constructor A ( i : int)
	{
	    a = i ;
	}
	
	def foo1 ( y )
	{
		
		return = a + y;
	}
	
}
class B extends A
{
    b : int;
	constructor B ( i : int)
	{
	    a = i;
		b = i;
	}
	
	def foo2 ( x )
	{
		
		return = foo1 ( x ) + b + a;
	}
	
}
b1 = B.B(2);
t1 = b1.foo2(1);
t2 = b1.foo1(1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t2", 3, 0);
            thisTest.Verify("t1", 7, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void Z017_Defect_1456898()
        {
            string code = @"
class A
{
                a : var;
                
                constructor CreateA ( a1 : int )
                {              
                                a = a1 ;
                }                              
                
                def CreateNewVal ( )
                {
                                y = [Associative]
                                {
                                                
                                    return = a + 1;
                               }
                                return = y + a;
                }
}
b1;
[Imperative]
{
    a1 = A.CreateA(1);
    b1 = a1.CreateNewVal();
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object VariableToCheck = 3;
            thisTest.Verify("b1", VariableToCheck);
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
        [Category("SmokeTest")]
        public void Z018_Defect_1456798()
        {
            string code = @"
class A
{    
	a : var[];    
	constructor ByPoints(ptsOnCurve : int[])    
	{        
		a = ptsOnCurve;    
	}		
	
	def add()    
	{        
		return = a[0] + a[1] + a[2];    
	}
}
x = { 0, 1, 2 };
a1 = A.ByPoints(x);
b1 = a1.a[0] + a1.a[1] + a1.a[2];
b2 = a1.add();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification            
            thisTest.Verify("b1", 3);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z018_Defect_1456798_2()
        {
            string code = @"
class B
{    
	a : var;    
	constructor B ( b : double )    
	{        
		a = b;    
	}		
	
}
class A
{    
	a : var[];    
	constructor A ( b : B[] )    
	{        
		a = b;    
	}		
	
}
a2;
[Imperative]
{
	x = { 0.5, 1.0, 2.0 };
	b1 = B.B( x );
	a1 = A.A(b1);
	a2 = a1.a[0].a + a1.a[1].a +a1.a[2].a ;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification            
            thisTest.Verify("a2", 3.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z018_Defect_1456798_3()
        {
            string code = @"
class A
{    
	a : var[];    
	constructor A ( b : int )    
	{        
		a = { b, b, b };    
	}		
	
}
a2;
[Imperative]
{
	x = { 1, 2, 3 };
	a1 = A.A( x );
	t1 = a1[0];
	t2 = a1[1];
	t3 = a1[2];
	a2 = t1.a[0] + t2.a[1] + t3.a[2];
	//a2 = a1[0].a[0] + a1[1].a[1] +a1[2].a[2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification            
            thisTest.Verify("a2", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z019_Defect_1457053()
        {
            string code = @"
class A
{
    a : int[];
    constructor A1(ptsOnCurve : int[])
    {
        a = ptsOnCurve;
    }
	
	constructor A2()
    {
        a = { 1, 2, 3.5};
    }
	
	constructor A3(i : int)
    {
        a = {i, i ,i};
    }
	
	def add()
    {
        return = a[0] + a[1] + a[2];
    }
}
x = { 1, 2, 3 };
a1 = A.A1(x);
b1 = a1.a[0] + a1.a[1] + a1.a[2];
b2 = a1.add();
a2 = A.A2 ( );
b3 = a2.a[0] + a2.a[1] + a2.a[2];
b4 = a2.add();
a3 = A.A3 ( 3 );
b5 = a3.a[0] + a3.a[1] + a3.a[2];
b6 = a3.add();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification  
            thisTest.Verify("b1", 6);
            thisTest.Verify("b2", 6);
            thisTest.Verify("b3", 7);
            thisTest.Verify("b4", 7);
            thisTest.Verify("b5", 9);
            thisTest.Verify("b6", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z020_Defect_1457290()
        {
            string code = @"
class A {
    a: int[][];
    constructor create(b : int[][]) 
    {
        a=b;
		a[0][0] = 20;
    }
    def foo:int[]()
    {
         
		 a[0][1] = a[0][0];
		 return = a[0];
    }
}
b = { { 1, 2 },  { 3, 4 } };
a1 = A.create(b);
a2 = a1.foo();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 20, 20 };
            //Verification 
            thisTest.Verify("a2", a);

        }

        [Test]
        [Category("SmokeTest")]
        public void Z020_Defect_1457290_2()
        {
            string code = @"
class A {
    a: int[][];
    constructor create(b : int[][]) 
    {
        a=b;
		a[0][0] = 20;
    }
    def foo:int[]()
    {
         
		 a[0][1] = a[0][0];
		 return = a[0];
    }
}
a2;
[Imperative]
{
	b = { { 1, 2 },  { 3, 4 } };
	a1 = A.create(b);
	a2 = a1.foo();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 20, 20 };
            //Verification 
            thisTest.Verify("a2", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z021_Defect_1458785()
        {
            string code = @"
class A
{
        x : var;
	constructor A ()
	{
	    x = 1;
	}
	constructor A2 (x1 : int)
	{
	    x = x1;
	    y = 3;
	}
	
	def foo ( i )
	{
		return = i;
	}
}
	
a1 = A.A();
a2 = a1.foo();
a3 = 2;
a4;
a5;
[Imperative]
{
	def foo ( a:A )
	{
	    return = a.x;
	}
		
	a4 = foo(x);
	a5 = 2;
}
	
a11 = A.A2(2);
x11 = a11.x;
y11 = a11.y;
z11 = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
            //Verification 
            thisTest.Verify("a3", 2);
            thisTest.Verify("a2", v1);
            thisTest.Verify("a4", v1);
            thisTest.Verify("a5", 2);
            thisTest.Verify("y11", v1);
            thisTest.Verify("z11", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z021_Defect_1458785_2()
        {
            string code = @"
class A
{
        x : var;
	constructor A ()
	{
	    x = 1;
	}
	
}
	
a1 = A.A(1);
a2 = a1.foo();
a3 = a1.x();
a4 = 2;
a5 = foo();
a6 = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
            //Verification 
            thisTest.Verify("a1", v1);
            thisTest.Verify("a2", v1);
            thisTest.Verify("a3", v1);
            thisTest.Verify("a4", 2);
            thisTest.Verify("a5", v1);
            thisTest.Verify("a6", 3);

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
        public void Z021_Defect_1458785_4()
        {
            string code = @"
class A
{
    private x:int;
    constructor A()
    {
        x = 3;
    }
    def testPublic() 
    {
        x=x+2; // x= 5 
        return= x;
    }
     private def testprivate()
    {
        x=x-1;  
        return =x;
    }
    def testmethod() // to test calling private methods
    {
        a=testprivate();
        return=a;
    }
    
}
test1=A.A();
test2=z.x; 
test3=test1.testPublic();
test4=test1.testprivate();
test5= test1.testmethod(); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
            //Verification 
            thisTest.Verify("test2", v1, 0);
            thisTest.Verify("test3", 5, 0);
            thisTest.Verify("test4", v1, 0);
            thisTest.Verify("test5", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z021_Defect_1458785_5()
        {
            string code = @"
class A
{ 
    public x : var ;    
    private y : var ;
    //protected z : var = 0 ;
    constructor A ()
    {
               
    }
    public def foo1 (a)
    {
        x = a;
        y = a + 1;
        return = x + y;
    } 
    private def foo2 (a)
    {
        x = a;
        y = a + 1;
        return = x + y;
    }    
}
class B extends A
{
    
}
a = B.B();
a1 = a.foo1(1);
a2 = a.foo2(1);
a.x = 4;
//a.y = 5;
a3 = a.x;
a4 = a.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
            //Verification 
            thisTest.Verify("a1", 3, 0);
            thisTest.Verify("a2", v1, 0);
            thisTest.Verify("a3", 4, 0);
            thisTest.Verify("a4", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z021_Defect_1458785_6()
        {
            string code = @"
class A
{
    x : var;
	constructor A ()
	{
	    x = foo();
	}
	
	
	def foo ( i )
	{
		x = foo2();
		return = x + i;
	}
}
	
a1 = A.A();
a2 = a1.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
            //Verification 
            thisTest.Verify("a2", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z022_Defect_1455292()
        {
            string code = @"
class Geometry
{
    public constructor Create()
    {}
    
    public def Intersect : Geometry (other : Geometry)
    {
        return = null;
    }
}
class Point extends Geometry
{
    x : var;
    
    public constructor Create()
    {
        x = 100;
    }
}
class Line 
{
    public constructor Create()
    {}
    
    def Intersect : Geometry (other : Line) //This is the issue, if return a Point here, it is working
    {
        return = Point.Create();
    }
}
test = Point.Create();
a = test.x;
l1 = Line.Create();
l2 = Line.Create();
intpt = l1.Intersect(l2);
dummy = intpt.x;    // i was expecting 100 here
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Verification 
            thisTest.Verify("dummy", 100, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z022_Defect_1455292_2()
        {
            string code = @"
class f 
{ 
	x : var; 
	constructor f() 
	{ 
		x = 100; 
	} 
} 
class d extends f 
{ 
	y : var; 
	constructor d() 
	{ 
		y = 100; 
		x = 10;
	} 
} 
def foo : f() 
{ 
	return = d.d(); 
} 
p = foo(); 
xx = p.x;
yy = p.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Verification 
            thisTest.Verify("xx", 10, 0);
            thisTest.Verify("yy", 100, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z022_Defect_1455292_3()
        {
            string code = @"
class A 
{ 
	x : var; 
	constructor A() 
	{ 
		x = 1; 
	} 
} 
class B extends A 
{ 
	y : var; 
	constructor B1() 
	{ 
		y = 3; 
		x = 2;
	} 
	constructor B2() : base.A()
	{ 
		y = 4;		
	} 
} 
def foo1 : A() 
{ 
	return = B.B1(); 
} 
def foo2 : B() 
{ 
	return = B.B2(); 
}
def foo3 () 
{ 
	return = B.B2(); 
}
p1 = foo1();
p2 = foo2();
p3 = foo3();
t = [Imperative]
{
	x1 = p1.x;
	y1 = p1.y;
	x2 = p2.x;
	y2 = p2.y;
	x3 = p3.x;
	y3 = p3.y;
	return = { x1, y1, x2, y2, x3, y3 };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 3, 1, 4, 1, 4 };

            //Verification 
            thisTest.Verify("t", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z023_Defect_1455131()
        {
            string code = @"
class TestClass
    
    {
    
    X: var;
    Y: var;
    
    constructor Create(x : double, y : double)
        
        
        {
			this.X = x;
			this.Y = y;
        
        }
    
    
        def foo : double ()
        
        {
        
        return = this.X + this.Y;
        
        
        }
		
		def foo2 : double (this2: TestClass)
        
        {
        
            return = this2.X + this2.Y;        
        
        }
    
    
    }
    
    
    test = TestClass.Create (10.0, 11.0);
    test2 = TestClass.Create (1.0, 2.0);
	 
    result = test.foo();
	
	result2 = test.foo2();
	
	result3 = test.foo2(test2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;

            //Verification 
            thisTest.Verify("result", 21.0, 0);
            thisTest.Verify("result2", v1, 0);
            thisTest.Verify("result3", 3.0, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void Z023_Defect_1455131_2()
        {
            string code = @"
class TestClass
    
    {
    
    X: var;
    Y: var;
    
    constructor Create(x : double, y : double)
        
        
        {
			this.X = x;
			this.Y = y;
        
        }
    
    
        def foo : double ()
        
        {
        
            t = [Imperative]
			{
			    if(this.X > 1 )
				    return = this.X ;
				else
				    return = this.Y;
			}
             return = t;
        
        }
		
		def foo2 : double (this2: TestClass)
        
        {
            
            t = [Imperative]
			{
			    if(this2.X > 1 )
				    return = this2.X ;
				else
				    return = this2.Y;
			}
             return = t;       
        
        }
    
    
    }
    
t = [Imperative]
{
    test = TestClass.Create (10.0, 11.0);
    test2 = TestClass.Create (1.0, 2.0);
	 
    result = test.foo();
	
	result2 = test.foo2();
	
	result3 = test.foo2(test2);
	
	return = { result, result2, result3 };
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 10.0, null, 2.0 };
            //Verification 
            thisTest.Verify("t", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z024_Defect_1461133()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class A
{ 
    public x : int ;    
    constructor A ( a)
    {
        x = a;
    }
    public def foo ( this : A)
    {
        return = this.x;       
    } 
	
}
a = 2;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void Z024_Defect_1461133_2()
        {
            string code = @"
class A
{ 
    public x : int ;    
    constructor A ( a)
    {
        x = a;
    }    
	
	public def foo ( thiss : A)
    {
        return = thiss.x;       
    } 
}
a1 = A.A ( 1 );
a2 = A.A ( 2 );
test = a1.foo( a2 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("test", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z025_Defect_1459626()
        {
            string code = @"
class A
{
    X : int;	
	
	constructor A( x : int )
	{
	    X = x;			
	}
}
class B
{
    Y : A;	
	
	constructor B( y : A )
	{
	    Y = y;
	}
	
}
a = { 10, 20 };
//a1 = { A.A( 1 ), A.A( 2) } ; // If I create the array a1 like this, then the scripts works fine
a1 = [Imperative]
{    
	b2 = { 0, 0 };
	c2 = 0;
	for (i in a )
	{
	    b2[c2] = A.A( i );
		c2 = c2 + 1;		
	}
	return = b2;
}
t1 = a1[0].X;
b1 = [Imperative]
{    
	b2 = { 0, 0 };
	c2 = 0;
	for (i in a1 )
	{
	    b2[c2] = B.B( i );
		c2 = c2 + 1;		
	}
	return = b2;
}
t = b1[1].Y;
t2 = t.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("t1", 10, 0);
            thisTest.Verify("t2", 20, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z026_Defect_1458563()
        {
            string code = @"
class A
{ 
	x1 : int ;
	
	constructor A () 
	{	
		x1  = { true, 2.5 };  
		
	}
}
a = A.A();
t1 = a.x1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("t1", null);

        }

        [Test]
        [Category("Update")]
        public void Z026_Defect_1458563_2()
        {
            string code = @"
class B
{ 
	b1: var;
    constructor B (a : var)
	{
	    b1 = a;
	}
}
class A
{ 
	x1 : int = 1 ;
	x2 : bool = true;
	x3 : double = 1.0;
	x4 : B = B.B(1);	
	x5 : int[] = { 1, 2 };
	x6 : B[] = { B.B(1), B.B(2) };
	x7 : bool = false;
	
	def change()
	{
	    x4 = 1 ;
		x3 = { B.B(1), B.B(2) };
		x2 = 1.0;
		x1 = B.B(1);
		x5  = false;
		x6  = true;
		x7  = { 1, 2 };
		return = true;
	}
}
a = A.A();
t1 = a.x1;
t2 = a.x2;
t3 = a.x3;
t4 = a.x4.b1;
t5 = a.x5[1];
t6 = a.x6[1].b1;
t7 = a.x7;
test = [Imperative]
{
    return = a.change();
}
	
p1 = t1;//a1.x1;
p2 = t2;//a.x2;
p3 = t3;//a.x3;
p4 = t4;//a.x4.b1;
p5 = t5;//a.x5[1];
p6 = t6;//a.x6[1].b1;
p7 = t7;//a.x7;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2 };
            Object v2 = null;
            //Assert.Fail("1463700 - Sprint 20 : rev 2147 : Update is not happening when a collection property of a class instance is updated using a class method");
            //Verification 
            thisTest.Verify("p1", 1);
            thisTest.Verify("p2", true);
            thisTest.Verify("p3", 1.0);
            thisTest.Verify("p4", 1);
            thisTest.Verify("p5", 2);
            thisTest.Verify("p6", 2);
            thisTest.Verify("p7", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z027_Defect_1461365()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class A
{
    a : int;	
}
class B extends B // negative case
{
    b : int;
}
b1 = B.B();
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void Z028_Same_Name_Constructor_And_Method_Negative()
        {
            string code = @"
class A
{
    a : int;	
	constructor A ()
	{
	}
	def A ( )
	{
	    return = 1;
	}
}
a = A.A();
b = a.A();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z029_Calling_Class_Constructor_From_Instance_Negative()
        {
            string code = @"
class A
{
    a : int;	
	constructor A ()
	{
	    a = 1;
	}	
}
a1 = A.A();
b1 = a1.A();
c1 = b1.a;";
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
class A
{
    a : var;
	constructor A ()
	{
	    a = 1;
	}
}
A = A.A();
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
class A
{
    a : var;
	constructor A ()
	{
	    a = 1;
	}
}
A = A.A();
t = A.a;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestDefaultArgInConstructor01()
        {
            string code = @"
class Test{	x : int;	constructor Test(m:int = 1)	{		x = m;	}}p = Test.Test(2); i = p.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestDefaultArgInConstructor02()
        {
            string code = @"
class Test{	x : int;	y : int;	constructor Test(m:int, n:int = 2)	{		x = m;		y = n;	}}p = Test.Test(1); i = p.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", 2);
        }

    }
}
