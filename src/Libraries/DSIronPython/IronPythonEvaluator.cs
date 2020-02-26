using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Dynamo.Utilities;
using IronPython.Hosting;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace DSIronPython
{
    [SupressImportIntoVM]
    public enum EvaluationState { Begin, Success, Failed }

    [SupressImportIntoVM]
    public delegate void EvaluationEventHandler(EvaluationState state,
                                                ScriptEngine engine,
                                                ScriptScope scope,
                                                string code,
                                                IList bindingValues);

    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class IronPythonEvaluator
    {
        /// <summary> stores a copy of the previously executed code</summary>
        private static string prev_code { get; set; }
        /// <summary> stores a copy of the previously compiled engine</summary>
        private static ScriptSource prev_script { get; set; }
        /// <summary> stores a reference to the executing Dynamo Core directory</summary>
        private static string dynamoCorePath { get; set; }

        /// <summary>
        /// Attempts to build a path referencing the Python Standard Library in Dynamo Core,
        /// returns null if unable to retrieve a valid path.
        /// </summary>
        /// <returns>path to the Python Standard Library in Dynamo Core</returns>
        private static string pythonStandardLibPath()
        {
            // Attempt to get and cache the Dynamo Core directory path
            if(string.IsNullOrEmpty(dynamoCorePath))
            {
                // Gather executing assembly info
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                dynamoCorePath = Path.GetDirectoryName(assembly.Location);
            }

            // Return the standard library path
            if (!string.IsNullOrEmpty(dynamoCorePath))
            { return dynamoCorePath + @"\IronPython.StdLib.2.7.9"; }

            return null;
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
        public static object EvaluateIronPythonScript(
            string code,
            IList bindingNames,
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            // TODO - it would be nice if users could modify a preference
            // setting enabling the ability to load additional paths

            // Container for paths that will be imported in the PythonEngine
            List<string> paths = new List<string>();

            // Attempt to get the Standard Python Library
            string stdLib = pythonStandardLibPath();

            if (code != prev_code)
            {
                ScriptEngine PythonEngine = Python.CreateEngine();
                if (!string.IsNullOrEmpty(stdLib))
                {
                    code = "import sys" + System.Environment.NewLine + code;
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

        #region Marshalling

        /// <summary>
        ///     Data Marshaler for all data coming into a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public static DataMarshaler InputMarshaler
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
        ///     Data Marshaler for all data coming out of a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public static DataMarshaler OutputMarshaler
        {
            get { return outputMarshaler ?? (outputMarshaler = new DataMarshaler()); }
        }

        private static DataMarshaler inputMarshaler;
        private static DataMarshaler outputMarshaler;

        #endregion

        #region Evaluation events

        /// <summary>
        ///     Emitted immediately before execution begins
        /// </summary>
        [SupressImportIntoVM]
        public static event EvaluationEventHandler EvaluationBegin;

        /// <summary>
        ///     Emitted immediately after execution ends or fails
        /// </summary>
        [SupressImportIntoVM]
        public static event EvaluationEventHandler EvaluationEnd;

        /// <summary>
        /// Called immediately before evaluation starts
        /// </summary>
        /// <param name="engine">The engine used to do the evaluation</param>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to be evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private static void OnEvaluationBegin(  ScriptEngine engine, 
                                                ScriptScope scope, 
                                                string code, 
                                                IList bindingValues )
        {
            if (EvaluationBegin != null)
            {
                EvaluationBegin(EvaluationState.Begin, engine, scope, code, bindingValues);
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
        private static void OnEvaluationEnd( bool isSuccessful,
                                            ScriptEngine engine,
                                            ScriptScope scope,
                                            string code,
                                            IList bindingValues)
        {
            if (EvaluationEnd != null)
            {
                EvaluationEnd( isSuccessful ? EvaluationState.Success : EvaluationState.Failed, 
                    engine, scope, code, bindingValues);
            }
        }

        #endregion

    }
}
