using System;
using System.IO;
using Dynamo.Models;
using Autodesk.DesignScript.Runtime;
using ProtoCore.AST.AssociativeAST;
using System.Collections.Generic;

namespace Dynamo.Nodes
{
    //public abstract class FileReaderBase : NodeModel
    //{
    //    readonly FileSystemEventHandler handler;

    //    string path;
    //    protected string storedPath
    //    {
    //        get { return path; }
    //        set
    //        {
    //            if (value != null && !value.Equals(path))
    //            {
    //                if (watch != null)
    //                    watch.FileChanged -= handler;

    //                path = value;
    //                watch = new FileWatch(path);
    //                watch.FileChanged += handler;
    //            }
    //        }
    //    }

    //    FileWatch watch;

    //    protected FileReaderBase(WorkspaceModel ws) : base(ws)
    //    {
    //        handler = watcher_FileChanged;
    //        InPortData.Add(new PortData("path", "Path to the file"));
    //    }

    //    void watcher_FileChanged(object sender, FileSystemEventArgs e)
    //    {
    //        if (!this.Workspace.DynamoModel.Runner.Running)
    //            RequiresRecalc = true;
    //        else
    //        {
    //            //TODO: Refactor
    //            DisableReporting();
    //            RequiresRecalc = true;
    //            EnableReporting();
    //        }
    //    }
    //}

    

    
}
