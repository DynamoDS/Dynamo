using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dynamo;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using Dynamo.Logging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System.Reflection;

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
        public Dictionary<string, Type> RegexToType = new Dictionary<string, Type>();
        
        /// <summary>
        /// Maps a basic variable regex to a basic python type.
        /// </summary>
        public List<Tuple<Regex, Type>> BasicVariableTypes;
        
        /// <summary>
        /// Tracks already referenced CLR modules
        /// </summary>
        public HashSet<string> clrModules { get; set; }
        
        /// <summary>
        /// Keeps track of failed statements to avoid poluting the log
        /// </summary>
        public Dictionary<string, int> badStatements { get; set; }

        /// <summary>
        /// A bunch of regexes for use in introspaction
        /// </summary>
        public static string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s?)+)";
        public static string variableName = @"([0-9a-zA-Z_]+(\.[a-zA-Z_0-9]+)*)";
        public static string quotesStringRegex = "[\"']([^\"']*)[\"']";
        public static string arrayRegex = "(\\[.*\\])";
        public static string spacesOrNone = @"(\s*)";
        public static string atLeastOneSpaceRegex = @"(\s+)";
        public static string equalsRegex = @"(=)";
        public static string dictRegex = "({.*})";
        public static string doubleRegex = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
        public static string intRegex = @"([-+]?\d+)[\s\n]*$";
        public static string basicImportRegex = @"(import)";
        public static string fromImportRegex = @"^(from)";
        
        private static readonly Regex MATCH_LAST_WORD = new Regex(@"\w+$", RegexOptions.Compiled);
        private static readonly Regex MATCH_FIRST_QUOTED_NAME = new Regex(quotesStringRegex, RegexOptions.Compiled);
        private static readonly Regex MATCH_VALID_TYPE_NAME_CHARACTERS_ONLY = new Regex(@"^\w+", RegexOptions.Compiled);
        private static readonly Regex TRIPPLE_QUOTE_STRINGS = new Regex(".*?\\\"{{3}}[\\s\\S]+?\\\"{{3}}", RegexOptions.Compiled);
        
        private static readonly Regex MATCH_IMPORT_STATEMENTS = new Regex(@"^import\s+?(.+)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex MATCH_FROM_IMPORT_STATEMENTS = new Regex(@"from\s+?([\w.]+)\s+?import\s+?([\w, *]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex MATCH_VARIABLE_ASSIGNMENTS = new Regex(@"^[ \t]*?(\w+(\s*?,\s*?\w+)*)\s*?=\s*(.+)", RegexOptions.Compiled | RegexOptions.Multiline);
        
        private static readonly Regex STRING_VARIABLE = new Regex("[\"']([^\"']*)[\"']", RegexOptions.Compiled);
        private static readonly Regex DOUBLE_VARIABLE = new Regex("^-?\\d+\\.\\d+", RegexOptions.Compiled);
        private static readonly Regex INT_VARIABLE = new Regex("^-?\\d+", RegexOptions.Compiled);
        private static readonly Regex LIST_VARIABLE = new Regex("\\[.*\\]", RegexOptions.Compiled);
        private static readonly Regex DICT_VARIABLE = new Regex("{.*}", RegexOptions.Compiled);
        
        private static readonly string BAD_ASSIGNEMNT_ENDS = ",([{";
        
        #endregion

        /// <summary>
        /// Class constructor
        /// </summary>
        public IronPythonCompletionProvider()
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            scope = engine.CreateScope();

            VariableTypes = new Dictionary<string, Type>();
            ImportedTypes = new Dictionary<string, Type>();
            clrModules = new HashSet<string>();
            badStatements = new Dictionary<string, int>();
            
            //special case for python variables defined as null
            ImportedTypes["None"] = null;
            
            BasicVariableTypes = new List<Tuple<Regex, Type>>();
            
            BasicVariableTypes.Add(Tuple.Create(STRING_VARIABLE, typeof(string)));
            BasicVariableTypes.Add(Tuple.Create(DOUBLE_VARIABLE,  typeof(double)));
            BasicVariableTypes.Add(Tuple.Create(INT_VARIABLE,  typeof(int)));
            BasicVariableTypes.Add(Tuple.Create(LIST_VARIABLE,  typeof(IronPython.Runtime.List)));
            BasicVariableTypes.Add(Tuple.Create(DICT_VARIABLE,  typeof(PythonDictionary)));
            
            //main clr module
            engine.CreateScriptSourceFromString("import clr\n", SourceCodeKind.SingleStatement).Execute(scope);
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Any(x => x.GetName().Name == "RevitAPI"))
            {
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

            if (assemblies.Any(x => x.GetName().Name == "ProtoGeometry"))
            {
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
            
            string pythonLibDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                                                         "IronPython 2.7\\Lib");
            if (System.IO.Directory.Exists(pythonLibDir) )
            {
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
                Log("Valid IronPython installation not found. Python autocomplete will not see native modules.");
            }
        }

        /// <summary>
        /// Generates completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        public ICompletionData[] GetCompletionData(string code)
        {
            var items = new List<IronPythonCompletionData>();
            
            if (code.Contains("\"\"\""))
            {
                code = StripDocStrings(code);
            }
            
            UpdateImportedTypes(code);
            UpdateVariableTypes(code); // this is where hindley-milner could come into play
            
            string name = GetLastName(code);
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    AutocompletionInProgress = true;

                    // is it a CLR type?
                    var type = TryGetType(name);
                    if (type != null)
                    {
                        items = EnumerateMembers(type, name);
                    }
                    // it's a variable?
                    else if (VariableTypes.TryGetValue(name, out type))
                    {
                        items = EnumerateMembers(type, name);
                    }
                    // is it a namespace or python type?
                    else
                    {
                        var mem = LookupMember(name);
                        if (mem is NamespaceTracker)
                        {
                            items = EnumerateMembers(mem as NamespaceTracker, name);
                        }
                        else
                        {
                            var pythonModule = mem as PythonModule;
                            if (pythonModule != null)
                            {
                                items = EnumerateMembers(pythonModule, name);
                            }
                            else if (mem is PythonType)
                            {
                                // shows static and instance methods in just the same way :(
                                var value = ClrModule.GetClrType(mem as PythonType);
                                if (value != null)
                                {
                                    items = EnumerateMembers(value, name);
                                }
                            }
                        }

                    }
                }
                catch
                {
                    //Dynamo.this.logger.Log("EXCEPTION: GETTING COMPLETION DATA");
                }
                AutocompletionInProgress = false;
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
                var ct = member.Value is BuiltinFunction ? IronPythonCompletionData.CompletionType.METHOD : IronPythonCompletionData.CompletionType.FIELD;
                items.Add(new IronPythonCompletionData( (string)member.Key, name, false, ct, this));
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
                else if (member.Value is Microsoft.Scripting.Actions.PropertyTracker)
                {
                    items.Add(new IronPythonCompletionData(member.Key, name, false, IronPythonCompletionData.CompletionType.PROPERTY, this));
                }
                else if (member.Value is Microsoft.Scripting.Actions.TypeTracker)
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
                if ( (methodInfoItem.IsPublic)
                    && (methodInfoItem.Name.IndexOf("get_") != 0) && (methodInfoItem.Name.IndexOf("set_") != 0)
                    && (methodInfoItem.Name.IndexOf("add_") != 0) && (methodInfoItem.Name.IndexOf("remove_") != 0)
                    && (methodInfoItem.Name.IndexOf("__") != 0))
                {
                    if (!completionsList.ContainsKey(methodInfoItem.Name))
                        completionsList.Add(methodInfoItem.Name, IronPythonCompletionData.CompletionType.METHOD);
                }
                
            }

            foreach (PropertyInfo propertyInfoItem in propertyInfo)
            {
                if (!completionsList.ContainsKey(propertyInfoItem.Name))
                    completionsList.Add(propertyInfoItem.Name, IronPythonCompletionData.CompletionType.PROPERTY);
            }

            foreach (FieldInfo fieldInfoItem in fieldInfo)
            {
                if (!completionsList.ContainsKey(fieldInfoItem.Name))
                    completionsList.Add(fieldInfoItem.Name, IronPythonCompletionData.CompletionType.FIELD);
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

            var currentName = name.Substring(0,periodIndex);
            var theRest = name.Substring(periodIndex+1);

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
        ///     Recursively lookup a variable in the _scope
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
                    if (isInstance) docCommand = "type(" + stub + ")" + "." + item + ".__doc__";
                    else docCommand = stub + "." + item + ".__doc__";
                    object value = engine.CreateScriptSourceFromString(docCommand, SourceCodeKind.Expression).Execute(scope);

                    if (!String.IsNullOrEmpty((string)value))
                        description = (string)value;
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
        ///     Traverse the given source code and define variable types based on
        ///     the current scope
        /// </summary>
        /// <param name="code">The source code to look through</param>
        public void UpdateVariableTypes(string code)
        {
            VariableTypes.Clear(); // for now...
            VariableTypes = FindAllVariableAssignments(code);
        }
        
        /// <summary>
        ///     Returns a type from a name.  For example: System.Collections or System.Collections.ArrayList
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The type or null if its not a valid type</returns>
        protected Type TryGetType(string name)
        {
            if (ImportedTypes.ContainsKey(name))
            {
                return ImportedTypes[name];
            }
            
            //if the type name does noe exist in the local or built-in variables, then it is out of scope
            string lookupScr = String.Format("clr.GetClrType({0}) if (\"{0}\" in locals() or \"{0}\" in __builtins__) and isinstance({0}, type) else None", name);
            
            dynamic type = null;
            try
            {
                type = engine.CreateScriptSourceFromString(lookupScr, SourceCodeKind.Expression).Execute(scope);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                Log(String.Format("Failed to look up type: {0}", name));
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
        /// <returns>A list of tuples that contain the namespace, the module and the custom name</returns>
        public static List<Tuple<string, string, string>> FindAllImportStatements(string code)
        {
            var statements = new List<Tuple<string, string, string>>();
            
            //i.e. import math
            //or import math, cmath as cm
            var importMatches = MATCH_IMPORT_STATEMENTS.Matches(code);
            foreach (Match m in importMatches)
            {
                var names = m.Groups[1].Value.Trim().Split(new char[]{','}).Select(x => x.Trim() );
                foreach (string n in names)
                {
                    var parts = n.Split(new string []{" as "}, 2, StringSplitOptions.RemoveEmptyEntries);
                    var name = parts[0];
                    string asname = parts.Length > 1 ? parts[1] : null;
                    statements.Add(new Tuple<string, string, string>(null, name, asname));
                }
            }
            
            //i.e. from Autodesk.Revit.DB import *
            //or from Autodesk.Revit.DB import XYZ, Line, Point as rvtPoint
            var fromMatches = MATCH_FROM_IMPORT_STATEMENTS.Matches(code);
            foreach (Match m in fromMatches)
            {
                var module = m.Groups[1].Value;
                var names = m.Groups[2].Value.Trim().Split(new char[]{','}).Select(x => x.Trim() );
                foreach (string n in names)
                {
                    var parts = n.Split(new string []{" as "}, 2, StringSplitOptions.RemoveEmptyEntries);
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
                if(BAD_ASSIGNEMNT_ENDS.Contains(_right.Last()))
                {
                    continue; //incomplete statement
                }
                
                string[] left = _left.Split(new char[]{','}).Select(x => x.Trim() ).ToArray();
                string[] right = _right.Split(new char[]{','}).Select(x => x.Trim() ).ToArray();
                
                if (right.Length < left.Length)
                {
                    continue; // we can't resolve iterable unpacking
                }
                
                if (left.Length == 1 && right.Length > 1)
                {
                    //most likely we broke up an iterable assignment
                    right = new string[]{_right};
                }
                
                //try to resolve each variable, assignment pair
                if (left.Length == right.Length)
                {
                    for (int i = 0; i < left.Length; i++)
                    {
                        //check the basics first
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
                        
                        //check the scope for a possible match
                        if(!foundBasicMatch)
                        {
                            var possibleTypeName = GetFirstPossibleTypeName(right[i]);
                            if (!String.IsNullOrEmpty(possibleTypeName))
                            {
                                Type t1;
                                //check if this is pointing to a predefined variable
                                if(!assignments.TryGetValue(possibleTypeName, out t1))
                                {
                                    //else proceed with a regular scope type check
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
            // from lib import *

            var matches = Regex.Matches(code, fromImportRegex + atLeastOneSpaceRegex + variableName +
                                        atLeastOneSpaceRegex + basicImportRegex + atLeastOneSpaceRegex + @"\*$", RegexOptions.Multiline);

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

            var matches = Regex.Matches(code, fromImportRegex + atLeastOneSpaceRegex + variableName +
                                        atLeastOneSpaceRegex + basicImportRegex + atLeastOneSpaceRegex + commaDelimitedVariableNamesRegex + "$", RegexOptions.Multiline);

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
                        continue;
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
            var matches = Regex.Matches(code, "^" + basicImportRegex + spacesOrNone + variableName, RegexOptions.Multiline);

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
        /// Find a variable assignment of the form "varName = bla" where bla is matched by
        /// the given regex
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <param name="valueRegex">Your regex to match the type</param>
        /// <returns>A dictionary of name to assignment line pairs</returns>
        public static Dictionary<string, string> FindVariableStatementWithRegex(string code, string valueRegex)
        {
            var matches = Regex.Matches(code, variableName + spacesOrNone + equalsRegex + spacesOrNone + valueRegex);

            var paramMatches = new Dictionary<string, string>();

            for (var i = 0; i < matches.Count; i++)
            {
                var name = matches[i].Groups[1].Value.Trim();
                var val = matches[i].Groups[6].Value.Trim();
                paramMatches.Add(name, val);
            }
            return paramMatches;

        }
        
        
        public List<string> findClrReferences(string code)
        {
            var statements = new List<string>();
            foreach (var line in code.Split(new[] {'\n', ';'}))
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
        /// The ImportedTypes dictionary is
        /// </summary>
        /// <param name="code">The code to discover the import statements.</param>
        public void UpdateImportedTypes(string code)
        {
            //detect all lib references prior to attempting to import anything
            var refs = findClrReferences(code);
            foreach (var statement in refs)
            {
                
                int previousTries = 0;
                badStatements.TryGetValue(statement, out previousTries);
                if (previousTries > 3)
                {
                    continue;
                }
                
                try
                {
                    string libName = MATCH_FIRST_QUOTED_NAME.Match(statement).Groups[1].Value;
                    if (!clrModules.Contains(libName))
                    {
                        if (statement.Contains("AddReferenceToFileAndPath"))
                        {
                            engine.CreateScriptSourceFromString(statement, SourceCodeKind.SingleStatement).Execute(scope);
                            //it's an assembly path, don't check the current appdomain
                            clrModules.Add(libName);
                            continue;
                        }
                        
                        if(AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == libName))
                        {
                            engine.CreateScriptSourceFromString(statement, SourceCodeKind.SingleStatement).Execute(scope);
                            clrModules.Add(libName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    Log(String.Format("Failed to reference library: {0}", statement));
                    badStatements[statement] = previousTries + 1;
                }
            }
            
            var importStatements = FindAllImportStatements(code);
            foreach (var i in importStatements)
            {
                string module = i.Item1, memberName = i.Item2, asname = i.Item3;
                string name = asname ?? memberName;
                string statement = "";
                int previousTries = 0;
                
                if (name != "*" && (scope.ContainsVariable(name) || ImportedTypes.ContainsKey(name)))
                {
                    continue;
                }
                
                try
                {
                    if (module == null)
                    {
                        statement = String.Format("import {0} as {1}", memberName, name);
                    }
                    else
                    {
                        if (memberName != "*")
                        {
                            statement = String.Format("from {0} import {1} as {2}", module, memberName, name);
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
                catch (Exception e)
                {
                    Log(e.ToString());
                    Log(String.Format("Failed to load module: {0}, with statement: {1}", memberName, statement));
                    badStatements[statement] = previousTries + 1;
                }
            }
        }

        /// <summary>
        /// Find all variable assignments in the source code and attempt to discover their type
        /// </summary>
        /// <param name="code">The code from which to get the assignments</param>
        /// <returns>A dictionary matching the name of the variable to a tuple of typeName, character at which the assignment was found, and the CLR type</returns>
        public Dictionary<string, Tuple<string, int, Type> > FindAllVariables(string code)
        {
            var variables = new Dictionary<string, Tuple<string, int, Type> >();

            var variableStatements = Regex.Matches(code, variableName + spacesOrNone + equalsRegex + spacesOrNone + @"(.*)", RegexOptions.Multiline);

            for (var i = 0; i < variableStatements.Count; i++)
            {
                var name = variableStatements[i].Groups[1].Value.Trim();
                var typeString = variableStatements[i].Groups[6].Value.Trim(); // type
                var currentIndex = variableStatements[i].Index;
                
                var possibleTypeName = GetFirstPossibleTypeName(typeString);
                if (!String.IsNullOrEmpty(possibleTypeName))
                {
                    var t1 = TryGetType(possibleTypeName);
                    if (t1 != null)
                    {
                        if (variables.ContainsKey(name))
                        {
                            if (currentIndex > variables[name].Item2)
                            {
                                variables[name] = new Tuple<string, int, Type>(typeString, currentIndex, t1);
                            }
                        }
                        else // we've never seen it, add the type
                        {
                            variables.Add(name, new Tuple<string, int, Type>(typeString, currentIndex, t1));
                        }
                        
                        continue;
                    }
                }
                
                // match default types (numbers, dicts, arrays, strings)
                foreach (var pair in RegexToType)
                {
                    var matches = Regex.Matches(typeString, "^" + pair.Key + "$", RegexOptions.Singleline);
                    if (matches.Count > 0)
                    {
                        // if we already saw this var
                        if (variables.ContainsKey(name))
                        {
                            if (currentIndex > variables[name].Item2)
                            {
                                variables[name] = new Tuple<string, int, Type>(typeString, currentIndex, pair.Value);
                            }
                        }
                        else // we've never seen it, add the type
                        {
                            variables.Add(name, new Tuple<string, int, Type>(typeString, currentIndex, pair.Value));
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
        string GetLastName(string text)
        {
            return MATCH_LAST_WORD.Match(text.Trim('.').Trim()).Value;
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
        /// Removes any docstring charactes from the source code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string StripDocStrings(string code)
        {
            var matches = TRIPPLE_QUOTE_STRINGS.Split(code);
            return String.Join("", matches);
        }
    }
}
