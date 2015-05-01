using System.ComponentModel;
using Dynamo.PackageManager;
using Dynamo.PackageManager.Interfaces;
using Moq;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageUploadBuilderTests
    {
        
        [Test]
        public void NewPackage_FailsToZipForOverlyLargeFile()
        {
            var fs = new RecordedFileSystem();

            var bigFile = new Mock<IFileInfo>();
            bigFile.Setup(x => x.Length).Returns(PackageUploadBuilder.MaximumPackageSize + 1);
            bigFile.Setup(x => x.Name).Returns("Foo");
            
            var zip = new Mock<IFileCompressor>();
            zip.Setup((x) => x.Zip("directory")).Returns(bigFile.Object);

            //var pub = new PackageDirectoryBuilder(fs, zip.Object);

        }



    }
}
