using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;
using RevitServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests
{
    [TestFixture]
    class CeilingPlanViewTests
    {
        [Test]
        public void ByLevel_ValidArgs()
        {
            var elevation = 100;
            var level = DSLevel.ByElevation(elevation);
            Assert.NotNull(level);

            var view = DSCeilingPlanView.ByLevel(level);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByLevel_BadArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSCeilingPlanView.ByLevel(null));
        }
    }
}
