using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class BackgroundPreviewExportTests : VisualizationTest
    {
        [Test]
        public void WhenBackgroundPreviewIsRenderedThenExportCreatesPngFile()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            ViewModel.BackgroundPreviewViewModel.Active = true;
            DispatcherUtil.DoEvents();

            Assert.That(View.BackgroundPreview, Is.Not.Null);
            Assert.That(View.BackgroundPreview.View, Is.Not.Null);

            var exportPath = Path.Combine(Path.GetTempPath(), $"DynamoBackgroundExportTest_{Guid.NewGuid():N}.png");

            try
            {
                Watch3DImageExporter.TrySaveViewportToPng(
                    View.BackgroundPreview.View,
                    exportPath,
                    maxExportDimension: 1024);

                Assert.That(File.Exists(exportPath), Is.True);
                Assert.That(new FileInfo(exportPath).Length, Is.GreaterThan(0));
            }
            finally
            {
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
            }
        }

        [Test]
        public void WhenBackgroundPreviewExportIsScaledThenExportCreatesNonBlankPngFile()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            ViewModel.BackgroundPreviewViewModel.Active = true;
            DispatcherUtil.DoEvents();

            Assert.That(View.BackgroundPreview, Is.Not.Null);
            Assert.That(View.BackgroundPreview.View, Is.Not.Null);

            var exportPath = Path.Combine(Path.GetTempPath(), $"DynamoBackgroundExportScaledTest_{Guid.NewGuid():N}.png");

            try
            {
                Watch3DImageExporter.TrySaveViewportToPng(
                    View.BackgroundPreview.View,
                    exportPath,
                    maxExportDimension: 128);

                Assert.That(File.Exists(exportPath), Is.True);
                Assert.That(PngHasImageVariation(exportPath), Is.True);
            }
            finally
            {
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
            }
        }

        [Test]
        public void WhenBackgroundPreviewIsInactiveThenExportIsRejected()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            ViewModel.BackgroundPreviewViewModel.Active = false;

            var args = new ImageSaveEventArgs(Path.Combine(Path.GetTempPath(), "unused.png"));
            ViewModel.OnRequestSave3DImage(ViewModel, args);

            Assert.That(args.Success, Is.False);
        }

        private static bool PngHasImageVariation(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                BitmapSource bitmapSource = frame.Format == PixelFormats.Bgra32
                    ? frame
                    : new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);

                var stride = bitmapSource.PixelWidth * 4;
                var pixels = new byte[stride * bitmapSource.PixelHeight];
                bitmapSource.CopyPixels(pixels, stride, 0);

                for (var i = 4; i < pixels.Length; i += 4)
                {
                    if (pixels[i] != pixels[0] ||
                        pixels[i + 1] != pixels[1] ||
                        pixels[i + 2] != pixels[2] ||
                        pixels[i + 3] != pixels[3])
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
