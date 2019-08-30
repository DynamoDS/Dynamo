using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dynamo.Applications;
using NUnit.Framework;

namespace IntegrationTests
{
    public class DynamoApplicationTests
    {
        [Test]
        public void MakeModelLoadsASMFromValidPath()
        {
            var versions = new List<Version>(){
                    new Version(224, 4, 0),
                    new Version(224, 0, 1),
                    new Version(223, 0, 1),
                    new Version(225, 0, 0) };


            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            Assert.DoesNotThrow(() =>
            {
                var model = Dynamo.Applications.StartupUtils.MakeModel(true, locatedPath);

            });

            var firstASMmodulePath = string.Empty;
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.FileName.Contains("ASMAHL"))
                {
                    firstASMmodulePath = module.FileName;
                    break;
                }
            }
            //assert that ASM is really loaded from exactly where we specified.
            Assert.AreEqual(Path.GetDirectoryName(firstASMmodulePath), locatedPath);

        }

        [Test]
        public void IfASMPathInvalidExceptionThrown()
        {
            var asmMockPath = @"./doesNotExist/";
            Assert.Throws<FileNotFoundException>(() =>
            {
                var model = Dynamo.Applications.StartupUtils.MakeModel(true, asmMockPath);
            });

        }
        [Test]
        public void GetVersionFromASMPath_returnsFileVersionForMockdll()
        {
            var version = StartupUtils.GetVersionFromASMPath(@"./", "DynamoCore*.dll");
            var thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Assert.AreEqual(version.Major, thisVersion.Major);
            Assert.AreEqual(version.Minor, thisVersion.Minor);
            Assert.AreEqual(version.Build, thisVersion.Build);
        }
    }
}
