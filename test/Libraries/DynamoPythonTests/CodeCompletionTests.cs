using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DSCPython;
using DSIronPython;
using Dynamo.Logging;
using Dynamo.Python;
using Dynamo.Utilities;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using Python.Runtime;

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

            var completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.IronPython2);
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

             var completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.IronPython2);
             var completionList = completionData.Select(d => d.Text);
             Assert.IsTrue(completionList.Any());
             Assert.IsTrue(completionList.Intersect(new[] { "degrees", "radians", "fmod" }).Count() == 3);
             Assert.AreEqual(45, completionData.Length);

            // For CPython3 engine.
            provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.CPython3, dynCorePath);
            Assert.IsNotNull(provider);

            completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.CPython3);
            completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "degrees", "radians", "fmod" }).Count() == 3);
            Assert.AreEqual(60, completionData.Length);
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

            Assembly.LoadFrom(Path.Combine(moduleRootFolder, "DSCPython.dll"));
          //  Assembly.LoadFrom(Path.Combine(moduleRootFolder, "Python.Included.dll"));
          //  Assembly.LoadFrom(Path.Combine(moduleRootFolder, "Python.Runtime.dll"));

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
        public void CanMatchBasicNumVarSingleLine()
        {
            var matchNumVar = "a = 5.0";

            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchNumVar, SharedCompletionProvider.doubleRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("5.0", matches["a"]);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchNumVar, SharedCompletionProvider.doubleRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("5.0", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicArrayVarSingleLine()
        {
            var matchArray = "a = []";
            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchArray, SharedCompletionProvider.arrayRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("[]", matches["a"]);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchArray, SharedCompletionProvider.arrayRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("[]", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicDictVarSingleLine()
        {
            var matchDict = "a = {}";
            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{}", matches["a"]);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{}", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchIntSingleLine()
        {
            var matchDict = "a = 2";
            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict, SharedCompletionProvider.intRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("2", matches["a"]);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict, SharedCompletionProvider.intRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("2", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchComplexDictVarSingleLine()
        {
            var matchDict2 = "a = { 'Alice': 7, 'Toby': 'Nuts' }";
            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict2, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict2, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchComplexDictVarMultiLine()
        {
            var matchDict2 = "\n\na = { 'Alice': 7, 'Toby': 'Nuts' }\nb = 5.0";
            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict2, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict2, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void DoesntMatchBadVariable()
        {
            var matchDict2 = "a! = { 'Alice': 7, 'Toby': 'Nuts' }";
            var matches = IronPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict2, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(0, matches.Count);

            matches = DSCPythonCodeCompletionProviderCore.FindVariableStatementWithRegex(matchDict2, SharedCompletionProvider.dictRegex);
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchAllVariablesSingleLine()
        {
            var str = "a = { 'Alice': 7, 'Toby': 'Nuts' }";
            var ironPythonCompletionProvider = new IronPythonCodeCompletionProviderCore();
            var matches = ironPythonCompletionProvider.FindAllVariables(str);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["a"].Item3);

            var CPythonCompletionProvider = new DSCPythonCodeCompletionProviderCore();
            matches = CPythonCompletionProvider.FindAllVariables(str);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(typeof(Python.Runtime.PyDict), matches["a"].Item3);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchAllVariableTypes()
        {
            var str = "a = { 'Alice': 7, 'Toby': 'Nuts' }\nb = {}\nc = 5.0\nd = 'pete'\ne = []";
            var ironPythonCompletionProvider = new IronPythonCodeCompletionProviderCore();
            var matches = ironPythonCompletionProvider.FindAllVariables(str);

            Assert.AreEqual(5, matches.Count);
            Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["a"].Item3);
            Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["b"].Item3);
            Assert.AreEqual(typeof(double), matches["c"].Item3);
            Assert.AreEqual(typeof(string), matches["d"].Item3);
            Assert.AreEqual(typeof(IronPython.Runtime.List), matches["e"].Item3);

            var CPythonCompletionProvider = new DSCPythonCodeCompletionProviderCore();
            matches = CPythonCompletionProvider.FindAllVariables(str);

            Assert.AreEqual(5, matches.Count);
            Assert.AreEqual(typeof(Python.Runtime.PyDict), matches["a"].Item3);
            Assert.AreEqual(typeof(Python.Runtime.PyDict), matches["b"].Item3);
            Assert.AreEqual(typeof(double), matches["c"].Item3);
            Assert.AreEqual(typeof(string), matches["d"].Item3);
            Assert.AreEqual(typeof(Python.Runtime.PyList), matches["e"].Item3);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicImportStatement()
        {
            var str = "import System";
            var matches = IronPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));

            matches = DSCPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicImportStatementMultiLine()
        {
            var str = "\nimport System\n";
            var matches = IronPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));

            matches = DSCPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanImportLibrary()
        {
            var str = "\nimport System\n";
            var ironPythonCompletionProvider = new IronPythonCodeCompletionProviderCore();
            ironPythonCompletionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(2, ironPythonCompletionProvider.ImportedTypes.Count);
            Assert.IsTrue(ironPythonCompletionProvider.ImportedTypes.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void DuplicateCallsToImportShouldBeFine()
        {
            var str = "\nimport System\nimport System";
            var ironPythonCompletionProvider = new IronPythonCodeCompletionProviderCore();
            ironPythonCompletionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(2, ironPythonCompletionProvider.ImportedTypes.Count);
            Assert.IsTrue(defaultImports.SequenceEqual(ironPythonCompletionProvider.ImportedTypes.Keys.ToList()));
        }

        [Test]
        [Category("UnitTests")]
        public void CanImportSystemLibraryAndGetCompletionData()
        {
            var str = "\nimport System\nSystem.";
            var dynCorePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, dynCorePath);

            var completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.IronPython2);

            // Randomly verify some namepsaces are in the completion list
            var completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "IO", "Console", "Reflection" }).Count() == 3);
          //  Assert.AreEqual(2, provider.ImportedTypes.Count);
          //  Assert.IsTrue(defaultImports.SequenceEqual(completionProvider.ImportedTypes.Keys.ToList()));

            var completionProvider = new IronPythonCompletionProvider();

            completionData = completionProvider.GetCompletionData(str);

            // Randomly verify some namepsaces are in the completion list
            completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "IO", "Console", "Reflection" }).Count() == 3);
            Assert.AreEqual(2, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(defaultImports.SequenceEqual(completionProvider.ImportedTypes.Keys.ToList()));
        }

        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        public void CanImportSystemCollectionsLibraryAndGetCompletionData()
        {
            var str = "\nimport System.Collections\nSystem.Collections.";
            var dynCorePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, dynCorePath);

            var completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.IronPython2);
            var completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "Hashtable", "Queue", "Stack"}).Count() == 3);
            Assert.AreEqual(29, completionData.Length);

            provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.CPython3, dynCorePath);

            completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.CPython3);
            completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "Hashtable", "Queue", "Stack" }).Count() == 3);
            Assert.AreEqual(29, completionData.Length);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchImportSystemLibraryWithComment()
        {
            var str = "# Write your script here.\r\nimport System.";
            var matches = IronPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));

            matches = DSCPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchImportSystemAndLoadLibraryAndWithComment()
        {
            var str = "# Write your script here.\r\nimport System.";
            var ironPythonCompletionProvider = new IronPythonCodeCompletionProviderCore();
            ironPythonCompletionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(2, ironPythonCompletionProvider.ImportedTypes.Count);
            Assert.IsTrue(defaultImports.SequenceEqual(ironPythonCompletionProvider.ImportedTypes.Keys.ToList()));
        }

        [Test]
        [Category("UnitTests")]
        public void CanIdentifyVariableTypeAndGetCompletionData()
        {
            var str = "a = 5.0\na.";

            var dynCorePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, dynCorePath);
            var completionData = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.IronPython2);

            Assert.AreNotEqual(0, completionData.Length);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindTypeSpecificImportsMultipleTypesSingleLine()
        {
            var str = "from math import sin, cos\n";

            var imports = IronPythonCodeCompletionProviderCore.FindTypeSpecificImportStatements(str);
            Assert.IsTrue( imports.ContainsKey("sin") );
            Assert.AreEqual("from math import sin", imports["sin"]);
            Assert.IsTrue( imports.ContainsKey("cos") );
            Assert.AreEqual("from math import cos", imports["cos"]);


            imports = DSCPythonCodeCompletionProviderCore.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("sin"));
            Assert.AreEqual("from math import sin", imports["sin"]);
            Assert.IsTrue(imports.ContainsKey("cos"));
            Assert.AreEqual("from math import cos", imports["cos"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindTypeSpecificImportsSingleTypeSingleLine()
        {
            var str = "from math import sin\n";

            var imports = IronPythonCodeCompletionProviderCore.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("sin"));
            Assert.AreEqual("from math import sin", imports["sin"]);

            imports = DSCPythonCodeCompletionProviderCore.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("sin"));
            Assert.AreEqual("from math import sin", imports["sin"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindTypeSpecificAutodeskImportsSingleTypeSingleLine()
        {
            var str = "from Autodesk.Revit.DB import Events\n";

            var imports = IronPythonCodeCompletionProviderCore.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Events"));
            Assert.AreEqual("from Autodesk.Revit.DB import Events", imports["Events"]);

            imports = DSCPythonCodeCompletionProviderCore.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Events"));
            Assert.AreEqual("from Autodesk.Revit.DB import Events", imports["Events"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindAllTypeImports()
        {
            var str = "from Autodesk.Revit.DB import *\n";

            var imports = IronPythonCodeCompletionProviderCore.FindAllTypeImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Autodesk.Revit.DB"));
            Assert.AreEqual("from Autodesk.Revit.DB import *", imports["Autodesk.Revit.DB"]);

            imports = DSCPythonCodeCompletionProviderCore.FindAllTypeImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Autodesk.Revit.DB"));
            Assert.AreEqual("from Autodesk.Revit.DB import *", imports["Autodesk.Revit.DB"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindDifferentTypesOfImportsAndLoad()
        {
            var str = "from itertools import *\nimport math\nfrom sys import callstats\n";

            var ironPythonCodeCompletionProvider = new IronPythonCodeCompletionProviderCore();
            ironPythonCodeCompletionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(3, ironPythonCodeCompletionProvider.ImportedTypes.Count);
            Assert.IsTrue(((ScriptScope)ironPythonCodeCompletionProvider.Scope).ContainsVariable("repeat"));
            Assert.IsTrue(((ScriptScope)ironPythonCodeCompletionProvider.Scope).ContainsVariable("izip"));
            Assert.IsTrue(((ScriptScope)ironPythonCodeCompletionProvider.Scope).ContainsVariable("math"));
            Assert.IsTrue(((ScriptScope)ironPythonCodeCompletionProvider.Scope).ContainsVariable("callstats"));
        }

        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        public void CanFindSystemCollectionsAssignmentAndType()
        {
            var str = "from System.Collections import ArrayList\na = ArrayList()\n";
            var completionProvider = new IronPythonCodeCompletionProviderCore();
            completionProvider.UpdateImportedTypes(str);
            completionProvider.UpdateVariableTypes(str);

            Assert.IsTrue(completionProvider.VariableTypes.ContainsKey("a"));
            Assert.AreEqual(typeof(System.Collections.ArrayList), completionProvider.VariableTypes["a"]);

            var CPythonCompletionProvider = new DSCPythonCodeCompletionProviderCore();
            CPythonCompletionProvider.UpdateImportedTypes(str);
            CPythonCompletionProvider.UpdateVariableTypes(str);

            Assert.IsTrue(CPythonCompletionProvider.VariableTypes.ContainsKey("a"));
            Assert.AreEqual(typeof(System.Collections.ArrayList), CPythonCompletionProvider.VariableTypes["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanGetCompletionDataForArrayListVariable()
        {
            var dynCorePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var str = "from System.Collections import ArrayList\na = ArrayList()\na.";

            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.IronPython2, dynCorePath);
            var matches = provider.GetCompletionData(str, PythonNodeModels.PythonEngineVersion.IronPython2);

            Assert.AreNotEqual(0, matches.Length);
            //Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["a"].Item3);
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
