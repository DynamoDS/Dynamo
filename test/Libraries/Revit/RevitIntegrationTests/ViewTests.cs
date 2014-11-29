using System.IO;
using System.Linq;

using Dynamo.Nodes;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class ViewTests : SystemTest
    {

        [Test]
        [TestModel(@".\View\AxonometricView.rfa")]
        public void AxonometricView()
        {
            string samplePath = Path.Combine(workingDirectory, @".\View\AxonometricView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\View\OverrideElementColorInView.rvt")]
        public void OverrideElementColorInView()
        {
            string samplePath = Path.Combine(workingDirectory, @".\View\OverrideElementColorInView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\View\PerspectiveView.rfa")]
        public void PerspectiveView()
        {
            string samplePath = Path.Combine(workingDirectory, @".\View\PerspectiveView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test, TestModel(@".\Empty.rvt")]
        public void ExportAsImage()
        {
            string samplePath = Path.Combine(workingDirectory, @".\View\ExportImage.dyn");
            string testPath = Path.GetFullPath(samplePath);

            OpenDynamoDefinition(testPath);
            
            AssertNoDummyNodes();

            // Find the CBN and change it to have two temporary paths.
            var stringNodes = Model.CurrentWorkspace.Nodes.Where(x => x is StringInput).Cast<StringInput>().ToList();
            Assert.AreEqual(stringNodes.Count, 2);

            var tmp1 = Path.GetTempFileName();
            var tmp2 = Path.GetTempFileName();

            tmp1 = Path.ChangeExtension(tmp1, ".png");
            tmp2 = Path.ChangeExtension(tmp1, ".png");

            stringNodes[0].Value = tmp1;
            stringNodes[1].Value = tmp2;

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            // Ensure that our two temporary files have some data
            var tmp1Info = new FileInfo(tmp1);
            Assert.Greater(tmp1Info.Length, 0);

            var tmp2Info = new FileInfo(tmp2);
            Assert.Greater(tmp2Info.Length, 0);

        }
    }
}
