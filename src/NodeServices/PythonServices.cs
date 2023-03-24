using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.PythonServices.EventHandlers;

namespace PythonNodeModels
{
    /// <summary>
    /// Enum of possible values of python engine versions.
    /// </summary>
    [Obsolete("This Enum will be remove in Dynamo 3.0")]
    public enum PythonEngineVersion
    {
        Unspecified,
        IronPython2,
        CPython3,
        Unknown
    }
}

namespace Dynamo.PythonServices.EventHandlers
{
    [SupressImportIntoVM]
    public delegate void ScopeSetAction(string name, object value);
    [SupressImportIntoVM]
    public delegate object ScopeGetAction(string name);

    [SupressImportIntoVM]
    public delegate void EvaluationStartedEventHandler(string code,
                                                       IList bindingValues,
                                                       ScopeSetAction scopeSet);
    [SupressImportIntoVM]
    public delegate void EvaluationFinishedEventHandler(EvaluationState state,
                                                        string code,
                                                        IList bindingValues,
                                                        ScopeGetAction scopeGet);
}

namespace Dynamo.PythonServices
{
    [SupressImportIntoVM]
    /// <summary>
    /// Enum of possible python evaluation states.
    /// </summary>
    public enum EvaluationState { Success, Failed }

    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    /// <summary>
    /// This abstract class is intended to act as a base class for different python engines
    /// </summary>
    public abstract class PythonEngine
    {
        /// <summary>
        /// Data Marshaler for all data coming into a Python node.
        /// </summary>
        public abstract object InputDataMarshaler
        {
            get;
        }

        /// <summary>
        /// Data Marshaler for all data coming out of a Python node.
        /// </summary>
        public abstract object OutputDataMarshaler
        {
            get;
        }

        /// <summary>
        /// Name of the Python engine
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Add an event handler before the Python evaluation begins
        /// </summary>
        /// <param name="callback"></param>
        public abstract event EvaluationStartedEventHandler EvaluationStarted;

        /// <summary>
        /// Add an event handler after the Python evaluation has finished
        /// </summary>
        /// <param name="callback"></param>
        public abstract event EvaluationFinishedEventHandler EvaluationFinished;

        /// <summary>
        ///     Executes a Python script with custom variable names. Script may be a string
        ///     read from a file, for example. Pass a list of names (matching the variable
        ///     names in the script) to bindingNames and pass a corresponding list of values
        ///     to bindingValues.
        /// </summary>
        /// <param name="code">Python script as a string.</param>
        /// <param name="bindingNames">Names of values referenced in Python script.</param>
        /// <param name="bindingValues">Values referenced in Python script.</param>
        public abstract object Evaluate(string code,
                        IList bindingNames,
                        [ArbitraryDimensionArrayImport] IList bindingValues);
    }

    /// <summary>
    /// Singleton class that other class can access and use for query loaded Python Engine info.
    /// </summary>
    public sealed class PythonEngineManager
    {
        /// <summary>
        /// Use Lazy&lt;PythonEngineManager&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        internal static readonly Lazy<PythonEngineManager>
            lazy =
            new Lazy<PythonEngineManager>
            (() => new PythonEngineManager());

        #region Public members
        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        public static PythonEngineManager Instance { get { return lazy.Value; } }
        #endregion

        /// <summary>
        /// An observable collection of all the loaded Python engines
        /// </summary>
        public ObservableCollection<PythonEngine> AvailableEngines;

        #region Constant strings
        
        /// <summary>
        /// CPython Engine name
        /// </summary>
        internal static readonly string CPython3EngineName = "CPython3";

        /// <summary>
        /// IronPython2 Engine name
        /// </summary>
        internal static readonly string IronPython2EngineName = "IronPython2";

        internal static string PythonEvaluatorSingletonInstance = "Instance";

        internal static string IronPythonEvaluatorClass = "IronPythonEvaluator";
        internal static string IronPythonEvaluationMethod = "EvaluateIronPythonScript";

        internal static string CPythonEvaluatorClass = "CPythonEvaluator";
        internal static string CPythonEvaluationMethod = "EvaluatePythonScript";

        internal static string IronPythonAssemblyName = "DSIronPython";
        internal static string CPythonAssemblyName = "DSCPython";

        internal static string IronPythonTypeName = IronPythonAssemblyName + "." + IronPythonEvaluatorClass;
        internal static string CPythonTypeName = CPythonAssemblyName + "." + CPythonEvaluatorClass;

        internal static string PythonInputMarshalerProperty = "InputMarshaler";
        internal static string PythonOutputMarshalerProperty = "OutputMarshaler";

        internal static string DummyEvaluatorClass = "DummyPythonEvaluator";
        internal static string DummyEvaluatorMethod = "Evaluate";
        #endregion

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to evaluate a Python node
        /// </summary>
        private PythonEngineManager()
        {
            AvailableEngines = new ObservableCollection<PythonEngine>();

            // We check only for the default python engine because it is the only one loaded by static references.
            // Other engines can only be loaded through package manager
            LoadDefaultPythonEngine(AppDomain.CurrentDomain.GetAssemblies().
               FirstOrDefault(a => a != null && a.GetName().Name == CPythonAssemblyName));

            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler((object sender, AssemblyLoadEventArgs args) => LoadDefaultPythonEngine(args.LoadedAssembly));
        }

        private void LoadDefaultPythonEngine(Assembly a)
        {
            if (a == null ||
                a.GetName().Name != CPythonAssemblyName)
            {
                return;
            }

            try
            {
                LoadPythonEngine(a);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load {CPythonAssemblyName} with error: {e.Message}");
            }
        }

        private PythonEngine GetEngine(string version)
        {
            return AvailableEngines.FirstOrDefault(x => x.Name == version);
        }

        // This method can throw exceptions.
        internal void LoadPythonEngine(IEnumerable<Assembly> assemblies)
        {
            foreach (var a in assemblies)
            {
                LoadPythonEngine(a);
            }
        }

        // This method can throw exceptions.
        private void LoadPythonEngine(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            // Currently we are using try-catch to validate loaded assembly and Singleton Instance method exist
            // but we can optimize by checking all loaded types against evaluators interface later
            try
            {
                Type eType = null;
                PropertyInfo instanceProp = null;
                try
                {
                    eType = assembly.GetTypes().FirstOrDefault(x => typeof(PythonEngine).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
                    if (eType == null) return;

                    instanceProp = eType?.GetProperty(PythonEvaluatorSingletonInstance, BindingFlags.NonPublic | BindingFlags.Static);
                    if (instanceProp == null) return;
                }
                catch {
                    // Ignore exceptions from iterating assembly types.
                    return;
                }

                PythonEngine engine = (PythonEngine)instanceProp.GetValue(null);
                if (engine == null) 
                {
                    throw new Exception($"Could not get a valid PythonEngine instance by calling the {eType.Name}.{PythonEvaluatorSingletonInstance} method");
                }

                if (GetEngine(engine.Name) == null)
                {
                    AvailableEngines.Add(engine);
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to add a Python engine from assembly {assembly.GetName().Name}.dll with error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Concrete type that gets returned and converted to an Avalonedit type implementing
    /// ICompletionData when used from WPF ScriptEditorContorl.
    /// </summary>
    internal class PythonCodeCompletionDataCore : IExternalCodeCompletionData
    {
        private readonly IExternalCodeCompletionProviderCore provider;
        private string description;

        public PythonCodeCompletionDataCore(string text, string stub, bool isInstance,
            ExternalCodeCompletionType completionType, IExternalCodeCompletionProviderCore providerCore)
        {
            Text = text;
            Stub = stub;
            IsInstance = isInstance;
            provider = providerCore;
            CompletionType = completionType;
        }

        public string Text { get; private set; }

        public string Stub { get; private set; }

        public bool IsInstance { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return this.Text; }
        }

        public string Description
        {
            get
            {
                // lazily get the description
                if (description == null)
                {
                    description = provider.GetDescription(this.Stub, this.Text, this.IsInstance).TrimEnd('\r', '\n');
                }

                return description;
            }
        }
        public double Priority { get { return 0; } }

        public ExternalCodeCompletionType CompletionType { get; private set; }
    }

    /// <summary>
    /// This class represents a base class for Python code completion providers
    /// It partially implements the IExternalCodeCompletionProviderCore interface and
    /// contains a collection of utility functions/properties that are common among existing code completion provider classes
    /// </summary>
    internal abstract class PythonCodeCompletionProviderCommon : IExternalCodeCompletionProviderCore
    {
        #region internal constants
        internal static readonly string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s?)+)";
        internal static readonly string variableName = @"([0-9a-zA-Z_]+(\.[a-zA-Z_0-9]+)*)";
        internal static readonly string spacesOrNone = @"(\s*)";
        internal static readonly string atLeastOneSpaceRegex = @"(\s+)";
        internal static readonly string dictRegex = "({.*})";
        internal static readonly string basicImportRegex = @"(import)";
        internal static readonly string fromImportRegex = @"^(from)"; internal static string doubleQuoteStringRegex = "(\"[^\"]*\")"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        internal static string singleQuoteStringRegex = "(\'[^\']*\')"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        internal static string arrayRegex = "(\\[.*\\])";
        internal static string equals = @"(=)"; // Not CLS compliant - replaced with equalsRegex - Remove in Dynamo 3.0
        internal static string doubleRegex = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
        internal static string intRegex = @"([-+]?\d+)[\s\n]*$";
        internal const string quotesStringRegex = "[\"']([^\"']*)[\"']";
        internal const string equalsRegex = @"(=)";

        internal static readonly Regex MATCH_LAST_NAMESPACE = new Regex(@"[\w.]+$", RegexOptions.Compiled);
        internal static readonly Regex MATCH_LAST_WORD = new Regex(@"\w+$", RegexOptions.Compiled);
        internal static readonly Regex MATCH_FIRST_QUOTED_NAME = new Regex(quotesStringRegex, RegexOptions.Compiled);
        internal static readonly Regex MATCH_VALID_TYPE_NAME_CHARACTERS_ONLY = new Regex(@"^\w+", RegexOptions.Compiled);
        internal static readonly Regex TRIPLE_QUOTE_STRINGS = new Regex(".*?\\\"{{3}}[\\s\\S]+?\\\"{{3}}", RegexOptions.Compiled);

        internal static readonly Regex MATCH_IMPORT_STATEMENTS = new Regex(@"^import\s+?(.+)", RegexOptions.Compiled | RegexOptions.Multiline);
        internal static readonly Regex MATCH_FROM_IMPORT_STATEMENTS = new Regex(@"from\s+?([\w.]+)\s+?import\s+?([\w, *]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        internal static readonly Regex MATCH_VARIABLE_ASSIGNMENTS = new Regex(@"^[ \t]*?(\w+(\s*?,\s*?\w+)*)\s*?=\s*(.+)", RegexOptions.Compiled | RegexOptions.Multiline);

        internal static readonly Regex STRING_VARIABLE = new Regex("[\"']([^\"']*)[\"']", RegexOptions.Compiled);
        internal static readonly Regex DOUBLE_VARIABLE = new Regex("^-?\\d+\\.\\d+", RegexOptions.Compiled);
        internal static readonly Regex INT_VARIABLE = new Regex("^-?\\d+", RegexOptions.Compiled);
        internal static readonly Regex LIST_VARIABLE = new Regex("\\[.*\\]", RegexOptions.Compiled);
        internal static readonly Regex DICT_VARIABLE = new Regex("{.*}", RegexOptions.Compiled);

        internal static readonly string BAD_ASSIGNEMNT_ENDS = ",([{";

        internal static readonly string inBuiltMethod = "built-in";
        internal static readonly string method = "method";
        internal static readonly string internalType = "Autodesk";

        internal static readonly string clrReference = "clr.AddReference";

        /// <summary>
        /// A list of short assembly names used with the TryGetTypeFromFullName method
        /// </summary>
        private static string[] knownAssemblies = {
            "mscorlib",
            "RevitAPI",
            "RevitAPIUI",
            "ProtoGeometry"
        };
        #endregion

        #region Common members
        protected enum PythonScriptType
        {
            SingleStatement,
            Statements,
            Expression
        }

        /// <summary>
        /// Maps a basic variable regex to a basic python type.
        /// </summary>
        protected List<Tuple<Regex, Type>> BasicVariableTypes;

        /// <summary>
        /// Tracks already referenced CLR modules
        /// </summary>
        protected HashSet<string> ClrModules { get; set; }

        protected abstract object GetDescriptionObject(string docCommand);
        #endregion

        #region IExternalCodeCompletionProviderCore implementation
        /// <summary>
        /// Keeps track of failed statements to avoid poluting the log
        /// </summary>
        protected Dictionary<string, int> BadStatements { get; set; }

        public Dictionary<string, Type> VariableTypes { get; set; }

        public Dictionary<string, Type> ImportedTypes { get; set; }

        public abstract IExternalCodeCompletionData[] GetCompletionData(string code, bool expand = false);

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

                    object value = GetDescriptionObject(docCommand);

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

        public abstract bool IsSupportedEngine(string engineName);

        public abstract void Initialize(string dynamoCorePath);
        #endregion

        #region Utility Methods

        /// <summary>
        /// List all of the members in a CLR type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name for the type</param>
        /// <returns>A list of completion data for the type</returns>
        protected IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(Type type, string name)
        {
            var items = new List<Tuple<string, string, bool, ExternalCodeCompletionType>>();
            var completionsList = new SortedList<string, ExternalCodeCompletionType>();

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

            return items;
        }

        /// <summary>
        /// Returns the last name from the input line. The regex ignores tabs, spaces, the first new line, etc.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected static string GetLastName(string text)
        {
            return MATCH_LAST_WORD.Match(text.Trim('.').Trim()).Value;
        }

        /// <summary>
        /// Returns the entire namespace from the end of the input line. The regex ignores tabs, spaces, the first new line, etc.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected static string GetLastNameSpace(string text)
        {
            return MATCH_LAST_NAMESPACE.Match(text.Trim('.').Trim()).Value;
        }

        /// <summary>
        /// Returns the first possible type name from the type's declaration line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        protected static string GetFirstPossibleTypeName(string line)
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
        protected static string StripDocStrings(string code)
        {
            var matches = TRIPLE_QUOTE_STRINGS.Split(code);
            return String.Join("", matches);
        }

        /// <summary>
        /// Detect all library references given the provided code
        /// </summary>
        /// <param name="code">Script code to search for CLR references</param>
        /// <returns></returns>
        protected static List<string> FindClrReferences(string code)
        {
            var statements = new List<string>();
            foreach (var line in code.Split(new[] { '\n', ';' }))
            {
                if (line.Contains(clrReference))
                {
                    statements.Add(line.Trim());
                }
            }

            return statements;
        }

        /// <summary>
        /// Check if a full type name is found in one of the known pre-loaded assemblies and return the type
        /// </summary>
        /// <param name="name">a full type name</param>
        /// <returns></returns>
        protected static Type TryGetTypeFromFullName(string name)
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

        protected abstract object EvaluateScript(string script, PythonScriptType type);

        protected abstract void LogError(string msg);

        protected internal abstract bool ScopeHasVariable(string name);

        protected abstract Type GetCLRType(string name);

        /// <summary>
        /// Find all import statements and import into scope.  If the type is already in the scope, this will be skipped.
        /// </summary>
        /// <param name="code">The code to discover the import statements.</param>
        protected internal void UpdateImportedTypes(string code)
        {
            // Detect all lib references prior to attempting to import anything
            var refs = FindClrReferences(code);
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
                    string libName = MATCH_FIRST_QUOTED_NAME.Match(statement).Groups[1].Value;

                    //  If the library name cannot be found in the loaded clr modules
                    if (!ClrModules.Contains(libName))
                    {
                        if (statement.Contains("AddReferenceToFileAndPath"))
                        {
                            EvaluateScript(statement, PythonScriptType.SingleStatement);
                            ClrModules.Add(libName);
                            continue;
                        }

                        if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == libName))
                        {
                            EvaluateScript(statement, PythonScriptType.SingleStatement);
                            ClrModules.Add(libName);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogError(String.Format("Failed to reference library: {0}", statement));
                    LogError(e.ToString());
                    BadStatements[statement] = previousTries + 1;
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

                if (name != "*" && (ScopeHasVariable(name) || ImportedTypes.ContainsKey(name)))
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

                    EvaluateScript(statement, PythonScriptType.SingleStatement);

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
                    LogError(String.Format("Failed to load module: {0}, with statement: {1}", memberName, statement));
                    // Log(e.ToString());
                    BadStatements[statement] = previousTries + 1;
                }
            }
        }

        /// <summary>
        /// Traverse the given source code and define variable types based on
        /// the current scope
        /// </summary>
        /// <param name="code">The source code to look through</param>
        internal void UpdateVariableTypes(string code)
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
        protected Dictionary<string, Type> FindAllVariableAssignments(string code)
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
        /// Retrieves the clr Type corresponding to the input paramater name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected Type TryGetType(string name)
        {
            if (ImportedTypes.ContainsKey(name))
            {
                return ImportedTypes[name];
            }

            Type type = null;
            try
            {
                type = GetCLRType(name);
            }
            catch (Exception e)
            {
                LogError(String.Format("Failed to look up type: {0}", name));
                LogError(e.ToString());
            }
            if (type != null)
            {
                ImportedTypes[name] = type;
            }

            return type;
        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     from lib import *
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        internal static Dictionary<string, string> FindAllTypeImportStatements(string code)
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
        internal static Dictionary<string, string> FindTypeSpecificImportStatements(string code)
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
        /// Find a variable assignment of the form "varName = bla" where bla is matched by
        /// the given regex
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <param name="valueRegex">Your regex to match the type</param>
        /// <returns>A dictionary of name to assignment line pairs</returns>
        internal static Dictionary<string, string> FindVariableStatementWithRegex(string code, string valueRegex)
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
        /// Attempts to find all import statements in the code
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A list of tuples that contain the namespace, the module, and the custom name</returns>
        protected static List<Tuple<string, string, string>> FindAllImportStatements(string code)
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
                        foreach (string statement in allStatements)
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
        /// Find a variable assignment of the form "varName = bla" where bla is matched by
        /// the given regex
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <param name="valueRegex">Your regex to match the type</param>
        /// <returns>A dictionary of name to assignment line pairs</returns>
        internal Dictionary<string, Tuple<string, int, Type>> FindAllVariables(string code)
        {
            var variables = new Dictionary<string, Tuple<string, int, Type>>();
            var pattern = variableName + spacesOrNone +
                equalsRegex + spacesOrNone + @"(.*)";
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
        /// Attempts to find import statements that look like
        ///     import lib
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        internal static Dictionary<string, string> FindBasicImportStatements(string code)
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
        #endregion
    }
}
