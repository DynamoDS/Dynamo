using System;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// This file contains tests for the classes in the Attributes file.
    /// </summary>
    [TestFixture]
    class NodeAttributesTests
    {
        [Test]
        [Category("UnitTests")]
        public void NodeSearchTagsAttributeConstructorTest()
        {
            //resourceType is null
            string tagsID = "tagsID";
            Type resourceType = null;

            Assert.Throws<ArgumentNullException>(() => _ = new NodeSearchTagsAttribute(tagsID, resourceType));
        }

        [Test]
        [Category("UnitTests")]
        public void NodeDescriptionAttributeConstructorTest()
        {
            //resourceType is null
            Assert.Throws<ArgumentNullException>(() => _ = new NodeDescriptionAttribute("", null));

            //resourceType is valid
            string descriptionResourceID = "WatchNodeDescription";
            Type resourceType = typeof(Resources);

            var attr = new NodeDescriptionAttribute(descriptionResourceID, resourceType);
            Assert.AreEqual("Visualize the output of node.", attr.ElementDescription);

            //resourceType is valid but descriptionResourceID is not
            descriptionResourceID = "Nil";
            resourceType = typeof(Resources);

            attr = new NodeDescriptionAttribute(descriptionResourceID, resourceType);
            Assert.AreEqual(descriptionResourceID, attr.ElementDescription);
        }

        [Test]
        [Category("UnitTests")]
        public void DoNotLoadOnPlatformsAttributeConstructorTest()
        {
            //values is null
            var attr = new DoNotLoadOnPlatformsAttribute(null);
            Assert.AreEqual(null, attr.Values);

            //values is valid
            attr = new DoNotLoadOnPlatformsAttribute(new string[] { "" });
            Assert.AreEqual(1, attr.Values.Length);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeObsoleteAttributeConstructorTest()
        {
            //base with message
            var attr = new NodeObsoleteAttribute("Message");
            Assert.AreEqual("Message", attr.Message);

            //resourceType is null
            Assert.Throws<ArgumentNullException>(() => _ = new NodeObsoleteAttribute("", null));

            //resourceType is valid
            string descriptionResourceID = "WatchNodeDescription";
            Type resourceType = typeof(Resources);

            attr = new NodeObsoleteAttribute(descriptionResourceID, resourceType);
            Assert.AreEqual("Visualize the output of node.", attr.Message);

            //resourceType is valid but descriptionResourceID is not
            descriptionResourceID = "Nil";
            resourceType = typeof(Resources);

            attr = new NodeObsoleteAttribute(descriptionResourceID, resourceType);
            Assert.AreEqual(descriptionResourceID, attr.Message);
        }

        [Test]
        [Category("UnitTests")]
        public void InPortNamesAttributeConstructorTest()
        {
            //resourceType is null
            var attr = new InPortNamesAttribute(null, new string[] { "" });
            Assert.AreEqual(0, attr.PortNames.Count());

            //resourceType is valid
            string[] resourceNames = { "WatchNodeDescription" };
            Type resourceType = typeof(Resources);

            attr = new InPortNamesAttribute(resourceType, resourceNames);
            Assert.AreEqual(1, attr.PortNames.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void InPortTypesAttributeConstructorTest()
        {
            //resourceType is null
            var attr = new InPortTypesAttribute(null, new string[] { "" });
            Assert.AreEqual(0, attr.PortTypes.Count());

            //resourceType is valid
            string[] resourceNames = { "WatchNodeDescription" };
            Type resourceType = typeof(Resources);

            attr = new InPortTypesAttribute(resourceType, resourceNames);
            Assert.AreEqual(1, attr.PortTypes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void OutPortNamesAttributeConstructorTest()
        {
            //resourceType is null
            var attr = new OutPortNamesAttribute(null, new string[] { "" });
            Assert.AreEqual(0, attr.PortNames.Count());

            //resourceType is valid
            string[] resourceNames = { "WatchNodeDescription" };
            Type resourceType = typeof(Resources);

            attr = new OutPortNamesAttribute(resourceType, resourceNames);
            Assert.AreEqual(1, attr.PortNames.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void OutPortTypesAttributeConstructorTest()
        {
            //resourceType is null
            var attr = new OutPortTypesAttribute(null, new string[] { "" });
            Assert.AreEqual(0, attr.PortTypes.Count());

            //resourceType is valid
            string[] resourceNames = { "WatchNodeDescription" };
            Type resourceType = typeof(Resources);

            attr = new OutPortTypesAttribute(resourceType, resourceNames);
            Assert.AreEqual(1, attr.PortTypes.Count());
        }
    }
}