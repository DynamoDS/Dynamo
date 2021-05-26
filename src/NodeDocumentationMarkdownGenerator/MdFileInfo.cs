using System;
using System.Xml;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Search.SearchElements;

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

        public string FullCategory { get; }

        public string QualifiedFileName { get; }

        /// <summary>
        /// Name of external library this node belongs to
        /// </summary>
        public string ExternalLib { get; }

        public MdFileInfo(string nodeName, string nodeNamespace, string fullCategory, string externalLib, string qualifiedName)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException($"'{nameof(nodeName)}' cannot be null or empty.", nameof(nodeName));
            }

            if (string.IsNullOrEmpty(nodeNamespace))
            {
                throw new ArgumentException($"'{nameof(nodeNamespace)}' cannot be null or empty.", nameof(nodeNamespace));
            }

            if (string.IsNullOrEmpty(fullCategory))
            {
                throw new ArgumentException($"'{nameof(fullCategory)}' cannot be null or empty.", nameof(fullCategory));
            }

            if (string.IsNullOrEmpty(externalLib))
            {
                throw new ArgumentException($"'{nameof(externalLib)}' cannot be null or empty.", nameof(externalLib));
            }

            if (string.IsNullOrEmpty(qualifiedName))
            {
                throw new ArgumentException($"'{nameof(qualifiedName)}' cannot be null or empty.", nameof(qualifiedName));
            }

            NodeName = nodeName;
            NodeNamespace = nodeNamespace;
            FullCategory = fullCategory;
            ExternalLib = externalLib;
            QualifiedFileName = qualifiedName;
        }

        internal static MdFileInfo FromCustomNode(string path, ILogger log)
        {
            WorkspaceInfo header = null;

            if (DynamoUtilities.PathHelper.isValidXML(path, out XmlDocument xmlDoc, out Exception ex))
            {
                WorkspaceInfo.FromXmlDocument(xmlDoc, path, true, false, log, out header);
            }
            else if (DynamoUtilities.PathHelper.isValidJson(path, out string jsonDoc, out ex))
            {
                WorkspaceInfo.FromJsonDocument(jsonDoc, path, true, false, log, out header);
            }
            else throw ex;

            if (!header.IsVisibleInDynamoLibrary) return null;

            var nodeName = header.Name;
            var nodeNamspace = header.Category;
            var fullCategory = $"{header.Category}.{header.Name}";
            return new MdFileInfo(nodeName, nodeNamspace, fullCategory, header.Category, nodeNamspace);
        }

        internal static bool TryGetFromSearchEntry(NodeSearchElement entry, ILogger logger, out MdFileInfo info)
        {
            try
            {
                var nodeName = string.IsNullOrEmpty(entry.Parameters) ?
                    entry.Name :
                    entry.Name + entry.Parameters;

                var category = entry.FullCategoryName;
                var nodeNamespace = "";

                string fileName = "";
                if (entry is ReflectionZeroTouhSearchElement reflectionSearch)
                {
                    nodeNamespace = entry.FullName
                        .Remove(entry.FullName.LastIndexOf(entry.Name) - 1, entry.Name.Length + 1);

                    var qualifiedNameWithOutNodeName = reflectionSearch.Descriptor.QualifiedName
                        .Remove(reflectionSearch.Descriptor.QualifiedName.LastIndexOf(entry.Name) - 1, entry.Name.Length + 1);

                    fileName = $"{qualifiedNameWithOutNodeName}.{nodeName}";
                }
                else
                {
                    nodeNamespace = entry.CreationName;
                    fileName = $"{nodeNamespace}.{nodeName}";
                }

                info = new MdFileInfo(nodeName, nodeNamespace, category, entry.Assembly, fileName);
                return true;
            }
            catch (Exception e)
            {
                logger.Log(e);
                info = null;
                return false;
            }
        }
    }
}
