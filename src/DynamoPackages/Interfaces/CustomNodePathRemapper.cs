using System.IO;
using Dynamo.Graph;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     A simplified interface for remapping the file path of a CustomNodeWorkspace.
    ///     Useful for when a custom node will be moved by a package creation operation.
    /// </summary>
    public interface IPathRemapper
    {
        /// <summary>
        ///     Remap the custom node path
        /// </summary>
        /// <param name="originalPath">The original path</param>
        /// <param name="newDirectoryPath">The final directory path</param>
        /// <returns>If successful, returns true</returns>
        bool SetPath(string originalPath, string newDirectoryPath);
    }

    /// <summary>
    ///     An IPathRemapper that requires a CustomNodeManager to mutate a custom node workspace path
    /// </summary>
    internal class CustomNodePathRemapper : IPathRemapper
    {
        private readonly ICustomNodeManager customNodeManager;
        private readonly bool isTestMode;

        internal CustomNodePathRemapper(ICustomNodeManager customNodeManager, bool isTestMode)
        {
            this.customNodeManager = customNodeManager;
            this.isTestMode = isTestMode;
        }

        public bool SetPath(string originalPath, string newDirectoryPath)
        {
            var id = customNodeManager.GuidFromPath(originalPath);

            ICustomNodeWorkspaceModel def;
            var res = customNodeManager.TryGetFunctionWorkspace(id, this.isTestMode, out def);

            if (!res) return false;

            var newPath = Path.Combine(newDirectoryPath, Path.GetFileName(def.FileName));

            // TODO: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7989
            var cdef = def as CustomNodeWorkspaceModel;
            cdef.FileName = newPath;

            return true;
        }
    }

}
