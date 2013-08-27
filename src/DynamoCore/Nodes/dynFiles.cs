﻿//Copyright 2013 Ian Keough

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
using System.Linq;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    public abstract class dynFileReaderBase : dynNodeWithOneOutput
    {
        FileSystemEventHandler handler;

        string _path;
        protected string storedPath
        {
            get { return _path; }
            set
            {
                if (value != null && !value.Equals(_path))
                {
                    if (watcher != null)
                        watcher.FileChanged -= handler;

                    _path = value;
                    watcher = new FileWatcher(_path);
                    watcher.FileChanged += handler;
                }
            }
        }

        FileWatcher watcher;

        public dynFileReaderBase()
        {
            handler = new FileSystemEventHandler(watcher_FileChanged);

            InPortData.Add(new PortData("path", "Path to the file", typeof(Value.String)));
            

            //NodeUI.RegisterInputsAndOutput();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!Controller.Running)
                RequiresRecalc = true;
            else
            {
                //TODO: Refactor
                DisableReporting();
                RequiresRecalc = true;
                EnableReporting();
            }
        }
    }

    [NodeName("Read Text File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Reads data from a file.")]
    public class dynFileReader : dynNodeWithOneOutput
    {
        FileSystemEventHandler handler;

        string _path;
        string storedPath
        {
            get { return _path; }
            set
            {
                if (value != null && !value.Equals(_path))
                {
                    if (watcher != null)
                        watcher.FileChanged -= handler;

                    _path = value;
                    watcher = new FileWatcher(_path);
                    watcher.FileChanged += handler;
                }
            }
        }

        FileWatcher watcher;

        public dynFileReader()
        {
            handler = watcher_FileChanged;

            InPortData.Add(new PortData("path", "Path to the file", typeof(string)));
            OutPortData.Add(new PortData("contents", "File contents", typeof(string)));

            RegisterAllPorts();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!Controller.Running)
                RequiresRecalc = true;
            else
            {
                //TODO: Refactor
                DisableReporting();
                RequiresRecalc = true;
                EnableReporting();
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            storedPath = ((Value.String)args[0]).Item;

            if (File.Exists(storedPath))
            {
                string contents;

                contents = File.ReadAllText(storedPath);

                return Value.NewString(contents);
            }
            else
                return Value.NewString("");
        }
    }

    [NodeName("Write File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Writes the given string to the given file. Creates the file if it doesn't exist.")]
    public class dynFileWriter : dynNodeWithOneOutput
    {
        public dynFileWriter()
        {
            InPortData.Add(new PortData("path", "Path to the file", typeof(Value.String)));
            InPortData.Add(new PortData("text", "Text to be written", typeof(Value.String)));
            OutPortData.Add(new PortData("success?", "Whether or not the operation was successful.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string path = ((Value.String)args[0]).Item;
            string text = ((Value.String)args[1]).Item;

            try
            {
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
                return Value.NewNumber(0);
            }

            return Value.NewNumber(1);
        }
    }

    [NodeName("Write CSV File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Writes a list of lists into a file using a comma-separated values format. Outer list represents rows, inner lists represent column.")]
    public class dynListToCSV : dynNodeWithOneOutput
    {
        public dynListToCSV()
        {
            InPortData.Add(new PortData("path", "Filename to write to", typeof(Value.String)));
            InPortData.Add(new PortData("data", "List of lists to write into CSV", typeof(Value.List)));
            OutPortData.Add(new PortData("success?", "Whether or not the file writing was successful", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string path = ((Value.String)args[0]).Item;
            var data = ((Value.List)args[1]).Item;

            try
            {
                StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write));

                foreach (Value line in data)
                {
                    writer.WriteLine(string.Join(",", ((Value.List)line).Item.Select(x => x.Print())));
                }

                writer.Close();
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
                return Value.NewNumber(0);
            }

            return Value.NewNumber(1);
        }
    }

    [NodeName("Write Image File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Writes the given image to an image file. Creates the file if it doesn't exist.")]
    public class dynImageFileWriter : dynNodeWithOneOutput
    {
        public dynImageFileWriter()
        {
            InPortData.Add(new PortData("path", "Path to the file", typeof(Value.String)));
            InPortData.Add(new PortData("filename", "name of the file", typeof(Value.String)));
            InPortData.Add(new PortData("image", "Image to be written", typeof(Value.Container)));
            OutPortData.Add(new PortData("success?", "Whether or not the operation was successful.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string path = ((Value.String)args[0]).Item;
            string name = ((Value.String)args[1]).Item;
            System.Drawing.Image image = (System.Drawing.Image)((Value.Container)args[2]).Item;
            string pathName = path + "\\" + name + ".png";

            try
            {
                //if (image != null)
                //{
                    image.Save(pathName);
                    DynamoLogger.Instance.Log("Saved Image File " + pathName);
                //}


            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Error Saving Image File " + pathName);
                DynamoLogger.Instance.Log(e);
                return Value.NewNumber(0);
            }

            return Value.NewNumber(1);
        }
    }

    #region File Watcher

    [NodeName("Watch File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Creates a FileWatcher for watching changes in a file.")]
    public class dynFileWatcher : dynNodeWithOneOutput
    {
        public dynFileWatcher()
        {
            InPortData.Add(new PortData("path", "Path to the file to create a watcher for.", typeof(Value.String)));
            OutPortData.Add(new PortData("fw", "Instance of a FileWatcher.", typeof (Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string fileName = ((Value.String)args[0]).Item;
            return Value.NewContainer(new FileWatcher(fileName));
        }
    }

    [NodeName("Watched File Changed?")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Checks if the file watched by the given FileWatcher has changed.")]
    public class dynFileWatcherChanged : dynNodeWithOneOutput
    {
        public dynFileWatcherChanged()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(Value.Container)));
            OutPortData.Add(new PortData("changed?", "Whether or not the file has been changed.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var watcher = (FileWatcher)((Value.Container)args[0]).Item;

            return Value.NewNumber(watcher.Changed ? 1 : 0);
        }
    }

    //TODO: Add UI for specifying whether should error or continue (checkbox?)
    [NodeName("Watched File Wait")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Waits for the specified watched file to change.")]
    public class dynFileWatcherWait : dynNodeWithOneOutput
    {
        public dynFileWatcherWait()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(Value.Container)));
            InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing.", typeof(Value.Number)));
            OutPortData.Add(new PortData("changed?", "True: File was changed. False: Timed out.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var watcher = (FileWatcher)((Value.Container)args[0]).Item;
            double timeout = ((Value.Number)args[1]).Item;

            timeout = timeout == 0 ? double.PositiveInfinity : timeout;

            int tick = 0;
            while (!watcher.Changed)
            {
                if (Controller.RunCancelled)
                    throw new CancelEvaluationException(false);

                Thread.Sleep(10);
                tick += 10;

                if (tick >= timeout)
                {
                    throw new Exception("File watcher timeout!");
                }
            }

            return Value.NewNumber(1);
        }
    }

    [NodeName("Reset File Watch")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Resets state of FileWatcher so that it watches again.")]
    public class dynFileWatcherReset : dynNodeWithOneOutput
    {
        public dynFileWatcherReset()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(Value.Container)));
            OutPortData.Add(new PortData("fw", "Updated watcher.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var watcher = (FileWatcher)((Value.Container)args[0]).Item;

            watcher.Reset();

            return Value.NewContainer(watcher);
        }
    }

    class FileWatcher : IDisposable
    {
        public bool Changed { get; private set; }

        private readonly FileSystemWatcher _watcher;
        private readonly FileSystemEventHandler _handler;

        public event FileSystemEventHandler FileChanged;

        public FileWatcher(string filePath)
        {
            Changed = false;

            _watcher = new FileSystemWatcher(
                Path.GetDirectoryName(filePath), Path.GetFileName(filePath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _handler = watcher_Changed;
            _watcher.Changed += _handler;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Changed = true;
            if (FileChanged != null)
                FileChanged(sender, e);
        }

        public void Reset()
        {
            Changed = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _watcher.Changed -= _handler;
            _watcher.Dispose();
        }

        #endregion
    }

    #endregion
}
