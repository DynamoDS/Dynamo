using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    public static class ApproximateAssertExtensions
    {
        public const double Epsilon = 1e-6;

        public static bool ShouldBeApproximately(this Point p0, Point p1, double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Vector p0, Vector p1, double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Vector p1, double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Vector p0, Autodesk.Revit.DB.XYZ p1, double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Point p0, Autodesk.Revit.DB.XYZ p1, double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Autodesk.Revit.DB.XYZ p1,
            double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Point p1, double epsilon = Epsilon)
        {
            return p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static bool ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, double x, double y, double z,
            double epsilon = Epsilon)
        {
            return ShouldBeApproximately(p0.X, p0.Y, p0.Z, x, y, z, epsilon);
        }

        public static bool ShouldBeApproximately(this Vector p0, double x, double y, double z, double epsilon = Epsilon)
        {
            return ShouldBeApproximately(p0.X, p0.Y, p0.Z, x, y, z, epsilon);
        }

        public static bool ShouldBeApproximately(this Point p0, double x, double y, double z, double epsilon = Epsilon)
        {
            return ShouldBeApproximately(p0.X, p0.Y, p0.Z, x, y, z, epsilon);
        }

        private static bool ShouldBeApproximately(
            double x0, double y0, double z0, double x, double y, double z, double epsilon)
        {
            return x0.ShouldBeApproximately(x, epsilon) && y0.ShouldBeApproximately(y, epsilon)
                && z0.ShouldBeApproximately(z, epsilon);
        }

        public static bool ShouldBeApproximately(this Double d0, Double d1, double epsilon = Epsilon)
        {
            return Math.Abs(d0 - d1) <= epsilon;
        }

        public static void AssertShouldBeApproximately(this Point p0, Point p1, double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Vector p0, Vector p1, double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Vector p1, double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Vector p0, Autodesk.Revit.DB.XYZ p1, double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Point p0, Autodesk.Revit.DB.XYZ p1, double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Autodesk.Revit.DB.XYZ p1,
            double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Point p1, double epsilon = Epsilon)
        {
            p0.AssertShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void AssertShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, double x, double y, double z,
            double epsilon = Epsilon)
        {
            Assert.AreEqual(x, p0.X, epsilon, "X property does not match");
            Assert.AreEqual(y, p0.Y, epsilon, "Y property does not match");
            Assert.AreEqual(z, p0.Z, epsilon, "Z property does not match");
        }

        public static void AssertShouldBeApproximately(this Vector p0, double x, double y, double z, double epsilon = Epsilon)
        {
            Assert.AreEqual(x, p0.X, epsilon, "X property does not match");
            Assert.AreEqual(y, p0.Y, epsilon, "Y property does not match");
            Assert.AreEqual(z, p0.Z, epsilon, "Z property does not match");
        }

        public static void AssertShouldBeApproximately(this Point p0, double x, double y, double z, double epsilon = Epsilon)
        {
            Assert.AreEqual(x, p0.X, epsilon, "X property does not match");
            Assert.AreEqual(y, p0.Y, epsilon, "Y property does not match");
            Assert.AreEqual(z, p0.Z, epsilon, "Z property does not match");
        }

        public static void AssertShouldBeApproximately(this Double d0, Double d1, double epsilon = Epsilon)
        {
            Assert.AreEqual(d1, d0, epsilon);
        }
    }
}
