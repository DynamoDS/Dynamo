using Autodesk.DesignScript.Runtime;

using Dynamo.Models;
using Dynamo.Nodes;

namespace DSCore.File
{
    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a file on the system to get its filename.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class Filename : FileSystemBrowser
    {
        public Filename(WorkspaceModel workspace) : base(workspace, "Filename") { }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }
}