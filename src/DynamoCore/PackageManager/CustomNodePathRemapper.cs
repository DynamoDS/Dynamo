using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Models;

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
        private readonly CustomNodeManager customNodeManager;
        private readonly bool isTestMode;

        internal CustomNodePathRemapper(CustomNodeManager customNodeManager, bool isTestMode)
        {
            this.customNodeManager = customNodeManager;
            this.isTestMode = isTestMode;
        }

        public bool SetPath(string originalPath, string newDirectoryPath)
        {
            var id = customNodeManager.GuidFromPath(originalPath);

            CustomNodeWorkspaceModel def;
            var res = customNodeManager.TryGetFunctionWorkspace(id, this.isTestMode, out def);

            if (!res) return false;

            var newPath = Path.Combine(newDirectoryPath, Path.GetFileName(def.FileName));
            def.FileName = newPath;

            return true;
        }
    }

}
