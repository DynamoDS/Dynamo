using System.Collections;
using System.Drawing;
using System.IO;

namespace DSCore
{
    public static class Image
    {
        public static IList ReadImageFile(string path, int numX, int numY)
        {
            IList result = List.Empty;
            if (File.Exists(path))
            {
                var bmp = new Bitmap(path);

                for (int y = 0; y < numY; y++)
                {
                    for (int x = 0; x < numX; x++)
                    {
                        System.Drawing.Color pixelColor =
                            bmp.GetPixel(x * (int)(bmp.Width / numX), y * (int)(bmp.Height / numY));
                        result.Insert(0, pixelColor); // Insert new color at the front.
                    }
                }

                return result;
            }
            return List.Empty;
        }
    }
}
