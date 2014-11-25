using System.Xml;

namespace DynamoUtilities
{
    public class XmlHelper
    {
        public static XmlDocument CreateDocument(string rootName)
        {
            var document = new XmlDocument();
            document.InsertBefore(document.CreateXmlDeclaration("1.0", "UTF-8", null), document.DocumentElement);
            document.AppendChild(document.CreateElement(rootName));

            return document;
        }

        public static XmlNode AddNode(XmlNode parent, string name, string value)
        {
            XmlElement element = parent.OwnerDocument.CreateElement(name);

            if (!string.IsNullOrEmpty(value))
            {
                XmlText text = parent.OwnerDocument.CreateTextNode(value);
                element.AppendChild(text);
            }

            return parent.AppendChild(element);
        }

        public static void AddAttribute(XmlNode parent, string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                return;

            XmlAttribute attribute = parent.OwnerDocument.CreateAttribute(name);
            attribute.Value = value;

            parent.Attributes.Append(attribute);
        }
    }
}
