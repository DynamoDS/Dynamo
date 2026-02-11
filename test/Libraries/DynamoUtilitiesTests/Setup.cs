using System;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using Dynamo.Utilities;
using Microsoft.Win32;
using NUnit.Framework;

[SetUpFixture]
public class Setup
{
    private AssemblyHelper assemblyHelper;
    private DirectoryInfo tempDir;

    [OneTimeSetUp]
    public void RunBeforeAllTests()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var moduleRootFolder = new DirectoryInfo(assemblyPath).Parent;

        var resolutionPaths = new[]
        {
            // These tests need "CoreNodeModels.dll" under "nodes" folder.
            Path.Combine(moduleRootFolder.FullName, "nodes"),
            Path.Combine(moduleRootFolder.Parent.Parent.Parent.FullName, "test", "test_dependencies")
        };

        assemblyHelper = new AssemblyHelper(moduleRootFolder.FullName, resolutionPaths);
        AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;

        // Setup fake ASC installs
        tempDir = Directory.CreateTempSubdirectory();
        var majorVersion = tempDir.CreateSubdirectory(@"WorkingMajorVersion");
        majorVersion.CreateSubdirectory(@"1.0.0");
        RegistryKey key = Registry.CurrentUser;
        var workingMajorVersion = key.CreateSubKey(@"SOFTWARE\Autodesk\SharedComponents\WorkingMajorVersion");
        workingMajorVersion.SetValue(@"Version", @"1.0.0");
        workingMajorVersion.SetValue(@"InstallPath", majorVersion.FullName);

        var nonWorkingMajorVersion = key.CreateSubKey(@"SOFTWARE\Autodesk\SharedComponents\nonWorkingMajorVersion");
        nonWorkingMajorVersion.SetValue(@"Version", @"1.0.0");
        nonWorkingMajorVersion.SetValue(@"InstallPath", @"BadPath");
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
        assemblyHelper = null;
        tempDir.Delete(true);
        RegistryKey key = Registry.CurrentUser;
        key.DeleteSubKey(@"SOFTWARE\Autodesk\SharedComponents\WorkingMajorVersion");
        key.DeleteSubKey(@"SOFTWARE\Autodesk\SharedComponents\nonWorkingMajorVersion");
    }
}
