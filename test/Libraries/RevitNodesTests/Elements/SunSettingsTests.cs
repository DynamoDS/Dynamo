using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;
using Revit.GeometryConversion;
using RevitNodesTests;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class SunSettingsTests : RevitNodeTestBase
    {
        [Test, TestModel(@".\Empty.rvt")]
        public void Current()
        {
            Assert.AreEqual(DocumentManager.Instance.CurrentDBDocument.ActiveView.
                SunAndShadowSettings.Id, SunSettings.Current().InternalSunAndShadowSettings.Id);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void Direction()
        {
            Vector.ByCoordinates(39.898058, -28.624325, 87.113678).ShouldBeApproximately(
                SunSettings.Current().SunDirection);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void Altitude()
        {
            SunSettings.Current().Altitude.ShouldBeApproximately(60.591011);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void Azimuth()
        {
            SunSettings.Current().Azimuth.ShouldBeApproximately(125.657039);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void StartDateTime()
        {
            Assert.AreEqual(
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.StartDateAndTime,
                SunSettings.Current().StartDateTime);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void EndDateTime()
        {
            Assert.AreEqual(
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.EndDateAndTime,
                SunSettings.Current().EndDateTime);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void CurrentDateTime()
        {
            Assert.AreEqual(
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.ActiveFrameTime,
                SunSettings.Current().CurrentDateTime);
        }
    }
}
