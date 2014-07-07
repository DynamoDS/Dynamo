using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class IntersectionTests:DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\Intersect\CurveCurveIntersection.rfa")]
        public void CurveCurveIntersection()
        {
            string samplePath = Path.Combine(_testPath, @".\Intersect\CurveCurveIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Intersect\CurveFaceIntersection.rfa")]
        public void CurveFaceIntersection()
        {
            string samplePath = Path.Combine(_testPath, @".\Intersect\CurveFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Intersect\FaceFaceIntersection.rfa")]
        public void FaceFaceIntersection()
        {
            string samplePath = Path.Combine(_testPath, @".\Intersect\FaceFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Intersect\EdgePlaneIntersection.rfa")]
        public void EdgePlaneIntersection()
        {
            string samplePath = Path.Combine(_testPath, @".\Intersect\EdgePlaneIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
