using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CoreNodeModelsWpf.Charts.Utilities
{
    public static class Export
    {
        public static void ToPng(UserControl control)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)control.ActualWidth, (int)control.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(control);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));
            MemoryStream stream = new MemoryStream();
            png.Save(stream);
            var image = System.Drawing.Image.FromStream(stream);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "NodeModelChart"; // Default file name
            dialog.DefaultExt = ".png"; // Default file extension
            dialog.Filter = "Image files (*.png) | *.png"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dialog.FileName;
                image.Save(filename, ImageFormat.Png);
            }
        }
    }
}
