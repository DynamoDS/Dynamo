using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    public class RenderingAsAServiceTests : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenDaylightingMinimalSample()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\DaylightingMinimal.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes are loaded
            Assert.AreEqual(33, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenSimpleCloudRender()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\SimpleCloudRender.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);
            AssertNoDummyNodes();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenDaylightingExtractingData()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\DaylightingExtractingData.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(53, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }

    }
}
