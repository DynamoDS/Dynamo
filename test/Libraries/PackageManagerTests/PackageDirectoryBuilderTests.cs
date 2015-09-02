﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.PackageManager.Interfaces;
using Dynamo.Tests;
using Moq;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageDirectoryBuilderTests
    {
        public static bool ComparePaths(string path1, string path2)
        {
            return PackageDirectoryBuilder.NormalizePath(path1) == PackageDirectoryBuilder.NormalizePath(path2);
        }

        #region BuildPackageDirectory

        [Test]
        public void BuildPackageDirectory_DoesExpectedNumberOfOperations()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Any((x) => ComparePaths(x,fn)));

            var db = new PackageDirectoryBuilder(fs, MockMaker.Empty<IPathRemapper>());

            var pkgsDir = @"C:\dynamopackages";

            db.BuildDirectory(pkg, pkgsDir, files);

            Assert.AreEqual(4, fs.DirectoriesCreated.Count());
            Assert.AreEqual(2, fs.CopiedFiles.Count());
            Assert.AreEqual(2, fs.DeletedFiles.Count());
            Assert.AreEqual(1, fs.NewFilesWritten.Count());
        }

        [Test]
        public void BuildPackageDirectory_BuildsExpectedDirectories()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));

            var db = new PackageDirectoryBuilder(fs, MockMaker.Empty<IPathRemapper>());

            var pkgsDir = @"C:\dynamopackages";

            db.BuildDirectory(pkg, pkgsDir, files);

            var rootDir = Path.Combine(pkgsDir, pkg.Name);
            var binDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.BinaryDirectoryName);
            var dyfDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.CustomNodeDirectoryName);
            var extraDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.ExtraDirectoryName);

            Assert.IsTrue(fs.DirectoriesCreated.Any(x => x.FullName == rootDir));
            Assert.IsTrue(fs.DirectoriesCreated.Any(x => x.FullName == dyfDir));
            Assert.IsTrue(fs.DirectoriesCreated.Any(x => x.FullName == binDir));
            Assert.IsTrue(fs.DirectoriesCreated.Any(x => x.FullName == extraDir));
        }

        [Test]
        public void BuildPackageDirectory_FormsPackageHeader()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));

            var pr = new Mock<IPathRemapper>();
            var db = new PackageDirectoryBuilder(fs, pr.Object);

            var pkgsDir = @"C:\dynamopackages";

            // where the magic happens...
            db.BuildDirectory(pkg, pkgsDir, files);

            var rootDir = Path.Combine(pkgsDir, pkg.Name);
            
            Assert.IsTrue(fs.NewFilesWritten.Any(x => x.Item1 == Path.Combine(rootDir, PackageDirectoryBuilder.PackageJsonName)));
        }

        [Test]
        public void BuildPackageDirectory_RemapsCustomNodePaths()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));

            var pr = new Mock<IPathRemapper>();

            var remappedPaths = new List<Tuple<string, string>>();

            pr.Setup(x => x.SetPath(files[0], It.IsAny<string>()))
                .Callback((string f, string s) => remappedPaths.Add(new Tuple<string, string>(f, s)));

            pr.Setup(x => x.SetPath(files[1], It.IsAny<string>()))
                .Callback((string f, string s) => remappedPaths.Add(new Tuple<string, string>(f, s)));

            var db = new PackageDirectoryBuilder(fs, pr.Object);

            var pkgsDir = @"C:\dynamopackages";

            db.BuildDirectory(pkg, pkgsDir, files);

            var dyfDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.CustomNodeDirectoryName);

            Assert.IsTrue(remappedPaths.Any(x => x.Item1 == files[0] && x.Item2 == dyfDir));
            Assert.IsTrue(remappedPaths.Any(x => x.Item1 == files[1] && x.Item2 == dyfDir));
        }

        [Test]
        public void BuildPackageDirectory_UpdatesTheArgumentPackageWithNewDirectories()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));

            var pr = new Mock<IPathRemapper>();
            var db = new PackageDirectoryBuilder(fs, pr.Object);

            var pkgsDir = @"C:\dynamopackages";

            db.BuildDirectory(pkg, pkgsDir, files);

            var rootDir = Path.Combine(pkgsDir, pkg.Name);
            var binDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.BinaryDirectoryName);
            var dyfDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.CustomNodeDirectoryName);
            var extraDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.ExtraDirectoryName);

            // The package itself is updated

            Assert.AreEqual(rootDir, pkg.RootDirectory);
            Assert.AreEqual(binDir, pkg.BinaryDirectory);
            Assert.AreEqual(dyfDir, pkg.CustomNodeDirectory);
            Assert.AreEqual(extraDir, pkg.ExtraDirectory);
        }

        [Test]
        public void BuildPackageDirectory_CopiesTheOriginalFiles()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));

            var pr = new Mock<IPathRemapper>();
            var db = new PackageDirectoryBuilder(fs, pr.Object);

            var pkgsDir = @"C:\dynamopackages";

            db.BuildDirectory(pkg, pkgsDir, files);

            var dyfDir = Path.Combine(pkgsDir, pkg.Name, PackageDirectoryBuilder.CustomNodeDirectoryName);

            Assert.IsTrue(fs.CopiedFiles.Any(x => ComparePaths(x.Item2, Path.Combine(dyfDir, "file1.dyf"))));
            Assert.IsTrue(fs.CopiedFiles.Any(x => ComparePaths(x.Item2, Path.Combine(dyfDir, "file2.dyf"))));
        }

        [Test]
        public void BuildPackageDirectory_DeletesTheOriginalFiles()
        {
            var files = new[] { @"C:\file1.dyf", @"C:\file2.dyf" };
            var pkg = new Package(@"C:\pkg", "Foo", "0.1.0", "MIT");

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));

            var pr = new Mock<IPathRemapper>();

            var db = new PackageDirectoryBuilder(fs, pr.Object);

            var pkgsDir = @"C:\dynamopackages";

            db.BuildDirectory(pkg, pkgsDir, files);

            // The original files are moved

            Assert.Contains(files[0], fs.DeletedFiles.ToList());
            Assert.Contains(files[1], fs.DeletedFiles.ToList());
        }

        #endregion

        #region CopyFilesIntoPackageDirectory

        [Test]
        public void CopyFilesIntoPackageDirectory_DoesNotMoveFilesAlreadyWithinDirectory()
        {
            var files = new[] { @"C:\foo/dyf\file1.dyf", @"C:\\foo\dyf\file2.dyf", @"C:\\foo\dyf\file3.dyf", 
                @"C:\foo/bin\file1.dll", @"C:\\foo\bin\file2.dll", @"C:\\foo\bin\file3.dll", 
                @"C:\foo/extra\file1.pdf", @"C:\\foo\extra\file2.rvt", @"C:\\foo\extra\file3.poo" };

            var fs = new RecordedFileSystem((fn) => files.Contains(fn), (dn) => true);
            var f = new PackageDirectoryBuilder(fs, new Mock<IPathRemapper>().Object);

            var dyf = new Mock<IDirectoryInfo>();
            dyf.SetupGet((i) => i.FullName).Returns(() => "C:/foo/dyf");

            var bin = new Mock<IDirectoryInfo>();
            bin.SetupGet((i) => i.FullName).Returns(() => "C:/foo/bin");

            var extra = new Mock<IDirectoryInfo>();
            extra.SetupGet((i) => i.FullName).Returns(() => "C:/foo/extra");

            f.CopyFilesIntoPackageDirectory(files, dyf.Object, bin.Object, extra.Object);

            // no files should be copied, they are all already within their intended directory
            Assert.IsEmpty(fs.CopiedFiles);
        }

        #endregion

    }
}
