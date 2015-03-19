using System;
using System.Collections.Generic;

namespace Dynamo.DSEngine
{
    class MemberDocumentNode
    {
        private readonly string fullyQualifiedName;
        private readonly Dictionary<string, string> parameters;

        internal string FullyQualifiedName { get { return fullyQualifiedName; } }
        internal string Summary { get; set; }
        internal string SearchTags { get; set; }
        internal IDictionary<string, string> Parameters { get { return parameters; } }

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
        internal MemberDocumentNode(string assemblyName, string memberName)
        {
            fullyQualifiedName = MakeFullyQualifiedName(assemblyName, memberName);
            parameters = new Dictionary<string, string>();
        }

        internal void AddParameter(string name, string description)
        {
            parameters[name] = description;
        }

        internal static string MakeFullyQualifiedName(string assemblyName, string memberName)
        {
            return string.Format("{0}.{1}", assemblyName, memberName);
        }
    }
}
