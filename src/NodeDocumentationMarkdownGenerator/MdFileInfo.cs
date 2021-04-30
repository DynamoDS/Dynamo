using System;

namespace NodeDocumentationMarkdownGenerator
{
    internal class MdFileInfo
    {
        public string NodeName { get; }
        public string NodeNamespace { get; }

        public MdFileInfo(string nodeName, string nodeNamespace)
        {
            NodeName = nodeName ?? throw new ArgumentNullException(nameof(nodeName));
            NodeNamespace = nodeNamespace ?? throw new ArgumentNullException(nameof(nodeNamespace));
        }
    }
}
