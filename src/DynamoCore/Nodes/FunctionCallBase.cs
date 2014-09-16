using Dynamo.DSEngine;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Node base class for all nodes that produce a DS function call.
    /// </summary>
    public abstract class FunctionCallBase<T> : NodeModel where T : IFunctionDescriptor
    {
        /// <summary>
        ///     Controller used to sync node with a function definition.
        /// </summary>
        public FunctionCallNodeController<T> Controller { get; private set; }

        protected FunctionCallBase(FunctionCallNodeController<T> controller)
        {
            Controller = controller;
            Controller.SyncNodeWithDefinition(this);
        }
    }
}