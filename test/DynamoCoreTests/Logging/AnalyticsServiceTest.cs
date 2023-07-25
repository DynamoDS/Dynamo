using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Diagnostics.Runtime;
using NUnit.Framework;

namespace Dynamo.Tests.Loggings
{
    public class DynamoAnalyticsDisableTest
    {
        [Test,Category("FailureNET6")]//TODO this test requires finding ASM using the registry, will not run on linux.
        public void DisableAnalytics()
        {
            var versions = new List<Version>(){

                    new Version(229, 0,0),
                    new Version(228, 6, 0)
            };

            var directory = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            var testDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
            string openPath = Path.Combine(testDirectory, @"core\Angle.dyn");
            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process dynamoCLI = null;

            DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            try
            {
                Assert.DoesNotThrow(() =>
                {

                    dynamoCLI = Process.Start(new ProcessStartInfo(Path.Combine(coreDirectory, "DynamoCLI.exe"), $"--GeometryPath \"{locatedPath}\" -k --DisableAnalytics -o \"{openPath}\" ") { UseShellExecute = true });

                    Thread.Sleep(5000);// Wait 5 seconds to open the dyn
                    Assert.IsFalse(dynamoCLI.HasExited);
                    var dt = DataTarget.AttachToProcess(dynamoCLI.Id, false);
                    var assemblies = dt
                          .ClrVersions
                          .Select(dtClrVersion => dtClrVersion.CreateRuntime())
                          .SelectMany(runtime => runtime.AppDomains.SelectMany(runtimeAppDomain => runtimeAppDomain.Modules))
                          .Select(clrModule => clrModule.AssemblyName)
                          .Distinct()
                          .Where(x => x != null)
                          .ToList();

                    var firstASMmodulePath = string.Empty;
                    foreach (string module in assemblies)
                    {
                        if (module.IndexOf("Analytics", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            Assert.Fail("Analytics module was loaded");
                        }
                        if (module.IndexOf("AdpSDKCSharpWrapper", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            Assert.Fail("ADP module was loaded");
                        }
                    }
                });
            }
            finally
            {

                dynamoCLI?.Kill();
            }
        }
    }
}
