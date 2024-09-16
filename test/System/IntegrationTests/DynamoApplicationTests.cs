using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dynamo.Applications;
using Dynamo.Logging;
using Dynamo.Models;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace IntegrationTests
{
    public class DynamoApplicationTests
    {
        [Test]
        public void DynamoSandboxLoadsASMFromValidPath()
        {
            var versions = new List<Version>(){
                new Version(230, 0, 0),
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
            Assert.AreEqual(DynamoModel.HostAnalyticsInfo.HostName, "DynamoFormIt");
        }
        [Test]
        public void DynamoModelStartedWithNoNetworkMode_AlsoDisablesAnalytics()
        {
            var startConfig = new DefaultStartConfiguration() { NoNetworkMode = true };
            var model = DynamoModel.Start(startConfig);
            Assert.AreEqual(true, Analytics.DisableAnalytics);
            model.ShutDown(false);
        }
        [Test]
        public void DynamoModelStartedWithNoNetworkModeFalse_DisablesAnalyticsCanBeTrue()
        {
            var startConfig = new DefaultStartConfiguration() { NoNetworkMode = false };
            Analytics.DisableAnalytics = true;
            var model = DynamoModel.Start(startConfig);
            Assert.AreEqual(true, Analytics.DisableAnalytics);
            model.ShutDown(false);
        }

        [Test]
        public void IfASMPathInvalidExceptionNotThrown()
        {
            var asmMockPath = @"./doesNotExist/";
            Assert.DoesNotThrow(() =>
            {
                var model = StartupUtils.MakeModel(true, asmMockPath);
                Assert.IsNotNull(model);
            });
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
