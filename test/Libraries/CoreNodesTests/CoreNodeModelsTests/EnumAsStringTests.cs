using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dynamo.Graph;
using Moq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using TestUINodes;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class EnumAsStringTests
    {
        private EnumAsStringConcrete testNode;
        private Mock<AssociativeNode> associativeNodeMock;


        [SetUp]
        public void TestsSetup()
        {
            testNode = new EnumAsStringConcrete();
            associativeNodeMock = new Mock<AssociativeNode>(MockBehavior.Strict);
            testNode.Name = "testNodeName";

        }

        [Test]
        public void PopulateItemsCoreTest()
        {
            var response = testNode.PopulateItemsCore("");
            Assert.AreEqual("Restore", response);
            Assert.AreEqual(3, testNode.Items.Count);
        }

        [Test]
        public void SelectedIndexTest()
        {
            Assert.AreEqual(-1, testNode.SelectedIndex);

            testNode.SelectedIndex = 1;
            Assert.AreEqual(1, testNode.SelectedIndex);

            testNode.SelectedIndex = testNode.Items.Count+1;
            Assert.AreEqual(-1, testNode.SelectedIndex);
        }

        [Test]
        public void BuildOutputAstTest()
        {
            testNode.SelectedIndex = 0;
            var response = testNode.BuildOutputAst(new List<AssociativeNode> {associativeNodeMock.Object});
            Assert.AreEqual(1, response.Count());
        }

        [Test]
        public void SerializeTest()
        {
            testNode.SelectedIndex = 0;

            //Serializes node into xml
            var testDocument = new XmlDocument();
            testDocument.LoadXml("<ElementTag/>");
            var testElement = testDocument.DocumentElement;
            testNode.SerializeCore(testElement, SaveContext.None);

            var item = testElement.Attributes.GetNamedItem("index");

            string expectedInnerXML = String.Format("{0}:{1}",
                testNode.SelectedIndex, testEnum.A );
            Assert.AreEqual(expectedInnerXML, item.Value);
        }

        [Test]
        public void DeserializeTest()
        {
            testNode.SelectedIndex = 0;

            //Serializes selection into xml
            var testDocument = new XmlDocument();
            testDocument.LoadXml("<ElementTag/>");
            var testElement = testDocument.DocumentElement;
            testNode.SerializeCore(testElement, SaveContext.None);

            //Clears items
            testNode.SelectedIndex = 1;

            //Recovers selection from xml
            testNode.DesrializeCore(testElement, SaveContext.None);
            Assert.AreEqual(0, testNode.SelectedIndex);
        }

        [Test]
        public void UpdateValueCoreTest()
        {
            //Sets property to be updated
            testNode.Name = "Starting Name";

            //Updates Name property with new value
            var updatedValue = new UpdateValueParams("Name", "UpdatedName");
            bool response = testNode.UpdateValueCore(updatedValue);
            Assert.IsTrue(response);
            Assert.AreEqual(updatedValue.PropertyValue, testNode.Name);

            //Case: valid update "Value" property 
            //Sets selectedIndex
            testNode.SelectedIndex = 0;

            ////Updates selectedIndex value
            updatedValue = new UpdateValueParams("Value", "1");
            response = testNode.UpdateValueCore(updatedValue);
            Assert.IsTrue(response);
            Assert.AreEqual(1, testNode.SelectedIndex);
        }
    }
}

