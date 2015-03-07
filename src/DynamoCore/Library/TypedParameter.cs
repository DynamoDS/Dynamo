using System;
using System.Linq;

using Dynamo.DSEngine;

namespace Dynamo.Library
{
    public struct TypedParameterParams
    {
        public TypedParameterParams(string parameterName, ProtoCore.Type type) : 
            this()
        {
            ParameterName = parameterName;
            Type = type;
        }

        public string ParameterName { get; set; }
        public ProtoCore.Type Type { get; set; }
        public object DefaultValue { get; set; }
        public FunctionDescriptor Function { get; set; }
    }

    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private string summary;

        public TypedParameter(TypedParameterParams parameter)
        {
            if (string.IsNullOrEmpty(parameter.ParameterName))
                throw new ArgumentException("Invalid ParameterName");

            Name = parameter.ParameterName;
            Type = parameter.Type;
            DefaultValue = parameter.DefaultValue;
            Function = parameter.Function;
        }

        public FunctionDescriptor Function { get; set; }
        public string Name { get; private set; }
        public ProtoCore.Type Type { get; private set; }
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
