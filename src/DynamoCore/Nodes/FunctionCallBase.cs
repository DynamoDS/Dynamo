using Dynamo.Models;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Node base class for all nodes that produce a DS function call.
    /// </summary>
    public abstract class FunctionCallBase : NodeModel
    {
        /// <summary>
        ///     Controller used to sync node with a function definition.
        /// </summary>
        public FunctionCallNodeController Controller { get; private set; }

        protected FunctionCallBase(WorkspaceModel workspace, FunctionCallNodeController controller) 
            : base(workspace)
        {
            Controller = controller;
            Controller.SyncNodeWithDefinition(this);
        }

        /// <summary>
        /// The unique name that was created the node by
        /// </summary>
        public override string CreatingName
        {
            get
            {
                return this.Controller != null ? this.Controller.Definition.MangledName : this.Name;
            }
        }
    }
}