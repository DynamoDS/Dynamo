using Autodesk.DesignScript.Interfaces;
using Dynamo.Logging;
using Dynamo.PythonServices;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DSIronPython
{
    internal class IronPythonCodeCompletionProviderCore : PythonCodeCompletionProviderCommon, ILegacyPythonCompletionCore, ILogSource, IDisposable
    {
        #region Private Members
        internal static readonly string clrTypeLookup = "clr.GetClrType({0}) if (\"{0}\" in locals() or \"{0}\" in __builtins__) and isinstance({0}, type) else None";

        private ScriptEngine engine;
        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
        private ScriptScope scope;
        #endregion

        #region BACKING LEGACY CLASS DO NOT MODIFY UNTIL 3
        //ILegacyPythonCompletionCore implementations
        //!!!- do not modify these signatures until that class is removed.
        //We do not know if anyone was using that class, but we needed to remove the compile
        //time references between PythonNodeModels and DSIronPython to support dynamically loading
        //python versions.

        //note that the public members below are not really public (this is an internal class)
        //but must be marked that way to satisfy the legacy interface

        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
        public object Engine
        {
            get { return engine; }
            set { engine = (ScriptEngine)value; }
        }
        /// <summary>
        /// The scope used by the engine.  This is where all the loaded symbols
        /// are stored.  It's essentially an environment dictionary.
        /// </summary>
        public object Scope
        {
            get { return scope; }
            set { scope = (ScriptScope)value; }
        }
        /// <summary>
        /// List all of the members in a CLR Namespace
        /// </summary>
        /// <param name="ns">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the namespace</returns>
        public IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembersFromTracker(object nameSpaceTracker, string name)
        {
            var items = new List<Tuple<string, string, bool, ExternalCodeCompletionType>>();
            var ns = nameSpaceTracker as NamespaceTracker;
            foreach (var member in ns)
            {
                if (member.Value is NamespaceTracker)
                {
                    items.Add(Tuple.Create(member.Key, name, false, ExternalCodeCompletionType.Namespace));
                }
                else if (member.Value is FieldTracker)
                {
                    items.Add(Tuple.Create(member.Key, name, false, ExternalCodeCompletionType.Field));
                }
                else if (member.Value is PropertyTracker)
                {
                    items.Add(Tuple.Create(member.Key, name, false, ExternalCodeCompletionType.Property));
                }
                else if (member.Value is TypeTracker)
                {
                    items.Add(Tuple.Create(member.Key, name, false, ExternalCodeCompletionType.Class));
                }
            }

            return items;
        }
        public new IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(Type type, string name)
        {
            return base.EnumerateMembers(type, name);
        }
        /// <summary>
        /// List all of the members in a PythonModule
        /// </summary>
        /// <param name="module">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the module</returns>
        public IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(object module, string name)
        {
            var items = new List<Tuple<string, string, bool, ExternalCodeCompletionType>>();
            var d = (module as PythonModule).Get__dict__();

            foreach (var member in d)
            {
                var completionType = member.Value is BuiltinFunction ? ExternalCodeCompletionType.Method : ExternalCodeCompletionType.Field;
                items.Add(Tuple.Create((string)member.Key, name, false, completionType));
            }

            return items;
        }
        /// <summary>
        /// Recursively lookup a member in a given namespace.
        /// </summary>
        /// <param name="name">A name for a type, possibly delimited by periods.</param>
        /// <param name="nameSpaceTracker">The namespace</param>
        /// <returns>The type as an object</returns>
        public object LookupMember(string name, object nameSpaceTracker)
        {
            if (!(nameSpaceTracker is NamespaceTracker))
            {
                throw new ArgumentException("parameter n must be of type NameSpaceTracker");
            }
            var nst = nameSpaceTracker as NamespaceTracker;
            object varOutput;

            var periodIndex = name.IndexOf('.');
            if (periodIndex == -1)
            {
                if (nst.TryGetValue(name, out varOutput))
                {
                    return varOutput;
                }
                return null;
            }

            var currentName = name.Substring(0, periodIndex);
            var theRest = name.Substring(periodIndex + 1);

            if (nst.TryGetValue(currentName, out varOutput))
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
        public new void UpdateImportedTypes(string code)
        {
            base.UpdateImportedTypes(code);
        }
        public new void UpdateVariableTypes(string code)
        {
            base.UpdateVariableTypes(code);
        }
        public new Dictionary<string, Type> FindAllVariableAssignments(string code)
        {
            return base.FindAllVariableAssignments(code);
        }
        public new Dictionary<string, Tuple<string, int, Type>> FindAllVariables(string code)
        {
            return base.FindAllVariables(code);
        }
        public new Type TryGetType(string name)
        {
            return base.TryGetType(name);
        }
        #endregion

        #region PythonCodeCompletionProviderCommon implementations
        /// <summary>
        /// Generate completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="expand">Determines if the entire namespace should be used</param>
        /// <returns>Return a list of IExternalCodeCompletionData </returns>
        public override IExternalCodeCompletionData[] GetCompletionData(string code, bool expand = false)
        {
            IEnumerable<IronPythonCodeCompletionDataCore> items = new List<IronPythonCodeCompletionDataCore>();

            if (code.Contains("\"\"\""))
            {
                code = StripDocStrings(code);
            }

            UpdateImportedTypes(code);
            UpdateVariableTypes(code);  // Possibly use hindley-milner in the future

            // If expand param is true use the entire namespace from the line of code
            // Else just return the last name of the namespace
            string name = expand ? GetLastNameSpace(code) :
                GetLastName(code);
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    // Attempt to get type using naming
                    Type type = expand ? TryGetTypeFromFullName(name) : TryGetType(name);

                    // CLR type
                    if (type != null)
                    {
                        items = EnumerateMembers(type, name).Select(x => new IronPythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                    }
                    // Variable type
                    else if (VariableTypes.TryGetValue(name, out type))
                    {
                        items = EnumerateMembers(type, name).Select(x => new IronPythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                    }
                    else
                    {
                        var mem = LookupMember(name);
                        var namespaceTracker = mem as NamespaceTracker;

                        // Namespace type
                        if (namespaceTracker != null)
                        {
                            items = EnumerateMembersFromTracker(namespaceTracker, name).Select(x => new IronPythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                        }
                        else
                        {
                            var pythonModule = mem as PythonModule;

                            // Python Module type
                            if (pythonModule != null)
                            {
                                items = EnumerateMembers(pythonModule, name).Select(x => new IronPythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                            }
                            // Python type
                            else if (mem is PythonType)
                            {
                                // Shows static and instance methods in the same way :(
                                var value = ClrModule.GetClrType(mem as PythonType);

                                if (value != null)
                                {
                                    items = EnumerateMembers(value, name).Select(x => new IronPythonCodeCompletionDataCore(x.Item1, x.Item2, x.Item3, x.Item4, this));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
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
        /// Used to determine if this IExternalCodeCompletionProviderCore can provide completions for the given engine.
        /// </summary>
        /// <param name="engineName"></param>
        /// <returns></returns>
        public override bool IsSupportedEngine(string engineName)
        {
            // Do not reference 'PythonEngineManager.IronPython2EngineName' here
            // because it will break compatibility with Dynamo2.13.X
            //
            //if (engineName == PythonEngineManager.IronPython2EngineName)
            if (engineName == "IronPython2")
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
                pythonLibDir = Path.Combine(dynamoCorePath, IronPythonEvaluator.PythonLibName);
            }

            // If IronPython.Std folder is excluded from DynamoCore (which could be user mistake or integrator exclusion)
            if (!Directory.Exists(pythonLibDir))
            {
                // Try to load IronPython from extension package
                pythonLibDir = Path.Combine((new DirectoryInfo(Path.GetDirectoryName(executionPath))).Parent.FullName, IronPythonEvaluator.packageExtraFolderName, IronPythonEvaluator.PythonLibName);
            }

            if (!String.IsNullOrEmpty(pythonLibDir))
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

        protected override object GetDescriptionObject(string docCommand)
        {
            try
            {

                return engine.CreateScriptSourceFromString(docCommand, SourceCodeKind.Expression).Execute(scope);
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
        protected override object EvaluateScript(string script, PythonScriptType evalType)
        {
            switch (evalType)
            {
                case PythonScriptType.Expression:
                    return engine.CreateScriptSourceFromString(script, SourceCodeKind.Expression).Execute(scope);
                case PythonScriptType.SingleStatement:
                    return engine.CreateScriptSourceFromString(script, SourceCodeKind.SingleStatement).Execute(scope);
                case PythonScriptType.Statements:
                    return engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements).Execute(scope);
            }
            return null;
        }

        protected override void LogError(string msg)
        {
            Log(msg);
        }

        protected internal override bool ScopeHasVariable(string name)
        {
            return scope.ContainsVariable(name);
        }

        protected override Type GetCLRType(string name)
        {
            return EvaluateScript(String.Format(clrTypeLookup, name), PythonScriptType.Expression) as Type;
        }
        #endregion

        #region constructor

        public IronPythonCodeCompletionProviderCore()
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            scope = engine.CreateScope();

            VariableTypes = new Dictionary<string, Type>();
            ImportedTypes = new Dictionary<string, Type>();
            ClrModules = new HashSet<string>();
            BadStatements = new Dictionary<string, int>();

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
                        "clr.AddReference('ProtoGeometry')\nfrom Autodesk.DesignScript.Geometry import *\n";

                    engine.CreateScriptSourceFromString(libGImports, SourceCodeKind.Statements).Execute(scope);
                    ClrModules.Add("ProtoGeometry");
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    Log("Failed to load ProtoGeometry types for autocomplete. Python autocomplete will not see Autodesk namespace types.");
                }
            }

           
        }

        #endregion

        #region ILogSource Implementation
        /// <summary>
        /// Raise this event to request loggers log this message.
        /// </summary>
        public event Action<ILogMessage> MessageLogged;

        private void Log( string message)
        {
            MessageLogged?.Invoke(LogMessage.Info(message));
        }
        #endregion

        public void Dispose()
        {}
    }
}
