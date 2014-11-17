using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class FamilyTests : SystemTest
    {   
        [Test]
        [TestModel(@".\Family\GetFamilyInstancesByType.rvt")]
        public void GetFamilyInstancesByType()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Family\GetFamilyInstancesByType.dyn");
            string testPath = Path.GetFullPath(samplePath);


            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            Assert.AreEqual(100, GetPreviewValue("5eac6ab9-e736-49a9-a90a-8b6d93676813"));
        }

        [Test]
        [TestModel(@".\Family\GetFamilyInstanceLocation.rvt")]
        public void GetFamilyInstanceLocation()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            AssertPreviewCount("4274fd18-23b8-4c5c-9006-14d927fa3ff3", 100);

        }

        [Test]
        [TestModel(@".\Family\AC_locationStandAlone.rfa")]
        public void CanLocateAdaptiveComponent()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Family\AC_locationStandAlone.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            var pnt = GetPreviewValue("79dde258-ddce-49b7-9700-da21b2d5a9ae") as Autodesk.DesignScript.Geometry.Point;
            Assert.IsNotNull(pnt);

            //Check the value is (-9.586931, -35.209237, 8.170673) in the metric system
            Assert.IsTrue(IsFuzzyEqual(pnt.X, -9.586931, 1.0e-6));
            Assert.IsTrue(IsFuzzyEqual(pnt.Y, -35.209237, 1.0e-6));
            Assert.IsTrue(IsFuzzyEqual(pnt.Z, 8.170673, 1.0e-6));
        }

        [Test]
        [TestModel(@".\Family\AC_locationInDividedSurface.rfa")]
        public void CanLocateAdaptiveComponentInDividedSurface()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Family\AC_locationInDividedSurface.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            AssertPreviewCount("76076507-d16e-4480-802c-14ba87d88f81", 25);
        }
    }
}
