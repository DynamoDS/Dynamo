using System;
using System.Linq;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryReferences;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class DividedSurfaceTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\block.rfa")]
        public void ByFaceUVDivisions_ValidArgs()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.ElementFaceReferences.First();

            var divSrf = DividedSurface.ByFaceAndUVDivisions(face, 5, 6);
            Assert.NotNull(divSrf);

            Assert.AreEqual(5, divSrf.UDivisions);
            Assert.AreEqual(6, divSrf.VDivisions);
            Assert.AreEqual(0.0, divSrf.Rotation, 0.001);
        }

        [Test]
        [TestModel(@".\block.rfa")]
        public void ByFaceUVDivisionsRotation_ValidArgs()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.ElementFaceReferences.First();

            var divSrf = DividedSurface.ByFaceUVDivisionsAndRotation(face, 5, 6, 30);
            Assert.NotNull(divSrf);

            Assert.AreEqual(5, divSrf.UDivisions);
            Assert.AreEqual(6, divSrf.VDivisions);
            Assert.AreEqual(30.0, divSrf.Rotation, 0.001);
        }

        [Test]
        [TestModel(@".\block.rfa")]
        public void ByFaceUVDivisionsRotation_InvalidDivisions()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.ElementFaceReferences.First();

            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceUVDivisionsAndRotation(face, 5, 0, 30));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceUVDivisionsAndRotation(face, 5, 0, 30));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceUVDivisionsAndRotation(face, 0, 0, 30));
        }

        [Test]
        [TestModel(@".\block.rfa")]
        public void ByFaceUVDivisions_InvalidDivisions()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.ElementFaceReferences.First();

            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceAndUVDivisions(face, 5, 0));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceAndUVDivisions(face, 0, 5));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceAndUVDivisions(face, 0, 0));
        }

        [Test]
        [TestModel(@".\block.rfa")]
        public void ByFaceUVDivisions_NullArgument()
        {
            ElementFaceReference faceRef = null;
            Assert.Throws(typeof(ArgumentNullException), () => DividedSurface.ByFaceAndUVDivisions(faceRef, 5, 5));

            Autodesk.DesignScript.Geometry.Surface srf = null;
            Assert.Throws(typeof(ArgumentNullException), () => DividedSurface.ByFaceAndUVDivisions(srf, 5, 5));
        }

        [Test]
        [TestModel(@".\block.rfa")]
        public void ByFaceUVDivisionsRotation_NullArgument()
        {
            ElementFaceReference faceRef = null;
            Assert.Throws(typeof(ArgumentNullException), () => DividedSurface.ByFaceUVDivisionsAndRotation(faceRef, 5, 5, 30));

            Autodesk.DesignScript.Geometry.Surface srf = null;
            Assert.Throws(typeof(ArgumentNullException), () => DividedSurface.ByFaceUVDivisionsAndRotation(srf, 5, 5, 30));
        }

    }
}
