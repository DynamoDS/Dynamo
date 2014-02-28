using System;
using Dynamo.Python;
using IronPython.Hosting;
using Microsoft.Scripting;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
    internal class CodeCompletionTests
    {
       
        [Test]
        public void CanMatchBasicNumVarSingleLine()
        {
            var matchNumVar = "a = 5.0";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchNumVar, IronPythonCompletionProvider.doubleRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("5.0", matches["a"]);
        }

        [Test]
        public void CanMatchBasicArrayVarSingleLine()
        {
            var matchArray = "a = []";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchArray, IronPythonCompletionProvider.arrayRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("[]", matches["a"]);
        }

        [Test]
        public void CanMatchBasicDictVarSingleLine()
        {
            var matchDict = "a = {}";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchDict, IronPythonCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{}", matches["a"]);
        }

        [Test]
        public void CanMatchIntSingleLine()
        {
            var matchDict = "a = 2";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchDict, IronPythonCompletionProvider.intRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("2", matches["a"]);
        }

        [Test]
        public void CanMatchComplexDictVarSingleLine()
        {
            var matchDict2 = "a = { 'Alice': 7, 'Toby': 'Nuts' }";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchDict2, IronPythonCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);
        }

        [Test]
        public void CanMatchComplexDictVarMultiLine()
        {
            var matchDict2 = "\n\na = { 'Alice': 7, 'Toby': 'Nuts' }\nb = 5.0";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchDict2, IronPythonCompletionProvider.dictRegex);
            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("a"));
            Assert.AreEqual("{ 'Alice': 7, 'Toby': 'Nuts' }", matches["a"]);
        }

        [Test]
        public void DoesntMatchBadVariable()
        {
            var matchDict2 = "a! = { 'Alice': 7, 'Toby': 'Nuts' }";
            var matches = IronPythonCompletionProvider.FindVariableStatementWithRegex(matchDict2, IronPythonCompletionProvider.dictRegex);
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void CanMatchAllVariablesSingleLine()
        {
            var str = "a = { 'Alice': 7, 'Toby': 'Nuts' }";
            var completionProvider = new IronPythonCompletionProvider();

            var matches = completionProvider.FindAllVariables(str);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(typeof( IronPython.Runtime.PythonDictionary), matches["a"].Item3);
        }

        [Test]
        public void CanMatchAllVariableTypes()
        {
            var str = "a = { 'Alice': 7, 'Toby': 'Nuts' }\nb = {}\nc = 5.0\nd = 'pete'\ne = []";
            var completionProvider = new IronPythonCompletionProvider();

            var matches = completionProvider.FindAllVariables(str);

            Assert.AreEqual(5, matches.Count);
            Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["a"].Item3);
            Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["b"].Item3);
            Assert.AreEqual(typeof(double), matches["c"].Item3);
            Assert.AreEqual(typeof(string), matches["d"].Item3);
            Assert.AreEqual(typeof(IronPython.Runtime.List), matches["e"].Item3);
        }

        [Test]
        public void CanMatchBasicImportStatement()
        {
            var str = "import System";
            var matches = IronPythonCompletionProvider.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        public void CanMatchBasicImportStatementMultiLine()
        {
            var str = "\nimport System\n";
            var matches = IronPythonCompletionProvider.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches.ContainsKey("System"));
        }

        [Test]
        public void CanImportLibrary()
        {
            var str = "\nimport System\n";
            var completionProvider = new IronPythonCompletionProvider();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(1, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.ImportedTypes.ContainsKey("System"));
        }

        [Test]
        public void DuplicateCallsToImportShouldBeFine()
        {
            var str = "\nimport System\nimport System";
            var completionProvider = new IronPythonCompletionProvider();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(1, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.ImportedTypes.ContainsKey("System"));
        }

        [Test]
        public void CanImportSystemLibraryAndGetCompletionData()
        {
            var str = "\nimport System\nSystem.";
            var completionProvider = new IronPythonCompletionProvider();

            var completionData = completionProvider.GetCompletionData(str);

            Assert.AreEqual(224, completionData.Length);
            Assert.AreEqual(1, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.ImportedTypes.ContainsKey("System"));

        }

        [Test]
        public void CanImportSystemCollectionsLibraryAndGetCompletionData()
        {
            var str = "\nimport System.Collections\nSystem.Collections.";
            var completionProvider = new IronPythonCompletionProvider();

            var completionData = completionProvider.GetCompletionData(str);

            Assert.AreEqual(29, completionData.Length);
        }

        [Test]
        public void CanMatchImportSystemLibraryWithComment()
        {
            var str = "# Write your script here.\r\nimport System.";
            var matches = IronPythonCompletionProvider.FindBasicImportStatements(str);

            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public void CanMatchImportSystemAndLoadLibraryAndWithComment()
        {
            var str = "# Write your script here.\r\nimport System.";
            var completionProvider = new IronPythonCompletionProvider();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(1, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.ImportedTypes.ContainsKey("System"));
        }

        [Test]
        public void CanIdentifyVariableTypeAndGetCompletionData()
        {
            var str = "a = 5.0\na.";

            var completionProvider = new IronPythonCompletionProvider();
            var completionData = completionProvider.GetCompletionData(str);

            Assert.AreNotEqual(0, completionData.Length);

        }

        [Test]
        public void CanFindTypeSpecificImportsMultipleTypesSingleLine()
        {
            var str = "from math import sin, cos\n";

            var imports = IronPythonCompletionProvider.FindTypeSpecificImportStatements(str);
            Assert.IsTrue( imports.ContainsKey("sin") );
            Assert.AreEqual("from math import sin", imports["sin"]);
            Assert.IsTrue( imports.ContainsKey("cos") );
            Assert.AreEqual("from math import cos", imports["cos"]);

        }

        [Test]
        public void CanFindTypeSpecificImportsSingleTypeSingleLine()
        {
            var str = "from math import sin\n";

            var imports = IronPythonCompletionProvider.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("sin"));
            Assert.AreEqual("from math import sin", imports["sin"]);

        }

        [Test]
        public void CanFindTypeSpecificAutodeskImportsSingleTypeSingleLine()
        {
            var str = "from Autodesk.Revit.DB import Events\n";

            var imports = IronPythonCompletionProvider.FindTypeSpecificImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Events"));
            Assert.AreEqual("from Autodesk.Revit.DB import Events", imports["Events"]);

        }

        [Test]
        public void CanFindAllTypeImports()
        {
            var str = "from Autodesk.Revit.DB import *\n";

            var imports = IronPythonCompletionProvider.FindAllTypeImportStatements(str);
            Assert.IsTrue(imports.ContainsKey("Autodesk.Revit.DB"));
            Assert.AreEqual("from Autodesk.Revit.DB import *", imports["Autodesk.Revit.DB"]);

        }

        [Test]
        public void CanFindDifferentTypesOfImportsAndLoad()
        {
            var str = "from itertools import *\nimport math\nfrom sys import callstats\n";

            var completionProvider = new IronPythonCompletionProvider();
            completionProvider.UpdateImportedTypes(str);

            Assert.AreEqual(3, completionProvider.ImportedTypes.Count);
            Assert.IsTrue(completionProvider.Scope.ContainsVariable("repeat"));
            Assert.IsTrue(completionProvider.Scope.ContainsVariable("izip"));
            Assert.IsTrue(completionProvider.Scope.ContainsVariable("math"));
            Assert.IsTrue(completionProvider.Scope.ContainsVariable("callstats"));
        }

        [Test]
        public void CanFindSystemCollectionsAssignmentAndType()
        {
            var str = "from System.Collections import ArrayList\na = ArrayList()\n";
            var completionProvider = new IronPythonCompletionProvider();
            completionProvider.UpdateImportedTypes(str);
            completionProvider.UpdateVariableTypes(str);

            Assert.IsTrue(completionProvider.VariableTypes.ContainsKey("a"));
            Assert.AreEqual(typeof(System.Collections.ArrayList), completionProvider.VariableTypes["a"]);
        }

        [Test]
        public void CanGetCompletionDataForArrayListVariable()
        {
            var str = "from System.Collections import ArrayList\na = ArrayList()\na.";
            var completionProvider = new IronPythonCompletionProvider();
            var matches = completionProvider.GetCompletionData(str);

            Assert.AreNotEqual(0, matches.Length);
            //Assert.AreEqual(typeof(IronPython.Runtime.PythonDictionary), matches["a"].Item3);
        }
    }
}
