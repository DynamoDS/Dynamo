using Dynamo.DSEngine;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Node base class for all nodes that produce a DS function call.
    /// </summary>
    public abstract class FunctionCallBase<TController, TDescriptor> : NodeModel
        where TController : FunctionCallNodeController<TDescriptor>
        where TDescriptor : IFunctionDescriptor
    {
        /// <summary>
        ///     Controller used to sync node with a function definition.
        /// </summary>
        public TController Controller { get; private set; }

        protected FunctionCallBase(TController controller)
        {
            Controller = controller;
            Controller.SyncNodeWithDefinition(this);
        }
    }
}