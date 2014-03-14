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
    public abstract class FileReaderBase : NodeWithOneOutput
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
                    if (_watch != null)
                        _watch.FileChanged -= handler;

                    _path = value;
                    _watch = new FileWatch(_path);
                    _watch.FileChanged += handler;
                }
            }
        }

        FileWatch _watch;

        public FileReaderBase()
        {
            handler = watcher_FileChanged;

            InPortData.Add(new PortData("path", "Path to the file", typeof(Value.String)));
            

            //NodeUI.RegisterInputsAndOutput();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!dynSettings.Controller.Running)
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

    class FileWatch : IDisposable
    {
        public bool Changed { get; private set; }

        private readonly FileSystemWatcher _watcher;
        private readonly FileSystemEventHandler _handler;

        public event FileSystemEventHandler FileChanged;

        public FileWatch(string filePath)
        {
            Changed = false;

            var dir = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(dir))
                dir = ".";

            var name = Path.GetFileName(filePath);

            _watcher = new FileSystemWatcher(dir, name)
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
}
