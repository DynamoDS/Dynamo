using Autodesk.DesignScript.Geometry;
using DynamoUnits;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TestServices;

namespace GeometryTests
{
    [TestFixture]
    internal class GeometryImportHelperTests:GeometricTestBase
    {
        internal static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }
        internal static void ShouldBeApproximate(double x, double y, double epsilon)
        {
            Assert.AreEqual(x, y, epsilon);
        }
        internal string TestDirectory => GetTestDirectory(new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName);

        [Test]
        public void SATImportWithNullImportsAsUnitless()
        {
            var satpath = Path.Combine(TestDirectory, "core\\WorkflowTestFiles\\GeometryDefects\\SweepAsSolid", "profile.sat");
            var surface = ImportHelpers.ImportFromSATByUnits(satpath, null).FirstOrDefault() as Autodesk.DesignScript.Geometry.Surface;
            Assert.NotNull(surface);
            ShouldBeApproximate(surface.Area, 1.938695, .000001);
        }
        [Test]
        public void SATImportWithSameUnits_ImportsAtSameSize()
        {
            var satpath = Path.Combine(TestDirectory, "core\\WorkflowTestFiles\\GeometryDefects\\SweepAsSolid", "profile.sat");
            const string ft = "autodesk.unit.unit:feet";
            var ftunit = Unit.ByTypeID($"{ft}-1.0.1");
            var surface = ImportHelpers.ImportFromSATByUnits(satpath, ftunit).FirstOrDefault() as Autodesk.DesignScript.Geometry.Surface;
            Assert.NotNull(surface);
            ShouldBeApproximate(surface.Area, 1.938695, .000001);
        }
        [Test]
        public void SATImportWithSmallerUnits_ImportsAsLargerSize()
        {
            var satpath = Path.Combine(TestDirectory, "core\\WorkflowTestFiles\\GeometryDefects\\SweepAsSolid", "profile.sat");
            const string inch = "autodesk.unit.unit:inches";
            var inunit = Unit.ByTypeID($"{inch}-1.0.0");
            var surface = ImportHelpers.ImportFromSATByUnits(satpath, inunit).FirstOrDefault() as Autodesk.DesignScript.Geometry.Surface;
            Assert.NotNull(surface);
            //feet in the sat file are converted to inches, which means the resulting object is much larger.
            ShouldBeApproximate(surface.Area, 279.172131, .000001);
        }
        [Test]
        public void SATImportWithWrongUnitType_Throws()
        {
            var satpath = Path.Combine(TestDirectory, "core\\WorkflowTestFiles\\GeometryDefects\\SweepAsSolid", "profile.sat");
            const string lb = "autodesk.unit.unit:poundsMass";
            var lbunit = Unit.ByTypeID($"{lb}-1.0.0");
            Assert.Throws<Exception>((TestDelegate)(()=>{
                var surface = ImportHelpers.ImportFromSATByUnits(satpath, lbunit) as Autodesk.DesignScript.Geometry.Surface;
            }));
        }

    }
}
