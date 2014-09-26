using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class RenderingAsAServiceTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenDaylightingMinimalSample()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(_testPath, @".\Samples\DaylightingMinimal.dyn");
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
            string samplePath = Path.Combine(_testPath, @".\Samples\SimpleCloudRender.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanOpenDaylightingExtractingData()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(_testPath, @".\Samples\DaylightingExtractingData.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            Assert.AreEqual(53, model.CurrentWorkspace.Nodes.Count);
            AssertNoDummyNodes();
        }

    }
}
