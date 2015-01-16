using System;
using System.Linq;

using Dynamo.DSEngine;

namespace Dynamo.Library
{

    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private string summary;

        public TypedParameter(string parameter, string type, object defaultValue = null)
            : this(null, parameter, type, defaultValue) { }

        public TypedParameter(
            FunctionDescriptor function, string name, string type, object defaultValue = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();
            Name = name;

            if (null == type)
                throw new ArgumentNullException(/*NXLT*/"type", /*NXLT*/@"Type cannot be null.");
            Type = type;
            DefaultValue = defaultValue;
            Function = function;
        }

        public FunctionDescriptor Function { get; set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public object DefaultValue { get; private set; }

        public string Summary
        {
            get { return summary ?? (summary = this.GetDescription()); }
        }

        public string Description
        {
            get
            {
                return !String.IsNullOrEmpty(Summary)
                    ? Summary + /*NXLT*/" (" + (string.IsNullOrEmpty(Type) ? /*NXLT*/"var" : DisplayTypeName) + /*NXLT*/")"
                    : (string.IsNullOrEmpty(Type) ? /*NXLT*/"var" : DisplayTypeName);
            }
        }

        public string DisplayTypeName
        {
            get { return Type.Split('.').Last(); }
        }

        public override string ToString()
        {
            string str = Name;

            if (!String.IsNullOrEmpty(Type))
                str = Name + /*NXLT*/": " + Type.Split('.').Last();

            if (DefaultValue != null)
            {
                string strDefaultValue = DefaultValue.ToString();
                if (DefaultValue is bool)
                {
                    strDefaultValue = strDefaultValue.ToLower();
                }
                str = str + /*NXLT*/" = " + strDefaultValue;
            }

            return str;
        }
    }

}
