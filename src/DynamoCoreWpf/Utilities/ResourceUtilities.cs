using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Dynamo.Utilities
{
    public static class ResourceUtilities
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ConvertToImageSource(System.Drawing.Bitmap bmp)
        {
            if (bmp == null)
                return null;

            ImageSource imageSource;
            var hbitmap = bmp.GetHbitmap();
            try
            {
                imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
            finally
            {
                DeleteObject(hbitmap);
            }

            return imageSource;
        }
    }
}
