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
using Expression = Dynamo.FScheme.Expression;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace Dynamo.Elements
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

            //base.RegisterInputsAndOutputs();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!this.Bench.Running)
                this.IsDirty = true;
            else
            {
                //TODO: Refactor
                this.DisableReporting();
                this.IsDirty = true;
                this.EnableReporting();
            }
        }
    }

    [ElementName("Read File")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Reads data from a file.")]
    [RequiresTransaction(false)]
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

            base.RegisterInputsAndOutputs();
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!this.Bench.Running)
                this.IsDirty = true;
            else
            {
                //TODO: Refactor
                this.DisableReporting();
                this.IsDirty = true;
                this.EnableReporting();
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            storedPath = ((Expression.String)args[0]).Item;

            if (File.Exists(storedPath))
            {
                StreamReader reader = new StreamReader(
                    new FileStream(storedPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                );
                string contents = reader.ReadToEnd();
                reader.Close();

                return Expression.NewString(contents);
            }
            else
                return Expression.NewString("");
        }
    }

    [ElementName("Read Image File")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Reads data from an image file.")]
    [RequiresTransaction(false)]
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

            this.inputGrid.Children.Add(image1);

            base.RegisterInputsAndOutputs();

            this.Width = 450;
            this.Height = 240 + 5;

            //this.Loaded += new RoutedEventHandler(topControl_Loaded);

        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            storedPath = ((Expression.String)args[0]).Item;
            double xDiv = ((Expression.Number)args[1]).Item;
            double yDiv = ((Expression.Number)args[1]).Item;

            FSharpList<Expression> result = FSharpList<Expression>.Empty;
            if (File.Exists(storedPath))
            {

                    try
                    {
                        using (Bitmap bmp = new Bitmap(storedPath))
                        {

                            this.Dispatcher.Invoke(new Action(
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
                                    result = FSharpList<Expression>.Cons(Expression.NewContainer(pixelColor), result);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.Bench.Log(e.ToString());
                    }


                return Expression.NewList(result);
            }
            else
                return Expression.NewList(FSharpList<Expression>.Empty);
        }
    }

    [ElementName("Write File")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Writes the given string to the given file. Creates the file if it doesn't exist.")]
    public class dynFileWriter : dynNode
    {
        public dynFileWriter()
        {
            InPortData.Add(new PortData("path", "Path to the file", typeof(string)));
            InPortData.Add(new PortData("text", "Text to be written", typeof(string)));
            OutPortData = new PortData("success?", "Whether or not the operation was successful.", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string path = ((Expression.String)args[0]).Item;
            string text = ((Expression.String)args[1]).Item;

            try
            {
                StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));
                writer.Write(text);
                writer.Close();
            }
            catch (Exception e)
            {
                this.Bench.Log(e);
                return Expression.NewNumber(0);
            }

            return Expression.NewNumber(1);
        }
    }

    [ElementName("Write CSV File")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Writes a list of lists into a file using a comma-separated values format. Outer list represents rows, inner lists represent column.")]
    public class dynListToCSV : dynNode
    {
        public dynListToCSV()
        {
            InPortData.Add(new PortData("path", "Filename to write to", typeof(string)));
            InPortData.Add(new PortData("data", "List of lists to write into CSV", typeof(IList<IList<string>>)));
            OutPortData = new PortData("success?", "Whether or not the file writing was successful", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string path = ((Expression.String)args[0]).Item;
            var data = ((Expression.List)args[1]).Item;

            try
            {
                StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));

                foreach (Expression line in data)
                {
                    writer.WriteLine(string.Join(",", ((Expression.List)line).Item.Select(x => ((Expression.String)x))));
                }

                writer.Close();
            }
            catch (Exception e)
            {
                this.Bench.Log(e);
                return Expression.NewNumber(0);
            }

            return Expression.NewNumber(1);
        }
    }


    #region File Watcher

    [ElementName("Watch File")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Creates a FileWatcher for watching changes in a file.")]
    [RequiresTransaction(false)]
    public class dynFileWatcher : dynNode
    {
        public dynFileWatcher()
        {
            this.InPortData.Add(new PortData("path", "Path to the file to create a watcher for.", typeof(FileWatcher)));
            this.OutPortData = new PortData("fw", "Instance of a FileWatcher.", typeof(FileWatcher));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string fileName = ((Expression.String)args[0]).Item;
            return Expression.NewContainer(new FileWatcher(fileName));
        }
    }

    [ElementName("Watched File Changed?")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Checks if the file watched by the given FileWatcher has changed.")]
    [RequiresTransaction(false)]
    public class dynFileWatcherChanged : dynNode
    {
        public dynFileWatcherChanged()
        {
            this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            this.OutPortData = new PortData("changed?", "Whether or not the file has been changed.", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;

            return Expression.NewNumber(watcher.Changed ? 1 : 0);
        }
    }

    //TODO: Add UI for specifying whether should error or continue (checkbox?)
    [ElementName("File Watcher Wait for Change")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Waits for the specified watched file to change.")]
    [RequiresTransaction(false)]
    public class dynFileWatcherWait : dynNode
    {
        public dynFileWatcherWait()
        {
            this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            this.InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing.", typeof(double)));
            this.OutPortData = new PortData("changed?", "True: File was changed. False: Timed out.", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;
            double timeout = ((Expression.Number)args[1]).Item;

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

            return Expression.NewNumber(1);
        }
    }

    [ElementName("Reset File Watcher")]
    [ElementCategory(BuiltinElementCategories.FILES)]
    [ElementDescription("Resets state of FileWatcher so that it watches again.")]
    [RequiresTransaction(false)]
    public class dynFileWatcherReset : dynNode
    {
        public dynFileWatcherReset()
        {
            this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
            this.OutPortData = new PortData("fw", "Updated watcher.", typeof(FileWatcher));

            base.RegisterInputsAndOutputs();
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
