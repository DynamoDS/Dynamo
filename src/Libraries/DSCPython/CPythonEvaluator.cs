﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using DSCPython.Encoders;
using Dynamo.Events;
using Dynamo.Logging;
using Dynamo.Session;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSCPython
{
    /// <summary>
    /// Used to compare DynamoCPythonHandles by their PythonIDs.
    /// </summary>
    internal class DynamoCPythonHandleComparer : IEqualityComparer<DynamoCPythonHandle>
    {

        public bool Equals(DynamoCPythonHandle x, DynamoCPythonHandle y)
        {
            return x.PythonObjectID.Equals(y.PythonObjectID);
        }

        public int GetHashCode(DynamoCPythonHandle obj)
        {
            return obj.PythonObjectID.GetHashCode();
        }
    }

    /// <summary>
    /// This class wraps a PythonNet.PyObj and performs
    /// disposal tasks to make sure the underlying object is removed from
    /// the shared global scope between all CPython scopes.
    /// If you construct an instance of this class manually or
    /// as a consequence of using the CPythonEvaluator.Evaluate method, an instance
    /// of this class is constructed, and is not returned to the DSVM (graph context)
    /// then make sure to call Dispose when you are done with the instance.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    internal class DynamoCPythonHandle : IDisposable
    {
        /// <summary>
        /// A static map of DynamoCPythonHandle counts, is used to avoid removing the underlying python objects from the 
        /// global scope while there are still handles referencing them.
        /// </summary>
        internal static Dictionary<DynamoCPythonHandle, int> HandleCountMap = new Dictionary<DynamoCPythonHandle, int>(new DynamoCPythonHandleComparer());

        /// <summary>
        /// A unique ID that identifies this python object. It's as a lookup
        /// symbol within the global scope to find this python object instance.
        /// </summary>
        internal IntPtr PythonObjectID { get; set; }

        public DynamoCPythonHandle(IntPtr id)
        {
            PythonObjectID = id;
            if (HandleCountMap.ContainsKey(this))
            {
                HandleCountMap[this] = HandleCountMap[this] + 1;
            }
            else
            {
                HandleCountMap.Add(this, 1);
            }
        }

        public override string ToString()
        {
            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                using (Py.GIL())
                {
                    var scope = PyScopeManager.Global.Get(CPythonEvaluator.globalScopeName);
                    var pyobj = scope.Get(PythonObjectID.ToString());
                    return pyobj.ToString();
                }
            }
            catch (Exception e)
            {
                //TODO(DYN-2941) implement Ilogsource or pass logger to python eval so can log to Dynamo console here. 
                System.Diagnostics.Debug.WriteLine($"error getting string rep of pyobj {this.PythonObjectID} {e.Message}");
                return this.PythonObjectID.ToString();
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }

        /// <summary>
        /// When this handle goes out of scope
        /// we should remove the pythonObject from the globalScope.
        /// In most cases the DSVM will call this.
        /// </summary>
        public void Dispose()
        {
            //key doesn't exist.
            if (!HandleCountMap.ContainsKey(this))
            {
                return;
            }
            //there are more than 1 reference left, don't dispose.
            //decrement refs
            if (HandleCountMap[this] > 1)
            {
                HandleCountMap[this] = HandleCountMap[this] - 1;
                return;
            }

            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                using (Py.GIL())
                {
                    PyScopeManager.Global.Get(CPythonEvaluator.globalScopeName).Remove(PythonObjectID.ToString());
                    HandleCountMap.Remove(this);
                }
            }
            catch (Exception e)
            {
                //TODO(DYN-2941) implement Ilogsource or pass logger to python eval so can log to Dynamo console here. 
                System.Diagnostics.Debug.WriteLine($"error removing python object from global scope {e.Message}");
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }
    }

    [SupressImportIntoVM]
    [Obsolete("Do not use! This will be subject to changes in a future version of Dynamo")]
    public enum EvaluationState { Begin, Success, Failed }

    [SupressImportIntoVM]
    [Obsolete("Do not use! This will be subject to changes in a future version of Dynamo")]
    public delegate void EvaluationEventHandler(EvaluationState state, PyScope scope, string code, IList bindingValues);

    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class CPythonEvaluator
    {
        private const string DynamoSkipAttributeName = "__dynamoskipconversion__";
        private const string DynamoPrintFuncName = "__dynamoprint__";
        private const string NodeName = "__dynamonodename__";
        static PyScope globalScope;
        internal static readonly string globalScopeName = "global";

        static CPythonEvaluator()
        {
            InitializeEncoders();
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
        public static object EvaluatePythonScript(
            string code,
            IList bindingNames,
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            var evaluationSuccess = true;
            if (code == null)
            {
                return null;
            }

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
                    if (globalScope == null)
                    {
                        globalScope = CreateGlobalScope();
                    }

                    using (PyScope scope = Py.CreateScope())
                    {
                        // Reset the 'sys.path' value to the default python paths on node evaluation. 
                        var pythonNodeSetupCode = "import sys" + Environment.NewLine + "sys.path = sys.path[0:3]";
                        scope.Exec(pythonNodeSetupCode);

                        ProcessAdditionalBindings(scope, bindingNames, bindingValues);

                        int amt = Math.Min(bindingNames.Count, bindingValues.Count);

                        for (int i = 0; i < amt; i++)
                        {
                            scope.Set((string)bindingNames[i], InputMarshaler.Marshal(bindingValues[i]).ToPython());
                        }

                        try
                        {
                            OnEvaluationBegin(scope, code, bindingValues);
                            scope.Exec(code);
                            var result = scope.Contains("OUT") ? scope.Get("OUT") : null;

                            return OutputMarshaler.Marshal(result);
                        }
                        catch (Exception e)
                        {
                            evaluationSuccess = false;
                            var traceBack = GetTraceBack(e);
                            if (!string.IsNullOrEmpty(traceBack))
                            {
                                // Throw a new error including trace back info added to the message
                                throw new InvalidOperationException($"{e.Message} {traceBack}", e);
                            }
                            else
                            {
                                throw e;
                            }
                        }
                        finally
                        {
                            OnEvaluationEnd(evaluationSuccess, scope, code, bindingValues);
                        }
                    }
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }

        /// <summary>
        /// Creates and initializaes the global Python scope.
        /// </summary>
        private static PyScope CreateGlobalScope()
        {
            var scope = Py.CreateScope(globalScopeName);
            // Allows discoverability of modules by inspecting their attributes
            scope.Exec(@"
import clr
clr.setPreload(True)
");

            return scope;
        }

        /// <summary>
        /// Processes additional bindings that are not actual inputs.
        /// Currently, only the node name is received in this way.
        /// </summary>
        /// <param name="scope">Python scope where execution will occur</param>
        /// <param name="bindingNames">List of binding names received for evaluation</param>
        /// <param name="bindingValues">List of binding values received for evaluation</param>
        private static void ProcessAdditionalBindings(PyScope scope, IList bindingNames, IList bindingValues)
        {
            const string NodeNameInput = "Name";
            string nodeName;
            if (bindingNames.Count == 0 || !bindingNames[0].Equals(NodeNameInput))
            {
                // Defensive code to fallback in case the additional binding is not there, like
                // when the evaluator is called directly in unit tests.
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
                scope.Set(DynamoPrintFuncName, logFunction.ToPython());
                scope.Exec(RedirectPrint());
            }
        }

        private static string RedirectPrint()
        {
            return string.Format(@"
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

        /// <summary>
        /// Registers custom encoders and decoders with the Python.NET runtime
        /// </summary>
        private static void InitializeEncoders()
        {
            var shared = new object[] { new BigIntegerEncoderDecoder(), new ListEncoderDecoder() };
            var encoders = shared.Cast<IPyObjectEncoder>().ToArray();
            var decoders = shared.Cast<IPyObjectDecoder>().Concat(new IPyObjectDecoder[]
            {
                new DictionaryDecoder()
            }).ToArray();
            Array.ForEach(encoders, e => PyObjectConversions.RegisterEncoder(e));
            Array.ForEach(decoders, d => PyObjectConversions.RegisterDecoder(d));
        }

        /// <summary>
        /// Gets the trace back message from the exception, if it is a PythonException.
        /// </summary>
        /// <param name="e">Exception to inspect</param>
        /// <returns>Trace back message</returns>
        private static string GetTraceBack(Exception e)
        {
            var pythonExc = e as PythonException;
            if (!(e is PythonException))
            {
                return null;
            }

            // Return the value of the trace back field (private)
            var field = typeof(PythonException).GetField("_tb", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new NotSupportedException(Properties.Resources.InternalErrorTraceBackInfo);
            }
            return field.GetValue(pythonExc).ToString();
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
                        delegate (IList lst)
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

                    inputMarshaler.RegisterMarshaler(
                       delegate (DynamoCPythonHandle handle)
                       {
                           var scope = PyScopeManager.Global.Get(globalScopeName);
                           return scope.Get(handle.PythonObjectID.ToString());
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
                            // First, check if we are dealing with a wrapped .NET object.
                            // This simplifies the cases that come afterwards, as wrapped
                            // .NET collections pass some Python checks but not others. 
                            var clrObj = pyObj.GetManagedObject();
                            if (clrObj != null)
                            {
                                return outputMarshaler.Marshal(clrObj);
                            }

                            if (IsMarkedToSkipConversion(pyObj))
                            {
                                return GetDynamoCPythonHandle(pyObj);
                            }

                            // Dictionaries are iterable, so they should come first
                            if (PyDict.IsDictType(pyObj))
                            {
                                using (var pyDict = new PyDict(pyObj))
                                {
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
                            }
                            // Other iterables should become lists, except for strings
                            if (PyIter.IsIterable(pyObj) && !PyString.IsStringType(pyObj))
                            {
                                using (var pyList = PyList.AsList(pyObj))
                                {
                                    var list = new List<object>();
                                    foreach (PyObject item in pyList)
                                    {
                                        list.Add(outputMarshaler.Marshal(item));
                                    }
                                    return list;
                                }
                            }
                            // Special case for big long values: decode them as BigInteger
                            if (PyLong.IsLongType(pyObj))
                            {
                                using (var pyLong = PyLong.AsLong(pyObj))
                                {
                                    try
                                    {
                                        return pyLong.ToInt64();
                                    }
                                    catch (PythonException exc) when (exc.Message.StartsWith("OverflowError"))
                                    {
                                        return pyLong.ToBigInteger();
                                    }
                                }
                            }
                            // Default handling for other Python objects
                            var unmarshalled = pyObj.AsManagedObject(typeof(object));
                            if (unmarshalled is PyObject)
                            {
                                using (unmarshalled as PyObject)
                                {
                                    if (unmarshalled.Equals(pyObj))
                                    {
                                        return GetDynamoCPythonHandle(pyObj);
                                    }
                                    else
                                    {
                                        return outputMarshaler.Marshal(unmarshalled);
                                    }
                                }
                            }

                            return outputMarshaler.Marshal(unmarshalled);
                        });
                }
                return outputMarshaler;
            }
        }

        private static DynamoCPythonHandle GetDynamoCPythonHandle(PyObject pyObj)
        {
            var globalScope = PyScopeManager.Global.Get(globalScopeName);
            globalScope.Set(pyObj.Handle.ToString(), pyObj);
            return new DynamoCPythonHandle(pyObj.Handle);
        }

        private static bool IsMarkedToSkipConversion(PyObject pyObj)
        {
            return pyObj.HasAttr(DynamoSkipAttributeName);
        }

        private static DataMarshaler inputMarshaler;
        private static DataMarshaler outputMarshaler;

        #endregion

        #region Evaluation events

        /// <summary>
        ///     Emitted immediately before execution begins
        /// </summary>
        [SupressImportIntoVM]
        [Obsolete("Do not use! This will be subject to changes in a future version of Dynamo")]
        public static event EvaluationEventHandler EvaluationBegin;

        /// <summary>
        ///     Emitted immediately after execution ends or fails
        /// </summary>
        [SupressImportIntoVM]
        [Obsolete("Do not use! This will be subject to changes in a future version of Dynamo")]
        public static event EvaluationEventHandler EvaluationEnd;

        /// <summary>
        /// Called immediately before evaluation starts
        /// </summary>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to be evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private static void OnEvaluationBegin(PyScope scope,
                                              string code,
                                              IList bindingValues)
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
        private static void OnEvaluationEnd(bool isSuccessful,
                                            PyScope scope,
                                            string code,
                                            IList bindingValues)
        {
            if (EvaluationEnd != null)
            {
                EvaluationEnd(isSuccessful ? EvaluationState.Success : EvaluationState.Failed,
                    scope, code, bindingValues);
            }
        }

        #endregion

    }
}
