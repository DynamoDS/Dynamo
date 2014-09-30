using Autodesk.DesignScript.Runtime;

using Dynamo.Models;

namespace DSCore.File
{
    [SupressImportIntoVM]
    public abstract class FileSystemBrowser : DSCoreNodesUI.String
    {
        protected FileSystemBrowser(WorkspaceModel workspace, string tip)
            : base(workspace)
        {
            OutPortData[0].ToolTipString = tip;
            RegisterAllPorts();

            Value = "";
        }
    }
}