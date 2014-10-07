using Dynamo.Models;
using Dynamo.Nodes;

using Autodesk.DesignScript.Runtime;

namespace DSCore.File
{
    [NodeName("Directory Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a directory on the system to get its path.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    //MAGN -3382 [IsVisibleInDynamoLibrary(false)]
    public class Directory : FileSystemBrowser
    {
        public Directory(WorkspaceModel workspace) : base(workspace, "Directory") { }
    }
}
