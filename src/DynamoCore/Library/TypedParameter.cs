using System;
using System.Linq;

using Dynamo.DSEngine;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Library
{

    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private string summary;

        public TypedParameter(string parameter, ProtoCore.Type type, object defaultValue = null, AssociativeNode defaultArgumentNode = null)
            : this(null, parameter, type, defaultValue, defaultArgumentNode) { }

        public TypedParameter(
            FunctionDescriptor function, string name, ProtoCore.Type type, object defaultValue = null, AssociativeNode defaultArgumentNode = null)
        {
            if (name == null) 
                throw new ArgumentNullException("name");

            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            DefaultExpression = defaultArgumentNode;
            Function = function;
        }

        public FunctionDescriptor Function { get; set; }
        public string Name { get; private set; }
        public ProtoCore.Type Type { get; private set; }
        public object DefaultValue { get; private set; }
        public AssociativeNode DefaultExpression { get; private set; }

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
            string strDefaultValue = string.Empty;
            if (DefaultExpression != null)
            {
                strDefaultValue = DefaultExpression.ToString();
            }
            else if (DefaultValue != null)
            {
                strDefaultValue = DefaultValue.ToString();
                if (DefaultValue is bool)
                {
                    strDefaultValue = strDefaultValue.ToLower();
                }
            }

            string str = Name + ": " + DisplayTypeName;
            if (!string.IsNullOrEmpty(strDefaultValue))
            {
                str = str + " = " + strDefaultValue;
            }

            return str;
        }
    }

}
