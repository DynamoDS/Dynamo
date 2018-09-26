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
    class libGPreloaderTests
    {
        //TODO can we make this not require revit of specific version?
        [Test]
        public void preloaderMapsOldLibraryVersionToSpecificVersion()
        {

            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var preloader = new Preloader(Path.GetDirectoryName(assemblyPath), LibraryVersion.Version223);

            //cleanup libg directory structure

        }

        [Test]
        public void GetInstalledASMVersions2FindsVersionedLibGFolders()
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

                {"revit2017_InstallLocation" ,Tuple.Create<int,int,int,int>(222,0,0,0)},
                {"revit2018_InstallLocation" ,Tuple.Create<int,int,int,int>(223,0,1,0)},
                {"revit2019_InstallLocation" ,Tuple.Create<int,int,int,int>(224,0,1,0)},
                //TODO what is the real version 2020 will bundle
                //TODO what si the real version 2019.2 will bundle
                {"revit2019.2_InstallLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},
                {"revit2020_InstallLocation" ,Tuple.Create<int,int,int,int>(224,4,0,0)},

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

            Assert.AreEqual(newestASM,foundVersion );
            Assert.AreEqual("revit2020_InstallLocation", foundPath);
        }

        [Test]
        public void GetInstalledASMVersions2FindsVersionedLibGFoldersWithFallback()
        {


        }
    }
}
