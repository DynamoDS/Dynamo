using NUnit.Framework;

using RevitServices.Persistence;

using RevitNodesTests;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class AbstractElementTests : RevitNodeTestBase
    {
        [Test, Ignore, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void GetParameterByName_ValidArgs()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
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
        [TestModel(@".\empty.rfa")]
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

        [Test, Ignore, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void OverrideColorInView_ValidArgs()
        {
            Assert.Inconclusive("Finish me.");
        }
    }
}
