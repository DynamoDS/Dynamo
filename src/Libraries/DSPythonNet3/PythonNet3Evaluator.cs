using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using DSPythonNet3.Encoders;
using Dynamo.Events;
using Dynamo.Logging;
using Dynamo.PythonServices.EventHandlers;
using Dynamo.Session;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSPythonNet3
{
    /// <summary>
    /// Used to compare DynamoPythonNet3Handles by their PythonIDs.
    /// </summary>
    internal class DynamoPythonNet3HandleComparer : IEqualityComparer<DynamoPythonNet3Handle>
    {

        public bool Equals(DynamoPythonNet3Handle x, DynamoPythonNet3Handle y)
        {
            return x.PythonObjectID.Equals(y.PythonObjectID);
        }

        public int GetHashCode(DynamoPythonNet3Handle obj)
        {
            return obj.PythonObjectID.GetHashCode();
        }
    }

    /// <summary>
    /// This class wraps a PythonNet.PyObj and performs
    /// disposal tasks to make sure the underlying object is removed from
    /// the shared global scope between all PythonNet scopes.
    /// If you construct an instance of this class manually or
    /// as a consequence of using the PythonNet3Evaluator.Evaluate method, an instance
    /// of this class is constructed, and is not returned to the DSVM (graph context)
    /// then make sure to call Dispose when you are done with the instance.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    internal class DynamoPythonNet3Handle : IDisposable
    {
        /// <summary>
        /// A static map of DynamoPythonNet3Handle counts, is used to avoid removing the underlying python objects from the 
        /// global scope while there are still handles referencing them.
        /// </summary>
        internal static Dictionary<DynamoPythonNet3Handle, int> HandleCountMap = new Dictionary<DynamoPythonNet3Handle, int>(new DynamoPythonNet3HandleComparer());

        /// <summary>
        /// A unique ID that identifies this python object. It's as a lookup
        /// symbol within the global scope to find this python object instance.
        /// </summary>
        internal IntPtr PythonObjectID { get; set; }

        public DynamoPythonNet3Handle(IntPtr id)
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
            var gs = Py.GIL();
            try
            {
                var pyobj = PythonNet3Evaluator.globalScope.Get(PythonObjectID.ToString());
                return pyobj.ToString();
            }
            catch (Exception e)
            {
                PythonNet3Evaluator.DynamoLogger?.Log($"error getting string rep of pyobj {this.PythonObjectID} {e.Message}");
                return this.PythonObjectID.ToString();
            }
            finally
            {
                gs.Dispose();
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

            var gs = Py.GIL();
            try
            {
                PythonNet3Evaluator.globalScope.Remove(PythonObjectID.ToString());
                HandleCountMap.Remove(this);
            }
            catch (Exception e)
            {
                PythonNet3Evaluator.DynamoLogger?.Log($"error removing python object from global scope {e.Message}");
            }
            finally
            {
                gs.Dispose();
            }
        }
    }


    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class PythonNet3Evaluator : Dynamo.PythonServices.PythonEngine
    {
        private const string DynamoSkipAttributeName = "__dynamoskipconversion__";
        private const string DynamoPrintFuncName = "__dynamoprint__";
        private const string NodeName = "__dynamonodename__";
        internal static readonly string globalScopeName = "global";

        internal static PyModule globalScope;
        private static DynamoLogger dynamoLogger;
        private static string path;

        internal static DynamoLogger DynamoLogger
        {
            get
            { // Session is null when running unit tests.
                if (ExecutionEvents.ActiveSession != null)
                {
                    dynamoLogger = ExecutionEvents.ActiveSession.GetParameterValue(ParameterKeys.Logger) as DynamoLogger;
                    return dynamoLogger;
                }
                return dynamoLogger;
            }
        }

        /// <summary>
        /// Use Lazy&lt;PythonEngineManager&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        private static readonly Lazy<PythonNet3Evaluator>
            lazy =
            new Lazy<PythonNet3Evaluator>
            (() => new PythonNet3Evaluator());

        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        internal static PythonNet3Evaluator Instance { get { return lazy.Value; } }

        static PythonNet3Evaluator()
        {
            InitializeEncoders();
            Dynamo.Models.DynamoModel.RequestPythonReset += RequestPythonResetHandler;
        }

        public override string Name => Dynamo.PythonServices.PythonEngineManager.PythonNet3EngineName;

        internal static void RequestPythonResetHandler(string pythonEngine)
        {
            //check if python engine request is for this engine, and engine is currently started
            if (PythonEngine.IsInitialized && pythonEngine == Instance.Name)
            {
                DynamoLogger?.Log("attempting reload of pythonnet3 modules", LogLevel.Console);
                using (Py.GIL())
                {
                    //the following is inspired by:
                    //https://github.com/ipython/ipython/blob/master/IPython/extensions/autoreload.py
                    globalScope?.Exec(@"import sys
import importlib
import importlib.util
import os
## Reloading sys, __main__, builtins and other key modules is not recommended.
## don't reload modules without file attributes
def should_reload(module):
    if not hasattr(module, '__file__') or module.__file__ is None:
        return None
## don't reload modules that are currently running ie __main__, __mp_main__ is renamed by multiprocessing module.
    if getattr(module, '__name__', None) in [None, '__mp_main__', '__main__']:
        # we cannot reload(__main__) or reload(__mp_main__)
        return None

    filename = module.__file__
    path, ext = os.path.splitext(filename)
## only reload .py files.
    if ext.lower() == '.py':
        py_filename = filename
    else:
        try:
            py_filename = importlib.util.source_from_cache(filename)
        except ValueError:
            return None
    return py_filename

## copy sys.modules so it's not modified during reload.
for modname,mod in sys.modules.copy().items():
    print('considering', modname)
    file = should_reload(mod)
    if file is None:
        continue
    print('reloading', modname)
    try:
        importlib.reload(mod)
    except Exception as inst:
        print('failed to reload', modname, inst)

");
                }
                Analytics.TrackEvent(
                   Dynamo.Logging.Actions.Start,
                   Dynamo.Logging.Categories.PythonOperations,
                   "PythonNet3Reset");
            }
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
            var evaluationSuccess = true;
            if (code == null)
            {
                return null;
            }

            InstallPython();
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();

                using (Py.GIL())
                using (PyModule scope = Py.CreateScope())
                {
                    scope.Exec("import sys" + Environment.NewLine + "path = str(sys.path)");
                    path = scope.Get<string>("path");
                }
            }

            using (Py.GIL())
            {
                globalScope ??= CreateGlobalScope();
                using (PyModule scope = Py.CreateScope())
                {
                    if (path is not null)
                    {
                        // Reset the 'sys.path' value to the default python paths on node evaluation. See https://github.com/DynamoDS/Dynamo/pull/10977. 
                        var pythonNodeSetupCode = "import sys" + Environment.NewLine + $"sys.path = {path}";
                        scope.Exec(pythonNodeSetupCode);
                    }

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
#pragma warning disable CA2200 // Rethrow to preserve stack details
                            throw e;
#pragma warning restore CA2200 // Rethrow to preserve stack details
                        }
                    }
                    finally
                    {
                        OnEvaluationEnd(evaluationSuccess, scope, code, bindingValues);
                    }
                }
            }
        }

        public static object EvaluatePythonScript(
            string code,
            IList bindingNames,
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            return Instance.Evaluate(code, bindingNames, bindingValues);
        }

        private static bool isPythonInstalled = false;
        /// <summary>
        /// Makes sure Python is installed on the system and it's location added to the path.
        /// NOTE: Calling SetupPython multiple times will add the install location to the path many times,
        /// potentially causing the environment variable to overflow.
        /// </summary>
        internal static void InstallPython()
        {
            if (!isPythonInstalled)
            {
                Python.Included.Installer.SetupPython();
                isPythonInstalled = true;
            }
        }

        /// <summary>
        /// Creates and initializes the global Python scope.
        /// </summary>
        private static PyModule CreateGlobalScope()
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
        private static void ProcessAdditionalBindings(PyModule scope, IList bindingNames, IList bindingValues)
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
                var logger = DynamoLogger;
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
            var shared = new object[]
            {
                new BigIntegerEncoderDecoder(),
                new ListEncoderDecoder()
            };
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
            if (e is not PythonException pythonExc || pythonExc.Traceback == null)
            {
                return null;
            }

            // Return the value of the trace back field (private)
            var field = typeof(PythonException).GetMethod("TracebackToString", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                throw new NotSupportedException(Properties.Resources.InternalErrorTraceBackInfo);
            }

            return (string?)field.Invoke(pythonExc, [pythonExc.Traceback]);

            //var pythonExc = e as PythonException;
            //if (!(e is PythonException))
            //{
            //    return null;
            //}

            //// Return the value of the trace back field (private)
            //var field = typeof(PythonException).GetField("_tb", BindingFlags.NonPublic | BindingFlags.Instance);
            //if (field == null)
            //{
            //    throw new NotSupportedException(Properties.Resources.InternalErrorTraceBackInfo);
            //}
            //return field.GetValue(pythonExc).ToString();
        }

        #region Marshalling

        /// <summary>
        /// Add additional data marshalers to handle host data.
        /// </summary>
        [SupressImportIntoVM]
        internal override void RegisterHostDataMarshalers()
        {
            DataMarshaler dataMarshalerToUse = HostDataMarshaler as DataMarshaler;
            dataMarshalerToUse?.RegisterMarshaler((PyObject pyObj) =>
            {
                try
                {
                    using (Py.GIL())
                    {
                        if (PyDict.IsDictType(pyObj))
                        {
                            using (var pyDict = new PyDict(pyObj))
                            {
                                var dict = new PyDict();
                                foreach (PyObject item in pyDict.Items())
                                {
                                    dict.SetItem(
                                        ConverterExtension.ToPython(dataMarshalerToUse.Marshal(item.GetItem(0))),
                                        ConverterExtension.ToPython(dataMarshalerToUse.Marshal(item.GetItem(1)))
                                    );
                                }
                                return dict;
                            }
                        }
                        var unmarshalled = pyObj.AsManagedObject(typeof(object));

                        // Avoid calling this marshaler infinitely.
                        if (unmarshalled is PyObject)
                        {
                            return unmarshalled;
                        }

                        return dataMarshalerToUse.Marshal(unmarshalled);
                    }
                }
                catch (Exception e)
                {
                    DynamoLogger?.Log($"error marshaling python object {pyObj.Handle} {e.Message}");
                    return pyObj;
                }
            });
        }

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
                       delegate (DynamoPythonNet3Handle handle)
                       {
                           return globalScope.Get(handle.PythonObjectID.ToString());
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
                                return GetDynamoPythonNet3Handle(pyObj);
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
                            if (pyObj.IsIterable() && !PyString.IsStringType(pyObj))
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
                            var unmarshalled = pyObj.AsManagedObject(typeof(object));
                            if (unmarshalled is PyInt)
                            {
                                using (var pyLong = PyInt.AsInt(pyObj))
                                {
                                    try
                                    {
                                        return pyLong.ToInt64();
                                    }
                                    catch (PythonException exc) when (exc.Message.StartsWith("int too big"))
                                    {
                                        return pyLong.ToBigInteger();
                                    }
                                }
                            }
                            // Default handling for other Python objects
                            if (unmarshalled is PyObject)
                            {
                                using (unmarshalled as PyObject)
                                {
                                    if (unmarshalled.Equals(pyObj))
                                    {
                                        return GetDynamoPythonNet3Handle(pyObj);
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

        /// <summary>
        ///     Data Marshaler for all data coming out of a Python node.
        /// </summary>
        [SupressImportIntoVM]
        public static DataMarshaler OutputMarshaler => Instance.OutputDataMarshaler as DataMarshaler;

        private static DynamoPythonNet3Handle GetDynamoPythonNet3Handle(PyObject pyObj)
        {
            globalScope.Set(pyObj.Handle.ToString(), pyObj);
            return new DynamoPythonNet3Handle(pyObj.Handle);
        }

        private static bool IsMarkedToSkipConversion(PyObject pyObj)
        {
            return pyObj.HasAttr(DynamoSkipAttributeName);
        }

        private DataMarshaler inputMarshaler;
        private DataMarshaler outputMarshaler;

        #endregion

        #region Evaluation events

        /// <summary>
        ///     Emitted immediately before execution begins
        /// </summary>
        [SupressImportIntoVM]
        public override event EvaluationStartedEventHandler EvaluationStarted;

        /// <summary>
        ///     Emitted immediately after execution ends or fails
        /// </summary>
        [SupressImportIntoVM]
        public override event EvaluationFinishedEventHandler EvaluationFinished;

        /// <summary>
        /// Called immediately before evaluation starts
        /// </summary>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to be evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private void OnEvaluationBegin(PyModule scope,
                                              string code,
                                              IList bindingValues)
        {
            if (EvaluationStarted != null)
            {
                EvaluationStarted(code, bindingValues, (n, v) => { scope.Set(n, InputMarshaler.Marshal(v).ToPython()); });
                Analytics.TrackEvent(
                    Dynamo.Logging.Actions.Start,
                    Dynamo.Logging.Categories.PythonOperations,
                    "PythonNet3Evaluation");
            }
        }

        /// <summary>
        /// Called when the evaluation has completed successfully or failed
        /// </summary>
        /// <param name="isSuccessful">Whether the evaluation succeeded or not</param>
        /// <param name="scope">The scope in which the code is executed</param>
        /// <param name="code">The code to that was evaluated</param>
        /// <param name="bindingValues">The binding values - these are already added to the scope when called</param>
        private void OnEvaluationEnd(bool isSuccessful,
                                            PyModule scope,
                                            string code,
                                            IList bindingValues)
        {
            if (EvaluationFinished != null)
            {
                EvaluationFinished(isSuccessful ? Dynamo.PythonServices.EvaluationState.Success : Dynamo.PythonServices.EvaluationState.Failed,
                    code, bindingValues, (n) => {
                        return OutputMarshaler.Marshal(scope.Get(n));
                    });
                Analytics.TrackEvent(
                    Dynamo.Logging.Actions.End,
                    Dynamo.Logging.Categories.PythonOperations,
                    "PythonNet3Evaluation");
            }
        }

        #endregion

    }
}
