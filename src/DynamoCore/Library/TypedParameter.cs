using System;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Library
{
    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private IPathManager pathManager;
        private string summary = null; // Indicating that it is not initialized.

        public TypedParameter(string parameter, ProtoCore.Type type, AssociativeNode defaultValue = null)
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
        public AssociativeNode DefaultValue { get; private set; }

        public string Summary
        {
            get
            {
                // If 'summary' data member is 'null', it means its value has 
                // to be repopulated. If an IPathManager is supplied, then 
                // retrieve the description through it, otherwise set 'summary'
                // to empty string, so it wil not keep retrieving after failing 
                // once.
                // 
                return summary ?? (summary = ((pathManager != null)
                    ? this.GetDescription(pathManager) : string.Empty));
            }
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

        public void UpdateFunctionDescriptor(FunctionDescriptor funcDesc, IPathManager pathManager)
        {
            Function = funcDesc;

            // Setting 'summary' to 'null' so its value is retrieved later 
            // when 'Summary' property is invoked. See 'Summary' for details.
            summary = null;
            this.pathManager = pathManager;
        }

        public override string ToString()
        {
            string str = Name + ": " + DisplayTypeName;
            if (DefaultValue != null)
            {
                str = str + " = " + DefaultValue.ToString();
            }

            return str;
        }
    }

}
