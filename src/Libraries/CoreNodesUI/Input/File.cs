using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Autodesk.DesignScript.Runtime;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

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

    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a file on the system to get its filename.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class Filename : FileSystemBrowser
    {
        public Filename(WorkspaceModel workspace) : base(workspace, "Filename") { }

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }
    }

    [NodeName("Directory Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a directory on the system to get its path.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class Directory : FileSystemBrowser
    {
        public Directory(WorkspaceModel workspace) : base(workspace, "Directory") { }

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }
    }

    /// <summary>
    ///     Base class for nodes that instantiate a File System Object, that also watches the file
    ///     system for changes.
    /// </summary>
    /// <typeparam name="T">
    ///     Data returned from the node, to be used for watching for changes to the file system.
    /// </typeparam>
    [SupressImportIntoVM]
    public abstract class FileSystemObject<T> : NodeModel
    {
        private IEnumerable<IDisposable> registrations = Enumerable.Empty<IDisposable>();
        private readonly Func<string, T> func;

        protected FileSystemObject(WorkspaceModel workspaceModel, Func<string, T> func)
            : base(workspaceModel)
        {
            this.func = func;
        }

        public override void Destroy()
        {
            base.Destroy();
            StopWatching();
        }

        private void StopWatching()
        {
            foreach (var reg in registrations)
                reg.Dispose();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            yield return
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(func, inputAstNodes));

            yield return
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), GetAstIdentifierForOutputIndex(0)));
        }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        private void DataBridgeCallback(object data)
        {
            StopWatching();
            registrations = UpdateWatchedFiles(data).ToList();
        }

        /// <summary>
        ///     Watches for changes on the file system, given some data.
        /// </summary>
        /// <param name="obj">Some data representing something to watch on the file system.</param>
        /// <returns>An IDisposable used to stop watching for file system changes.</returns>
        protected abstract IDisposable WatchFileSystemObject(T obj);

        protected abstract class FileSystemObjectDisposable : IDisposable
        {
            private readonly NodeModel node;

            protected FileSystemObjectDisposable(NodeModel nodeModel)
            {
                node = nodeModel;
            }

            protected void Modified()
            {
                node.ForceReExecuteOfNode = true;
                node.RequiresRecalc = true;
            }

            public abstract void Dispose();
        }

        private IEnumerable<IDisposable> UpdateWatchedFiles(object data)
        {
            if (data is T) //Single piece of data
            {
                try
                {
                    //Initiate watching, return IDisposable.
                    return Singleton(WatchFileSystemObject((T)data));
                }
                catch (Exception e)
                {
                    Warning(e.Message);
                }
            }

            if (data is ICollection) // Multiple pieces of data, recur
            {
                var paths = data as IEnumerable;
                return paths.Cast<object>().SelectMany(UpdateWatchedFiles);
            }

            // Data does not match expected type, skip
            return Enumerable.Empty<IDisposable>();
        }

        private static IEnumerable<TItem> Singleton<TItem>(TItem x)
        {
            yield return x;
        }
    }

    [NodeName("File.FromPath")]
    [NodeCategory(BuiltinNodeCategories.CORE_IO)]
    [NodeDescription("Creates a file object from a path.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class FileObject : FileSystemObject<FileInfo>
    {
        public FileObject(WorkspaceModel workspaceModel)
            : base(workspaceModel, IO.File.FromPath)
        {
            InPortData.Add(new PortData("path", "Path to the file."));
            OutPortData.Add(new PortData("file", "File object"));
            RegisterAllPorts();
        }

        protected override IDisposable WatchFileSystemObject(FileInfo path)
        {
            var dir =
                path.Directory ?? new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            var watcher = new FileSystemWatcher(dir.FullName, path.Name) { EnableRaisingEvents = true };

            return new FileObjectDisposable(watcher, this);
        }

        private class FileObjectDisposable : FileSystemObjectDisposable
        {
            private readonly FileSystemWatcher watcher;

            public FileObjectDisposable(FileSystemWatcher watcher, NodeModel node)
                : base(node)
            {
                this.watcher = watcher;

                watcher.Changed += watcher_Changed;
                watcher.Renamed += watcher_Changed;
                watcher.Deleted += watcher_Changed;
            }

            void watcher_Changed(object sender, FileSystemEventArgs e)
            {
                Modified();
            }

            public override void Dispose()
            {
                watcher.Changed -= watcher_Changed;
                watcher.Renamed -= watcher_Changed;
                watcher.Deleted -= watcher_Changed;
                watcher.Dispose();
            }
        }
    }

    [NodeName("Directory.FromPath")]
    [NodeCategory(BuiltinNodeCategories.CORE_IO)]
    [NodeDescription("Creates a directory object from a path.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class DirectoryObject : FileSystemObject<DirectoryInfo>
    {
        public DirectoryObject(WorkspaceModel workspaceModel)
            : base(workspaceModel, IO.Directory.FromPath)
        {
            InPortData.Add(new PortData("path", "Path to the directory."));
            OutPortData.Add(new PortData("directory", "Directory object."));
            RegisterAllPorts();
        }

        protected override IDisposable WatchFileSystemObject(DirectoryInfo path)
        {
            var watcher = new FileSystemWatcher(path.FullName) { EnableRaisingEvents = true };
            return new DirectoryObjectDisposable(watcher, this);
        }

        private class DirectoryObjectDisposable : FileSystemObjectDisposable
        {
            private readonly FileSystemWatcher watcher;

            public DirectoryObjectDisposable(FileSystemWatcher watcher, NodeModel node)
                : base(node)
            {
                this.watcher = watcher;

                watcher.Created += watcher_Changed;
                watcher.Renamed += watcher_Changed;
                watcher.Deleted += watcher_Changed;
            }

            void watcher_Changed(object sender, FileSystemEventArgs e)
            {
                Modified();
            }

            public override void Dispose()
            {
                watcher.Created -= watcher_Changed;
                watcher.Renamed -= watcher_Changed;
                watcher.Deleted -= watcher_Changed;
                watcher.Dispose();
            }
        }
    }
}
