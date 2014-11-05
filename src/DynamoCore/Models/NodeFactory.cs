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
    public interface INodeSource
    {
        NodeModel CreateNode();
    }

    public class NodeFactory : LogSourceBase
    {
        //TODO(Steve): TypeLoadData should be abstract, takes appropriate action based off of node type (0-touch, Custom, ModelExtension)
        private readonly Dictionary<string, INodeSource> nodeSourcesByName =
            new Dictionary<string, INodeSource>();

        /// <summary>
        ///     Create a NodeModel from a function descriptor name, a NodeModel name, a NodeModel nickname, or a custom node name
        /// </summary>
        /// <param name="name">A name</param>
        /// <param name="engineController"></param>
        /// <param name="manager"></param>
        /// <param name="logger"></param>
        /// <returns>If the name is valid, a new NodeModel.  Otherwise, null.</returns>
        [Obsolete("What the fuck is going on here")]
        public NodeModel CreateNodeInstance(string name, EngineController engineController, CustomNodeManager manager)
        {
            NodeModel node;

            // Depending on node type, get a node instance
            FunctionDescriptor functionItem = engineController.GetFunctionDescriptor(name);
            if (functionItem != null)
                node = GetDSFunctionFromFunctionItem(functionItem);
            else if (nodeSourcesByName.ContainsKey(name))
                node = GetNodeModelInstanceByName(name);
            else
                node = GetCustomNodeByName(manager, name);

            return node;
        }

        /// <summary>
        ///     Create a NodeModel with a given type as the method generic parameter
        /// </summary>
        /// <param name="logger"></param>
        /// <returns> The newly instantiated NodeModel with a new guid</returns>
        public T CreateNodeInstance<T>() where T : NodeModel
        {
            var node = GetNodeModelInstanceByType<T>();
            if (node == null) 
                return null;

            node.GUID = Guid.NewGuid();

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
        public NodeModel CreateNodeInstance(Type elementType, string nickName, string signature, Guid guid)
        {
            object createdNode = GetNodeModelInstanceByType(elementType);

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

        private bool GetNodeModelInstanceByName(string name, out NodeModel node)
        {
            INodeSource data;
            if (!ResolveTypeData(name, out data))
            {
                node = null;
                return false;
            }
            node = data.CreateNode();
            return true;
        }

        private T GetNodeModelInstanceByType<T>() where T : NodeModel
        {
            try
            {
                return (T) typeof(T).GetInstance();
            }
            catch (Exception ex)
            {
                Log("Failed to load built-in type", WarningLevel.Error);
                Log(ex);
                return null;
            }
        }

        private NodeModel GetNodeModelInstanceByType(Type type)
        {
            try
            {
                return (NodeModel)type.GetInstance();
            }
            catch (Exception ex)
            {
                Log("Failed to load built-in type");
                Log(ex);
                return null;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="fullyQualifiedName"></param>
        /// <param name="builtInTypes"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ResolveTypeData(string fullyQualifiedName, out INodeSource data)
        {
            if (fullyQualifiedName == null)
                throw new ArgumentNullException(@"fullyQualifiedName");

            if (nodeSourcesByName.TryGetValue(fullyQualifiedName, out data))
                return true; // Found among built-in types, return it.

            
            //TODO(Steve): Handle during load, store separate entries for AlsoKnownAs
            var query = from builtInType in nodeSourcesByName
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

            Log(string.Format("Could not load node of type: {0}", fullyQualifiedName));
            Log("Loading will continue but nodes " + "might be missing from your workflow.");

            return false;
        }

        public NodeModel CreateNodeFromXml(XmlNode elNode)
        {
            XmlAttribute typeAttrib = elNode.Attributes["type"];
            XmlAttribute guidAttrib = elNode.Attributes["guid"];
            XmlAttribute nicknameAttrib = elNode.Attributes["nickname"];
            XmlAttribute xAttrib = elNode.Attributes["x"];
            XmlAttribute yAttrib = elNode.Attributes["y"];
            XmlAttribute isVisAttrib = elNode.Attributes["isVisible"];
            XmlAttribute isUpstreamVisAttrib = elNode.Attributes["isUpstreamVisible"];
            XmlAttribute lacingAttrib = elNode.Attributes["lacing"];

            // Retrieve optional 'function' attribute (only for DSFunction).
            XmlAttribute signatureAttrib = elNode.Attributes["function"];
            var signature = signatureAttrib == null ? null : signatureAttrib.Value;

            string typeName = Nodes.Utilities.PreprocessTypeName(typeAttrib.Value);

            //test the GUID to confirm that it is non-zero
            //if it is zero, then we have to fix it
            //this will break the connectors, but it won't keep
            //propagating bad GUIDs
            Guid guid;
            if (!Guid.TryParse(guidAttrib.Value, out guid))
                guid = Guid.NewGuid();

            string nickname = nicknameAttrib.Value;

            double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
            double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

            bool isVisible = true;
            if (isVisAttrib != null)
                isVisible = isVisAttrib.Value == "true";

            bool isUpstreamVisible = true;
            if (isUpstreamVisAttrib != null)
                isUpstreamVisible = isUpstreamVisAttrib.Value == "true";

            NodeModel node;
            if (!GetNodeModelInstanceByName(typeName, out node))
            {
                // If a given function is not found during file load, then convert the 
                // function node into a dummy node (instead of crashing the workflow).
                var dummyElement = MigrationManager.CreateMissingNode(elNode as XmlElement, 1, 1);

                // The new type representing the dummy node.
                typeName = dummyElement.GetAttribute("type");
                GetNodeModelInstanceByName(typeName, out node);
                node.Load(dummyElement);
            }
            else
            {
                node.Load(elNode);
            }

            try
            {
                // The attempt to create node instance may fail due to "type" being
                // something else other than "NodeModel" derived object type. This 
                // is possible since some legacy nodes have been made to derive from
                // "MigrationNode" object type that is not derived from "NodeModel".
                // 
                Type type = ResolveType(typeName, nodeSourcesByName, AsLogger());
                if (type != null)
                    node = CreateNodeInstance(type, nickname, signature, guid);

                if (node != null)
                {
                    node.Load(elNode);
                }
                else
                {
                    var e = elNode as XmlElement;
                    dummyElement = MigrationManager.CreateMissingNode(e, 1, 1);
                }
            }
            catch (UnresolvedFunctionException)
            {
                // If a given function is not found during file load, then convert the 
                // function node into a dummy node (instead of crashing the workflow).
                // 
                var e = elNode as XmlElement;
                dummyElement = MigrationManager.CreateUnresolvedFunctionNode(e);
            }

            //=====HOME=====

            // If a custom node fails to load its definition, convert it into a dummy node.
            var function = node as Function;
            if ((function != null) && (function.Definition == null))
            {
                var e = elNode as XmlElement;
                dummyElement = MigrationManager.CreateMissingNode(
                    e, node.InPortData.Count, node.OutPortData.Count);
            }

            //==============

            if (dummyElement != null) // If a dummy node placement is desired.
            {
                // The new type representing the dummy node.
                typeName = dummyElement.GetAttribute("type");
                var type = Dynamo.Nodes.Utilities.ResolveType(dynamoModel, typeName);

                node = NodeFactory.CreateNodeInstance(type, nickname, string.Empty, guid);
                node.Load(dummyElement);
            }

            node.X = x;
            node.Y = y;

            if (lacingAttrib != null)
            {
                if (node.ArgumentLacing != LacingStrategy.Disabled)
                {
                    LacingStrategy lacing;
                    Enum.TryParse(lacingAttrib.Value, out lacing);
                    node.ArgumentLacing = lacing;
                }
            }

            // This is to fix MAGN-3648. Method reference in CBN that gets 
            // loaded before method definition causes a CBN to be left in 
            // a warning state. This is to clear such warnings and set the 
            // node to "Dead" state (correct value of which will be set 
            // later on with a call to "EnableReporting" below). Please 
            // refer to the defect for details and other possible fixes.
            // 
            if (node.State == ElementState.Warning && (node is CodeBlockNodeModel))
                node.State = ElementState.Dead; // Condition to fix MAGN-3648


            node.IsVisible = isVisible;
            node.IsUpstreamVisible = isUpstreamVisible;
        }
    }
}
