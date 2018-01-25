using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using Dynamo.Migration;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    [Category("JsonTestExclude")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void CreateFunctionNodeFrom04()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("guid", "D514AA10-63F0-4479-BB9F-0FEBEB2274B0");
            
            // Non-existence attribute will result in a same-name attribute 
            // in the resulting XmlElement with an empty value.
            XmlElement dstElement = MigrationManager.CreateFunctionNodeFrom(
                srcElement, new string[] { "guid", "dummy" });

            Assert.IsNotNull(dstElement);
            Assert.AreEqual(3, dstElement.Attributes.Count);
            Assert.AreEqual("D514AA10-63F0-4479-BB9F-0FEBEB2274B0",
                dstElement.Attributes["guid"].Value);

            Assert.AreEqual("", dstElement.Attributes["dummy"].Value);
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void CreateFunctionNodeFrom05()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.CreateFunctionNodeFrom(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void CreateFunctionNodeFrom06()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            XmlElement dstElement = MigrationManager.CreateFunctionNodeFrom(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(1, dstElement.Attributes.Count);
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        [Category("UnitTests")]
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
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSFunction", dstElement.Attributes["type"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void CreateVarArgFunctionNodeFrom00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.CreateFunctionNodeFrom(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void CreateVarArgFunctionNodeFrom01()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            XmlElement dstElement = MigrationManager.CreateVarArgFunctionNodeFrom(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(2, dstElement.Attributes.Count);
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction", dstElement.Attributes["type"].Value);
            Assert.AreEqual("0", dstElement.Attributes["inputcount"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void CreateVarArgFunctionNodeFrom02()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("one", "1");
            srcElement.SetAttribute("two", "2");
            srcElement.SetAttribute("three", "3");

            XmlElement dstElement = MigrationManager.CreateVarArgFunctionNodeFrom(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(5, dstElement.Attributes.Count);
            Assert.AreEqual("1", dstElement.Attributes["one"].Value);
            Assert.AreEqual("2", dstElement.Attributes["two"].Value);
            Assert.AreEqual("3", dstElement.Attributes["three"].Value);
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction", dstElement.Attributes["type"].Value);
            Assert.AreEqual("0", dstElement.Attributes["inputcount"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void CreateVarArgFunctionNodeFrom03()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            XmlElement input1 = xmlDocument.CreateElement("Element");
            XmlElement input2 = xmlDocument.CreateElement("Element");
            srcElement.PrependChild(input1);
            srcElement.PrependChild(input2);
            XmlElement dstElement = MigrationManager.CreateVarArgFunctionNodeFrom(srcElement);

            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);
            Assert.AreEqual(2, dstElement.Attributes.Count);
            Assert.AreEqual("Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction", dstElement.Attributes["type"].Value);
            Assert.AreEqual("2", dstElement.Attributes["inputcount"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void TestVersionFromString()
        {
            var versionString = "1.20.300.4000";
            Version version = MigrationManager.VersionFromString(versionString);
            Assert.IsNotNull(version);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(20, version.Minor);
            Assert.AreEqual(300, version.Build);
            Assert.AreEqual(0, version.Revision);
        }

        [Test]
        [Category("UnitTests")]
        public void TestShouldMigrateFile()
        {
            Version oldVer = new Version(0, 7, 0);
            Version newVer = new Version(0, 7, 1);
            Version newestVer = new Version(0, 7, 2);

            DynamoModel.IsTestMode = false;
            var decision1 = MigrationManager.ShouldMigrateFile(oldVer, newVer, true);
            var decision2 = MigrationManager.ShouldMigrateFile(newestVer, newVer, true);
            Assert.AreEqual(MigrationManager.Decision.Migrate, decision1);
            Assert.AreEqual(MigrationManager.Decision.Retain, decision2);

            DynamoModel.IsTestMode = true;
            decision1 = MigrationManager.ShouldMigrateFile(oldVer, newVer, true);
            decision2 = MigrationManager.ShouldMigrateFile(newestVer, newVer, true);
            Assert.AreEqual(MigrationManager.Decision.Migrate, decision1);
            Assert.AreEqual(MigrationManager.Decision.Retain, decision2);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCreateCodeBlockNodeFrom00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.CreateCodeBlockNodeFrom(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestCreateCodeBlockNodeFrom01()
        {
            XmlElement srcElement = xmlDocument.CreateElement("Element");
            srcElement.SetAttribute("isVisible", "true");
            srcElement.SetAttribute("lacing", "Longest");

            XmlElement dstElement = MigrationManager.CreateCodeBlockNodeFrom(srcElement);
            Assert.IsNotNull(dstElement);
            Assert.IsNotNull(dstElement.Attributes);

            XmlAttributeCollection attribs = dstElement.Attributes;
            Assert.AreEqual(6, attribs.Count);
            Assert.AreEqual("true", attribs["isVisible"].Value);
            Assert.AreEqual("Disabled", attribs["lacing"].Value);
            Assert.AreEqual("Dynamo.Graph.Nodes.CodeBlockNodeModel", attribs["type"].Value);
            Assert.AreEqual(string.Empty, attribs["CodeText"].Value);
            Assert.AreEqual("false", attribs["ShouldFocus"].Value);
            Assert.AreEqual("Code Block", attribs["nickname"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCreateDummyNode00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                XmlElement dummy = MigrationManager.CreateDummyNode(null, 1, 2);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestCreateDummyNode01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                XmlElement srcElement = xmlDocument.CreateElement("OldNamespace.OldClass");
                XmlElement dummy = MigrationManager.CreateDummyNode(srcElement, -1, 2);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestCreateDummyNode02()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                XmlElement srcElement = xmlDocument.CreateElement("OldNamespace.OldClass");
                XmlElement dummy = MigrationManager.CreateDummyNode(srcElement, 2, -1);
            });
        }

        [Test]
        [Category("UnitTests")]
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

            Assert.AreEqual("Dynamo.Graph.Nodes.DummyNode", dummy.Name);
            Assert.AreEqual("Dynamo.Graph.Nodes.DummyNode", attribs["type"].Value);
            Assert.AreEqual("OldNamespace.OldClass", attribs["legacyNodeName"].Value);
            Assert.AreEqual("Deprecated", attribs["nodeNature"].Value);
            Assert.AreEqual("6", attribs["inputCount"].Value);
            Assert.AreEqual("8", attribs["outputCount"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCreateMissingNode00()
        {
            XmlElement srcElement = xmlDocument.CreateElement("OldNamespace.OldClass");
            srcElement.SetAttribute("type", srcElement.Name);
            srcElement.SetAttribute("a", "1");
            srcElement.SetAttribute("b", "2");
            srcElement.SetAttribute("c", "3");

            XmlElement dummy = MigrationManager.CreateMissingNode(srcElement, 6, 8);

            // Ensure existing attributes are retained.
            var attribs = dummy.Attributes;
            Assert.AreEqual("1", attribs["a"].Value);
            Assert.AreEqual("2", attribs["b"].Value);
            Assert.AreEqual("3", attribs["c"].Value);

            Assert.AreEqual("Dynamo.Graph.Nodes.DummyNode", dummy.Name);
            Assert.AreEqual("Dynamo.Graph.Nodes.DummyNode", attribs["type"].Value);
            Assert.AreEqual("OldNamespace.OldClass", attribs["legacyNodeName"].Value);
            Assert.AreEqual("Unresolved", attribs["nodeNature"].Value);
            Assert.AreEqual("6", attribs["inputCount"].Value);
            Assert.AreEqual("8", attribs["outputCount"].Value);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeMigrationData00()
        {
            // Should have a default list that is empty.
            NodeMigrationData data = new NodeMigrationData(xmlDocument);
            Assert.IsNotNull(data.MigratedNodes);
        }

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
        public void TestGetBackupFolder00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.GetBackupFolder(null, false);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetBackupFolder01()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.GetBackupFolder(string.Empty, false);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetBackupFolder02()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Test invalid folder exception.
                var folder = @"k:\i-dont-think-this-folder-exists\";
                MigrationManager.GetBackupFolder(folder, false);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetBackupFolder03()
        {
            var location = Assembly.GetCallingAssembly().Location;
            var folder = Path.GetDirectoryName(location);
            var expected = Path.Combine(folder, "backup");
            Assert.AreEqual(expected, MigrationManager.GetBackupFolder(folder, false));
        }

        [Test]
        [Category("UnitTests")]
        public void TestExtractFileIndex00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.ExtractFileIndex(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestExtractFileIndex01()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                MigrationManager.ExtractFileIndex(string.Empty);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestExtractFileIndex02()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // File name should have "*.backup" extension.
                MigrationManager.ExtractFileIndex("abc.txt");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestExtractFileIndex03()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // File name should be in the form of "*.NNN.backup".
                MigrationManager.ExtractFileIndex("abc.backup");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestExtractFileIndex04()
        {
            Assert.Throws<FormatException>(() =>
            {
                // "NNN" should be in numeric form.
                MigrationManager.ExtractFileIndex("abc.NNN.backup");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestExtractFileIndex05()
        {
            Assert.AreEqual(123, MigrationManager.ExtractFileIndex("abc.123.backup"));
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetUniqueIndex00()
        {
            Assert.AreEqual(0, MigrationManager.GetUniqueIndex(null));
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetUniqueIndex01()
        {
            Assert.AreEqual(0, MigrationManager.GetUniqueIndex(new string[] { }));
        }

        [Test]
        [Category("UnitTests")]
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
