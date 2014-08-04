using System;
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
        ///     Create a build-in node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="signature"> The signature of the function along with parameter information </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <returns> The newly instantiated dynNode</returns>
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
                node = GetBuiltinTypeByName(name);
            }
            else if (dynamoModel.BuiltInTypesByNickname.ContainsKey(name))
            {
                node = GetBuiltinTypByNickName(name);
            }
            else
            {
                node = GetCustomNodeByName(name);
            }

            if (node == null) return node;

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
            Function func;

            if (dynamoModel.CustomNodeManager.GetNodeInstance(Guid.Parse(name), out func))
            {
                return func;
            }

            dynamoModel.Logger.Log("Failed to find CustomNodeDefinition.");
            return null;
        }

        private NodeModel GetBuiltinTypeByName(string name)
        {
            TypeLoadData tld = dynamoModel.BuiltInTypesByName[name];

            ObjectHandle obj = Activator.CreateInstance(tld.Assembly.Location, tld.Type.FullName, false, 0, 
                null, new object[]{this.workspaceModel}, null, null);
            var newEl = (NodeModel)obj.Unwrap();
            newEl.DisableInteraction();
            return newEl;
        }

        private NodeModel GetBuiltinTypByNickName(string name)
        {
            TypeLoadData tld = dynamoModel.BuiltInTypesByNickname[name];
            try
            {
                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName, false, 0,
                    null, new object[] { this.workspaceModel }, null, null);
                var newEl = (NodeModel)obj.Unwrap();
                newEl.DisableInteraction();
                return newEl;
            }
            catch (Exception ex)
            {
                dynamoModel.Logger.Log("Failed to load built-in type");
                dynamoModel.Logger.Log(ex);
                return null;
            }
        }

        #endregion

        public NodeModel CreateNodeInstance(Type elementType, string nickName, string signature, Guid guid)
        {
            object createdNode = null;

            if (elementType.IsAssignableFrom(typeof(DSVarArgFunction)))
            {
                // If we are looking at a 'DSVarArgFunction', we'd better had 
                // 'signature' readily available, otherwise we have a problem.
                if (string.IsNullOrEmpty(signature))
                {
                    var message = "Unknown function signature";
                    throw new ArgumentException(message, "signature");
                }

                // Invoke the constructor that takes in a 'FunctionDescriptor'.
                var functionDescriptor = dynamoModel.EngineController.GetFunctionDescriptor(signature);

                if (functionDescriptor == null)
                    throw new UnresolvedFunctionException(signature);

                createdNode = Activator.CreateInstance(elementType,
                    new object[] { this.workspaceModel, functionDescriptor });
            }
            else
            {
                createdNode = Activator.CreateInstance(
                    elementType,
                    new object[] { this.workspaceModel });
            }

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

    }
}
