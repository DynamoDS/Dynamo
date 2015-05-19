using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Associative
{
    class Update : ProtoTestBase
    {
        ProtoScript.Config.RunConfiguration runnerConfig;
        ProtoScript.Runners.DebugRunner fsr;
        string importPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        public override void Setup()
        {
            base.Setup();
           
            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper());
            CLRModuleType.ClearTypes();
        }

        public override void TearDown()
        {
            base.TearDown();
            fsr = null;
        }

        [Test]
        [Category("SmokeTest")]
        public void T001_Simple_Update()
        {
            string code = @"
a = 1;
b = a + 1;
a = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("a", 2, 0);
            thisTest.Verify("b", 3, 0);
        }

        [Test]
        [Category("Update")]
        public void T002_Update_Collection()
        {
            string code = @"
a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            //Verification
            thisTest.Verify("c", 14, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T003_Update_In_Function_Call()
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            Object[] v0 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v1 = new Object[] { 2, 1, 2, 3, 4 };
            thisTest.Verify("a", v0, 0);
            thisTest.Verify("d", v1, 0);
            thisTest.Verify("e1", v1, 0);
        }

        [Test]
        [Category("Update")]
        public void T004_Update_In_Function_Call_2()
        {
            string errmsg = "";//1467302 - rev 3778 : invalid cyclic dependency detected";
            string src = @"def foo1 ( a : int ) 
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
a[1] = a[1] + 1;";
            thisTest.VerifyRunScriptSource(src, errmsg);
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 

            //Verification
            Object[] v1 = new Object[] { 13, 12, 12, 13, 14 };
            Object[] v2 = new Object[] { 14, 13, 13, 14, 15 };
            Object[] v3 = new Object[] { 10, 12, 12, 13, 14 };
            thisTest.Verify("a", v3, 0);
            thisTest.Verify("e1", v1, 0);
            thisTest.Verify("b", v1, 0);
            thisTest.Verify("c", v2, 0);
        }

        [Test]
        [Category("Update")]
        public void T005_Update_In_collection()
        {
            string code = @"
a=1;
b=2;
c=4;
collection = {a,b,c};
collection[1] = collection[1] + 0.5;
d = collection[1];
d = d + 0.1;    // updates the result of accessing the collection - Redefinition
                // 'd' now only depends on any changes to 'd'
b = b + 0.1;    // updates the source member of the collection";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 

            //Verification
            Object[] v1 = new Object[] { 1, 2.6, 4 };
            thisTest.Verify("collection", v1, 0);
            thisTest.Verify("b", 2.1, 0);
            thisTest.Verify("d", 2.6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_Update_In_Class()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	Z : double;
	
	constructor ByCoordinates( x : double, y : double, z : double )
	{
	    X = x;
		Y = y;
		Z = z;		
	}
}
class Line
{
    P1 : Point;
	P2 : Point;
	
	constructor ByStartPointEndPoint( p1 : Point, p2 : Point )
	{
	    P1 = p1;
		P2 = p2;
	}
	
	def PointAtParameter (p : double )
	{
	
	    t1 = P1.X + ( p * (P2.X - P1.X) );
		return = Point.ByCoordinates( t1, P1.Y, P1.Z);
	    
	}
	
}
startPt = Point.ByCoordinates(1, 1, 0);
endPt   = Point.ByCoordinates(1, 5, 0);
line_0  = Line.ByStartPointEndPoint(startPt, endPt); 	// create line_0
//startPt = Point.ByCartesianCoordinates((1..5..1), 1, 0); // with range expression
//endPt   = Point.ByCartesianCoordinates((1..5..1), 5, 0); // with range expression.. but line does not replicate
//line_0  = Line.ByStartPointEndPoint(startPt<1>, endPt<2>); 	// add replication guides <1> <2>
startPt2 = [Imperative]
{
    x2 = 1..5..1;
	p2 = 0..0..#5;
	c2 = 0;
	for (i in x2 )
	{
	    p2[c2] = Point.ByCoordinates(i, 1, 0);		
		c2 = c2 + 1;
	}
	return = p2;
}
endPt2 = [Imperative]
{
    x2 = 11..15..1;
	p2 = 0..0..#5;
	c2 = 0;
	for (i in x2 )
	{
	    p2[c2] = Point.ByCoordinates(i, 5, 0);		
		c2 = c2 + 1;
	}
	return = p2;
}
line_0 = [Imperative]
{    
	p2 = 0..0..#25;
	c2 = 0;
	for (i in startPt2 )
	{
	    for ( j in endPt2 )
		{
		    p2[c2] = Line.ByStartPointEndPoint(i, j);
			c2 = c2 + 1;
		}
			
	}
	return = p2;
}
x1_start = line_0[0].P1.X;
x1_end = line_0[0].P2.X;
x5_start = line_0[4].P1.X;
x5_end = line_0[4].P2.X;
//line_0  = Line.ByStartPointEndPoint(startPt, endPt); 		// remove replication guides
line_0 = [Imperative]
{    
	p2 = 0..0..#5;
	c2 = 0;
	for (i in startPt2 )
	{
	    p2[c2] = Line.ByStartPointEndPoint(startPt2[c2], endPt2[c2]);
		c2 = c2 + 1;
			
	}
	return = p2;
}
//startPt = Point.ByCoordinates(1, 1, 0); // go back to single line
//endPt   = Point.ByCoordinates(1, 5, 0);
//line_0  = Line.ByStartPointEndPoint(startPt, endPt); 	// create line_0 as a singleton again
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification         
            thisTest.Verify("x1_start", 1.0, 0);
            thisTest.Verify("x1_end", 11.0, 0);
            thisTest.Verify("x5_start", 5.0, 0);
            thisTest.Verify("x5_end", 15.0, 0);
        }

        [Test]
        [Category("Update")]
        public void T007_Update_In_Class()
        {
            string error = "1460783 - Sprint 18 : Rev 1661 : Forward referencing is not being allowed in class property ( related to Update issue )";
            string src = @"def sin ( a : double ) = 0.5 * a;
def cos ( a : double ) = 0.5 * a;
def atan ( a : double ) = 0.5 * a;
def sqrt ( a : double ) = 0.5 * a;
class Point
{
    X : double;
	Y : double;
	Z : double;
	
	public constructor ByCartesianCoordinates( xValue : double , yValue : double, zValue : double )
    {
		X = xValue; 			
		Y = yValue;
		Z = zValue;
	}
}
class MyPoint 
{
	// define general system of dependencies
	
	x : double = radius * cos(theta*180/180); // x dependent on theta and radius
	y : double = radius * sin(theta*180/180); // y dependent on theta and radius
	z : double = 0.0;
											
	theta :double = 3.0;//atan(y/x) * 180 / 180;		 	 // theta  dependent on x and y
	radius :double = 4.0;//sqrt(x*x + y*y);				 // radius dependent on x and y
	
	inner  : Point = Point.ByCartesianCoordinates(x,y,z);	 // create inner point dependent on x and y
	
    public constructor ByXYcoordinates(xValue : double , yValue : double)
    {
		x = xValue; 			// assigning argument values to specific properties
		y = yValue; 			// overrides defaut graph and triggers remianing depenencies
								// we don't need to add in the statemenst to recompute theta and radius
								// this will happen 'automatically', because of the dependencies
								// defined in the body of the class
	}
	
	public constructor ByAngleRadius(thetaValue : double , radiusValue : double)
    {
		theta  = thetaValue;	// assigning argument values to specific properties
		radius = radiusValue; 	// overrides defaut graph and triggers remaining depenencies								// we don't need to add in the statemenst to recompute theta and radius
								// we don't need to add in the statemenst to recompute x and y
								// this will happen 'automatically', because of the dependencies
								// defined in the body of the class
	}
	
	// add 'incremental' modifiers
	
	def incrementX(mp : MyPoint, xValue : double) = ByXYcoordinates(mp.x + xValue, mp.y);
	def incrementY(mp : MyPoint, yValue : double) = ByXYcoordinates(mp.x,mp.y + yValue);
	def incrementTheta(mp : MyPoint, thetaValue : double)  = ByAngleRadius(mp.theta + thetaValue, mp.radius );
	def incrementRadius(mp : MyPoint, radiusValue : double)= ByAngleRadius(mp.theta, mp.radius + radiusValue );
}
a 		= MyPoint.ByXYcoordinates(1.0, 1.0);			// create an instance 'a' using one constructor
origin  = Point.ByCartesianCoordinates(WCS, 0,0,0);  				// create a reference point
testLine= Line.ByStartPointEndPoint(origin, a.inner);	// create a testLine (to see some results)
aX 		= a.x;											// report the properties of 'a'
aY 		= a.y;
aTheta 	= a.theta;
aRadius = a.radius;
a 		= MyPoint.ByAngleRadius(60.0, 1.0);				// switch to a different constructor [POINT updates]
//a		= a.visible(false); 
a 		= a.incrementX(0.2);							// apply different modifiers [POINT does not updates]
//a		= a.visible(false);
a 		= a.incrementY(-0.2);							// apply different modifiers [POINT does not updates]
a 		= MyPoint.ByAngleRadius(45.0, 0.75);			// redefine a (by using a constructor) [POINT updates]
//a		= a.visible(false);
a 		= a.incrementTheta(10.0);						// apply different modifiers [POINT does not updates]
//a		= a.visible(false);
a 		= a.incrementRadius(0.2); 						// [POINT does not updates]
";
            thisTest.VerifyRunScriptSource(src, error);
            //Verification
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_Update_Of_Variables()
        {
            string code = @"
class A 
{
    a : var;
	constructor A ( )
	{
	    a = 5;
	}
}
a = 1;
b = a + 1;
a = 2;
t1 = { 1, 2 };
t2 = t1 [0] + 1;
t1 = 5.5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("b", 3, 0);
            Assert.IsTrue(mirror.GetValue("t2").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_Update_Of_Undefined_Variables()
        {
            string code = @"
u1 = u2;
u2 = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("u1", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_Update_Of_Singleton_To_Collection()
        {
            string code = @"
s1 = 3;
s2 = s1 -1;
s1 = { 3, 4 } ;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            Object[] v1 = new Object[] { 2, 3 };
            thisTest.Verify("s2", v1, 0);
        }

        [Test]
        [Category("Update")]
        public void T011_Update_Of_Variable_To_Null()
        {
            string src = @"x = 1;
y = 2/x;
x = 0;
v1 = 2;
v2 = v1 * 3;
v1 = null;";
            thisTest.VerifyRunScriptSource(src);
            TestFrameWork.AssertInfinity("y");
            thisTest.Verify("v2", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_Update_Of_Variables_To_Bool()
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification         
            Assert.IsTrue(mirror.GetValue("p2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("q2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("s2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("t2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("r2").DsasmValue.IsNull);

        }

        [Test]
        [Category("SmokeTest")]
        public void T013_Update_Of_Variables_To_User_Defined_Class()
        {
            string code = @"
class A 
{
    a : var;
	constructor A ( )
	{
	    a = 5;
	}
}
r1 = 2.0;
r2 = r1+1;
r1 = A.A();
t1 = { 1, 2 };
t2 = t1 [0] + 1;
t1 = A.A();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification      
            Assert.IsTrue(mirror.GetValue("t2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("r2").DsasmValue.IsNull);

        }

        [Test]
        [Category("SmokeTest")]
        public void T014_Update_Of_Class_Properties()
        {
            string code = @"
class A 
{
    a : var;
	constructor A ( x)
	{
	    a = x;
	}
}
x = 3;
a1 = A.A(x);
b1 = a1.a;
x = 4;
c1 = b1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification      
            thisTest.Verify("c1", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_Update_Of_Class_Properties()
        {
            string code = @"
class A 
{
    a : int[];
	constructor A ( x : int[])
	{
	    a = x;
	}
}
x = { 3, 4 } ;
a1 = A.A(x);
b1 = a1.a;
x[0] = x [0] + 1;
c1 = b1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("c1", v1, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T016_Update_Of_Variable_Types()
        {
            string code = @"
class A 
{
    a : int;
	constructor A ( x : int)
	{
	    a = x;
	}
}
x = { 3, 4 } ;
y = x[0] + 1;
x =  { 3.5, 4.5 } ;
x =  { A.A(1).a, A.A(2).a } ;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("y", 2, 0);
        }

        [Test]
        [Category("Update")]
        public void T017_Update_Of_Class_Instances()
        {
            string src = @"class Point
{
    X : double;
		
    public constructor ByCoordinates( xValue : double  )
    {
	X = xValue; 			
    }
}
class Line
{
    P1 : Point;
    P2 : Point;
		
    public constructor ByStartPointEndPoint( p1 : Point, p2:Point  )
    {
	P1 = p1;
	P2 = p2;		
    }
}
class MyPoint 
{
    // define general system of dependencies
    x : double = 1; 
    y : double = 2;	
    inner  : Point = Point.ByCoordinates(0);	 // create inner point dependent on x and y
	
    public constructor ByXYcoordinates(xValue : double )
    {
	x = xValue; 			
	inner = Point.ByCoordinates(x);
    }
	
    public constructor ByAngleRadius(y1 : double)
    {
	y = y1;	
	inner = Point.ByCoordinates(y);
    }
	
    // add 'incremental' modifiers
    def incrementX(xValue : double) = ByXYcoordinates(x + xValue);
    def incrementY(yValue : double) = ByAngleRadius(y + yValue);	
}
a 	 = MyPoint.ByXYcoordinates(1.0);                // create an instance 'a' using one constructor
origin   = Point.ByCoordinates(0);  	                // create a reference point
testLine = Line.ByStartPointEndPoint(origin, a.inner);	// create a testLine (to see some results)
aX 	 = a.x;						// report the properties of 'a'
aY 	 = a.y;
aP 	 = a.inner;
// test update
a 	 = MyPoint.ByAngleRadius(2.5);		      	    
a 	 = a.incrementX(3.0);							   
a 	 = MyPoint.ByAngleRadius(5.0);					        
";
            thisTest.RunScriptSource(src);
            //Assert.Fail("1467219 - sprint25: rev 3339 : False cyclic dependency with class update");
            thisTest.Verify("aX", 1.0);
            thisTest.Verify("aY", 5.0);

            /*
            //ExecutionMirror mirror = thisTest.RunScript(testCasePath, "T017_Update_Of_Class_Instances.ds");
            string src =
                Path.GetFullPath(string.Format("{0}{1}", testCasePath, "T017_Update_Of_Class_Instances.ds")); 
            
            fsr.LoadAndPreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp1 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 25,
                LineNo = 56,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            ProtoCore.CodeModel.CodePoint cp2 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 25,
                LineNo = 57,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            ProtoCore.CodeModel.CodePoint cp3 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 25,
                LineNo = 58,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp1);
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "aX", 1.0);
            thisTest.DebugModeVerification(vms.mirror, "aY", 2);
            fsr.ToggleBreakpoint(cp2);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "aX", 1);
            thisTest.DebugModeVerification(vms.mirror, "aY", 2.0);
            fsr.ToggleBreakpoint(cp3);
            fsr.Run();
          
            thisTest.DebugModeVerification(vms.mirror, "aX", 4.0);
            thisTest.DebugModeVerification(vms.mirror, "aY", 2);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "aX", 1);
            thisTest.DebugModeVerification(vms.mirror, "aY", 5.0);
            */
        }

        [Test]
        [Category("Update")]
        public void T018_Update_Inside_Class_Constructor()
        {
            // Assert.Fail("1467220 - sprint25: rev 1399 : Update of class properties inside class methods is not happening"); 
            string src = @"class Point
{
    X : double;
    Y : double;
    
    public constructor ByXCoordinates( xValue : double  )
    {
	X = xValue;
	Y = X + 1;
	X = X + 1;        	
    }
    
    def addandIncr()
    {
        X = X + 1;
	Y = X + 1;
	X = X + 1;
	return = X + Y;
    }
}
p1 = Point.ByXCoordinates( 1 );
x1 = p1.X;
y1 = p1.Y;
z = p1.addandIncr();
";
            thisTest.RunScriptSource(src);
            //Verification   
            thisTest.Verify("z", 9.0, 0);
        }

        [Test]
        [Category("Update")]
        public void T018_Update_Inside_Class_Constructor_2()
        {
            string errmesg = "1467220 - sprint25: rev 1399 : Update of class properties inside class methods is not happening";
            string src = @"class Point
{
    X : double;
    Y  =  X + 1;
    
    public constructor ByXCoordinates( xValue : double  )
    {
	X = xValue;
	X = X + 1;        	
    }
    
    def addandIncr()
    {
        X = X + 1;
	X = X + 1;
	return = X + Y;
    }
}
p1 = Point.ByXCoordinates( 1 );
x1 = p1.X;
y1 = p1.Y;
z = p1.addandIncr();
";
            thisTest.VerifyRunScriptSource(src, errmesg);
            //Verification   
            thisTest.Verify("z", 7.0, 0);// though i expected 9, but it is as designed as of now . 
            //Refer to  :DNL-1467220 sprint25: rev 1399 : Update of class properties inside class methods is not happening
        }

        [Test]
        [Category("Update")]
        public void T019_Update_General()
        {
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case"); 

            string code = @"
X = 1;
Y = X + 1;
X = X + 1;
X = X + 1;
//Y = X + 1;
//X  = X + 1;
test = X + Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification  
            thisTest.Verify("test", 7, 0);
        }

        [Test]
        [Category("Update")]
        public void T020_Update_Inside_Class_Constructor()
        {
            //Assert.Fail("1467072 - Sprint23 : rev 2650 : Update issue: Class properties not getting updated in class  constructor"); 

            string code = @"
class Point
{
    X : double;
    Y : double;
    
    public constructor ByXCoordinates( xValue : double  )
    {
	X = xValue;
	Y = X + 1;
	X = X + 1;        	
    }
    
    def addandIncr()
    {
        X = X + 1;
	Y = X + 1;
	X = X + 1;
	return = X + Y;
    }
}
a = 0;
p1 = Point.ByXCoordinates( a );
x1 = p1.X;
y1 = p1.Y;
z = p1.addandIncr();
a = a + 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification  
            thisTest.Verify("z", 9.0, 0);
        }

        [Test]
        [Category("Update")]
        public void T021_Update_Inside_Class_Constructor()
        {
            string err = "1467220 - sprint25: rev 1399 : Update of class properties inside class methods is not happening";
            string src = @"def foo ( x )
{
    return = x + 1;
}
class Point
{
    X : double;
    Y : double;
    
    public constructor ByXCoordinates( xValue : double  )
    {
	X = xValue;
	Y = foo (X );
	X = X + 1;        	
    }
    
    def addandIncr()
    {
        X = X + 1;
	Y = foo ( X );
	X = X + 1;
	return = X + Y;
    }
}
a = 0;
p1 = Point.ByXCoordinates( a );
x1 = p1.X;
y1 = p1.Y;
z = p1.addandIncr();
a = a + 1;
";
            thisTest.VerifyRunScriptSource(src, err);
            //Verification  
            thisTest.Verify("z", 9.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Defect_1459905()
        {
            string code = @"
class A
{
    X : int;
	constructor A(x : int)
	{
	    X = x; 
	}
}
a = A.A(1);
a = a.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("a", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Defect_1459905_2()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	def foo ( )
	{
		return = x3;
	}
	
}
class A extends B
{ 
	x1 : int ;
	
	
	constructor A(a1,a2) : base.B(a2)
	{	
		x1 = a1; 				
	}
	def foo1 ( )
	{
		return = x1;
	}	
}
a1 = A.A( 1, 2 );
a1 = a1.foo(); //works fine
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("a1", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Defect_1459905_3()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
def foo ( b1 : B )
{
    return = b1.x3;
}
b1 = B.B( 1 );
b1 = foo(b1); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("b1", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_Defect_1459905_4()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
class B2
{ 
	x3 : int ;
		
	constructor B2(a) 
	{	
		x3 = a;
	}
	
}
def foo ( b1 : B )
{
    return = b1.x3;
}
b1 = B.B( 1 );
x = b1.x3;
b1 = B2.B2( 2 );
y = x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("y", 2, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T023_Defect_1459789()
        {
            string code = @"
class MyPoint 
{
	// define general system of dependencies	
	x : double = 1; 
    y : double = 2;
    public constructor ByXYcoordinates(xValue : double )
    {
		x = xValue; 			
		y = x;
	}	
	// add 'incremental' modifiers	
	def incrementX(xValue : double) 
	{
	    return = ByXYcoordinates(x + xValue);
	}
	
}
a 		 = MyPoint.ByXYcoordinates(1.0);			        
aY 		  = a.y;
aX 	          = a.x;
// test update
a 		  = MyPoint.ByXYcoordinates(2.0);
// expected : aY = 2.0 and aX = 2.0
// recieved : aY = 1.0 and aX = 1.0
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("aY", 2.0, 0);
            thisTest.Verify("aX", 2.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_Defect_1459789_2()
        {
            string code = @"
class MyPoint 
{
    // define general system of dependencies	
    x : double = 1; 
    y : double = 2;
    public constructor ByXYcoordinates(xValue : double )
    {
	x = xValue; 			
	y = x;
    }	
    // add 'incremental' modifiers	
    def incrementX(xValue : double) 
    {
	return = ByXYcoordinates(x + xValue);
    }	
}
a 		  = MyPoint.ByXYcoordinates(1.0);			        
aY 		  = a.y;
aX 	          = a.x;
// test update     	    
a		  = a.incrementX(3.0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("aY", 4.0, 0);
            thisTest.Verify("aX", 4.0, 0);
        }

        [Test]
        public void T023_Defect_1459789_3()
        {
            string code = @"
class A 
{    
    a1: var;    
    a2 : double[];    
    constructor A ( a: int, b : double[] )    
    {        
        a1 = a;    
	a2 = b;    
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( a: int, b : double[], c : bool, d : int ) : base.A ( a, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
b1 = B.B ( 1, {1.0, 2.0}, true, 1 );
test1 = b1.a2[0];
b1.a2[0] = b1.a2[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value is not reflected");
            //Verification   
            thisTest.Verify("test1", 2.0, 0);
        }

        [Test]
        public void T023_Defect_1459789_4()
        {
            string code = @"
class A 
{    
    a1: var;    
    a2 : double[]..[];    
    constructor A ( a: int, b : double[]..[] )    
    {        
        a1 = a;    
	a2 = b;    
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( a: int, b : double[]..[], c : bool, d : int ) : base.A ( a, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
a = { { 1.0, 2} , 3 };
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1.0, 2.0 };
            //Verification   
            thisTest.Verify("test1", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_Defect_1459789_5()
        {
            string code = @"
class A 
{    
    a1: var;    
    a2 : double[]..[];    
    constructor A ( a: int, b : double[]..[] )    
    {        
        a1 = a;    
	a2 = b;    
    }
    def create ()
    {
        temp1 = A.A ( 2, { 0.0, 0.0 } );
	return = temp1;
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( a: int, b : double[]..[], c : bool, d : int ) : base.A ( a, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
a = { { 1.0, 2} , 3 };
b1 = b1.create();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Verification   
            thisTest.Verify("test1", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_Defect_1459789_6()
        {
            string code = @"
class A 
{    
    a1: var;    
    a2 : double[]..[];    
    constructor A ( a: int, b : double[]..[] )    
    {        
        a1 = a;    
	a2 = b;    
    }
    def create ()
    {
        temp1 = [Imperative]
	{
	    return = A.A ( 2, { 0.0, 0.0 } );
	}
	return = temp1;
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( a: int, b : double[]..[], c : bool, d : int ) : base.A ( a, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
a = { { 1.0, 2} , 3 };
b1 = b1.create();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("test1", 0.0);
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T023_Defect_1459789_7()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1506
            string err = "MAGN-1506: Update issue : When a class instance is updated from imperative scope the update is not happening as expected in some cases";
            string src = @"class A 
{    
    a1: var;    
    a2 : double[];    
    constructor A ( ax: int, b : double[] )    
    {        
        a1 = ax;    
	a2 = b;    
    }
    def create ()
    {
        temp1 = [Imperative]
	{
	    return = A.A ( 2, { 0.0, 0.0 } );
	}
	return = temp1;
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( ax: int, b : double[], c : bool, d : int ) : base.A ( ax, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
[Imperative]
{
        a = {  3.0, 2 };
	b1 = b1.create();
}
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, err);

            //Verification   
            thisTest.Verify("test1", 0.0);
        }

        [Test]
        [Category("Update")]
        public void T023_Defect_1459789_8()
        {
            string code = @"
class A 
{    
    a1: var;    
    a2 : double[];    
    constructor A ( ax: int, b : double[] )    
    {        
        a1 = ax;    
	a2 = b;    
    }
    def create ()
    {
        temp1 = [Imperative]
	{
	    return = A.A ( 2, { 0.0, 0.0 } );
	}
	return = temp1;
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( ax: int, b : double[], c : bool, d : int ) : base.A ( ax, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
def foo ( x : B )
{
    return = x.create();
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
[Imperative]
{
	a = { { 1.0, 2} , 3 };
        b1 = foo ( b1 );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467116 Sprint24 : rev 2806 : Cross language update issue");
            //Verification   
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kCyclicDependency);
        }

        [Test]
        [Category("Update")]
        public void T023_Defect_1459789_9()
        {
            string err = "";//1466768 - VALIDATION NEEDED - Sprint 23 : rev 2479 : global variables are not getting updated through function calls";
            string src = @"class A 
{    
    a1: var;    
    a2 : double[];    
    constructor A ( ax: int, b : double[] )    
    {        
        a1 = ax;    
	a2 = b;    
    }
    def create ()
    {
        temp1 = [Imperative]
	{
	    return = A.A ( 2, { 0.0, 0.0 } );
	}
	return = temp1;
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( ax: int, b : double[], c : bool, d : int ) : base.A ( ax, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
def foo (  )
{
    b1 = b1.create();
    return = null;
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
dummy = foo (  );
";
            thisTest.VerifyRunScriptSource(src, err);
            //Verification   
            thisTest.Verify("test1", 0.0);
        }

        [Test]
        public void T023_Defect_1459789_10()
        {
            string err = "1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases";
            string src = @"class A 
{    
    a1: var;    
    a2 : double[];    
    constructor A ( ax: int, b : double[] )    
    {        
        a1 = ax;    
	a2 = b;    
    }
    def create ()
    {
        temp1 = [Imperative]
	{
	    return = A.A ( 2, { 0.0, 0.0 } );
	}
	return = temp1;
    }
}
class B extends A
{    
    b1  :bool;    
    b2: int;    
    constructor B ( ax: int, b : double[], c : bool, d : int ) : base.A ( ax, b )    
    {        
        b1 = c;    
	b2 = d;    
    }
}
a = { 1.0, 2.0 };
b1 = B.B ( 1, a, true, 1 );
test1 = b1.a2[0];
a = { { 2.0, 2.0 } => a1;
      a1[0] + 1 => a2;
      { a2, a2 } ;
    }
";
            thisTest.VerifyRunScriptSource(src, err);

            //Verification   
            thisTest.Verify("test1", 3.0);
        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470()
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            thisTest.Verify("c", 14);

        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_2()
        {
            string errmsg = "";//DNL-1467335 Rev 3971 : REGRESSION : Update not happening inside class constructor";
            string src = @"class A
{
	a : var;
	b : var;
	c : var;
	constructor A ()
	{
		a = 0;
		b = a;
		c = b + 1;
		a = 1;	
	}
}
x = A.A();
a1 = x.a;
b1 = x.b;
c1 = x.c;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            //Verification   
            thisTest.Verify("c1", 2);
        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_3()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4020
            string errmsg = "MAGN-4020 Update of global variables from inside function call is not happening as expected";
            string src = @"def foo ()
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
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            //Verification   
            thisTest.Verify("c", 14);
        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_4()
        {
            string errmsg = "";//DNL-1467337 Rev 3971 : Update of global variables from inside function call is not happening as expectedr";
            string src = @"a = {1,2,3,4};
b = a;
c = b[2];
d = a[2];
a[0..1] = {1, 2};
b[2..3] = 5;
	
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            //Verification   
            thisTest.Verify("c", 5);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("Update")]
        public void T025_Defect_1459704()
        {
            string err = "1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases";
            string src = @"a = b;
b = 3;
c = a;
					        
";
            thisTest.VerifyRunScriptSource(src, err);

            //Verification   
            thisTest.Verify("a", 3, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("Update")]
        public void T025_Defect_1459704_2()
        {
            string code = @"
class A 
{
    a : int;
    b : int;
    constructor A ( a1:int)
    {
        a = b + 1;
	b = a1;
    }
}
def foo ( a1 : int)
{
    b = 0;	
    a = b + 1;
    b = a1;
    return  = a ;
}
p = A.A(1);
a1 = p.a;
a2 = foo(10);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467072 - Sprint23 : rev 2650 : Update issue: Class properties not getting updated in class  constructor");
            //Verification   
            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 11, 0);
        }

        [Test]
        [Category("Update")]
        public void T026_Defect_1459631()
        {
            string err = "1460783 - Sprint 18 : Rev 1661 : Forward referencing is not being allowed in class property ( related to Update issue )";
            string src = @"class A
{
    x : int = 1;
	y : int = x + 1;	
	constructor A ()
	{
	    x = 2;		
	}
}
a1 = A.A();
t1 = a1.x;
t2 = a1.y;
					        
";
            thisTest.VerifyRunScriptSource(src, err);
            //Verification   
            thisTest.Verify("t1", 2, 0);
            thisTest.Verify("t2", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_Defect_1459631_2()
        {
            string code = @"
def foo (a ) = a * 2;
class A
{
        x : int = foo ( 1 ) ;
	y : int = x + foo ( x) ;
        z : int = x + y;	
	w :int = 1;
	constructor A ()
	{
	    w = 4;		
	}
}
a1 = A.A();
t1 = a1.x;
t2 = a1.y;
t3 = a1.z;
t4 = a1.w;
					        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("t1", 2, 0);
            thisTest.Verify("t2", 6, 0);
            thisTest.Verify("t3", 8, 0);
            thisTest.Verify("t4", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_Defect_1459631_3()
        {
            string code = @"
def foo (a ) = a * 2;
def foo2 (a ) 
{
    b = { a, a, a};
    return = b;
        
}
class A
{
        x : int   = foo ( 1 ) ;
	y : int   = x + foo ( x ) ;
        z : int[] = foo2 ( x + y );	
	w : int   = z[0];
	constructor A ()
	{
	    //w = 4;		
	}
}
a1 = A.A();
t1 = a1.x;
t2 = a1.y;
t3 = a1.z;
t4 = a1.w;
					        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 8, 8, 8 };
            //Verification   
            thisTest.Verify("t1", 2, 0);
            thisTest.Verify("t2", 6, 0);
            thisTest.Verify("t3", v1, 0);
            thisTest.Verify("t4", 8, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T027_Defect_1460741()
        {
            string code = @"
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
x2;
x3 = [Imperative]
{
    a2 = B.B();
    x2 = a2.y;
    return = x2;
}
					        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3, 4 };
            //Verification   
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v1);
            thisTest.Verify("x3", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T027_Defect_1460741_2()
        {
            string code = @"
class C
{ 
	c1 : double[];
	
	constructor C ()
	{
	    c1 = {1.0,2.0};            	    
	}	
}
class A
{ 
	y : int[];
	x : C;
	
	constructor A ()
	{
	    y={1,2};
            x = false;   
	}	
}
class B extends A
{
	constructor B()
	{
	    y={3,4};   
	    x = C.C();	    
	}
}
a1 = B.B();
x1 = a1.y;
x2 = a1.x.c1;
x4;
x3 = [Imperative]
{
    x4 = a1.x.c1;
    return = x4;
}
					        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3, 4 };
            Object[] v2 = new Object[] { 1.0, 2.0 };
            //Verification   
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v2);
            thisTest.Verify("x3", v2);
            thisTest.Verify("x4", v2);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T028_Modifier_Stack_Simple()
        {
            string code = @"
a = {
     2 ;
    +4;
    +3;                                
} //expected : a = 9; received : 13
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 9);
        }

        [Test]
        [Category("Update")]
        public void T029_Defect_1460139_Update_In_Class()
        {
            string code = @"
X = 1;
Y = X + 1;
X = X + 1;
X = X + 1; // this line causing the problem
test = X + Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case");

            thisTest.Verify("test", 7);
        }

        [Test]
        [Category("Update")]
        public void T030_Defect_1467236_Update_In_Class()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(tmp : Child) // definition with inherited class
{
tmp.A = tmp.A +1;
tmp.B = tmp.B +1;
tmp.C = tmp.C +1;
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
test1 = modify( oldPoint ); // call function with object of parent class
test2 = modify( derivedpoint );
x1 = oldPoint.A;
x2 = derivedpoint.B;
//expected : x2 = 9
//received : cyclic dependency 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 9);
        }

        [Test]
        [Category("Update")]
        public void T030_Defect_1467236_Update_In_Class_2()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
	constructor Create( x:int, y:int, z:int )
	{
		A = x;
		B = y;
		C = z;
	}
}
class Child extends Parent
{
	constructor Create( x:int, y:int, z:int )
	{
		[Imperative]
		{
			A = x;
			B = y;
			C = z;
		}
	}
}
def modify(tmp : Child) // definition with inherited class
{
	[Imperative]
	{
		tmp.A = tmp.A +1;
		tmp.B = tmp.B +1;
		tmp.C = tmp.C +1;
		return=true;
	}
	return = true;
}
x1;
x2;
[Imperative]
{
	oldPoint = Parent.Create( 1, 2, 3 );
	derivedpoint = Child.Create( 7,8,9 );
	test1 = modify( oldPoint ); // call function with object of parent class
	test2 = modify( derivedpoint );
	x1 = oldPoint.A;
	x2 = derivedpoint.B;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 9);
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302()
        {
            String code =
@"
def foo ( a : int[] ) 
{
    b = a;
    b[0] = b[1] + 1;
    return = b;
}
a = { 0, 1, 2};
e1 = foo(a);
a = { 1, 2};
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { 3, 2 });
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302_2()
        {
            String code =
@"
def foo ( a : int[] ) 
{
    b = a;
    b[0] = b[1] + 1;
    return = b;
}
i = 1..2;
a = { 0, 1, 2, 3};
e1 = foo(a[i]);
i = 0..2;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { 2, 1, 2 });
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302_3()
        {
            String code =
@"
class A
{
    b : int[] = { 0, 1, 2, 3 };
    
    def foo (i:int ) 
    {
        b[i] = b[i] + 1;
        return = b;
    }
}
i = 1..2;
e1 = A.A().foo(i);
i = 0..2;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg); 
            
            //
            // SSA will transform 
            //      e1 = A.A().foo(i);
            //
            //      to
            //
            //      t0 = A.A()
            //      t1 = i
            //      t2 = t0.foo(t1)
            //
            // This means that the initial value of 'b' will be preserved as class A will not be re-initialed after the update
            //
            thisTest.Verify("e1", new Object[] { new Object[] { 1, 2, 3, 3 }, new Object[] { 1, 3, 3, 3 }, new Object[] { 1, 3, 4, 3 } });
        }

        [Test]
        [Category("Update")]
        public void T032_Defect_1467335_Update_In_class_Constructor()
        {
            String code =
@"
class A
{
    b : int[] = { 0, 1, 2, 3 };
    
    def foo (i:int ) 
    {
        b[i] = b[i] + 1;
        return = b;
    }
}
i = 1..2;
e1 = A.A().foo(i);
i = 0..2;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            //
            // SSA will transform 
            //      e1 = A.A().foo(i);
            //
            //      to
            //
            //      t0 = A.A()
            //      t1 = i
            //      t2 = t0.foo(t1)
            //
            // This means that the initial value of 'b' will be preserved as class A will not be re-initialed after the update
            //
            thisTest.Verify("e1", new Object[] { new Object[] { 1, 2, 3, 3 }, new Object[] { 1, 3, 3, 3 }, new Object[] { 1, 3, 4, 3 } });
        }

        [Test]
        [Category("Update")]
        public void T033_Defect_1467187_Update_In_class_collection_property()
        {
            String code =
@"
class Point
{
    X : double;
    
    constructor ByCoordinates( x : double )
    {
        X = x;
    
    } 
def foo ( y )
{
    [Imperative]
    {
        X = y + 1;
    }
    return = true;
}    
    
}
p1 = Point.ByCoordinates(1);
test = p1.X;
dummy = p1.foo(2);
//expected test to update to '3'
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", 3.0);
        }

        [Test]
        [Category("Update")]
        public void T033_Defect_1467187_Update_In_class_collection_property_2()
        {
            String code =
@"
class B
{    
    a1 : int;
    a2 : double[];
    constructor B (a:int, b : double[])    
    {        
        a1 = a;
        a2 = b;
    }
}
b1 = B.B ( 1, {1.0, 2.0} );
test1 = b1.a2[0];
b1.a2[0] = b1.a2[1];
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", 2.0);
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T033_Defect_1467187_Update_In_class_collection_property_3()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4021
            String code =
@"
class Point
{
    X : double[];
    
    constructor ByCoordinates( x : double[] )
    {
        X = x;
    
    } 
    def foo ( y :double[])
    {
        X = y + 1;
        [Imperative]
        {
           count = 0;
           for (i in y)
           {
               X[count] = y[count] + X[count];
               count = count + 1;
           }
        }
        return = true;
    }    
    
}
p1 = Point.ByCoordinates({0,0,0,0});
test = p1.X;
dummy = p1.foo({1,1,1,1});
p1.X[0..1] = -1;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "MAGN-4021 Update issue with collection property in nested imperative scope";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { -1, -1, 2, 2 });
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T034_UpdaetStaticProperty()
        {
            string code = @"
class Base
{
    static x : int[];
}
b = Base.Base();
t = b.x;               // x is a static property
Base.x = { 5.2, 3.9 }; // expect t = {5, 4}, but t = {null}
";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1271
            string errmsg = "MAGN-1271 Modify static property doesn't trigger update";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t", new object[] { 5, 4 });
        }

        [Test]
        [Category("Update")]
        public void T035_FalseCyclicDependency()
        {
            string code = @"
def foo()
{
    a = b;
    return = null;
}
def bar()
{
    b = a;
    return = null;
}
a = 1;
b = 0;
r = bar();
q = a;
";
            string errmsg = "DNL-1467336 Rev 3971 :global and local scope identifiers of same name causing cyclic dependency issue";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("q", 1);
        }

        [Test]
        [Category("Update")]
        public void T036_Defect_1467491()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4033
            string errmsg = "MAGN-4033 Update issue with object redefinition";
            string code = @"import(""T031_Defect_1467491_ImportUpdate_Sub.ds"");
t = 5;
z = a.x;    // This is a redefinition test where 'a' was redefined in the imported file
";
            thisTest.RunScriptSource(code, errmsg, importPath);
            thisTest.Verify("z", 3);

        }

        [Test]
        public void TestCyclicDependency01()
        {
            string code = @"
b = 1;
a = b + 1;
b = a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
            Object n1 = null;
            thisTest.Verify("a", n1);
            thisTest.Verify("b", n1);
        }

        [Test]
        public void TestCyclicDependency02()
        {

            string code = @"


a1;
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
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
            Object n1 = null;
            thisTest.Verify("a1", n1);
        }

        [Test]
        public void TestStringIndexing()
        {

            string code = @"
a = {};
x = 1;
y = 2;
a[""x""] = x;
a[""y""] = y;
x = 3;
y = 4;
r1 = a[""x""];
r2 = a[""y""];
";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 3);
            thisTest.Verify("r2", 4);
        }
    }
}
