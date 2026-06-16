using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Captures Background 3D Preview viewport content to PNG files.
    /// </summary>
    internal static class Watch3DImageExporter
    {
        /// <summary>
        /// Maximum pixel dimension for exported images. Prevents excessive memory use on
        /// high-DPI displays and large viewports when exporting complex geometry.
        /// </summary>
        internal const int DefaultMaxExportDimension = 4096;

        /// <summary>
        /// Renders the viewport to a bitmap at the requested pixel dimensions.
        /// </summary>
        /// <param name="view">The Helix viewport to capture.</param>
        /// <param name="targetPixelWidth">Target pixel width.</param>
        /// <param name="targetPixelHeight">Target pixel height.</param>
        /// <returns>The rendered bitmap.</returns>
        internal static BitmapSource RenderViewportBitmapAtSize(
            Viewport3DX view,
            int targetPixelWidth,
            int targetPixelHeight)
        {
            if (view?.RenderHost == null) throw new ArgumentNullException(nameof(view));
            if (targetPixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(targetPixelWidth));
            if (targetPixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(targetPixelHeight));

            view.InvalidateRender();

            var dpiScale = view.RenderHost.DpiScale;
            var originalWidth = view.RenderHost.ActualWidth;
            var originalHeight = view.RenderHost.ActualHeight;
            var captureWidth = Math.Max(1, (int)(targetPixelWidth / dpiScale));
            var captureHeight = Math.Max(1, (int)(targetPixelHeight / dpiScale));

            view.RenderHost.Resize(captureWidth, captureHeight);
            try
            {
                return view.RenderBitmap();
            }
            finally
            {
                if (originalWidth > 0 && originalHeight > 0)
                {
                    view.RenderHost.Resize((int)originalWidth, (int)originalHeight);
                }
            }
        }

        /// <summary>
        /// Renders the viewport and saves the result as a PNG file.
        /// </summary>
        /// <param name="view">The Helix viewport to capture.</param>
        /// <param name="path">Destination file path.</param>
        /// <param name="maxExportDimension">
        /// Maximum pixel width or height. Use 0 to capture at the current viewport resolution.
        /// </param>
        /// <returns>True when the image was saved successfully.</returns>
        internal static bool TrySaveViewportToPng(Viewport3DX view, string path, int maxExportDimension = DefaultMaxExportDimension)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            var bitmap = RenderViewportBitmap(view, maxExportDimension);
            SaveBitmapSourceAsPng(bitmap, path);
            return true;
        }

        /// <summary>
        /// Renders the viewport to a bitmap, optionally scaling down when the pixel size exceeds
        /// <paramref name="maxExportDimension"/>.
        /// </summary>
        /// <param name="view">The Helix viewport to capture.</param>
        /// <param name="maxExportDimension">
        /// Maximum pixel width or height. Use 0 to capture at the current viewport resolution.
        /// </param>
        /// <returns>The rendered bitmap.</returns>
        internal static BitmapSource RenderViewportBitmap(Viewport3DX view, int maxExportDimension = DefaultMaxExportDimension)
        {
            if (view?.RenderHost == null) throw new ArgumentNullException(nameof(view));

            view.InvalidateRender();

            var bitmap = view.RenderBitmap();
            return ScaleBitmapSourceToMaxDimension(bitmap, maxExportDimension);
        }

        private static BitmapSource ScaleBitmapSourceToMaxDimension(BitmapSource bitmapSource, int maxExportDimension)
        {
            if (bitmapSource == null) throw new ArgumentNullException(nameof(bitmapSource));
            if (maxExportDimension <= 0)
            {
                return bitmapSource;
            }

            var maxPixelDimension = Math.Max(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            if (maxPixelDimension <= maxExportDimension)
            {
                return bitmapSource;
            }

            var scale = maxExportDimension / (double)maxPixelDimension;
            var transformedBitmap = new TransformedBitmap(bitmapSource, new ScaleTransform(scale, scale));
            transformedBitmap.Freeze();
            return transformedBitmap;
        }

        /// <summary>
        /// Saves a bitmap source as a PNG file, replacing any existing file at the path.
        /// </summary>
        /// <param name="bitmapSource">The bitmap to save.</param>
        /// <param name="path">Destination file path.</param>
        internal static void SaveBitmapSourceAsPng(BitmapSource bitmapSource, string path)
        {
            if (bitmapSource == null) throw new ArgumentNullException(nameof(bitmapSource));
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var stream = File.Create(path))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
            }
        }
    }
}
