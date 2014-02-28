using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dynamo;
using Dynamo.Utilities;
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
    public class IronPythonCompletionProvider
    {
        #region Properties and fields

        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and 
        /// imported symbols.
        /// </summary>
        private ScriptEngine _engine;
        public ScriptEngine Engine
        {
            get { return _engine; }
            set { _engine = value; }
        }

        /// <summary>
        /// The scope used by the engine.  This is where all the loaded symbols
        /// are stored.  It's essentially an environment dictionary.
        /// </summary>
        private ScriptScope _scope;
        public ScriptScope Scope
        {
            get { return _scope; }
            set { _scope = value; }
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
        /// A bunch of regexes for use in introspaction
        /// </summary>
        public static string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s*)+)";
        public static string variableName = @"([0-9a-zA-Z_]+(\.[a-zA-Z_0-9]+)*)";   
        public static string doubleQuoteStringRegex = "(\"[^\"]*\")";
        public static string singleQuoteStringRegex = "(\'[^\']*\')";
        public static string arrayRegex = "(\\[.*\\])";
        public static string spacesOrNone = @"(\s*)";
        public static string atLeastOneSpaceRegex = @"(\s+)";
        public static string equals = @"(=)";
        public static string dictRegex = "({.*})";
        public static string doubleRegex = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
        public static string intRegex = @"([-+]?\d+)[\s\n]*$";
        public static string basicImportRegex = @"(import)";
        public static string fromImportRegex = @"^(from)";

#endregion

        /// <summary>
        /// Class constructor
        /// </summary>
        public IronPythonCompletionProvider()
        {
            _engine = IronPython.Hosting.Python.CreateEngine();
            _scope = _engine.CreateScope();

            VariableTypes = new Dictionary<string, Type>();
            ImportedTypes = new Dictionary<string, Type>();

            RegexToType.Add(singleQuoteStringRegex, typeof(string));
            RegexToType.Add(doubleQuoteStringRegex, typeof(string));
            RegexToType.Add(doubleRegex, typeof(double));
            RegexToType.Add(intRegex, typeof(int));
            RegexToType.Add(arrayRegex, typeof(List));
            RegexToType.Add(dictRegex, typeof(PythonDictionary));

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")))
            {
                try
                {
                    _scope.Engine.CreateScriptSourceFromString("import clr\n", SourceCodeKind.Statements).Execute(_scope);

                    var revitImports =
                        "clr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\nimport Autodesk\n";

                    _scope.Engine.CreateScriptSourceFromString(revitImports, SourceCodeKind.Statements).Execute(_scope);
                }
                catch
                {
                    DynamoLogger.Instance.Log("Failed to load Revit types for autocomplete.  Python autocomplete will not see Autodesk namespace types.");
                }
            }

            if (!assemblies.Any(x => x.FullName.Contains("LibG.Managed")))
            {
                AssemblyHelper.LoadLibG();

                //refresh the assemblies collection
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            if (assemblies.Any(x => x.FullName.Contains("LibG.Managed")))
            {
                try
                {
                    _scope.Engine.CreateScriptSourceFromString("import clr\n", SourceCodeKind.Statements).Execute(_scope);

                    var libGImports =
                        "import clr\nclr.AddReference('LibG.Managed')\nfrom Autodesk.LibG import *\n";

                    _scope.Engine.CreateScriptSourceFromString(libGImports, SourceCodeKind.Statements).Execute(_scope);
                }
                catch (Exception e)
                {
                    DynamoLogger.Instance.Log(e.ToString());
                    DynamoLogger.Instance.Log("Failed to load LibG types for autocomplete.  Python autocomplete will not see Autodesk namespace types.");
                }
            }

        }

        /// <summary>
        /// Generates completion data for the specified text, while import the given types into the 
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="line">The code to parse</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        public ICompletionData[] GetCompletionData(string line)
        {
            var items = new List<IronPythonCompletionData>();

            this.UpdateImportedTypes(line);
            this.UpdateVariableTypes(line); // this is where hindley-milner could come into play

            string name = GetName(line);
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
                    else if (this.VariableTypes.ContainsKey(name) ) 
                    {
                        items = EnumerateMembers(this.VariableTypes[name], name);
                    }
                    // is it a namespace or python type?
                    else 
                    {
                        var mem = LookupMember(name);

                        if (mem is NamespaceTracker)
                        {
                            items = EnumerateMembers(mem as NamespaceTracker, name);
                        } 
                        else if (mem is PythonModule)
                        {
                            items = EnumerateMembers(mem as PythonModule, name);
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
                catch
                {
                    //Dynamo.DynamoLogger.Instance.Log("EXCEPTION: GETTING COMPLETION DATA");
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
                if ( member.Value is BuiltinFunction )
                {
                    items.Add(new IronPythonCompletionData( (string) member.Key, name, false, IronPythonCompletionData.CompletionType.METHOD, this));
                }
                else
                {
                    items.Add(new IronPythonCompletionData((string)member.Key, name, false, IronPythonCompletionData.CompletionType.FIELD, this));
                }
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
                if ((methodInfoItem.IsPublic)
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
                if (_scope.TryGetVariable(name, out varOutput))
                {
                    return varOutput;
                }
                return null;
            }
            var currentName = name.Substring(0, periodIndex);
            var theRest = name.Substring(periodIndex + 1);

            if (_scope.TryGetVariable(currentName, out varOutput))
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
                    object value = _engine.CreateScriptSourceFromString(docCommand, SourceCodeKind.Expression).Execute(_scope);

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
        /// <param name="line">The source code to look through</param>
        public void UpdateVariableTypes(string line)
        {
            this.VariableTypes.Clear(); // for now...

            var vars = this.FindAllVariables(line);
            foreach (var varData in vars)
            {
                if (this.VariableTypes.ContainsKey(varData.Key))
                {
                    VariableTypes[varData.Key] = varData.Value.Item3;
                }
                else
                {
                    VariableTypes.Add(varData.Key, varData.Value.Item3);
                }
            }
        }

        /// <summary>
        ///     Get a type from a name.  For example: System.Collections or System.Collections.ArrayList
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The type or null if its not a valid type</returns>
        protected Type TryGetType(string name)
        {
            if (ImportedTypes.ContainsKey(name))
            {
                return ImportedTypes[name];
            }

            string tryGetType = name + ".GetType()";
            dynamic type = null;
            try
            {
                type = _scope.Engine.CreateScriptSourceFromString(tryGetType, SourceCodeKind.Expression).Execute(_scope);
            }
            catch (Exception e)
            {
                Dynamo.DynamoLogger.Instance.Log(e.ToString());
                Dynamo.DynamoLogger.Instance.Log("Failed to look up type");
            }
            return type as Type;
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
                    if (importMatches.ContainsKey(libName))
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
        /// <returns>A dictionary of name to assignment line pairs/returns>
        public static Dictionary<string, string> FindVariableStatementWithRegex(string code, string valueRegex)
        {
            var matches = Regex.Matches(code, variableName + spacesOrNone + equals + spacesOrNone + valueRegex);

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
        /// Find all import statements and import into scope.  If the type is already in the scope, this will be skipped.  
        /// The ImportedTypes dictionary is 
        /// </summary>
        /// <param name="code">The code to discover the import statements.</param>
        public void UpdateImportedTypes(string code)
        {
            // look all import statements
            var imports = FindBasicImportStatements(code)
                .Union(FindTypeSpecificImportStatements(code))
                .Union(FindAllTypeImportStatements(code));

            // try and load modules into python scope
            foreach (var import in imports)
            {
                if (_scope.ContainsVariable(import.Key))
                {
                    continue;
                }
                try
                {
                    _scope.Engine.CreateScriptSourceFromString(import.Value, SourceCodeKind.SingleStatement)
                         .Execute(this._scope);
                    var type = Type.GetType(import.Key);
                    this.ImportedTypes.Add(import.Key, type);
                }
                catch
                {
                    Console.WriteLine();
                }
            }

        }

        /// <summary>
        /// Find all variable assignments in the source code and attempt to discover their type
        /// </summary>
        /// <param name="code">The code from whih to get the assignments</param>
        /// <returns>A dictionary matching the name of the variable to a tuple of typeName, character at which the assignment was found, and the CLR type</returns>
        public Dictionary<string, Tuple<string, int, Type> > FindAllVariables(string code)
        {
            // regex to collection
            var variables = new Dictionary<string, Tuple<string, int, Type>>();

            var variableStatements = Regex.Matches(code, variableName + spacesOrNone + equals + spacesOrNone + @"(.*)", RegexOptions.Multiline);

            for (var i = 0; i < variableStatements.Count; i++)
            {
                var name = variableStatements[i].Groups[1].Value.Trim();
                var typeString = variableStatements[i].Groups[6].Value.Trim(); // type
                var currentIndex = variableStatements[i].Index;

                // check if matches typename(blabla) - in this case its a type we need to look up
                var typeStringMatches = Regex.Matches(typeString, @"^(.*)\(.*\)$", RegexOptions.Singleline);
                if (typeStringMatches.Count > 0)
                {
                    typeString = typeStringMatches[0].Groups[1].Value.Trim();
                    var typeInScope = this.LookupMember(typeString);

                    if (typeInScope is PythonType) // dictionary, enum, etc
                    {
                        var type = ClrModule.GetClrType(typeInScope as PythonType);

                        //// if we already saw this var
                        if (variables.ContainsKey(name))
                        {
                            var varInfo = variables[name];
                            if (currentIndex > varInfo.Item2)
                            {
                                variables[name] = new Tuple<string, int, Type>(typeString, currentIndex, type);
                            }
                        }
                        else // we've never seen it, add the type
                        {
                            variables.Add(name, new Tuple<string, int, Type>(typeString, currentIndex, type));
                        }
                    }

                }
                else // match default types (numbers, dicts, arrays, strings)
                {
                    foreach (var pair in RegexToType)
                    {
                        var matches = Regex.Matches(typeString, "^" + pair.Key + "$", RegexOptions.Singleline);
                        if (matches.Count > 0)
                        {
                            // if we already saw this var
                            if (variables.ContainsKey(name))
                            {
                                var varInfo = variables[name];
                                if (currentIndex > varInfo.Item2)
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
            }

            return variables;
        }

        /// <summary>
        /// Get the name from the end of a string.  Matches back to the first space and trims off spaces or ('s
        /// from the end of the line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string GetName(string text)
        {
            text = text.Replace("\t", "   ");
            text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");
            int startIndex = text.LastIndexOf(' ');
            return text.Substring(startIndex + 1).Trim('.').Trim('(');
        }

    }
}