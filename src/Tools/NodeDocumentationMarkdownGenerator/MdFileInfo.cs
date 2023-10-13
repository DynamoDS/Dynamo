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
        /// Nodes namespace, this is used to lookup dictionary content
        /// for this node
        /// </summary>
        public string NodeNamespace { get; }

        /// <summary>
        /// Full node category, as showed in the dynamo library
        /// </summary>
        public string FullCategory { get; }

        /// <summary>
        /// Name of markdown file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Name of external library this node belongs to
        /// </summary>
        public string ExternalLibraryName { get; }

        public MdFileInfo(string nodeName, string nodeNamespace, string fullCategory, string externalLib, string fileName)
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

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or empty.", nameof(fileName));
            }

            NodeName = nodeName;
            NodeNamespace = nodeNamespace;
            FullCategory = fullCategory;
            ExternalLibraryName = externalLib;
            FileName = fileName;
        }

        internal static MdFileInfo FromCustomNode(string path)
        {
            WorkspaceInfo header = null;
            ILogger log = new DummyConsoleLogger();

            if (DynamoUtilities.PathHelper.isValidJson(path, out string jsonDoc, out Exception ex))
            {
                WorkspaceInfo.FromJsonDocument(jsonDoc, path, true, false, log, out header);
            }
            else if (DynamoUtilities.PathHelper.isValidXML(path, out XmlDocument xmlDoc, out ex))
            {
                WorkspaceInfo.FromXmlDocument(xmlDoc, path, true, false, log, out header);
            }
            else throw ex;

            if (!header.IsVisibleInDynamoLibrary) return null;

            var nodeName = header.Name;
            var nodeNamspace = header.Category;
            var fullCategory = $"{header.Category}.{header.Name}";
            return new MdFileInfo(nodeName, nodeNamspace, fullCategory, header.Category, fullCategory);
        }

        internal static bool TryGetMdFileInfoFromSearchEntry(NodeSearchElement entry, out MdFileInfo info)
        {
            try
            {
                var nodeName = string.IsNullOrEmpty(entry.Parameters) ?
                    entry.Name :
                    entry.Name + entry.Parameters;

                var category = entry.FullCategoryName;
                var nodeNamespace = "";

                string fileName = "";
                if (entry is ZeroTouchSearchElement searchElement)
                {
                    // the ZeroTouchSearchElements FulleName includes the node name at the end
                    // we need the namespace to not contain the nodename therefor we remove it here.
                    nodeNamespace = entry.FullName
                        .Remove(entry.FullName.LastIndexOf(entry.Name) - 1, entry.Name.Length + 1);

                    // Create the filename from the nodes className + nodeName
                    // We need the filename to be structured like this as this is
                    // how Dynamo matches the file with the correct node.

                    //if this type is really a global ds function - classname will be null.
                    var classNameEmpty = string.IsNullOrEmpty(searchElement.Descriptor.ClassName);
                    var seperator = classNameEmpty ? string.Empty : ".";
                    fileName = $"{searchElement.Descriptor.ClassName}{seperator}{nodeName}";
                }
                else
                {
                    nodeNamespace = entry.CreationName
                            .Remove(entry.CreationName.LastIndexOf('.'));

                    fileName = entry.CreationName;
                }

                info = new MdFileInfo(nodeName, nodeNamespace, category, entry.Assembly, fileName);
                return true;
            }
            catch (Exception e)
            {
                CommandHandler.LogExceptionToConsole(e);
                info = null;
                return false;
            }
        }
    }
}
