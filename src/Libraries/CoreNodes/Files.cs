
using System.Collections;
using System.Drawing;

namespace DSCore.File
{
    public static class FileReader
    {
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
                    result.Insert(0, bmp.GetPixel(xParam, yParam));
                }
            }

            return result;
        }

        public static string ReadText(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
                return string.Empty;

            return System.IO.File.ReadAllText(filePath);
        }
    }
}
