using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using PythonNodeModels;

namespace PythonNodeModels
{
    /// <summary>
    /// Enum of possible values of python engine versions.
    /// TODO: Remove when dynamic loading logic is there then we no longer need a hard copy of the options
    /// </summary>
    public enum PythonEngineVersion
    {
        Unspecified,
        IronPython2,
        CPython3
    }
}

namespace Dynamo.PythonServices
{
    [SupressImportIntoVM]
    public enum EvaluationState { Success, Failed }

    public delegate void ScopeSetAction(string name, object value);
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

    /// <summary>
    /// Singleton class that other class can access and use for query loaded Python Engine info.
    /// </summary>
    [SupressImportIntoVM]
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

        internal ObservableCollection<PythonEngineProxy> AvailableEngines;

        #region Constant strings
        // TODO: The following fields might be removed after dynamic loading applied
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
            AvailableEngines = new ObservableCollection<PythonEngineProxy>();

            ScanPythonEngines();
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadEventHandler);
        }

        internal PythonEngineProxy CreateEngineProxy(Assembly assembly, PythonEngineVersion version)
        {
            // Currently we are using try-catch to validate loaded assembly and evaluation method exist
            // but we can optimize by checking all loaded types against evaluators interface later
            try
            {
                if (version == PythonEngineVersion.IronPython2 && assembly.GetName().Name.Equals(IronPythonAssemblyName))
                {
                    var eType = assembly.GetType(IronPythonTypeName);
                    if (eType != null && eType.GetMethod(IronPythonEvaluationMethod) != null)
                    {
                        return new PythonEngineProxy(eType, PythonEngineVersion.IronPython2);
                    }
                }
            }
            catch
            {
                return null;
            }

            try
            {
                if (version == PythonEngineVersion.CPython3 && assembly.GetName().Name.Equals(CPythonAssemblyName))
                {
                    var eType = assembly.GetType(CPythonTypeName);
                    if (eType != null && eType.GetMethod(CPythonEvaluationMethod) != null)
                    {
                        return new PythonEngineProxy(eType, PythonEngineVersion.CPython3);
                    }
                }
            }
            catch
            {
                //Do nothing for now
            }
            return null;
        }

        /// <summary>
        /// Scan loaded Python engines
        /// </summary>
        internal void ScanPythonEngines()
        {
            var assems = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assems)
            {
                ScanPythonEngine(assembly);
            }
        }

        /// <summary>
        /// The shared logic for Python node evaluation
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="evaluatorClass"></param>
        /// <param name="evaluationMethod"></param>
        internal void GetEvaluatorInfo(PythonEngineVersion engine, out string evaluatorClass, out string evaluationMethod)
        {
            // Provide evaluator info when the selected engine is loaded
            if (engine == PythonEngineVersion.IronPython2 &&
                AvailableEngines.Any(x => x.Version == PythonEngineVersion.IronPython2))
            {
                evaluatorClass = IronPythonEvaluatorClass;
                evaluationMethod = IronPythonEvaluationMethod;
                return;
            }
            if (engine == PythonEngineVersion.CPython3 &&
                AvailableEngines.Any(x => x.Version == PythonEngineVersion.CPython3))
            {
                evaluatorClass = CPythonEvaluatorClass;
                evaluationMethod = CPythonEvaluationMethod;
                return;
            }

            // Throwing at the compilation stage is handled as a non-retryable error by the Dynamo engine.
            // Instead, we want to produce an error at the evaluation stage, so we can eventually recover.
            // We handle this by providing a dummy Python evaluator that will evaluate to an error message.
            evaluatorClass = DummyEvaluatorClass;
            evaluationMethod = DummyEvaluatorMethod;
        }

        private void AssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
        {
            PythonEngineManager.Instance.ScanPythonEngine(args.LoadedAssembly);
        }

        private void ScanPythonEngine(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            if (!AvailableEngines.Any(x => x.Version == PythonEngineVersion.IronPython2))
            {
                var engine = CreateEngineProxy(assembly, PythonEngineVersion.IronPython2);
                if (engine != null)
                {
                    AvailableEngines.Add(engine);
                }
            }

            if (!AvailableEngines.Any(x => x.Version == PythonEngineVersion.CPython3))
            {
                var engine = CreateEngineProxy(assembly, PythonEngineVersion.CPython3);
                if (engine != null)
                {
                    AvailableEngines.Add(engine);
                }
            }
        }
    }

    /// <summary>
    /// This class is intended to wrap or act as a proxy for many different python engine versions
    /// </summary>
    [SupressImportIntoVM]
    public class PythonEngineProxy
    {
        /// <summary>
        /// The version of the Python Engine connected to this proxy instance
        /// </summary>
        public readonly PythonEngineVersion Version;

        private readonly Type EngineType;
        private EvaluationStartedEventHandler EvaluationStartedCallback;
        private EvaluationFinishedEventHandler EvaluationFinishedCallback;

        /// <summary>
        /// Add an event handler before the Python evaluation begins
        /// </summary>
        /// <param name="callback"></param>
        public void OnEvaluationBegin(EvaluationStartedEventHandler callback)
        {
            if (EvaluationStartedCallback != null)
            {
                RemoveEventHandler(EvaluationStartedCallback.GetType(), nameof(HandleEvaluationStarted));
            }

            EvaluationStartedCallback = callback;
            AddEventHandler(EvaluationStartedCallback.GetType(), nameof(HandleEvaluationStarted));
        }

        /// <summary>
        /// Add an event handler after the Python evaluation ends
        /// </summary>
        /// <param name="callback"></param>
        public void OnEvaluationEnd(EvaluationFinishedEventHandler callback)
        {
            if (EvaluationFinishedCallback != null)
            {
                RemoveEventHandler(EvaluationFinishedCallback.GetType(), nameof(HandleEvaluationFinished));
            }

            EvaluationFinishedCallback = callback;
            AddEventHandler(EvaluationFinishedCallback.GetType(), nameof(HandleEvaluationFinished));
        }

        internal PythonEngineProxy(Type eType, PythonEngineVersion version)
        {
            EngineType = eType;
            Version = version;
        }

        /// <summary>
        /// Gets the input Marshaler from the target Python Engine
        /// </summary>
        /// <returns>DataMarshaler as object</returns>
        public object GetInputMarshaler()
        {
            try
            {
                return EngineType.GetProperty(PythonEngineManager.PythonInputMarshalerProperty).GetValue(null);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the output Marshaler from the target Python Engine
        /// </summary>
        /// <returns>DataMarshaler as object</returns>
        public object GetOutputMarshaler()
        {
            try
            {
                return EngineType.GetProperty(PythonEngineManager.PythonOutputMarshalerProperty).GetValue(null);
            }
            catch
            {
                return null;
            }
        }

        private void AddEventHandler(Type handlerType, string handlerName)
        {
            try
            {
                var eventInfo = EngineType.GetEvents().FirstOrDefault(x => x.EventHandlerType == handlerType);
                if (eventInfo != null)
                {
                    MethodInfo handlerInfo = typeof(PythonEngineProxy).GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance);
                    var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType,
                                         this,
                                         handlerInfo);
                    eventInfo.AddEventHandler(this, handler);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private void RemoveEventHandler(Type handlerType, string handlerName)
        {
            try
            {
                MethodInfo handlerInfo = typeof(PythonEngineProxy).GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance);
                var handler = Delegate.CreateDelegate(handlerType, this, handlerInfo);
                var eventInfo = EngineType.GetEvents().FirstOrDefault(x => x.EventHandlerType == typeof(EvaluationStartedEventHandler));
                eventInfo?.RemoveEventHandler(this, handler);
            }
            catch { }
        }

        private void HandleEvaluationStarted(string code,
                                             IList bindingValues,
                                             ScopeSetAction scopeSet)
        {
            EvaluationStartedCallback?.Invoke(code, bindingValues, scopeSet);
        }

        private void HandleEvaluationFinished(EvaluationState state,
                                              string code,
                                              IList bindingValues,
                                              ScopeGetAction scopeGet)
        {
            EvaluationFinishedCallback?.Invoke(state, code, bindingValues, scopeGet);
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

    internal static class PythonCodeCompletionUtils
    {
        #region internal constants
        internal static readonly string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s?)+)";
        internal static readonly string variableName = @"([0-9a-zA-Z_]+(\.[a-zA-Z_0-9]+)*)";
        internal static readonly string spacesOrNone = @"(\s*)";
        internal static readonly string atLeastOneSpaceRegex = @"(\s+)";
        internal static readonly string dictRegex = "({.*})";
        internal static readonly string basicImportRegex = @"(import)";
        internal static readonly string fromImportRegex = @"^(from)";

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

        /// <summary>
        /// Returns the last name from the input line. The regex ignores tabs, spaces, the first new line, etc.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string GetLastName(string text)
        {
            return MATCH_LAST_WORD.Match(text.Trim('.').Trim()).Value;
        }

        /// <summary>
        /// Returns the entire namespace from the end of the input line. The regex ignores tabs, spaces, the first new line, etc.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string GetLastNameSpace(string text)
        {
            return MATCH_LAST_NAMESPACE.Match(text.Trim('.').Trim()).Value;
        }

        /// <summary>
        /// Returns the first possible type name from the type's declaration line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string GetFirstPossibleTypeName(string line)
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
        internal static string StripDocStrings(string code)
        {
            var matches = TRIPLE_QUOTE_STRINGS.Split(code);
            return String.Join("", matches);
        }

        /// <summary>
        /// Detect all library references given the provided code
        /// </summary>
        /// <param name="code">Script code to search for CLR references</param>
        /// <returns></returns>
        internal static List<string> FindClrReferences(string code)
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
        /// Check if a full type name is found in one of the known pre-loaded assemblies and return the type
        /// </summary>
        /// <param name="name">a full type name</param>
        /// <returns></returns>
        internal static Type TryGetTypeFromFullName(string name)
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

        //!!!- do not modify these signatures until that class is removed.
        //We do not know if anyone was using that class, but we needed to remove the compile
        //time references between PythonNodeModels and DSCPython to support dynamically loading
        //python versions.

        //note that the public members below are not really public (this is an internal class)
        //but must be marked that way to satisfy the legacy interface
        #region BACKING LEGACY CLASS DO NOT MODIFY UNTIL 3

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

        internal static List<Tuple<string, string, string>> FindAllImportStatements(string code)
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

        [Obsolete("Only used to support legacy IronPythonCodeCompletionProvider - remove in 3.0")]
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
