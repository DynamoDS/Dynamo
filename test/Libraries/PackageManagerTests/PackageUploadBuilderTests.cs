﻿using System;
using System.Collections.Generic;
using Dynamo.PackageManager.Interfaces;
using Dynamo.Tests;
using Moq;
using NUnit.Framework;

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
