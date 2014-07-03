using System;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    public static class ApproximateAssertExtensions
    {
        public const double Epsilon = 1e-6;

        public static void ShouldBeApproximately(this Point p0, Point p1, double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Vector p0, Vector p1, double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Vector p1, double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Vector p0, Autodesk.Revit.DB.XYZ p1, double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Point p0, Autodesk.Revit.DB.XYZ p1, double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Autodesk.Revit.DB.XYZ p1,
            double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, Point p1, double epsilon = Epsilon)
        {
            p0.ShouldBeApproximately(p1.X, p1.Y, p1.Z);
        }

        public static void ShouldBeApproximately(this Autodesk.Revit.DB.XYZ p0, double x, double y, double z,
            double epsilon = Epsilon)
        {
            Assert.AreEqual(x, p0.X, epsilon, "X property does not match");
            Assert.AreEqual(y, p0.Y, epsilon, "Y property does not match");
            Assert.AreEqual(z, p0.Z, epsilon, "Z property does not match");
        }

        public static void ShouldBeApproximately(this Vector p0, double x, double y, double z, double epsilon = Epsilon)
        {
            Assert.AreEqual(x, p0.X, epsilon, "X property does not match");
            Assert.AreEqual(y, p0.Y, epsilon, "Y property does not match");
            Assert.AreEqual(z, p0.Z, epsilon, "Z property does not match");
        }

        public static void ShouldBeApproximately(this Point p0, double x, double y, double z, double epsilon = Epsilon)
        {
            Assert.AreEqual(x, p0.X, epsilon, "X property does not match");
            Assert.AreEqual(y, p0.Y, epsilon, "Y property does not match");
            Assert.AreEqual(z, p0.Z, epsilon, "Z property does not match");
        }

        public static void ShouldBeApproximately(this Double d0, Double d1, double epsilon = Epsilon)
        {
            Assert.AreEqual(d1, d0, epsilon);
        }
    }
}
