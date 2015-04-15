using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Core;
using NUnit.Framework;

namespace Dynamo
{
    public class UserDataMigrationTests : UnitTestBase
    {
        private static void VerifySortedOrder(List<FileVersion> list)
        {
            for (int i = 0; i < list.Count() - 1; ++i)
            {
                var f1 = list[i];
                var f2 = list[i + 1];
                Assert.IsTrue(f1 >= f2);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void FileVersionMajorMinorParts()
        {
            var fileVersion = new FileVersion(12, 34);
            Assert.AreEqual(12, fileVersion.MajorPart);
            Assert.AreEqual(34, fileVersion.MinorPart);
        }

        [Test]
        [Category("UnitTests")]
        public void FileVersionCompare_Sorting()
        {
            var fv1 = new FileVersion(0, 7);
            var fv2 = new FileVersion(0, 8);

            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(0, 0);
            fv2 = new FileVersion(0, 0);
            Assert.IsFalse(fv1 < fv2);
            Assert.IsFalse(fv1 > fv2);
            Assert.IsFalse(fv2 < fv1);
            Assert.IsFalse(fv2 > fv1);

            fv1 = new FileVersion(1, 7);
            fv2 = new FileVersion(1, 8);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(1, 9);
            fv2 = new FileVersion(2, 0);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-1, 9);
            fv2 = new FileVersion(0, 0);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-2, 9);
            fv2 = new FileVersion(-1, 0);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(2, -9);
            fv2 = new FileVersion(3, 0);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(2, -9);
            fv2 = new FileVersion(2, -8);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-2, -9);
            fv2 = new FileVersion(-2, -8);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-2, -9);
            fv2 = new FileVersion(-1, -9);
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);
        }

        [Test]
        [Category("UnitTests")]
        public void SortFileVersionList_SortedList()
        {
            var list = new List<FileVersion>
            {
                new FileVersion(0, 7),
                new FileVersion(0, 8),
                new FileVersion(0, 0),
                new FileVersion(0, 0),
                new FileVersion(1, 7),
                new FileVersion(1, 8),
                new FileVersion(1, 9),
                new FileVersion(2, 0),
                new FileVersion(-1, 9),
                new FileVersion(0, 0),
                new FileVersion(-2, 9),
                new FileVersion(-1, 0),
                new FileVersion(2, -9),
                new FileVersion(3, 0),
                new FileVersion(2, -9),
                new FileVersion(2, -8),
                new FileVersion(-2, -9),
                new FileVersion(-2, -8),
                new FileVersion(-2, -9),
                new FileVersion(-1, -9),

            };

            list.Sort();

            VerifySortedOrder(list);
        }

        [Test]
        [Category("UnitTests")]
        public void GetSortedInstalledVersions_FromDirectoryInspection()
        {
            var tempPath = base.TempFolder;
            var uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "0.7"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "0.8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "0.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "1.7"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "1.8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "1.9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "2.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-1.9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-2.9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-1.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "3.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "2.-9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "2.-8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-2.-9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-2.-8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-1.-9"));

            var versionList = DynamoMigratorBase.GetInstalledVersions(tempPath).ToList();

            VerifySortedOrder(versionList);

            try
            {
                DynamoMigratorBase.GetInstalledVersions("");
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.ParamName == "rootFolder");
            }
            
            try
            {
                DynamoMigratorBase.GetInstalledVersions(null);
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.ParamName == "rootFolder");                
            }
            
        }
    }
}
