using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DynamoShapeManager;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class libGPreloaderTests
    {

        [Test]
        public void GetInstalledASMVersions2_FindsVersionedLibGFolders()
        {
            var versions = new List<Version>()
            {
                    new Version(224,4,0),
                    new Version(224,0,1),
                    new Version(225,0,0)
            };

            versions.Sort();
            versions.Reverse();

            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"revit2017_InstallLocation" ,Tuple.Create<int,int,int,int>(222,0,0,0)},
                {"revit2018_InstallLocation" ,Tuple.Create<int,int,int,int>(223,0,1,0)},
                {"revit2019_InstallLocation" ,Tuple.Create<int,int,int,int>(224,0,1,0)},
                {"revit2019.2_InstallLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},
                {"revit2020_InstallLocation" ,Tuple.Create<int,int,int,int>(225,0,0,0)}

            };

            var newestASM = new Version(225, 0, 0);
            //first version should be 225.0.0
            Console.WriteLine(versions);
            Assert.AreEqual(newestASM, versions.First());

            //mock a folder with libASMLibVersionToVersionG folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            //both versions of libG exist
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            var libG22500path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_225_0_0"));


            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            Assert.AreEqual(newestASM, foundVersion);
            Assert.AreEqual("revit2020_InstallLocation", foundPath);
            //cleanup
            libG22440path.Delete(true);
            libG22500path.Delete(true);
        }

        [Test]
        public void GetInstalledASMVersions2_MultipleClientsWithSameASM()
        {
            var versions = new List<Version>()
            {
                    new Version(224,4,0),
                    new Version(224,0,1),
                    new Version(225,0,0)
            };

            versions.Sort();
            versions.Reverse();


            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"revit2017_InstallLocation" ,Tuple.Create<int,int,int,int>(222,0,0,0)},
                {"revit2018_InstallLocation" ,Tuple.Create<int,int,int,int>(223,0,1,0)},
                {"revit2019_InstallLocation" ,Tuple.Create<int,int,int,int>(224,0,1,0)},
                {"revit2019.2_InstallLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},
                {"revit2020_InstallLocation" ,Tuple.Create<int,int,int,int>(225,0,0,0)},
                {"AutoCAD_InstallLocation" ,Tuple.Create<int,int,int,int>(225,0,0,0)},

            };

            var newestASM = new Version(225, 0, 0);
            //first version should be 225.0.0
            Console.WriteLine(versions);
            Assert.AreEqual(newestASM, versions.First());

            //mock a folder with libASMLibVersionToVersionG folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            //both versions of libG exist
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            var libG22500path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_225_0_0"));


            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            Assert.AreEqual(newestASM, foundVersion);
            Assert.AreEqual("revit2020_InstallLocation", foundPath);
            //cleanup
            libG22440path.Delete(true);
            libG22500path.Delete(true);
        }

        [Test]
        public void GetInstalledASMVersions2_ASMVersionFallback()
        {
            var versions = new List<Version>()
            {
                    new Version(224,4,0),
                    new Version(224,0,1),
                    // Notice the lookup version here is different than the actual found version below
                    // because there is no local mocked product with that specific ASM version installed
                    new Version(225,4,0)
            };

            versions.Sort();
            versions.Reverse();


            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"revit2017_InstallLocation" ,Tuple.Create<int,int,int,int>(222,0,0,0)},
                {"revit2018_InstallLocation" ,Tuple.Create<int,int,int,int>(223,0,1,0)},
                {"revit2019_InstallLocation" ,Tuple.Create<int,int,int,int>(224,0,1,0)},
                {"revit2019.2_InstallLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},
                {"revit2020_InstallLocation" ,Tuple.Create<int,int,int,int>(225,0,0,0)},

            };

            var newestASM = new Version(225, 4, 0);
            //first version should be 225.4.0
            Console.WriteLine(versions);
            Assert.AreEqual(newestASM, versions.First());

            //mock a folder with libASMLibVersionToVersionG folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            //both versions of libG exist
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            var libG22500path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_225_0_0"));


            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            // The found version in this case is a fallback of lowest version within same major which should be 225.0.0
            Assert.AreNotEqual(newestASM, foundVersion);
            Assert.AreEqual("revit2020_InstallLocation", foundPath);
            //cleanup
            libG22440path.Delete(true);
            libG22500path.Delete(true);
        }

        [Test]
        public void GetLibGPreloaderLocation_libGVersionFallback()
        {
            var versions = new List<Version>()
            {
                    new Version(225,4,0)
            };

            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"revit_Prerelease_InstallLocation" ,Tuple.Create<int,int,int,int>(225,3,0,0)},
            };

            var targetVersion = new Version(225, 3, 0);

            // mock a folder with libASMLibVersionToVersionG folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            // both versions of libG exist
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            var libG22500path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_225_0_0"));

            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            // The found ASM version in this case is a fallback of lowest version within same major which should be 225.3.0
            Assert.AreEqual(targetVersion, foundVersion);
            Assert.AreEqual("revit_Prerelease_InstallLocation", foundPath);

            // The found libG preloader version in this case is another fallback of closest version below 225.3.0
            Assert.AreEqual(libG22500path.FullName.ToLower(), DynamoShapeManager.Utilities.GetLibGPreloaderLocation(foundVersion, rootFolder).ToLower());
            // cleanup
            libG22440path.Delete(true);
            libG22500path.Delete(true);
        }

        [Test]
        public void GetGeometryFactoryPath_libGVersionFallback()
        {
            var versions = new List<Version>()
            {
                    new Version(225,0,0)
            };

            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"revit_2020_InstallLocation" ,Tuple.Create<int,int,int,int>(225,2,0,0)},
            };

            var targetVersion = new Version(225, 2, 0);

            // mock a folder with libASMLibVersionToVersion folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            // both versions of libG exist
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            var libG22500path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_225_0_0"));
            //create a fake libg interface assembly
            File.WriteAllText(Path.Combine(libG22500path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly), "someText");

            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            // The found ASM version in this case is a fallback of lowest version within same major which should be 225.2.0
            Assert.AreEqual(targetVersion, foundVersion);
            Assert.AreEqual("revit_2020_InstallLocation", foundPath);

            // The found libG preloader version in this case is another fallback of closest version below 225.2.0
            Assert.AreEqual(libG22500path.FullName.ToLower(), DynamoShapeManager.Utilities.GetLibGPreloaderLocation(foundVersion, rootFolder).ToLower());

            //assert that the geometryFactory method returns the path of the lib225_0_0 path.
            Assert.AreEqual(Path.Combine(libG22500path.FullName,DynamoShapeManager.Utilities.GeometryFactoryAssembly).ToLower(),
                DynamoShapeManager.Utilities.GetGeometryFactoryPath2(rootFolder, targetVersion).ToLower());

            // cleanup
            libG22440path.Delete(true);
            libG22500path.Delete(true);
        }

        [Test]
        public void GetGeometryFactoryPathTolerant_NoMatch()
        {
            var versions = new List<Version>()
            {
                    new Version(225,0,0)
            };

            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"someInstallWithNoMatchingASM" ,Tuple.Create<int,int,int,int>(224,0,0,0)},
            };

            // mock a folder with libASMLibVersionToVersion folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            //there is no matching libG for the installed version of asm.
            var libG22500path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_225_0_0"));
            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            // There is no match, so found version is null.
            Assert.AreEqual(null, foundVersion);
            // There is no match, so path to ASM is null.
            Assert.AreEqual(string.Empty, foundPath);

            // when passed null as a version, GetLibGPreloaderLocation will return LibG_0_0_0
            Assert.IsTrue(DynamoShapeManager.Utilities.GetLibGPreloaderLocation(foundVersion, rootFolder).ToLower().Contains("libg_0_0_0"));

            //when passed a null version this method should throw
            Assert.Throws<ArgumentNullException>(() => { DynamoShapeManager.Utilities.GetGeometryFactoryPath2(rootFolder, null); });

            //when passed a null root directory this method should throw - as a valid root directory is required.
            Assert.Throws<ArgumentNullException>(() => { DynamoShapeManager.Utilities.GetGeometryFactoryPath2(null, new Version(224, 24, 24)); });

            // when passed a non null, non matching version and existing root directory
            // this method should throw.

            Assert.Throws<DirectoryNotFoundException>(() => { DynamoShapeManager.Utilities.GetGeometryFactoryPath2(rootFolder, new Version(224, 24, 24)); });

            // cleanup
            libG22500path.Delete(true);
        }


        [Test]
        public void GetInstalledASMVersions2_FindsVersionedLibGFolders_WithRootFolderFallback()
        {
            var versions = new List<Version>()
            {
                    new Version(224,4,0),
                    new Version(224,0,1),
            };

            versions.Sort();
            versions.Reverse();


            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {
            };

            //mock a folder with libASMLibVersionToVersionG folders with correct names and ASM dlls nested.
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");

            //create some
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            File.WriteAllText(Path.Combine(libG22440path.FullName, "ASMAHL224A.dll"), "someText");
            File.WriteAllText(Path.Combine(libG22440path.FullName, "tbb.dll"), "someText");
            File.WriteAllText(Path.Combine(libG22440path.FullName, "tbbmalloc.dll"), "someText");
            var libG22401path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_0_1"));
            File.WriteAllText(Path.Combine(libG22401path.FullName, "ASMAHL224A.dll"), "someText");
            File.WriteAllText(Path.Combine(libG22401path.FullName, "tbb.dll"), "someText");
            File.WriteAllText(Path.Combine(libG22401path.FullName, "tbbmalloc.dll"), "someText");


            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });
            var newestASM = new Version(224, 4, 0);

            Assert.AreEqual(newestASM, foundVersion);
            Assert.AreEqual(libG22440path, new DirectoryInfo(foundPath));
            libG22440path.Delete(true);
            libG22401path.Delete(true);

        }


        [Test]
        public void GetInstalledASMVersions2_ReturnsFirstLocationWithSameVersion()
        {
            var versions = new List<Version>()
            {
                    new Version(224,4,0),
                    new Version(224,0,1),
            };

            versions.Sort();
            versions.Reverse();


            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"firstLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},
                {"secondLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},

            };

            var newestASM = new Version(224, 4, 0);
            //first version should be 224.4.0
            Console.WriteLine(versions);
            Assert.AreEqual(newestASM, versions.First());

            //mock a folder with libASMLibVersionToVersionG folders with correct names
            var foundPath = "";
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");
            //both versions of libG exist
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            var libG22401path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_0_1"));

            var foundVersion = DynamoShapeManager.Utilities.GetInstalledAsmVersion2(
                versions, ref foundPath, rootFolder, (path) => { return mockedInstalledASMs; });

            Assert.AreEqual(newestASM, foundVersion);
            Assert.AreEqual("firstLocation", foundPath);
            //cleanup
            libG22440path.Delete(true);
            libG22401path.Delete(true);
        }

        [Test]
        public void GetGeometryFactoryPath_CalledFromOldClient_ShouldGetNewVersion()
        {
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");

            //setup some mock libG folders with protoInterface.dll nested.
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            File.WriteAllText(Path.Combine(libG22440path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly), "someText");
            var libG22401path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_0_1"));
            File.WriteAllText(Path.Combine(libG22401path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly), "someText");

            //look for old version of libG 224 from old client
            var foundGeoPath = DynamoShapeManager.Utilities.GetGeometryFactoryPath(rootFolder, LibraryVersion.Version224);

            var expectedDirectoryInfo = new DirectoryInfo(Path.Combine(libG22401path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly));
            Assert.AreEqual(expectedDirectoryInfo, new DirectoryInfo(foundGeoPath));
            //cleanup
            libG22440path.Delete(true);
            libG22401path.Delete(true);
        }

        [Test]
        public void GetGeometryFactoryPath2_CalledFromNewClient_ShouldGetCorrectVersion()
        {
            var rootFolder = Path.Combine(Path.GetTempPath(), "LibGTest");

            //setup some mock libG folders with protoInterface.dll nested.
            var libG22440path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_4_0"));
            File.WriteAllText(Path.Combine(libG22440path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly), "someText");
            var libG22401path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_0_1"));
            File.WriteAllText(Path.Combine(libG22401path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly), "someText");

            //a client like revit 2.1 might make this call.
            var foundGeoPath = DynamoShapeManager.Utilities.GetGeometryFactoryPath2(rootFolder, new Version(224, 4, 0));

            var expectedDirectoryInfo = new DirectoryInfo(Path.Combine(libG22440path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly));
            Assert.AreEqual(expectedDirectoryInfo, new DirectoryInfo(foundGeoPath));
            //cleanup
            libG22440path.Delete(true);
            libG22401path.Delete(true);
        }

        [Test]
        public void LoadASMFromPathShouldWorkWithOldPath()
        {
            var oldPath = Path.Combine("C", "Dynamo", "Extern", "FakePath", "LibG_223");
            var newPath = DynamoShapeManager.Utilities.RemapOldLibGPathToNewVersionPath(oldPath);
            Assert.AreEqual(new DirectoryInfo(Path.Combine("C", "Dynamo", "Extern", "FakePath", "LibG_223_0_1")), new DirectoryInfo(newPath));
        }
        [Test]
        public void RemapPathShouldReturnEmptyStringForNewPath()
        {
            var oldPath = Path.Combine("C", "Dynamo", "Extern", "FakePath", "LibG_223_0_1");
            var newPath = DynamoShapeManager.Utilities.RemapOldLibGPathToNewVersionPath(oldPath);
            Assert.AreEqual(String.Empty, newPath);
        }
        [Test]
        public void RemapPathShouldReturnEmptyStringForNullPath()
        {
            string oldPath = null;
            var newPath = DynamoShapeManager.Utilities.RemapOldLibGPathToNewVersionPath(oldPath);
            Assert.AreEqual(string.Empty, newPath);
        }
        [Test]
        public void PreloaderThatDoesNotFindASMDoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var preloader = new Preloader(Path.GetTempPath(), new[] { new Version(999, 999, 999) });
            });
        }
    }
}
