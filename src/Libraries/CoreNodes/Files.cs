
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
}
