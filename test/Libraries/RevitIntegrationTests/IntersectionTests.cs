using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class IntersectionTests : SystemTest
    {
        [Test]
        [TestModel(@".\Intersect\CurveCurveIntersection.rfa")]
        public void CurveCurveIntersection()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Intersect\CurveCurveIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\Intersect\CurveFaceIntersection.rfa")]
        public void CurveFaceIntersection()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Intersect\CurveFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\Intersect\FaceFaceIntersection.rfa")]
        public void FaceFaceIntersection()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Intersect\FaceFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\Intersect\EdgePlaneIntersection.rfa")]
        public void EdgePlaneIntersection()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Intersect\EdgePlaneIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
