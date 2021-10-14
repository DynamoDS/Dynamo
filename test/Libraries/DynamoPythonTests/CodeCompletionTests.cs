using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Logging;
using Dynamo.Python;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
    internal class SharedCodeCompletionProviderTests: CodeCompletionTests
    {
        [Test]
        public void SharedCoreCanFindLoadedProviders()
        {
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, "");
            Assert.IsNotNull(provider);
        }

        [Test]
        public void SharedCoreCanFindFirstLoadedIfNotMatch()
        {
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.CPython3, "");
            Assert.IsNotNull(provider);
        }

        [Test]
        public void SharedCoreCanReturnCLRCompletionData()
        {
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, "");
            Assert.IsNotNull(provider);
            var str = "\nimport System.Collections\nSystem.Collections.";

            var completionData = provider.GetCompletionData(str);
            var completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "Hashtable", "Queue", "Stack" }).Count() == 3);
            Assert.AreEqual(29, completionData.Length);
        }

        [Test]
        public void SharedCoreCanReturnPythonCompletionData()
        {
            var dynCorePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, dynCorePath);
            Assert.IsNotNull(provider);
            var str = "import math\n math.";

            var completionData = provider.GetCompletionData(str);
            var completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "degrees", "radians", "fmod" }).Count() == 3);
            Assert.AreEqual(45, completionData.Length);
        }

    }

    [TestFixture]
    internal class CodeCompletionTests
    {
        private AssemblyHelper assemblyHelper;

        private class SimpleLogger : ILogger
        {

            public string LogPath
            {
                get { return ""; }
            }

            public void Log(string message)
            {
                
            }

            public void Log(string message, LogLevel level)
            {
                
            }

            public void Log(string tag, string message)
            {
                
            }

            public void LogError(string error)
            {
                
            }

            public void LogWarning(string warning, WarningLevel level)
            {
                
            }

            public void Log(Exception e)
            {
                
            }

            public void ClearLog()
            {
                
            }

            public string LogText
            {
                get { return ""; }
            }

            public string Warning
            {
                get
                {
                    return "";
                }
                set { return; }
            }
        }

        private ILogger logger;

        // List of expected default imported types
        private List<string> defaultImports = new List<string>()
        {
            "None",
            "System"
        };

        [SetUp]
        public void SetupPythonTests()
        {
            this.logger = new SimpleLogger();

            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var moduleRootFolder = Path.GetDirectoryName(assemblyPath);

            var resolutionPaths = new[]
            {
                // These tests need "DSIronPythonNode.dll" under "nodes" folder.
                Path.Combine(moduleRootFolder, "nodes")
            };
            //for some legacy tests we'll need the DSIronPython binary loaded manually
            //as the types are found using reflection - during normal dynamo use these types are already loaded. 
            Assembly.LoadFrom(Path.Combine(moduleRootFolder, "DSIronPython.dll"));

            assemblyHelper = new AssemblyHelper(moduleRootFolder, resolutionPaths);
            AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
        }

        [TearDown]
        public void RunAfterAllTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
        }

        [Test]
        [Category("UnitTests")]
        public void VerifyIronPythonLoadedAssemblies()
        {
            // Verify IronPython assebmlies are loaded a single time
            List<string> matches = new List<string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if(assembly.FullName.StartsWith("IronPython"))
                {
                    if(matches.Contains(assembly.FullName))
                    {
                        Assert.Fail("Attempted to load an IronPython assembly multiple times: " + assembly.FullName);
                    }
                    else
                    {
                        matches.Add(assembly.FullName);
                    }
                }
            }
        }
    }
}
