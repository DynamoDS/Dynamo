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
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace Dynamo.Nodes
{
    public abstract class dynFileReaderBase : dynNode
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
            this.handler = new FileSystemEventHandler(watcher_FileChanged);

            InPortData.Add(new PortData("path", "Path to the file", typeof(string)));
            OutPortData = new PortData("contents", "File contents", typeof(string));

            //NodeUI.RegisterInputsAndOutput();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!this.Bench.Running)
                this.RequiresRecalc = true;
            else
            {
                //TODO: Refactor
                this.DisableReporting();
                this.RequiresRecalc = true;
                this.EnableReporting();
            }
        }
    }

    [NodeName("Read File")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Reads data from a file.")]
    public class dynFileReader : dynNode
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
            this.handler = new FileSystemEventHandler(watcher_FileChanged);

            InPortData.Add(new PortData("path", "Path to the file", typeof(string)));
            OutPortData = new PortData("contents", "File contents", typeof(string));

            NodeUI.RegisterInputsAndOutput();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!this.Bench.Running)
                this.RequiresRecalc = true;
            else
            {
                //TODO: Refactor
                this.DisableReporting();
                this.RequiresRecalc = true;
                this.EnableReporting();
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            storedPath = ((Value.String)args[0]).Item;

            if (File.Exists(storedPath))
            {
                StreamReader reader = new StreamReader(
                    new FileStream(storedPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                );
                string contents = reader.ReadToEnd();
                reader.Close();

                return Value.NewString(contents);
            }
            else
                return Value.NewString("");
        }
    }

    [NodeName("Read Image File")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Reads data from an image file.")]
    public class dynImageFileReader : dynFileReaderBase
    {

        System.Windows.Controls.Image image1;
        int width = 320;
        int height = 240;

        public dynImageFileReader()
        {

            InPortData.Add(new PortData("numX", "Number of samples in the X direction.", typeof(object)));
            InPortData.Add(new PortData("numY", "Number of samples in the Y direction.", typeof(object)));

            image1 = new System.Windows.Controls.Image();
            image1.Width = 320;
            image1.Height = 240;
            image1.Margin = new Thickness(5);
            image1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            image1.Name = "image1";
            image1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            
            //image1.Margin = new Thickness(0, 0, 0, 0);

            NodeUI.inputGrid.Children.Add(image1);

            NodeUI.RegisterInputsAndOutput();

            NodeUI.Width = 450;
            NodeUI.Height = 240 + 5;

            //this.Loaded += new RoutedEventHandler(topControl_Loaded);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            storedPath = ((Value.String)args[0]).Item;
            double xDiv = ((Value.Number)args[1]).Item;
            double yDiv = ((Value.Number)args[1]).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;
            if (File.Exists(storedPath))
            {

                    try
                    {
                        using (Bitmap bmp = new Bitmap(storedPath))
                        {

                            NodeUI.Dispatcher.Invoke(new Action(
                                delegate
                                {
                                    // how to convert a bitmap to an imagesource http://blog.laranjee.com/how-to-convert-winforms-bitmap-to-wpf-imagesource/ 
                                    // TODO - watch out for memory leaks using system.drawing.bitmaps in managed code, see here http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/4e213af5-d546-4cc1-a8f0-462720e5fcde
                                    // need to call Dispose manually somewhere, or perhaps use a WPF native structure instead of bitmap?

                                    var hbitmap = bmp.GetHbitmap();
                                    var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                                    image1.Source = imageSource;
                                }
                            ));


                            // Do some processing
                            for (int y = 0; y < yDiv; y++)
                            {
                                for (int x = 0; x < xDiv; x++)
                                {
                                    Color pixelColor = bmp.GetPixel(x * (int)(bmp.Width / xDiv), y * (int)(bmp.Height / yDiv));
                                    result = FSharpList<Value>.Cons(Value.NewContainer(pixelColor), result);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.Bench.Log(e.ToString());
                    }


                return Value.NewList(result);
            }
            else
                return Value.NewList(FSharpList<Value>.Empty);
        }
    }

    [NodeName("Write File")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Writes the given string to the given file. Creates the file if it doesn't exist.")]
    public class dynFileWriter : dynNode
    {
        public dynFileWriter()
        {
            InPortData.Add(new PortData("path", "Path to the file", typeof(string)));
            InPortData.Add(new PortData("text", "Text to be written", typeof(string)));
            OutPortData = new PortData("success?", "Whether or not the operation was successful.", typeof(bool));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string path = ((Value.String)args[0]).Item;
            string text = ((Value.String)args[1]).Item;

            try
            {
                StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));
                writer.Write(text);
                writer.Close();
            }
            catch (Exception e)
            {
                this.Bench.Log(e);
                return Value.NewNumber(0);
            }

            return Value.NewNumber(1);
        }
    }

    [NodeName("Write CSV File")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Writes a list of lists into a file using a comma-separated values format. Outer list represents rows, inner lists represent column.")]
    public class dynListToCSV : dynNode
    {
        public dynListToCSV()
        {
            InPortData.Add(new PortData("path", "Filename to write to", typeof(string)));
            InPortData.Add(new PortData("data", "List of lists to write into CSV", typeof(IList<IList<string>>)));
            OutPortData = new PortData("success?", "Whether or not the file writing was successful", typeof(bool));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string path = ((Value.String)args[0]).Item;
            var data = ((Value.List)args[1]).Item;

            try
            {
                StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));

                foreach (Value line in data)
                {
                    writer.WriteLine(string.Join(",", ((Value.List)line).Item.Select(x => ((Value.String)x))));
                }

                writer.Close();
            }
            catch (Exception e)
            {
                this.Bench.Log(e);
                return Value.NewNumber(0);
            }

            return Value.NewNumber(1);
        }
    }


    #region File Watcher

    [NodeName("Watch File")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Creates a FileWatcher for watching changes in a file.")]
    public class dynFileWatcher : dynNode
    {
        public dynFileWatcher()
        {
            this.InPortData.Add(new PortData("path", "Path to the file to create a watcher for.", typeof(FileWatcher)));
            this.OutPortData = new PortData("fw", "Instance of a FileWatcher.", typeof(FileWatcher));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string fileName = ((Value.String)args[0]).Item;
            return Value.NewContainer(new FileWatcher(fileName));
        }
    }

    [NodeName("Watched File Changed?")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Checks if the file watched by the given FileWatcher has changed.")]
    public class dynFileWatcherChanged : dynNode
    {
        public dynFileWatcherChanged()
        {
            this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            this.OutPortData = new PortData("changed?", "Whether or not the file has been changed.", typeof(bool));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FileWatcher watcher = (FileWatcher)((Value.Container)args[0]).Item;

            return Value.NewNumber(watcher.Changed ? 1 : 0);
        }
    }

    //TODO: Add UI for specifying whether should error or continue (checkbox?)
    [NodeName("File Watcher Wait for Change")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Waits for the specified watched file to change.")]
    public class dynFileWatcherWait : dynNode
    {
        public dynFileWatcherWait()
        {
            this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            this.InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing.", typeof(double)));
            this.OutPortData = new PortData("changed?", "True: File was changed. False: Timed out.", typeof(bool));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FileWatcher watcher = (FileWatcher)((Value.Container)args[0]).Item;
            double timeout = ((Value.Number)args[1]).Item;

            timeout = timeout == 0 ? double.PositiveInfinity : timeout;

            int tick = 0;
            while (!watcher.Changed)
            {
                if (Bench.RunCancelled)
                    throw new Dynamo.Controls.CancelEvaluationException(false);

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

    [NodeName("Reset File Watcher")]
    [NodeCategory(BuiltinNodeCategories.FILES)]
    [NodeDescription("Resets state of FileWatcher so that it watches again.")]
    public class dynFileWatcherReset : dynNode
    {
        public dynFileWatcherReset()
        {
            this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            this.OutPortData = new PortData("fw", "Updated watcher.", typeof(FileWatcher));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FileWatcher watcher = (FileWatcher)((Value.Container)args[0]).Item;

            watcher.Reset();

            return Value.NewContainer(watcher);
        }
    }

    class FileWatcher : IDisposable
    {
        public bool Changed { get; private set; }

        private FileSystemWatcher watcher;
        private FileSystemEventHandler handler;

        public event FileSystemEventHandler FileChanged;

        public FileWatcher(string filePath)
        {
            this.Changed = false;

            this.watcher = new FileSystemWatcher(
               Path.GetDirectoryName(filePath),
               Path.GetFileName(filePath)
            );
            this.handler = new FileSystemEventHandler(watcher_Changed);

            this.watcher.Changed += handler;

            this.watcher.NotifyFilter = NotifyFilters.LastWrite;

            this.watcher.EnableRaisingEvents = true;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.Changed = true;
            if (FileChanged != null)
                FileChanged(sender, e);
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
