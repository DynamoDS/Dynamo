using System;
using System.IO;
using System.Reflection;
using Dynamo.Utilities;
using NUnit.Framework;


[SetUpFixture]
public class Setup
{
    private AssemblyHelper assemblyHelper;

    [SetUp]
    public void SetUp()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var moduleRootFolder = new DirectoryInfo(assemblyPath).Parent;

        var resolutionPaths = new[]
        {
            // These tests need "CoreNodeModels.dll" under "nodes" folder.
            Path.Combine(moduleRootFolder.FullName, "nodes"),
            Path.Combine(moduleRootFolder.Parent.Parent.Parent.FullName, "test", "test_dependencies")

        };

        assemblyHelper = new AssemblyHelper(moduleRootFolder.FullName, resolutionPaths, true);
        AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
    }

    [TearDown]
    public void TearDown()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
        assemblyHelper = null;
    }
}
