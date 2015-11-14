using System;
using Dynamo.Engine;
using Dynamo.Interfaces;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Library
{
    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private string summary = null; // Indicating that it is not initialized.
        private readonly string defaultValueString;

        public TypedParameter(string parameter, ProtoCore.Type type, AssociativeNode defaultValue = null, string shortArgumentName = null, string summary = null)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            Name = parameter;
            Type = type;
            DefaultValue = defaultValue;

            if (defaultValue != null)
                defaultValueString = defaultValue.ToString();
            else
                defaultValueString = shortArgumentName;

            this.summary = summary;
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
                // to be repopulated. 
                if (summary == null)
                    summary = this.GetDescription();

                return summary;
            }
        }

        public string Description
        {
            get
            {
                string description = string.Empty;
                if (!string.IsNullOrEmpty(Summary))
                    description = description + Summary + "\n\n";

                description = description + DisplayTypeName;

                if (defaultValueString != null)
                {
                    description = String.Format("{0}\n{1} : {2}",
                        description,
                        Properties.Resources.DefaultValue,
                        defaultValueString);
                }

                return description;
            }
        }

        public string DisplayTypeName
        {
            get { return Type.ToShortString(); }
        }

        public void UpdateFunctionDescriptor(FunctionDescriptor funcDesc)
        {
            Function = funcDesc;

            // Setting 'summary' to 'null' so its value is retrieved later 
            // when 'Summary' property is invoked. See 'Summary' for details.
            summary = null;
        }

        public override string ToString()
        {
            string str = Name + ": " + DisplayTypeName;
            if (defaultValueString != null)
            {
                str = str + " = " + defaultValueString;
            }

            return str;
        }
    }

}
