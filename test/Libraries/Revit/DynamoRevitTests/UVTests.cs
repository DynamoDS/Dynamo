using System.IO;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class UVTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void UVRandom()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\UV\UVRandom.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }
    }
}
