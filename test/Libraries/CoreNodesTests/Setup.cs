using System;
using System.IO;
using System.Reflection;
using Dynamo;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [SetUpFixture]
    public class Setup
    {
        private AssemblyHelper assemblyHelper;

        [SetUp]
        public void SetUp()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var moduleRootFolder = Path.GetDirectoryName(assemblyPath);

            var resolutionPaths = new[]
            {
                // These tests need "DSCoreNodesUI.dll" under "nodes" folder.
                Path.Combine(moduleRootFolder, "nodes")
            };

            assemblyHelper = new AssemblyHelper(moduleRootFolder, resolutionPaths);
            AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
        }

        [TearDown]
        public void TearDown()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
        }
    }
}
