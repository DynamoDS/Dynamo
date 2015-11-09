using Dynamo.Engine;

namespace Dynamo.Graph.Nodes
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

        /// <summary>
        /// The unique name that the node was created by
        /// </summary>
        public override string CreationName
        {
            get
            {
                return this.Controller != null ? this.Controller.Definition.MangledName : this.Name;
            }
        }
    }
}