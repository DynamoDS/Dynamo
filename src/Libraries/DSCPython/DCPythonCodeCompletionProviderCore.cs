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
    internal class DSCPythonCodeCompletionProviderCore : PythonCodeCompletionProviderCommon, ILogSource, IDisposable
    {
        #region Private members

        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
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
        /// List all of the members in a PythonModule
        /// </summary>
        /// <param name="module">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the module</returns>
        private IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembersForModule(object module, string name)
        {
            var items = new List<Tuple<string, string, bool, ExternalCodeCompletionType>>();
            var d = module as PyObject;

            foreach (var member in d.Dir())
            {
                try
                {
                    var completionType = ExternalCodeCompletionType.Field;

                    if (d.GetAttr(member.ToString()).ToString().Contains(inBuiltMethod) ||
                        d.GetAttr(member.ToString()).ToString().Contains(method))
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

        #region PythonCodeCompletionProviderCommon public methods implementations

        /// <summary>
        /// Generate completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="expand">Determines if the entire namespace should be used</param>
        /// <returns>Return a list of IExternalCodeCompletionData </returns>
        public override IExternalCodeCompletionData[] GetCompletionData(string code, bool expand = false)
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
                        code = StripDocStrings(code);
                    }

                    if (Scope == null)
                    {
                        Scope = CreateUniquePythonScope();
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
                            // Attempt to get type using naming
                            Type type = expand ? TryGetTypeFromFullName(name) : TryGetType(name);

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
                                    items = EnumerateMembersForModule(pythonModule, name).Select(x => new PythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
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

        protected override object GetDescriptionObject(string docCommand)
        {
            try
            {
                return ExecutePythonScriptCode(docCommand);
            }
            catch
            {
                //This empty catch block is intentional- 
                //because we are using a python engine to evaluate the completions
                //but this engine has not actually loaded the types, it will throw lots of exceptions
                //we wish to suppress.
            }

            return null;
        }

        /// <summary>
        /// Used to determine if this IExternalCodeCompletionProviderCore can provide completions for the given engine.
        /// </summary>
        /// <param name="engineName"></param>
        /// <returns></returns>
        public override bool IsSupportedEngine(string engineName)
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
        public override void Initialize(string dynamoCorePath)
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

        #region PythonCodeCompletionProviderCommon protected methods implementations
        protected override object EvaluateScript(string script, PythonScriptType evalType)
        {
            switch (evalType)
            {
                case PythonScriptType.Expression:
                    return Scope.Eval(script);
                default:
                    Scope.Exec(script);
                    return null;
            }
        }

        protected override void LogError(string msg)
        {
            Log(msg);
        }

        protected override bool ScopeHasVariable(string name)
        {
            return Scope.Contains(name);
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

            BasicVariableTypes.Add(Tuple.Create(STRING_VARIABLE, typeof(string)));
            BasicVariableTypes.Add(Tuple.Create(DOUBLE_VARIABLE, typeof(double)));
            BasicVariableTypes.Add(Tuple.Create(INT_VARIABLE, typeof(int)));
            BasicVariableTypes.Add(Tuple.Create(LIST_VARIABLE, typeof(PyList)));
            BasicVariableTypes.Add(Tuple.Create(DICT_VARIABLE, typeof(PyDict)));

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