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

        /// <summary>
        /// This function creates TypedParameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type</param>
        /// <param name="defaultValue">parameter default value</param>
        /// <param name="shortArgumentName">short name is used as tooltip</param>
        /// <param name="summary">parameter description</param>
        public TypedParameter(string name, ProtoCore.Type type, AssociativeNode defaultValue = null, string shortArgumentName = null, string summary = null)
        {
            if (name == null)
                throw new ArgumentNullException("parameter");

            Name = name;
            Type = type;
            DefaultValue = defaultValue;

            if (defaultValue != null)
                defaultValueString = defaultValue.ToString();
            else
                defaultValueString = shortArgumentName;

            this.summary = summary;
        }

        /// <summary>
        /// Returns DesignScript function.
        /// </summary>
        public FunctionDescriptor Function { get; private set; }

        /// <summary>
        /// Returns the name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns type of the parameter.
        /// </summary>
        public ProtoCore.Type Type { get; private set; }

        /// <summary>
        /// Returns default value of the parameter.
        /// </summary>
        public AssociativeNode DefaultValue { get; private set; }

        /// <summary>
        /// Returns summary of the parameter.
        /// </summary>
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

        /// <summary>
        /// Returns description of the parameter.
        /// </summary>
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

        /// <summary>
        /// Returns short type name of the parameter.
        /// </summary>
        public string DisplayTypeName
        {
            get { return Type.ToShortString(); }
        }

        internal void UpdateFunctionDescriptor(FunctionDescriptor funcDesc)
        {
            Function = funcDesc;

            // Setting 'summary' to 'null' so its value is retrieved later 
            // when 'Summary' property is invoked. See 'Summary' for details.
            summary = null;
        }

        /// <summary>
        /// Turns into string.
        /// </summary>
        /// <returns>string</returns>
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
