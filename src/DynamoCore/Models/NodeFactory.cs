﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;

using Dynamo.DSEngine;
using Dynamo.Nodes;

namespace Dynamo.Models
{
    internal class NodeFactory
    {
        private readonly DynamoModel dynamoModel;
        private readonly WorkspaceModel workspaceModel;

        internal NodeFactory(WorkspaceModel workspaceModel, DynamoModel dynamoModel)
        {
            this.dynamoModel = dynamoModel;
            this.workspaceModel = workspaceModel;
        }

        /// <summary>
        ///     Create a NodeModel from a function descriptor name, a NodeModel name, a NodeModel nickname, or a custom node name
        /// </summary>
        /// <param name="name">A name</param>
        /// <returns>If the name is valid, a new NodeModel.  Otherwise, null.</returns>
        internal NodeModel CreateNodeInstance(string name)
        {
            NodeModel node;

            // Depending on node type, get a node instance
            FunctionDescriptor functionItem = (dynamoModel.EngineController.GetFunctionDescriptor(name));
            if (functionItem != null)
            {
                node = GetDSFunctionFromFunctionItem(functionItem);
            }
            else if (dynamoModel.BuiltInTypesByName.ContainsKey(name))
            {
                node = GetNodeModelInstanceByName(name);
            }
            else if (dynamoModel.BuiltInTypesByNickname.ContainsKey(name))
            {
                node = GetNodeModelInstanceByNickName(name);
            }
            else
            {
                node = GetCustomNodeByName(name);
            }

            return node;
        }

        /// <summary>
        ///     Create a NodeModel with a given type as the method generic parameter
        /// </summary>
        /// <returns> The newly instantiated NodeModel with a new guid</returns>
        internal T CreateNodeInstance<T>() where T : NodeModel
        {
            var node = this.GetNodeModelInstanceByType<T>();
            if (node == null) return node;

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
        /// <returns> The newly instantiated NodeModel</returns>
        internal NodeModel CreateNodeInstance(Type elementType, string nickName, string signature, Guid guid)
        {
            object createdNode = this.GetNodeModelInstanceByType(elementType);

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
                return new DSVarArgFunction(this.workspaceModel, functionItem);
            return new DSFunction(this.workspaceModel, functionItem);
        }

        private NodeModel GetCustomNodeByName(string name)
        {
            CustomNodeDefinition def;

            if (dynamoModel.CustomNodeManager.GetDefinition(Guid.Parse(name), out def))
            {
                return new Function(this.workspaceModel, def)
                {
                    NickName = def.WorkspaceModel.Name
                };
            }

            dynamoModel.Logger.Log("Failed to find CustomNodeDefinition!");
            return null;
        }

        private NodeModel GetNodeModelInstanceByName(string name)
        {
            TypeLoadData tld = dynamoModel.BuiltInTypesByName[name];
            return this.GetNodeModelInstanceByType(tld.Type);
        }

        private NodeModel GetNodeModelInstanceByNickName(string name)
        {
            TypeLoadData tld = dynamoModel.BuiltInTypesByNickname[name];
            return this.GetNodeModelInstanceByType(tld.Type);
        }

        private T GetNodeModelInstanceByType<T>() where T : NodeModel
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T), this.workspaceModel);
            }
            catch (Exception ex)
            {
                dynamoModel.Logger.Log("Failed to load built-in type");
                dynamoModel.Logger.Log(ex);
                return null;
            }
        }

        private NodeModel GetNodeModelInstanceByType(Type type)
        {
            try
            {
                return (NodeModel)Activator.CreateInstance(type, this.workspaceModel);
            }
            catch (Exception ex)
            {
                dynamoModel.Logger.Log("Failed to load built-in type");
                dynamoModel.Logger.Log(ex);
                return null;
            }
        }

        #endregion

    }
}
