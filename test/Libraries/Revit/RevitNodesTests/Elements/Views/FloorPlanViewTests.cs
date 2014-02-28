using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revit;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryObjects;
using NUnit.Framework;
using RevitServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests
{
    [TestFixture]
    class FloorPlanViewTests
    {
        [Test]
        public void ByLevel_ValidArgs()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);
            Assert.NotNull(level);

            var view = FloorPlanView.ByLevel(level);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByLevel_BadArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => FloorPlanView.ByLevel(null));
        }
    }
}
