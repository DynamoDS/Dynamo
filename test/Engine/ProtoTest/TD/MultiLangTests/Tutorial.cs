using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
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
A = [1,2,3,4]; 		// redefine A as a collection
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
collection = [a,b,c];
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
a = [1,0,-1];
b = [0, 5, 10];
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
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T00004_Geometry_002_line_by_points_replication_simple()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");

            string code = @"
import (""FFITarget.dll"");
startPt = DummyPoint.ByCoordinates( 1, 1, 0 );
endPt   = DummyPoint.ByCoordinates( 1, 5, 0 );
line_0  = DummyLine.ByStartPointEndPoint( startPt, endPt ); 	// create line_0
line_0_StartPoint_X = line_0.Start.X;
startPt = DummyPoint.ByCoordinates( (1..5..1), 1, 0 ); // with range expression
endPt   = DummyPoint.ByCoordinates( (1..5..1), 5, 0 ); // with range expression.. but line does not replicate
line_0  = DummyLine.ByStartPointEndPoint( startPt<1>, endPt<2> ); 	// add replication guides <1> <2>
line_0  = DummyLine.ByStartPointEndPoint( startPt, endPt ); 		// remove replication guides
t1 = line_0[0].Start.X;
t2 = line_0[1].Start.X;
t3 = line_0[2].Start.X;
t4 = line_0[3].Start.X;
t5 = line_0[4].Start.X;
";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T00005_Geometry_002_line_by_points_replication_simple_correction()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");

            string code = @"
import (""FFITarget.dll"");
startPt = DummyPoint.ByCoordinates( 1, 1, 0 );
endPt   = DummyPoint.ByCoordinates( 1, 5, 0 );
line_0  = Line.ByStartPointEndPoint(startPt, endPt); 	// create line_0
line_0_StartPoint_X = line_0.Start.X;
startPt = DummyPoint.ByCoordinates( (1..5..1), 1, 0 ); // with range expression
endPt   = DummyPoint.ByCoordinates( (1..5..1), 5, 0 ); // with range expression
line_0  = DummyLine.ByStartPointEndPoint(startPt, endPt); 		// no replication guides
line_0  = DummyLine.ByStartPointEndPoint(startPt<1>, endPt<1>); 	// add replication guides <1> <1>
line_0  = DummyLine.ByStartPointEndPoint(startPt<1>, endPt<2>); 	// add replication guides <1> <2>
line_0  = DummyLine.ByStartPointEndPoint(startPt<1>, endPt<1>); 	// add replication guides <1> <1>
line_0  = DummyLine.ByStartPointEndPoint(startPt, endPt); 		// remove replication guides
t1 = line_0[0].Start.X;
t2 = line_0[1].Start.X;
t3 = line_0[2].Start.X;
t4 = line_0[3].Start.X;
t5 = line_0[4].Start.X;";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T00006_Geometry_003_line_by_points_replication_array()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            string errmsg = "DNL-1467298 rev 3769 : replication guides with partial array indexing is not supported by parser";
            string code = @"
import (""FFITarget.dll"");
startPt = DummyPoint.ByCoordinates( 1, 1, 0 );
endPt   = DummyPoint.ByCoordinates( 1, 5, 5 );
line_0  = DummyLine.ByStartPointEndPoint(startPt, endPt ); 	// create line_0
line_0_StartPoint_X = line_0.Start.X;
startPt = Point.ByCartesianCoordinates( (1..5..1), 1, 0 ); // replicate in X
startPt = Point.ByCartesianCoordinates( (1..5..1), (1..5..1), 0 ); // replicate in X and Y
startPt = Point.ByCartesianCoordinates( (1..5..1)<1>, (1..5..1)<2>, 0 ); // replicate in X and Y with replication guides
line_0  = Line.ByStartPointEndPoint(startPt[2], endPt); // create line_0, select from startPt
startPt = DummyPoint.ByCoordinates( (1..5..1)<2>, (1..5..1)<1>, 0 ); // replicate in X and Y with replication guides
line_0  = DummyLine.ByStartPointEndPoint(startPt, endPt); // create line_0, select from startPt
startPt = DummyPoint.ByCoordinates( (1..5..1), (1..5..1), 0 ); // replicate in X and Y with replication guides
startPt = DummyPoint.ByCoordinates( (1..5..1)<1>, (1..5..1)<1>, 0 ); // replicate in X and Y with replication guides
startPt = DummyPoint.ByCoordinates( (1..5..1)<1>, (1..5..1)<2>, 0 ); // replicate in X and Y with replication guides
startPt = DummyPoint.ByCoordinates( (1..8..1)<1>, (1..8..1)<2>, 0 ); // replicate in X and Y with replication guides
startPt = DummyPoint.ByCoordinates( (1..8..2)<1>, (1..8..2)<2>, 0 ); // replicate in X and Y with replication guides
startPt = DummyPoint.ByCoordinates( (1..5..1)<1>, (1..5..1)<2>, 0 ); // replicate in X and Y with replication guides
startPt = DummyPoint.ByCoordinates( 2, 1, 0 );
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg, importPath);

            thisTest.Verify("line_0_StartPoint_X", 2.0, 0);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T00009_Geometry_006_circle_all_unique_combinations()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators "); 
            //Assert.Fail("1467075 - Sprint23 : rev 2660 : replication with nested array is not working as expected");
            //Assert.Fail("1467174 sprint24 : rev 3150 : warning:Function 'get_X' not Found");

            string code = @"
import(""FFITarget.dll"");
def drawUniqueLines (points : DummyPoint[], start : int, end : int)
{
    return = DummyLine.ByStartPointEndPoint(points[(0..start..1)],points[end]); 
}
circlePoints = DummyPoint.ByCoordinates( 10..13, 4..7, 0.0 );
lines = drawUniqueLines(circlePoints, (1..(Count(circlePoints)-2)..1), (2..(Count(circlePoints)-1)..1));
lines_StartPoint_X = lines.Start.X; 
t1 = lines[0][0].Start.X;
t2 = lines[1][2].Start.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", 12.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T00016_Geometry_012_centroid_1()
        {
            string code = @"
import(""FFITarget.dll"");
def sumCollection(arr : double[])
{
    return = sumCollectionInternal(arr, Count(arr)-1);
}
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

def average(arr : double[])
{
    return = sumCollection(arr) / Count(arr);
}

def centroid(points : DummyPoint[])
{
    return = DummyPoint.ByCoordinates( average(points.X), average(points.Y),  average(points.Z) );
}
// [2] create some points
point_1 = DummyPoint.ByCoordinates( 30.0, 80.0, 0.0 );
point_2 = DummyPoint.ByCoordinates( 10.0, 50.0, 0.0 );
point_3 = DummyPoint.ByCoordinates( 50.0, 50.0, 0.0 );
// [3] create centrePoint
centrePoint = centroid( [point_1, point_2, point_3] );
// [4] test with lines
lineTest  = DummyLine.ByStartPointEndPoint( centrePoint, [ point_1, point_2, point_3 ] );
// [5] move a point
point_1 = DummyPoint.ByCoordinates( 40.0, 80.0, 0.0 );
x1 = lineTest[2].End.X;

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            thisTest.Verify("x1", 50.0, 0);
        }

    }
}
