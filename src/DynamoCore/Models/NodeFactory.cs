using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INodeLoader<out T> where T : NodeModel
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="elNode"></param>
        /// <returns></returns>
        T CreateNodeFromXml(XmlElement elNode);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class NodeFactory : LogSourceBase
    {
        private readonly Dictionary<string, INodeLoader<NodeModel>> nodeSources =
            new Dictionary<string, INodeLoader<NodeModel>>();

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="loader"></param>
        public void AddLoader<T>(INodeLoader<T> loader) where T : NodeModel
        {
            nodeSources[typeof(T).FullName] = loader;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="alsoKnownAs"></param>
        public void AddLoader<T>(IEnumerable<string> alsoKnownAs=null) where T : NodeModel
        {
            AddLoader(typeof(T), alsoKnownAs);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="alsoKnownAs"></param>
        public void AddLoader(Type nodeType, IEnumerable<string> alsoKnownAs=null)
        {
            if (!nodeType.IsSubclassOf(typeof(NodeModel)))
                throw new ArgumentException(@"Given type is not a subclass of NodeModel.", "nodeType");

            var loader = new NodeModelTypeLoader(nodeType);

            nodeSources[nodeType.FullName] = loader;

            if (alsoKnownAs != null)
            {
                foreach (var name in alsoKnownAs)
                    nodeSources[name] = loader;
            }
        }

        private class NodeModelTypeLoader : INodeLoader<NodeModel>
        {
            private readonly Func<NodeModel> constructor;

            public NodeModelTypeLoader(Type t)
            {
                constructor = t.GetDefaultConstructor<NodeModel>();
            }

            public NodeModel CreateNodeFromXml(XmlElement elNode)
            {
                return constructor();
            }
        }

        private bool GetNodeSourceFromType(Type type, out INodeLoader<NodeModel> data)
        {
            if (nodeSources.TryGetValue(type.FullName, out data))
                return true; // Found among built-in types, return it.

            Log(string.Format("Could not load node of type: {0}", type.FullName));
            Log("Loading will continue but nodes might be missing from your workflow.");

            return false;
        }

        private bool GetNodeModelInstanceByName(string name, XmlElement elNode, SaveContext context, out NodeModel node)
        {
            Type type;
            if (!ResolveType(name, out type))
            {
                node = null;
                return false;
            }
            
            INodeLoader<NodeModel> data;
            if (!GetNodeSourceFromType(type, out data))
            {
                node = null;
                return false;
            }

            node = data.CreateNodeFromXml(elNode);
            node.Deserialize(elNode, context);
            return true;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="fullyQualifiedName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ResolveType(string fullyQualifiedName, out Type type)
        {
            if (fullyQualifiedName == null)
                throw new ArgumentNullException(@"fullyQualifiedName");

            type = Type.GetType(fullyQualifiedName);
            return type != null;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="elNode"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public NodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context)
        {
            XmlAttribute typeAttrib = elNode.Attributes["type"];
            string typeName = Nodes.Utilities.PreprocessTypeName(typeAttrib.Value);

            NodeModel node;
            if (!GetNodeModelInstanceByName(typeName, elNode, context, out node))
            {
                //TODO(Steve): Create Dummy node directly

                // If a given function is not found during file load, then convert the 
                // function node into a dummy node (instead of crashing the workflow).
                var dummyElement = MigrationManager.CreateMissingNode(elNode, 1, 1);

                // The new type representing the dummy node.
                typeName = dummyElement.GetAttribute("type");
                GetNodeModelInstanceByName(typeName, dummyElement, context, out node);
                node.Deserialize(dummyElement, context);
            }
            return node;

            //TODO(Steve): This should go in the INodeSource for DSFunction
            #region ZeroTouch func can't be found
            try
            {

            }
            catch (UnresolvedFunctionException)
            {
                // If a given function is not found during file load, then convert the 
                // function node into a dummy node (instead of crashing the workflow).
                // 
                var e = elNode as XmlElement;
                dummyElement = MigrationManager.CreateUnresolvedFunctionNode(e);
            }
            #endregion

            //TODO(Steve): This should go in the INodeSource for CustomNodes
            #region Custom Node def can't be found

            // If a custom node fails to load its definition, convert it into a dummy node.
            if ((function != null) && (function.Definition == null))
            {
                var e = elNode as XmlElement;
                dummyElement = MigrationManager.CreateMissingNode(
                    e, function.InPortData.Count, function.OutPortData.Count);
            }

            #endregion

            //TODO(Steve): This should go in both of the above.
            if (dummyElement != null) // If a dummy node placement is desired.
            {
                // The new type representing the dummy node.
                typeName = dummyElement.GetAttribute("type");
                var type = Dynamo.Nodes.Utilities.ResolveType(dynamoModel, typeName);

                node = NodeFactory.CreateNodeInstance(type, nickname, string.Empty, guid);
                node.Load(dummyElement);
            }
        }
    }

    /// <summary>
    ///     Xml Loader for ZeroTouch nodes.
    /// </summary>
    public class ZeroTouchNodeLoader : INodeLoader<DSFunctionBase>
    {
        private readonly LibraryServices libraryServices;

        public ZeroTouchNodeLoader(LibraryServices libraryServices)
        {
            this.libraryServices = libraryServices;
        }

        public DSFunctionBase CreateNodeFromXml(XmlElement nodeElement)
        {
            string assembly = null;
            string function;

            FunctionDescriptor descriptor;

            Trace.Assert(nodeElement.Attributes != null, "nodeElement.Attributes != null");

            if (nodeElement.Attributes["assembly"] == null && nodeElement.Attributes["function"] == null)
            {
                // To open old file
                var helper =
                    nodeElement.ChildNodes.Cast<XmlElement>()
                        .Where(subNode => subNode.Name.Equals(typeof(FunctionDescriptor).FullName))
                        .Select(subNode => new XmlElementHelper(subNode))
                        .FirstOrDefault();

                if (helper != null)
                {
                    assembly = helper.ReadString("Assembly", "");
                }

                function = nodeElement.Attributes["nickname"].Value.Replace(".get", ".");
            }
            else
            {
                var xmlAttribute = nodeElement.Attributes["assembly"];
                if (xmlAttribute != null)
                    assembly = xmlAttribute.Value;

                string xmlSignature = nodeElement.Attributes["function"].Value;

                string hintedSigniture =
                    libraryServices.FunctionSignatureFromFunctionSignatureHint(xmlSignature);

                function = hintedSigniture ?? xmlSignature;
            }

            if (!string.IsNullOrEmpty(assembly))
            {
                var document = nodeElement.OwnerDocument;
                var docPath = Nodes.Utilities.GetDocumentXmlPath(document);
                assembly = Nodes.Utilities.MakeAbsolutePath(docPath, assembly);

                libraryServices.ImportLibrary(assembly);
                descriptor = libraryServices.GetFunctionDescriptor(assembly, function);
            }
            else
            {
                descriptor = libraryServices.GetFunctionDescriptor(function);
            }

            if (null == descriptor)
            {
                throw new UnresolvedFunctionException(function);
            }

            DSFunctionBase result = descriptor.IsVarArg
                ? new DSVarArgFunction(descriptor) as DSFunctionBase
                : new DSFunction(descriptor);

            //TODO(Steve): Move to DSFunctionBase constructor
            if (descriptor.IsObsolete)
                result.Warning(descriptor.ObsoleteMessage);

            return result;
        }
    }

    /// <summary>
    ///     Xml Loader for Custom Nodes.
    /// </summary>
    public class CustomNodeLoader : INodeLoader<Function>
    {
        private readonly CustomNodeManager customNodeManager;
        private readonly bool isTestMode;

        public CustomNodeLoader(CustomNodeManager customNodeManager, bool isTestMode=false)
        {
            this.customNodeManager = customNodeManager;
            this.isTestMode = isTestMode;
        }
        
        public Function CreateNodeFromXml(XmlElement nodeElement)
        {
            XmlNode idNode =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .LastOrDefault(subNode => subNode.Name.Equals("ID"));

            if (idNode == null || idNode.Attributes == null) 
                return null;

            string id = idNode.Attributes[0].Value;

            string nickname = nodeElement.Attributes["nickname"].Value;

            Guid funcId;
            if (!Guid.TryParse(id, out funcId))
            {
                funcId = GuidUtility.Create(GuidUtility.UrlNamespace, nickname);
            }

            var node = customNodeManager.CreateCustomNodeInstance(funcId, nickname, isTestMode);
            if (node == null)
            {
                //TODO(Steve): Create and return proxy instead.
            }
            return node;
        }
    }
}
