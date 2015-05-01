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
            //var bigFile = new Mock<IFileInfo>();
            //bigFile.Setup(x => x.Length).Returns(PackageUploadBuilder.MaximumPackageSize + 1);
            //bigFile.Setup(x => x.Name).Returns("Foo");
            
            //var zip = new Mock<IFileCompressor>();
            //zip.Setup((x) => x.Zip(It.IsAny<string>())).Returns(bigFile.Object);

            //var pdb = new Mock<IPackageDirectoryBuilder>();

            //var pub = new PackageUploadBuilder(pdb.Object, zip.Object);

            //var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            //var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");
            //var pkgsDir = @"C:\dynamopackages";
            //var handle = new PackageUploadHandle(PackageUploadBuilder.r);

            //pub.NewPackageUpload(pkg, pkgsDir, files, 

        }
    }
}
