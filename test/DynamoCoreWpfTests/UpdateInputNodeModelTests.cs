using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class UpdateInputNodeModelTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");

            base.GetLibrariesToPreload(libraries);
        }
        //This test depends on the TestUINodes.dll - which depends on DynamoCoreWPF.
        [Test]
        public void TestUpdateInputNodeModel()
        {
            // DYN file contains nodemodel node that reads an image from hardcoded file name and returns a bitmap
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\updateInputNodeModel.dyn");
            RunModel(openPath);

            // Assert on one of the color values belonging to the image read
            var guid = "313bc594-8879-492b-aa99-8efc61c5707a";
            AssertPreviewValue(guid, 91);

            // Create backup of original file
            var originalFile = Path.Combine(TestDirectory, @"core\astbuilder\hardcoded_image_file.jpg");
            var backupFile = Path.Combine(TestDirectory, @"core\astbuilder\hardcoded_image_file - Copy.jpg");
            File.Copy(originalFile, backupFile);

            // Overwrite the file "harcoded_image_file.jpg" with the file "harcoded_image_file2.jpg"
            var path = Path.Combine(TestDirectory, @"core\astbuilder\hardcoded_image_file2.jpg");
            File.Copy(path, originalFile, true);

            // Force re-execute the nodemodel node 
            var guid2 = "a787e45f-f6ed-439f-9eb3-60008b4c8a72";
            var nodeModel = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(guid2);
            nodeModel.OnNodeModified(true);

            File.Delete(originalFile);
            // restore the file, "harcoded_image_file.jpg"
            File.Move(backupFile, originalFile);

            // Assert that the same color value has changed as the file is now pointing to a different image
            AssertPreviewValue(guid, 41);
        }
    }
}
