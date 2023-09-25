using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DSCPython;
using Dynamo;
using Dynamo.Python;
using Dynamo.PythonServices;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
    internal class SharedCodeCompletionProviderTests : UnitTestBase
    {
        public override void Setup()
        {
            base.Setup();
            //for some legacy tests we'll need the DSCPython binary loaded manually
            //as the types are found using reflection - during normal dynamo use these types are already loaded. 
            Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DSCPython.dll"));
        }

        [Test]
        public void SharedCoreCanFindLoadedProviders()
        {
            var provider = new SharedCompletionProvider(PythonEngineManager.CPython3EngineName, "");
            Assert.IsNotNull(provider);
        }

        [Test]
        public void SharedCoreCanReturnCLRCompletionData()
        {
            var provider = new SharedCompletionProvider(PythonEngineManager.CPython3EngineName, "");
            Assert.IsNotNull(provider);
            var str = "\nimport System.Collections\nSystem.Collections.";

            var completionData = provider.GetCompletionData(str);
            var completionList = completionData.Select(d => d.Text);

            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "Hashtable", "Queue", "Stack" }).Count() == 3);
            Assert.IsTrue(completionData.Length == 31);
        }
    }

    [TestFixture]
    internal class CodeCompletionTests
    {
        // List of expected default imported types
        private List<string> defaultImports = new List<string>()
        {
            "None",
            "System"
        };

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicNumVarSingleLine()
        {
            var matchNumVar = "a = 5.0";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchNumVar, PythonCodeCompletionProviderCommon.doubleRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("5.0", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicArrayVarSingleLine()
        {
            var matchArray = "a = []";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchArray, PythonCodeCompletionProviderCommon.arrayRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("[]", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicDictVarSingleLine()
        {
            var matchDict = "a = {}";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchDict, PythonCodeCompletionProviderCommon.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{}", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchIntSingleLine()
        {
            var matchDict = "a = 2";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchDict, PythonCodeCompletionProviderCommon.intRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("2", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchComplexDictVarSingleLine()
        {
            var matchDict2 = "a = { 'Alice': 7, 'Toby': 'Nuts' }";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchDict2, PythonCodeCompletionProviderCommon.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchComplexDictVarMultiLine()
        {
            var matchDict2 = "\n\na = { 'Alice': 7, 'Toby': 'Nuts' }\nb = 5.0";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchDict2, PythonCodeCompletionProviderCommon.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void DoesntMatchBadVariable()
        {
            var matchDict2 = "a! = { 'Alice': 7, 'Toby': 'Nuts' }";
            var matches = PythonCodeCompletionProviderCommon.FindVariableStatementWithRegex(matchDict2, PythonCodeCompletionProviderCommon.dictRegex);
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchAllVariablesSingleLine()
        {
            var str = "a = { 'Alice': 7, 'Toby': 'Nuts' }";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();

            var matches = completionProvider.FindAllVariables(str);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(typeof(Python.Runtime.PyDict), matches["a"].Item3);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchAllVariableTypes()
        {
            var str = "a = { 'Alice': 7, 'Toby': 'Nuts' }\nb = {}\nc = 5.0\nd = 'pete'\ne = []";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();

            var matches = completionProvider.FindAllVariables(str);

            Assert.AreEqual(5, matches.Count);
            Assert.AreEqual(typeof(Python.Runtime.PyDict), matches["a"].Item3);
            Assert.AreEqual(typeof(Python.Runtime.PyDict), matches["b"].Item3);
            Assert.AreEqual(typeof(Python.Runtime.PyFloat), matches["c"].Item3);
            Assert.AreEqual(typeof(Python.Runtime.PyString), matches["d"].Item3);
            Assert.AreEqual(typeof(Python.Runtime.PyList), matches["e"].Item3);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicImportStatement()
        {
            var str = "import System";
            var matches = PythonCodeCompletionProviderCommon.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchBasicImportStatementMultiLine()
        {
            var str = "\nimport System\n";
            var matches = PythonCodeCompletionProviderCommon.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanImportLibrary()
        {
            var str = "\nimport System\n";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(2, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.ImportedTypes.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void DuplicateCallsToImportShouldBeFine()
        {
            var str = "\nimport System\nimport System";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(2, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(defaultImports.SequenceEqual(completionProvider.ImportedTypes.Keys.ToList()));
        }

        [Test]
        [Category("UnitTests")]
        public void CanImportSystemLibraryAndGetCompletionData()
        {
            var str = "\nimport System\nSystem.";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();

            var completionData = completionProvider.GetCompletionData(str);

            // Randomly verify some namepsaces are in the completion list
            var completionList = completionData.Select(d => d.Text);
            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "IO", "Console", "Reflection" }).Count() == 3);
            Assert.AreEqual(2, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(defaultImports.SequenceEqual(completionProvider.ImportedTypes.Keys.ToList()));
        }

        [Test]
        [Category("UnitTests")]
        public void CanImportSystemCollectionsLibraryAndGetCompletionData()
        {
            var str = "\nimport System.Collections\nSystem.Collections.";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();

            var completionData = completionProvider.GetCompletionData(str);
            var completionList = completionData.Select(d => d.Text);

            Assert.IsTrue(completionList.Any());
            Assert.IsTrue(completionList.Intersect(new[] { "Hashtable", "Queue", "Stack" }).Count() == 3);
            Assert.IsTrue(completionData.Length == 31);
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchImportSystemLibraryWithComment()
        {
            var str = "# Write your script here.\r\nimport System.";
            var matches = DSCPythonCodeCompletionProviderCore.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanMatchImportSystemAndLoadLibraryAndWithComment()
        {
            var str = "# Write your script here.\r\nimport System.";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(2, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(defaultImports.SequenceEqual(completionProvider.ImportedTypes.Keys.ToList()));
        }

        [Test]
        [Category("UnitTests")]
        public void CanIdentifyVariableTypeAndGetCompletionData()
        {
            var str = "a = 5.0\na.";

            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            var completionData = completionProvider.GetCompletionData(str);

            Assert.AreNotEqual(0, completionData.Length);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindTypeSpecificImportsMultipleTypesSingleLine()
        {
            var str = "from math import sin, cos\n";

            var imports = PythonCodeCompletionProviderCommon.FindTypeSpecificImportStatements(str);
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

            var imports = PythonCodeCompletionProviderCommon.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("sin"));
            Assert.AreEqual("from math import sin", imports["sin"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindTypeSpecificAutodeskImportsSingleTypeSingleLine()
        {
            var str = "from Autodesk.Revit.DB import Events\n";

            var imports = PythonCodeCompletionProviderCommon.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Events"));
            Assert.AreEqual("from Autodesk.Revit.DB import Events", imports["Events"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindAllTypeImports()
        {
            var str = "from Autodesk.Revit.DB import *\n";

            var imports = PythonCodeCompletionProviderCommon.FindAllTypeImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Autodesk.Revit.DB"));
            Assert.AreEqual("from Autodesk.Revit.DB import *", imports["Autodesk.Revit.DB"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindDifferentTypesOfImportsAndLoad()
        {
            var str = "from itertools import *\nimport math\nfrom sys import exit\n";

            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            try
            {
                completionProvider.UpdateImportedTypes(str);
            }
            catch(Exception e) { Console.WriteLine(e.Message); }
            

            Assert.AreEqual(3, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.ScopeHasVariable("repeat"));
            Assert.IsTrue(completionProvider.ScopeHasVariable("math"));
            Assert.IsTrue(completionProvider.ScopeHasVariable("exit"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanFindSystemCollectionsAssignmentAndType()
        {
            var str = "from System.Collections import ArrayList\na = ArrayList()\n";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            completionProvider.UpdateImportedTypes(str);
            completionProvider.UpdateVariableTypes(str);

            Assert.IsTrue(completionProvider.VariableTypes.ContainsKey("a"));
            Assert.AreEqual(typeof(System.Collections.ArrayList), completionProvider.VariableTypes["a"]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanGetCompletionDataForArrayListVariable()
        {
            var str = "from System.Collections import ArrayList\na = ArrayList()\na.";
            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            var matches = completionProvider.GetCompletionData(str);

            var matchedTexts = matches.Select(x => x.Text);
            var arrayListMembers = typeof(System.Collections.ArrayList).GetMembers()
                .Select(x => x.Name);
            var commonMembers = arrayListMembers.Intersect(matchedTexts).ToList();

            Assert.AreNotEqual(0, matches.Length);
            Assert.AreEqual(matches.Length, commonMembers.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanGetCompletionDataForPyBuiltInTypes()
        {
            var completionProvider = new DSCPythonCodeCompletionProviderCore();
            var code = "str.";
            var matches = completionProvider.GetCompletionData(code);
            Assert.AreNotEqual(0, matches.Length);
            Assert.IsTrue(matches.Any(x => x.Text.Contains("capitalize")));
            Assert.IsTrue(matches.Any(x => x.Text.Contains("count")));

            code = "sys.";
            matches = completionProvider.GetCompletionData(code);
            Assert.AreNotEqual(0, matches.Length);
            Assert.IsTrue(matches.Any(x => x.Text.Contains("path")));
            Assert.IsTrue(matches.Any(x => x.Text.Contains("modules")));

            code = "clr.";
            matches = completionProvider.GetCompletionData(code);
            Assert.AreNotEqual(0, matches.Length);
            Assert.IsTrue(matches.Any(x => x.Text.Contains("GetClrType")));
            Assert.IsTrue(matches.Any(x => x.Text.Contains("AddReference")));
        }

        [Test]
        [Category("UnitTests")]
        public void VerifyPythonLoadedAssemblies()
        {
            // Verify IronPython assebmlies are loaded a single time
            List<string> matches = new List<string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("DSCPython"))
                {
                    if (matches.Contains(assembly.FullName))
                    {
                        Assert.Fail("Attempted to load an DSCPython assembly multiple times: " + assembly.FullName);
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
