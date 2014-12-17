using System;

using Dynamo.DSEngine;
using Dynamo.Nodes;
using Dynamo.Utilities;

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

        /// A proxy custom node is a custom node without its definition loaded 
        /// in Dynamo. The creation of a proxy custom node relies on information 
        /// provided by the caller since the definition is not readily available 
        /// for reading. The actual definition may become available at a later 
        /// time by means of user uploading the definition.
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
            if (!Guid.TryParse(name, out guid))
            {
                return null;
            }

            var tld = Nodes.Utilities.GetDataForType(dynamoModel, typeof(Function));
            // create an instance of Function node 
            Function result = CreateNodeInstance(tld, nickName, null, id) as Function;
            // create its definition and add inputs and outputs
            result.LoadNode(guid, inputs, outputs);
            return result;
        }

        /// <summary>
        ///     Create a NodeModel from a type object
        /// </summary>
        /// <param name="data"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="signature"> The signature of the function along with parameter information </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <returns> The newly instantiated NodeModel</returns>
        internal NodeModel CreateNodeInstance(TypeLoadData data, string nickName, string signature, Guid guid)
        {
            object createdNode = GetNodeModelInstance(data);

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
                var libraryServices = dynamoModel.EngineController.LibraryServices;
                node.NickName = libraryServices.NicknameFromFunctionSignatureHint(signature);
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
            Guid guid;
            // Check name is correct guid
            if (Guid.TryParse(name, out guid) && dynamoModel.CustomNodeManager.GetDefinition(guid, out def))
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
            return this.GetNodeModelInstance(tld);
        }

        private NodeModel GetNodeModelInstanceByNickName(string name)
        {
            TypeLoadData tld = dynamoModel.BuiltInTypesByNickname[name];
            return this.GetNodeModelInstance(tld);
        }

        private T GetNodeModelInstanceByType<T>() where T : NodeModel
        {
            try
            {
                return (T) typeof(T).GetInstance(this.workspaceModel);
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
                return (NodeModel) type.GetInstance(this.workspaceModel);
            }
            catch (Exception ex)
            {
                dynamoModel.Logger.Log("Failed to load built-in type");
                dynamoModel.Logger.Log(ex);
                return null;
            }
            
        }

        private NodeModel GetNodeModelInstance(TypeLoadData type)
        {
            var node = GetNodeModelInstanceByType(type.Type);
            if (type.IsObsolete)
                node.Warning(type.ObsoleteMessage);
            return node;
        }

        #endregion

    }
}
