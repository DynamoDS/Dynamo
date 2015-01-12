using System;
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

        public NodeModelSearchElement(TypeLoadData typeLoadData) : base(typeLoadData)
        {
            constructor = typeLoadData.Type.GetDefaultConstructor<NodeModel>();
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return constructor();
        }
    }
}