using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using DSCPython.Encoders;
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
            if (x.PythonObjectID.Equals(y.PythonObjectID)) return true;
            return false;
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
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class DynamoCPythonHandle : IDisposable
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
                        globalScope = Py.CreateScope(globalScopeName);
                    }
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
                            var result = scope.Contains("OUT") ? scope.Get("OUT") : null;

                            return OutputMarshaler.Marshal(result);
                        }
                        catch (Exception e)
                        {
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
                            OnEvaluationEnd(false, scope, code, bindingValues);
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
                            if (PyList.IsListType(pyObj))
                            {
                                using (var pyList = new PyList(pyObj))
                                {
                                    var list = new List<object>();
                                    foreach (PyObject item in pyList)
                                    {
                                        list.Add(outputMarshaler.Marshal(item));
                                    }
                                    return list;
                                }
                            }
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

                            var unmarshalled = pyObj.AsManagedObject(typeof(object));
                            if (unmarshalled is PyObject)
                            {
                                using (unmarshalled as PyObject)
                                {
                                    if (unmarshalled.Equals(pyObj))
                                    {
                                        var globalScope = PyScopeManager.Global.Get(globalScopeName);
                                        //try moving object to global scope
                                        globalScope.Set(pyObj.Handle.ToString(), pyObj);

                                        return new DynamoCPythonHandle(pyObj.Handle);
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
