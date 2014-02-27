using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class MigrationManagerTests
    {
        private XmlDocument xmlDocument = null;

        [SetUp]
        public void SetupTests()
        {
            xmlDocument = new XmlDocument();
        }

        [TearDown]
        public void TearDownTests()
        {
            xmlDocument = null;
        }

        [Test]
        public void CreateFunctionNodeFrom00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // First argument being null should throw an exception.
                MigrationManager.CreateFunctionNodeFrom(null, null);
            });
        }

        [Test]
        public void CreateFunctionNodeFrom01()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");

            Assert.Throws<ArgumentException>(() =>
            {
                // Second argument being null should throw an exception.
                MigrationManager.CreateFunctionNodeFrom(srcElement, null);
            });
        }

        [Test]
        public void CreateFunctionNodeFrom02()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");

            Assert.Throws<ArgumentException>(() =>
            {
                // Second argument being empty should throw an exception.
                MigrationManager.CreateFunctionNodeFrom(srcElement, new string[] { });
            });
        }

        [Test]
        public void CreateFunctionNodeFrom03()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");

            // Non-existence attribute will result in a same-name attribute 
            // in the resulting XmlElement with an empty value.
            XmlElement dstElement = MigrationManager.CreateFunctionNodeFrom(
                srcElement, new string[] { "dummy" });

            Assert.IsNotNull(dstElement);
            Assert.AreEqual(2, dstElement.Attributes.Count);
            Assert.AreEqual("", dstElement.Attributes["dummy"].Value);
            Assert.AreEqual("Dynamo.Nodes.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        public void CreateFunctionNodeFrom04()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("guid", "D514AA10-63F0-4479-BB9F-0FEBEB2274B0");
            srcElement.SetAttribute("isUpstreamVisible", "yeah");

            // Non-existence attribute will result in a same-name attribute 
            // in the resulting XmlElement with an empty value.
            XmlElement dstElement = MigrationManager.CreateFunctionNodeFrom(
                srcElement, new string[] { "guid", "dummy", "isUpstreamVisible" });

            Assert.IsNotNull(dstElement);
            Assert.AreEqual(4, dstElement.Attributes.Count);
            Assert.AreEqual("D514AA10-63F0-4479-BB9F-0FEBEB2274B0",
                dstElement.Attributes["guid"].Value);

            Assert.AreEqual("", dstElement.Attributes["dummy"].Value);
            Assert.AreEqual("yeah", dstElement.Attributes["isUpstreamVisible"].Value);
            Assert.AreEqual("Dynamo.Nodes.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        public void CreateFunctionNodeFrom05()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.CreateFunctionNodeFrom(null);
            });
        }

        [Test]
        public void CreateFunctionNodeFrom06()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            XmlElement dstElement = MigrationManager.CreateFunctionNodeFrom(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(1, dstElement.Attributes.Count);
            Assert.AreEqual("Dynamo.Nodes.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        public void CreateFunctionNodeFrom07()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("one", "1");
            srcElement.SetAttribute("two", "2");
            srcElement.SetAttribute("three", "3");

            XmlElement dstElement = MigrationManager.CreateFunctionNodeFrom(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(4, dstElement.Attributes.Count);
            Assert.AreEqual("1", dstElement.Attributes["one"].Value);
            Assert.AreEqual("2", dstElement.Attributes["two"].Value);
            Assert.AreEqual("3", dstElement.Attributes["three"].Value);
            Assert.AreEqual("Dynamo.Nodes.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        public void TestCreateCodeBlockNodeFrom00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.CreateCodeBlockNodeFrom(null);
            });
        }

        [Test]
        public void TestCreateCodeBlockNodeFrom01()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("isVisible", "true");
            srcElement.SetAttribute("isUpstreamVisible", "false");
            srcElement.SetAttribute("lacing", "Longest");

            XmlElement dstElement = MigrationManager.CreateCodeBlockNodeFrom(srcElement);
            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);

            XmlAttributeCollection attribs = dstElement.Attributes;
            Assert.AreEqual(7, attribs.Count);
            Assert.AreEqual("true", attribs["isVisible"].Value);
            Assert.AreEqual("false", attribs["isUpstreamVisible"].Value);
            Assert.AreEqual("Disabled", attribs["lacing"].Value);
            Assert.AreEqual("Dynamo.Nodes.CodeBlockNodeModel", attribs["type"].Value);
            Assert.AreEqual(string.Empty, attribs["CodeText"].Value);
            Assert.AreEqual("false", attribs["ShouldFocus"].Value);
            Assert.AreEqual("Code Block", attribs["nickname"].Value);
        }

        [Test]
        public void TestCreateDummyNode00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                XmlElement dummy = MigrationManager.CreateDummyNode(null, 1, 2);
            });
        }

        [Test]
        public void TestCreateDummyNode01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                XmlElement srcElement = xmlDocument.CreateElement("OldNamespace.OldClass");
                XmlElement dummy = MigrationManager.CreateDummyNode(srcElement, -1, 2);
            });
        }

        [Test]
        public void TestCreateDummyNode02()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                XmlElement srcElement = xmlDocument.CreateElement("OldNamespace.OldClass");
                XmlElement dummy = MigrationManager.CreateDummyNode(srcElement, 2, -1);
            });
        }

        [Test]
        public void TestCreateDummyNode03()
        {
            XmlElement srcElement = xmlDocument.CreateElement("OldNamespace.OldClass");
            srcElement.SetAttribute("type", srcElement.Name);
            srcElement.SetAttribute("a", "1");
            srcElement.SetAttribute("b", "2");
            srcElement.SetAttribute("c", "3");

            XmlElement dummy = MigrationManager.CreateDummyNode(srcElement, 6, 8);

            // Ensure existing attributes are retained.
            var attribs = dummy.Attributes;
            Assert.AreEqual("1", attribs["a"].Value);
            Assert.AreEqual("2", attribs["b"].Value);
            Assert.AreEqual("3", attribs["c"].Value);

            Assert.AreEqual("DSCoreNodesUI.DummyNode", dummy.Name);
            Assert.AreEqual("DSCoreNodesUI.DummyNode", attribs["type"].Value);
            Assert.AreEqual("OldNamespace.OldClass", attribs["legacyNodeName"].Value);
            Assert.AreEqual("6", attribs["inputCount"].Value);
            Assert.AreEqual("8", attribs["outputCount"].Value);
        }

        [Test]
        public void NodeMigrationData00()
        {
            // Should have a default list that is empty.
            NodeMigrationData data = new NodeMigrationData(xmlDocument);
            Assert.IsNotNull(data.MigratedNodes);
        }

        [Test]
        public void NodeMigrationData01()
        {
            NodeMigrationData data = new NodeMigrationData(xmlDocument);

            XmlElement first = xmlDocument.CreateElement("Element");
            XmlElement second = xmlDocument.CreateElement("Element");
            data.AppendNode(first);
            data.AppendNode(second);

            Assert.IsNotNull(data.MigratedNodes);
            Assert.AreEqual(first, data.MigratedNodes.ElementAt(0));
            Assert.AreEqual(second, data.MigratedNodes.ElementAt(1));
        }

        [Test]
        public void TestExtractFileIndex00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.ExtractFileIndex(null);
            });
        }

        [Test]
        public void TestExtractFileIndex01()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.ExtractFileIndex(string.Empty);
            });
        }

        [Test]
        public void TestExtractFileIndex02()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // File name should have "*.backup" extension.
                MigrationManager.ExtractFileIndex("abc.txt");
            });
        }

        [Test]
        public void TestExtractFileIndex03()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // File name should be in the form of "*.NNN.backup".
                MigrationManager.ExtractFileIndex("abc.backup");
            });
        }

        [Test]
        public void TestExtractFileIndex04()
        {
            Assert.Throws<FormatException>(() =>
            {
                // "NNN" should be in numeric form.
                MigrationManager.ExtractFileIndex("abc.NNN.backup");
            });
        }

        [Test]
        public void TestExtractFileIndex05()
        {
            Assert.AreEqual(123, MigrationManager.ExtractFileIndex("abc.123.backup"));
        }

        [Test]
        public void TestGetUniqueIndex00()
        {
            Assert.AreEqual(0, MigrationManager.GetUniqueIndex(null));
        }

        [Test]
        public void TestGetUniqueIndex01()
        {
            Assert.AreEqual(0, MigrationManager.GetUniqueIndex(new string[] { }));
        }

        [Test]
        public void TestGetUniqueIndex02()
        {
            var fileNames = new string[]
            {
                "FileName.12.backup",
                "FileName.13.backup",
                "FileName.456.backup"
            };

            Assert.AreEqual(457, MigrationManager.GetUniqueIndex(fileNames));
        }
    }
}
