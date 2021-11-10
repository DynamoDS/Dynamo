using Autodesk.DesignScript.Interfaces;
using Dynamo.Logging;
using Dynamo.PythonServices;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DSCPython
{
    internal class DSCPythonCodeCompletionProviderCore : IExternalCodeCompletionProviderCore, ILogSource, IDisposable
    {
        #region Private members
        /// <summary>
        /// Maps a basic variable regex to a basic python type.
        /// </summary>
        private List<Tuple<Regex, Type>> BasicVariableTypes;

        /// <summary>
        /// Tracks already referenced CLR modules
        /// </summary>
        private HashSet<string> ClrModules { get; set; }

        /// <summary>
        /// Keeps track of failed statements to avoid poluting the log
        /// </summary>
        private Dictionary<string, int> BadStatements { get; set; }

        private PythonEngine engine;
        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
        private object Engine
        {
            get { return engine; }
            set { engine = (PythonEngine)value; }
        }
        private PyScope scope;

        private string uniquePythonScopeName = "uniqueScope";

        /// <summary>
        /// The scope used by the engine.  This is where all the loaded symbols
        /// are stored.  It's essentially an environment dictionary.
        /// </summary>
        private PyScope Scope
        {
            get { return scope; }
            set { scope = (PyScope)value; }
        }
        #endregion

        #region Private Methods
        private PyScope CreateUniquePythonScope()
        {
            PyScopeManager.Global.TryGet(uniquePythonScopeName, out PyScope Scope);

            if (Scope == null)
            {
                Scope = Py.CreateScope(uniquePythonScopeName);
                Scope.Exec(@"
import clr
import sys
clr.setPreload(True)
");
            }

            return Scope;
        }

        private object ExecutePythonScriptCode(string code)
        {
            Python.Included.Installer.SetupPython().Wait();

            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                using (Py.GIL())
                {
                    var result = Scope.Eval(code);
                    return CPythonEvaluator.OutputMarshaler.Marshal(result);
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }


        /// <summary>
        /// Find all import statements and import into scope.  If the type is already in the scope, this will be skipped.
        /// </summary>
        /// <param name="code">The code to discover the import statements.</param>
        private void UpdateImportedTypes(string code)
        {
            // Detect all lib references prior to attempting to import anything
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                var refs = PythonCodeCompletionUtils.FindClrReferences(code);
                foreach (var statement in refs)
                {
                    var previousTries = 0;
                    BadStatements.TryGetValue(statement, out previousTries);
                    // TODO - Why is this 3?  Should this be a constant? Is it related to knownAssembies.Length?
                    if (previousTries > 3)
                    {
                        continue;
                    }

                    try
                    {
                        string libName = PythonCodeCompletionUtils.MATCH_FIRST_QUOTED_NAME.Match(statement).Groups[1].Value;

                        //  If the library name cannot be found in the loaded clr modules
                        if (!ClrModules.Contains(libName))
                        {
                            if (statement.Contains("AddReferenceToFileAndPath"))
                            {
                                Scope.Exec(statement);
                                ClrModules.Add(libName);
                                continue;
                            }

                            if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == libName))
                            {
                                Scope.Exec(statement);
                                ClrModules.Add(libName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log(String.Format("Failed to reference library: {0}", statement));
                        Log(e.ToString());
                        BadStatements[statement] = previousTries + 1;
                    }
                }

                var importStatements = PythonCodeCompletionUtils.FindAllImportStatements(code);

                // Format import statements based on available data
                foreach (var i in importStatements)
                {
                    string module = i.Item1;
                    string memberName = i.Item2;
                    string asname = i.Item3;
                    string name = asname ?? memberName;
                    string statement = "";
                    var previousTries = 0;

                    if (name != "*" && (Scope.Contains(name) || ImportedTypes.ContainsKey(name)))
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

                        BadStatements.TryGetValue(statement, out previousTries);

                        if (previousTries > 3)
                        {
                            continue;
                        }

                        Scope.Exec(statement);

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
                        BadStatements[statement] = previousTries + 1;
                    }
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }

        /// <summary>
        /// Traverse the given source code and define variable types based on
        /// the current scope
        /// </summary>
        /// <param name="code">The source code to look through</param>
        private void UpdateVariableTypes(string code)
        {
            VariableTypes.Clear();
            VariableTypes = FindAllVariableAssignments(code);
        }

        /// <summary>
        /// Attempts to find all variable assignments in the code. Has basic variable unpacking support.
        /// We don't need to check the line indices because regex matches are ordered as per the code.
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary of variable name and type pairs</returns>
        private Dictionary<string, Type> FindAllVariableAssignments(string code)
        {

            var assignments = new Dictionary<string, Type>();

            var varMatches = PythonCodeCompletionUtils.MATCH_VARIABLE_ASSIGNMENTS.Matches(code);
            foreach (Match m in varMatches)
            {
                string _left = m.Groups[1].Value.Trim(), _right = m.Groups[3].Value.Trim();
                if (PythonCodeCompletionUtils.BAD_ASSIGNEMNT_ENDS.Contains(_right.Last()))
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
                            var possibleTypeName = PythonCodeCompletionUtils.GetFirstPossibleTypeName(right[i]);
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

        private Type TryGetType(string name)
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
                type = ExecutePythonScriptCode(lookupScr);
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
        /// List all of the members in a PythonModule
        /// </summary>
        /// <param name="module">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the module</returns>
        private IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(object module, string name)
        {
            var items = new List<Tuple<string, string, bool, ExternalCodeCompletionType>>();
            var d = (module as PyObject);

            foreach (var member in d.Dir())
            {
                try
                {
                    var completionType = ExternalCodeCompletionType.Field;

                    if (d.GetAttr(member.ToString()).ToString().Contains(PythonCodeCompletionUtils.inBuiltMethod) ||
                        d.GetAttr(member.ToString()).ToString().Contains(PythonCodeCompletionUtils.method))
                    {
                        completionType = ExternalCodeCompletionType.Method;
                    }

                    items.Add(Tuple.Create((string)member.ToString(), name, false, completionType));
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
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
        private IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(Type type, string name)
        {
            var items = new List<Tuple<string, string, bool, ExternalCodeCompletionType>>();
            var completionsList = new SortedList<string, ExternalCodeCompletionType>();

            if (type.FullName.Contains(PythonCodeCompletionUtils.internalType))
            {
                var methodInfo = type.GetMethods();
                var propertyInfo = type.GetProperties();
                var fieldInfo = type.GetFields();

                foreach (MethodInfo methodInfoItem in methodInfo)
                {
                    if (methodInfoItem.IsPublic
                        && (methodInfoItem.Name.IndexOf("get_") != 0) && (methodInfoItem.Name.IndexOf("set_") != 0)
                        && (methodInfoItem.Name.IndexOf("add_") != 0) && (methodInfoItem.Name.IndexOf("remove_") != 0)
                        && (methodInfoItem.Name.IndexOf("__") != 0))
                    {
                        if (!completionsList.ContainsKey(methodInfoItem.Name))
                        {
                            completionsList.Add(methodInfoItem.Name, ExternalCodeCompletionType.Method);
                        }
                    }
                }

                foreach (PropertyInfo propertyInfoItem in propertyInfo)
                {
                    if (!completionsList.ContainsKey(propertyInfoItem.Name))
                    {
                        completionsList.Add(propertyInfoItem.Name, ExternalCodeCompletionType.Property);
                    }
                }

                foreach (FieldInfo fieldInfoItem in fieldInfo)
                {
                    if (!completionsList.ContainsKey(fieldInfoItem.Name))
                    {
                        completionsList.Add(fieldInfoItem.Name, ExternalCodeCompletionType.Field);
                    }
                }

                if (type.IsEnum)
                {
                    foreach (string en in type.GetEnumNames())
                    {
                        if (!completionsList.ContainsKey(en))
                        {
                            completionsList.Add(en, ExternalCodeCompletionType.Field);
                        }
                    }
                }

                foreach (var completionPair in completionsList)
                {
                    items.Add(Tuple.Create(completionPair.Key, name, true, completionPair.Value));
                }
            }

            return items;
        }

        /// <summary>
        /// Recursively lookup a variable in the _scope
        /// </summary>
        /// <param name="name">A name for a type, possibly delimited by periods.</param>
        /// <returns>The type as an object</returns>
        private object LookupMember(string name)
        {
            var periodIndex = name.IndexOf('.');

            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                using (Py.GIL())
                {
                    if (periodIndex == -1)
                    {
                        if (Scope.TryGet(name, out object varOutput))
                        {
                            return varOutput;
                        }
                    }
                    return null;
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }
        #endregion

        /// <summary>
        /// Already discovered variable types.
        /// </summary>
        public Dictionary<string, Type> VariableTypes { get; set; }

        #region IExternalCodeCompletionProviderCore implementation

        /// <summary>
        /// Types that have already been imported into the scope.
        /// </summary>
        public Dictionary<string, Type> ImportedTypes { get; set; }

        /// <summary>
        /// Generate completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="expand">Determines if the entire namespace should be used</param>
        /// <returns>Return a list of IExternalCodeCompletionData </returns>
        public IExternalCodeCompletionData[] GetCompletionData(string code, bool expand = false)
        {
            IEnumerable<PythonCodeCompletionDataCore> items = new List<PythonCodeCompletionDataCore>();

            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                using (Py.GIL())
                {
                    if (code.Contains("\"\"\""))
                    {
                        code = PythonCodeCompletionUtils.StripDocStrings(code);
                    }

                    if (Scope == null)
                    {
                        Scope = CreateUniquePythonScope();
                    }

                    UpdateImportedTypes(code);
                    UpdateVariableTypes(code);  // Possibly use hindley-milner in the future

                    // If expand param is true use the entire namespace from the line of code
                    // Else just return the last name of the namespace
                    string name = expand ? PythonCodeCompletionUtils.GetLastNameSpace(code) : PythonCodeCompletionUtils.GetLastName(code);
                    if (!String.IsNullOrEmpty(name))
                    {
                        try
                        {
                            // Attempt to get type using naming
                            Type type = expand ? PythonCodeCompletionUtils.TryGetTypeFromFullName(name) : TryGetType(name);

                            // CLR type
                            if (type != null)
                            {
                                items = EnumerateMembers(type, name).Select(x => new PythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                            }
                            // Variable type
                            else if (VariableTypes.TryGetValue(name, out type))
                            {
                                items = EnumerateMembers(type, name).Select(x => new PythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                            }
                            else
                            {
                                var mem = LookupMember(name);
                                var pythonModule = mem as PyObject;
                                // Python Module type
                                if (pythonModule != null)
                                {
                                    items = EnumerateMembers(pythonModule, name).Select(x => new PythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log(ex.ToString());
                        }
                    }
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
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
        /// Try to generate a description from a typename.
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
                    string docCommand = "";

                    if (isInstance)
                    {
                        docCommand = "type(" + stub + ")" + "." + item + ".__doc__";
                    }
                    else
                    {
                        docCommand = stub + "." + item + ".__doc__";
                    }

                    object value = ExecutePythonScriptCode(docCommand);
                    //object value = engine.CreateScriptSourceFromString(docCommand, SourceCodeKind.Expression).Execute(scope);

                    if (!String.IsNullOrEmpty((string)value))
                    {
                        description = (string)value;
                    }
                }
                catch
                {
                    //This empty catch block is intentional- 
                    //because we are using a python engine to evaluate the completions
                    //but this engine has not actually loaded the types, it will throw lots of exceptions
                    //we wish to suppress.
                }
            }

            return description;
        }

        /// <summary>
        /// Used to determine if this IExternalCodeCompletionProviderCore can provide completions for the given engine.
        /// </summary>
        /// <param name="engineName"></param>
        /// <returns></returns>
        public bool IsSupportedEngine(string engineName)
        {
            if (engineName == "CPython3")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used to load initialize libraries and types that should be available by default.
        /// </summary>
        /// <param name="dynamoCorePath"></param>
        public void Initialize(string dynamoCorePath)
        {
            var pythonLibDir = string.Empty;
            var executionPath = Assembly.GetExecutingAssembly().Location;
            // Determine if the Python Standard Library is available in the DynamoCore path
            if (!String.IsNullOrEmpty(dynamoCorePath))
            {
                // set pythonLibDir path.
            }

            // TODO: check if any python libraries for CPython engine has to be imported. 
            if (!Directory.Exists(pythonLibDir))
            {
                // import them here if needed. 
            }

            if (!String.IsNullOrEmpty(pythonLibDir))
            {
                // Try to load Python Standard Library Type for autocomplete
                try
                {
                    var pyLibImports = String.Format("import sys\nsys.path.append(r'{0}')\n", pythonLibDir);
                    // engine.CreateScriptSourceFromString(pyLibImports, SourceCodeKind.Statements).Execute(scope);
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


        #region constructor
        public DSCPythonCodeCompletionProviderCore()
        {
            VariableTypes = new Dictionary<string, Type>();
            ImportedTypes = new Dictionary<string, Type>();
            ClrModules = new HashSet<string>();
            BadStatements = new Dictionary<string, int>();
            Guid pythonScopeGUID = Guid.NewGuid();
            uniquePythonScopeName += pythonScopeGUID.ToString();

            // Special case for python variables defined as null
            ImportedTypes["None"] = null;

            BasicVariableTypes = new List<Tuple<Regex, Type>>();

            BasicVariableTypes.Add(Tuple.Create(PythonCodeCompletionUtils.STRING_VARIABLE, typeof(string)));
            BasicVariableTypes.Add(Tuple.Create(PythonCodeCompletionUtils.DOUBLE_VARIABLE, typeof(double)));
            BasicVariableTypes.Add(Tuple.Create(PythonCodeCompletionUtils.INT_VARIABLE, typeof(int)));
            BasicVariableTypes.Add(Tuple.Create(PythonCodeCompletionUtils.LIST_VARIABLE, typeof(PyList)));
            BasicVariableTypes.Add(Tuple.Create(PythonCodeCompletionUtils.DICT_VARIABLE, typeof(PyDict)));

            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                using (Py.GIL())
                {
                    if (Scope == null)
                    {
                        Scope = CreateUniquePythonScope();
                    }

                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    // Determine if the Revit API is available in the given context
                    if (assemblies.Any(x => x.GetName().Name == "RevitAPI"))
                    {
                        // Try to load Revit Type for autocomplete
                        try
                        {
                            var revitImports =
                                "import clr\nclr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\nimport Autodesk\n";

                            Scope.Exec(revitImports);

                            ClrModules.Add("RevitAPI");
                            ClrModules.Add("RevitAPIUI");
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
                                "import clr\nclr.AddReference('ProtoGeometry')\nfrom Autodesk.DesignScript.Geometry import *\n";

                            Scope.Exec(libGImports);
                            ClrModules.Add("ProtoGeometry");
                        }
                        catch (Exception e)
                        {
                            Log(e.ToString());
                            Log("Failed to load ProtoGeometry types for autocomplete. Python autocomplete will not see Autodesk namespace types.");
                        }
                    }
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }

        #endregion

        #region ILogSource Implementation
        /// <summary>
        /// Raise this event to request loggers log this message.
        /// </summary>
        public event Action<ILogMessage> MessageLogged;

        internal void Log(string message)
        {
            MessageLogged?.Invoke(LogMessage.Info(message));
        }

        public void Dispose()
        {
            if (Scope != null)
            {
                if (!PythonEngine.IsInitialized)
                {
                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                }

                IntPtr gs = PythonEngine.AcquireLock();
                try
                {
                    using (Py.GIL())
                    {
                        Scope.Dispose();
                    }
                }
                finally
                {
                    PythonEngine.ReleaseLock(gs);
                }
            }
        }
        #endregion
    }
}