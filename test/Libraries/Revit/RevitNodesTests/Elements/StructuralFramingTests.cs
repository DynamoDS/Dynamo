﻿using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitTestFramework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class StructuralFramingTests : RevitNodeTestBase
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

    }
}
