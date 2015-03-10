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

        public TypedParameter(string parameter, ProtoCore.Type type, object defaultValue = null, string defaultExpression = null)
            : this(null, parameter, type, defaultValue) { }

        public TypedParameter(
            FunctionDescriptor function, string name, ProtoCore.Type type, object defaultValue = null, string defaultExpression = null)
        {
            if (name == null) 
                throw new ArgumentNullException("name");

            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            DefaultExpression = defaultExpression;
            Function = function;
        }

        public FunctionDescriptor Function { get; set; }
        public string Name { get; private set; }
        public ProtoCore.Type Type { get; private set; }
        public object DefaultValue { get; private set; }
        public String DefaultExpression { get; private set; }

        public string Summary
        {
            get { return summary ?? (summary = this.GetDescription()); }
        }

        public string Description
        {
            get
            {
                return !String.IsNullOrEmpty(Summary)
                    ? Summary + " (" + DisplayTypeName + ")"
                    : DisplayTypeName;
            }
        }

        public string DisplayTypeName
        {
            get { return Type.ToShortString(); }
        }

        public override string ToString()
        {
            string str = Name + ": " + DisplayTypeName;

            if (DefaultValue != null)
            {
                string strDefaultValue = DefaultValue.ToString();
                if (DefaultValue is bool)
                {
                    strDefaultValue = strDefaultValue.ToLower();
                }
                str = str + " = " + strDefaultValue;
            }

            return str;
        }
    }

}
