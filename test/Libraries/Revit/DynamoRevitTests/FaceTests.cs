using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class FaceTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void GetSurfaceDomain()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Face\GetSurfaceDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}
