using System.IO;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ModelTextTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void ModelText()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ModelText\ModelText.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }
    }
}
