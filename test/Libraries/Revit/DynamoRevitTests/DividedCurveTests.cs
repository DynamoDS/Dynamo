using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class DividedCurveTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\DividedCurve\DividedCurve.rfa")]
        public void DividedCurve()
        {
            string samplePath = Path.Combine(_testPath, @".\DividedCurve\DividedCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
