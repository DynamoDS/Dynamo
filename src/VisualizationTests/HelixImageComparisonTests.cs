
//UNCOMMENT THIS DEFINE TO UPDATE THE REFERENCE IMAGES.
//#define UPDATEIMAGEDATA
//UNCOMMENT TO SAVE DEBUG IMAGES.
//#define SAVEDEBUGIMAGES
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Selection;
using DynamoCoreWpfTests.Utility;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;

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
        private static void UpdateTestData(string pathToUpdate, BitmapSource imageFileSource)
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

        private string GenerateImagePathFromTest(string testname, bool debug = false)
        {
            var debugstring = debug ? "debug" : string.Empty;
            var fileName = testname + debugstring + ".png";
            string relativePath = Path.Combine(
               GetTestDirectory(ExecutingDirectory),
               string.Format(@"core\visualization\imageComparison\referenceImages\{0}", fileName));
            return relativePath;
        }

        private void RenderCurrentViewAndCompare(string testName)
        {
            DispatcherUtil.DoEvents();
            var path = GenerateImagePathFromTest(testName);
            var bitmapsource = DynamoRenderBitmap(BackgroundPreview.View, 1024, 1024);
#if UPDATEIMAGEDATA
            UpdateTestData(path, bitmapsource);
#endif
            var refimage = new BitmapImage(new Uri(path));
            var refbitmap = BitmapFromSource(refimage);
            var newImage = BitmapFromSource(bitmapsource);

#if SAVEDEBUGIMAGES
            var debugPath = GenerateImagePathFromTest(testName,true);
            SaveBitMapSourceAsPNG(debugPath, bitmapsource);
#endif

            CompareImageColors(refbitmap, newImage);
        }
        private static void CompareImageColors(Bitmap expectedImage, Bitmap actualImage)
        {
            Assert.AreEqual(expectedImage.Width, actualImage.Width);
            Assert.AreEqual(expectedImage.Height, actualImage.Height);
            Assert.AreEqual(expectedImage.PixelFormat, actualImage.PixelFormat);

            //x,y,expected,result
            var differences = new List<Tuple<int, int, Color, Color>>();

            for (var x = 0; x < expectedImage.Width; x++)
            {
                for (var y = 0; y < expectedImage.Height; y++)
                {
                    var expectedCol = expectedImage.GetPixel(x, y);
                    var otherCol = actualImage.GetPixel(x, y);
                    if ((ColorDistance(expectedCol, otherCol) > 128))
                    {
                        differences.Add(Tuple.Create(x, y, expectedCol, otherCol));
                        // this can be painfully slow, but is useful during debug - uncomment for more info.
                        // Console.WriteLine($"{expectedCol}, {otherCol}, pixel {x}:{y} was not in expected range");
                    }
                }
            }
            var diff = CalculatePercentDiff(expectedImage, differences);
            Console.WriteLine($"% difference by out of range pixels was {(diff * 100).ToString("N" + 3)}%");
            Assert.LessOrEqual(diff, .01);

        }

        /// <summary>
        /// Euclidean distance between colors.
        /// Does not use the alpha channel.
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        private static double ColorDistance(Color color1, Color color2)
        {
            var r1 = color1.R;
            var g1 = color1.G;
            var b1 = color1.B;

            var r2 = color2.R;
            var g2 = color2.G;
            var b2 = color2.B;
            return Math.Sqrt(Math.Pow((r2 - r1), 2) + Math.Pow((g2 - g1), 2) + Math.Pow((b2 - b1), 2));
        }

        private static double CalculatePercentDiff(Bitmap image1, List<Tuple<int, int, Color, Color>> differences)
        {
            //TODO should we remove background pixels from this calculation via color?
            // or other filtering techniques for more precision
            var totalPixels = image1.Width * image1.Height;
            return (double)differences.Count / (double)totalPixels;
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
        public void Points()
        {
            OpenVisualizationTest(@"imageComparison\pointcolors.dyn");
            RunCurrentModel();
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void PointsIsolated()
        {
            OpenVisualizationTest(@"imageComparison\pointcolors.dyn");
            RunCurrentModel();
            View.BackgroundPreview.ViewModel.IsolationMode = true;

            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void PointsSelected()
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
        public void PointsFrozen()
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
        public void Lines()
        {
            OpenVisualizationTest(@"imageComparison\linecolors.dyn");
            RunCurrentModel();
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void LinesIsolated()
        {
            OpenVisualizationTest(@"imageComparison\linecolors.dyn");
            RunCurrentModel();
            View.BackgroundPreview.ViewModel.IsolationMode = true;
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        [Test]
        public void LinesSelected()
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
        public void LinesFrozen()
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

        #region surfaces
        [Test]
        public void WavySurfaceRender()
        {
            OpenVisualizationTest(@"imageComparison\wavysurface.dyn");
            RunCurrentModel();
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }
        #endregion

        #region SpecialRenderPackages
        [Test]
        public void DirectManipulator()
        {
            var pointOriginNode =
            new DSFunction(Model.LibraryServices.GetFunctionDescriptor("Point.ByCoordinates@double,double,double"));

            var command = new Dynamo.Models.DynamoModel.CreateNodeCommand(pointOriginNode, 0, 0, true, false);
            Model.ExecuteCommand(command);
            View.BackgroundPreview.ViewModel.NavigationKeyIsDown = true;
            RunCurrentModel();
            DynamoSelection.Instance.Selection.Add(pointOriginNode);
            RenderCurrentViewAndCompare(MethodBase.GetCurrentMethod().Name);
        }

        #endregion

    }
}
