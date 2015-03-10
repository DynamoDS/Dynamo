using System;
using System.IO;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;

namespace Dynamo.Library
{
    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private string summary = string.Empty;

        public TypedParameter(string parameter, ProtoCore.Type type, object defaultValue = null)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            Name = parameter;
            Type = type;
            DefaultValue = defaultValue;
        }

        public FunctionDescriptor Function { get; private set; }
        public string Name { get; private set; }
        public ProtoCore.Type Type { get; private set; }
        public object DefaultValue { get; private set; }
        public string Summary { get { return summary; } }

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

        public void UpdateFunctionDescriptor(FunctionDescriptor funcDesc, IPathManager pathManager)
        {
            Function = funcDesc;
            summary = ((pathManager != null) ? this.GetDescription(pathManager) : string.Empty);
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
