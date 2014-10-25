using System.IO;

using NUnit.Framework;

using RevitSystemTests;

using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class FaceTests : SystemTest
    {
        [Test]
        [TestModel(@".\Face\GetSurfaceDomain.rvt")]
        public void GetSurfaceDomain()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Face\GetSurfaceDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
