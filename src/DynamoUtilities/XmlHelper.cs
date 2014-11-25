using System.Xml;

namespace DynamoUtilities
{
    public class XmlHelper
    {
        public static XmlNode AddNode(XmlNode parent, string tagName, string value)
        {
            XmlElement element = parent.OwnerDocument.CreateElement(tagName);
            if (!string.IsNullOrEmpty(value))
                element.Value = value;

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
