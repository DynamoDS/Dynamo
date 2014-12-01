using System.IO;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using Revit.Elements.Views;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements.Views
{
    [TestFixture]
    class ViewTests : RevitNodeTestBase
    {
        [Test, TestModel(@".\Empty.rvt")]
        public void ExportAsImage_GoodArgs()
        {
            var testView = CreateTestView();

            var tmp1 = Path.GetTempFileName();

            tmp1 = Path.ChangeExtension(tmp1, ".png");
            var bmp = testView.ExportAsImage(tmp1);

            var tmp1Info = new FileInfo(tmp1);
            Assert.Greater(tmp1Info.Length, 0);
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void ExportAsImage_BadArgs()
        {
            var testView = CreateTestView();
            Assert.Throws<System.ArgumentException>(()=>testView.ExportAsImage(""));
        }

        private static View CreateTestView()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);

            var v = AxonometricView.ByEyePointAndTarget(eye, target);
            var view = (Autodesk.Revit.DB.View3D)v.InternalElement;
            Assert.AreEqual(view.Name, View3D.DEFAULT_VIEW_NAME);
            Assert.False(view.CropBoxActive);

            return v;
        }
        
    }
}
