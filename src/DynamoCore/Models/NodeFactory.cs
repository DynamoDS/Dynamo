using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using DSCoreNodesUI;

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
        private readonly Dictionary<Type, INodeLoader<NodeModel>> nodeSources =
            new Dictionary<Type, INodeLoader<NodeModel>>();

        private readonly Dictionary<string, Type> alsoKnownAsMappings =
            new Dictionary<string, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loader"></param>
        public void AddLoader<T>(INodeLoader<T> loader) where T : NodeModel
        {
            AddLoader(typeof(T), loader);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeType"></param>
        /// <param name="loader"></param>
        /// <param name="alsoKnownAs"></param>
        public void AddLoader<T>(Type nodeType, INodeLoader<T> loader, IEnumerable<string> alsoKnownAs=null) where T : NodeModel
        {
            if (!nodeType.IsSubclassOf(typeof(NodeModel)))
                throw new ArgumentException(@"Given type is not a subclass of NodeModel.", "nodeType");

            nodeSources[nodeType] = loader;
            alsoKnownAsMappings[nodeType.FullName] = nodeType;
            
            if (alsoKnownAs != null)
            {
                foreach (var name in alsoKnownAs)
                    alsoKnownAsMappings[name] = nodeType;
            }
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

            try
            {
                var loader = new NodeModelTypeLoader(nodeType);
                AddLoader(nodeType, loader, alsoKnownAs);
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="aka"></param>
        /// <param name="realType"></param>
        /// <param name="overwrite"></param>
        public void AddAlsoKnownAs(string aka, Type realType, bool overwrite=false)
        {
            if (!overwrite && alsoKnownAsMappings.ContainsKey(aka))
                throw new InvalidOperationException(string.Format("There already exists an AlsoKnownAs mapping for {0}.", aka));
            alsoKnownAsMappings[aka] = realType;
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
            if (GetNodeSourceFromTypeHelper(type, out data))
                return true; // Found among built-in types, return it.

            Log(string.Format("Could not load node of type: {0}", type.FullName));
            Log("Loading will continue but nodes might be missing from your workflow.");

            return false;
        }

        private bool GetNodeSourceFromTypeHelper(Type type, out INodeLoader<NodeModel> data)
        {
            while (true)
            {
                if (type == null || type == typeof(NodeModel))
                {
                    data = null;
                    return false;
                }

                if (nodeSources.TryGetValue(type, out data))
                    return true; // Found among built-in types, return it.

                type = type.BaseType;
            }
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
        public bool ResolveType(string fullyQualifiedName, out Type type)
        {
            if (fullyQualifiedName == null)
                throw new ArgumentNullException(@"fullyQualifiedName");

            if (alsoKnownAsMappings.TryGetValue(fullyQualifiedName, out type))
                return true;

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
                node = new DummyNode(1, 1, typeName, elNode, "", DummyNode.Nature.Deprecated);
            return node;
        }
    }

    /// <summary>
    ///     Xml Loader for CodeBlock nodes.
    /// </summary>
    public class CodeBlockNodeLoader : INodeLoader<CodeBlockNodeModel>
    {
        private readonly IEngineControllerManager engineManager;

        public CodeBlockNodeLoader(IEngineControllerManager manager)
        {
            engineManager = manager;
        }

        public CodeBlockNodeModel CreateNodeFromXml(XmlElement elNode)
        {
            return new CodeBlockNodeModel(engineManager.EngineController.LiveRunnerCore);
        }
    }

    /// <summary>
    ///     Xml Loader for ZeroTouch nodes.
    /// </summary>
    public class ZeroTouchNodeLoader : INodeLoader<NodeModel>
    {
        private readonly LibraryServices libraryServices;

        public ZeroTouchNodeLoader(LibraryServices libraryServices)
        {
            this.libraryServices = libraryServices;
        }

        public NodeModel CreateNodeFromXml(XmlElement nodeElement)
        {
            string assembly = "";
            string function;
            var nickname = nodeElement.Attributes["nickname"].Value;

            FunctionDescriptor descriptor;

            Trace.Assert(nodeElement.Attributes != null, "nodeElement.Attributes != null");

            if (nodeElement.Attributes["assembly"] == null)
            {
                assembly = DetermineAssemblyName(nodeElement);
                function = nickname.Replace(".get", ".");
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
                var inputcount = DetermineFunctionInputCount(nodeElement);
                return new DummyNode(
                    inputcount,
                    1,
                    nickname,
                    nodeElement,
                    assembly,
                    DummyNode.Nature.Unresolved);
            }

            DSFunctionBase result = descriptor.IsVarArg
                ? new DSVarArgFunction(descriptor) as DSFunctionBase
                : new DSFunction(descriptor);

            return result;
        }

        private static int DetermineFunctionInputCount(XmlElement element)
        {
            int additionalPort = 0;

            // "DSVarArgFunction" is a "VariableInputNode", therefore it will 
            // have "inputcount" as one of the attributes. If such attribute 
            // does not exist, throw an ArgumentException.
            if (element.Name.Equals("Dynamo.Nodes.DSVarArgFunction"))
            {
                var inputCountAttrib = element.Attributes["inputcount"];

                if (inputCountAttrib == null)
                {
                    throw new ArgumentException(string.Format(
                        "Function inputs cannot be determined ({0}).",
                        element.GetAttribute("nickname")));
                }

                return Convert.ToInt32(inputCountAttrib.Value);
            }

            var signature = string.Empty;
            var signatureAttrib = element.Attributes["function"];
            if (signatureAttrib != null)
                signature = signatureAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                signature = string.Format("{0}@{1}",
                    childElement.GetAttribute("DisplayName"),
                    childElement.GetAttribute("Parameters").Replace(';', ','));

                // We need one more port for instance methods/properties.
                switch (childElement.GetAttribute("Type"))
                {
                    case "InstanceMethod":
                    case "InstanceProperty":
                        additionalPort = 1; // For taking the instance itself.
                        break;
                }
            }

            if (string.IsNullOrEmpty(signature))
            {
                const string message = "Function signature cannot be determined.";
                throw new ArgumentException(message);
            }

            int atSignIndex = signature.IndexOf('@');
            if (atSignIndex >= 0) // An '@' sign found, there's param information.
            {
                signature = signature.Substring(atSignIndex + 1); // Skip past '@'.
                var parts = signature.Split(new[] { ',' });
                return (parts.Length) + additionalPort;
            }

            return additionalPort + 1; // At least one.
        }

        private static string DetermineAssemblyName(XmlElement element)
        {
            var assemblyName = string.Empty;
            var assemblyAttrib = element.Attributes["assembly"];
            if (assemblyAttrib != null)
                assemblyName = assemblyAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                var funcItemAsmAttrib = childElement.Attributes["Assembly"];
                if (funcItemAsmAttrib != null)
                    assemblyName = funcItemAsmAttrib.Value;
            }

            if (string.IsNullOrEmpty(assemblyName))
                return string.Empty;

            try { return Path.GetFileName(assemblyName); }
            catch (Exception) { return string.Empty; }
        }
    }

    /// <summary>
    ///     Xml Loader for Custom Nodes.
    /// </summary>
    public class CustomNodeLoader : INodeLoader<Function>
    {
        private readonly ICustomNodeSource customNodeManager;
        private readonly bool isTestMode;

        public CustomNodeLoader(ICustomNodeSource customNodeManager, bool isTestMode = false)
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
                funcId = GuidUtility.Create(GuidUtility.UrlNamespace, nickname);

            return customNodeManager.CreateCustomNodeInstance(funcId, nickname, isTestMode);
        }
    }
}
