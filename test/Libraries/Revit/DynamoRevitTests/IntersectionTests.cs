using System.IO;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class IntersectionTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void CurveCurveIntersection()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Intersect\CurveCurveIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CurveFaceIntersection()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Intersect\CurveFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }

        [Test]
        public void FaceFaceIntersection()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Intersect\FaceFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }

        [Test]
        public void EdgePlaneIntersection()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Intersect\EdgePlaneIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }
    }
}
