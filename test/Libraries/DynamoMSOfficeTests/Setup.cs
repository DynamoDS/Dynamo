using System;
using System.IO;
using System.Reflection;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [SetUpFixture]
    public class Setup
    {
        private AssemblyHelper assemblyHelper;

#if NETFRAMEWORK
    [SetUp]
#elif NET6_0_OR_GREATER
        [OneTimeSetUp]
#endif
        public void RunBeforeAllTests()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var moduleRootFolder = Path.GetDirectoryName(assemblyPath);

            var resolutionPaths = new[]
            {
                // These tests need "CoreNodeModels.dll" under "nodes" folder.
                Path.Combine(moduleRootFolder, "nodes")
            };

            assemblyHelper = new AssemblyHelper(moduleRootFolder, resolutionPaths);
            AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
        }

#if NETFRAMEWORK
    [TearDown]
#elif NET6_0_OR_GREATER
        [OneTimeTearDown]
#endif
        public void RunAfterAllTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
        }
    }
}
