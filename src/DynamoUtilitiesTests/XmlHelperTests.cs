using System.Linq;
using System.Xml;
using DynamoUtilities;
using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    [TestFixture]
    public class XmlHelperTests
    {
        [Test]
        [Category("UnitTests")]
        public void CreateDocumentTest()
        {
            var document = XmlHelper.CreateDocument(null);
            Assert.IsNull(document);

            document = XmlHelper.CreateDocument("");
            Assert.IsNull(document);

            document = XmlHelper.CreateDocument("The.Root");

            XmlDeclaration declaration = document.ChildNodes.OfType<XmlDeclaration>().
                FirstOrDefault();

            Assert.IsNotNull(declaration);
            Assert.AreEqual("UTF-8", declaration.Encoding);
            Assert.AreEqual("1.0", declaration.Version);

            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("The.Root", document.DocumentElement.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void AddNodeWithInvalidInputTest()
        {
            var document = XmlHelper.CreateDocument("TreeLibrary");

            var node = XmlHelper.AddNode(document, null, "");
            Assert.IsNull(node);

            node = XmlHelper.AddNode(document, "", null);
            Assert.IsNull(node);

            node = XmlHelper.AddNode(null, null, null);
            Assert.IsNull(node);
        }

        [Test]
        [Category("UnitTests")]
        public void AddNodeElementOnlyTest()
        {
            var document = XmlHelper.CreateDocument("TreeLibrary");

            var node = XmlHelper.AddNode(document.DocumentElement, "Nam1");
            Assert.IsNotNull(node);

            Assert.AreEqual(document, node.OwnerDocument);
            Assert.AreEqual(document.DocumentElement, node.ParentNode);
            Assert.AreEqual("Nam1", node.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void AddNodeElementAndTextNodeTest()
        {
            var document = XmlHelper.CreateDocument("TreeLibrary");

            var node = XmlHelper.AddNode(document.DocumentElement, "Nam1", "Val1");
            Assert.IsNotNull(node);

            Assert.AreEqual(document, node.OwnerDocument);
            Assert.AreEqual(document.DocumentElement, node.ParentNode);
            Assert.AreEqual("Nam1", node.Name);

            var textNode = node.FirstChild;
            Assert.AreEqual(document, textNode.OwnerDocument);
            Assert.AreEqual(node, textNode.ParentNode);
            Assert.AreEqual("Val1", textNode.Value);
        }

        [Test]
        [Category("UnitTests")]
        public void AddAttributeTest()
        {
            var document = XmlHelper.CreateDocument("TreeLibrary");
            var node = document.DocumentElement;
            // No exception expected.
            XmlHelper.AddAttribute(node, null, "");

            // No exception expected.
            XmlHelper.AddAttribute(node, "", null);

            // No exception expected.
            XmlHelper.AddAttribute(null, null, null);

            XmlHelper.AddAttribute(node, "Attr1", "Val1");

            Assert.AreEqual(node.Attributes.Count, 1);
            Assert.AreEqual(node.GetAttribute("Attr1"), "Val1");

            XmlHelper.AddAttribute(node, "Attr2", null);

            Assert.AreEqual(node.Attributes.Count, 2);
            Assert.AreEqual(node.GetAttribute("Attr2"), "");

            XmlHelper.AddAttribute(node, "Attr3", "");

            Assert.AreEqual(node.Attributes.Count, 3);
            Assert.AreEqual(node.GetAttribute("Attr3"), "");
        }
    }
}
