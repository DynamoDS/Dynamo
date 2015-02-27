using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class Tutorial : ProtoTestBase
    {
        string importPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        [Category("Replication")]
        public void T00001_Language_001_Variable_Expression_1()
        {
            string code = @"
A = 10;   	   	// assignment of single literal value
B = 2*A;   	   	// expression involving previously defined variables
A = A + 1; 	   	// expressions modifying an existing variable;
A = 15;		   	// redefine A, removing modifier
A = {1,2,3,4}; 		// redefine A as a collection
A = 1..10..2;  		// redefine A as a range expression (start..end..inc)
A = 1..10..~4; 		// redefine A as a range expression (start..end..approx_inc)
A = 1..10..#4; 		// redefine A as a range expression (start..end..no_of_incs)
A = A + 1; 		// modifying A as a range expression
A = 1..10..2;  		// redefine A as a range expression (start..end..inc)
B[1] = B[1] + 0.5; 	// modify a member of a collection [problem here]
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 3, 5, 7, 9 };
            thisTest.Verify("A", v1, 0);
        }

        [Test]
        public void T00002_Language_001a_array_test_4()
        {
            string errmsg = "";// "1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements ";
            string code = @"a=1;
b=2;
c=4;
collection = {a,b,c};
collection[1] = collection[1] + 0.5;
d = collection[1];
d = d + 0.1; // updates the result of accessing the collection
b = b + 0.1; // updates the source member of the collection
t1 = collection[0];
t2 = collection[1];
t3 = collection[2];
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            Object[] v1 = new Object[] { 1, 2.600, 4 };
            thisTest.Verify("collection", v1, 0);
            thisTest.Verify("d", 2.6, 0);
            /*thisTest.Verify("t1", 1, 0);
            thisTest.Verify("t2", 2.6, 0);
            thisTest.Verify("t3", 4, 0);*/
        }

        [Test]
        [Category("Replication")]
        public void T00003_Language_001b_replication_expressions()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");

            string code = @"
a = {1,0,-1};
b = {0, 5, 10};
zipped_sum = a + b; // {1, 5, 9}
cartesian_sum  = a<1> + b<2>;
// cartesian_sum =    {{1, 6, 11},
//        			   {0, 5, 10},
//        			   {-1, 4, 9}}
cartesian_sum  = a<2> + b<1>;
t1 = zipped_sum[0];
t2 = zipped_sum[1];
t3 = zipped_sum[2];
t4 = cartesian_sum[0][0];
t5 = cartesian_sum[1][0];
t6 = cartesian_sum[2][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] v1 = new Object[] { 1, 5, 9 };
            Object[] v2 = new Object[] { new Object[] { 1, 0, -1 }, new Object[] { 6, 5, 4 }, new Object[] { 11, 10, 9 } };
            thisTest.Verify("zipped_sum", v1, 0);
            thisTest.Verify("cartesian_sum", v2, 0);
            /*thisTest.Verify("t1", 1, 0);
            thisTest.Verify("t2", 5, 0);
            thisTest.Verify("t3", 9, 0);
            thisTest.Verify("t4", 1, 0);
            thisTest.Verify("t5", 6, 0);
            thisTest.Verify("t5", 11, 0);*/
        }

        [Test]
        [Category("Replication")]
        public void T00004_Geometry_002_line_by_points_replication_simple()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
startPt = Point.ByCartesianCoordinates( 1, 1, 0 );
endPt   = Point.ByCartesianCoordinates( 1, 5, 0 );
line_0  = Line.ByStartPointEndPoint( startPt, endPt ); 	// create line_0
line_0_StartPoint_X = line_0.StartPoint.X;
startPt = Point.ByCartesianCoordinates( (1..5..1), 1, 0 ); // with range expression
endPt   = Point.ByCartesianCoordinates( (1..5..1), 5, 0 ); // with range expression.. but line does not replicate
line_0  = Line.ByStartPointEndPoint( startPt<1>, endPt<2> ); 	// add replication guides <1> <2>
line_0  = Line.ByStartPointEndPoint( startPt, endPt ); 		// remove replication guides
t1 = line_0[0].StartPoint.X;
t2 = line_0[1].StartPoint.X;
t3 = line_0[2].StartPoint.X;
t4 = line_0[3].StartPoint.X;
t5 = line_0[4].StartPoint.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            //Object[] v1 = new Object[] { 1, 2, 3, 4, 5 };
            //thisTest.Verify("line_0_StartPoint_X", v1, 0);
            thisTest.Verify("t1", 1.0, 0);
            thisTest.Verify("t2", 2.0, 0);
            thisTest.Verify("t3", 3.0, 0);
            thisTest.Verify("t4", 4.0, 0);
            thisTest.Verify("t5", 5.0, 0);
        }

        [Test]
        [Category("Replication")]
        public void T00005_Geometry_002_line_by_points_replication_simple_correction()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
//#include ""GeometryLibForLanguageTesting.ds""
startPt = Point.ByCartesianCoordinates( 1, 1, 0 );
endPt   = Point.ByCartesianCoordinates( 1, 5, 0 );
line_0  = Line.ByStartPointEndPoint(startPt, endPt); 	// create line_0
line_0_StartPoint_X = line_0.StartPoint.X;
startPt = Point.ByCartesianCoordinates( (1..5..1), 1, 0 ); // with range expression
endPt   = Point.ByCartesianCoordinates( (1..5..1), 5, 0 ); // with range expression
line_0  = Line.ByStartPointEndPoint(startPt, endPt); 		// no replication guides
line_0  = Line.ByStartPointEndPoint(startPt<1>, endPt<1>); 	// add replication guides <1> <1>
line_0  = Line.ByStartPointEndPoint(startPt<1>, endPt<2>); 	// add replication guides <1> <2>
line_0  = Line.ByStartPointEndPoint(startPt<1>, endPt<1>); 	// add replication guides <1> <1>
line_0  = Line.ByStartPointEndPoint(startPt, endPt); 		// remove replication guides
t1 = line_0[0].StartPoint.X;
t2 = line_0[1].StartPoint.X;
t3 = line_0[2].StartPoint.X;
t4 = line_0[3].StartPoint.X;
t5 = line_0[4].StartPoint.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            Object[] v1 = new Object[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

            thisTest.Verify("line_0_StartPoint_X", v1, 0);
            thisTest.Verify("t1", 1.0, 0);
            thisTest.Verify("t2", 2.0, 0);
            thisTest.Verify("t3", 3.0, 0);
            thisTest.Verify("t4", 4.0, 0);
            thisTest.Verify("t5", 5.0, 0);
        }

        [Test]
        [Category("Replication")]
        public void T00006_Geometry_003_line_by_points_replication_array()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            string errmsg = "DNL-1467298 rev 3769 : replication guides with partial array indexing is not supported by parser";
            string code = @"import (""GeometryLibForLanguageTesting.ds"");
//#include ""GeometryLibForLanguageTesting.ds""
startPt = Point.ByCartesianCoordinates( 1, 1, 0 );
endPt   = Point.ByCartesianCoordinates( 1, 5, 5 );
line_0  = Line.ByStartPointEndPoint(startPt, endPt ); 	// create line_0
line_0_StartPoint_X = line_0.StartPoint.X;
startPt = Point.ByCartesianCoordinates( (1..5..1), 1, 0 ); // replicate in X
startPt = Point.ByCartesianCoordinates( (1..5..1), (1..5..1), 0 ); // replicate in X and Y
startPt = Point.ByCartesianCoordinates( (1..5..1)<1>, (1..5..1)<2>, 0 ); // replicate in X and Y with replication guides
line_0  = Line.ByStartPointEndPoint(startPt[2], endPt); // create line_0, select from startPt
startPt = Point.ByCartesianCoordinates( (1..5..1)<2>, (1..5..1)<1>, 0 ); // replicate in X and Y with replication guides
line_0  = Line.ByStartPointEndPoint(startPt, endPt); // create line_0, select from startPt
startPt = Point.ByCartesianCoordinates( (1..5..1), (1..5..1), 0 ); // replicate in X and Y with replication guides
startPt = Point.ByCartesianCoordinates( (1..5..1)<1>, (1..5..1)<1>, 0 ); // replicate in X and Y with replication guides
startPt = Point.ByCartesianCoordinates( (1..5..1)<1>, (1..5..1)<2>, 0 ); // replicate in X and Y with replication guides
startPt = Point.ByCartesianCoordinates( (1..8..1)<1>, (1..8..1)<2>, 0 ); // replicate in X and Y with replication guides
startPt = Point.ByCartesianCoordinates( (1..8..2)<1>, (1..8..2)<2>, 0 ); // replicate in X and Y with replication guides
startPt = Point.ByCartesianCoordinates( (1..5..1)<1>, (1..5..1)<2>, 0 ); // replicate in X and Y with replication guides
startPt = Point.ByCartesianCoordinates( 2, 1, 0 );
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg, importPath);

            thisTest.Verify("line_0_StartPoint_X", 2.0, 0);

        }


        [Test]
        public void T00007_Geometry_004_circle_all_combinations()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators ");
            string errmsg = "";//DNL-1467282 Replication guides not working in constructor of class";
            string code = @"import (""GeometryLibForLanguageTesting.ds"");
import(""DSCoreNodes.dll"");
//#include ""GeometryLibForLanguageTesting.ds""
//circlePoint = Point.ByCartesianCoordinates(10.0*cos(0..(360)..#21), 10.0*sin(0..(360)..#21), 0.0);
circlePoint = Point.ByCartesianCoordinates( 10.0 * Math.Cos(0..(360)..#4), 10.0 * Math.Sin(0..(360)..#4), 0.0);
lines = Line.ByStartPointEndPoint(circlePoint<1>,circlePoint<2>);
lines_StartPoint_X = lines.StartPoint.X; 
t1 = lines[0][0].StartPoint.X;
t2 = lines[1][0].StartPoint.X;
t3 = lines[2][0].StartPoint.X;
t4 = lines[3][0].StartPoint.X;";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg, importPath);

            //Object[] v1 = new Object[] { new Object[] { 10.000, 10.000, 10.000, 10.000 }, new Object[] { -5.000, -5.000, -5.000, -5.000 }, new Object[] { -5.000, -5.000, -5.000, -5.000 }, new Object[] { 10.000, 10.000, 10.000, 10.000 } };
            //thisTest.Verify("lines_StartPoint_X", v1, 0);
            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", -5.0, 0);
            thisTest.Verify("t3", -5.0, 0);
            thisTest.Verify("t4", 10.0, 0);
        }


        [Test]
        public void T00008_Geometry_005_circle_adjacent_pairs_externalised()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators "); 

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
//#include ""GeometryLibForLanguageTesting.ds""
import(""DSCoreNodes.dll"");
//numPoints = 21;
numPoints = 5;
circlePoint = Point.ByCartesianCoordinates( 10.0*Math.Cos(0..(360)..#numPoints), 10.0*Math.Sin(0..(360)..#numPoints), 0.0 );
lines = Line.ByStartPointEndPoint( circlePoint[-1..(count(circlePoint)-2)..1], circlePoint[0..(count(circlePoint)-1)..1] ); 
lines_StartPoint_X = lines.StartPoint.X; 
//numPoints = 11;
numPoints = 3;
t1 = lines[0].StartPoint.X;
t2 = lines[1].StartPoint.X;
t3 = lines[2].StartPoint.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            //Object[] v1 = new Object[] { 10.000, 10.000, -10.000 };
            //thisTest.Verify("lines_StartPoint_X", v1, 0);
            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", 10.0, 0);
            thisTest.Verify("t3", -10.0, 0);
        }


        [Test]
        [Category("Replication")]
        public void T00009_Geometry_006_circle_all_unique_combinations()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators "); 
            //Assert.Fail("1467075 - Sprint23 : rev 2660 : replication with nested array is not working as expected");
            //Assert.Fail("1467174 sprint24 : rev 3150 : warning:Function 'get_X' not Found");

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
def drawUniqueLines (points : Point[], start : int, end : int) = Line.ByStartPointEndPoint(points[(0..start..1)],points[end]); 
circlePoints = Point.ByCartesianCoordinates( 10..13, 4..7, 0.0 );
lines = drawUniqueLines(circlePoints, (1..(Count(circlePoints)-2)..1), (2..(Count(circlePoints)-1)..1));
lines_StartPoint_X = lines.StartPoint.X; 
t1 = lines[0][0].StartPoint.X;
t2 = lines[1][2].StartPoint.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", 12.0, 0);
        }

        [Test]
        [Category("Failure")]
        public void T00010_Geometry_007_specialPoint_2()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4010
            Assert.Fail("1460783 - Sprint 18 : Rev 1661 : Forward referencing is not being allowed in class property ( related to Update issue ) ");

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
import(""DSCoreNodes.dll"");
class MyPoint 
{
	// define general system of dependencies
	
	x : double = radius * Math.Cos(theta*180/180); // x dependent on theta and radius
	y : double = radius * Math.Sin(theta*180/180); // y dependent on theta and radius
	z : double = 0.0;
											
	theta  = Math.atan(y/x) * 180 / 180;		 	 // theta  dependent on x and y
	radius = Math.sqrt(x*x + y*y);				 // radius dependent on x and y
	
	inner  : Point = Point.ByCartesianCoordinates( x, y, z );	 // create inner point dependent on x and y
	
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
	
	def incrementX(this : MyPoint, xValue : double) = ByXYcoordinates(this.x + xValue, this.y);
	def incrementY(this : MyPoint, yValue : double) = ByXYcoordinates(this.x,this.y + yValue);
	def incrementTheta(this : MyPoint, thetaValue : double)  = ByAngleRadius(this.theta + thetaValue, this.radius );
	def incrementRadius(this : MyPoint, radiusValue : double)= ByAngleRadius(this.theta, this.radius + radiusValue );
}
a 		= MyPoint.ByXYcoordinates( 1.0, 1.0 );		// create an instance 'a' using one constructor
origin         = Point.ByCartesianCoordinates( 0, 0, 0 );  		// create a reference point
testLine       = Line.ByStartPointEndPoint(origin, a.inner);	// create a testLine (to see some results)
testLine_SP_X = testLine.StartPoint.X; 
aX 		= a.x;							// report the properties of 'a'
aY 		= a.y;
aTheta 	= a.theta;
aRadius        = a.radius;
a 		= MyPoint.ByAngleRadius(60.0, 1.0);			// switch to a different constructor [POINT updates]
//a		= a.visible(false); 
a 		= a.incrementX(0.2);					// apply different modifiers [POINT does not updates]
//a		= a.visible(false);
a 		= a.incrementY(-0.2);					// apply different modifiers [POINT does not updates]
a 		= MyPoint.ByAngleRadius(45.0, 0.75);			// redefine a (by using a constructor) [POINT updates]
//a		= a.visible(false);
a 		= a.incrementTheta(10.0);				// apply different modifiers [POINT does not updates]
//a		= a.visible(false);
a 		= a.incrementRadius(0.2); 				// [POINT does not updates]
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("testLine_SP_X", 0, 0);
            thisTest.Verify("aRadius", 0.95, 0);
            thisTest.Verify("aTheta", 55.0, 0);
            thisTest.Verify("aY", 0.778, 0);
            thisTest.Verify("aX", 0.545, 0);
        }

        [Test]
        [Category("Replication")]
        [Category("Failure")]
        public void T00011_Geometry_008_trim_then_tube_4()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4010
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException "); 

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
myPoint = Point.ByCartesianCoordinates( (2..10..2)<1>, (2..10..2)<2>, 2 ); // create 2D point array
controlPoint_1 = Point.ByCartesianCoordinates( 5, 5, 5 ); 		// create control point
controlPoint_2 = Point.ByCartesianCoordinates( 5, 10, 5 ); 	// create control point
myLine = Line.ByStartPointEndPoint( myPoint[1], controlPoint_1 ); 	// select 1d array of points to create a 1D array lines
//myLine[2] = myLine[2].Trim({0.9, 0.1}, false); 	// trim one member of the array of points (modify a member of a collection)
myLine[2] = myLine[2].Trim( 0.5 );
//tubes   = Tube.ByStartPointEndPointRadius(myLine.StartPoint, myLine.EndPoint, 0.25, 0.125); // use the whole array of lines for tubing
//tubes   = Solid.Cone(myLine.StartPoint, myLine.EndPoint, 0.25, 0.125); // use the whole array of lines for tubing
tubes      = Solid.Cone(myLine.StartPoint, myLine.EndPoint, 0.25, 0.125);
controlPoint_1 = Point.ByCartesianCoordinates( 7, 7, 5 ); 		// move the control point, change gets propagated to lines, trim and tubes, 
//myLine[3] = myLine[3].Trim({0.9, 0.2}, false); 		// trim another member of the array of points (modify a member of a collection)
myLine[3] = myLine[3].Trim( 0.8); 	
controlPoint_1 = Point.ByCartesianCoordinates( 8, 7, 5 ); 		// move the control point, change gets propagated to lines, trim and tubes, 
t1 = tubes[0].EndPoint.X;
t2 = tubes[1].EndPoint.X;
t3 = tubes[2].EndPoint.X;
t4 = tubes[3].EndPoint.X;
t5 = tubes[4].EndPoint.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("t1", 8.0, 0);
            thisTest.Verify("t2", 8.0, 0);
            thisTest.Verify("t3", 4.0, 0);
            thisTest.Verify("t4", 6.4, 0);
            thisTest.Verify("t5", 8.0, 0);
        }


        [Test]
        [Category("Failure")]
        public void T00012_Geometry_008a_alternative_method_invocations_1()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4010
            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
startRadius = 0.1;
endRadius	= 0.2;
startParam  = 0.2;
endParam    = 0.8;
// [1] regular method call [separate Line constructor, modifier, then Tube constructor to new variable]
start_0 = Point.ByCartesianCoordinates( 1, 1, 0 );
end_0   = Point.ByCartesianCoordinates( 1, 5, 0 );
line_0  = Line.ByStartPointEndPoint(start_0, end_0); 	// create line_0
line_0  = line_0.Trim({endParam, startParam}, false);
tube_0  = Tube.ByStartPointEndPointRadius(line_0.StartPoint, line_0.EndPoint, startRadius, endRadius);
// [2] method chain, with embedded point arguments
x_1 = 5;
line_1  = Line.ByStartPointEndPoint(Point.ByCartesianCoordinates( x_1, 1, 0 ), Point.ByCartesianCoordinates( 5, 5, 0 )).Trim({endParam, startParam}, false); 	// create line_1 method chain
x_1 = 7;
// [3] doubly embedded methods as arguments
x_2 = 10;
line_2  = Line.Trim(Line.ByStartPointEndPoint(Point.ByCartesianCoordinates( x_2, 1, 0 ), Point.ByCartesianCoordinates( 10, 5, 0 )),{endParam, startParam}, false); 	// create line_2 embedded method call
x_2 = 12; // cause an update
// [4] define a function as a compound operation
def TrimTube(line : Line, startParam: double, endParam: double, startRadius : double, endRadius : double)
{
	intermediateLine = line.Trim( {endParam, startParam}, false);
	return = Solid.Cone(intermediateLine.StartPoint, intermediateLine.EndPoint, startRadius, endRadius);
}
// [5] apply compound operation to create a new result (tube) variable, given existing input (line) variable
x_3 = 15;
line_3 = Line.ByStartPointEndPoint(Point.ByCartesianCoordinates( x_3 , 1, 0 ), Point.ByCartesianCoordinates( 15, 5, 0 ));
tube_3 = TrimTube(line_3, 0.2, 0.7, 0.3, 0.15); // apply operations
x_3 = 17;
// [6] apply compound operation to modify existing input (line) variable AND effectively change its type from Line to Tube
x_4 = 20; 
line_4 = Line.ByStartPointEndPoint(Point.ByCartesianCoordinates( x_4, 1, 0 ), Point.ByCartesianCoordinates( 20, 5, 0 ));
line_4 = TrimTube(line_4, 0.2, 0.7, 0.3, 0.15); // apply operations as modifier AND effectively change the type of line_4
otherPoint = Point.ByCartesianCoordinates( x_4+5, 10, 0 );
otherLine  = Line.ByStartPointEndPoint(otherPoint,line_4.StartPoint); // even though line_4 is now a tube, 
																 // we should be able to still refer 
																 // to a property of its previous state
																 // i.e. to its start point 
x_4 = 22; 
x1 = line_0.StartPoint.X;
x2 = line_1.StartPoint.X;
x3 = line_2.StartPoint.X;
x4 = line_3.StartPoint.X;
x5 = line_4.StartPoint.X;
x6 = otherLine.StartPoint.X;
x7 = tube_0.StartPoint.X;
x8 = tube_3.StartPoint.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("x1", 0.8, 0);
            thisTest.Verify("x2", 5.6, 0);
            thisTest.Verify("x3", 9.6, 0);
            thisTest.Verify("x4", 17, 0);
            thisTest.Verify("x5", 15.4, 0);
            thisTest.Verify("x6", 27, 0);
            thisTest.Verify("x7", 0.8, 0);
            thisTest.Verify("x8", 11.9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T00013_Geometry_009_nested_user_defined_feature_2b()
        {
            //Assert.Fail("1456115 - Sprint16: Rev 889 : Replication over a collection is not working as expected ");

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
//#include ""GeometryLibForLanguageTesting.ds""
start    = Point.ByCartesianCoordinates( 2, 2, 0 );
end      = Point.ByCartesianCoordinates( 7, 7, 0 );
line     = Line.ByStartPointEndPoint(start, end);
midPoint = line.PointAtParameter(0.5);
// [2] create MyLine user define feature using sub classing and 'this' instance
class MyLine 
{
    InternalLine : Line;  		// user defined feature contains a line property
    MidPoint : Point;	// and a midPoint property
    public constructor ByStartPointEndPoint(start : Point, end : Point)
    {
		InternalLine = Line.ByStartPointEndPoint(start, end);
		MidPoint = InternalLine.PointAtParameter(0.5);
    }
 }
point_a  = Point.ByCartesianCoordinates( 30, 40, 0 );
point_b  = Point.ByCartesianCoordinates( 10, 10, 0 );
point_c  = Point.ByCartesianCoordinates( 50, 10, 0 );
// [3] use MyLine to construct model for MyTriangle user defined feature
side_a_b = MyLine.ByStartPointEndPoint( point_a, point_b );
side_b_c = MyLine.ByStartPointEndPoint( point_b, point_c );
side_c_a = MyLine.ByStartPointEndPoint( point_c, point_a );
// [4] ceate MyTriangle user defined feature
class MyTriangle
{
    Side_a_b : MyLine;
    Side_b_c : MyLine;
    Side_c_a : MyLine;
    // with a constructor
    constructor ByPoints(point_a : Point, point_b : Point, point_c : Point)
	{
		Side_a_b = MyLine.ByStartPointEndPoint( point_a, point_b );
		Side_b_c = MyLine.ByStartPointEndPoint( point_b, point_c );
		Side_c_a = MyLine.ByStartPointEndPoint( point_c, point_a );
	}    
}
// [5] Create initial three defining points.
point_1 = Point.ByCartesianCoordinates( 30, 80, 0 );
point_2 = Point.ByCartesianCoordinates( 10, 50, 0 );
point_3 = Point.ByCartesianCoordinates( 50, 50, 0 );
// [6] Create outer instance of MyTriangle from defining points.
triangle0000 = MyTriangle.ByPoints(point_1, point_2, point_3);
// [7] Create inner instance of MyTriangle from midPoints of sides of the outer instance of MyTriangle.
triangle0001 = MyTriangle.ByPoints(  triangle0000.Side_a_b.MidPoint,triangle0000.Side_b_c.MidPoint,triangle0000.Side_c_a.MidPoint);
// [8] Create inner instance of MyTriangle from midPoints of sides of the outer instance of MyTriangle.
triangle0002 = MyTriangle.ByPoints(  triangle0001.Side_a_b.MidPoint,triangle0001.Side_b_c.MidPoint,triangle0001.Side_c_a.MidPoint);
// [8] change the definition of the MyLine user define feature from 'composition' to 'sub classing' 
//     from an existing class (in this case from the Line class)
//     redefining MyTriangle should update all existing instances of MyTriangle
// [9] move point_1	 								 
point_1 = Point.ByCartesianCoordinates( 50, 80, 0 ); // points do not clean up from old position
point_1 = Point.ByCartesianCoordinates( 60, 90, 0 ); // when points are single values
// [10] replicate point_1 								 			 
point_1 = Point.ByCartesianCoordinates( (20..28..2), 80, 0 ); // points do clean up when going from single value to collection
point_1 = Point.ByCartesianCoordinates( (70..78..1), 80, 0 ); // points not do clean up when going from collection of one size to collection of another size
point_1 = Point.ByCartesianCoordinates( (20..28..2), 80, 0 ); // points not do clean up when going from collection of one size to collection of another size
point_1 = Point.ByCartesianCoordinates( (50..58..2), 80, 0 ); // points do clean up when going from collection of one size to collection of the same size
// [11] move point_1	 								 								 
point_1 = Point.ByCartesianCoordinates( 10, 80, 0 ); // points do clean up when going from collection to single value 
point_1 = Point.ByCartesianCoordinates( 20, 100, 0 ); // points do not clean up from old position when going from single value to single value 
x1 = triangle0000.Side_b_c.MidPoint.X;
x2 = triangle0001.Side_b_c.MidPoint.X;
x3 = triangle0002.Side_b_c.MidPoint.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("x1", 5.0, 0);
            thisTest.Verify("x2", 2.5, 0);
            thisTest.Verify("x3", 1.25, 0);
        }


        [Test]
        [Category("Replication")]
        public void T00014_Geometry_010_nested_user_defined_feature_rand_2()
        {
            //Assert.Fail("1460965 - Sprint 18 : Rev 1700 : Design Issue : Accessing properties from collection of class instances using the dot operator should yield a collection of the class properties");

            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
//#include ""GeometryLibForLanguageTesting.ds""
// [1] create initial model from which to make the MyLine user defined feature
start    = Point.ByCartesianCoordinates( 2, 2, 0 );
end      = Point.ByCartesianCoordinates( 7, 7, 0 );
line     = Line.ByStartPointEndPoint( start, end );
midPoint = line.PointAtParameter( 0.5 );
// [2] create MyLine user define feature using sub classing
								// and 'this' instance
class MyLine 
{
    line : Line;  		// user defined feature contains a line property
    midPoint : Point;	// and a midPoint property
    public constructor ByStartPointEndPoint(start : Point, end : Point)
    {
	line     = Line.ByStartPointEndPoint( start, end );
	//midPoint = line.PointAtParameter( rand(2, 8)*0.1 );
    	midPoint = line.PointAtParameter(0.5);
    }
 }
point_a  = Point.ByCartesianCoordinates( 30, 40, 0 );
point_b  = Point.ByCartesianCoordinates( 10, 10, 0 );
point_c  = Point.ByCartesianCoordinates( 50, 10, 0 );
// [3] use MyLine to construct model for MyTriangle user defined feature
side_a_b = MyLine.ByStartPointEndPoint( point_a, point_b );
side_b_c = MyLine.ByStartPointEndPoint( point_b, point_c );
side_c_a = MyLine.ByStartPointEndPoint( point_c, point_a );
// [4] ceate MyTriangle user defined feature
class MyTriangle
{
    side_a_b : MyLine;
    side_b_c : MyLine;
    side_c_a : MyLine;
    // with a constructor
    constructor ByPoints(point_a : Point, point_b : Point, point_c : Point)
    {
        side_a_b = MyLine.ByStartPointEndPoint( point_a, point_b );
        side_b_c = MyLine.ByStartPointEndPoint( point_b, point_c );
        side_c_a = MyLine.ByStartPointEndPoint(point_c, point_a );
    } 
}
// [5] Create initial three defining points.
point_1 = Point.ByCartesianCoordinates( 30, 80, 0 );
point_2 = Point.ByCartesianCoordinates( 10, 50, 0 );
point_3 = Point.ByCartesianCoordinates( 50, 50, 0 );
// [6] Create outer instance of MyTriangle from defining points.
MyTriangle0000 = MyTriangle.ByPoints( point_1, point_2, point_3 );
// [7] Create inner instance of MyTriangle from midPoints of sides of the outer instance of MyTriangle.
MyTriangle0001 = MyTriangle.ByPoints( MyTriangle0000.side_a_b.midPoint,
                                     MyTriangle0000.side_b_c.midPoint,
                                     MyTriangle0000.side_c_a.midPoint );
									 
// [8] Create inner instance of MyTriangle from midPoints of sides of the outer instance of MyTriangle.
MyTriangle0002 = MyTriangle.ByPoints(MyTriangle0001.side_a_b.midPoint,
                                     MyTriangle0001.side_b_c.midPoint,
                                     MyTriangle0001.side_c_a.midPoint);
									 
// [8] change the definition of the MyLine user define feature from 'composition' to 'sub classing' 
//     from an existing class (in this case from the Line class)
//     redefining MyTriangle should update all existing instances of MyTriangle
// [9] replicate point_1									 
point_1 = Point.ByCartesianCoordinates( (20..28..2), 80, 0 );
x1 = MyTriangle0001[0].side_a_b.midPoint.X;
x2 = MyTriangle0001[1].side_b_c.midPoint.X;
x3 = MyTriangle0001[2].side_c_a.midPoint.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("x1", 5.0, 0);
            thisTest.Verify("x2", 2.5, 0);
            thisTest.Verify("x3", 12.5, 0);
        }

        [Test]
        [Category("Feature")]
        public void T00015_Geometry_011_nested_user_defined_feature_with_partial_class_1()
        {
            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
// [1] create initial model from which to make the MyLine user defined feature
start    = Point.ByCartesianCoordinates( 2, 2, 0 );
end      = Point.ByCartesianCoordinates( 7, 7, 0 );
line     = Line.ByStartPointEndPoint(start, end);
midPoint = line.PointAtParameter(0.5);
// [2] create MyLine user define feature using sub classing
								// and 'this' instance
class MyLine 
{
    InternalLine : Line;  	// user defined feature contains a line property
    MidPoint     : Point;	// and a midPoint property
    public constructor ByStartPointEndPoint(start : Point, end : Point)
    {
		InternalLine = Line.ByStartPointEndPoint(start, end);
		MidPoint 	 = InternalLine.PointAtParameter(0.5);
    }
 }
point_a  = Point.ByCartesianCoordinates( 30, 40, 0 );
point_b  = Point.ByCartesianCoordinates( 10, 10, 0 );
point_c  = Point.ByCartesianCoordinates( 50, 10, 0 );
// [3] use MyLine to construct model for MyTriangle user defined feature
side_a_b = MyLine.ByStartPointEndPoint(point_a, point_b);
side_b_c = MyLine.ByStartPointEndPoint(point_b, point_c);
side_c_a = MyLine.ByStartPointEndPoint(point_c, point_a);


x1 = side_a_b.MidPoint.X;
x2 = side_b_c.MidPoint.X;
x3 = side_c_a.MidPoint.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);

            thisTest.Verify("x1", 15.0, 0);
            thisTest.Verify("x2", 5.0, 0);
            thisTest.Verify("x3", 25.0, 0);
            
        }

        [Test]
        [Category("Feature")]
        public void T00016_Geometry_012_centroid_1()
        {
            string code = @"
import (""GeometryLibForLanguageTesting.ds"");
// [1] create functions to calculate the centroid of a collection of points 
def sumCollection(arr : double[]) = sumCollectionInternal(arr, Count(arr)-1);
def sumCollectionInternal(arr : double[], i : int ) 
{
    return = [Imperative]
    {
        if( i > -1) 
        {
            return = arr[i] + sumCollectionInternal(arr, i-1);
        }
        else
        {
            return = 0;
        }
    }
}

def average(arr : double[]) = sumCollection(arr) / Count(arr);
def centroid(points : Point[]) = Point.ByCartesianCoordinates( average(points.X), average(points.Y),  average(points.Z) );
// [2] create some points
point_1 = Point.ByCartesianCoordinates( 30.0, 80.0, 0.0 );
point_2 = Point.ByCartesianCoordinates( 10.0, 50.0, 0.0 );
point_3 = Point.ByCartesianCoordinates( 50.0, 50.0, 0.0 );
// [3] create centrePoint
centrePoint = centroid( {point_1, point_2, point_3} );
// [4] test with lines
lineTest  = Line.ByStartPointEndPoint( centrePoint, { point_1, point_2, point_3 } );
// [5] move a point
point_1 = Point.ByCartesianCoordinates( 40.0, 80.0, 0.0 );
x1 = lineTest[2].EndPoint.X;

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            thisTest.Verify("x1", 50.0, 0);
        }

    }
}
