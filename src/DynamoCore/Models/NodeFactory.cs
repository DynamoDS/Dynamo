using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public interface INodeSource<out T> where T : NodeModel
    {
        T CreateNode(XmlNode elNode);
    }

    public class NodeFactory : LogSourceBase
    {
        private readonly Dictionary<Type, INodeSource<NodeModel>> nodeSources =
            new Dictionary<Type, INodeSource<NodeModel>>();

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        public void AddSource<T>(INodeSource<T> source) where T : NodeModel
        {
            nodeSources[typeof(T)] = source;
        }

        /// <summary>
        ///     Create a NodeModel from a function descriptor name, a NodeModel name, a NodeModel nickname, or a custom node name
        /// </summary>
        /// <param name="name">A name</param>
        /// <param name="engineController"></param>
        /// <param name="manager"></param>
        /// <param name="logger"></param>
        /// <returns>If the name is valid, a new NodeModel.  Otherwise, null.</returns>
        [Obsolete("Lookup by type instead", true)]
        public NodeModel CreateNodeInstance(string name, EngineController engineController, CustomNodeManager manager)
        {
            NodeModel node;

            // Depending on node type, get a node instance
            FunctionDescriptor functionItem = engineController.GetFunctionDescriptor(name);
            if (functionItem != null)
                node = GetDSFunctionFromFunctionItem(functionItem);
            else if (!GetNodeModelInstanceByName(name, null, out node))
                node = GetCustomNodeByName(manager, name);

            return node;
        }

        /// <summary>
        ///     Create a NodeModel from a type object
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="signature"> The signature of the function along with parameter information </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <param name="logger"></param>
        /// <returns> The newly instantiated NodeModel</returns>
        [Obsolete("Lookup by type instead", true)]
        public NodeModel CreateNodeInstance(Type elementType, string nickName, string signature, Guid guid)
        {
            object createdNode = null; //GetNodeModelInstanceByType(elementType);

            // The attempt to create node instance may fail due to "elementType"
            // being something else other than "NodeModel" derived object type. 
            // This is possible since some legacy nodes have been made to derive
            // from "MigrationNode" object that is not derived from "NodeModel".
            // 
            var node = createdNode as NodeModel;
            if (node == null)
                return null;

            if (!string.IsNullOrEmpty(signature))
            {
                node.NickName = LibraryServices.GetInstance().NicknameFromFunctionSignatureHint(signature);
            }
            else if (!string.IsNullOrEmpty(nickName)) 
            {
                node.NickName = nickName;
            }
            else
            {
                var elNameAttrib =
                    node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    node.NickName = elNameAttrib.Name;
                }
            }

            node.GUID = guid;

            return node;
        }
        
        [Obsolete("This is handled in the TypeLoadData for DSFunction (and sub-classes)", true)]
        private static NodeModel GetDSFunctionFromFunctionItem(FunctionDescriptor functionItem)
        {
            if (functionItem.IsVarArg)
                return new DSVarArgFunction(functionItem);
            return new DSFunction(functionItem);
        }

        //TODO(Steve): Move to CustomNodeManager
        [Obsolete("Lookup by type instead", true)]
        private NodeModel GetCustomNodeByName(CustomNodeManager manager, string name)
        {
            CustomNodeDefinition def;

            if (manager.GetDefinition(Guid.Parse(name), out def))
            {
                return new Function(def)
                {
                    NickName = def.WorkspaceModel.Name
                };
            }

            Log("Failed to find CustomNodeDefinition!");
            return null;
        }

        private bool GetNodeSourceFromType(Type type, out INodeSource<NodeModel> data)
        {
            if (nodeSources.TryGetValue(type, out data))
                return true; // Found among built-in types, return it.

            //TODO(Steve): Handle during load, store separate entries for AlsoKnownAs
            #region AlsoKnownAs
            var query = from builtInType in nodeSources
                        let t =
                            new
                            {
                                builtInType,
                                attribs =
                                    builtInType.Value.Type.GetCustomAttributes(
                                        typeof(AlsoKnownAsAttribute),
                                        false)
                            }
                        where
                            t.attribs.Any()
                                & (t.attribs[0] as AlsoKnownAsAttribute).Values.Contains(fullyQualifiedName)
                        select t.builtInType;

            if (query.Any()) // Found a matching type.
            {
                var builtInType = query.First();
                Log(
                    string.Format(
                        "Found matching node for {0} also known as {1}",
                        builtInType.Key,
                        fullyQualifiedName));

                data = builtInType.Value;
                return true;
            }
            #endregion

            Log(string.Format("Could not load node of type: {0}", type.FullName));
            Log("Loading will continue but nodes might be missing from your workflow.");

            return false;
        }

        private bool GetNodeModelInstanceByName(string name, XmlNode elNode, out NodeModel node)
        {
            Type type;
            if (!ResolveType(name, out type))
            {
                node = null;
                return false;
            }
            
            INodeSource<NodeModel> data;
            if (!GetNodeSourceFromType(type, out data))
            {
                node = null;
                return false;
            }

            node = data.CreateNode(elNode);
            node.Load(elNode);
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
        /// <returns></returns>
        public NodeModel CreateNodeFromXml(XmlNode elNode)
        {
            XmlAttribute typeAttrib = elNode.Attributes["type"];
            string typeName = Nodes.Utilities.PreprocessTypeName(typeAttrib.Value);

            NodeModel node;
            if (!GetNodeModelInstanceByName(typeName, elNode, out node))
            {
                // If a given function is not found during file load, then convert the 
                // function node into a dummy node (instead of crashing the workflow).
                var dummyElement = MigrationManager.CreateMissingNode(elNode as XmlElement, 1, 1);

                // The new type representing the dummy node.
                typeName = dummyElement.GetAttribute("type");
                GetNodeModelInstanceByName(typeName, dummyElement, out node);
                node.Load(dummyElement);
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
            var function = node as Function;
            if ((function != null) && (function.Definition == null))
            {
                var e = elNode as XmlElement;
                dummyElement = MigrationManager.CreateMissingNode(
                    e, node.InPortData.Count, node.OutPortData.Count);
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
}
