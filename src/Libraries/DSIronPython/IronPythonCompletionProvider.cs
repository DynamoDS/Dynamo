using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dynamo.Logging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;

namespace Dynamo.Python
{
    /// <summary>
    /// Provides code completion for the Python Editor
    /// </summary>
    public class IronPythonCompletionProvider : LogSourceBase
    {

        #region Properties and fields

        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
        private ScriptEngine engine;
        public ScriptEngine Engine
        {
            get { return engine; }
            set { engine = value; }
        }

        /// <summary>
        /// The scope used by the engine.  This is where all the loaded symbols
        /// are stored.  It's essentially an environment dictionary.
        /// </summary>
        private ScriptScope scope;
        public ScriptScope Scope
        {
            get { return scope; }
            set { scope = value; }
        }

        /// <summary>
        /// Store a reference to the Dynamo Core Python Standard Library directory
        /// </summary>
        private string pythonLibDir { get; set; }

        /// <summary>
        /// A list of short assembly names used with the TryGetTypeFromFullName method
        /// </summary>
        private static string[] knownAssemblies = {
            "mscorlib",
            "RevitAPI",
            "RevitAPIUI",
            "ProtoGeometry"
        };

        /// <summary>
        /// Already discovered variable types
        /// </summary>
        public Dictionary<string, Type> VariableTypes { get; set; }

        /// <summary>
        /// This will eventually be used for multi-threading.
        /// </summary>
        internal volatile bool AutocompletionInProgress = false;

        /// <summary>
        /// Types that have already been imported into the scope
        /// </summary>
        public Dictionary<string, Type> ImportedTypes { get; set; }

        /// <summary>
        /// Maps a regex to a particular python type.  Useful for matching things like dicts,
        /// floats, strings, etc.  Initialized by
        /// </summary>
        [Obsolete("This property has been replaced with BasicVariableTypes and will be removed in Dynamo 3.0")]
        public Dictionary<string, Type> RegexToType = new Dictionary<string, Type>();

        /// <summary>
        /// Maps a basic variable regex to a basic python type.
        /// </summary>
        internal List<Tuple<Regex, Type>> BasicVariableTypes;

        /// <summary>
        /// Tracks already referenced CLR modules
        /// </summary>
        internal HashSet<string> clrModules { get; set; }

        /// <summary>
        /// Keeps track of failed statements to avoid poluting the log
        /// </summary>
        internal Dictionary<string, int> badStatements { get; set; }

        /// <summary>
        /// Maps a basic variable regex to a basic python type.
        /// </summary>
        // TODO - these can all be internal constants - Dynamo 3.0
        public static string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s?)+)";
        public static string variableName = @"([0-9a-zA-Z_]+(\.[a-zA-Z_0-9]+)*)";
        public static string doubleQuoteStringRegex = "(\"[^\"]*\")"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        public static string singleQuoteStringRegex = "(\'[^\']*\')"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        public static string arrayRegex = "(\\[.*\\])";
        public static string spacesOrNone = @"(\s*)";
        public static string atLeastOneSpaceRegex = @"(\s+)";
        public static string equals = @"(=)"; // Not CLS compliant - replaced with equalsRegex - Remove in Dynamo 3.0
        public static string dictRegex = "({.*})";
        public static string doubleRegex = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
        public static string intRegex = @"([-+]?\d+)[\s\n]*$";
        public static string basicImportRegex = @"(import)";
        public static string fromImportRegex = @"^(from)";

        internal const string quotesStringRegex = "[\"']([^\"']*)[\"']";
        internal const string equalsRegex = @"(=)";

        internal static readonly Regex MATCH_LAST_NAMESPACE = new Regex(@"[\w.]+$", RegexOptions.Compiled);
        internal static readonly Regex MATCH_LAST_WORD = new Regex(@"\w+$", RegexOptions.Compiled);
        internal static readonly Regex MATCH_FIRST_QUOTED_NAME = new Regex(quotesStringRegex, RegexOptions.Compiled);
        internal static readonly Regex MATCH_VALID_TYPE_NAME_CHARACTERS_ONLY = new Regex(@"^\w+", RegexOptions.Compiled);
        internal static readonly Regex TRIPPLE_QUOTE_STRINGS = new Regex(".*?\\\"{{3}}[\\s\\S]+?\\\"{{3}}", RegexOptions.Compiled);

        internal static readonly Regex MATCH_IMPORT_STATEMENTS = new Regex(@"^import\s+?(.+)", RegexOptions.Compiled | RegexOptions.Multiline);
        internal static readonly Regex MATCH_FROM_IMPORT_STATEMENTS = new Regex(@"from\s+?([\w.]+)\s+?import\s+?([\w, *]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        internal static readonly Regex MATCH_VARIABLE_ASSIGNMENTS = new Regex(@"^[ \t]*?(\w+(\s*?,\s*?\w+)*)\s*?=\s*(.+)", RegexOptions.Compiled | RegexOptions.Multiline);

        internal static readonly Regex STRING_VARIABLE = new Regex("[\"']([^\"']*)[\"']", RegexOptions.Compiled);
        internal static readonly Regex DOUBLE_VARIABLE = new Regex("^-?\\d+\\.\\d+", RegexOptions.Compiled);
        internal static readonly Regex INT_VARIABLE = new Regex("^-?\\d+", RegexOptions.Compiled);
        internal static readonly Regex LIST_VARIABLE = new Regex("\\[.*\\]", RegexOptions.Compiled);
        internal static readonly Regex DICT_VARIABLE = new Regex("{.*}", RegexOptions.Compiled);

        internal static readonly string BAD_ASSIGNEMNT_ENDS = ",([{";

        #endregion

        #region Constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        [Obsolete("Use additional constructor passing the Dynamo Core Directory to enable Python Std Lib autocomplete.")]
        public IronPythonCompletionProvider() : this("")
        {
            // TODO - remove this constructor in Dynamo 3.0)
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public IronPythonCompletionProvider(string dynamoCoreDir)
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            scope = engine.CreateScope();

            VariableTypes = new Dictionary<string, Type>();
            ImportedTypes = new Dictionary<string, Type>();
            clrModules = new HashSet<string>();
            badStatements = new Dictionary<string, int>();

            // Special case for python variables defined as null
            ImportedTypes["None"] = null;

            BasicVariableTypes = new List<Tuple<Regex, Type>>();

            BasicVariableTypes.Add(Tuple.Create(STRING_VARIABLE, typeof(string)));
            BasicVariableTypes.Add(Tuple.Create(DOUBLE_VARIABLE, typeof(double)));
            BasicVariableTypes.Add(Tuple.Create(INT_VARIABLE, typeof(int)));
            BasicVariableTypes.Add(Tuple.Create(LIST_VARIABLE, typeof(IronPython.Runtime.List)));
            BasicVariableTypes.Add(Tuple.Create(DICT_VARIABLE, typeof(PythonDictionary)));

            // Main CLR module
            engine.CreateScriptSourceFromString("import clr\n", SourceCodeKind.SingleStatement).Execute(scope);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Determine if the Revit API is available in the given context
            if (assemblies.Any(x => x.GetName().Name == "RevitAPI"))
            {
                // Try to load Revit Type for autocomplete
                try
                {
                    var revitImports =
                        "clr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\nimport Autodesk\n";

                    engine.CreateScriptSourceFromString(revitImports, SourceCodeKind.Statements).Execute(scope);
                    clrModules.Add("RevitAPI");
                    clrModules.Add("RevitAPIUI");
                }
                catch
                {
                    Log("Failed to load Revit types for autocomplete. Python autocomplete will not see Autodesk namespace types.");
                }
            }

            // Determine if the ProtoGeometry is available in the given context
            if (assemblies.Any(x => x.GetName().Name == "ProtoGeometry"))
            {
                // Try to load ProtoGeometry Type for autocomplete
                try
                {
                    var libGImports =
                        "clr.AddReference('ProtoGeometry')\nfrom Autodesk.DesignScript.Geometry import *\n";

                    engine.CreateScriptSourceFromString(libGImports, SourceCodeKind.Statements).Execute(scope);
                    clrModules.Add("ProtoGeometry");
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    Log("Failed to load ProtoGeometry types for autocomplete. Python autocomplete will not see Autodesk namespace types.");
                }
            }

            // Determine if the Python Standard Library is available in the given context
            if(!String.IsNullOrEmpty(dynamoCoreDir))
            {
                pythonLibDir = Path.Combine(dynamoCoreDir, @"IronPython.StdLib.2.7.8");
            }

            if (!String.IsNullOrEmpty(pythonLibDir) && Directory.Exists(pythonLibDir))
            {
                // Try to load Python Standard Library Type for autocomplete
                try
                {
                    var pyLibImports = String.Format("import sys\nsys.path.append(r'{0}')\n", pythonLibDir);
                    engine.CreateScriptSourceFromString(pyLibImports, SourceCodeKind.Statements).Execute(scope);
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    Log("Failed to register IronPython's native library. Python autocomplete will not see standard modules.");
                }
            }
            else
            {
                Log("Valid IronPython Standard Library not found. Python autocomplete will not see native modules.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates completion data for the specified text, while import the given types into the 
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="line">The code to parse</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        [Obsolete("Please use GetCompletionData with additional parameters, this method will be removed in Dynamo 3.0.")]
        public ICompletionData[] GetCompletionData(string line)
        {
            return GetCompletionData(line, false);
        }

        /// <summary>
        /// Generate completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="expand">Determines if the entire namespace should be used</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        public ICompletionData[] GetCompletionData(string code, bool expand = false)
        {
            var items = new List<IronPythonCompletionData>();

            if (code.Contains("\"\"\""))
            {
                code = StripDocStrings(code);
            }

            UpdateImportedTypes(code);
            UpdateVariableTypes(code);  // Possibly use hindley-milner in the future

            // If expand param is true use the entire namespace from the line of code
            // Else just return the last name of the namespace
            string name = expand ? GetLastNameSpace(code) : GetLastName(code);
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    AutocompletionInProgress = true;
                    
                    // Attempt to get type using naming
                    Type type = expand ? TryGetTypeFromFullName(name) : TryGetType(name);

                    // CLR type
                    if (type != null)
                    {
                        items = EnumerateMembers(type, name);
                    }
                    // Variable type
                    else if (VariableTypes.TryGetValue(name, out type))
                    {
                        items = EnumerateMembers(type, name);
                    }
                    else
                    {
                        var mem = LookupMember(name);
                        var namespaceTracker = mem as NamespaceTracker;

                        // Namespace type
                        if (namespaceTracker != null)
                        {
                            items = EnumerateMembers(namespaceTracker, name);
                        }
                        else
                        {
                            var pythonModule = mem as PythonModule;

                            // Python Module type
                            if (pythonModule != null)
                            {
                                items = EnumerateMembers(pythonModule, name);
                            }
                            // Python type
                            else if (mem is PythonType)
                            {
                                // Shows static and instance methods in the same way :(
                                var value = ClrModule.GetClrType(mem as PythonType);

                                if (value != null)
                                {
                                    items = EnumerateMembers(value, name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }

                AutocompletionInProgress = false;
            }

            // If unable to find matching results and expand was set to false,
            // try again using the full namespace (set expand to true)
            if (!items.Any() && !expand)
            {
                return GetCompletionData(code, true);
            }

            return items.ToArray();
        }

        /// <summary>
        /// List all of the members in a PythonModule
        /// </summary>
        /// <param name="module">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the module</returns>
        public List<IronPythonCompletionData> EnumerateMembers(PythonModule module, string name)
        {
            var items = new List<IronPythonCompletionData>();
            var d = module.Get__dict__();

            foreach (var member in d)
            {
                var completionType = member.Value is BuiltinFunction ? IronPythonCompletionData.CompletionType.METHOD : IronPythonCompletionData.CompletionType.FIELD;
                items.Add(new IronPythonCompletionData((string)member.Key, name, false, completionType, this));
            }

            return items;
        }

        /// <summary>
        /// List all of the members in a CLR Namespace
        /// </summary>
        /// <param name="ns">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the namespace</returns>
        public List<IronPythonCompletionData> EnumerateMembers(NamespaceTracker ns, string name)
        {
            var items = new List<IronPythonCompletionData>();

            foreach (var member in ns)
            {
                if (member.Value is NamespaceTracker)
                {
                    items.Add(new IronPythonCompletionData(member.Key, name, false, IronPythonCompletionData.CompletionType.NAMESPACE, this));
                }
                else if (member.Value is FieldTracker)
                {
                    items.Add(new IronPythonCompletionData(member.Key, name, false, IronPythonCompletionData.CompletionType.FIELD, this));
                }
                else if (member.Value is PropertyTracker)
                {
                    items.Add(new IronPythonCompletionData(member.Key, name, false, IronPythonCompletionData.CompletionType.PROPERTY, this));
                }
                else if (member.Value is TypeTracker)
                {
                    items.Add(new IronPythonCompletionData(member.Key, name, false, IronPythonCompletionData.CompletionType.CLASS, this));
                }
            }

            return items;
        }

        /// <summary>
        /// List all of the members in a CLR type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name for the type</param>
        /// <returns>A list of completion data for the type</returns>
        protected List<IronPythonCompletionData> EnumerateMembers(Type type, string name)
        {
            var items = new List<IronPythonCompletionData>();

            var completionsList = new SortedList<string, IronPythonCompletionData.CompletionType>();

            var methodInfo = type.GetMethods();
            var propertyInfo = type.GetProperties();
            var fieldInfo = type.GetFields();

            foreach (MethodInfo methodInfoItem in methodInfo)
            {
                if ( methodInfoItem.IsPublic
                    && (methodInfoItem.Name.IndexOf("get_") != 0) && (methodInfoItem.Name.IndexOf("set_") != 0)
                    && (methodInfoItem.Name.IndexOf("add_") != 0) && (methodInfoItem.Name.IndexOf("remove_") != 0)
                    && (methodInfoItem.Name.IndexOf("__") != 0) )
                {
                    if (!completionsList.ContainsKey(methodInfoItem.Name))
                    {
                        completionsList.Add(methodInfoItem.Name, IronPythonCompletionData.CompletionType.METHOD);
                    }
                }
            }

            foreach (PropertyInfo propertyInfoItem in propertyInfo)
            {
                if (!completionsList.ContainsKey(propertyInfoItem.Name))
                {
                    completionsList.Add(propertyInfoItem.Name, IronPythonCompletionData.CompletionType.PROPERTY);
                }
            }

            foreach (FieldInfo fieldInfoItem in fieldInfo)
            {
                if (!completionsList.ContainsKey(fieldInfoItem.Name))
                {
                    completionsList.Add(fieldInfoItem.Name, IronPythonCompletionData.CompletionType.FIELD);
                }
            }

            if (type.IsEnum)
            {
                foreach (string en in type.GetEnumNames())
                {
                    if (!completionsList.ContainsKey(en))
                    {
                        completionsList.Add(en, IronPythonCompletionData.CompletionType.FIELD);
                    }
                }
            }

            foreach (var completionPair in completionsList)
            {
                items.Add(new IronPythonCompletionData(completionPair.Key, name, true, completionPair.Value, this));
            }

            return items;
        }

        /// <summary>
        /// Recursively lookup a member in a given namespace.
        /// </summary>
        /// <param name="name">A name for a type, possibly delimited by periods.</param>
        /// <param name="n">The namespace</param>
        /// <returns>The type as an object</returns>
        public object LookupMember(string name, NamespaceTracker n)
        {
            object varOutput;

            var periodIndex = name.IndexOf('.');
            if (periodIndex == -1)
            {
                if (n.TryGetValue(name, out varOutput))
                {
                    return varOutput;
                }
                return null;
            }

            var currentName = name.Substring(0, periodIndex);
            var theRest = name.Substring(periodIndex + 1);

            if (n.TryGetValue(currentName, out varOutput))
            {
                if (varOutput is NamespaceTracker)
                {
                    return LookupMember(theRest, varOutput as NamespaceTracker);
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively lookup a variable in the _scope
        /// </summary>
        /// <param name="name">A name for a type, possibly delimited by periods.</param>
        /// <returns>The type as an object</returns>
        public object LookupMember(string name)
        {
            object varOutput;

            var periodIndex = name.IndexOf('.');
            if (periodIndex == -1)
            {
                if (scope.TryGetVariable(name, out varOutput))
                {
                    return varOutput;
                }
                return null;
            }
            var currentName = name.Substring(0, periodIndex);
            var theRest = name.Substring(periodIndex + 1);

            if (scope.TryGetVariable(currentName, out varOutput))
            {
                if (varOutput is NamespaceTracker)
                {
                    return LookupMember(theRest, varOutput as NamespaceTracker);
                }
            }
            return null;

        }

        /// <summary>
        /// Generates completion data for the specified text. The text should be everything before
        /// the dot character that triggered the completion. The text can contain the command line prompt
        /// '>>>' as this will be ignored.
        /// </summary>
        [Obsolete("Please use additional GetDescription method as this one will be removed in Dynamo 3.0.")]
        public void GetDescription(string stub, string item, DescriptionUpdateDelegate updateDescription, bool isInstance)
        {
            string description = this.GetDescription(stub, item, isInstance);
            updateDescription(description);
        }

        /// <summary>
        /// Try to generate a description from a typename
        /// </summary>
        /// <param name="stub">Everything before the last namespace or type name e.g. System.Collections in System.Collections.ArrayList</param>
        /// <param name="item">Everything after the stub</param>
        /// <param name="isInstance">Whether it's an instance or not</param>
        public string GetDescription(string stub, string item, bool isInstance)
        {
            string description = "No description available"; // the default
            if (!String.IsNullOrEmpty(item))
            {
                try
                {
                    // Is this a faster alternative?
                    //object value = _engine.CreateScriptSourceFromString(stub + "." + item, SourceCodeKind.Expression).Execute(_scope);
                    //var des = _engine.Operations.GetDocumentation(value);

                    string docCommand = "";

                    if (isInstance)
                    {
                        docCommand = "type(" + stub + ")" + "." + item + ".__doc__";
                    }
                    else
                    {
                        docCommand = stub + "." + item + ".__doc__";
                    }

                    object value = engine.CreateScriptSourceFromString(docCommand, SourceCodeKind.Expression).Execute(scope);

                    if (!String.IsNullOrEmpty((string)value))
                    {
                        description = (string)value;
                    }

                }
                catch
                {

                }
            }

            return description;
        }

        /// <summary>
        /// A delegate used to update the description - useful for multi-threading
        /// </summary>
        /// <param name="description"></param>
        public delegate void DescriptionUpdateDelegate(string description);

        /// <summary>
        /// Traverse the given source code and define variable types based on
        /// the current scope
        /// </summary>
        /// <param name="code">The source code to look through</param>
        public void UpdateVariableTypes(string code)
        {
            VariableTypes.Clear();
            VariableTypes = FindAllVariableAssignments(code);
        }

        /// <summary>
        /// Check if a full type name is found in one of the known pre-loaded assemblies and return the type
        /// </summary>
        /// <param name="name">a full type name</param>
        /// <returns></returns>
        private Type TryGetTypeFromFullName(string name)
        {
            foreach (var asName in knownAssemblies)
            {
                Type foundType = Type.GetType(String.Format("{0},{1}", name, asName));
                if (foundType != null)
                {
                    return foundType;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a type from a name.  For example: System.Collections or System.Collections.ArrayList
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The type or null if its not a valid type</returns>
        protected Type TryGetType(string name)
        {
            if (ImportedTypes.ContainsKey(name))
            {
                return ImportedTypes[name];
            }

            // If the type name does not exist in the local or built-in variables, then it is out of scope
            string lookupScr = String.Format("clr.GetClrType({0}) if (\"{0}\" in locals() or \"{0}\" in __builtins__) and isinstance({0}, type) else None", name);

            dynamic type = null;
            try
            {
                type = engine.CreateScriptSourceFromString(lookupScr, SourceCodeKind.Expression).Execute(scope);
            }
            catch (Exception e)
            {
                Log(String.Format("Failed to look up type: {0}", name));
                Log(e.ToString());
            }

            var foundType = type as Type;
            if (foundType != null)
            {
                ImportedTypes[name] = foundType;
            }

            return foundType;
        }

        /// <summary>
        /// Attempts to find all import statements in the code
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A list of tuples that contain the namespace, the module, and the custom name</returns>
        private static List<Tuple<string, string, string>> FindAllImportStatements(string code)
        {
            var statements = new List<Tuple<string, string, string>>();

            // i.e. import math
            // or import math, cmath as cm
            var importMatches = MATCH_IMPORT_STATEMENTS.Matches(code);
            foreach (Match m in importMatches)
            {
                var names = new List<string>();

                // If the match ends with '.'
                if (m.Value.EndsWith("."))
                {
                    // For each group in mathces
                    foreach (Group item in m.Groups)
                    {
                        // Clone
                        var text = m.Value;

                        // Reformat statment
                        text = text.Replace("\t", "   ")
                                   .Replace("\n", " ")
                                   .Replace("\r", " ");
                        var spaceIndex = text.LastIndexOf(' ');
                        var equalsIndex = text.LastIndexOf('=');
                        var clean = text.Substring(Math.Max(spaceIndex, equalsIndex) + 1).Trim('.').Trim('(');

                        // Check for multi-line statement
                        var allStatements = clean.Trim().Split(new char[] { ',' }).Select(x => x.Trim()).ToList();
                        
                        // Build names output
                        foreach(string statement in allStatements)
                        {
                            names.Add(statement);
                        }
                    }
                }
                else
                {
                    // Check for multi-line statement
                    names = m.Groups[1].Value.Trim().Split(new char[] { ',' }).Select(x => x.Trim()).ToList();
                }

                foreach (string n in names)
                {
                    var parts = n.Split(new string[] { " as " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var name = parts[0];
                    string asname = parts.Length > 1 ? parts[1] : null;
                    statements.Add(new Tuple<string, string, string>(null, name, asname));
                }
            }

            // i.e. from Autodesk.Revit.DB import *
            // or from Autodesk.Revit.DB import XYZ, Line, Point as rvtPoint
            var fromMatches = MATCH_FROM_IMPORT_STATEMENTS.Matches(code);
            foreach (Match m in fromMatches)
            {
                var module = m.Groups[1].Value;
                var names = m.Groups[2].Value.Trim().Split(new char[] { ',' }).Select(x => x.Trim());
                foreach (string n in names)
                {
                    var parts = n.Split(new string[] { " as " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var name = parts[0];
                    string asname = parts.Length > 1 ? parts[1] : null;
                    statements.Add(new Tuple<string, string, string>(module, name, asname));
                }
            }
            return statements;
        }

        /// <summary>
        /// Attempts to find all variable assignments in the code. Has basic variable unpacking support.
        /// We don't need to check the line indices because regex matches are ordered as per the code.
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary of variable name and type pairs</returns>
        public Dictionary<string, Type> FindAllVariableAssignments(string code)
        {
            var assignments = new Dictionary<string, Type>();

            var varMatches = MATCH_VARIABLE_ASSIGNMENTS.Matches(code);
            foreach (Match m in varMatches)
            {
                string _left = m.Groups[1].Value.Trim(), _right = m.Groups[3].Value.Trim();
                if (BAD_ASSIGNEMNT_ENDS.Contains(_right.Last()))
                {
                    continue; // Incomplete statement
                }

                string[] left = _left.Split(new char[] { ',' }).Select(x => x.Trim()).ToArray();
                string[] right = _right.Split(new char[] { ',' }).Select(x => x.Trim()).ToArray();

                if (right.Length < left.Length)
                {
                    continue; // Unable to resolve iterable unpacking
                }

                if (left.Length == 1 && right.Length > 1)
                {
                    // Likely an iterable assignment has been broken up
                    right = new string[] { _right };
                }

                // Attempt to resolve each variable, assignment pair
                if (left.Length == right.Length)
                {
                    for (int i = 0; i < left.Length; i++)
                    {
                        // Check the basics first
                        bool foundBasicMatch = false;
                        foreach (Tuple<Regex, Type> rx in BasicVariableTypes)
                        {
                            if (rx.Item1.IsMatch(right[i]))
                            {
                                assignments[left[i]] = rx.Item2;
                                foundBasicMatch = true;
                                break;
                            }
                        }

                        // Check the scope for a possible match
                        if (!foundBasicMatch)
                        {
                            var possibleTypeName = GetFirstPossibleTypeName(right[i]);
                            if (!String.IsNullOrEmpty(possibleTypeName))
                            {
                                Type t1;
                                // Check if this is pointing to a predefined variable
                                if (!assignments.TryGetValue(possibleTypeName, out t1))
                                {
                                    // Proceed with a regular scope type check
                                    t1 = TryGetType(possibleTypeName);
                                }

                                if (t1 != null)
                                {
                                    assignments[left[i]] = t1;
                                }
                            }
                        }
                    }
                }
            }

            return assignments;
        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     from lib import *
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        public static Dictionary<string, string> FindAllTypeImportStatements(string code)
        {
            // matches the following types:
            //     from lib import *

            var pattern = fromImportRegex +
                          atLeastOneSpaceRegex +
                          variableName +
                          atLeastOneSpaceRegex +
                          basicImportRegex +
                          atLeastOneSpaceRegex +
                          @"\*$";

            var matches = Regex.Matches(code, pattern, RegexOptions.Multiline);

            var importMatches = new Dictionary<string, string>();

            for (var i = 0; i < matches.Count; i++)
            {
                var wholeLine = matches[i].Groups[0].Value;
                var libName = matches[i].Groups[3].Value.Trim();

                if (importMatches.ContainsKey(libName))
                    continue;

                importMatches.Add(libName, wholeLine);
            }

            return importMatches;
        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     from lib import type1, type2
        /// Doesn't currently match types with namespace qualifiers like Collections.ArrayList
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        public static Dictionary<string, string> FindTypeSpecificImportStatements(string code)
        {
            var pattern = fromImportRegex +
                          atLeastOneSpaceRegex +
                          variableName +
                          atLeastOneSpaceRegex +
                          basicImportRegex +
                          atLeastOneSpaceRegex +
                          commaDelimitedVariableNamesRegex +
                          "$";

            var matches = Regex.Matches(code, pattern, RegexOptions.Multiline);

            var importMatches = new Dictionary<string, string>();

            for (var i = 0; i < matches.Count; i++)
            {
                var wholeLine = matches[i].Groups[0].Value.TrimEnd('\r', '\n');
                var libName = matches[i].Groups[3].Value.Trim();
                var joinedTypeNames = matches[i].Groups[8].Value.Trim();

                var allTypes = joinedTypeNames.Replace(" ", "").Split(',');

                foreach (var typeName in allTypes)
                {
                    if (importMatches.ContainsKey(typeName))
                    {
                        continue;
                    }

                    importMatches.Add(typeName, wholeLine.Replace(joinedTypeNames, typeName));
                }
            }

            return importMatches;
        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     import lib
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        public static Dictionary<string, string> FindBasicImportStatements(string code)
        {
            var pattern = "^" + basicImportRegex + spacesOrNone + variableName;

            var matches = Regex.Matches(code, pattern, RegexOptions.Multiline);

            var importMatches = new Dictionary<string, string>();

            for (var i = 0; i < matches.Count; i++)
            {
                var wholeLine = matches[i].Groups[0].Value;
                var libName = matches[i].Groups[3].Value.Trim();

                if (importMatches.ContainsKey(libName))
                {
                    continue;
                }

                importMatches.Add(libName, wholeLine);
            }

            return importMatches;
        }

        /// <summary>
        /// Find a variable assignment of the form "varName = bla" where bla is matched by
        /// the given regex
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <param name="valueRegex">Your regex to match the type</param>
        /// <returns>A dictionary of name to assignment line pairs</returns>
        public static Dictionary<string, string> FindVariableStatementWithRegex(string code, string valueRegex)
        {
            var pattern = variableName + spacesOrNone + equalsRegex + spacesOrNone + valueRegex;

            var matches = Regex.Matches(code, pattern);

            var paramMatches = new Dictionary<string, string>();

            for (var i = 0; i < matches.Count; i++)
            {
                var name = matches[i].Groups[1].Value.Trim();
                var val = matches[i].Groups[6].Value.Trim();
                paramMatches.Add(name, val);
            }

            return paramMatches;
        }

        /// <summary>
        /// Detect all library references given the provided code
        /// </summary>
        /// <param name="code">Script code to search for CLR references</param>
        /// <returns></returns>
        private List<string> findClrReferences(string code)
        {
            var statements = new List<string>();
            foreach (var line in code.Split(new[] { '\n', ';' }))
            {
                if (line.Contains("clr.AddReference"))
                {
                    statements.Add(line.Trim());
                }
            }

            return statements;
        }

        /// <summary>
        /// Find all import statements and import into scope.  If the type is already in the scope, this will be skipped.
        /// </summary>
        /// <param name="code">The code to discover the import statements.</param>
        public void UpdateImportedTypes(string code)
        {
            // Detect all lib references prior to attempting to import anything
            var refs = findClrReferences(code);
            foreach (var statement in refs)
            {
                var previousTries = 0;
                badStatements.TryGetValue(statement, out previousTries);
                // TODO - Why is this 3?  Should this be a constant? Is it related to knownAssembies.Length?
                if (previousTries > 3)
                {
                    continue;
                }

                try
                {
                    string libName = MATCH_FIRST_QUOTED_NAME.Match(statement).Groups[1].Value;

                    //  If the library name cannot be found in the loaded clr modules
                    if (!clrModules.Contains(libName))
                    {
                        if (statement.Contains("AddReferenceToFileAndPath"))
                        {
                            engine.CreateScriptSourceFromString(statement, SourceCodeKind.SingleStatement).Execute(scope);
                            clrModules.Add(libName);
                            continue;
                        }

                        if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == libName))
                        {
                            engine.CreateScriptSourceFromString(statement, SourceCodeKind.SingleStatement).Execute(scope);
                            clrModules.Add(libName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(String.Format("Failed to reference library: {0}", statement));
                    Log(e.ToString());
                    badStatements[statement] = previousTries + 1;
                }
            }

            var importStatements = FindAllImportStatements(code);

            // Format import statements based on available data
            foreach (var i in importStatements)
            {
                string module = i.Item1;
                string memberName = i.Item2;
                string asname = i.Item3;
                string name = asname ?? memberName;
                string statement = "";
                var previousTries = 0;

                if (name != "*" && (scope.ContainsVariable(name) || ImportedTypes.ContainsKey(name)))
                {
                    continue;
                }

                try
                {
                    if (module == null)
                    {
                        if (asname == null)
                        {
                            statement = String.Format("import {0}", memberName);
                        }
                        else
                        {
                            statement = String.Format("import {0} as {1}", memberName, asname);
                        }
                    }
                    else
                    {
                        if (memberName != "*" && asname != null)
                        {
                            statement = String.Format("from {0} import {1} as {2}", module, memberName, asname);
                        }
                        else
                        {
                            statement = String.Format("from {0} import *", module);
                        }
                    }

                    badStatements.TryGetValue(statement, out previousTries);

                    if (previousTries > 3)
                    {
                        continue;
                    }

                    engine.CreateScriptSourceFromString(statement, SourceCodeKind.SingleStatement).Execute(scope);

                    if (memberName == "*")
                    {
                        continue;
                    }

                    string typeName = module == null ? memberName : String.Format("{0}.{1}", module, memberName);
                    var type = Type.GetType(typeName);
                    ImportedTypes.Add(name, type);
                }
                catch (Exception)
                {
                    Log(String.Format("Failed to load module: {0}, with statement: {1}", memberName, statement));
                    // Log(e.ToString());
                    badStatements[statement] = previousTries + 1;
                }
            }
        }

        /// <summary>
        /// Find all variable assignments in the source code and attempt to discover their type
        /// </summary>
        /// <param name="code">The code from which to get the assignments</param>
        /// <returns>A dictionary matching the name of the variable to a tuple of typeName, character at which the assignment was found, and the CLR type</returns>
        public Dictionary<string, Tuple<string, int, Type>> FindAllVariables(string code)
        {
            var variables = new Dictionary<string, Tuple<string, int, Type>>();
            var pattern = variableName + spacesOrNone + equalsRegex + spacesOrNone + @"(.*)";
            var variableStatements = Regex.Matches(code, pattern, RegexOptions.Multiline);

            for (var i = 0; i < variableStatements.Count; i++)
            {
                var name = variableStatements[i].Groups[1].Value.Trim();
                var typeString = variableStatements[i].Groups[6].Value.Trim(); // type
                var currentIndex = variableStatements[i].Index;

                var possibleTypeName = GetFirstPossibleTypeName(typeString);
                if (!String.IsNullOrEmpty(possibleTypeName))
                {
                    var variableType = TryGetType(possibleTypeName);
                    if (variableType != null)
                    {
                        // If variable has already been encountered
                        if (variables.ContainsKey(name))
                        {
                            if (currentIndex > variables[name].Item2)
                            {
                                variables[name] = new Tuple<string, int, Type>(typeString, currentIndex, variableType);
                            }
                        }
                        else // New type, add it
                        {
                            variables.Add(name, new Tuple<string, int, Type>(typeString, currentIndex, variableType));
                        }

                        continue;
                    }
                }

                // Match default types (numbers, dicts, arrays, strings)
                foreach (var pair in BasicVariableTypes)
                {
                    var matches = Regex.Matches(typeString, "^" + pair.Item1 + "$", RegexOptions.Singleline);
                    if (matches.Count > 0)
                    {
                        // If variable has already been encountered
                        if (variables.ContainsKey(name))
                        {
                            if (currentIndex > variables[name].Item2)
                            {
                                variables[name] = new Tuple<string, int, Type>(typeString, currentIndex, pair.Item2);
                            }
                        }
                        else // New type, add it
                        {
                            variables.Add(name, new Tuple<string, int, Type>(typeString, currentIndex, pair.Item2));
                        }

                        break;
                    }
                }
            }

            return variables;
        }

        /// <summary>
        /// Returns the last name from the input line. The regex ignores tabs, spaces, the first new line, etc.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string GetLastName(string text)
        {
            return MATCH_LAST_WORD.Match(text.Trim('.').Trim()).Value;
        }

        /// <summary>
        /// Returns the entire namespace from the end of the input line. The regex ignores tabs, spaces, the first new line, etc.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string GetLastNameSpace(string text)
        {
            return MATCH_LAST_NAMESPACE.Match(text.Trim('.').Trim()).Value;
        }

        /// <summary>
        /// Returns the first possible type name from the type's declaration line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string GetFirstPossibleTypeName(string line)
        {
            var match = MATCH_VALID_TYPE_NAME_CHARACTERS_ONLY.Match(line);
            string possibleTypeName = match.Success ? match.Value : "";
            return possibleTypeName;
        }

        /// <summary>
        /// Removes any docstring characters from the source code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string StripDocStrings(string code)
        {
            var matches = TRIPPLE_QUOTE_STRINGS.Split(code);
            return String.Join("", matches);
        }

        /// <summary>
        /// Returns the name from the end of a string.  Matches back to the first space and trims off spaces or ('s
        /// from the end of the line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [Obsolete("This method will be removed in Dynamo 3.0")]
        string GetName(string text)
        {
            text = text.Replace("\t", "   ");
            text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");
            int startIndex = text.LastIndexOf(' ');
            return text.Substring(startIndex + 1).Trim('.').Trim('(');
        }
        #endregion
    }
}