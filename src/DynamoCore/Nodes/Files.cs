using System;
using System.IO;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    public abstract class FileReaderBase : NodeModel
    {
        readonly FileSystemEventHandler handler;

        string path;
        protected string storedPath
        {
            get { return path; }
            set
            {
                if (value != null && !value.Equals(path))
                {
                    if (watch != null)
                        watch.FileChanged -= handler;

                    path = value;
                    watch = new FileWatch(path);
                    watch.FileChanged += handler;
                }
            }
        }

        FileWatch watch;

        protected FileReaderBase(WorkspaceModel ws) : base(ws)
        {
            handler = watcher_FileChanged;
            InPortData.Add(new PortData("path", "Path to the file"));
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!this.Workspace.DynamoModel.Runner.Running)
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

        private readonly FileSystemWatcher watcher;
        private readonly FileSystemEventHandler handler;

        public event FileSystemEventHandler FileChanged;

        public FileWatch(string filePath)
        {
            Changed = false;

            var dir = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(dir))
                dir = ".";

            var name = Path.GetFileName(filePath);

            watcher = new FileSystemWatcher(dir, name)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            handler = watcher_Changed;
            watcher.Changed += handler;
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
            watcher.Changed -= handler;
            watcher.Dispose();
        }

        #endregion
    }
}
