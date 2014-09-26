using System.Linq;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;

namespace RevitTestServices.Elements
{
    [TestFixture]
    public class StructuralFramingTests : GeometricRevitNodeTest
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCurveLevelUpVectorAndType_ValidArgs()
        {
            var start = Point.ByCoordinates(1, 2, 3);
            var end = Point.ByCoordinates(5, 8, 3);
            var line = Line.ByStartPointEndPoint(start, end);
            
            var level = Level.ByElevation(3);
            var up = Vector.ByCoordinates(0, 0, 1);

            var structuralType = StructuralType.Beam;
            var famSym = FamilySymbol.ByName("W12X26");

            var structure = StructuralFraming.ByCurveLevelUpVectorAndType(line, level, up, structuralType, famSym);

            Assert.NotNull(structure);
            Assert.NotNull(structure.InternalElement);
            Assert.IsTrue( DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID(structure.InternalElement.UniqueId) )) ;
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCurveLevelUpVectorAndType_BadArgs()
        {
            var start = Point.ByCoordinates(1, 2, 3);
            var end = Point.ByCoordinates(5, 8, 3);
            var line = Line.ByStartPointEndPoint(start, end);

            var level = Level.ByElevation(3);
            var up = Vector.ByCoordinates(0, 0, 1);

            var structuralType = StructuralType.Beam;
            var famSym = FamilySymbol.ByName("W12X26");

            Assert.Throws(typeof(System.ArgumentNullException), () => StructuralFraming.ByCurveLevelUpVectorAndType(null, level, up, structuralType, famSym));
            Assert.Throws(typeof(System.ArgumentNullException), () => StructuralFraming.ByCurveLevelUpVectorAndType(line, null, up, structuralType, famSym));
            Assert.Throws(typeof(System.ArgumentNullException), () => StructuralFraming.ByCurveLevelUpVectorAndType(line, level, null, structuralType, famSym));
            Assert.Throws(typeof(System.ArgumentNullException), () => StructuralFraming.ByCurveLevelUpVectorAndType(line, level, up, structuralType, null));
        }

        [Test]
        [TestModel(@".\StructuralFramingLocationTest.rvt")]
        public void TestLocationOfStructuralFraming()
        {
            var e = ElementSelector.ByType<Autodesk.Revit.DB.FamilyInstance>(true).FirstOrDefault();
            Assert.NotNull(e);

            var beam = e as StructuralFraming;
            Assert.NotNull(beam);
            var curve = beam.Location;
            Assert.NotNull(curve.StartPoint);
            Assert.NotNull(curve.EndPoint);
        }
    }
}
