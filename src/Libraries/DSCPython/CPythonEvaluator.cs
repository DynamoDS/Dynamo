using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSCPython
{
    [SupressImportIntoVM]
    internal enum EvaluationState { Begin, Success, Failed }

    [SupressImportIntoVM]
    internal delegate void EvaluationEventHandler(EvaluationState state,
                                                  PyScope scope,
                                                  string code,
                                                  IList bindingValues);

    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class CPythonEvaluator
    {
        /// <summary> stores a copy of the previously executed code</summary>
        private static string prev_code { get; set; }

        /// <summary>
        ///     Executes a Python script with custom variable names. Script may be a string
        ///     read from a file, for example. Pass a list of names (matching the variable
        ///     names in the script) to bindingNames and pass a corresponding list of values
        ///     to bindingValues.
        /// </summary>
        /// <param name="code">Python script as a string.</param>
        /// <param name="bindingNames">Names of values referenced in Python script.</param>
        /// <param name="bindingValues">Values referenced in Python script.</param>
        public static object EvaluatePythonScript(
            string code,
            IList bindingNames,
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            if (code != prev_code)
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
                        using (PyScope scope = Py.CreateScope())
                        {
                            int amt = Math.Min(bindingNames.Count, bindingValues.Count);

                            for (int i = 0; i < amt; i++)
                            {
                                scope.Set((string)bindingNames[i], InputMarshaler.Marshal(bindingValues[i]).ToPython());
                            }

                            try
                            {
                                OnEvaluationBegin(scope, code, bindingValues);
                                scope.Exec(code);
                                OnEvaluationEnd(false, scope, code, bindingValues);

                                var result = scope.Contains("OUT") ? scope.Get("OUT") : null;

                                return OutputMarshaler.Marshal(result);
                            }
                            catch (Exception e)
                            {
                                OnEvaluationEnd(false, scope, code, bindingValues);
                                throw e;
                            }
                        }
                    }
                }
                catch (PythonException pe)
                {
                    throw pe;
                }
                finally
                {
                    PythonEngine.ReleaseLock(gs);
                }
            }
            return null;  
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
                            var pyList = new PyList();
                            foreach (var item in lst.Cast<object>().Select(inputMarshaler.Marshal))
                            {
                                pyList.Append(item.ToPython());
                            }
                            return pyList;
                        });
                    inputMarshaler.RegisterMarshaler(
                        delegate (DesignScript.Builtin.Dictionary dict)
                        {
                            var pyDict = new PyDict();
                            foreach (var key in dict.Keys)
                            {
                                pyDict.SetItem(inputMarshaler.Marshal(key).ToPython(), inputMarshaler.Marshal(dict.ValueAtKey(key)).ToPython());
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
            get
            {
                if (outputMarshaler == null)
                {
                    outputMarshaler = new DataMarshaler();
                    outputMarshaler.RegisterMarshaler(
                        delegate (PyObject pyObj)
                        {
                            if (PyList.IsListType(pyObj))
                            {
                                var pyList = new PyList(pyObj);
                                var list = new List<object>();
                                foreach (PyObject item in pyList)
                                {
                                    list.Add(outputMarshaler.Marshal(item));
                                }
                                return list;
                            }
                            else if (PyDict.IsDictType(pyObj))
                            {
                                var pyDict = new PyDict(pyObj);
                                var dict = new Dictionary<object, object>();
                                foreach (PyObject item in pyDict.Items())
                                {
                                    dict.Add(
                                        outputMarshaler.Marshal(item.GetItem(0)),
                                        outputMarshaler.Marshal(item.GetItem(1))
                                    );
                                }
                                return dict;
                            }
                            else if(PyLong.IsLongType(pyObj))
                            {
                                return PyLong.AsLong(pyObj).ToInt64();
                            }
                            else
                            {
                                return outputMarshaler.Marshal(pyObj.AsManagedObject(typeof(object)));
                            }
                        });
                }
                return outputMarshaler;
            }
            
        }

        private static DataMarshaler inputMarshaler;
        private static DataMarshaler outputMarshaler;

        #endregion

        #region Evaluation events

        /// <summary>
        ///     Emitted immediately before execution begins
        /// </summary>
        [SupressImportIntoVM]
        internal static event EvaluationEventHandler EvaluationBegin;

        /// <summary>
        ///     Emitted immediately after execution ends or fails
        /// </summary>
        [SupressImportIntoVM]
        internal static event EvaluationEventHandler EvaluationEnd;

        /// <summary>
        /// Called immediately before evaluation starts
        /// </summary>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to be evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private static void OnEvaluationBegin(  PyScope scope, 
                                                string code, 
                                                IList bindingValues )
        {
            if (EvaluationBegin != null)
            {
                EvaluationBegin(EvaluationState.Begin, scope, code, bindingValues);
            }
        }

        /// <summary>
        /// Called when the evaluation has completed successfully or failed
        /// </summary>
        /// <param name="isSuccessful">Whether the evaluation succeeded or not</param>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to that was evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private static void OnEvaluationEnd( bool isSuccessful,
                                            PyScope scope,
                                            string code,
                                            IList bindingValues)
        {
            if (EvaluationEnd != null)
            {
                EvaluationEnd( isSuccessful ? EvaluationState.Success : EvaluationState.Failed, 
                    scope, code, bindingValues);
            }
        }

        #endregion

    }
}
