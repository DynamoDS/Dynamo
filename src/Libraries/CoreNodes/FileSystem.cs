using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Autodesk.DesignScript.Runtime;
using Dynamo.Events;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Session;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;

namespace DSCore.IO
{
    /// <summary>
    ///     Methods for working with Files.
    /// </summary>
    public static class FileSystem
    {
        #region file methods

        /// <summary>
        /// Returns absolute path from the given path. If the given path is 
        /// relative path then it is resolved with respect to the current 
        /// workspace. If file doesn't exist at the relative path but exists
        /// at the given hintPath then hintPath is returned.
        /// </summary>
        /// <param name="path">Relative path or full path</param>
        /// <param name="hintPath">Last resolved path</param>
        /// <returns>Absolute path</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static string AbsolutePath(string path, string hintPath = null)
        {
            //If the path is absolute path no need to transform.
            if (Path.IsPathRooted(path)) return path;

            var session = Dynamo.Events.ExecutionEvents.ActiveSession;
            if (session != null && !string.IsNullOrEmpty(session.CurrentWorkspacePath))
            {
                var parent = Path.GetDirectoryName(session.CurrentWorkspacePath);
                var filepath = Path.Combine(parent, path);
                //If hint path is null or file exists at this location return the computed path
                //If hint path doesn't exist then the relative path might be for write operation.
                if (FileSystem.FileExists(filepath) || string.IsNullOrEmpty(hintPath) || !FileSystem.FileExists(hintPath))
                    return Path.GetFullPath(filepath);
            }

            return string.IsNullOrEmpty(hintPath) ? path : hintPath;
        }

        /// <summary>
        /// Creates File object from given file path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static FileInfo FileFromPath(string path)
        {
            return new FileInfo(AbsolutePath(path));
        }

        /// <summary>
        ///     Reads a text file and returns the contents as a string.
        /// </summary>
        /// <param name="file"> File object to read text from</param>
        /// <returns name="string">Contents of the text file.</returns>
        /// <search>read file,text,file</search>
        public static string ReadText(FileInfo file)
        {
            return System.IO.File.ReadAllText(file.FullName);
        }

        /// <summary>
        ///  Moves a specified file to a new location
        /// </summary>
        /// <param name="path">String representation of existing path</param>
        /// <param name="newPath">String representation of new path</param>
        /// <param name="overwrite">Toggle to overwrite existing files</param>
        /// <returns name="void">Node performs a task, doesn’t produce an output </returns>
        public static void MoveFile(string path, string newPath, bool overwrite = false)
        {
            if (overwrite && FileExists(newPath))
                DeleteFile(newPath);
            System.IO.File.Move(path, newPath);
        }

        /// <summary>
        ///   Deletes the specified file.
        /// </summary>
        /// <param name="path">File path to delete</param>
        /// <returns name="void">Node performs a task, doesn’t produce an output </returns>
        public static void DeleteFile(string path)
        {
            System.IO.File.Delete(path);
        }

        /// <summary>
        ///     Copies a file.
        /// </summary>
        /// <param name="file">File object to copy</param>
        /// <param name="destinationPath">String representation of destination file path</param>
        /// <param name="overwrite">Toggle to overwrite existing files</param>
        /// <returns name="bool">Node performs a task, return true of copy action succeed.</returns>
        public static bool CopyFile(FileInfo file, string destinationPath, bool overwrite = false)
        {
            try
            {
                if (Path.GetDirectoryName(destinationPath) != string.Empty) 
                {
                    file.CopyTo(destinationPath, overwrite);
                }
                else
                {
                    throw new FileNotFoundException(Properties.Resources.InvalidDestinationPathErrorMessage, destinationPath);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return true;
            
        }

        /// <summary>
        ///     Determines if a file exists at the given path.
        /// </summary>
        /// <param name="path">String representing a file path</param>
        /// <returns name="bool">True if file exists, false if it doesn't</returns>
        /// <search>filepath</search>
        public static bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        /// <summary>
        ///     Write the text content to a file specified by the path
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="text">Text content</param>
        /// <returns name="void">No output</returns>
        /// <search>write file,text,file,filepath</search>
        public static void WriteText(string filePath, string text)
        {
            var fullpath = AbsolutePath(filePath);
            System.IO.File.WriteAllText(fullpath, text);
        }

        /// <summary>
        /// Append the text content to a file specified by the path
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="text">Text content</param>
        /// <returns name="void">Node performs a task, doesn’t produce an output </returns>
        /// <search>append file,write file,text,file,filepath</search>
        public static void AppendText(string filePath, string text)
        {
            var fullpath = AbsolutePath(filePath);
            System.IO.File.AppendAllText(fullpath, text);
        }

        /// <summary>
        ///     Combines multiple strings into a single file path.
        /// </summary>
        /// <param name="strings">Strings to combine into a path</param>
        /// <returns name="string">Combined file path</returns>
        public static string CombinePath(params string[] strings)
        {
            return Path.Combine(strings);
        }

        /// <summary>
        /// Returns the extension from a file path.
        /// </summary>
        /// <param name="path">Path to get extension of</param>
        /// <returns name="string">Extension of file</returns>
        public static string FileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        ///     Changes the extension of a file path.
        /// </summary>
        /// <param name="path">Path to change extension of</param>
        /// <param name="newExtension">String representation of new extension</param>
        /// <returns name="string">File path with changed extension</returns>
        public static string ChangePathExtension(string path, string newExtension)
        {
            return Path.ChangeExtension(path, newExtension);
        }

        /// <summary>
        /// Returns the directory name of a file path.
        /// </summary>
        /// <param name="path">Path to get directory information of</param>
        /// <returns name="string">Directory name of file path</returns>
        /// <search>directorypath</search>
        public static string DirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Returns the file name of a file path.
        /// </summary>
        /// <param name="path">Path to get the file name of</param>
        /// <param name="withExtension">Toggle to include extension in result</param>
        /// <returns name="string">File name from file path</returns>
        public static string FileName(string path, bool withExtension = true)
        {
            return withExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        ///     Determines whether or not a file path contains an extension.
        /// </summary>
        /// <param name="path">Path to check for an extension</param>
        /// <returns name="bool">True if file path contains extension, false if it doesn't</returns>
        public static bool FileHasExtension(string path)
        {
            return Path.HasExtension(path);
        }

        /// <summary>
        /// Will return a list of files and directories that are contained within a given directory. An optional searchString can be used to filter the results.
        /// </summary>
        /// <param name="directory">Directory to get contents of</param>
        /// <param name="searchString">Search string used to filter results</param>
        /// <param name="includeSubdirectories">Set to true to include files and folders in subdirectories (recursive) or set to false to include results from top-level of given directory only.</param>
        /// <returns name="files">Resulting files from query</returns>
        /// <returns name="directories">Resulting directories from query</returns>
        [MultiReturn("files", "directories")]
        public static Dictionary<string, IList> GetDirectoryContents(DirectoryInfo directory, string searchString = "*.*", bool includeSubdirectories = false)
        {
            var searchOptions = SearchOption.TopDirectoryOnly;
            if (includeSubdirectories == true) searchOptions = SearchOption.AllDirectories;

            return new Dictionary<string, IList>
            {
                { "files", directory.EnumerateFiles(searchString, searchOptions).Select(x => x.FullName).ToList() },
                { "directories", directory.EnumerateDirectories(searchString, searchOptions).Select(x => x.FullName).ToList() }
            };
        }
        #endregion

        #region directory methods
        /// <summary>
        ///     Copies a directory to a destination location.
        /// </summary>
        /// <param name="directory">Directory to copy</param>
        /// <param name="destinationPath">Destination of the copy operation on disk</param>
        /// <param name="overwriteFiles">Toggle to overwrite existing directory</param>
        /// <returns name="void">Node performs a task, doesn’t produce an output </returns>
        public static void CopyDirectory(DirectoryInfo directory, string destinationPath, bool overwriteFiles = false)
        {
            if (!FileExists(destinationPath))
                System.IO.Directory.CreateDirectory(destinationPath);

            foreach (var file in directory.EnumerateFiles())
            {
                var newFilePath = Path.Combine(destinationPath, file.Name);
                CopyFile(file, newFilePath, overwriteFiles);
            }

            foreach (var dir in directory.EnumerateDirectories())
            {
                var newDirPath = Path.Combine(destinationPath, dir.Name);
                CopyDirectory(dir, newDirPath, overwriteFiles);
            }
        }

        /// <summary>
        ///     Deletes a directory.
        /// </summary>
        /// <param name="path">Path to a directory on disk</param>
        /// <param name="recursive">Whether or not to delete all contents of the directory, defaults to false.</param>
        /// <returns name="void">Node performs a task, doesn’t produce an output </returns>
        public static void DeleteDirectory(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);
        }

        /// <summary>
        ///     Determines if a directory exists at the given path.
        /// </summary>
        /// <param name="path">Path to a directory on disk</param>
        /// <returns name="bool">True if directory exists, false if it doesn’t</returns>
        /// <search>directorypath</search>
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
        [IsVisibleInDynamoLibrary(false)]
        public static DirectoryInfo DirectoryFromPath(string path)
        {
            if (!DirectoryExists(path))
                Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }

        /// <summary>
        ///     Moves a directory to a new location.
        /// </summary>
        /// <param name="path">String representation of existing path</param>
        /// <param name="newPath">String representation of new path</param>
        /// <param name="overwriteFiles">Toggle to overwrite existing files</param>
        /// <returns name="void">Node performs a task, doesn’t produce an output </returns>
        public static void MoveDirectory(string path, string newPath, bool overwriteFiles = false)
        {
            if (!DirectoryExists(newPath))
            {
                Directory.Move(path, newPath);
                return;
            }

            var info = new DirectoryInfo(path);
            foreach (var file in info.EnumerateFiles())
            {
                var newFilePath = Path.Combine(newPath, file.Name);
                MoveFile(file.FullName, newFilePath, overwriteFiles);
            }

            foreach (var dir in info.EnumerateDirectories())
            {
                var newDirPath = Path.Combine(newPath, dir.Name);
                MoveDirectory(dir.FullName, newDirPath, overwriteFiles);
            }
        }
        #endregion

        #region Obsolete Methods

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        [NodeObsolete("ReadImageObsolete", typeof(Properties.Resources))]
        public static Color[] ReadImage(string path, int xSamples, int ySamples)
        {
            var info = FileFromPath(path);
            var image = Image.ReadFromFile(info);
            return Image.Pixels(image, xSamples, ySamples).SelectMany(x => x).ToArray();
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        [NodeObsolete("LoadImageFromPathObsolete", typeof(Properties.Resources))]
        public static Bitmap LoadImageFromPath(string path)
        {
            return Image.ReadFromFile(FileFromPath(path));
        }

        [NodeObsolete("ReadTextObsolete", typeof(Properties.Resources))]
        public static string ReadText(string path)
        {
            return ReadText(FileFromPath(path));
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        [NodeObsolete("WriteImageObsolete", typeof(Properties.Resources))]
        public static bool WriteImage(string filePath, string fileName, Bitmap image)
        {
            fileName = Path.ChangeExtension(fileName, "png");
            Image.WriteToFile(Path.Combine(filePath, fileName), image);
            return true;
        }

        [NodeObsolete("ExportToCSVObsolete", typeof(Properties.Resources))]
        public static bool ExportToCSV(string filePath, object[][] data)
        {
            return false;
        }
        #endregion
    }

    /// <summary>
    ///     Methods for operating on Image Bitmaps.
    /// </summary>
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static class Image
    {
        /// <summary>
        ///     Loads the file as a bitmap.
        /// </summary>
        /// <param name="file">File object to load image from</param>
        /// <returns name="image">Image object from file</returns>
        public static Bitmap ReadFromFile(FileInfo file)
        {
            using (var fs = new FileStream(file.FullName, FileMode.Open))
                return new Bitmap(System.Drawing.Image.FromStream(fs));
        }

        /// <summary>
        ///     Reads an image file and returns the color values at the specified grid locations.
        /// </summary>
        /// <param name="image">Image object to get pixel colors from</param>
        /// <param name="xSamples">Number of sample grid points in the X direction.</param>
        /// <param name="ySamples">Number of sample grid points in the Y direction.</param>
        /// <returns name="colors">Colors at the specified grid points</returns>
        /// <search>read,image,bitmap,png,jpg,jpeg</search>
        public static Color[][] Pixels(Bitmap image, int? xSamples = null, int? ySamples = null)
        {
            var numX = xSamples ?? image.Width;
            var numY = ySamples ?? image.Height;

            return
                Enumerable.Range(0, numY)
                    .Select(
                        y =>
                            Enumerable.Range(0, numX)
                                .Select(x =>
                                     Color.ByColor(image.GetPixel(
                                     (int)(x * ((float)(image.Width-1) / (numX-1))),
                                     (int)(y * ((float)(image.Height-1) / (numY-1))))
                                     ))
                                .ToArray())
                    .ToArray();
        }

        /// <summary>
        ///     Constructs an image from a 2d list of pixels.
        /// </summary>
        /// <param name="colors">2d rectangular list of colors representing the pixels</param>
        /// <returns name="image">Image from 2d list of pixels</returns>
        public static Bitmap FromPixels(Color[][] colors)
        {
            var height = colors.Length;
            var width = colors[0].Length;

            var rgbVals = colors.SelectMany(row => row);

            return FromPixelsHelper(rgbVals, width, height);
        }

        /// <summary>
        ///     Constructs an image from a flat list of pixels, a width, and a height.
        /// </summary>
        /// <param name="colors">List of colors representing the pixels</param>
        /// <param name="width">Width of the new image, in pixels</param>
        /// <param name="height">Height of the new image, in pixels</param>
        /// <returns name="image">Image from list of pixels</returns>
        public static Bitmap FromPixels(Color[] colors, int width, int height)
        {
            return FromPixelsHelper(colors, width, height);
        }

        private static Bitmap FromPixelsHelper(IEnumerable<Color> colors, int width, int height)
        {
            //if the color data is larger than will fit in the bitmap buffer
            //then this can cause an access violation - so we'll throw an exception.
            if(colors?.Count() > width * height)
            {
                throw new ArgumentOutOfRangeException("colors", Properties.Resources.BitmapOverflowError);
            }
            
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            foreach (var colorByte in
                colors.SelectMany(PixelsFromColor).Select((pixel, idx) => new { color = pixel, idx }))
            {
                Marshal.WriteByte(data.Scan0, colorByte.idx, colorByte.color);
            }

            bitmap.UnlockBits(data);

            return bitmap;
        }

        private static IEnumerable<byte> PixelsFromColor(Color color)
        {
            yield return color.Blue;
            yield return color.Green;
            yield return color.Red;
            yield return color.Alpha;
        }

        /// <summary>
        /// Returns the width and height of an image.
        /// </summary>
        /// <param name="image">Image to get dimensions of.</param>
        /// <returns name="width"> width of image in pixels</returns>
        /// <returns name="height"> height of image in pixels </returns>
        [MultiReturn("width", "height")]
        public static Dictionary<string, int> Dimensions(Bitmap image)
        {
            return new Dictionary<string, int>
            {
                { "width", image.Width },
                { "height", image.Height }
            };
        }

        /// <summary>
        ///     Write the image to a path, given the specified file name.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="image">The image to write</param>
        /// <returns name="image">Created image object</returns>
        /// <search>write image,image,file,filepath</search>
        public static Bitmap WriteToFile(string path, Bitmap image)
        {
            image.Save(FileSystem.AbsolutePath(path));

            return image;
        }
    }
}
