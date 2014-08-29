using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dynamo.Utilities
{
    public class XmlElementHelper
    {
        private XmlElement internalElement = null;

        public XmlElementHelper(XmlElement xmlElement)
        {
            this.internalElement = xmlElement;
        }

        #region Attributes - Write Methods

        public void SetAttribute(string name, int value)
        {
            string s = value.ToString(CultureInfo.InvariantCulture);
            internalElement.SetAttribute(name, s);
        }

        public void SetAttribute(string name, double value)
        {
            string s = value.ToString(CultureInfo.InvariantCulture);
            internalElement.SetAttribute(name, s);
        }

        public void SetAttribute(string name, bool value)
        {
            string s = value.ToString().ToLower();
            internalElement.SetAttribute(name, s);
        }

        public void SetAttribute(string name, string value)
        {
            internalElement.SetAttribute(name, value);
        }

        public void SetAttribute(string name, System.Type value)
        {
            internalElement.SetAttribute(name, value.ToString());
        }

        public void SetAttribute(string name, System.Guid value)
        {
            internalElement.SetAttribute(name, value.ToString());
        }

        public void SetAttribute(string name, System.Windows.Media.Color value)
        {
            var converter = new System.Windows.Media.BrushConverter();
            var attribValue = converter.ConvertToString(value);
            internalElement.SetAttribute(name, attribValue);
        }

        #endregion

        #region Attributes - Read Methods

        public int ReadInteger(string attribName)
        {
            XmlAttribute attrib = GetGuaranteedAttribute(attribName);
            return int.Parse(attrib.Value);
        }

        public int ReadInteger(string attribName, int defaultValue)
        {
            int result = defaultValue;
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib || (!int.TryParse(attrib.Value, out result)))
                return defaultValue;

            return result;
        }

        public double ReadDouble(string attribName)
        {
            XmlAttribute attrib = GetGuaranteedAttribute(attribName);
            return double.Parse(attrib.Value, CultureInfo.InvariantCulture);
        }

        public double ReadDouble(string attribName, double defaultValue)
        {
            double result = defaultValue;
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib || (!double.TryParse(attrib.Value, out result)))
                return defaultValue;

            return result;
        }

        public bool ReadBoolean(string attribName)
        {
            XmlAttribute attrib = GetGuaranteedAttribute(attribName);
            return Boolean.Parse(attrib.Value);
        }

        public bool ReadBoolean(string attribName, bool defaultValue)
        {
            bool result = defaultValue;
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib || (!Boolean.TryParse(attrib.Value, out result)))
                return defaultValue;

            return result;
        }

        public string ReadString(string attribName)
        {
            XmlAttribute attrib = GetGuaranteedAttribute(attribName);
            return attrib.Value;
        }

        public string ReadString(string attribName, string defaultValue)
        {
            XmlAttribute attrib = internalElement.Attributes[attribName];
            return ((null == attrib) ? defaultValue : attrib.Value);
        }

        public System.Guid ReadGuid(string attribName)
        {
            XmlAttribute attrib = GetGuaranteedAttribute(attribName);
            return Guid.Parse(attrib.Value);
        }

        public System.Guid ReadGuid(string attribName, System.Guid defaultValue)
        {
            System.Guid result = defaultValue;
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib || (!Guid.TryParse(attrib.Value, out result)))
                return defaultValue;

            return result;
        }

        public System.Windows.Media.Color ReadColor(string attribName)
        {
            XmlAttribute attrib = GetGuaranteedAttribute(attribName);
            var converter = new System.Windows.Media.BrushConverter();
            var brush = converter.ConvertFromString(attrib.Value);
            return (brush as System.Windows.Media.SolidColorBrush).Color;
        }

        public System.Windows.Media.Color ReadColor(string attribName,
            System.Windows.Media.Color defaultValue)
        {
            System.Windows.Media.Color result = defaultValue;
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib)
                return defaultValue;

            try
            {
                var converter = new System.Windows.Media.BrushConverter();
                var brush = converter.ConvertFromString(attrib.Value);
                result = (brush as System.Windows.Media.SolidColorBrush).Color;
            }
            catch (Exception) { }

            return result;
        }

        public TEnum ReadEnum<TEnum>(string attribName, TEnum defaultValue) where TEnum : struct
        {
            TEnum result = defaultValue;
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib || (!System.Enum.TryParse(attrib.Value, out result)))
                return defaultValue;

            return result;
        }

        private XmlAttribute GetGuaranteedAttribute(string attribName)
        {
            XmlAttribute attrib = internalElement.Attributes[attribName];
            if (null == attrib)
            {
                string fmt = "Mandatory attribute '{0}' does not exist";
                throw new InvalidOperationException(string.Format(fmt, attribName));
            }

            return attrib;
        }

        #endregion
    }
}

