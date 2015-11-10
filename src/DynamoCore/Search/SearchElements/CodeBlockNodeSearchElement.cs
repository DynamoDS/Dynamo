using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Search element for Code Block nodes.
    /// </summary>
    public class CodeBlockNodeSearchElement : NodeModelSearchElementBase
    {
        private readonly LibraryServices libraryServices;

        public CodeBlockNodeSearchElement(TypeLoadData data, LibraryServices manager)
            : base(data)
        {
            libraryServices = manager;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return new CodeBlockNodeModel(libraryServices);
        }
    }
}