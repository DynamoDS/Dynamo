
using System.Collections;
using System.Drawing;
using Autodesk.DesignScript.Runtime;

namespace DSCore.File
{
    [IsVisibleInDynamoLibrary(false)]
    public static class FileReader
    {
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
                    int xParam = x * (int)(bmp.Width / numX);
                    int yParam = y * (int)(bmp.Height / numY);

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
        /// <search>read,text,file</search>
        public static string ReadText(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
                return string.Empty;

            return System.IO.File.ReadAllText(filePath);
        }
    }
}
