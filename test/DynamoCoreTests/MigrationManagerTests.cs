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
    }
}
