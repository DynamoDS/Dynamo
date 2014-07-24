using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;

using Dynamo.DSEngine;
using Dynamo.Nodes;

namespace Dynamo.Models
{
    internal class NodeFactory
    {
        private DynamoModel dynamoModel;

        internal NodeFactory(DynamoModel model)
        {
            this.dynamoModel = model;
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
            NodeModel result;

#if USE_DSENGINE
            FunctionDescriptor functionItem = (dynamoModel.EngineController.GetFunctionDescriptor(name));
            if (functionItem != null)
            {
                if (functionItem.IsVarArg)
                    return new DSVarArgFunction(functionItem);
                return new DSFunction(functionItem);
            }
#endif
            if (dynamoModel.BuiltInTypesByName.ContainsKey(name))
            {
                TypeLoadData tld = dynamoModel.BuiltInTypesByName[name];

                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (NodeModel)obj.Unwrap();
                newEl.DisableInteraction();
                result = newEl;
            }
            else if (dynamoModel.BuiltInTypesByNickname.ContainsKey(name))
            {
                TypeLoadData tld = dynamoModel.BuiltInTypesByNickname[name];
                try
                {
                    ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                    var newEl = (NodeModel)obj.Unwrap();
                    newEl.DisableInteraction();
                    result = newEl;
                }
                catch (Exception ex)
                {
                    dynamoModel.Logger.Log("Failed to load built-in type");
                    dynamoModel.Logger.Log(ex);
                    result = null;
                }
            }
            else
            {
                Function func;

                if (dynamoModel.CustomNodeManager.GetNodeInstance(Guid.Parse(name), out func))
                {
                    result = func;
                }
                else
                {
                    dynamoModel.Logger.Log("Failed to find CustomNodeDefinition.");
                    return null;
                }
            }

            return result;
        }

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
                    new object[] { functionDescriptor });
            }
            else
            {
                createdNode = Activator.CreateInstance(elementType);
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

            //string name = nodeUI.NickName;
            return node;
        }

    }
}
