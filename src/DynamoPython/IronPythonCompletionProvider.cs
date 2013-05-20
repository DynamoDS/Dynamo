using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dynamo;
using ICSharpCode.AvalonEdit.CodeCompletion;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System.Threading;
using System.Reflection;

namespace DynamoPython
{
    /// <summary>
    /// Provides code completion for the Python Editor
    /// </summary>
    public class IronPythonCompletionProvider
    {
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
        /// This will eventually be used for multi-threading.
        /// </summary>
        internal volatile bool AutocompletionInProgress = false;

        public Dictionary<string, string> LoadedVariables { get; set; }
        public Dictionary<string, Type> LoadedTypes { get; set; }
        public Dictionary<string, Type> RegexToType = new Dictionary<string, Type>();

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

        public IronPythonCompletionProvider()
        {
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();

            LoadedTypes = new Dictionary<string, Type>();
            LoadedVariables = new Dictionary<string, string>();
            this.InitRegexTypes();

            var defaultImports =
                "import clr\n"; 

            _scope.Engine.CreateScriptSourceFromString(defaultImports, SourceCodeKind.Statements).Execute(_scope);

            try
            {
                var revitImports =
                    "clr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\n";

                _scope.Engine.CreateScriptSourceFromString(revitImports, SourceCodeKind.Statements).Execute(_scope);
            }
            catch
            {
                DynamoLogger.Instance.Log("Failed to load Revit types for Autocomplete.  Autocomplete will not see Autodesk namespace types.");
            }
        }


        /// <summary>
        /// Generates completion data for the specified text. 
        /// </summary>
        public ICompletionData[] GenerateCompletionData(string line)
        {
            var items = new List<DynamoCompletionData>();

            this.FindImportStatementsAndTryLoad(line);
            this.FindVariableStatementsAndGetType(line);

            string name = GetName(line);

            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    AutocompletionInProgress = true;

                    Type type = TryGetType(name);
                    if (type != null && type.Namespace != "IronPython.Runtime")
                    {
                        items = EnumerateMembers(type, name);
                    }
                    else if (this.LoadedVariables.ContainsKey(name))
                    {
                        string dirCommand = "dir(" + LoadedVariables[name] + ")";
                        object value = _scope.Engine.CreateScriptSourceFromString(dirCommand, SourceCodeKind.Expression).Execute(_scope);
                        if (value != null)
                        {
                            items.AddRange((value as IronPython.Runtime.List)
                                 .Select(member => new DynamoCompletionData((string) member, LoadedVariables[name], false, DynamoCompletionData.CompletionType.METHOD, this)));
                        }
                    } 
                    else
                    {
                        // look up a namespace 
                        var mem = this.LookupMember(name);
                        if (mem != null)
                        {
                            if (mem is NamespaceTracker)
                            {
                                items = this.EnumerateMembers(mem as NamespaceTracker, name);
                            } 
                            else if (mem is PythonModule)
                            {
                                items = this.EnumerateMembers(mem as PythonModule, name);
                            } 
                            else if (mem is PythonType)
                            {
                                // shows static and instance methods in just the same way :(
                                string dirCommand = "clr.GetClrType(" + name + ")";
                                object value = _scope.Engine.CreateScriptSourceFromString(dirCommand, SourceCodeKind.Expression).Execute(_scope);
                                if (value is Type)
                                {
                                    items = EnumerateMembers( value as Type, name);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Do nothing.
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
        public List<DynamoCompletionData> EnumerateMembers(PythonModule module, string name)
        {
            var items = new List<DynamoCompletionData>();
            var d = module.Get__dict__();

            foreach (var member in d)
            {
                if ( member.Value is BuiltinFunction )
                {
                    items.Add(new DynamoCompletionData( (string) member.Key, name, false, DynamoCompletionData.CompletionType.METHOD, this));
                }
                else
                {
                    items.Add(new DynamoCompletionData((string)member.Key, name, false, DynamoCompletionData.CompletionType.FIELD, this));
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
        public List<DynamoCompletionData> EnumerateMembers(NamespaceTracker ns, string name)
        {
            var items = new List<DynamoCompletionData>();

            foreach (var member in ns)
            {
                if (member.Value is NamespaceTracker)
                {
                    items.Add(new DynamoCompletionData(member.Key, name, false, DynamoCompletionData.CompletionType.NAMESPACE, this));
                }
                else if (member.Value is FieldTracker)
                {
                    items.Add(new DynamoCompletionData(member.Key, name, false, DynamoCompletionData.CompletionType.FIELD, this));
                }
                else if (member.Value is Microsoft.Scripting.Actions.PropertyTracker)
                {
                    items.Add(new DynamoCompletionData(member.Key, name, false, DynamoCompletionData.CompletionType.PROPERTY, this));
                }
                else if (member.Value is Microsoft.Scripting.Actions.TypeTracker)
                {
                    items.Add(new DynamoCompletionData(member.Key, name, false, DynamoCompletionData.CompletionType.CLASS, this));
                }
            }
            return items;
        }

        /// <summary>
        /// List all of the members in a CLR type
        /// </summary>
        /// <param name="ns">The type</param>
        /// <param name="name">The name for the type</param>
        /// <returns>A list of completion data for the type</returns>
        protected List<DynamoCompletionData> EnumerateMembers(Type type, string name)
        {
            var items = new List<DynamoCompletionData>();

            var completionsList = new SortedList<string, DynamoCompletionData.CompletionType>();

            var methodInfo = type.GetMethods();
            var propertyInfo = type.GetProperties();
            var fieldInfo = type.GetFields();

            foreach (MethodInfo methodInfoItem in methodInfo)
            {
                if ((methodInfoItem.IsPublic)
                    && (methodInfoItem.Name.IndexOf("get_") != 0) && (methodInfoItem.Name.IndexOf("set_") != 0)
                    && (methodInfoItem.Name.IndexOf("add_") != 0) && (methodInfoItem.Name.IndexOf("remove_") != 0)
                    && (methodInfoItem.Name.IndexOf("__") != 0))
                    completionsList.Add(methodInfoItem.Name, DynamoCompletionData.CompletionType.METHOD);
            }

            foreach (PropertyInfo propertyInfoItem in propertyInfo)
            {
                completionsList.Add(propertyInfoItem.Name, DynamoCompletionData.CompletionType.PROPERTY);
            }

            foreach (FieldInfo fieldInfoItem in fieldInfo)
            {
                completionsList.Add(fieldInfoItem.Name, DynamoCompletionData.CompletionType.FIELD);
            }

            foreach (var completionPair in completionsList)
            {
                items.Add(new DynamoCompletionData(completionPair.Key, name, true, completionPair.Value, this));
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
        public void GenerateDescription(string stub, string item, DescriptionUpdateDelegate updateDescription, bool isInstance)
        {
            string description = this.GenerateDescription(stub, item, isInstance);
            updateDescription(description);
        }

        /// <summary>
        /// Try to generate a description from a typename
        /// </summary>
        /// <param name="stub">Everything before the last namespace or type name e.g. System.Collections in System.Collections.ArrayList</param>
        /// <param name="item">Everything after the stub</param>
        /// <param name="isInstance">Whether it's an instance or not</param>
        public string GenerateDescription(string stub, string item, bool isInstance)
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

        public delegate void DescriptionUpdateDelegate(string description);

        /// <summary>
        ///     Traverse the given source code and 
        /// </summary>
        /// <param name="line"></param>
        private void FindVariableStatementsAndGetType(string line)
        {
            this.LoadedVariables.Clear();

            Dictionary<string, Tuple<string, int, Type>> vars = this.FindAllVariables(line);
            foreach (var varData in vars)
            {
                if (this.LoadedVariables.ContainsKey(varData.Key))
                {
                    LoadedVariables[varData.Key] = TypeToPythonType[varData.Value.Item3];
                }
                else
                {
                    LoadedVariables.Add(varData.Key, TypeToPythonType[varData.Value.Item3]);
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

            if (LoadedTypes.ContainsKey(name))
            {
                return LoadedTypes[name];
            }

            string tryGetType = name + ".GetType()";
            dynamic type = null;
            try
            {
                type = _scope.Engine.CreateScriptSourceFromString(tryGetType, SourceCodeKind.Expression).Execute(_scope);
            }
            catch (ThreadAbortException tae)
            {
                if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException) Thread.ResetAbort();
            }
            catch
            {
                Console.WriteLine();
            }
            return type as Type;
        }

        public static string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s*)+)";

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

        public static Dictionary<string, string> FindTypeSpecificImportStatements(string code)
        {
            // matches the following types:

                // from lib import type, type1
                // from lib import *

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

        public static Dictionary<string, string> FindBasicImportStatements(string code)
        {
            // matches the following types:

                // import bla

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

        public void FindImportStatementsAndTryLoad(string code)
        {
            // look all import statements
            var imports = FindBasicImportStatements(code)
                .Union(FindTypeSpecificImportStatements(code))
                .Union(FindAllTypeImportStatements(code));

            // try and load modules into python _scope
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
                    this.LoadedTypes.Add(import.Key, Type.GetType(import.Key));
                }
                catch
                {
                    Console.WriteLine();
                }
            }
        }

        Dictionary<Type, string> TypeToPythonType = new Dictionary<Type, string>();

        public void InitRegexTypes()
        {
            RegexToType.Add(singleQuoteStringRegex, typeof( string ));
            RegexToType.Add(doubleQuoteStringRegex, typeof( string ));
            RegexToType.Add(doubleRegex, typeof( double ));
            RegexToType.Add(intRegex, typeof( int ));
            RegexToType.Add(arrayRegex, typeof(IronPython.Runtime.List));
            RegexToType.Add(dictRegex, typeof(IronPython.Runtime.PythonDictionary));

            TypeToPythonType.Add(typeof(string), "str");
            TypeToPythonType.Add(typeof(double), "float");
            TypeToPythonType.Add(typeof(int), "int");
            TypeToPythonType.Add(typeof(IronPython.Runtime.List), "[]");
            TypeToPythonType.Add(typeof(IronPython.Runtime.PythonDictionary), "{}");
        }

        public Dictionary<string, Tuple<string, int, Type> > FindAllVariables(string code)
        {
            // regex to collection
            var variables = new Dictionary<string, Tuple<string, int, Type>>();

            // for each regex
            foreach (var pair in RegexToType)
            {
                var matches = Regex.Matches(code, variableName + spacesOrNone + equals + spacesOrNone + pair.Key, RegexOptions.Multiline);

                // for all of the matches
                for (var i = 0; i < matches.Count; i++)
                {
                    var name = matches[i].Groups[1].Value.Trim();
                    var val = matches[i].Groups[6].Value.Trim();
                    var currentIndex = matches[i].Index;

                    // if we already saw this var
                    if (variables.ContainsKey(name))
                    {
                        var varInfo = variables[name];
                        if (currentIndex > varInfo.Item2)
                        {
                            variables[name] = new Tuple<string, int, Type>(val, currentIndex, pair.Value);
                        }
                    }
                    else // we've never seen it, add the type
                    {
                        variables.Add( name, new Tuple<string, int, Type>(val, currentIndex, pair.Value));
                    }
                }
            }
            return variables;
        }

        string GetName(string text)
        {
            text = text.Replace("\t", "   ");
            text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");
            int startIndex = text.LastIndexOf(' ');
            return text.Substring(startIndex + 1).Trim('.');
        }

        // TODO: traverse source and process import statements
        // TODO: variable autocompletion?  we want to get types without executing!


    }
}