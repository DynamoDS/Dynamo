using System;
using System.Collections.Generic;
using System.Xml;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Logging;
using Dynamo.Utilities;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Nodes.NodeLoaders
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
        /// <param name="resolver">Element resolver for resolve namespace conflict</param>
        /// <returns></returns>
        T CreateNodeFromXml(XmlElement elNode, SaveContext context, ElementResolver resolver = null);
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
        internal void AddLoader<T>(INodeLoader<T> loader) where T : NodeModel
        {
            AddLoader(typeof(T), loader);
        }

        /// <summary>
        ///     Adds a node loader to this manager, for the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeType"></param>
        /// <param name="loader"></param>
        internal void AddLoader<T>(Type nodeType, INodeLoader<T> loader) where T : NodeModel
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
        internal void AddFactory<T>(INodeFactory<T> loader) where T : NodeModel
        {
            AddFactory(typeof(T), loader);
        }

        /// <summary>
        ///     Adds a node factory to this manager, for a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeType"></param>
        /// <param name="loader"></param>
        internal void AddFactory<T>(Type nodeType, INodeFactory<T> loader) where T : NodeModel
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
        internal bool AddTypeFactoryAndLoader<T>() where T : NodeModel
        {
            return AddTypeFactoryAndLoader(typeof(T));
        }

        /// <summary>
        ///     A proxy custom node is a custom node without its definition loaded 
        ///     in Dynamo. The creation of a proxy custom node relies on information 
        ///     provided by the caller since the definition is not readily available 
        ///     for reading. The actual definition may become available at a later 
        ///     time by means of user uploading the definition.
        /// </summary>
        /// <param name="id">Identifier of the custom node instance.</param>
        /// <param name="name">The name represents the GUID of the custom node 
        /// definition that is used for creating the custom node instance.</param>
        /// <param name="nickName">The display name of the custom node.</param>
        /// <param name="inputs">Number of input ports.</param>
        /// <param name="outputs">Number of output ports.</param>
        /// <returns>Returns the custom node instance if creation was successful, 
        /// or null otherwise.</returns>
        internal NodeModel CreateProxyNodeInstance(Guid id, string name, string nickName, int inputs, int outputs)
        {
            Guid guid;
            INodeLoader<NodeModel> data;
            if (!Guid.TryParse(name, out guid) || !GetNodeSourceFromType(typeof(Function), out data))
            {
                return null;
            }


            // create an instance of Function node 
            var result = (data as CustomNodeLoader).CreateProxyNode(guid, nickName, id, inputs, outputs);
            return result;
        }

        /// <summary>
        ///     Attempts to create a new factory and loader for a given type.
        /// </summary>
        /// <param name="nodeType"></param>
        internal bool AddTypeFactoryAndLoader(Type nodeType)
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
        internal void AddAlsoKnownAs(Type realType, string aka, bool overwrite = false)
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
        internal void AddAlsoKnownAs(Type realType, IEnumerable<string> names, bool overwrite = false)
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

            public NodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context, ElementResolver resolver)
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

        private bool LoadNodeModelInstanceByType(Type type, XmlElement elNode, SaveContext context, ElementResolver resolver, out NodeModel node)
        {
            INodeLoader<NodeModel> data;
            if (!GetNodeSourceFromType(type, out data))
            {
                node = null;
                return false;
            }

            node = data.CreateNodeFromXml(elNode, context, resolver);
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
        internal bool ResolveType(string fullyQualifiedName, out Type type)
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
        /// <param name="resolver"></param>
        /// <returns></returns>
        internal NodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context, ElementResolver resolver)
        {
            XmlAttribute typeAttrib = elNode.Attributes["type"];
            string typeName = Nodes.Utilities.PreprocessTypeName(typeAttrib.Value);

            Type type;
            NodeModel node;
            if (ResolveType(typeName, out type)
                && LoadNodeModelInstanceByType(type, elNode, context, resolver, out node))
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
        internal bool CreateNodeFromTypeName(string typeName, out NodeModel node)
        {
            Type type;
            if (ResolveType(typeName, out type) && CreateNodeModelInstanceByType(type, out node))
                return true;

            node = null;
            return false;
        }
    }
}
