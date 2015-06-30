using System.IO;
using Dynamo;
using Dynamo.Wpf.ViewModels.Core;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class GalleryTests : UnitTestBase
    {
        [Test]
        public void LoadGalleryContents()
        {
            //Test loading of xml gallery contents
            var galleryFilePath = Path.Combine(TestDirectory,
                @"core\gallery\NormalGalleryContents.xml");
            var contents = GalleryContents.Load(galleryFilePath).GalleryUiContents;
            Assert.IsTrue(contents.Count == 6);

            for (int i = 0; i < contents.Count; i++)
            {
                Assert.IsTrue(contents[i].ImagePath == string.Format("{0}.png", i + 1));
                Assert.IsTrue(contents[i].Header == string.Format("Library items are now draggable {0}", i + 1));
                Assert.IsTrue(contents[i].Body == string.Format("Give me some random texts inside {0}", i + 1));
            }
        }

        [Test]
        public void LoadEmptyGalleryContent()
        {
            //Test loading of an empty xml file
            var galleryFilePath = Path.Combine(TestDirectory,
                "EmptyGalleryContents.xml");
            var contents = GalleryContents.Load(galleryFilePath).GalleryUiContents;
            Assert.IsTrue(contents.Count == 0);
        }
    }
}
