//Copyright 2012 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using System.Windows.Forms;

namespace Dynamo.Nodes
{
    [NodeName("Read File")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Create an element for reading and watching data in a file on disk.")]
    public class dynFileReader : dynNode
    {
        public dynFileReader()
        {
            InPortData.Add(new PortData("path", "Path to the file", typeof(string)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("contents", "File contents", typeof(string));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string arg = ((Expression.String)args[0]).Item;

            StreamReader reader = new StreamReader(new FileStream(arg, FileMode.Open, FileAccess.Read, FileShare.Read));
            string contents = reader.ReadToEnd();
            reader.Close();

            return Expression.NewString(contents);
        }
    }

    #region File Watcher

    //SJE
    //TODO: Update (or make different versions)
    [NodeName("Watch File")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Create an element for reading and watching data in a file on disk.")]
    public class dynFileWatcher : dynNode
    {
        public dynFileWatcher()
        {
            InPortData.Add(new PortData("path", "Path to the file to create a watcher for.", typeof(FileWatcher)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("fw", "Instance of a FileWatcher.", typeof(FileWatcher));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string fileName = ((Expression.String)args[0]).Item;
            return Expression.NewContainer(new FileWatcher(fileName));
        }
    }

    [NodeName("Watched File Changed?")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Checks if the file watched by the given FileWatcher has changed.")]
    public class dynFileWatcherChanged : dynNode
    {
        public dynFileWatcherChanged()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("changed?", "Whether or not the file has been changed.", typeof(bool));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;

            return Expression.NewNumber(watcher.Changed ? 1 : 0);
        }
    }

    //TODO: Add UI for specifying whether should error or continue (checkbox?)
    [NodeName("Wait for Change")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Waits for the specified watched file to change.")]
    public class dynFileWatcherWait : dynNode
    {
        public dynFileWatcherWait()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing.", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("changed?", "True: File was changed. False: Timed out.", typeof(bool));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;
            double timeout = ((Expression.Number)args[1]).Item;

            int tick = 0;
            while (!watcher.Changed)
            {
                Thread.Sleep(10);
                tick += 10;

                if (tick >= timeout)
                {
                    throw new Exception("File watcher timeout!");
                }
            }

            return Expression.NewNumber(1);
        }
    }

    [NodeName("Reset File Watcher")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Resets state of FileWatcher so that it watches again.")]
    public class dynFileWatcherReset : dynNode
    {
        public dynFileWatcherReset()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("fw", "Updated watcher.", typeof(FileWatcher));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;

            watcher.Reset();

            return Expression.NewContainer(watcher);
        }
    }

    class FileWatcher : IDisposable
    {
        public bool Changed = false;

        private FileSystemWatcher watcher;
        private FileSystemEventHandler handler;

        public FileWatcher(string filePath)
        {
            this.watcher = new FileSystemWatcher(
               Path.GetDirectoryName(filePath),
               Path.GetFileName(filePath)
            );
            this.handler = new FileSystemEventHandler(watcher_Changed);

            this.watcher.Changed += handler;
            this.watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            this.watcher.EnableRaisingEvents = true;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.Changed = true;
        }

        public void Reset()
        {
            this.Changed = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.watcher.Changed -= handler;
            this.watcher.Dispose();
        }

        #endregion
    }

    #endregion
}
