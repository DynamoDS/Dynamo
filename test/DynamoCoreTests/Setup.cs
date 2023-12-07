using System;
using System.IO;
using System.Reflection;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;

    [SetUpFixture]
    public class Setup
    {
        private AssemblyHelper assemblyHelper;

        private bool NodeModelAssemblyLoader_shouldLoadAssemblyPath(string assemblyPath)
        {
            if (assemblyPath.Contains("WPF", StringComparison.OrdinalIgnoreCase) || assemblyPath.Contains("UI", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

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
            NodeModelAssemblyLoader.shouldLoadAssemblyPath += NodeModelAssemblyLoader_shouldLoadAssemblyPath;
        }

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
            NodeModelAssemblyLoader.shouldLoadAssemblyPath -= NodeModelAssemblyLoader_shouldLoadAssemblyPath;
        }
    }
