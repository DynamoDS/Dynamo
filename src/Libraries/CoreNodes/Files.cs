
using System.Collections;
using System.Drawing;
using Autodesk.DesignScript.Runtime;
using System.IO;
using System.Text;
using System;

namespace DSCore
{
    public static class File
    {
        /// <summary>
        ///     Load a bitmap from a file path.
        /// </summary>
        /// <param name="path">The path to the image file.</param>
        /// <returns name="bitmap">Bitmap</returns>
        public static Bitmap LoadImageFromPath(string path)
        {
            if (!System.IO.File.Exists(path)) return null;

            var bmp = new Bitmap(path);
            return bmp;
        }

        /// <summary>
        ///     Reads an image file and returns the color values at the specified grid locations.
        /// </summary>
        /// <param name="filePath">Path to the image file.</param>
        /// <param name="numX">Number of sample grid points in the X direction.</param>
        /// <param name="numY">Number of sample grid points in the Y direction.</param>
        /// <returns name="colors">Colors at the specified grid points.</returns>
        /// <search>read,image,bitmap,png,jpg,jpeg</search>
        public static IList ReadImage(string filePath, int numX, int numY)
        {
            if (System.IO.File.Exists(filePath) == false)
                return List.Empty;

            IList result = List.Empty;
            var bmp = new Bitmap(filePath);

            for (int y = 0; y < numY; y++)
            {
                for (int x = 0; x < numX; x++)
                {
                    int xParam = x * (bmp.Width / numX);
                    int yParam = y * (bmp.Height / numY);

                    // Insert new color at the front of the list.
                    var c = bmp.GetPixel(xParam, yParam);
                    result.Insert(0, Color.ByARGB(c.A,c.R,c.G,c.B));
                }
            }

            return result;
        }

        /// <summary>
        ///     Reads a text file and returns the contents as a string.
        /// </summary>
        /// <param name="filePath">Path to the text file.</param>
        /// <returns name="str">Contents of the text file.</returns>
        /// <search>read file,text,file</search>
        public static string ReadText(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
                return string.Empty;

            return System.IO.File.ReadAllText(filePath);
        }

        /// <summary>
        ///     Write a list of lists into a file using a comma-separated values 
        ///     format. Outer list represents rows, inner lists represent column. 
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="data">List of lists to write into CSV</param>
        /// <returns name="str">Contents of the text file.</returns>
        /// <search>write,text,file</search>
        public static bool ExportToCSV(string filePath, double[][] data)
        {
            try
            {
                System.IO.StreamWriter writer = new StreamWriter(filePath);

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    StringBuilder line = new StringBuilder();

                    for (int j = 0; j < data[i].Length; j++)
                    {
                        line.Append(data[i][j]);
                        line.Append(", ");
                    }

                    string lineOut = line.ToString();
                    lineOut = lineOut.Substring(0, lineOut.LastIndexOf(','));
                    writer.WriteLine(lineOut);
                }
                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Write the text content to a file specified by the path
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="data">Text content</param>
        /// <returns name="ok">It is successful or not.</returns>
        /// <search>write file,text,file</search>
        public static bool WriteText(string filePath, string text)
        {
            try
            {
                System.IO.File.WriteAllText(filePath, text);
            }
            catch (Exception e)
            {
                throw new Exception("The text is not written to the file successfully!", e);
            }

            return true;
        }

        /// <summary>
        ///     Write the image to a path, given the specified file name.
        ///     The file name will be appended with .png. 
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="fileName">File name to save as</param>
        /// <param name="image">The image to write</param>
        /// <returns name="ok">It is successful or not.</returns>
        /// <search>write image,image,file</search>
        public static bool WriteImage(string filePath, string fileName, System.Drawing.Bitmap image)
        {
            string pathName = Path.Combine(filePath, fileName + ".png");

            try
            {
                image.Save(pathName);
            }
            catch (Exception e)
            {
                throw new Exception("The image is not saved successfully!", e);
            }

            return true;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class FileWatch : IDisposable
    {
        public bool Changed { get; private set; }

        private readonly FileSystemWatcher watcher;
        private readonly FileSystemEventHandler handler;

        public event FileSystemEventHandler FileChanged;

        [IsVisibleInDynamoLibrary(false)]
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
                FileChanged(this, e);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Reset()
        {
            Changed = false;
        }

        #region IDisposable Members

        [IsVisibleInDynamoLibrary(false)]
        public void Dispose()
        {
            watcher.Changed -= handler;
            watcher.Dispose();
        }

        #endregion
    }

    [IsVisibleInDynamoLibrary(false)]
    public static class FileWatcherCore
    {
        /// <summary>
        /// Creates a FileWatcher for watching changes to a file.
        /// </summary>
        /// <param name="fileName">Path to the file to create a watcher for</param>
        /// <returns>Instance of a FileWatcher</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static FileWatch FileWatcher(string fileName)
        {
            FileWatch fw = new FileWatch(fileName);
            fw.FileChanged += fileWatcherChanged;
            return fw;
        }

        private static void fileWatcherChanged(object sender, FileSystemEventArgs e)
        {
            // Need to re-execute "FileWatcherChanged" node in LiveRunner ??
            FileWatcherChanged((FileWatch)sender);
        }

        /// <summary>
        /// Checks if the file watched by the given FileWatcher has changed.
        /// </summary>
        /// <param name="fileWatcher">File Watcher to check for a change</param>
        /// <returns>Whether or not the file has been changed</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static bool FileWatcherChanged(FileWatch fileWatcher)
        {
            //dynSettings.Controller.DynamoViewModel.Model.HomeSpace.Nodes.Select(n => n is FileWatcherChanged);
            return fileWatcher.Changed;
        }

        /// <summary>
        /// Waits for the specified watched file to change
        /// </summary>
        /// <param name="fileWatcher">File Watcher to check for a change</param>
        /// <param name="limit">Amount of time (in milliseconds) to wait for an update before failing.</param>
        /// <returns>True: File was changed. False: Timed out.</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static bool FileWatcherWait(FileWatch fileWatcher, int limit)
        {
            double timeout = limit == 0 ? double.PositiveInfinity : limit;

            int tick = 0;
            while (!fileWatcher.Changed)
            {
                //if (dynSettings.Controller.RunCancelled)
                //    throw new Exception("Run Cancelled");

                System.Threading.Thread.Sleep(10);
                tick += 10;

                if (tick >= timeout)
                {
                    throw new Exception("File watcher timeout!");
                }
            }

            return true;
        }

        /// <summary>
        /// Resets state of FileWatcher so that it watches again.
        /// </summary>
        /// <param name="fileWatcher">File Watcher to check for a change</param>
        /// <returns>Updated watcher.</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static FileWatch FileWatcherReset(FileWatch fileWatcher)
        {
            fileWatcher.Reset();
            return fileWatcher;
        }
    }
}
