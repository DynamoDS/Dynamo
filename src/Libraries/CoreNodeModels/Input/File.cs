﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using DSCoreNodesUI.Properties;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace DSCoreNodesUI.Input
{
    [SupressImportIntoVM]
    public abstract class FileSystemBrowser : String
    {
        protected FileSystemBrowser(string tip)
            : base()
        {
            OutPortData[0].ToolTipString = tip;
            RegisterAllPorts();

            Value = "";
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            if (context == CompilationContext.NodeToCode)
            {
                var rhs = AstFactory.BuildStringNode(Value.Replace(@"\", @"\\"));
                var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);
                return new[] { assignment };
            }
            else
            {
                return base.BuildAst(inputAstNodes, context);
            }
        }
    }

    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("FilenameNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("FilePathSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.Filename")]
    public class Filename : FileSystemBrowser
    {
        public Filename() : base("Filename")
        {
            ShouldDisplayPreviewCore = false;
        }
    }

    [NodeName("Directory Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("DirectoryNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("DirectoryPathSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.Directory")]
    public class Directory : FileSystemBrowser
    {
        public Directory() : base("Directory")
        {
            ShouldDisplayPreviewCore = false;
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

        protected FileSystemObject(Func<string, T> func)
        {
            this.func = func;
        }

        public override void Dispose()
        {
            base.Dispose();
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
                node.OnNodeModified(forceExecute: true);
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
    [NodeDescription("FileObjectNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("FilePathSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.FileObject")]
    public class FileObject : FileSystemObject<FileInfo>
    {
        public FileObject()
            : base(DSCore.IO.File.FromPath)
        {
            InPortData.Add(new PortData("path", Resources.FileObjectPortDataPathToolTip));
            OutPortData.Add(new PortData("file", Resources.FileObjectPortDataResultToolTip));
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
    [NodeDescription("DirectoryObjectNodeDescription",typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("DirectoryPathSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.DirectoryObject")]
    public class DirectoryObject : FileSystemObject<DirectoryInfo>
    {
        public DirectoryObject()
            : base(DSCore.IO.Directory.FromPath)
        {
            InPortData.Add(new PortData("path", Resources.DirectoryObjectPortDataPathToolTip));
            OutPortData.Add(new PortData("directory", Resources.DirectoryObjectPortDataResultToolTip));
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
