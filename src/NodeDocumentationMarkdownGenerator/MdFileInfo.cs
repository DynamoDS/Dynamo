using System;

namespace NodeDocumentationMarkdownGenerator
{
    internal class MdFileInfo
    {
        /// <summary>
        /// Name of the node
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// Nodes namespace, this is used to generate the md file name
        /// </summary>
        public string NodeNamespace { get; }

        public MdFileInfo(string nodeName, string nodeNamespace)
        {
            NodeName = nodeName ?? throw new ArgumentNullException(nameof(nodeName));
            NodeNamespace = nodeNamespace ?? throw new ArgumentNullException(nameof(nodeNamespace));
        }
    }
}
