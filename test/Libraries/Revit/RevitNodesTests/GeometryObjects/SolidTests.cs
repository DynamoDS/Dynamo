using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Revit.Elements;
using Revit.GeometryConversion;
using NUnit.Framework;
using Dynamo.DSEngine;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Solid = Revit.GeometryObjects.Solid;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    public class SolidTests
    {
        private string geomDir;

        public string TestGeometryDirectory
        {
            get
            {
                var assDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName);
                geomDir = Path.Combine(assDir, @"test_geometry");
                if (!Directory.Exists(geomDir))
                {
                    Directory.CreateDirectory(geomDir);
                }
                return geomDir;
            }    
        }

        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

        [Test]
        public void ByExtrusion_ValidArgs()
        {
            var crvs = UnitRectangle();

            var dir = Vector.ByCoordinates(0, 0, 1);
            var dist = 5;
            var extrusion = Solid.ByExtrusion(crvs.ToArray(), dir, dist);

            Assert.NotNull(extrusion);
            Assert.AreEqual(5, extrusion.Volume, 0.01);
            Assert.AreEqual(22, extrusion.SurfaceArea, 0.01);
        }

        [Test]
        public void ByRevolve_ValidArgs()
        {
            //create a unit rectangle in the world XY plane
            var crvs = UnitRectangle();

            var origin = Point.ByCoordinates(0, 0, 0);
            var x = Vector.ByCoordinates(1, 0, 0);
            var y = Vector.ByCoordinates(0, 1, 0);
            var z = Vector.ByCoordinates(0, 0, 1);

            var cs = CoordinateSystem.ByOriginVectors(origin, x, y, z);

            var planeCs = CoordinateSystem.ByOriginVectors(origin, x, z, y);
            var transCrvs = crvs.Select(crv => crv.Transform(CoordinateSystem.Identity(), planeCs)).Cast<Curve>().ToList();

            var revolve = Solid.ByRevolve(PolyCurve.ByJoinedCurves(transCrvs.ToArray()), cs, 0, 3.14);
            Assert.NotNull(revolve);

            var package = new RenderPackage(); 
            revolve.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"ByRevolve_ValidArgs.obj");
            if(File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>(){package}); 
        }

        [Test]
        public void ByRevolve_ValidArgs_NonVertical()
        {
            //create a unit rectangle in the world XY plane
            var crvs = UnitRectangle();

            var origin = Point.ByCoordinates(0, 0, 0);

            var z = Vector.ByCoordinates(.5, .5, .5).Normalized();
            var zTmp = Vector.ByCoordinates(0, 0, 1);
            var x = z.Cross(zTmp).Normalized();
            var y = z.Cross(x).Normalized();

            var cs = CoordinateSystem.ByOriginVectors(origin, x, y, z);

            var planeCs = CoordinateSystem.ByOriginVectors(origin, x, z, y);
            var transCrvs = crvs.Select(crv => crv.Transform(CoordinateSystem.Identity(), planeCs)).Cast<Curve>().ToList();

            var revolve = Solid.ByRevolve(PolyCurve.ByJoinedCurves(transCrvs.ToArray()), cs, 0, 3.14);
            Assert.NotNull(revolve);

            var package = new RenderPackage();
            revolve.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"ByRevolve_ValidArgs_NonVertical.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void ByBlend_ValidArgs()
        {
            var rect1 = UnitRectangle();
            var rect2 = UnitRectangle();

            var originBottom = Point.ByCoordinates(0, 0, 0);
            var x = Vector.ByCoordinates(1, 0, 0);
            var y = Vector.ByCoordinates(0, 1, 0);
            var z = Vector.ByCoordinates(0, 0, 1);

            var originTop = Point.ByCoordinates(0, 0, 10);
            var x1 = Vector.ByCoordinates(.5, .5, 0).Normalized();
            var y1 = x1.Cross(z); 

            var csBottom = CoordinateSystem.ByOriginVectors(originBottom, x, y, z);
            var csTop = CoordinateSystem.ByOriginVectors(originTop, x1, y1, z);

            var bottCurves = rect1.Select(crv => crv.Transform(CoordinateSystem.Identity(), csBottom)).Cast<Curve>().ToList();
            var topCurves = rect2.Select(crv => crv.Transform(CoordinateSystem.Identity(), csTop)).Cast<Curve>().ToList();

            var blend = Solid.ByBlend(new List<PolyCurve> { PolyCurve.ByJoinedCurves(bottCurves.ToArray()), 
                PolyCurve.ByJoinedCurves(topCurves.ToArray()) });
            Assert.NotNull(blend);

            var package = new RenderPackage();
            blend.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"ByBlend_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void BySweptBlend_ValidArgs()
        {
            var package = new RenderPackage();

            var rect1 = UnitRectangle();

            //make the path curve
            var p1 = Point.ByCoordinates(0, 0, 0);
            var p5 = Point.ByCoordinates(0, 0, 4);

            var spine = Line.ByStartPointEndPoint(p1, p5);
            var spineRev = spine.ToRevitType();


            DrawCurve(spine.ToRevitType(), package);

            var cs1 = rect1.Select(crv => crv.Translate(0,0,0)).Cast<Curve>().ToList();
            var cs2 = rect1.Select(crv => crv.Translate(0,0,1)).Cast<Curve>().ToList();
            var cs3 = rect1.Select(crv => crv.Translate(0,0,3)).Cast<Curve>().ToList();
            var cs4 = rect1.Select(crv => crv.Translate(0, 0, 4)).Cast<Curve>().ToList();

            cs1.ForEach(x => DrawCurve(x.ToRevitType(), package));
            cs2.ForEach(x => DrawCurve(x.ToRevitType(), package));
            cs3.ForEach(x => DrawCurve(x.ToRevitType(), package));
            cs4.ForEach(x => DrawCurve(x.ToRevitType(), package));

            cs1[0].StartPoint.ShouldBeApproximately(0, 0, 0);
            cs2[0].StartPoint.ShouldBeApproximately(0, 0, 1);
            cs3[0].StartPoint.ShouldBeApproximately(0, 0, 3);
            cs4[0].StartPoint.ShouldBeApproximately(0, 0, 4);

            spine.EndPoint.ShouldBeApproximately(Point.ByCoordinates(0, 0, 4));
            spine.StartPoint.ShouldBeApproximately(Point.ByCoordinates(0, 0, 0));

            var modelPath = Path.Combine(TestGeometryDirectory, @"BySweptBlend_ValidArgs_Setup.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });


            var crvList = new[] {cs1, cs2, cs3, cs4};

            var blend = Solid.BySweptBlend( crvList.Select(x => PolyCurve.ByJoinedCurves(x.ToArray()) ).ToList(), spine, new List<double> {0, 0.25, 0.75, 1});
            Assert.NotNull(blend);

            blend.Tessellate(package);

            modelPath = Path.Combine(TestGeometryDirectory, @"BySweptBlend_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void Cylinder_ValidArgs()
        {
            var cylinder = Solid.Cylinder(Point.ByCoordinates(0, 0, 0), 5, Vector.ByCoordinates(0,0,1), 10);
            Assert.IsNotNull(cylinder);

            var package = new RenderPackage();
            cylinder.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"Cylinder_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });

        }

        [Test]
        public void Sphere_ValidArgs()
        {
            var sphere = Solid.Sphere(Point.ByCoordinates(0, 5, 3), 10);
            Assert.IsNotNull(sphere);

            var package = new RenderPackage();
            sphere.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"Sphere_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void Torus_ValidArgs()
        {
            var axis = Vector.ByCoordinates(.5, .5, .5);
            var center = Point.ByCoordinates(2, 3, 5);

            var torus = Solid.Torus(axis, center, 3, 1);
            Assert.IsNotNull(torus);

            var package = new RenderPackage();
            torus.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"Torus_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void BoxByMinimumMaximum_ValidArgs()
        {
            var min = Point.ByCoordinates(-2, -1, 5);
            var max = Point.ByCoordinates(3, 5, 10);

            var box = Solid.BoxByTwoCorners(min, max);
            Assert.IsNotNull(box);

            var package = new RenderPackage();
            box.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"BoxByMinimumMaximum_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void BoxByCenterAndDimensions_ValidArgs()
        {
            var center = Point.ByCoordinates(-2, -1, 5);

            var box = Solid.BoxByCenterAndDimensions(center, 2, 5, 10);
            Assert.IsNotNull(box);

            var package = new RenderPackage();
            box.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"BoxByCenterAndDimensions_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void ByBooleanUnion_ValidArgs()
        {
            var solidA = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 1, 1, 1);
            var solidB = Solid.Sphere(Point.ByCoordinates(1, 1, 0), 1);
            var union = Solid.ByBooleanUnion(solidA, solidB);
            Assert.IsNotNull(union);

            var package = new RenderPackage();
            union.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"ByBooleanUnion_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<RenderPackage>() { package });
        }

        [Test]
        public void ByBooleanIntersect_ValidArgs()
        {
            var solidA = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 1, 1, 1);
            var solidB = Solid.Sphere(Point.ByCoordinates(1, 1, 0), 1);
            var xSect = Solid.ByBooleanIntersection(solidA, solidB);
            Assert.IsNotNull(xSect);

            var package = new RenderPackage();
            xSect.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"ByBooleanIntersect_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<IRenderPackage>() { package });
        }

        [Test]
        public void ByBooleanDifference_ValidArgs()
        {
            var solidA = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 1, 1, 1);
            var solidB = Solid.Sphere(Point.ByCoordinates(1, 1, 0), 1);
            var difference = Solid.ByBooleanDifference(solidA, solidB);
            Assert.IsNotNull(difference);

            var package = new RenderPackage();
            difference.Tessellate(package);

            var modelPath = Path.Combine(TestGeometryDirectory, @"ByBooleanDifference_ValidArgs.obj");
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<IRenderPackage>() { package });
        }

        [Test]
        public void FromElement_ValidArgs()
        {
            var solidA = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 1, 1, 1);
            var solidB = Solid.Sphere(Point.ByCoordinates(1, 1, 0), 1);
            var difference = Solid.ByBooleanDifference(solidA, solidB);

            var ff = FreeForm.BySolid(difference);

            var extract = Solid.FromElement(ff);
            Assert.IsNotNull(extract);

            var package = new RenderPackage();
            extract.Tessellate(package);

            ExportModel("FromElement_ValidArgs.obj", package);
        }

        private void ExportModel(string fileName, IRenderPackage package)
        {
            var modelPath = Path.Combine(TestGeometryDirectory, fileName);
            if (File.Exists(modelPath))
                File.Delete(modelPath);
            WriteToOBJ(modelPath, new List<IRenderPackage>() { package });
        }

        private static List<Curve> UnitRectangle()
        {
            // construct a unit rectangle
            var l1 = Line.ByStartPointEndPoint(
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(1, 0, 0)
            );

            var l2 = Line.ByStartPointEndPoint(
                Point.ByCoordinates(1, 0, 0),
                Point.ByCoordinates(1, 1, 0)
            );

            var l3 = Line.ByStartPointEndPoint(
                Point.ByCoordinates(1, 1, 0),
                Point.ByCoordinates(0.0, 1, 0)
            );

            var l4 = Line.ByStartPointEndPoint(
                Point.ByCoordinates(0.0, 1, 0),
                Point.ByCoordinates(0.0, 0, 0)
            );

            var crvs = new List<Curve>{l1,l2,l3,l4};
            return crvs;
        }

        private static void WriteToOBJ(string path, IEnumerable<IRenderPackage> packages)
        {
            using (TextWriter tw = new StreamWriter(path))
            {
                foreach (var package in packages)
                {
                    int vertCount = 0;

                    for (int i = 0; i < package.TriangleVertices.Count; i += 3)
                    {
                        tw.WriteLine(string.Format("v {0} {1} {2}", package.TriangleVertices[i], package.TriangleVertices[i+1], package.TriangleVertices[i+2]));
                        vertCount++;
                    }

                    for (int i = 0; i < package.LineStripVertices.Count - 3; i+=3)
                    {
                        var a = Point.ByCoordinates(package.LineStripVertices[i], package.LineStripVertices[i + 1], package.LineStripVertices[i + 2]);
                        var b = Point.ByCoordinates(package.LineStripVertices[i + 3], package.LineStripVertices[i + 4], package.LineStripVertices[i + 5]);
                        var v1 = Vector.ByCoordinates(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
                        var vNorm = v1.Cross(Vector.ByCoordinates(0, 0, 1));

                        Point c;
                        Point d;

                        if (vNorm.Length > 1e-8)
                        {
                            c = (Point)b.Translate(vNorm, .025);
                            d = (Point)a.Translate(vNorm, .025);
                        }
                        else
                        {
                            c = b;
                            d = a;
                        }

                        tw.WriteLine(string.Format("v {0} {1} {2}", a.X, a.Y, a.Z));
                        tw.WriteLine(string.Format("v {0} {1} {2}", b.X, b.Y, c.Z));
                        tw.WriteLine(string.Format("v {0} {1} {2}", c.X, c.Y, c.Z));

                        tw.WriteLine(string.Format("v {0} {1} {2}", c.X, c.Y, c.Z));
                        tw.WriteLine(string.Format("v {0} {1} {2}", d.X, d.Y, d.Z));
                        tw.WriteLine(string.Format("v {0} {1} {2}", a.X, a.Y, a.Z));

                        vertCount += 6;
                    }

                    for (int i = 0; i < vertCount; i +=3)
                    {
                        tw.WriteLine(string.Format("f {0} {1} {2}", i+1, i + 2, i + 3));
                    }
                }
            }
        }

        private static void DrawCurve(Autodesk.Revit.DB.Curve curve, IRenderPackage package)
        {
            var pts = curve.Tessellate().ToList();

            for (int i =0; i < pts.Count-1; i++)
            {
                var pt = pts[i];
                var pt1 = pts[i+1];
                package.PushLineStripVertex(pt.X, pt.Y, pt.Z);
                package.PushLineStripVertex(pt1.X, pt1.Y, pt1.Z);
            }
        }

        private static void DrawCS(CoordinateSystem cs, IRenderPackage package)
        {
            //ccw unit rect points
            var a = Point.ByCoordinates(-.25, -.25, 0);
            var b = Point.ByCoordinates(.25, -.25, 0);
            var c = Point.ByCoordinates(.25, .25, 0);
            var d = Point.ByCoordinates(-.25, .25, 0);

            var planes = new List<Plane>() {cs.XYPlane, cs.YZPlane, cs.ZXPlane};
            foreach (var p in planes)
            {
                var z = p.Normal.Normalized();
                var y = z.IsParallel(Vector.ByCoordinates(0,0,1)) ? Vector.ByCoordinates(0,1,0) : 
                    z.Cross(Vector.ByCoordinates(0, 0, 1)).Normalized();
                var x = z.Cross(y).Normalized();

                var newCS = CoordinateSystem.ByOriginVectors(cs.Origin, x,y,z);

                var pA = (Point)a.Transform(CoordinateSystem.Identity(), newCS);
                var pB = (Point)b.Transform(CoordinateSystem.Identity(), newCS);
                var pC = (Point)c.Transform(CoordinateSystem.Identity(), newCS);
                var pD = (Point)d.Transform(CoordinateSystem.Identity(), newCS);

                package.PushTriangleVertex(pA.X, pA.Y, pA.Z);
                package.PushTriangleVertex(pB.X, pB.Y, pB.Z);
                package.PushTriangleVertex(pC.X, pC.Y, pC.Z);

                package.PushTriangleVertex(pC.X, pC.Y, pC.Z);
                package.PushTriangleVertex(pD.X, pD.Y, pD.Z);
                package.PushTriangleVertex(pA.X, pA.Y, pA.Z);
            }
        }
    }
}
