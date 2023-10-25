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
        public void DynamoSandboxLoadsASMFromValidPath()
        {
            var versions = new List<Version>(){
                    new Version(229,0,0),
                    new Version(228, 6, 0)
            };


            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Process dynamoSandbox = null;

            DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    // we use a new process to avoid checking against previously loaded
                    // asm modules in the nunit-agent process.
                    dynamoSandbox = System.Diagnostics.Process.Start(new ProcessStartInfo(Path.Combine(coreDirectory, "DynamoSandbox.exe"), $"-gp \"{locatedPath}\""){ UseShellExecute = true });
                    dynamoSandbox.WaitForInputIdle();

                    var firstASMmodulePath = string.Empty;
                    foreach (ProcessModule module in dynamoSandbox.Modules)
                    {
                        if (module.FileName.Contains("ASMAHL"))
                        {
                            firstASMmodulePath = module.FileName;
                            break;
                        }
                    }
                    // TODO: This test need to be updated somehow to bypass splash screen
                    if (!string.IsNullOrEmpty(firstASMmodulePath))
                    {
                        //assert that ASM is really loaded from exactly where we specified.
                        Assert.AreEqual(Path.GetDirectoryName(firstASMmodulePath), locatedPath);
                    }
                });
            }
            finally
            {

                dynamoSandbox?.Kill();

            }
        }

        [Test]
        public void DynamoMakeModelWithHostName()
        {
            var model = Dynamo.Applications.StartupUtils.MakeModel(false, string.Empty, "DynamoFormIt");
            Assert.AreEqual(model.HostName, "DynamoFormIt");
        }

        [Test]
        public void GetVersionFromASMPath_returnsFileVersionForMockdll()
        {
            var version = DynamoShapeManager.Utilities.GetVersionFromPath(@"./", "DynamoCore*.dll");
            var thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Assert.AreEqual(version.Major, thisVersion.Major);
            Assert.AreEqual(version.Minor, thisVersion.Minor);
            Assert.AreEqual(version.Build, thisVersion.Build);
        }
    }
}
