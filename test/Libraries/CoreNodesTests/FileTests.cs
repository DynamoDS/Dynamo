using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FileWritingTests : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void FileWriter()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\files\FileWriter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);

            var path = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.StringInput>("84693240-90f3-45f3-9cb3-88207499f0bc");
            string fullPath = Path.Combine(TempFolder, "filewriter.txt");
            path.Value = fullPath;

            ViewModel.Model.RunExpression();

            AssertPreviewValue("48c04164-6435-4124-9fe6-b3319ef177da", true);
        }

        [Test]
        public void ImageFileWriter()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\files\ImageFileWriter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            var filename = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.StringInput>("0aaae6d6-f84b-4e61-888b-14936343d80a");
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());
            var path = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.StringInput>("84693240-90f3-45f3-9cb3-88207499f0bc");
            path.Value = TempFolder;

            ViewModel.Model.RunExpression();

            AssertPreviewValue("48c04164-6435-4124-9fe6-b3319ef177da", true);
        }
    }

    [TestFixture]
    public class ZeroTouchMigrationFileTests : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void TestZeroTouchMigrationFile()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\files\MigrationHintGetClosestPoint.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);

            ViewModel.Model.RunExpression();

            AssertPreviewValue("8527c4f5-f8e1-491e-b446-64c495fa1848", 4.54606056566195);
        }
    }
}
