using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Diagnostics.Runtime;
using NUnit.Framework;
using System.Runtime.Versioning;

namespace Dynamo.Tests.Loggings
{
    public class DynamoAnalyticsDisableTest
    {
        [Test]
        [Platform("win")]//nunit attribute for now only run on windows until we know it's useful on linux.
        public void DisableAnalytics()
        {
            var versions = new List<Version>(){

                    new Version(230, 0,0),
            };

            var directory = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            var testDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
            string openPath = Path.Combine(testDirectory, @"core\Angle.dyn");
            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process dynamoCLI = null;
            //TODO an approach we could take to get this running on linux.
            //unclear if this needs to be compiled with an ifdef or runtime is ok.
            //related to https://jira.autodesk.com/browse/DYN-5705
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                DynamoShapeManager.Utilities.SearchForASMInLibGFallback(versions, ref locatedPath, coreDirectory, out _);
            }
            else
            {
                DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            }
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

        [Test]
        [Platform("win")]//nunit attribute for now only run on windows until we know it's useful on linux.
        public void DisableAnalyticsViaNoNetWorkMode()
        {
            var versions = new List<Version>(){

                    new Version(230, 0,0),
            };

            var directory = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            var testDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
            string openPath = Path.Combine(testDirectory, @"core\Angle.dyn");
            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process dynamoCLI = null;
            //TODO an approach we could take to get this running on linux.
            //unclear if this needs to be compiled with an ifdef or runtime is ok.
            //related to https://jira.autodesk.com/browse/DYN-5705
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                DynamoShapeManager.Utilities.SearchForASMInLibGFallback(versions, ref locatedPath, coreDirectory, out _);
            }
            else
            {
                DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            }
            try
            {
                Assert.DoesNotThrow(() =>
                {

                    dynamoCLI = Process.Start(new ProcessStartInfo(Path.Combine(coreDirectory, "DynamoCLI.exe"), $"--GeometryPath \"{locatedPath}\" -k --NoNetworkMode -o \"{openPath}\" ") { UseShellExecute = true });

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
