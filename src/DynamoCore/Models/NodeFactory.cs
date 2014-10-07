using System;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public class NodeFactory
    {
        //TODO(Steve): Kill these. Nodes should be loaded based off of TypeLoadData.
        //TODO(Steve): TypeLoadData should be abstract, takes appropriate action based off of node type (0-touch, Custom, ModelExtension)
        private readonly Dictionary<string, TypeLoadData> builtInTypesByName =
            new Dictionary<string, TypeLoadData>();

        public IDictionary<string, TypeLoadData> BuiltInTypesByName
        {
            get { return builtInTypesByName; }
        }

        private readonly SortedDictionary<string, TypeLoadData> builtInTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        public IDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtInTypesByNickname; }
        }

        /// <summary>
        ///     Create a NodeModel from a function descriptor name, a NodeModel name, a NodeModel nickname, or a custom node name
        /// </summary>
        /// <param name="name">A name</param>
        /// <param name="engineController"></param>
        /// <param name="manager"></param>
        /// <param name="logger"></param>
        /// <returns>If the name is valid, a new NodeModel.  Otherwise, null.</returns>
        public NodeModel CreateNodeInstance(string name, EngineController engineController, CustomNodeManager manager, ILogger logger)
        {
            NodeModel node;

            // Depending on node type, get a node instance
            FunctionDescriptor functionItem = engineController.GetFunctionDescriptor(name);
            if (functionItem != null)
            {
                node = GetDSFunctionFromFunctionItem(functionItem);
            }
            else if (builtInTypesByName.ContainsKey(name))
            {
                node = GetNodeModelInstanceByName(name, logger);
            }
            else if (builtInTypesByNickname.ContainsKey(name))
            {
                node = GetNodeModelInstanceByNickName(name, logger);
            }
            else
            {
                node = GetCustomNodeByName(manager, name, logger);
            }

            return node;
        }

        /// <summary>
        ///     Create a NodeModel with a given type as the method generic parameter
        /// </summary>
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
        public NodeModel CreateNodeInstance(Type elementType, string nickName, string signature, Guid guid, ILogger logger)
        {
            object createdNode = GetNodeModelInstanceByType(elementType, logger);

            // The attempt to create node instance may fail due to "elementType"
            // being something else other than "NodeModel" derived object type. 
            // This is possible since some legacy nodes have been made to derive
            // from "MigrationNode" object that is not derived from "NodeModel".
            // 
            var node = createdNode as NodeModel;
            if (node == null)
                return null;

            if (!string.IsNullOrEmpty(nickName))
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

        #region Helper methods

        private NodeModel GetDSFunctionFromFunctionItem(FunctionDescriptor functionItem)
        {
            if (functionItem.IsVarArg)
                return new DSVarArgFunction(functionItem);
            return new DSFunction(functionItem);
        }

        private NodeModel GetCustomNodeByName(CustomNodeManager manager, string name, ILogger logger)
        {
            CustomNodeDefinition def;

            if (manager.GetDefinition(Guid.Parse(name), out def))
            {
                return new Function(def)
                {
                    NickName = def.WorkspaceModel.Name
                };
            }

            logger.Log("Failed to find CustomNodeDefinition!");
            return null;
        }

        private NodeModel GetNodeModelInstanceByName(string name, ILogger logger)
        {
            TypeLoadData tld = BuiltInTypesByName[name];
            return GetNodeModelInstanceByType(tld.Type, logger);
        }

        private NodeModel GetNodeModelInstanceByNickName(string name, ILogger logger)
        {
            TypeLoadData tld = BuiltInTypesByNickname[name];
            return GetNodeModelInstanceByType(tld.Type, logger);
        }

        private T GetNodeModelInstanceByType<T>(ILogger logger) where T : NodeModel
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            catch (Exception ex)
            {
                logger.Log("Failed to load built-in type");
                logger.Log(ex);
                return null;
            }
        }

        private NodeModel GetNodeModelInstanceByType(Type type, ILogger logger)
        {
            try
            {
                return (NodeModel)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                logger.Log("Failed to load built-in type");
                logger.Log(ex);
                return null;
            }
        }

        #endregion

    }
}
