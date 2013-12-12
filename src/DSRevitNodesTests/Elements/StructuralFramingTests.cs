using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes.AnalysisDisplay;
using DSRevitNodes.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class StructuralFramingTests
    {
        [Test]
        public void ByCurveLevelUpVectorAndType_ValidArgs()
        {
            var start = Point.ByCoordinates(1, 2, 3);
            var end = Point.ByCoordinates(5, 8, 3);
            var line = Line.ByStartPointEndPoint(start, end);
            
            var level = DSLevel.ByElevation(3);
            var up = Vector.ByCoordinates(0, 0, 1);

            var structuralType = DSStructuralType.Beam;
            var famSym = DSFamilySymbol.ByName("W12X26");

            var structure = DSStructuralFraming.ByCurveLevelUpVectorAndType(line, level, up, structuralType, famSym);

            Assert.NotNull(structure);
            Assert.NotNull(structure.InternalElement);
            Assert.IsTrue( DocumentManager.GetInstance().ElementExistsInDocument(structure.InternalElement.Id )) ;
        }

        [Test]
        public void ByCurveLevelUpVectorAndType_BadArgs()
        {
            var start = Point.ByCoordinates(1, 2, 3);
            var end = Point.ByCoordinates(5, 8, 3);
            var line = Line.ByStartPointEndPoint(start, end);

            var level = DSLevel.ByElevation(3);
            var up = Vector.ByCoordinates(0, 0, 1);

            var structuralType = DSStructuralType.Beam;
            var famSym = DSFamilySymbol.ByName("W12X26");

            Assert.Throws(typeof(System.ArgumentNullException), () => DSStructuralFraming.ByCurveLevelUpVectorAndType(null, level, up, structuralType, famSym));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSStructuralFraming.ByCurveLevelUpVectorAndType(line, null, up, structuralType, famSym));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSStructuralFraming.ByCurveLevelUpVectorAndType(line, level, null, structuralType, famSym));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSStructuralFraming.ByCurveLevelUpVectorAndType(line, level, up, structuralType, null));
        }

    }
}
