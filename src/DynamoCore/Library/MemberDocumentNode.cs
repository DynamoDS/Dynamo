using System;
using System.Collections.Generic;

namespace Dynamo.DSEngine
{
    class MemberDocumentNode
    {
        private readonly string fullyQualifiedName;
        private string summary = String.Empty;
        private string searchTags = String.Empty;
        private readonly Dictionary<string, string> parameters;

        public string FullyQualifiedName { get { return fullyQualifiedName; } }

        public string Summary
        {
            get
            {
                return summary;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                summary = value;
            }
        }

        public string SearchTags
        {
            get
            {
                return searchTags;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                searchTags = value;
            }
        }

        public IDictionary<string, string> Parameters { get { return parameters; } }

        /// <summary>
        /// Constructs an instance of MemberDocumentNode object from its 
        /// given assembly and member name. 
        /// </summary>
        /// <param name="assemblyName">The assembly inside which this member 
        /// resides. If this parameter is null or empty, ArgumentNullException
        /// is thrown.</param>
        /// <param name="memberName">The fully qualified name that can be used
        /// to uniquely identify the member within the same assembly. For an 
        /// example:
        /// 
        ///     "M:Autodesk.DesignScript.Geometry.Point.ByCoordinates(System.Double,System.Double)"
        /// 
        /// </param>
        /// 
        public MemberDocumentNode(string assemblyName, string memberName)
        {
            if (String.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentNullException("Assembly Name");

            fullyQualifiedName = MakeFullyQualifiedName(assemblyName, memberName);
            parameters = new Dictionary<string, string>();
        }

        internal void AddParameter(string name, string description)
        {
            parameters[name] = description;
        }

        internal static string MakeFullyQualifiedName(string assemblyName, string memberName)
        {
            return string.Format("{0},{1}", assemblyName, memberName);
        }
    }
}
