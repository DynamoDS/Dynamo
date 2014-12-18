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
    ///     An object which can load a NodeModel from Xml.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INodeLoader<out T> where T : NodeModel
    {
        /// <summary>
        ///     Create a new NodeModel from its serialized form.
        /// </summary>
        /// <param name="elNode">Serialized NodeModel</param>
        /// <param name="context">Serialization context</param>
        /// <returns></returns>
        T CreateNodeFromXml(XmlElement elNode, SaveContext context);
    }

    /// <summary>
    ///     An object which can create a new NodeModel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INodeFactory<out T> where T : NodeModel
    {
        /// <summary>
        ///     Creates a new NodeModel instance.
        /// </summary>
        /// <returns></returns>
        T CreateNode();
    }

    /// <summary>
    ///     Manages factories and loaders for NodeModels. Can use registered factories
    ///     and loaders to instantiate and load new NodeModels.
    /// </summary>
    public class NodeFactory : LogSourceBase
    {
        private readonly Dictionary<Type, INodeFactory<NodeModel>> nodeFactories =
            new Dictionary<Type, INodeFactory<NodeModel>>();

        private readonly Dictionary<Type, INodeLoader<NodeModel>> nodeLoaders =
            new Dictionary<Type, INodeLoader<NodeModel>>();

        private readonly Dictionary<string, Type> alsoKnownAsMappings =
            new Dictionary<string, Type>();

        /// <summary>
        ///     Adds a node loader to this manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loader"></param>
        public void AddLoader<T>(INodeLoader<T> loader) where T : NodeModel
        {
            AddLoader(typeof(T), loader);
        }

        /// <summary>
        ///     Adds a node loader to this manager, for the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeType"></param>
        /// <param name="loader"></param>
        public void AddLoader<T>(Type nodeType, INodeLoader<T> loader) where T : NodeModel
        {
            if (!nodeType.IsSubclassOf(typeof(NodeModel)))
                throw new ArgumentException(@"Given type is not a subclass of NodeModel.", "nodeType");

            nodeLoaders[nodeType] = loader;
            alsoKnownAsMappings[nodeType.FullName] = nodeType;
        }

        /// <summary>
        ///     Adds a node factory to this manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loader"></param>
        public void AddFactory<T>(INodeFactory<T> loader) where T : NodeModel
        {
            AddFactory(typeof(T), loader);
        }

        /// <summary>
        ///     Adds a node factory to this manager, for a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeType"></param>
        /// <param name="loader"></param>
        public void AddFactory<T>(Type nodeType, INodeFactory<T> loader) where T : NodeModel
        {
            if (!nodeType.IsSubclassOf(typeof(NodeModel)))
                throw new ArgumentException(@"Given type is not a subclass of NodeModel.", "nodeType");

            nodeFactories[nodeType] = loader;
            alsoKnownAsMappings[nodeType.FullName] = nodeType;
        }

        /// <summary>
        ///     Attempts to create a new factory and loader for a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public bool AddTypeFactoryAndLoader<T>() where T : NodeModel
        {
            return AddTypeFactoryAndLoader(typeof(T));
        }

        /// <summary>
        ///     Attempts to create a new factory and loader for a given type.
        /// </summary>
        /// <param name="nodeType"></param>
        public bool AddTypeFactoryAndLoader(Type nodeType)
        {
            if (!nodeType.IsSubclassOf(typeof(NodeModel)))
                throw new ArgumentException(@"Given type is not a subclass of NodeModel.", "nodeType");

            try
            {
                var loader = new NodeModelTypeLoader(nodeType);
                AddLoader(nodeType, loader);
                AddFactory(nodeType, loader); // We don't re-use alsoKnownAs here, they are already registered from AddLoader.
                return true;
            }
            catch (Exception e)
            {
                Log(e);
                return false;
            }
        }

        /// <summary>
        ///     Registers a type with another name that it may go by.
        /// </summary>
        /// <param name="realType"></param>
        /// <param name="aka"></param>
        /// <param name="overwrite"></param>
        public void AddAlsoKnownAs(Type realType, string aka, bool overwrite = false)
        {
            Type old;
            if (!overwrite && alsoKnownAsMappings.TryGetValue(aka, out old) && old != realType)
            {
                Log(
                    new InvalidOperationException(
                        string.Format("There already exists an AlsoKnownAs mapping for {0}.", aka)));
                return;
            }
            alsoKnownAsMappings[aka] = realType;
        }

        /// <summary>
        ///     Registers a type with other names that it may go by.
        /// </summary>
        /// <param name="realType"></param>
        /// <param name="names"></param>
        /// <param name="overwrite"></param>
        public void AddAlsoKnownAs(Type realType, IEnumerable<string> names, bool overwrite = false)
        {
            foreach (var aka in names)
                AddAlsoKnownAs(realType, aka, overwrite);
        }

        private sealed class NodeModelTypeLoader : INodeLoader<NodeModel>, INodeFactory<NodeModel>
        {
            private readonly Func<NodeModel> constructor;

            public NodeModelTypeLoader(Type t)
            {
                constructor = t.GetDefaultConstructor<NodeModel>();
            }

            public NodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context)
            {
                var node = CreateNode();
                node.Deserialize(elNode, context);
                return node;
            }

            public NodeModel CreateNode()
            {
                return constructor();
            }
        }

        private bool GetNodeSourceFromType(Type type, out INodeLoader<NodeModel> data)
        {
            if (GetNodeSourceFromTypeHelper(type, out data))
                return true; // Found among built-in types, return it.

            Log(string.Format("Could not load node of type: {0}", type.FullName));

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

                if (nodeLoaders.TryGetValue(type, out data))
                    return true; // Found among built-in types, return it.

                type = type.BaseType;
            }
        }

        private bool LoadNodeModelInstanceByType(Type type, XmlElement elNode, SaveContext context, out NodeModel node)
        {
            INodeLoader<NodeModel> data;
            if (!GetNodeSourceFromType(type, out data))
            {
                node = null;
                return false;
            }

            node = data.CreateNodeFromXml(elNode, context);
            return true;
        }

        private bool GetNodeFactoryFromType(Type type, out INodeFactory<NodeModel> data)
        {
            if (GetNodeFactoryFromTypeHelper(type, out data))
                return true; // Found among built-in types, return it.

            Log(string.Format("Could not load node of type: {0}", type.FullName));
            return false;
        }

        private bool GetNodeFactoryFromTypeHelper(Type type, out INodeFactory<NodeModel> data)
        {
            while (true)
            {
                if (type == null || type == typeof(NodeModel))
                {
                    data = null;
                    return false;
                }

                if (nodeFactories.TryGetValue(type, out data))
                    return true; // Found among built-in types, return it.

                type = type.BaseType;
            }
        }

        private bool CreateNodeModelInstanceByType(Type type, out NodeModel node)
        {
            INodeFactory<NodeModel> data;
            if (!GetNodeFactoryFromType(type, out data))
            {
                node = null;
                return false;
            }

            node = data.CreateNode();
            return true;
        }

        /// <summary>
        ///     Given a type name, attempts to get the type associated with that name.
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
        ///     Creates and Loads a new NodeModel from its Serialized form, using the node loaders
        ///     registered in this factory. If loading fails, a Dummy Node is produced.
        /// </summary>
        /// <param name="elNode"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public NodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context)
        {
            XmlAttribute typeAttrib = elNode.Attributes["type"];
            string typeName = Nodes.Utilities.PreprocessTypeName(typeAttrib.Value);

            Type type;
            NodeModel node;
            if (ResolveType(typeName, out type)
                && LoadNodeModelInstanceByType(type, elNode, context, out node))
            {
                return node;
            }

            node = new DummyNode(1, 1, typeName, elNode, "", DummyNode.Nature.Deprecated);
            return node;
        }

        /// <summary>
        ///     Creates a new NodeModel from its typeName, using the node factories registered
        ///     in this factory.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CreateNodeFromTypeName(string typeName, out NodeModel node)
        {
            Type type;
            if (ResolveType(typeName, out type) && CreateNodeModelInstanceByType(type, out node))
                return true;

            node = null;
            return false;
        }
    }

    /// <summary>
    ///     Xml Loader for CodeBlock nodes.
    /// </summary>
    public class CodeBlockNodeLoader : INodeLoader<CodeBlockNodeModel>, INodeFactory<CodeBlockNodeModel>
    {
        private readonly LibraryServices libraryServices;

        public CodeBlockNodeLoader(LibraryServices manager)
        {
            libraryServices = manager;
        }

        public CodeBlockNodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context)
        {
            var node = CreateNode();
            node.Deserialize(elNode, context);
            return node;
        }

        public CodeBlockNodeModel CreateNode()
        {
            return new CodeBlockNodeModel(libraryServices);
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

        public NodeModel CreateNodeFromXml(XmlElement nodeElement, SaveContext context)
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

            if (context == SaveContext.File && !string.IsNullOrEmpty(assembly))
            {
                var document = nodeElement.OwnerDocument;
                var docPath = Nodes.Utilities.GetDocumentXmlPath(document);
                assembly = Nodes.Utilities.MakeAbsolutePath(docPath, assembly);

                descriptor = libraryServices.IsLibraryLoaded(assembly) || libraryServices.ImportLibrary(assembly)
                    ? libraryServices.GetFunctionDescriptor(assembly, function)
                    : libraryServices.GetFunctionDescriptor(function);
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

            DSFunctionBase result;
            if (descriptor.IsVarArg)
            {
                result = new DSVarArgFunction(descriptor);
                if (nodeElement.Name != typeof(DSVarArgFunction).FullName)
                {
                    VariableInputNodeController.SerializeInputCount(
                        nodeElement,
                        descriptor.Parameters.Count());
                }
            }
            else
                result = new DSFunction(descriptor);

            result.Deserialize(nodeElement, context);
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

            try
            {
                return Path.GetFileName(assemblyName); 
            }
            catch (Exception) 
            { 
                return string.Empty; 
            }
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
        
        public Function CreateNodeFromXml(XmlElement nodeElement, SaveContext context)
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

            var node = customNodeManager.CreateCustomNodeInstance(funcId, nickname, isTestMode);
            node.Deserialize(nodeElement, context);
            return node;
        }
    }
}
