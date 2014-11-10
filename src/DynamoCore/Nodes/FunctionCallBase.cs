using Dynamo.DSEngine;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Node base class for all nodes that produce a DS function call.
    /// </summary>
    public abstract class FunctionCallBase<T, TDesc> : NodeModel
        where T : FunctionCallNodeController<TDesc>
        where TDesc : IFunctionDescriptor
    {
        /// <summary>
        ///     Controller used to sync node with a function definition.
        /// </summary>
        public T Controller { get; private set; }

        protected FunctionCallBase(T controller)
        {
            Controller = controller;
            Controller.SyncNodeWithDefinition(this);
        }
    }
}