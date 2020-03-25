
//UNCOMMENT THIS DEFINE TO UPDATE THE REFERENCE IMAGES.
//#define UPDATEIMAGEDATA

using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Selection;
using DynamoCoreWpfTests.Utility;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace WpfVisualizationTests
{

    [TestFixture]
    public class HelixImageComparisonTests : HelixWatch3DViewModelTests
    {
        #region utilities

        /// <summary>
        /// TODO This updated function exists in Helix 2.12 - update when possible.
        /// Renders the viewport to a bitmap.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>A bitmap.</returns>
        private static BitmapSource DynamoRenderBitmap(
             Viewport3DX view, int width, int height)
        {
            var w = view.RenderHost.ActualWidth;
            var h = view.RenderHost.ActualHeight;
            view.RenderHost.Resize(width, height);
            var rtb = view.RenderBitmap();
            view.RenderHost.Resize((int)w, (int)h);
            return rtb;
        }
        private static void UpdateTestData(string pathToUpdate,BitmapSource imageFileSource)
        {
            SaveBitMapSourceAsPNG(pathToUpdate, imageFileSource);
        }

        private static void SaveBitMapSourceAsPNG(string filePath, BitmapSource bitmapSource)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }
        }

        private static System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        private string GenerateTestDataPathFromTest(string testname)
        {
             var fileName = testname+".png";
             string relativePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                string.Format(@"core\visualization\imageComparison\referenceImages\{0}", fileName));
            return relativePath;
        }

        private void RenderCurrentViewAndCompare(string testName)
        {
            DispatcherUtil.DoEvents();
            var path = GenerateTestDataPathFromTest(testName);
            var bitmapsource = DynamoRenderBitmap(BackgroundPreview.View,1024, 1024);
#if UPDATEIMAGEDATA
            UpdateTestData(path, bitmapsource);
#endif
            var refimage = new BitmapImage(new Uri(path));
            var refbitmap = BitmapFromSource(refimage);
            var newImage = BitmapFromSource(bitmapsource);

            compareImageColors(refbitmap, newImage);
        }

        //TODO consider something like:
        //(diferentPixelsCount / (mainImage.width* mainImage.height))*100
        //for a percent image diff.
        private static void compareImageColors(Bitmap image1,Bitmap image2)
        {
            Assert.AreEqual(image1.Width, image2.Width);
            Assert.AreEqual(image1.Height, image2.Height);
            Assert.AreEqual(image1.PixelFormat, image2.PixelFormat);

            for (var x = 0; x < image1.Width; x++)
            {
                for (var y = 0; y < image1.Height; y++)
                {
                    var expectedCol = image1.GetPixel(x, y);
                    var otherCol = image2.GetPixel(x, y);
                    Assert.AreEqual(expectedCol, otherCol,$"pixel {x}:{y} was not as expected");
                }
            }
        }

        #endregion

        #region meshes
        [Test]
        public void StandardMeshGeometryRender()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void StandardSelectedMeshGeometryRender()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            var node = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("spherenormal")).FirstOrDefault();
            DynamoSelection.Instance.Selection.Add(node);
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void StandardFrozenMeshGeometryRender()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            var node = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("spherenormal")).FirstOrDefault();
            node.IsFrozen = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void IsolateAllMeshGeometryRender()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            View.BackgroundPreview.ViewModel.IsolationMode = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void VertColorSelectedMeshGeometryRender()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            var node3 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("spherevertcolors")).FirstOrDefault();
            DynamoSelection.Instance.Selection.Add(node3);
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void VertColorFrozenMeshGeometryRender()
        {
            OpenVisualizationTest(@"imageComparison\spherecolors.dyn");
            RunCurrentModel();
            var node3 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("spherevertcolors")).FirstOrDefault();
            node3.IsFrozen = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }
        #endregion
        #region pointsAndLines
        [Test]
        public void points()
        {
            OpenVisualizationTest(@"imageComparison\pointcolors.dyn");
            RunCurrentModel();
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void pointsIsolated()
        {
            OpenVisualizationTest(@"imageComparison\pointcolors.dyn");
            RunCurrentModel();
            View.BackgroundPreview.ViewModel.IsolationMode = true;

            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void pointsSelected()
        {
            OpenVisualizationTest(@"imageComparison\pointcolors.dyn");
            RunCurrentModel();
            var node1 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("regularpoints")).FirstOrDefault();
            var node2 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("colorpoints")).FirstOrDefault();
            DynamoSelection.Instance.Selection.Add(node1);
            DynamoSelection.Instance.Selection.Add(node2);
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void pointsFrozen()
        {
            OpenVisualizationTest(@"imageComparison\pointcolors.dyn");
            RunCurrentModel();
            var node1 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("regularpoints")).FirstOrDefault();
            var node2 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("colorpoints")).FirstOrDefault();
            node1.IsFrozen = true;
            node2.IsFrozen = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void lines()
        {
            OpenVisualizationTest(@"imageComparison\linecolors.dyn");
            RunCurrentModel();
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void linesIsolated()
        {
            OpenVisualizationTest(@"imageComparison\linecolors.dyn");
            RunCurrentModel();
            View.BackgroundPreview.ViewModel.IsolationMode = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void linesSelected()
        {
            OpenVisualizationTest(@"imageComparison\linecolors.dyn");
            RunCurrentModel();
            var node1 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("coloredLines")).FirstOrDefault();
            var node2 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("regularLines")).FirstOrDefault();
            DynamoSelection.Instance.Selection.Add(node1);
            DynamoSelection.Instance.Selection.Add(node2);
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void linesFrozen()
        {
            OpenVisualizationTest(@"imageComparison\linecolors.dyn");
            RunCurrentModel();
            var node1 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("coloredLines")).FirstOrDefault();
            var node2 = ViewModel.CurrentSpace.Nodes.Where(x => x.Name.Contains("regularLines")).FirstOrDefault();
            node1.IsFrozen = true;
            node2.IsFrozen = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        #endregion

        #region SpecialRenderPackages
        [Test]
        public void directManipulator()
        {
            var pointOriginNode =
            new DSFunction(Model.LibraryServices.GetFunctionDescriptor("Point.ByCoordinates@double,double,double"));

            var command = new Dynamo.Models.DynamoModel.CreateNodeCommand(pointOriginNode, 0, 0, true, false);
            Model.ExecuteCommand(command);
            RunCurrentModel();
            DynamoSelection.Instance.Selection.Add(pointOriginNode);
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        #endregion

    }
}
