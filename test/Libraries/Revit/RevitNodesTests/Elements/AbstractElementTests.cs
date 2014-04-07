using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class AbstractElementTests
    {
        [Test]
        public void GetParameterByName_ValidArgs()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        public void SetParameterByName_ValidArgs()
        {
            ElementBinder.IsEnabled = false;

            var refPt = Revit.Elements.ReferencePoint.ByCoordinates(0, 0, 0);

            refPt.SetParameterByName("Name", "Tom");
            refPt.SetParameterByName("Name", "Dick");
            refPt.SetParameterByName("Name", "Harry");

            refPt.SetParameterByName("Mirrored", true);
            refPt.SetParameterByName("Mirrored", false);
        }

        [Test]
        public void SetParametersByName_InvalidArgs()
        {
            ElementBinder.IsEnabled = false;

            var refPt = Revit.Elements.ReferencePoint.ByCoordinates(0, 0, 0);

            //assert an exception is thrown when a parameter is not found
            Assert.Throws<System.Exception>
                (() => refPt.SetParameterByName("Blah", -1.0));

            //assert an exception is thrown when a parameter is found
            //but the value supplied is not of the correct type
            Assert.Throws<System.Exception>
                (() => refPt.SetParameterByName("Name", -1.0));
        }

        [Test]
        public void OverrideColorInView_ValidArgs()
        {
            Assert.Inconclusive("Finish me.");
        }
    }
}
