using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using System.Collections.Generic;
using ProtoTestFx.TD;
namespace ProtoTest.TD.DocTests
{
    class UserManualTests : ProtoTestBase
    {
        [Test]
        public void UM01_Print()
        {
            string code =
@"quote = ""Less is bore."";s = Print(quote + "" "" + quote + "" "" + quote);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM02_Point()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a point with the following x, y, and z// coordinates:x = 10;y = 2.5;z = -6;p = Point.ByCoordinates(x, y, z);a = p.X;b = p.Y;c = p.Z;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10.0);
            thisTest.Verify("b", 2.5);
            thisTest.Verify("c", -6.0);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM03_Point()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a point on a sphere with the following radius,// theta, and phi rotation angles (specified in degrees)radius = 5;theta = 75.5;phi = 120.3;cs = CoordinateSystem.Identity();p = Point.BySphericalCoordinates(cs, radius, theta,phi);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM04_Line()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create two points:p1 = Point.ByCoordinates(3, 10, 2);p2 = Point.ByCoordinates(-15, 7, 0.5);// construct a line between p1 and p2l = Line.ByStartPointEndPoint(p1, p2);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM05_SurfaceLoft()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create points:p1 = Point.ByCoordinates(3, 10, 2);p2 = Point.ByCoordinates(-15, 7, 0.5);p3 = Point.ByCoordinates(5, -3, 5);p4 = Point.ByCoordinates(-5, -6, 2);p5 = Point.ByCoordinates(9, -10, -2);p6 = Point.ByCoordinates(-11, -12, -4);// create lines:l1 = Line.ByStartPointEndPoint(p1, p2);l2 = Line.ByStartPointEndPoint(p3, p4);l3 = Line.ByStartPointEndPoint(p5, p6);// loft between cross section lines:surf = Surface.LoftFromCrossSections({l1, l2, l3});";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM06_SurfaceThicken()
        {
            string code =
@"import(""ProtoGeometry.dll"");p1 = Point.ByCoordinates(3, 10, 2);p2 = Point.ByCoordinates(-15, 7, 0.5);p3 = Point.ByCoordinates(5, -3, 5);p4 = Point.ByCoordinates(-5, -6, 2);l1 = Line.ByStartPointEndPoint(p1, p2);l2 = Line.ByStartPointEndPoint(p3, p4);surf = Surface.LoftFromCrossSections({l1, l2});solid = surf.Thicken(4.75, true);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM07_SolidPlaneIntersect()
        {
            string code =
@"import(""ProtoGeometry.dll"");p1 = Point.ByCoordinates(3, 10, 2);p2 = Point.ByCoordinates(-15, 7, 0.5);p3 = Point.ByCoordinates(5, -3, 5);p4 = Point.ByCoordinates(-5, -6, 2);l1 = Line.ByStartPointEndPoint(p1, p2);l2 = Line.ByStartPointEndPoint(p3, p4);surf = Surface.LoftFromCrossSections({l1, l2});solid = surf.Thicken(4.75, true);p = Plane.ByOriginNormal(Point.ByCoordinates(2, 0, 0),Vector.ByCoordinates(1, 1, 1));int_surf = solid.Intersect(p);int_line = int_surf.Intersect(Plane.ByOriginNormal(Point.ByCoordinates(0, 0, 0),Vector.ByCoordinates(1, 0, 0)));";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM08_CoordinateSystem()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a CoordinateSystem at a specific location,// no rotations, scaling, or sheering transformationsx_pos = 3.6;y_pos = 9.4;z_pos = 13.0;origin = Point.ByCoordinates(x_pos, y_pos, z_pos);identity = CoordinateSystem.Identity();cs = CoordinateSystem.ByOriginVectors(origin,identity.XAxis, identity.YAxis, identity.ZAxis);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM09_PointOnCylinderShere()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a point with x, y, and z coordinatesx_pos = 1;y_pos = 2;z_pos = 3;pCoord = Point.ByCoordinates(x_pos, y_pos, z_pos);// create a point in a specific coordinate systemcs = CoordinateSystem.Identity();pCoordSystem = Point.ByCartesianCoordinates(cs, x_pos,y_pos, z_pos);// create a point on a cylinder with the following// radius and heightradius = 5;height = 15;theta = 75.5;pCyl = Point.ByCylindricalCoordinates(cs, radius, theta,height);// create a point on a sphere with radius and two anglesphi = 120.3;pSphere = Point.BySphericalCoordinates(cs, radius,theta, phi);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM10_Line()
        {
            string code =
@"import(""ProtoGeometry.dll"");p1 = Point.ByCoordinates(-2, -5, -10);p2 = Point.ByCoordinates(6, 8, 10);// a line segment between two pointsl2pts = Line.ByStartPointEndPoint(p1, p2);// a line segment at p1 in direction 1, 1, 1 with// length 10lDir = Line.ByStartPointDirectionLength(p1,Vector.ByCoordinates(1, 1, 1), 10);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM11_Solids()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a cuboid with specified lengthscs = CoordinateSystem.Identity();cub = Cuboid.ByLengths(cs, 5, 15, 2);// create several conesp1 = Point.ByCoordinates(0, 0, 10);p2 = Point.ByCoordinates(0, 0, 20);p3 = Point.ByCoordinates(0, 0, 30);l = Line.ByStartPointEndPoint(p2, p3);cone1 = Cone.ByStartPointEndPointRadius(p1, p2, 10, 6);cone2 = Cone.ByCenterLineRadius(l, 6, 0);// make a cylindercylCS = cs.Translate(10, 0, 0);cyl = Cylinder.ByRadiusHeight(cylCS, 3, 10);// make a spherecenterP = Point.ByCoordinates(-10, -10, 0);sph = Sphere.ByCenterPointRadius(centerP, 5);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM11_LineColor()
        {
            string code =
@"import(""ProtoGeometry.dll"");p1 = Point.ByCoordinates(0, 0, 0);p2 = Point.ByCoordinates(10, 10, 0);l = Line.ByStartPointEndPoint(p1, p2);l.Color = Color.ByARGB(255, 0, 255, 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM12_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");// construct a Vector objectv = Vector.ByCoordinates(1, 2, 3);s = Print(v.X);s = Print(v.Y);s = Print(v.Z);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM13_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");a = Vector.ByCoordinates(5, 5, 0);b = Vector.ByCoordinates(4, 1, 0);// c has value x = 9, y = 6, z = 0c = Vector.op_Addition(a, b);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM14_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");a = Vector.ByCoordinates(5, 5, 0);b = Vector.ByCoordinates(4, 1, 0);// c has value x = 1, y = 4, z = 0c = Vector.op_Subtraction(a, b);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM15_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");a = Vector.ByCoordinates(4, 4, 0);// c has value x = 20, y = 20, z = 0c = a.Scale(5);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM16_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");a = Vector.ByCoordinates(1, 2, 3);a_len = a.Length;// set the a's length equal to 1.0b = a.Normalize();c = b.Scale(5);// len is equal to 5len = c.Length;";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM17_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");a = Vector.ByCoordinates(1, 0, 1);b = Vector.ByCoordinates(0, 1, 1);// c has value x = -1, y = -1, z = 1c = a.Cross(b);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM18_Vector()
        {
            string code =
@"import(""ProtoGeometry.dll"");a = Vector.ByCoordinates(1, 2, 1);b = Vector.ByCoordinates(5, -8, 4);// d has value -7d = a.Dot(b);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM19_Range()
        {
            string code =
@"a = 10;b = 1..6;s = Print(a);s = Print(b);x = b[3];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 4);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM20_RangeLine()
        {
            string code =
@"import(""ProtoGeometry.dll"");x_pos = 1..6;y_pos = 5;z_pos = 1;lines = Line.ByStartPointEndPoint(Point.ByCoordinates(0,0, 0), Point.ByCoordinates(x_pos, y_pos, z_pos));";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM21_Range()
        {
            string code =
@"a = 0..1..0.1;s = Print(a);x = a[5];y = a[7];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.5);
            thisTest.Verify("y", 0.7);
        }

        [Test]
        public void UM22_Range()
        {
            string code =
@"a = 0..7..0.75;s = Print(a);x = a[3];y = a[5];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2.25);
            thisTest.Verify("y", 3.75);
        }

        [Test]
        public void UM23_Range()
        {
            string code =
@"// DesignScript will increment by 0.777 not 0.75a = 0..7..~0.75;s = Print(a);x = a[3];y = a[5];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2.3333333333333335);
            thisTest.Verify("y", 3.8888888888888893);
        }

        [Test]
        public void UM24_Range()
        {
            string code =
@"// Interpolate between 0 and 7 such that// “a” will contain 9 elementsa = 0..7..#9;s = Print(a);x = a[6];y = a[8];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5.25);
            thisTest.Verify("y", 7.0);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM25_Collection()
        {
            string code =
@"import(""ProtoGeometry.dll"");// use a range expression to generate a collection of// numbersnums = 0..10..0.75;s = Print(nums);// use the collection of numbers to generate a// collection of Pointspoints = Point.ByCoordinates(nums, 0, 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM26_Collection()
        {
            string code =
@"import(""ProtoGeometry.dll"");// a collection of numbersnums = 0..10..0.75;s = Print(nums);// create a single point with the 6th element of the// collectionpoints = Point.ByCoordinates(nums[5], 0, 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM27_Collection()
        {
            string code =
@"// generate a collection of numbersa = 0..6;// change several of the elements of a collectiona[2] = 100;a[5] = 200;s = Print(a);x = a[2];y = a[3];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 100);
            thisTest.Verify("y", 3);
        }

        [Test]
        public void UM28_Collection()
        {
            string code =
@"// create a collection explicitlya = { 45, 67, 22 };// create an empty collectionb = {};// change several of the elements of a collectionb[0] = 45;b[1] = 67;b[2] = 22;s = Print(a);s = Print(b);x = a[1];y = b[1];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 67);
            thisTest.Verify("y", 67);
        }

        [Test]
        public void UM29_Collection()
        {
            string code =
@"a = 5..20;indices = {1, 3, 5, 7};// create a collection via a collection of indicesb = a[indices];s = Print(a);s = Print(b);x = b[2];y = b[3];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10);
            thisTest.Verify("y", 12);
        }

        [Test]
        public void UM30_NumberTypes()
        {
            string code =
@"// create an integer with value exactly 1i = 1;// create a floating point number with approximate// value 1f = 1.0;s = Print(i);s = Print(f);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM31_NumberTypes()
        {
            string code =
@"// create a floating point number approximately 1.0f1 = 1.0;// attempt to create a floating point number beyond the// precision of DesignScript. This number will be// rounded. f1 and f2 become the same numberf2 = 1.000000000000000000001;s = Print(f1);s = Print(f2);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f1", 1.0);
            thisTest.Verify("f2", 1.0);
        }

        [Test]
        public void UM32_NumberTypes()
        {
            string code =
@"import(""DSCoreNodes.dll"");// create an indexindex = 1.0;// create a collectioncollection = 1..10;pass_value = collection[index];// floor converts a floating point value to an integer// this will correctly access a member of the collectionvalue = collection[Math.Floor(index)];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("pass_value", 2);
            thisTest.Verify("value", 2);
        }

        [Test]
        public void UM33_NumberTypes()
        {
            string code =
@"// create two seemingly different numbersa = 1.0;b = 0.9999999;// test if the two numbers are equals = Print(a == b ? ""Are equal"" : ""Are not equal"");x = a == b ? 1 : 0;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void UM34_NumberTypes()
        {
            string code =
@"// create two seemingly different numbersa = 100000000;b = 99999999;// test if the two numbers are equals = Print(a == b ? ""Are equal"" : ""Are not equal"");x = a == b ? 1 : 0;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM35_Associativity()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a variable with value 10x = 10;// create a Point at x = 10, y = 10, z = 0p = Point.ByCoordinates(x, 10, 0);// update x to be 20// this will implicitly cause an update to p, calling// p = Point.ByCoordinates(x, 10, 0) automaticallyx = 20;";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM36_Associativity()
        {
            string code =
@"import(""ProtoGeometry.dll"");x = 10;p = Point.ByCoordinates(x, 10, 0);// updating x to be a collection of 10 numbers causes// p to become a collection of 10 Pointsx = 1..10;";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM37_Functions()
        {
            string code =
@"import(""ProtoGeometry.dll"");p1 = Point.ByCoordinates(0, 0, 0);p2 = Point.ByCoordinates(10, 0, 0);l = Line.ByStartPointEndPoint(p1, p2);// extrude a line vertically to create a surfacesurf = l.ExtrudeAsSurface(8, Vector.ByCoordinates(0, 0,1));// Extract the corner points of the surfacecorner_1 = surf.PointAtParameters(0, 0);corner_2 = surf.PointAtParameters(1, 0);corner_3 = surf.PointAtParameters(1, 1);corner_4 = surf.PointAtParameters(0, 1);// connect opposite corner points to create diagonalsdiag_1 = Line.ByStartPointEndPoint(corner_1, corner_3);diag_2 = Line.ByStartPointEndPoint(corner_2, corner_4);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM38_Functions()
        {
            string code =
@"def getTimesTwo(arg){return = arg * 2;}times_two = getTimesTwo(10);s = Print(times_two);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("times_two", 20);
        }

        [Test]
        public void UM39_Functions()
        {
            string code =
@"def getGoldenRatio(){return = 1.61803399;}gr = getGoldenRatio();s = Print(gr);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("gr", 1.618034);
        }

        [Test]
        public void UM40_Functions()
        {
            string code =
@"def returnTwoNumbers(){return = {1, 2};}two_nums = returnTwoNumbers();x = two_nums[0];y = two_nums[1];s = Print(two_nums);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM41_Functions()
        {
            string code =
@"import(""ProtoGeometry.dll"");def makeDiagonal(surface){corner_1 = surface.PointAtParameters(0, 0);corner_2 = surface.PointAtParameters(1, 0);corner_3 = surface.PointAtParameters(1, 1);corner_4 = surface.PointAtParameters(0, 1);diag_1 = Line.ByStartPointEndPoint(corner_1,corner_3);diag_2 = Line.ByStartPointEndPoint(corner_2,corner_4);return = {diag_1, diag_2};}c = Cuboid.ByLengths(CoordinateSystem.Identity(),10, 20, 30);diags = makeDiagonal(c.Faces.SurfaceGeometry);";
            string str = "DNL-1467479 Regression : NullReferenceException when dot operator is called using replication for some typical classes ( cuboid )";
            thisTest.VerifyRunScriptSource(code, str);
        }

        [Test]
        public void UM42_Math()
        {
            string code =
@"import (""DSCoreNodes.dll"");
val = 0.5;f = Math.Floor(val);c = Math.Ceiling(val);r = Math.Round(val);r2 = Math.Round(val + 0.001);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f", 0);
            thisTest.Verify("c", 1);
            thisTest.Verify("r", 0.0);
            thisTest.Verify("r2", 1.0);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM43_Math()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");num_pts = 20;// get degree values from 0 to 360theta = 0..360..#num_pts;p = Point.ByCoordinates(Math.Cos(theta),Math.Sin(theta), 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM44_Math()
        {
            string code =
@"s = Print(7 % 2);s = Print(6 % 2);s = Print(10 % 3);s = Print(19 % 7);x = 7 % 2;y = 19 % 7;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 5);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM45_Curves()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");num_pts = 6;s = Math.Sin(0..360..#num_pts) * 4;pts = Point.ByCoordinates(1..30..#num_pts, s, 0);int_curve = BSplineCurve.ByPoints(pts);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM46_Curves()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");pts = Point.ByCoordinates(Math.Cos(0..350..#10),Math.Sin(0..350..#10), 0);// create an closed curvecrv = BSplineCurve.ByPoints(pts, true);// the same curve, if left open:crv2 = BSplineCurve.ByPoints(pts.Translate(5, 0, 0),false);";
            string str = "DNL-1467479 Regression : NullReferenceException when dot operator is called using replication for some typical classes ( cuboid )";
            thisTest.VerifyRunScriptSource(code, str);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM47_Curves()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");num_pts = 6;pts = Point.ByCoordinates(1..30..#num_pts,Math.Sin(0..360..#num_pts) * 4, 0);// a B-Spline curve with degree 1 is a polylinectrl_curve = BSplineCurve.ByControlVertices(pts, 1);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM48_Curves()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");num_pts = 6;pts = Point.ByCoordinates(1..30..#num_pts,Math.Sin(0..360..#num_pts) * 4, 0);// a B-Spline curve with degree 2 is smoothctrl_curve = BSplineCurve.ByControlVertices(pts, 2);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM49_Curves()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");num_pts = 6;pts = Point.ByCoordinates(1..30..#num_pts,Math.Sin(0..360..#num_pts) * 4, 0);def create_curve(pts : Point[], degree : int){return = BSplineCurve.ByControlVertices(pts,degree);}ctrl_crvs = create_curve(pts, 1..11);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM50_Curves()
        {
            string code =
@"import(""ProtoGeometry.dll"");pts_1 = {};pts_1[0] = Point.ByCoordinates(0, 0, 0);pts_1[1] = Point.ByCoordinates(1, 1, 0);pts_1[2] = Point.ByCoordinates(5, 0.2, 0);pts_1[3] = Point.ByCoordinates(9, -3, 0);pts_1[4] = Point.ByCoordinates(11, 2, 0);crv_1 = BSplineCurve.ByControlVertices(pts_1, 3);crv_1.Color = Color.ByARGB(255, 0, 0, 255);pts_2 = {};pts_2[0] = pts_1[4];end_dir = pts_1[3].DirectionTo(pts_1[4]);pts_2[1] = Point.ByCoordinates(pts_2[0].X + end_dir.X,pts_2[0].Y + end_dir.Y, pts_2[0].Z + end_dir.Z);pts_2[2] = Point.ByCoordinates(15, 1, 0);pts_2[3] = Point.ByCoordinates(18, -2, 0);pts_2[4] = Point.ByCoordinates(21, 0.5, 0);crv_2 = BSplineCurve.ByControlVertices(pts_2, 3);crv_2.Color = Color.ByARGB(255, 255, 0, 255);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM51_IDE()
        {
            string code =
@"import(""ProtoGeometry.dll"");geometry = {};geometry[0] = Point.ByCoordinates(4.5, 8.9, 110.0);geometry[1] = Point.ByCoordinates(34.0, 77.0, 90.3);geometry[2] = Line.ByStartPointEndPoint (geometry[0],geometry[1]);geometry[3] = Point.ByCoordinates(-1, -45, 200.1);geometry[4] = geometry[1].Translate(50, 0, 0);geometry[5] = Point.ByCoordinates(54, 21, 0.24);// Some comment...// Setting a breakpoint at or before this line can// assist finding the bugsubset = geometry[0..4..#4];subset_move = subset.Translate(100, 0, 0);";
            string str = "DNL-1467479 Regression : NullReferenceException when dot operator is called using replication for some typical classes ( cuboid )";
            thisTest.VerifyRunScriptSource(code, str);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM52_IDE()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");def complexFunction(){num = 2.34057;v1 = Math.Log(num);v2 = Math.Sqrt(num);return = { v1, v2 };}result = complexFunction();point = Point.ByCoordinates(result[0], result[1], 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM53_Translation()
        {
            string code =
@"import(""ProtoGeometry.dll"");// create a point at x = 1, y = 2, z = 3p = Point.ByCoordinates(1, 2, 3);// translate the point 10 units in the x direction,// -20 in y, and 50 in z// p’s now position is x = 11, y = -18, z = 53p = p.Translate(10, -20, 50);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM54_Transform()
        {
            string code =
@"import(""ProtoGeometry.dll"");cube = Cuboid.ByLengths(CoordinateSystem.Identity(),10, 10, 10);new_cs = CoordinateSystem.Identity();new_cs = new_cs.Rotate(25,Vector.ByCoordinates(1,0,0.5));// get the existing coordinate system of the cubeold_cs = CoordinateSystem.Identity();cube = cube.Transform(old_cs, new_cs);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM55_Scale()
        {
            string code =
@"import(""ProtoGeometry.dll"");cube = Cuboid.ByLengths(CoordinateSystem.Identity(),10, 10, 10);new_cs = CoordinateSystem.Identity();new_cs = new_cs.Scale(20);old_cs = CoordinateSystem.Identity();cube = cube.Transform(old_cs, new_cs);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM56_ShearedCS()
        {
            string code =
@"import(""ProtoGeometry.dll"");line = Line.ByStartPointEndPoint(Point.ByCoordinates(0,0, 0), Point.ByCoordinates(10, 10, 10));new_cs = CoordinateSystem.ByOriginVectors(Point.ByCoordinates(0, 0, 0),Vector.ByCoordinates(1, 1, 1),Vector.ByCoordinates(0, 1, 0), true);old_cs = CoordinateSystem.Identity();line = line.Transform(old_cs, new_cs);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM57_Imperative()
        {
            string code =
@"import(""ProtoGeometry.dll"");cuboids = [Imperative]{c = {};cs = CoordinateSystem.Identity();cs = cs.Translate(20, 0, 0);c[0] = Cuboid.ByLengths(cs, 10, 10, 10);cs = cs.Rotate(25, Vector.ByCoordinates(1, 1, 1));cs = cs.Translate(0, 20, 20);c[1] = Cuboid.ByLengths(cs, 10, 10, 10);cs = cs.Rotate(35, Vector.ByCoordinates(1, 1, 1));cs = cs.Translate(0, 20, 20);c[2] = Cuboid.ByLengths(cs, 10, 10, 10);cs = cs.Rotate(15, Vector.ByCoordinates(1, 1, 1));cs = cs.Translate(0, 20, 20);c[3] = Cuboid.ByLengths(cs, 10, 10, 10);return = c;}";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM58_Associative()
        {
            string code =
@"import(""ProtoGeometry.dll"");import(""DSCoreNodes.dll"");cuboids = [Imperative]{c = {};cs = CoordinateSystem.Identity();c[0] = Cuboid.ByLengths(cs, 10, 10, 10);cs = cs.Translate(50, 0, 0);c[1] = Cuboid.ByLengths(cs, 10, 10, 10);rot = [Associative] {arr = 0..10;sqrts = Math.Sqrt(arr);return = Average(sqrts) * 40;}cs = cs.Rotate(rot, Vector.ByCoordinates(1, 1, 1));cs = cs.Translate(0, 50, 0);c[2] = Cuboid.ByLengths(cs, 10, 10, 10);return = c;}";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM59_Boolean()
        {
            string code =
@"import(""ProtoGeometry.dll"");geometry = [Imperative]{if (true){return = Point.ByCoordinates(1, -4, 6);}else{return = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0),Point.ByCoordinates(10, -4, 6));}}";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM60_Boolean()
        {
            string code =
@"import(""ProtoGeometry.dll"");geometry = [Imperative]{// change true to falseif (false){return = Point.ByCoordinates(1, -4, 6);}else{return = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0),Point.ByCoordinates(10, -4, 6));}}";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM61_Boolean()
        {
            string code =
@"result = 10 < 30;s = Print(result);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM62_Boolean()
        {
            string code =
@"result = 15 <= 15;s = Print(result);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", true);
        }

        [Test]
        public void UM63_Boolean()
        {
            string code =
@"result = 99 != 99;s = Print(result);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", false);
        }

        [Test]
        public void UM64_Boolean()
        {
            string code =
@"result = true && false;s = Print(result);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", false);
        }

        [Test]
        public void UM65_Boolean()
        {
            string code =
@"result = true || false;s = Print(result);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", true);
        }

        [Test]
        public void UM66_Boolean()
        {
            string code =
@"result = !false;s = Print(result);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", true);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM67_Boolean()
        {
            string code =
@"import(""ProtoGeometry.dll"");def make_geometry(i){return = [Imperative]{// test if the input is divisible// by either 2 or 3. See ""Math""if (i % 2 == 0 || i % 3 == 0){return = Point.ByCoordinates(i, -4, 10);}else{return = Line.ByStartPointEndPoint(Point.ByCoordinates(4, 10, 0),Point.ByCoordinates(i, -4, 10));}}}g = make_geometry(0..20);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM68_Looping()
        {
            string code =
@"import(""ProtoGeometry.dll"");geometry = [Imperative]{x = 1;start = Point.ByCoordinates(0, 0, 0);end = Point.ByCoordinates(x, x, x);line = Line.ByStartPointEndPoint(start, end);while (line.Length < 10){x = x + 1;end = Point.ByCoordinates(x, x, x);line = Line.ByStartPointEndPoint(start, end);}return = line;}";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM69_Looping()
        {
            string code =
@"geometry = [Imperative]{collection = 0..10;for (i in collection){s = Print(i);}}";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM70_Replication()
        {
            string code =
@"import(""ProtoGeometry.dll"");x_vals = 0..10;y_vals = 0..10;p = Point.ByCoordinates(x_vals, y_vals, 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM71_Replication()
        {
            string code =
@"import(""ProtoGeometry.dll"");x_vals = 0..10;y_vals = 0..10;// apply replication guides to the two collectionsp = Point.ByCoordinates(x_vals<1>, y_vals<2>, 0);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM72_Replication()
        {
            string code =
@"import(""ProtoGeometry.dll"");x_vals = 0..10;y_vals = 0..10;p1 = Point.ByCoordinates(x_vals<1>, y_vals<2>, 0);// apply the replication guides with a swapped order// and set the points 14 units higherp2 = Point.ByCoordinates(x_vals<2>, y_vals<1>, 14);curve1 = BSplineCurve.ByPoints(Flatten(p1));curve2 = BSplineCurve.ByPoints(Flatten(p2));";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM73_Replication()
        {
            string code =
@"import(""ProtoGeometry.dll"");x_vals = 0..10;y_vals = 0..10;z_vals = 0..10;// generate a 3D matrix of pointsp = Point.ByCoordinates(x_vals<1>,y_vals<2>,z_vals<3>);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM74_ModifierStack()
        {
            string code =
@"import(""ProtoGeometry.dll"");height = 5;p1 = Point.ByCoordinates((0..10)<1>, (0..10)<2>, 0);p2 = Point.ByCoordinates((0..10)<1>, (0..10)<2>,height);l = Line.ByStartPointEndPoint(p1, p2);l[4][6] = l[4][6].Translate(0, 0, 10);new_cs = CoordinateSystem.Identity();new_cs = new_cs.Rotate(45, Vector.ByCoordinates(1,0,0));l[4][6] = l[4][6].Transform(l[4][6].Context, new_cs);height = 10;";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM75_JaggedCollection()
        {
            string code =
@"j = {};j[0] = 1;j[1] = {2, 3, 4};j[2] = 5;j[3] = { {6, 7}, { {8} } };j[4] = 9;s = Print(j);x = j[1][1];y = j[3][1][0][0];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
            thisTest.Verify("y", 8);

        }

        [Test]
        public void UM76_JaggedCollectionFAIL()
        {
            ////the following test case is supposed to fail
            //                        string code =
            //@"
            //j = {1, {2, 3, 4}, 5, {{6, 7}, {{8}}}, 9};
            //j_sum = [Imperative]
            //{
            //sum = 0;
            //for (i in 0..(Count(j) – 1))
            //{
            //// access the ""i""th element of j,
            //// and add it to sum
            //sum = sum + j[i];
            //}
            //return = sum;
            //}
            //s = Print(j_sum);
            //            ";
            //                        thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM77_JaggedCollection()
        {
            string code =
@"// generate a jagged collectionj = {1, {2, 3, 4}, 5, {{6, 7}, {{8}}}, 9};s = Print(j);s = Print( j[0] );s = Print( j[1][0] );s = Print( j[1][1] );s = Print( j[1][2] );s = Print( j[2] );s = Print( j[3][0][0] );s = Print( j[3][0][1] );s = Print( j[3][1][0][0] );s = Print( j[4] );x = j[1][2];y = j[3][1][0][0];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 4);
            thisTest.Verify("y", 8);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM78_Surface()
        {
            string code =
@"import(""ProtoGeometry.dll"");// generate a grid of pointspts = Point.ByCoordinates((0..10)<1>, (0..10)<2>, 0);// create several ""bumps""pts[5][5] = pts[5][5].Translate(0, 0, 1);pts[8][2] = pts[8][2].Translate(0, 0, 1);surf = BSplineSurface.ByPoints(pts);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM79_Surface()
        {
            string code =
@"import(""ProtoGeometry.dll"");pts = Point.ByCoordinates((0..10)<1>, (0..10)<2>, 0);pts[5][5] = pts[5][5].Translate(0, 0, 1);pts[8][2] = pts[8][2].Translate(0, 0, 1);// create a surface of degree 1 with straight segmentssurf = BSplineSurface.ByPoints(pts, 1, 1);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM80_Surface()
        {
            string code =
@"import(""ProtoGeometry.dll"");pts = Point.ByCoordinates((0..10)<1>, (0..10)<2>, 0);pts[5][5] = pts[5][5].Translate(0, 0, 1);pts[8][2] = pts[8][2].Translate(0, 0, 1);// create a surface of degree 3 with smooth segmentssurf = BSplineSurface.ByPoints(pts, 3, 3);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM81_Surface()
        {
            string code =
@"import(""ProtoGeometry.dll"");p1 = Point.ByCoordinates(0..10, 0, 0);p1[2] = p1[2].Translate(0, 0, 1);c1 = BSplineCurve.ByPoints(p1);p2 = Point.ByCoordinates(0..10, 5, 0);p2[7] = p2[7].Translate(0, 0, -1);c2 = BSplineCurve.ByPoints(p2);p3 = Point.ByCoordinates(0..10, 10, 0);p3[5] = p3[5].Translate(0, 0, 1);c3 = BSplineCurve.ByPoints(p3);loft = Surface.LoftFromCrossSections({c1, c2, c3});";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM82_Surface()
        {
            string code =
@"import(""ProtoGeometry.dll"");pts = Point.ByCoordinates(4, 0, 0..7);pts[1] = pts[1].Translate(-1, 0, 0);pts[5] = pts[5].Translate(1, 0, 0);crv = BSplineCurve.ByPoints(pts);axis_origin = Point.ByCoordinates(0, 0, 0);axis = Vector.ByCoordinates(0, 0, 1);surf = Surface.Revolve(crv, axis_origin, axis, 0, 360);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM83_Parameterization()
        {
            string code =
@"import(""ProtoGeometry.dll"");pts = Point.ByCoordinates(4, 0, 0..6);pts[1] = pts[1].Translate(2, 0 ,0);pts[5] = pts[5].Translate(-1, 0 ,0);crv = BSplineCurve.ByPoints(pts);pts_at_param = crv.PointAtParameter(0..1..#11);// draw Lines to help visualize the pointslines = Line.ByStartPointEndPoint(pts_at_param,Point.ByCoordinates(4, 6, 0));";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM84_Parameterization()
        {
            string code =
@"import(""ProtoGeometry.dll"");pts = Point.ByCoordinates(4, 0, 0..7);pts[1] = pts[1].Translate(-1, 0, 0);pts[5] = pts[5].Translate(1, 0, 0);crv = BSplineCurve.ByPoints(pts);axis_origin = Point.ByCoordinates(0, 0, 0);axis = Vector.ByCoordinates(0, 0, 1);surf = Surface.Revolve(crv, axis_origin, axis, 90, 140);cs = surf.CoordinateSystemAtParameters((0..1..#5)<1>, (0..1..#5)<2>);lines_start = cs.Origin;lines_end = cs.Origin.Translate(cs.ZAxis, 1);lines = Line.ByStartPointEndPoint(lines_start,lines_end);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM85_Intersection()
        {
            string code =
@"import(""ProtoGeometry.dll"");WCS = CoordinateSystem.Identity();p = Point.ByCoordinates((0..10)<1>, (0..10)<2>, 0);p[1][1] = p[1][1].Translate(0, 0, 2);p[8][1] = p[8][1].Translate(0, 0, 2);p[2][6] = p[2][6].Translate(0, 0, 2);surf = BSplineSurface.ByPoints(p, 3, 3);pl = Plane.ByOriginNormal(WCS.Origin.Translate(0, 0,0.5), WCS.ZAxis);// intersect surface, generating three closed curvescrvs = surf.Intersect(pl);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM86_Trim()
        {
            string code =
@"import(""ProtoGeometry.dll"");p = Point.ByCoordinates((0..10)<1>, (0..10)<2>, 0);p1[1][1] = p[1][1].Translate(0, 0, 2);p1[8][1] = p[8][1].Translate(0, 0, 2);p1[2][6] = p[2][6].Translate(0, 0, 2);surf1 = BSplineSurface.ByPoints(p1, 3, 3);tool = Point.ByCoordinates((-10..20)<1>,(-10..20)<2>,1);pick_point = Point.ByCoordinates(5, 5, 5);// trim the surfaces closest to pick_pointcrvs = surf1.Trim(tool, pick_point);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM87_SelectTrim()
        {
            string code =
@"import(""ProtoGeometry.dll"");def makeSurf(p){p1 = p.Translate(1, 0, 0);p2 = p.Translate(1, 1, 0);p3 = p.Translate(0, 1, 0);pts = { {p, p1}, {p3, p2} };return = BSplineSurface.ByPoints(pts);}surf = makeSurf(Point.ByCoordinates((-10..10..1.5)<1>,(-10..10..1.5)<2>, 0));tool = Cuboid.ByLengths(CoordinateSystem.WCS,5.5, 5.5, 5.5);st = Surface.SelectTrim(surf, tool, false);s = surf.SetVisibility(false);";
            string str = "DNL-1467479 Regression : NullReferenceException when dot operator is called using replication for some typical classes ( cuboid )";
            thisTest.VerifyRunScriptSource(code, str);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM88_GeometricBoolean()
        {
            string code =
@"import(""ProtoGeometry.dll"");s1 = Sphere.ByCenterPointRadius(CoordinateSystem.WCS.Origin, 6);s2 = Sphere.ByCenterPointRadius(CoordinateSystem.WCS.Origin.Translate(4, 0, 0), 6);combined = s1.Union(s2);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM89_GeometricBoolean()
        {
            string code =
@"import(""ProtoGeometry.dll"");s = Sphere.ByCenterPointRadius(CoordinateSystem.WCS.Origin, 6);tool = Sphere.ByCenterPointRadius(CoordinateSystem.WCS.Origin.Translate(10, 0, 0), 6);result = s.Difference(tool);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM90_GeometricBoolean()
        {
            string code =
@"import(""ProtoGeometry.dll"");s = Sphere.ByCenterPointRadius(CoordinateSystem.WCS.Origin, 6);tool = Sphere.ByCenterPointRadius(CoordinateSystem.WCS.Origin.Translate(10, 0, 0), 6);result = s.Intersect(tool);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM91_NonManifoldSolid()
        {
            string code =
@"import(""ProtoGeometry.dll"");c = Cuboid.ByLengths(CoordinateSystem.WCS, 4, 4, 4);p = Plane.ByOriginNormal(CoordinateSystem.WCS.Origin,CoordinateSystem.WCS.YAxis);// the false flag tells DesignScript to create non-// manifold geometry, instead of a regular Solid.non_manifold_solid = c.Slice(p, false);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void UM92_NonManifoldSolid()
        {
            string code =
@"import(""ProtoGeometry.dll"");WCS = CoordinateSystem.WCS;c = Cuboid.ByLengths(WCS, 5, 5, 5);nms = c.Slice(Plane.ByOriginNormal(WCS.Origin,WCS.XAxis), false);nms2 = nms.Slice(Plane.ByOriginNormal(WCS.Origin.Translate(1, 0, 0),Vector.ByCoordinates(1, 1, 1)), false);nms3 = nms2.Slice(Plane.ByOriginNormal(WCS.Origin.Translate(0, 0, 1),Vector.ByCoordinates(-0.1, -0.1, 1)), false);nms4 = nms3.Slice(Plane.ByOriginNormal(WCS.Origin.Translate(0, 0, -1),Vector.ByCoordinates(-0.1, 0.1, 1)), false);cells =  Flatten(nms4.Cells)[2].AdjacentCells.SolidGeometry.Translate(0, 0, 14);";
            string str = "DNL-1467479 Regression : NullReferenceException when dot operator is called using replication for some typical classes ( cuboid )";
            thisTest.VerifyRunScriptSource(code, str);
        }
    }
}
