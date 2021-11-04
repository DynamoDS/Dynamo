using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
    /// Proxy class for any Python Engine that can be loaded by Dynamo
    /// Provides basic APIs to interact with the Python Engine
    /// </summary>
    [SupressImportIntoVM]
    public class PythonEngineProxy : IDisposable
    {
        /// <summary>
        /// The version of the Python Engine connected to this proxy instance
        /// </summary>
        public readonly PythonEngineVersion Version;
        private readonly Type EngineType;

        private Action<string, IList, Action<string, object>> OnBeginEvaluation;
        private Action<string, IList> OnEndEvaluation;

        internal PythonEngineProxy(Type eType, PythonEngineVersion version)
        {
            EngineType = eType;
            Version = version;
        }

        public void Dispose()
        {
            try
            {
                var events = typeof(PythonEngineProxy).GetEvents();
                foreach(var eventInfo in events)
                {
                    var eventHandlerName = String.Empty;
                    if (eventInfo.Name.Equals("EvaluationBegin"))
                    {
                        eventHandlerName = nameof(HandleEvaluationBegin);
                    }
                    else if (eventInfo.Name.Equals("EvaluationEnd"))
                    {
                        eventHandlerName = nameof(HandleEvaluationEnd);
                    }
                    else
                    {
                        continue;
                    }

                    MethodInfo handlerInfo = typeof(PythonEngineProxy).GetMethod(eventHandlerName, BindingFlags.NonPublic | BindingFlags.Instance);
                    var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerInfo);
                    eventInfo.RemoveEventHandler(this, handler);
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Add an event handler before the Python evaluation begins
        /// </summary>
        /// <param name="callback"></param>
        public void OnEvaluationBegin(Action<string, IList, Action<string, object>> callback)
        {

            OnBeginEvaluation = callback;
            AddEventHandler("EvaluationStarted", nameof(HandleEvaluationBegin));
        }

        /// <summary>
        /// Add an event handler after the Python evaluation ends
        /// </summary>
        /// <param name="callback"></param>
        public void OnEvaluationEnd(Action<string, IList> callback)
        {
            OnEndEvaluation = callback;
            AddEventHandler("EvaluationFinished", nameof(HandleEvaluationEnd));
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

        private void HandleEvaluationBegin(string code, IList bindingValues, Action<string, object> scopeFn)
        {
            OnBeginEvaluation?.Invoke(code, bindingValues, scopeFn);
        }

        private void HandleEvaluationEnd(string code, IList bindingValues)
        {
            OnEndEvaluation?.Invoke(code, bindingValues);
        }


        private void AddEventHandler(string targetEventName, string handlerName)
        {
            try
            {
                MethodInfo handlerInfo = typeof(PythonEngineProxy).GetMethod
                (handlerName, BindingFlags.NonPublic | BindingFlags.Instance);

                var eventInfo = EngineType.GetEvent(targetEventName);
                var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType,
                                     this,
                                     handlerInfo);

                eventInfo.AddEventHandler(this, handler);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
