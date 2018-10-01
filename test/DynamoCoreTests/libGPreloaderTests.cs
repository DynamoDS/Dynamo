using DynamoShapeManager;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                    new Version(223,0,1),
                    new Version(222,0,0),
                    new Version(221,0,0),
                    new Version(224,7,0)
            };

            versions.Sort();
            versions.Reverse();


            var mockedInstalledASMs = new Dictionary<string, Tuple<int, int, int, int>>()
            {

                {"revit2017_InstallLocation" ,Tuple.Create<int,int,int,int>(222,0,0,0)},
                {"revit2018_InstallLocation" ,Tuple.Create<int,int,int,int>(223,0,1,0)},
                {"revit2019_InstallLocation" ,Tuple.Create<int,int,int,int>(224,0,1,0)},
                //TODO what si the real version 2019.2 will bundle
                {"revit2019.2_InstallLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},
                //TODO what is the real version 2020 will bundle
                {"revit2020_InstallLocation" ,Tuple.Create<int,int,int,int>(224,7,0,0)},

            };

            var newestASM = new Version(224, 7, 0);
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
            Assert.AreEqual("revit2020_InstallLocation", foundPath);
            //cleanup
            libG22440path.Delete(true);
            libG22401path.Delete(true);
        }

        [Test]
        public void GetInstalledASMVersions2_FindsVersionedLibGFolders_WithRootFolderFallback()
        {
            var versions = new List<Version>()
            {
                    new Version(224,4,0),
                    new Version(224,0,1),
                    new Version(223,0,1),
                    new Version(222,0,0),
                    new Version(221,0,0)
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
            File.WriteAllText(Path.Combine(libG22440path.FullName, "ASMAHL.dll"), "someText");
            var libG22401path = System.IO.Directory.CreateDirectory(Path.Combine(rootFolder, "LibG_224_0_1"));
            File.WriteAllText(Path.Combine(libG22401path.FullName, "ASMAHL.dll"), "someText");


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
                    new Version(223,0,1),
                    new Version(222,0,0),
                    new Version(221,0,0)
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
            var foundGeoPath = DynamoShapeManager.Utilities.GetGeometryFactoryPath2(rootFolder, new Version(224,4,0));

            var expectedDirectoryInfo = new DirectoryInfo(Path.Combine(libG22440path.FullName, DynamoShapeManager.Utilities.GeometryFactoryAssembly));
            Assert.AreEqual(expectedDirectoryInfo, new DirectoryInfo(foundGeoPath));
            //cleanup
            libG22440path.Delete(true);
            libG22401path.Delete(true);
        }
    }
}
