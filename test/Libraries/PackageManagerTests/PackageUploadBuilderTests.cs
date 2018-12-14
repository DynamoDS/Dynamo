using Dynamo.PackageManager.Interfaces;
using Dynamo.Tests;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Dynamo.PackageManager.Tests
{
    class PackageUploadBuilderTests
    {
        #region Utilities

        private IPackageUploadBuilder BigPackageUploadBuilderMock()
        {
            // a IFileInfo object that is, by mocking, too large
            var bigzip = new Mock<IFileInfo>();
            bigzip.Setup(x => x.Length).Returns(PackageUploadBuilder.MaximumPackageSize + 1);

            // the zipper returns a big zip
            var zipper = new Mock<IFileCompressor>();
            zipper.Setup((x) => x.Zip(It.IsAny<IDirectoryInfo>())).Returns(bigzip.Object);

            var pdb = new Mock<IPackageDirectoryBuilder>();
            pdb.Setup(x => x.BuildDirectory(It.IsAny<Package>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns((new Mock<IDirectoryInfo>()).Object);

            // this package upload builder will try to return a zip that is too big
            return new PackageUploadBuilder(pdb.Object, zipper.Object);
        }

        private class directoryTestClass : IDirectoryInfo
        {
            public string FullName { get; set; }

        }

        #endregion

        private string ComputeHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                var hash = Convert.ToBase64String(md5.ComputeHash(File.ReadAllBytes(filePath)));
                return hash;
            }
        }

        #region GregFileUtilsTests
        [Test]
        public void FileCompressorZipsSamplePackageDirectoryToValidArchive()
        {
            var executingLocation = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            var testDirectory = Path.Combine(executingLocation.Parent.Parent.Parent.Parent.FullName, "test");
            var aPackagePath = new DirectoryInfo(Path.Combine(testDirectory, "pkgs", "sampleExtension"));
            var allFiles = aPackagePath.GetFiles("*", SearchOption.AllDirectories);
            var preZipfileCount = allFiles.Count();
            var preZipDiskSize = allFiles.Sum(x => x.Length);
            var preZipMD5Map = allFiles.ToDictionary(file => file.Name, file => ComputeHash(file.FullName));

            var packageDir = new directoryTestClass()
            {
                FullName = aPackagePath.FullName
            };

            var compressor = new MutatingFileCompressor();
            var zipPath = compressor.Zip(packageDir);

            //unzip the zipped directory
            var unzipPath = Greg.Utility.FileUtilities.UnZip(zipPath.Name);
            var unzippedDirectory = new DirectoryInfo(unzipPath);
            var allUnzippedFiles = unzippedDirectory.GetFiles("*", SearchOption.AllDirectories);

            var postZipMD5Map = allUnzippedFiles.ToDictionary(file => file.Name, file => ComputeHash(file.FullName));

            Assert.AreEqual(preZipDiskSize, allUnzippedFiles.Sum(x => x.Length));
            Assert.AreEqual(preZipfileCount, allUnzippedFiles.Length);
            foreach (var pair in preZipMD5Map)
            {
                Console.WriteLine(pair);
                Assert.AreEqual(pair.Value, postZipMD5Map[pair.Key]);
            }

        }
        [Test]
        public void FileCompressorZipsRegressionDirectoryToValidArchive()
        {
            //https://github.com/DynamoDS/Dynamo/issues/8982

            var executingLocation = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            var testDirectory = Path.Combine(executingLocation.Parent.Parent.Parent.Parent.FullName, "test");
            var aPackagePath = new DirectoryInfo(Path.Combine(testDirectory, "pkgs", "the-Saurus"));
            var allFiles = aPackagePath.GetFiles("*", SearchOption.AllDirectories);
            var preZipfileCount = allFiles.Count();
            var preZipDiskSize = allFiles.Sum(x => x.Length);
            var preZipMD5Map = allFiles.ToDictionary(file => file.Name, file => ComputeHash(file.FullName));


            var packageDir = new directoryTestClass()
            {
                FullName = aPackagePath.FullName
            };

            var compressor = new MutatingFileCompressor();
            var zipPath = compressor.Zip(packageDir);

            //unzip the zipped directory
            var unzipPath = Greg.Utility.FileUtilities.UnZip(zipPath.Name);
            var unzippedDirectory = new DirectoryInfo(unzipPath);
            var allUnzippedFiles = unzippedDirectory.GetFiles("*", SearchOption.AllDirectories);

            var postZipMD5Map = allUnzippedFiles.ToDictionary(file => file.Name, file => ComputeHash(file.FullName));

            Assert.AreEqual(preZipDiskSize, allUnzippedFiles.Sum(x => x.Length));
            Assert.AreEqual(preZipfileCount, allUnzippedFiles.Length);
            foreach (var pair in preZipMD5Map)
            {
                Console.WriteLine(pair);
                Assert.AreEqual(pair.Value, postZipMD5Map[pair.Key]);
            }
        }

        #endregion

        #region NewPackageUpload

        [Test]
        public void NewPackageUpload_ThrowsExceptionWhenPackageIsTooBig()
        {
            var pub = BigPackageUploadBuilderMock();

            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");
            var pkgsDir = @"C:\dynamopackages";

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));

            Assert.Throws<Exception>(() => pub.NewPackageUpload(pkg, pkgsDir, files, handle));
        }

        [Test]
        public void NewPackageVersionUpload_ThrowsForNullArguments()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");
            var pkgsDir = @"C:\dynamopackages";
            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));

            var m = new PackageUploadBuilder(MockMaker.Empty<IPackageDirectoryBuilder>(), MockMaker.Empty<IFileCompressor>());

            Assert.Throws<ArgumentNullException>(() => m.NewPackageVersionUpload(null, pkgsDir, files, handle));
            Assert.Throws<ArgumentNullException>(() => m.NewPackageVersionUpload(pkg, null, files, handle));
            Assert.Throws<ArgumentNullException>(() => m.NewPackageVersionUpload(pkg, pkgsDir, null, handle));
            Assert.Throws<ArgumentNullException>(() => m.NewPackageVersionUpload(pkg, pkgsDir, files, null));
        }

        #endregion

        #region NewPackageVersionUpload

        [Test]
        public void NewPackageVersionUpload_ThrowsExceptionWhenPackageIsTooBig()
        {
            var pub = BigPackageUploadBuilderMock();

            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");
            var pkgsDir = @"C:\dynamopackages";

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));

            Assert.Throws<Exception>(() => pub.NewPackageVersionUpload(pkg, pkgsDir, files, handle));
        }

        [Test]
        public void NewPackageUpload_ThrowsForNullArguments()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");
            var pkgsDir = @"C:\dynamopackages";
            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));

            var m = new PackageUploadBuilder(MockMaker.Empty<IPackageDirectoryBuilder>(), MockMaker.Empty<IFileCompressor>());

            Assert.Throws<ArgumentNullException>(() => m.NewPackageUpload(null, pkgsDir, files, handle));
            Assert.Throws<ArgumentNullException>(() => m.NewPackageUpload(pkg, null, files, handle));
            Assert.Throws<ArgumentNullException>(() => m.NewPackageUpload(pkg, pkgsDir, null, handle));
            Assert.Throws<ArgumentNullException>(() => m.NewPackageUpload(pkg, pkgsDir, files, null));
        }

        #endregion

        #region NewRequestBody

        [Test]
        public void NewRequestBody_SetsCorrectEngineName()
        {
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");
            var b = PackageUploadBuilder.NewRequestBody(pkg);

            Assert.AreEqual(PackageManagerClient.PackageEngineName, b.engine);
        }

        [Test]
        public void NewRequestBody_ThrowsForNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => PackageUploadBuilder.NewRequestBody(null));
        }

        #endregion

    }
}
