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
        public void RunBeforeAllTests()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var moduleRootFolder = new DirectoryInfo(assemblyPath).Parent;

            var resolutionPaths = new[]
            {
                // These tests need "CoreNodeModelsWpf.dll" under "nodes" folder.
                Path.Combine(moduleRootFolder.FullName, "nodes"),
                Path.Combine(moduleRootFolder.Parent.Parent.Parent.FullName, "test", "test_dependencies")

            };

            assemblyHelper = new AssemblyHelper(moduleRootFolder.FullName, resolutionPaths);
            AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
        }

        [TearDown]
        public void RunAfterAllTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
        }
    }
