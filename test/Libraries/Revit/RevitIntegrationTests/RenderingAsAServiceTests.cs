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
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenDaylightingandAnalysisDisplay()
        {
            //Analyze nodes do not resolve in daylight sample files.
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3757
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\DaylightingandAnalysisDisplay.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(42, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenhill_climbing_simple()
        {
            //Analyze nodes do not resolve in daylight sample files.
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3757
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\hill_climbing_simple.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenBruteForcingParametersandRendering()
        {
            //Analyze nodes do not resolve in daylight sample files.
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3757
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\BruteForcingParametersandRendering.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(33, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }
         [Test]
        [TestModel(@".\empty.rfa")]
        public void hill_climbing_daylighting()
        {
            //Analyze nodes do not resolve in daylight sample files.
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3757
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Samples\hill_climbing_daylighting.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(21, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }
        
    }
}
