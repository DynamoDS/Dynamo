using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ModelCurveTests
    {
        [Test]
        public void ByCurve_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 1, 1));
            Assert.NotNull(line);

            var modelCurve = ModelCurve.ByCurve(line);
            Assert.NotNull(line);

            var curveRef = modelCurve.CurveReference;
            Assert.NotNull(curveRef);
        }
    }
}
