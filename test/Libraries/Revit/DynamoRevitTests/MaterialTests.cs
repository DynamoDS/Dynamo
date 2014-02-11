using System.IO;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class MaterialTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void GetMaterialByName()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Material\GetMaterialByName.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }
    }
}
