using System;
using System.Collections.Generic;

namespace Dynamo.DSEngine
{
    class MemberDocumentNode
    {
        private readonly string fullyQualifiedName;
        private string summary;
        private string searchTags;
        private readonly Dictionary<string, string> parameters;

        public string FullyQualifiedName { get { return fullyQualifiedName; } }

        public string Summary
        {
            get
            {
                if (String.IsNullOrEmpty(summary))
                    return String.Empty;
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
                if (String.IsNullOrEmpty(searchTags))
                    return String.Empty;
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
        ///     "DSCore.IO.File.ReadText(System.IO.FileInfo)"
        /// 
        /// </param>
        /// 
        public MemberDocumentNode(string memberName)
        {
            fullyQualifiedName = memberName;
            parameters = new Dictionary<string, string>();
        }

        public MemberDocumentNode()
        {
            parameters = new Dictionary<string, string>();
        }

        public void AddParameter(string name, string description)
        {
            parameters[name] = description;
        }
    }
}
