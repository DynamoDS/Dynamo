using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class SunSettingsTests : RevitNodeTestBase
    {
        [Test, TestModel(@".\Empty.rvt")]
        public void Current()
        {
            Assert.AreSame(
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings,
                SunSettings.Current());
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void Direction()
        {
            Assert.AreEqual(Vector.ByCoordinates(39.898, -28.624, 87.114), SunSettings.Current().SunDirection);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void Altitude()
        {
            Assert.AreEqual(
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.Altitude,
                SunSettings.Current().Altitude);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void Azimuth()
        {
            Assert.AreEqual(
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.Azimuth,
                SunSettings.Current().Azimuth);
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
