using Autodesk.DesignScript.Geometry;
using Dynamo;
using DynamoUnits;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TestServices;

namespace GeometryTests
{


    [TestFixture]
    internal class GeometryImportNodeTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("BuiltIn.ds");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DynamoConversions.dll");
            libraries.Add("DynamoUnits.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("GeometryColor.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("Failure")]
        public void SATAndSABUnitImportNodeModelNodesWork_PartialApplication_Replication()
        {
            OpenModel(Path.Combine("core", "GeometryTestFiles", "sat_import_with_units.dyn"));
            RunCurrentModel();
            AssertNoDummyNodes();
            Assert.AreEqual(21, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            var output = CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.Name == "Math.Sum").FirstOrDefault();
            AssertPreviewValue(output.GUID.ToString(), 217.133880);
        }
    }

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
            var surface = ImportHelpers.ImportFromSATWithUnits(satpath, null).FirstOrDefault() as Autodesk.DesignScript.Geometry.Surface;
            Assert.NotNull(surface);
            ShouldBeApproximate(surface.Area, 1.938695, .000001);
        }
        [Test]
        public void SATImportWithSameUnits_ImportsAtSameSize()
        {
            var satpath = Path.Combine(TestDirectory, "core\\WorkflowTestFiles\\GeometryDefects\\SweepAsSolid", "profile.sat");
            const string ft = "autodesk.unit.unit:feet";
            var ftunit = Unit.ByTypeID($"{ft}-1.0.1");
            var surface = ImportHelpers.ImportFromSATWithUnits(satpath, ftunit).FirstOrDefault() as Autodesk.DesignScript.Geometry.Surface;
            Assert.NotNull(surface);
            ShouldBeApproximate(surface.Area, 1.938695, .000001);
        }
        [Test]
        public void SATImportWithSmallerUnits_ImportsAsLargerSize()
        {
            var satpath = Path.Combine(TestDirectory, "core\\WorkflowTestFiles\\GeometryDefects\\SweepAsSolid", "profile.sat");
            const string inch = "autodesk.unit.unit:inches";
            var inunit = Unit.ByTypeID($"{inch}-1.0.0");
            var surface = ImportHelpers.ImportFromSATWithUnits(satpath, inunit).FirstOrDefault() as Autodesk.DesignScript.Geometry.Surface;
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
            Assert.Throws<Exception>(()=>{
                var surface = ImportHelpers.ImportFromSATWithUnits(satpath, lbunit) as Autodesk.DesignScript.Geometry.Surface;
            });
        }
        [Test]
        public void SABImportWithNullImportsAsUnitless()
        {
            var cube = Cuboid.ByLengths(3, 4, 5);
         
            //current implementation serializes as meters
            var sab = Geometry.SerializeAsSAB(new Geometry[] { cube });
            var cube2 = ImportHelpers.DeserializeFromSABWithUnits(sab, null).FirstOrDefault();
            ShouldBeApproximate((cube2 as Solid).Volume, 60, .000001);
        }
        [Test]
        public void SABImportWithSameUnits_ImportsAtSameSize()
        {
            var cube = Cuboid.ByLengths(3, 4, 5);
            const string meters = "autodesk.unit.unit:meters";
            var metersUnit = Unit.ByTypeID($"{meters}-1.0.1");
            //current implementation serializes as meters
            var sab = Geometry.SerializeAsSAB(new Geometry[] { cube });
            var cube2 = ImportHelpers.DeserializeFromSABWithUnits(sab, metersUnit).FirstOrDefault();
            ShouldBeApproximate((cube2 as Solid).Volume, 60,.000001);
        }

        [Test]
        public void SABImportWithSmallerUnits_ImportsAsLargerSize()
        {
            var cube = Cuboid.ByLengths(3, 4, 5);
            const string millimeters = "autodesk.unit.unit:millimeters";
            var milliUnit = Unit.ByTypeID($"{millimeters}-1.0.1");
            //current implementation serializes as meters
            var sab = Geometry.SerializeAsSAB(new Geometry[] { cube });
            var cube2 = ImportHelpers.DeserializeFromSABWithUnits(sab, milliUnit).FirstOrDefault();
            ShouldBeApproximate((cube2 as Solid).Volume, 60.0*1000*1000*1000, .000001);
        }
        [Test]
        public void SABImportWithWrongUnitType_Throws()
        {
            var cube = Cuboid.ByLengths(3, 4, 5);
            const string lb = "autodesk.unit.unit:poundsMass";
            var lbunit = Unit.ByTypeID($"{lb}-1.0.0");
            //current implementation serializes as meters
            var sab = Geometry.SerializeAsSAB(new Geometry[] { cube });
            Assert.Throws<Exception>(() => {
                var cube2 = ImportHelpers.DeserializeFromSABWithUnits(sab, lbunit).FirstOrDefault();
            });
        }

    }
}
