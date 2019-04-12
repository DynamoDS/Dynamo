using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace CoreNodeModels.Input
{
    [SupressImportIntoVM]
    public abstract class FileSystemBrowser : String
    {
        private static readonly string HintPathString = "HintPath";
        public string HintPath { get; set; }

        protected FileSystemBrowser(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Value = "";
            HintPath = Value;
            PropertyChanged += OnPropertyChanged;
        }

        protected FileSystemBrowser(string tip)
            : base()
        {
            OutPorts[0].ToolTip = tip;
            RegisterAllPorts();

            Value = "";
            HintPath = Value;
            PropertyChanged += OnPropertyChanged;
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CachedValue")
                HintPath = CachedValue.Data as string; //The new value is my hint path.
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            if (context == CompilationContext.NodeToCode)
            {
                var rhs = AstFactory.BuildStringNode(Value.Replace(@"\", @"\\"));
                yield return AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);
            }
            else
            {
                var ast = new List<AssociativeNode>();
                ast.Add(AstFactory.BuildStringNode(Value));
                ast.Add(AstFactory.BuildStringNode(HintPath));
                yield return
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionCall<string,string,string>(DSCore.IO.FileSystem.AbsolutePath, ast));
            }
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called

            if (!string.IsNullOrEmpty(HintPath))
            {
                var xmlDocument = element.OwnerDocument;
                var subNode = xmlDocument.CreateElement(HintPathString);
                subNode.InnerText = HintPath;
                element.AppendChild(subNode);
            }
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context); // Base implementation must be called

            foreach (XmlNode subNode in nodeElement.ChildNodes.Cast<XmlNode>()
                .Where(subNode => subNode.Name.Equals(HintPathString)))
            {
                HintPath = subNode.InnerText;
            }
        }
    }

    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("FilenameNodeDescription", typeof(Resources))]
    [NodeSearchTags("FilePathSearchTags", typeof(Resources))]
    [SupressImportIntoVM]
    [InPortTypes("UI Input")]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.Filename", "DSCoreNodesUI.Input.Filename")]
    public class Filename : FileSystemBrowser
    {
        [JsonConstructor]
        private Filename(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = false;
        }

        public Filename() : base("Filename")
        {
            ShouldDisplayPreviewCore = false;
        }
    }

    [NodeName("Directory Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("DirectoryNodeDescription", typeof(Resources))]
    [NodeSearchTags("DirectoryPathSearchTags", typeof(Resources))]
    [InPortTypes("UI Input")]
    [OutPortTypes("bool")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.Directory", "DSCoreNodesUI.Input.Directory")]
    public class Directory : FileSystemBrowser
    {
        [JsonConstructor]
        private Directory(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = false;
        }

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

        protected FileSystemObject(Func<string, T> func, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            this.func = func;
        }

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

    [NodeName("File From Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_IO)]
    [NodeDescription("FileObjectNodeDescription", typeof(Resources))]
    [NodeSearchTags("FilePathSearchTags", typeof(Resources))]
    [SupressImportIntoVM]
    [OutPortTypes("object")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.FileObject", "DSCoreNodesUI.Input.FileObject")]
    public class FileObject : FileSystemObject<FileInfo>
    {

        [JsonConstructor]
        private FileObject(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : 
            base(DSCore.IO.FileSystem.FileFromPath, inPorts, outPorts) { }

        public FileObject()
            : base(DSCore.IO.FileSystem.FileFromPath)
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("path", Resources.FileObjectPortDataPathToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("file", Resources.FileObjectPortDataResultToolTip)));
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

    [NodeName("Directory From Path")]
    [NodeCategory("Core.File")]
    [NodeDescription("DirectoryObjectNodeDescription",typeof(Resources))]
    [NodeSearchTags("DirectoryPathSearchTags", typeof(Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.File.DirectoryObject", "DSCoreNodesUI.Input.DirectoryObject", "Directory.FromPath")]
    public class DirectoryObject : FileSystemObject<DirectoryInfo>
    {
        [JsonConstructor]
        private DirectoryObject(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : 
            base(DSCore.IO.FileSystem.DirectoryFromPath, inPorts, outPorts) { }

        public DirectoryObject()
            : base(DSCore.IO.FileSystem.DirectoryFromPath)
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("path", Resources.DirectoryObjectPortDataPathToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("directory", Resources.DirectoryObjectPortDataResultToolTip)));
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
