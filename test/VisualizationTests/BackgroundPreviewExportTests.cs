using System;
using System.IO;
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
        public void WhenBackgroundPreviewIsInactiveThenExportIsRejected()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            ViewModel.BackgroundPreviewViewModel.Active = false;

            var args = new ImageSaveEventArgs(Path.Combine(Path.GetTempPath(), "unused.png"));
            ViewModel.OnRequestSave3DImage(ViewModel, args);

            Assert.That(args.Success, Is.False);
        }
    }
}
