using Autodesk.DesignScript.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;

namespace DSCore.IO
{
    /// <summary>
    ///     Methods for operating on strings representing file paths.
    /// </summary>
    public static class FilePath
    {
        /// <summary>
        ///     Combines multiple strings into a single file path.
        /// </summary>
        /// <param name="paths">String to combine into a path.</param>
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        ///     Gets the extension from a file path.
        /// </summary>
        /// <param name="path">Path to get extension of.</param>
        public static string Extension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        ///     Changes the extension of a file path.
        /// </summary>
        /// <param name="path">Path to change extension of.</param>
        /// <param name="newExtension">New extension.</param>
        public static string ChangeExtension(string path, string newExtension)
        {
            return Path.ChangeExtension(path, newExtension);
        }

        /// <summary>
        ///     Gets the directory name of a file path.
        /// </summary>
        /// <param name="path">Path to get directory information of.</param>
        public static string DirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        ///     Gets the file name of a file path.
        /// </summary>
        /// <param name="path">Path to get the file name of.</param>
        /// <param name="withExtension">Determines whether or not the extension is included in the result, defaults to true.</param>
        public static string FileName(string path, bool withExtension=true)
        {
            return withExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        ///     Determines whether or not a file path contains an extension.
        /// </summary>
        /// <param name="path">Path to check for an extension.</param>
        public static bool HasExtension(string path)
        {
            return Path.HasExtension(path);
        }
    }

    /// <summary>
    ///     Methods for working with Files.
    /// </summary>
    public static class File
    {
        [IsVisibleInDynamoLibrary(false)]
        public static FileInfo FromPath(string path)
        {
            return new FileInfo(path);
        }

        /// <summary>
        ///     Reads a text file and returns the contents as a string.
        /// </summary>
        /// <returns name="str">Contents of the text file.</returns>
        /// <search>read file,text,file</search>
        public static string ReadText(FileInfo file)
        {
            return System.IO.File.ReadAllText(file.FullName);
        }

        /// <summary>
        ///  Moves a specified file to a new location
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newPath"></param>
        /// <param name="overwrite"></param>
        public static void Move(string path, string newPath, bool overwrite = false)
        {
            if (overwrite && Exists(newPath))
                Delete(newPath);
            System.IO.File.Move(path, newPath);
        }

        /// <summary>
        ///   Deletes the specified file.
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            System.IO.File.Delete(path);
        }

        /// <summary>
        ///     Copies a file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destinationPath"></param>
        /// <param name="overwrite"></param>
        public static void Copy(FileInfo file, string destinationPath, bool overwrite = false)
        {
            file.CopyTo(destinationPath, overwrite);
        }

        /// <summary>
        ///     Determines if a file exists at the given path.
        /// </summary>
        /// <param name="path"></param>
        public static bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        /// <summary>
        ///     Write the text content to a file specified by the path
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="text">Text content</param>
        /// <search>write file,text,file</search>
        public static void WriteText(string filePath, string text)
        {
            System.IO.File.WriteAllText(filePath, text);
        }

        #region Obsolete Methods

        [Obsolete("Use File.FromPath -> Image.ReadFromFile -> Image.Pixels nodes instead.")]
        public static Color[] ReadImage(string path, int xSamples, int ySamples)
        {
            var info = FromPath(path);
            var image = Image.ReadFromFile(info);
            return Image.Pixels(image, xSamples, ySamples).SelectMany(x => x).ToArray();
        }

        [Obsolete("Use File.FromPath -> Image.ReadFromFile nodes instead.")]
        public static Bitmap LoadImageFromPath(string path)
        {
            return Image.ReadFromFile(FromPath(path));
        }

        [Obsolete("Use File.FromPath -> File.ReadText nodes instead.")]
        public static string ReadText(string path)
        {
            return ReadText(FromPath(path));
        }

        [Obsolete("Use Image.WriteToFile node instead.")]
        public static bool WriteImage(string filePath, string fileName, Bitmap image)
        {
            fileName = Path.ChangeExtension(fileName, "png");
            Image.WriteToFile(Path.Combine(filePath, fileName), image);
            return true;
        }

        [Obsolete("Use CSV.WriteToFile node instead.")]
        public static void ExportToCSV(string filePath, object[][] data)
        {
            CSV.WriteToFile(filePath, data);
        }

        #endregion
    }

    /// <summary>
    ///     Methods for working with Directories.
    /// </summary>
    public static class Directory
    {
        [IsVisibleInDynamoLibrary(false)]
        public static DirectoryInfo FromPath(string path)
        {
            if (!Exists(path))
                System.IO.Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }

        /// <summary>
        ///     Moves a directory to a new location.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newPath"></param>
        /// <param name="overwriteFiles"></param>
        public static void Move(string path, string newPath, bool overwriteFiles = false)
        {
            if (!Exists(newPath))
            {
                System.IO.Directory.Move(path, newPath);
                return;
            }

            var info = new DirectoryInfo(path);
            foreach (var file in info.EnumerateFiles())
            {
                var newFilePath = Path.Combine(newPath, file.Name);
                File.Move(file.FullName, newFilePath, overwriteFiles);
            }

            foreach (var dir in info.EnumerateDirectories())
            {
                var newDirPath = Path.Combine(newPath, dir.Name);
                Move(dir.FullName, newDirPath, overwriteFiles);
            }
        }

        /// <summary>
        ///     Copies a directory to a destination location.
        /// </summary>
        /// <param name="directory">Directory to copy.</param>
        /// <param name="destinationPath">Destination of the copy operation on disk.</param>
        /// <param name="overwriteFiles"></param>
        public static void Copy(DirectoryInfo directory, string destinationPath, bool overwriteFiles = false)
        {
            if (!Exists(destinationPath))
                System.IO.Directory.CreateDirectory(destinationPath);

            foreach (var file in directory.EnumerateFiles())
            {
                var newFilePath = Path.Combine(destinationPath, file.Name);
                File.Copy(file, newFilePath, overwriteFiles);
            }

            foreach (var dir in directory.EnumerateDirectories())
            {
                var newDirPath = Path.Combine(destinationPath, dir.Name);
                Copy(dir, newDirPath, overwriteFiles);
            }
        }

        /// <summary>
        ///     Deletes a directory.
        /// </summary>
        /// <param name="path">Path to a directory on disk.</param>
        /// <param name="recursive">Whether or not to delete all contents of the directory, defaults to false.</param>
        public static void Delete(string path, bool recursive = false)
        {
            System.IO.Directory.Delete(path, recursive);
        }

        /// <summary>
        ///     Gets all of the contents of a given directory.
        /// </summary>
        /// <param name="directory">Directory to get contents of.</param>
        /// <param name="searchString">Search string used to filter results. Defaults to "*.*" (displays all contents).</param>
        [MultiReturn("files", "directories")]
        public static Dictionary<string, IList> Contents(DirectoryInfo directory, string searchString = "*.*")
        {
            return new Dictionary<string, IList>
            {
                { "files", directory.EnumerateFiles(searchString).Select(x => x.FullName).ToList() },
                { "directories", directory.EnumerateDirectories(searchString).Select(x => x.FullName).ToList() }
            };
        }

        /// <summary>
        ///     Determines if a directory exists at the given path.
        /// </summary>
        /// <param name="path">Path to a directory on disk.</param>
        public static bool Exists(string path)
        {
            return System.IO.Directory.Exists(path);
        }
    }

    /// <summary>
    ///     Methods for operating on Image Bitmaps.
    /// </summary>
    public static class Image
    {
        /// <summary>
        ///     Loads the file as a bitmap.
        /// </summary>
        /// <param name="file">File object to load image from.</param>
        /// <returns name="bitmap">Bitmap</returns>
        public static Bitmap ReadFromFile(FileInfo file)
        {
            using (var fs = new FileStream(file.FullName, FileMode.Open))
                return new Bitmap(System.Drawing.Image.FromStream(fs));
        }

        /// <summary>
        ///     Reads an image file and returns the color values at the specified grid locations.
        /// </summary>
        /// <param name="image">The image to read.</param>
        /// <param name="xSamples">Number of sample grid points in the X direction.</param>
        /// <param name="ySamples">Number of sample grid points in the Y direction.</param>
        /// <returns name="colors">Colors at the specified grid points.</returns>
        /// <search>read,image,bitmap,png,jpg,jpeg</search>
        public static Color[][] Pixels(Bitmap image, int? xSamples=null, int? ySamples=null)
        {
            var numX = xSamples ?? image.Width;
            var numY = ySamples ?? image.Height;

            return
                Enumerable.Range(0, numY)
                    .Select(
                        y =>
                            Enumerable.Range(0, numX)
                                .Select(x => 
                                     Color.ByColor(image.GetPixel(x * (image.Width / numX), y * (image.Height / numY))))
                                .ToArray())
                    .ToArray();
        }

        /// <summary>
        ///     Constructs an image from a 2d list of pixels.
        /// </summary>
        /// <param name="colors">2d rectangular list of colors representing the pixels.</param>
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
        /// <param name="colors">List of colors representing the pixels.</param>
        /// <param name="width">Width of the new image, in pixels.</param>
        /// <param name="height">Height of the new image, in pixels.</param>
        public static Bitmap FromPixels(Color[] colors, int width, int height)
        {
            return FromPixelsHelper(colors, width, height);
        }

        private static Bitmap FromPixelsHelper(IEnumerable<Color> colors, int width, int height)
        {
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
            yield return color.Alpha;
            yield return color.Red;
            yield return color.Green;
            yield return color.Blue;
        }

        /// <summary>
        ///     Gets the width and height of an image.
        /// </summary>
        /// <param name="image">Image to get dimensions of.</param>
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
        /// <returns name="ok">It is successful or not.</returns>
        /// <search>write image,image,file</search>
        public static void WriteToFile(string path, Bitmap image)
        {
            image.Save(path);
        }
    }

    /// <summary>
    ///     Methods for working with CSV (comma-separated values) files.
    /// </summary>
    public static class CSV
    {
        /// <summary>
        ///     Write a list of lists into a file using a comma-separated values 
        ///     format. Outer list represents rows, inner lists represent columns. 
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="data">List of lists to write into CSV</param>
        /// <returns name="str">Contents of the text file.</returns>
        /// <search>write,text,file</search>
        public static void WriteToFile(string filePath, object[][] data)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var line in data)
                {
                    int count = 0;
                    foreach (var entry in line)
                    {
                        writer.Write(entry);
                        if (++count < line.Length)
                            writer.Write(", ");
                    }
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Reads a text file containing comma-separated values into a two dimensional list.
        /// Outer list represents rows, inner lists represent columns.
        /// </summary>
        /// <param name="file">File object to read from.</param>
        /// <returns>CSV contents of the given file.</returns>
        public static object[][] ReadFromFile(FileInfo file)
        {
            var csvDataQuery = from line in System.IO.File.ReadLines(file.FullName)
                               let elements = line.Split(',')
                               let count = elements.Length
                               let data = elements.Select(MarshalStringFromCSV)
                               select new { count, data };

            var csvDataList = csvDataQuery.ToList();
            var numCols = csvDataList.Max(row => row.count);
            var resultArray =
                csvDataList.Select(
                    row =>
                        row.data
                            .Concat(Enumerable.Repeat("", numCols - row.count))
                            .ToArray())
                    .ToArray();
            return resultArray;
        }

        private static object MarshalStringFromCSV(string elementSt)
        {
            if (string.IsNullOrWhiteSpace(elementSt))
                return null;

            if (elementSt.Contains('.'))
            {
                double dbl;
                if (double.TryParse(elementSt, NumberStyles.Float, CultureInfo.InvariantCulture, out dbl))
                {
                    return dbl;
                }
            }

            int i;
            if (int.TryParse(elementSt, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                return i;

            return elementSt;
        }
    }
}
