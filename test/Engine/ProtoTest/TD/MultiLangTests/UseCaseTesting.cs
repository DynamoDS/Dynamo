using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class UseCaseTesting : ProtoTestBase
    {
        string testPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        [Category("SmokeTest")]
        public void T001_implicit_programming_Robert()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
// no paradigm specified, so assume associative
// some associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some imperative code ....
if (a>10) 	// implicit switch to imperative paradigm
{
	c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
	b=b/2;	// is executed before the statement on line 14 [as would be expected]
}
else
{
	[Associative] 	// explicit switch to associative paradigm [overrides the imperative paradigm]
	{
		c = b;    	// c references the final state of b, therefore [because we are in an associative paradigm] 
		b = b*2;	// the statement on line 21 is executed before the statement on line 20
	}
}
// some more associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a imperative block is nested within an associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the associative graph before the imperative block
			//			the imperative block
			//			the part of the associative graph after the imperative block
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                thisTest.Verify("a", 13);
                thisTest.Verify("b", 26);
                thisTest.Verify("c", 22);
            });
        }

        [Test]

        public void T001_implicit_programming_Robert_2()
        {
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case ");

            string code = @"
// no paradigm specified, so assume associative
// some associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some imperative code ....
[Imperative]
{
	if (a>10) 	// explicit switch to imperative paradigm
	{
		c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
		b=b/2;	// is executed before the statement on line 14 [as would be expected]
	}
	else
	{
		[Associative] 	// explicit switch to associative paradigm [overrides the imperative paradigm]
		{
			c = b;    	// c references the final state of b, therefore [because we are in an associative paradigm] 
			b = b*2;	// the statement on line 21 is executed before the statement on line 20
		}
	}
}
// some more associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a imperative block is nested within an associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the associative graph before the imperative block
			//			the imperative block
			//			the part of the associative graph after the imperative block
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 13);
            thisTest.Verify("b", 13.0);
            thisTest.Verify("c", 26);

        }

        [Test]
        [Category("Replication")]
        public void T002_limits_to_replication_1_Robert()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections ");

            string code = @"
a = 0..10..2; 
b = a>5? 0:1; 
[Imperative]
{
	c = a * 2; // replication within an imperative block [OK?]
	d = a > 5 ? 0:1; // in-line conditional.. operates on a collection [inside an imperative block, OK?]
	if( c[2] > 4 ) x = 10; // if statement evaluates a single term [OK]
	
	if( c > 4 ) // but... replication within a regular 'if..else' any support for this?
	{
		y = 1;
	}
	else
	{
		y = -1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1, 1, 0, 0, 0 };
            thisTest.Verify("b", v1);
        }


        [Test]
        [Category("SmokeTest")]
        public void T004_simple_order_1_Robert()
        {
            string code = @"
a1 = 10;        // =1
b1 = 20;        // =1
a2 = a1 + b1;   // =3
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5
a  = a2 + b;    // 6";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 10);
            thisTest.Verify("b1", 20);
            thisTest.Verify("a2", 30);
            thisTest.Verify("b2", 50);
            thisTest.Verify("b", 52);
            thisTest.Verify("a", 82);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T005_modifiers_with_right_assignments_Robert()
        {
            string code = @"
a = 
    {
        10     => @a1 ;  // =1
        + @b1  => @a2;   // =3
        + b ;            // 6 
    }            
    
b = 
    {
        20     => @b1;   // =1
        + @a2  => @b2 ;  // =3
        + 2 ;            // 5
    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 82);
            thisTest.Verify("b", 52);
            thisTest.Verify("@a2", 30);
            thisTest.Verify("@b1", 20);
            thisTest.Verify("@b2", 50);
            thisTest.Verify("@a1", 10);
      
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_grouped_1_Robert()
        {
            string code = @"
a1 = 10;        // =1
a2 = a1 + b1;   // =3
a  = a2 + b;    // 6    
    
b1 = 20;        // =1
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 82);
            thisTest.Verify("b", 52);
            thisTest.Verify("a2", 30);
            thisTest.Verify("b1", 20);
            thisTest.Verify("a1", 10);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T007_surface_trimmed_with_modifier_and_named_states_Robert()
        {

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1578
            string code = @"
class BSplineSurface
{
    x : double;
	constructor ByControlVertices( a : double)
	{
	    x = a;
	}
	def Trim ( a1 : BSplineSurface, p1 : Point )
	{
	    temp = a1.x + p1.x;
		n1 = BSplineSurface.ByControlVertices( temp);
		return = n1;
	}
	def AtParameter ( x1 : double, y1 : double )
	{
	    temp = x + x1 + y1;
		n1 = Point.ByCartesianCoordinates ( temp );
		return = n1;
	}
}
class Point
{
    x : double;
	constructor ByCartesianCoordinates( a : double)
	{
	    x = a;
	}	
}
a = 1;
b = 2;
mySurface = 
    {
        BSplineSurface.ByControlVertices ( a ) => mySurface@initial ; // built with some 2D array of points
        Trim(cuttingSurface, samplePoint) ;
    }
    
cuttingSurface = BSplineSurface.ByControlVertices ( b ); // built with another 2D array of points
samplePoint    = mySurface@initial.AtParameter( 0.5, 0.5 );
test = mySurface.x; //expected : 4
// sample points is created using the first state of mySurface [mySurface@initial]
// and then it used in creating the second (and final) state of mySurface";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Verification to be done later, when design issues in the code are sorted out
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_long_hand_surface_trim_Robert()
        {
            string code = @"
class BSplineSurface
{
    x : double;
	constructor ByControlVertices( a : double)
	{
	    x = a;
	}
	def Trim ( a1 : BSplineSurface, p1 : Point )
	{
	    temp = a1.x + p1.x;
		n1 = BSplineSurface.ByControlVertices( temp);
		return = n1;
	}
	def AtParameter ( x1 : double, y1 : double )
	{
	    temp = x + x1 + y1;
		n1 = Point.ByCartesianCoordinates ( temp );
		return = n1;
	}
}
class Point
{
    x : double;
	constructor ByCartesianCoordinates( a : double)
	{
	    x = a;
	}	
}
a = 1;
b = 2;
//initialSurface = BSplineSurface.ByControlVertices ( a ) => mySurface@initial // built with some 2D array of points
initialSurface = BSplineSurface.ByControlVertices ( a );
mySurface@initial = initialSurface;
cuttingSurface = BSplineSurface.ByControlVertices ( b );          // built with another 2D array of points
samplePoint    = mySurface@initial.AtParameter( 0.5, 0.5 );    // built using the initialSurface
trimmedSurface = initialSurface.Trim(cuttingSurface, samplePoint) ;  // now use the samplePoint in the triming
                                                                    // but create a new variable..trimmedSurface
test = trimmedSurface.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", 4.0);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T009_modifier_test_1_Robert()
        {
            string code = @"
x = {10,20};
x[0] = x[0] +1;     // this works x = {11, 20}
// now let's try the same type of construct using the modifier block syntax
y = { 
        {50, 60} ;   // initial definition
         + 1 => y@1 ;       // is this the correct syntax for modifying all members of a collection
         y@1[0] + 1 ;  // is this the correct syntax for modifying   a member  of a collection
    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 52);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_imperative_if_inside_for_loop_1_Robert()
        {
            string code = @"
x;
[Imperative]
{
	x = 0;
	
	for ( i in 1..10..2)
	{
		x = i;
		if(i>5) x = i*2; // tis is ignored
		// if(i<5) x = i*2; // this causes a crash
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 18);
        }

        [Test]
        public void T011_Cyclic_Dependency_From_Geometry()
        {
            string code = @"
import(""GeometryLibForLanguageTesting.ds"");
pt1a = Point.ByCartesianCoordinates( 0,0,0);
pt2a = Point.ByCartesianCoordinates( 5,0,0);
testBSNP = BSplineCurve.ByPoints({pt1a,pt2a});
testCurves = {testBSNP } ;//testArc, testCircle};
surfaceLine = Line.ByStartPointEndPoint(Point.ByCartesianCoordinates(-30,60,-30),Point.ByCartesianCoordinates(-30,-20,-30));
surfLength = 60;
surf = surfaceLine.ExtrudeAsSurface(surfLength,Vector.ByCoordinates(1.0, 0.0, 0.0));
projectVector = Vector.ByCoordinates(0,0,-1);
projectedCurve = testCurves.Project(surf,projectVector); //V0
test = projectedCurve.P1[0].X;
surfLength = 35; 
projectVector = Vector.ByCoordinates(5.0,0,-1);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testPath);
            //Assert.Fail("1467186 - sprint24 : rev 3172 : Cyclic dependency detected in update cases");
            thisTest.Verify("test", new Object[] { -30.0, 1.0, 5.0 });
        }

        [Test]
        public void T012_property_test_on_collections_2_Robert()
        {
            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
line1 = Line.ByStartPointEndPoint(Point.ByCartesianCoordinates(5.0 , 5.0, 0.0), Point.ByCartesianCoordinates(10.0 , 5.0, 0.0));
line2 = Line.ByStartPointEndPoint(Point.ByCartesianCoordinates(5.0 , {7.5, 10.0}, 0.0), Point.ByCartesianCoordinates(10.0 ,10.0, 0.0));
line1.Color = 0.0;
t1= line1.Color;
line2.Color = 1.0; // can't assign to a writable property if it is collection.. is this a replication issue?
t2= line2.Color;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", testPath);
            //Assert.Fail("1467186 - sprint24 : rev 3172 : Cyclic dependency detected in update cases");
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", new Object[] { 1.0, 1.0 });
        }


        [Test]
        public void T013_nested_programming_blocks_1_Robert()
        {
            string errmsg = "";
            string code = @"import (""GeometryLibForLanguageTesting.ds"");
controlPoint = Point.ByCartesianCoordinates(0, 7.5, 0);
internalLine :Line  = null; // define some variables
pointOnCurve : Point[] = null;
testLine : Line []     = null;
totalLength = 0;
i = 5;
[Imperative]
{
	while ( i <= 7 )//(totalLength < 5.0 ) 
	{
		[Associative] // within that loop build an associative model
		{
			startPoint   = Point.ByCartesianCoordinates(i, 5, 0);
			endPoint     = Point.ByCartesianCoordinates(5, 10, 0);
			internalLine = Line.ByStartPointEndPoint(startPoint, endPoint);
			pointOnCurve = internalLine.PointAtParameter(0.2..0.8..0.2);
			[Imperative] // within the associative model start some imperative scripting
			{
				for (j in 0..(Count(pointOnCurve)-1)) // iterate over the points
				{
					if(j%2==0) // consider every alternate point
					{
						pointOnCurve[j] = pointOnCurve[j].Translate(1,1,1); // actual : ( 0,0,1) modify by translation
					}
				}
			}
			// continue with more assocative modelling
			
			testLine     = Line.ByStartPointEndPoint(controlPoint, pointOnCurve);
			totalLength  = totalLength + Sum (testLine.Length);
		}
		i = i + 1; // increment i
	}
}";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg, testPath);

            thisTest.Verify("i", 8);
            thisTest.Verify("totalLength", 42.0);
        }

        [Test]
        public void T014_Robert_2012_09_14_MultipleNestedLanguage()
        {
            string code =
    @"
            
         def foo  ()
    {   
        t = [Imperative]
        {
              t1 = [Associative]
             {                    
                    t2 = 6;   
                    return = t2; 
            }     
           return = t1;                
        }
        return = t;   
    }
    def foo2  ()
    {   
        t = [Associative]
        {
              t1 = [Imperative]
             {                    
                    t2 = 6;   
                    return = t2; 
            }     
           return = t1;                
        }
        return = t;   
    }
    p1 = foo(); // expected 6, got null
    p2 = foo2();// expected 6, got 6
";
            thisTest.RunScriptSource(code,"", testPath);

            thisTest.Verify("p1", 6);
            thisTest.Verify("p2", 6);
            //thisTest.Verify("totalLength", 2.0 ); // this needs to be verified after the defect is fixed
        }
    }
}
