using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class DividedSurfaceTests 
    {
        [Test]
        public void ByFaceAndUVDivisions_ValidArgs()
        {
            var ele = ElementSelector.OfType<Autodesk.Revit.DB.Form>().FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as DSForm;
            var face = form.FaceReferences.First();

            var divSrf = DSDividedSurface.ByFaceUVDivisions(face, 5, 6);
            Assert.NotNull(divSrf);

            Assert.AreEqual(5, divSrf.UDivisions);
            Assert.AreEqual(6, divSrf.VDivisions);
            Assert.AreEqual(0.0, divSrf.Rotation, 0.001);

        }
    }
}
