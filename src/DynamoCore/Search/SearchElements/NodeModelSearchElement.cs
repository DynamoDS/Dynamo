using System;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Search element for basic NodeModels.
    /// </summary>
    public class NodeModelSearchElement : NodeModelSearchElementBase
    {
        private readonly Func<NodeModel> constructor;
        private readonly string creationName;

        public override string CreationName { get {return creationName;} }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeModelSearchElement"/> class.
        /// </summary>
        /// <param name="typeLoadData">Data to load.</param>
        internal NodeModelSearchElement(TypeLoadData typeLoadData) : base(typeLoadData)
        {
            constructor = typeLoadData.Type.GetDefaultConstructor<NodeModel>();
            creationName = typeLoadData.Type.ToString();
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return constructor();
        }


    }
}