using System;
using System.CodeDom;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSCore.IO;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using Color = DSCore.Color;
using Directory = System.IO.Directory;
using File = DSCore.IO.File;
using Image = DSCore.IO.Image;

namespace Dynamo.Tests
{
    public class FileLibraryTests : UnitTestBase
    {
        #region FilePaths
        [Test, Category("UnitTests")]
        public void SimpleWrappers()
        {
            Assert.AreEqual(Path.Combine("test"), FilePath.Combine("test"));
            Assert.AreEqual(Path.Combine("test", "1"), FilePath.Combine("test", "1"));
            Assert.AreEqual(Path.Combine("test/", @"1\"), FilePath.Combine("test/", @"1\"));

            const string aFilePath = @"hello\there.txt";
            const string aFileName = "hello";

            Assert.AreEqual(Path.GetExtension(aFilePath), FilePath.Extension(aFilePath));
            Assert.AreEqual(Path.GetExtension(aFileName), FilePath.Extension(aFileName));

            Assert.AreEqual(
                Path.ChangeExtension(aFilePath, ".png"),
                FilePath.ChangeExtension(aFilePath, ".png"));
            Assert.AreEqual(
                Path.ChangeExtension(aFileName, ".txt"),
                FilePath.ChangeExtension(aFileName, ".txt"));

            Assert.AreEqual(Path.GetDirectoryName(aFilePath), FilePath.DirectoryName(aFilePath));
            Assert.AreEqual(Path.GetDirectoryName(aFileName), FilePath.DirectoryName(aFileName));

            Assert.AreEqual(Path.HasExtension(aFilePath), FilePath.HasExtension(aFilePath));
            Assert.AreEqual(Path.HasExtension(aFileName), FilePath.HasExtension(aFileName));
        }

        [Test, Category("UnitTests")]
        public void FilePath_FileName()
        {
            const string aFilePath = @"hello\there.txt";

            Assert.AreEqual(Path.GetFileName(aFilePath), FilePath.FileName(aFilePath));
            Assert.AreEqual(
                Path.GetFileNameWithoutExtension(aFilePath),
                FilePath.FileName(aFilePath, withExtension: false));
        }
        #endregion

        #region Files
        [Test, Category("UnitTests")]
        public void File_FromPath()
        {
            var fn = GetNewFileNameOnTempPath(".txt");
            Assert.AreEqual(new FileInfo(fn).FullName, File.FromPath(fn).FullName);
        }

        [Test, Category("UnitTests")]
        public void File_ReadText()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            System.IO.File.WriteAllText(fn, contents);
            var fnInfo = File.FromPath(fn);
            Assert.AreEqual(contents, File.ReadText(fnInfo));
        }

        [Test, Category("UnitTests")]
        public void File_Move()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));

            var dest = GetNewFileNameOnTempPath(".txt");
            var destInfo = File.FromPath(dest);
            File.Move(fn, dest);
            Assert.IsTrue(System.IO.File.Exists(dest));
            Assert.IsFalse(System.IO.File.Exists(fn));
            Assert.AreEqual(contents, File.ReadText(destInfo));
        }

        [Test, Category("UnitTests")]
        public void File_Delete()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));

            File.Delete(fn);
            Assert.IsFalse(System.IO.File.Exists(fn));
        }

        [Test, Category("UnitTests")]
        public void File_Copy()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            var fnInfo = File.FromPath(fn);
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));

            var dest = GetNewFileNameOnTempPath(".txt");
            var destInfo = File.FromPath(dest);
            File.Copy(fnInfo, dest);
            Assert.IsTrue(System.IO.File.Exists(dest));
            Assert.IsTrue(System.IO.File.Exists(fn));
            Assert.AreEqual(contents, File.ReadText(fnInfo));
            Assert.AreEqual(contents, File.ReadText(destInfo));
        }

        [Test, Category("UnitTests")]
        public void File_Exists()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            Assert.IsFalse(System.IO.File.Exists(fn));
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));
        }

        [Test, Category("UnitTests")]
        public void File_WriteText()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            Assert.IsFalse(System.IO.File.Exists(fn));
            File.WriteText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));
            Assert.AreEqual(contents, System.IO.File.ReadAllText(fn));
        }
        #endregion

        #region Directories
        [Test, Category("UnitTests")]
        public void Directory_FromPath()
        {
            var tmp = GetNewFileNameOnTempPath("");
            Assert.IsFalse(Directory.Exists(tmp));
            var info = DSCore.IO.Directory.FromPath(tmp);
            Assert.AreEqual(tmp, info.FullName);
            Assert.IsTrue(info.Exists);

            //Make again now that it already exists
            var info2 = DSCore.IO.Directory.FromPath(tmp);
            Assert.AreEqual(tmp, info2.FullName);
            Assert.IsTrue(info2.Exists);
        }

        [Test, Category("UnitTests")]
        public void Directory_Move()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            Directory.CreateDirectory(tmpSrc);
            const string fileName = @"temp.txt";
            File.WriteText(FilePath.Combine(tmpSrc, fileName), "test");

            var tmpDest = GetNewFileNameOnTempPath("");
            DSCore.IO.Directory.Move(tmpSrc, tmpDest);
            Assert.IsFalse(DSCore.IO.Directory.Exists(tmpSrc));
            Assert.IsTrue(DSCore.IO.Directory.Exists(tmpDest));

            var destFileName = FilePath.Combine(tmpDest, fileName);
            Assert.IsTrue(File.Exists(destFileName));
            Assert.AreEqual("test", File.ReadText(File.FromPath(destFileName)));
        }

        [Test, Category("UnitTests")]
        public void Directory_Copy()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            var tmpSrcInfo = DSCore.IO.Directory.FromPath(tmpSrc);
            const string fileName = @"temp.txt";
            File.WriteText(FilePath.Combine(tmpSrc, fileName), "test");

            var tmpDest = GetNewFileNameOnTempPath("");
            DSCore.IO.Directory.Copy(tmpSrcInfo, tmpDest);
            Assert.IsTrue(DSCore.IO.Directory.Exists(tmpSrc));
            Assert.IsTrue(DSCore.IO.Directory.Exists(tmpDest));

            var destFileName = FilePath.Combine(tmpDest, fileName);
            Assert.IsTrue(File.Exists(destFileName));
            Assert.AreEqual("test", File.ReadText(File.FromPath(destFileName)));
        }

        [Test, Category("UnitTests")]
        public void Directory_Delete()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            Directory.CreateDirectory(tmpSrc);
            const string fileName = @"temp.txt";
            File.WriteText(FilePath.Combine(tmpSrc, fileName), "test");

            Assert.Throws<IOException>(() => DSCore.IO.Directory.Delete(tmpSrc));
            DSCore.IO.Directory.Delete(tmpSrc, recursive: true);
            Assert.IsFalse(DSCore.IO.Directory.Exists(tmpSrc));

            var tmpSrc2 = GetNewFileNameOnTempPath("");
            Directory.CreateDirectory(tmpSrc2);
            DSCore.IO.Directory.Delete(tmpSrc2);
            Assert.IsFalse(DSCore.IO.Directory.Exists(tmpSrc2));
        }

        [Test, Category("UnitTests")]
        public void Directory_Contents()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            var tmpSrcInfo = DSCore.IO.Directory.FromPath(tmpSrc);
            const string fileName = @"temp.txt";
            var newFile = FilePath.Combine(tmpSrc, fileName);
            File.WriteText(newFile, "test");

            const string dirName = @"subDir";
            var newDir = FilePath.Combine(tmpSrc, dirName);
            Directory.CreateDirectory(newDir);

            var contents = DSCore.IO.Directory.Contents(tmpSrcInfo);
            Assert.AreEqual(new[] { newFile }, contents["files"]);
            Assert.AreEqual(new[] { newDir }, contents["directories"]);
        }

        [Test, Category("UnitTests")]
        public void Directory_Exists()
        {
            var tmp = GetNewFileNameOnTempPath("");
            Assert.IsFalse(DSCore.IO.Directory.Exists(tmp), "Directory hasn't been created yet.");
            Directory.CreateDirectory(tmp);
            Assert.IsTrue(DSCore.IO.Directory.Exists(tmp), "Directory has been created.");
        }
        #endregion

        #region Images
        private IEnumerable<string> GetTestImageFiles()
        {
            string imagePath = Path.Combine(GetTestDirectory(), @"core\files\images\testImage");
            return new[] { "png", "jpg", "bmp", "tif" }.Select(
                ext => Path.ChangeExtension(imagePath, ext));
        }
            
        [Test, Category("UnitTests")]
        public void Image_ReadFromFile()
        {
            foreach (var file in GetTestImageFiles())
            {
                Image.ReadFromFile(File.FromPath(file));
                Assert.DoesNotThrow(
                    () => 
                    {
                        using (System.IO.File.OpenRead(file)) { } //Check that it's not locked
                    },
                    "File is locked after being read!");
            }
        }

        [Test, Category("UnitTests")]
        public void Image_Pixels()
        {
            foreach (var file in GetTestImageFiles())
            {
                using (var bmp = new Bitmap(file))
                {
                    var allPixels = Image.Pixels(bmp);
                    Assert.AreEqual(bmp.Height, allPixels.Length);
                    Assert.AreEqual(bmp.Width, allPixels[0].Length);

                    const int samples = 40;
                    var samplePixels = Image.Pixels(bmp, xSamples: samples, ySamples: samples);
                    Assert.AreEqual(samples, samplePixels.Length);
                    Assert.AreEqual(samples, samplePixels[0].Length);
                }
            }
        }

        [Test, Category("UnitTests")]
        public void Image_FromPixels()
        {
            const int size = 50;
            
            var rectPixels =
                Enumerable.Repeat(Enumerable.Repeat(Color.ByColor(System.Drawing.Color.Blue), size).ToArray(), size)
                    .ToArray();

            var bmpFromRect = Image.FromPixels(rectPixels);
            Assert.AreEqual(size, bmpFromRect.Width);
            Assert.AreEqual(size, bmpFromRect.Height);
            
            var flatPixels = rectPixels.SelectMany(x => x).ToArray();

            var bmpFromFlat = Image.FromPixels(flatPixels, size, size);
            Assert.AreEqual(size, bmpFromFlat.Width);
            Assert.AreEqual(size, bmpFromFlat.Height);

            Assert.AreEqual(Image.Pixels(bmpFromRect), Image.Pixels(bmpFromFlat));
        }

        [Test, Category("UnitTests")]
        public void Image_Dimensions()
        {
            foreach (var file in GetTestImageFiles())
            {
                using (var bmp = new Bitmap(file))
                {
                    var dims = Image.Dimensions(bmp);
                    Assert.AreEqual(bmp.Width, dims["width"]);
                    Assert.AreEqual(bmp.Height, dims["height"]);
                }
            }
        }

        [Test, Category("UnitTests")]
        public void Image_Write()
        {
            var tmp = GetNewFileNameOnTempPath("png");
            using (var bmp = new Bitmap(Path.Combine(GetTestDirectory(), @"core\files\images\testImage.png")))
            {
                Image.WriteToFile(tmp, bmp);
                using (var newBmp = new Bitmap(tmp))
                {
                    Assert.AreEqual(Image.Pixels(bmp), Image.Pixels(newBmp));
                }
            }
        }
        #endregion

        #region CSV Files
        [Test, Category("UnitTests")]
        public void CSV_Read()
        {
            //Make a CSV file
            var data = Enumerable.Range(0, 10).Select(row => Enumerable.Range(0, 10).Select(col => row + col));
            var fn = GetNewFileNameOnTempPath(".csv");
            System.IO.File.WriteAllText(fn, string.Join("\n", data.Select(row => string.Join(",", row))));

            //Now read it
            var fnInfo = File.FromPath(fn);
            Assert.AreEqual(data, CSV.ReadFromFile(fnInfo));
        }

        [Test, Category("UnitTests")]
        public void CSV_Write()
        {
            //Write data to CSV
            var data =
                Enumerable.Range(0, 10)
                    .Select(row => Enumerable.Range(0, 10).Select(col => row + col).Cast<object>().ToArray())
                    .ToArray();
            var fn = GetNewFileNameOnTempPath(".csv");
            CSV.WriteToFile(fn, data);

            //Confirm it's correct
            Assert.AreEqual(data, CSV.ReadFromFile(File.FromPath(fn)));
        }
        #endregion
    }

    [TestFixture]
    class FileWritingTests : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void FileWriter()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\files\FileWriter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);

            var path = model.CurrentWorkspace.NodeFromWorkspace<StringInput>("84693240-90f3-45f3-9cb3-88207499f0bc");
            path.Value = GetNewFileNameOnTempPath(".txt");

            ViewModel.HomeSpace.Run();
        }

        [Test]
        public void ImageFileWriter()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\files\ImageFileWriter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

            var filename = model.CurrentWorkspace.NodeFromWorkspace<StringInput>("0aaae6d6-f84b-4e61-888b-14936343d80a");
            filename.Value = GetNewFileNameOnTempPath(".png");

            ViewModel.HomeSpace.Run();
        }
    }

    [TestFixture]
    public class ZeroTouchMigrationFileTests : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void TestZeroTouchMigrationFile()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\files\MigrationHintGetClosestPoint.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);

            ViewModel.HomeSpace.Run();

            AssertPreviewValue("8527c4f5-f8e1-491e-b446-64c495fa1848", 4.54606056566195);
        }
    }
}
