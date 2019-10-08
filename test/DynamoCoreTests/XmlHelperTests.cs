using System;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class XmlHelperTests
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
        [Category("UnitTests")]
        public void TestIntegerAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", -1234);

            // Test reading of existing attribute.
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual(-1234, reader.ReadInteger("ValidName"));

            // Test reading of non-existence attribute with default value.
            Assert.AreEqual(5678, reader.ReadInteger("InvalidName", 5678));

            // Test reading of non-existence attribute without default value.
            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.ReadInteger("InvalidName");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestDoubleAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", -12.34);

            // Test reading of existing attribute.
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual(-12.34, reader.ReadDouble("ValidName"));

            // Test reading of non-existence attribute with default value.
            Assert.AreEqual(56.78, reader.ReadDouble("InvalidName", 56.78));

            // Test reading of non-existence attribute without default value.
            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.ReadDouble("InvalidName");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestBooleanAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", true);

            // Test reading of existing attribute.
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual(true, reader.ReadBoolean("ValidName"));

            // Test reading of non-existence attribute with default value.
            Assert.AreEqual(true, reader.ReadBoolean("InvalidName", true));
            Assert.AreEqual(false, reader.ReadBoolean("InvalidName", false));

            // Test reading of non-existence attribute without default value.
            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.ReadBoolean("InvalidName");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestStringAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", "Abc123");

            // Test reading of existing attribute.
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual("Abc123", reader.ReadString("ValidName"));

            // Test reading of non-existence attribute with default value.
            Assert.AreEqual("Xyz", reader.ReadString("InvalidName", "Xyz"));

            // Test reading of non-existence attribute without default value.
            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.ReadString("InvalidName");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestGuidAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            System.Guid guidValue = System.Guid.NewGuid();
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", guidValue);

            // Test reading of existing attribute.
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual(guidValue, reader.ReadGuid("ValidName"));

            // Test reading of non-existence attribute with default value.
            System.Guid defaultGuid = System.Guid.NewGuid();
            Assert.AreEqual(defaultGuid, reader.ReadGuid("InvalidName", defaultGuid));

            // Test reading of non-existence attribute without default value.
            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.ReadGuid("InvalidName");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestTypeAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", typeof(System.Environment));

            // Test reading of existing attribute.
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual("System.Environment", reader.ReadString("ValidName"));
        }

        [Test]
        [Category("UnitTests")]
        public void TestEnumAttributes()
        {
            XmlElement element = xmlDocument.CreateElement("element");

            // Test attribute writing.
            LacingStrategy strategy = LacingStrategy.CrossProduct;
            XmlElementHelper writer = new XmlElementHelper(element);
            writer.SetAttribute("ValidName", strategy.ToString());
            writer.SetAttribute("ValidName2", "UnknownEnumString");

            // Test reading of existing attributes (with valid/invalid values).
            XmlElementHelper reader = new XmlElementHelper(element);
            Assert.AreEqual(strategy, reader.ReadEnum("ValidName", LacingStrategy.Disabled));

            LacingStrategy defaultValue = LacingStrategy.First;
            Assert.AreEqual(defaultValue, reader.ReadEnum("ValidName2", defaultValue));
        }
    }
}
