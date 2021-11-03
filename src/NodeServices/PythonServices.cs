using Dynamo.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace Dynamo.PythonServices
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

    /// <summary>
    /// Singleton class that other class can access and use for query loaded Python Engine info.
    /// TODO: Make the Singleton class public when Dynamic loading is also ready.
    /// </summary>
    public sealed class PythonEngineManager
    {
        /// <summary>
        /// Use Lazy&lt;PythonEngineManager&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        internal static readonly Lazy<PythonEngineManager>
            lazy =
            new Lazy<PythonEngineManager>
            (() => new PythonEngineManager());

        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        public static PythonEngineManager Instance { get { return lazy.Value; } }

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

        internal ObservableCollection<PythonEngineProxy> AvailableEngines;

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to evaluate a Python node
        /// </summary>
        private PythonEngineManager()
        {
            AvailableEngines = new ObservableCollection<PythonEngineProxy>();

            ScanPythonEngines();
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadEventHandler);
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
            // Currently we are using try-catch to validate loaded assembly and evaluation method exist
            // but we can optimize by checking all loaded types against evaluators interface later
            try
            {
                if (!AvailableEngines.Any(x => x.Version == PythonEngineVersion.IronPython2) &&
                    assembly.GetName().Name.Equals(IronPythonAssemblyName) &&
                    assembly.GetType(IronPythonTypeName).GetMethod(IronPythonEvaluationMethod) != null)
                {
                    AvailableEngines.Add(new PythonEngineProxy(assembly, PythonEngineVersion.IronPython2));
                }
            }
            catch
            {
                //Do nothing for now
            }
            try
            {
                if (!AvailableEngines.Any(x => x.Version == PythonEngineVersion.CPython3) &&
                    assembly.GetName().Name.Equals(CPythonAssemblyName) &&
                    assembly.GetType(CPythonTypeName).GetMethod(CPythonEvaluationMethod) != null)
                {
                    AvailableEngines.Add(new PythonEngineProxy(assembly, PythonEngineVersion.CPython3));
                }
            }
            catch
            {
                //Do nothing for now
            }
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
    }

    public class PythonEngineProxy : IDisposable
    {
        public readonly PythonEngineVersion Version;
        private readonly Assembly Assembly;
        private readonly Type EngineType;

        private Action<string, IList, Action<string, object>> OnBeginEvaluation;
        private Action<string, IList> OnEndEvaluation;

        internal PythonEngineProxy(Assembly a, PythonEngineVersion vers)
        {
            Version = vers;
            Assembly = a;
            EngineType = Version == PythonEngineVersion.IronPython2 ?
                Assembly.GetType(PythonEngineManager.IronPythonTypeName) :
                Assembly.GetType(PythonEngineManager.CPythonTypeName);
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

        public void OnEvaluationBegin(Action<string, IList, Action<string, object>> callback)
        {

            OnBeginEvaluation = callback;
            AddEventHandler("EvaluationStarted", nameof(HandleEvaluationBegin));
        }

        public void OnEvaluationEnd(Action<string, IList> callback)
        {
            OnEndEvaluation = callback;
            AddEventHandler("EvaluationFinished", nameof(HandleEvaluationEnd));
        }

        public DataMarshaler GetInputMarshaler()
        {
            try
            {
                return EngineType.GetProperty(PythonEngineManager.PythonInputMarshalerProperty).GetValue(null) as DataMarshaler;
            }
            catch
            {
                return null;
            }
        }

        public DataMarshaler GetOutputMarshaler()
        {
            try
            {
                return EngineType.GetProperty(PythonEngineManager.PythonOutputMarshalerProperty).GetValue(null) as DataMarshaler;
            }
            catch
            {
                return null;
            }
        }
    }
}
