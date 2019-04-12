using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreNodeModels.Input;
using Dynamo.Events;
using Dynamo.Session;
using Moq;
using NUnit.Framework;
using Color = DSCore.Color;
using Directory = System.IO.Directory;
using FileSystem = DSCore.IO.FileSystem;
using Image = DSCore.IO.Image;

namespace Dynamo.Tests
{
    public class FileLibraryTests : UnitTestBase
    {
        private static void SetActiveSession(IExecutionSession value)
        {
            var type = typeof(ExecutionEvents);
            type.InvokeMember("ActiveSession", BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, new[] { value });
        }

        #region FilePaths
        [Test, Category("UnitTests")]
        public void SimpleWrappers()
        {
            Assert.AreEqual(Path.Combine("test"), FileSystem.CombinePath("test"));
            Assert.AreEqual(Path.Combine("test", "1"), FileSystem.CombinePath("test", "1"));
            Assert.AreEqual(Path.Combine("test/", @"1\"), FileSystem.CombinePath("test/", @"1\"));

            const string aFilePath = @"hello\there.txt";
            const string aFileName = "hello";

            Assert.AreEqual(Path.GetExtension(aFilePath), FileSystem.FileExtension(aFilePath));
            Assert.AreEqual(Path.GetExtension(aFileName), FileSystem.FileExtension(aFileName));

            Assert.AreEqual(
                Path.ChangeExtension(aFilePath, ".png"),
                FileSystem.ChangePathExtension(aFilePath, ".png"));
            Assert.AreEqual(
                Path.ChangeExtension(aFileName, ".txt"),
                FileSystem.ChangePathExtension(aFileName, ".txt"));

            Assert.AreEqual(Path.GetDirectoryName(aFilePath), FileSystem.DirectoryName(aFilePath));
            Assert.AreEqual(Path.GetDirectoryName(aFileName), FileSystem.DirectoryName(aFileName));

            Assert.AreEqual(Path.HasExtension(aFilePath), FileSystem.FileHasExtension(aFilePath));
            Assert.AreEqual(Path.HasExtension(aFileName), FileSystem.FileHasExtension(aFileName));
        }

        [Test, Category("UnitTests")]
        public void FilePath_FileName()
        {
            const string aFilePath = @"hello\there.txt";

            Assert.AreEqual(Path.GetFileName(aFilePath), FileSystem.FileName(aFilePath));
            Assert.AreEqual(
                Path.GetFileNameWithoutExtension(aFilePath),
                FileSystem.FileName(aFilePath, withExtension: false));
        }
        #endregion

        #region Files
        [Test, Category("UnitTests")]
        public void File_AbsolutePath_Exists()
        {
            Assert.IsNull(ExecutionEvents.ActiveSession);
            string wspath = Path.Combine(TestDirectory, @"core\files\dummy.dyn");

            var session = new Mock<IExecutionSession>();
            session.Setup(s => s.CurrentWorkspacePath).Returns(wspath);
            SetActiveSession(session.Object);
            var relativepath = @"images\testImage.jpg";
            var expectedpath = Path.Combine(TestDirectory, @"core\files", relativepath);
            Assert.AreEqual(expectedpath, FileSystem.AbsolutePath(relativepath));
            SetActiveSession(null);
        }

        [Test, Category("UnitTests")]
        public void File_AbsolutePath_RelativePathDontExist()
        {
            Assert.IsNull(ExecutionEvents.ActiveSession);
            string wspath = Path.Combine(TestDirectory, @"core\files\dummy.dyn");

            var session = new Mock<IExecutionSession>();
            session.Setup(s => s.CurrentWorkspacePath).Returns(wspath);
            SetActiveSession(session.Object);
            var relativepath = @"do not exist\no file.txt";
            var expectedpath = Path.Combine(TestDirectory, @"core\files", relativepath);
            Assert.AreEqual(expectedpath, FileSystem.AbsolutePath(relativepath));
            SetActiveSession(null);
        }

        [Test, Category("UnitTests")]
        public void File_AbsolutePath_WithValidHintPath_ReturnsHintPath()
        {
            Assert.IsNull(ExecutionEvents.ActiveSession);
            string wspath = Path.Combine(TestDirectory, @"core\files\dummy.dyn");

            var session = new Mock<IExecutionSession>();
            session.Setup(s => s.CurrentWorkspacePath).Returns(wspath);
            SetActiveSession(session.Object);
            var relativepath = @"excel\ascending.xlsx";
            var hintpath = Path.Combine(TestDirectory, "core", relativepath);
            Assert.AreEqual(hintpath, FileSystem.AbsolutePath(relativepath, hintpath));
            SetActiveSession(null);
        }

        [Test, Category("UnitTests")]
        public void File_AbsolutePath_WithFullPathInput_ReturnsInput()
        {
            Assert.IsNull(ExecutionEvents.ActiveSession);
            string wspath = Path.Combine(TestDirectory, @"core\files\dummy.dyn");
            var relativepath = @"excel\ascending.xlsx";
            var hintpath = Path.Combine(TestDirectory, "core", relativepath);
            Assert.AreEqual(wspath, FileSystem.AbsolutePath(wspath, hintpath));
        }

        [Test, Category("UnitTests")]
        public void File_AbsolutePath_WithoutSession_ReturnsHintPath()
        {
            Assert.IsNull(ExecutionEvents.ActiveSession);
            var relativepath = @"excel\ascending.xlsx";
            var hintpath = Path.Combine(TestDirectory, @"do not exist\no file.txt");
            Assert.AreEqual(hintpath, FileSystem.AbsolutePath(relativepath, hintpath));
        }

        [Test, Category("UnitTests")]
        public void File_FromPath()
        {
            var fn = GetNewFileNameOnTempPath(".txt");
            Assert.AreEqual(new FileInfo(fn).FullName, FileSystem.FileFromPath(fn).FullName);
        }

        [Test, Category("UnitTests")]
        public void File_ReadText()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            System.IO.File.WriteAllText(fn, contents);
            var fnInfo = FileSystem.FileFromPath(fn);
            Assert.AreEqual(contents, FileSystem.ReadText(fnInfo));
        }

        [Test, Category("UnitTests")]
        public void File_Move()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));

            var dest = GetNewFileNameOnTempPath(".txt");
            var destInfo = FileSystem.FileFromPath(dest);
            FileSystem.MoveFile(fn, dest);
            Assert.IsTrue(System.IO.File.Exists(dest));
            Assert.IsFalse(System.IO.File.Exists(fn));
            Assert.AreEqual(contents, FileSystem.ReadText(destInfo));
        }

        [Test, Category("UnitTests")]
        public void File_Delete()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));

            FileSystem.DeleteFile(fn);
            Assert.IsFalse(System.IO.File.Exists(fn));
        }

        [Test, Category("UnitTests")]
        public void File_Copy()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            var fnInfo = FileSystem.FileFromPath(fn);
            System.IO.File.WriteAllText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));

            var dest = GetNewFileNameOnTempPath(".txt");
            var destInfo = FileSystem.FileFromPath(dest);
            FileSystem.CopyFile(fnInfo, dest);
            Assert.IsTrue(System.IO.File.Exists(dest));
            Assert.IsTrue(System.IO.File.Exists(fn));
            Assert.AreEqual(contents, FileSystem.ReadText(fnInfo));
            Assert.AreEqual(contents, FileSystem.ReadText(destInfo));
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
            FileSystem.WriteText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));
            Assert.AreEqual(contents, System.IO.File.ReadAllText(fn));
        }

        [Test, Category("UnitTests")]
        public void File_AppendText()
        {
            const string contents = "test";
            var fn = GetNewFileNameOnTempPath(".txt");
            Assert.IsFalse(System.IO.File.Exists(fn));
            FileSystem.AppendText(fn, contents);
            Assert.IsTrue(System.IO.File.Exists(fn));
            FileSystem.AppendText(fn, contents);
            Assert.AreEqual(contents + contents, System.IO.File.ReadAllText(fn));
        }
        #endregion

        #region Directories
        [Test, Category("UnitTests")]
        public void Directory_FromPath()
        {
            var tmp = GetNewFileNameOnTempPath("");
            Assert.IsFalse(Directory.Exists(tmp));
            var info = FileSystem.DirectoryFromPath(tmp);
            Assert.AreEqual(tmp, info.FullName);
            Assert.IsTrue(info.Exists);

            //Make again now that it already exists
            var info2 = FileSystem.DirectoryFromPath(tmp);
            Assert.AreEqual(tmp, info2.FullName);
            Assert.IsTrue(info2.Exists);
        }

        [Test, Category("UnitTests")]
        public void Directory_Move()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            Directory.CreateDirectory(tmpSrc);
            const string fileName = @"temp.txt";
            FileSystem.WriteText(FileSystem.CombinePath(tmpSrc, fileName), "test");

            var tmpDest = GetNewFileNameOnTempPath("");
            FileSystem.MoveDirectory(tmpSrc, tmpDest);
            Assert.IsFalse(FileSystem.DirectoryExists(tmpSrc));
            Assert.IsTrue(FileSystem.DirectoryExists(tmpDest));

            var destFileName = FileSystem.CombinePath(tmpDest, fileName);
            Assert.IsTrue(FileSystem.FileExists(destFileName));
            Assert.AreEqual("test", FileSystem.ReadText(FileSystem.FileFromPath(destFileName)));
        }

        [Test, Category("UnitTests")]
        public void Directory_Copy()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            var tmpSrcInfo = FileSystem.DirectoryFromPath(tmpSrc);
            const string fileName = @"temp.txt";
            FileSystem.WriteText(FileSystem.CombinePath(tmpSrc, fileName), "test");

            var tmpDest = GetNewFileNameOnTempPath("");
            FileSystem.CopyDirectory(tmpSrcInfo, tmpDest);
            Assert.IsTrue(FileSystem.DirectoryExists(tmpSrc));
            Assert.IsTrue(FileSystem.DirectoryExists(tmpDest));

            var destFileName = FileSystem.CombinePath(tmpDest, fileName);
            Assert.IsTrue(FileSystem.FileExists(destFileName));
            Assert.AreEqual("test", FileSystem.ReadText(FileSystem.FileFromPath(destFileName)));
        }

        [Test, Category("UnitTests")]
        public void Directory_Delete()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            Directory.CreateDirectory(tmpSrc);
            const string fileName = @"temp.txt";
            FileSystem.WriteText(FileSystem.CombinePath(tmpSrc, fileName), "test");

            Assert.Throws<IOException>(() => FileSystem.DeleteDirectory(tmpSrc));
            FileSystem.DeleteDirectory(tmpSrc, recursive: true);
            Assert.IsFalse(FileSystem.DirectoryExists(tmpSrc));

            var tmpSrc2 = GetNewFileNameOnTempPath("");
            Directory.CreateDirectory(tmpSrc2);
            FileSystem.DeleteDirectory(tmpSrc2);
            Assert.IsFalse(FileSystem.DirectoryExists(tmpSrc2));
        }

        [Test, Category("UnitTests")]
        public void Directory_Contents()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            var tmpSrcInfo = FileSystem.DirectoryFromPath(tmpSrc);
            const string fileName = @"temp.txt";
            var newFile = FileSystem.CombinePath(tmpSrc, fileName);
            FileSystem.WriteText(newFile, "test");

            const string dirName = @"subDir";
            var newDir = FileSystem.CombinePath(tmpSrc, dirName);
            Directory.CreateDirectory(newDir);

            var contents = FileSystem.GetDirectoryContents(tmpSrcInfo);
            Assert.AreEqual(new[] { newFile }, contents["files"]);
            Assert.AreEqual(new[] { newDir }, contents["directories"]);
        }

        [Test, Category("UnitTests")]
        public void Directory_ContentsRecursive()
        {
            var tmpSrc = GetNewFileNameOnTempPath("");
            var tmpSrcInfo = FileSystem.DirectoryFromPath(tmpSrc);

            // make test file
            const string fileName = @"temp.txt";
            var newFile = FileSystem.CombinePath(tmpSrc, fileName);
            FileSystem.WriteText(newFile, "test");

            // make subdirectory
            const string dirName = @"subDir";
            var newDir = FileSystem.CombinePath(tmpSrc, dirName);
            Directory.CreateDirectory(newDir);

            // make another test file in subdirectory
            const string subdirFileName = @"tempSubdir.txt";
            var newSubdirFile = FileSystem.CombinePath(newDir, subdirFileName);
            FileSystem.WriteText(newSubdirFile, "testSubdir");

            var contents = FileSystem.GetDirectoryContents(tmpSrcInfo, "*.*", true);
            Assert.AreEqual(new[] { newFile, newSubdirFile }, contents["files"]);
            Assert.AreEqual(new[] { newDir }, contents["directories"]);
        }


        [Test, Category("UnitTests")]
        public void Directory_Exists()
        {
            var tmp = GetNewFileNameOnTempPath("");
            Assert.IsFalse(FileSystem.DirectoryExists(tmp), "Directory hasn't been created yet.");
            Directory.CreateDirectory(tmp);
            Assert.IsTrue(FileSystem.DirectoryExists(tmp), "Directory has been created.");
        }
        #endregion

        #region Images
        private IEnumerable<string> GetTestImageFiles()
        {
            string imagePath = Path.Combine(TestDirectory, @"core\files\images\testImage");
            return new[] { "png", "jpg", "bmp", "tif" }.Select(
                ext => Path.ChangeExtension(imagePath, ext));
        }

        [Test, Category("UnitTests")]
        public void Image_ReadFromFile()
        {
            foreach (var file in GetTestImageFiles())
            {
                Image.ReadFromFile(FileSystem.FileFromPath(file));
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
            using (var bmp = new Bitmap(Path.Combine(TestDirectory, @"core\files\images\testImage.png")))
            {
                Image.WriteToFile(tmp, bmp);
                using (var newBmp = new Bitmap(tmp))
                {
                    Assert.AreEqual(Image.Pixels(bmp), Image.Pixels(newBmp));
                }
            }
        }
        #endregion
    }

    [TestFixture]
    class FileWritingTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void FileWriter()
        {
            string openPath = Path.Combine(TestDirectory, @"core\files\FileWriter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var path = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<StringInput>("84693240-90f3-45f3-9cb3-88207499f0bc");
            path.Value = GetNewFileNameOnTempPath(".txt");

            BeginRun();
        }

        [Test]
        public void ImageFileWriter()
        {
            string openPath = Path.Combine(TestDirectory, @"core\files\ImageFileWriter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var filename = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<StringInput>("0aaae6d6-f84b-4e61-888b-14936343d80a");
            filename.Value = GetNewFileNameOnTempPath(".png");

            BeginRun();
        }
    }

    [TestFixture]
    public class ZeroTouchMigrationFileTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestZeroTouchMigrationFile()
        {
            string openPath = Path.Combine(TestDirectory, @"core\files\MigrationHintGetClosestPoint.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            BeginRun();

            AssertPreviewValue("8527c4f5-f8e1-491e-b446-64c495fa1848", 4.54606056566195);
        }
    }
}
