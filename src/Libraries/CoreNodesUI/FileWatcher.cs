using DSCore;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCoreNodesUI
{
    [NodeName("FileWatcher")]
    [NodeDescription("Creates a FileWatcher for watching changes to a file.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    public class FileWatcher : NodeModel
    {
        public FileWatcher(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("filepath", "Path to the file to create a watcher for."));
            OutPortData.Add(new PortData("fileWatcher", "Instance of a FileWatcher."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<string, DSCore.FileWatch>(FileWatcherCore.FileWatcher), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };

        }
    }

    [NodeName("FileWatcherChanged")]
    [NodeDescription("Checks if the file watched by the given FileWatcher has changed.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    public class FileWatcherChanged : NodeModel
    {
        public FileWatcherChanged(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("fileWatcher", "File Watcher to check for a change."));
            OutPortData.Add(new PortData("changed?", "Whether or not the file has been changed."));

            RegisterAllPorts();
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<DSCore.FileWatch, bool>(FileWatcherCore.FileWatcherChanged), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };

        }
    }

    [NodeName("FileWatcherWait")]
    [NodeDescription("Waits for the specified watched file to change.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    public class FileWatcherWait : NodeModel
    {
        public FileWatcherWait(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("fileWatcher", "File Watcher to check for a change."));
            InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing."));
            OutPortData.Add(new PortData("changed?", "True: File was changed. False: Timed out."));

            RegisterAllPorts();
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<DSCore.FileWatch, int, bool>(FileWatcherCore.FileWatcherWait), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };

        }
    }

    [NodeName("FileWatcherReset")]
    [NodeDescription("Resets state of FileWatcher so that it watches again.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    public class FileWatcherReset : NodeModel
    {
        public FileWatcherReset(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("fileWatcher", "File Watcher to check for a change."));
            OutPortData.Add(new PortData("fileWatcher", "Updated watcher."));

            RegisterAllPorts();
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<DSCore.FileWatch, DSCore.FileWatch>(FileWatcherCore.FileWatcherReset), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };

        }
    }
}
