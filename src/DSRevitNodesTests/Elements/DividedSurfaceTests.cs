using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Revit;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;
using DividedSurface = Revit.Elements.DividedSurface;
using Form = Revit.Elements.Form;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class DividedSurfaceTests 
    {
        [Test]
        public void ByFaceUVDivisions_ValidArgs()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.FaceReferences.First();

            var divSrf = DividedSurface.ByFaceAndUVDivisions(face, 5, 6);
            Assert.NotNull(divSrf);

            Assert.AreEqual(5, divSrf.UDivisions);
            Assert.AreEqual(6, divSrf.VDivisions);
            Assert.AreEqual(0.0, divSrf.Rotation, 0.001);
        }

        [Test]
        public void ByFaceUVDivisionsRotation_ValidArgs()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.FaceReferences.First();

            var divSrf = DividedSurface.ByFaceUVDivisionsAndRotation(face, 5, 6, 30);
            Assert.NotNull(divSrf);

            Assert.AreEqual(5, divSrf.UDivisions);
            Assert.AreEqual(6, divSrf.VDivisions);
            Assert.AreEqual(30.0, divSrf.Rotation, 0.001);
        }

        [Test]
        public void ByFaceUVDivisionsRotation_InvalidDivisions()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.FaceReferences.First();

            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceUVDivisionsAndRotation(face, 5, 0, 30));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceUVDivisionsAndRotation(face, 5, 0, 30));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceUVDivisionsAndRotation(face, 0, 0, 30));
        }

        [Test]
        public void ByFaceUVDivisions_InvalidDivisions()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var face = form.FaceReferences.First();

            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceAndUVDivisions(face, 5, 0));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceAndUVDivisions(face, 0, 5));
            Assert.Throws(typeof(Exception), () => DividedSurface.ByFaceAndUVDivisions(face, 0, 0));
        }

        [Test]
        public void ByFaceUVDivisions_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DividedSurface.ByFaceAndUVDivisions(null, 5, 5));
        }

        [Test]
        public void ByFaceUVDivisionsRotation_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DividedSurface.ByFaceAndUVDivisions(null, 5, 5));
        }

    }
}
