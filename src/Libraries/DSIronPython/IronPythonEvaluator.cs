using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Dynamo.Events;
using Dynamo.Logging;
using Dynamo.PythonServices;
using Dynamo.PythonServices.EventHandlers;
using Dynamo.Session;
using Dynamo.Utilities;
using IronPython.Hosting;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace DSIronPython
{
    [SupressImportIntoVM]
    [Obsolete("Deprecated. Please use Dynamo.PythonServices.EvaluationState instead.")]
    public enum EvaluationState { Begin, Success, Failed }

    [SupressImportIntoVM]
    [Obsolete("Deprecated. Please use evaluation handlers from Dynamo.PythonServices instead.")]
    public delegate void EvaluationEventHandler(EvaluationState state,
                                                ScriptEngine engine,
                                                ScriptScope scope,
                                                string code,
                                                IList bindingValues);

    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class IronPythonEvaluator : Dynamo.PythonServices.PythonEngine
    {
        private const string DynamoPrintFuncName = "__dynamoprint__";
        /// <summary> stores a copy of the previously executed code</summary>
        private static string prev_code { get; set; }
        /// <summary> stores a copy of the previously compiled engine</summary>
        private static ScriptSource prev_script { get; set; }

        /// <summary> stores a reference to path of IronPython std lib</summary>
        private static string pythonLibDir { get; set; }

        /// <summary>
        /// Name of IronPython Std lib
        /// </summary>
        public const string PythonLibName = @"IronPython.StdLib.2.7.9";

        /// <summary>
        /// extra folder name in package folder
        /// </summary>
        public const string packageExtraFolderName = @"extra";

        // Do not reference 'PythonEngineManager.IronPython2EngineName' here
        // because it will break compatibility with Dynamo2.13.X
        //
        //public override string Name => PythonEngineManager.IronPython2EngineName;
        
        /// <summary>
        /// Returns the name of this Python engine implementation.
        /// This name will appear in the Dynamo's UI.
        /// </summary>
        public override string Name => "IronPython2";

        /// <summary>
        /// Use Lazy&lt;PythonEngineManager&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        private static readonly Lazy<IronPythonEvaluator>
            lazy =
            new Lazy<IronPythonEvaluator>
            (() => new IronPythonEvaluator());

        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        internal static IronPythonEvaluator Instance { get { return lazy.Value; } }

        /// <summary>
        /// Attempts to build a path referencing the Python Standard Library,
        /// returns null if unable to retrieve a valid path.
        /// </summary>
        /// <returns>path to the Python Standard Library in Dynamo Core</returns>
        private static string PythonStandardLibPath()
        {
            // Attempt to get and cache the Dynamo Core directory path
            if (string.IsNullOrEmpty(pythonLibDir))
            {
                // Gather executing location, this could be DynamoCore folder or extension bin folder
                var executionPath = Assembly.GetExecutingAssembly().Location;

                // Assume the Python Standard Library is available in the DynamoCore path
                pythonLibDir = Path.Combine(Path.GetDirectoryName(executionPath), PythonLibName);

                // If IronPython.Std folder is excluded from DynamoCore (which could be user mistake or integrator exclusion)
                if (!Directory.Exists(pythonLibDir))
                {
                    // Try to load IronPython from extension package
                    pythonLibDir = Path.Combine((new DirectoryInfo(Path.GetDirectoryName(executionPath))).Parent.FullName, packageExtraFolderName, PythonLibName);
                }
            }
            return pythonLibDir;
        }

        /// <summary>
        ///     Executes a Python script with custom variable names. Script may be a string
        ///     read from a file, for example. Pass a list of names (matching the variable
        ///     names in the script) to bindingNames and pass a corresponding list of values
        ///     to bindingValues.
        /// </summary>
        /// <param name="code">Python script as a string.</param>
        /// <param name="bindingNames">Names of values referenced in Python script.</param>
        /// <param name="bindingValues">Values referenced in Python script.</param>
        public override object Evaluate(
            string code,
            IList bindingNames,
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            // TODO - it would be nice if users could modify a preference
            // setting enabling the ability to load additional paths

            // Container for paths that will be imported in the PythonEngine
            List<string> paths = new List<string>();

            // Attempt to get the Standard Python Library
            string stdLib = PythonStandardLibPath();

            if (code != prev_code)
            {
                ScriptEngine PythonEngine = Python.CreateEngine();
                if (!string.IsNullOrEmpty(stdLib))
                {
                    paths = PythonEngine.GetSearchPaths().ToList();
                    paths.Add(stdLib);
                }

                // If any paths were successfully retrieved, append them
                if (paths.Count > 0)
                {
                    PythonEngine.SetSearchPaths(paths);
                }

                ScriptSource script = PythonEngine.CreateScriptSourceFromString(code);
                script.Compile();
                prev_script = script;
                prev_code = code;
            }

            ScriptEngine engine = prev_script.Engine;
            ScriptScope scope = engine.CreateScope();
            // For backwards compatibility: "sys" was imported by default due to a bug so we keep it that way
            scope.ImportModule("sys");

            ProcessAdditionalBindings(scope, bindingNames, bindingValues, engine);

            int amt = Math.Min(bindingNames.Count, bindingValues.Count);

            for (int i = 0; i < amt; i++)
            {
                scope.SetVariable((string)bindingNames[i], InputMarshaler.Marshal(bindingValues[i]));
            }

            try
            {
                OnEvaluationBegin(engine, scope, code, bindingValues);
                prev_script.Execute(scope);
            }
            catch (Exception e)
            {
                OnEvaluationEnd(false, engine, scope, code, bindingValues);
                var eo = engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(e);
                throw new Exception(error);
            }

            OnEvaluationEnd(true, engine, scope, code, bindingValues);

            var result = scope.ContainsVariable("OUT") ? scope.GetVariable("OUT") : null;

            return OutputMarshaler.Marshal(result);
        }

        public static object EvaluateIronPythonScript(
            string code,
            IList bindingNames,
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            return Instance.Evaluate(code, bindingNames, bindingValues);
        }

        /// <summary>
        /// Processes additional bindings that are not actual inputs.
        /// Currently, only the node name is received in this way.
        /// </summary>
        /// <param name="scope">Python scope where execution will occur</param>
        /// <param name="bindingNames">List of binding names received for evaluation</param>
        /// <param name="bindingValues">List of binding values received for evaluation</param>
        private static void ProcessAdditionalBindings(ScriptScope scope, IList bindingNames, IList bindingValues, ScriptEngine engine)
        {
            const string NodeNameInput = "Name";
            string nodeName;
            if (bindingNames.Count == 0 || !bindingNames[0].Equals(NodeNameInput))
            {
                // Defensive code to fallback in case the additional binding is not there, like
                // when the evaluator is called directly in tests, passing bindings manually.
                nodeName = "USER";
            }
            else
            {
                bindingNames.RemoveAt(0);
                nodeName = (string)bindingValues[0];
                bindingValues.RemoveAt(0);
            }

            // Session is null when running unit tests.
            if (ExecutionEvents.ActiveSession != null)
            {
                dynamic logger = ExecutionEvents.ActiveSession.GetParameterValue(ParameterKeys.Logger);
                Action<string> logFunction = msg => logger.Log($"{nodeName}: {msg}", LogLevel.ConsoleOnly);
                scope.SetVariable(DynamoPrintFuncName, logFunction);
                ScriptSource source = engine.CreateScriptSourceFromString(RedirectPrint());
                source.Execute(scope);
            }
        }

        private static string RedirectPrint()
        {
            return String.Format(@"
import sys

class DynamoStdOut:
  def __init__(self, log_func):
    self.text = ''
    self.log_func = log_func
  def write(self, text):
    if text == '\n':
      self.log_func(self.text)
      self.text = ''
    else:
      self.text += text
sys.stdout = DynamoStdOut({0})
", DynamoPrintFuncName);
        }

        #region Marshalling

        /// <summary>
        ///     Data Marshaler for all data coming into a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public override object InputDataMarshaler
        {
            get
            {
                if (inputMarshaler == null)
                {
                    inputMarshaler = new DataMarshaler();
                    inputMarshaler.RegisterMarshaler(
                        delegate(IList lst)
                        {
                            var pyList = new IronPython.Runtime.List();
                            foreach (var item in lst.Cast<object>().Select(inputMarshaler.Marshal))
                            {
                                pyList.Add(item);
                            }
                            return pyList;
                        });
                    inputMarshaler.RegisterMarshaler(
                        delegate (DesignScript.Builtin.Dictionary dict)
                        {
                            var pyDict = new IronPython.Runtime.PythonDictionary();
                            foreach (var key in dict.Keys)
                            {
                                pyDict.Add(inputMarshaler.Marshal(key), inputMarshaler.Marshal(dict.ValueAtKey(key)));
                            }
                            return pyDict;
                        });
                }
                return inputMarshaler;
            }
        }

        /// <summary>
        ///     Data Marshaler for all data coming into a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public static DataMarshaler InputMarshaler => Instance.InputDataMarshaler as DataMarshaler;

        /// <summary>
        ///     Data Marshaler for all data coming out of a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public override object OutputDataMarshaler
        {
            get { return outputMarshaler ?? (outputMarshaler = new DataMarshaler()); }
        }

        /// <summary>
        ///     Data Marshaler for all data coming out of a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public static DataMarshaler OutputMarshaler => Instance.OutputDataMarshaler as DataMarshaler;

        private static DataMarshaler inputMarshaler;
        private static DataMarshaler outputMarshaler;

        #endregion

        #region Evaluation events

        /// <summary>
        ///     Emitted immediately before execution begins
        /// </summary>
        [SupressImportIntoVM]
        [Obsolete("Deprecated. Please use EvaluationStarted instead.")]
        public static event EvaluationEventHandler EvaluationBegin;

        /// <summary>
        ///     Emitted immediately before execution begins
        /// </summary>
        [SupressImportIntoVM]
        public override event EvaluationStartedEventHandler EvaluationStarted;

        /// <summary>
        ///     Emitted immediately after execution ends or fails
        /// </summary>
        [SupressImportIntoVM]
        [Obsolete("Deprecated. Please use EvaluationFinished instead.")]
        public static event EvaluationEventHandler EvaluationEnd;

        /// <summary>
        ///     Emitted immediately after execution ends or fails
        /// </summary>
        [SupressImportIntoVM]
        public override event EvaluationFinishedEventHandler EvaluationFinished;

        /// <summary>
        /// Called immediately before evaluation starts
        /// </summary>
        /// <param name="engine">The engine used to do the evaluation</param>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to be evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private void OnEvaluationBegin(  ScriptEngine engine,
                                                ScriptScope scope, 
                                                string code, 
                                                IList bindingValues )
        {
            // Call deprecated events until they are completely removed.
            EvaluationBegin?.Invoke(EvaluationState.Begin, engine, scope, code, bindingValues);
            
            if (EvaluationStarted != null)
            {
                EvaluationStarted(code, bindingValues, (n, v) => { scope.SetVariable(n, InputMarshaler.Marshal(v)); });
                Analytics.TrackEvent(
                    Dynamo.Logging.Actions.End,
                    Dynamo.Logging.Categories.PythonOperations,
                    "IronPythonEvaluation");
            }
        }

        /// <summary>
        /// Called when the evaluation has completed successfully or failed
        /// </summary>
        /// <param name="isSuccessful">Whether the evaluation succeeded or not</param>
        /// <param name="engine">The engine used to do the evaluation</param>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to that was evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private void OnEvaluationEnd( bool isSuccessful,
                                             ScriptEngine engine,
                                             ScriptScope scope,
                                             string code,
                                             IList bindingValues)
        {
            // Call deprecated events until they are completely removed.
            EvaluationEnd?.Invoke(isSuccessful ? EvaluationState.Success : EvaluationState.Failed,
                engine, scope, code, bindingValues);

            if (EvaluationFinished != null)
            {
                EvaluationFinished( isSuccessful ? Dynamo.PythonServices.EvaluationState.Success : Dynamo.PythonServices.EvaluationState.Failed, 
                    code, bindingValues, (n) => { return OutputMarshaler.Marshal(scope.GetVariable(n)); });

                Analytics.TrackEvent(
                    Dynamo.Logging.Actions.End,
                    Dynamo.Logging.Categories.PythonOperations,
                    "IronPythonEvaluation");
            }
        }

        #endregion

    }
}
