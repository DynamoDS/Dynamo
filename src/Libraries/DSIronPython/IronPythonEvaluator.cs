using System;
using System.Collections;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.Utilities;
using IronPython.Hosting;

using Microsoft.Scripting.Hosting;

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
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            var amt = Math.Min(bindingNames.Count, bindingValues.Count);

            for (int i = 0; i < amt; i++)
            {
                scope.SetVariable((string)bindingNames[i], InputMarshaler.Marshal(bindingValues[i]));
            }

            try
            {
                OnEvaluationBegin(engine, scope, code, bindingValues);
                engine.CreateScriptSourceFromString(code).Execute(scope);
            }
            catch (Exception e)
            {
                OnEvaluationEnd(false, engine, scope, code, bindingValues);
                var eo = engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(e);
                throw new Exception(error);
            }

            OnEvaluationEnd(true, engine, scope, code, bindingValues);

            var result = scope.ContainsVariable(/*NXLT*/"OUT") ? scope.GetVariable(/*NXLT*/"OUT") : null;

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
