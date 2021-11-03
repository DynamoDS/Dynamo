using Dynamo.PythonServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace PythonNodeModels
{
    /// <summary>
    /// Singleton class that other class can access and use for query loaded Python Engine info.
    /// TODO: Make the Singleton class public when Dynamic loading is also ready.
    /// </summary>
    internal sealed class PythonEngineSelector
    {
        /// <summary>
        /// Use Lazy&lt;PythonEngineSelector&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        internal static readonly Lazy<PythonEngineSelector>
            lazy =
            new Lazy<PythonEngineSelector>
            (() => new PythonEngineSelector());

        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        internal static PythonEngineSelector Instance { get { return lazy.Value; } }

        internal ObservableCollection<PythonEngineProxy> AvailableEngines;

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to evaluate a Python node
        /// </summary>
        private PythonEngineSelector()
        {
            AvailableEngines = new ObservableCollection<PythonEngineProxy>();

            ScanPythonEngines();
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadEventHandler);
        }

        private void AssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
        {
            PythonEngineSelector.Instance.ScanPythonEngine(args.LoadedAssembly);
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
                    assembly.GetName().Name.Equals(PythonEngineManager.IronPythonAssemblyName) &&
                    assembly.GetType(PythonEngineManager.IronPythonTypeName).GetMethod(PythonEngineManager.IronPythonEvaluationMethod) != null)
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
                    assembly.GetName().Name.Equals(PythonEngineManager.CPythonAssemblyName) &&
                    assembly.GetType(PythonEngineManager.CPythonTypeName).GetMethod(PythonEngineManager.CPythonEvaluationMethod) != null)
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
                evaluatorClass = PythonEngineManager.IronPythonEvaluatorClass;
                evaluationMethod = PythonEngineManager.IronPythonEvaluationMethod;
                return;
            }
            if (engine == PythonEngineVersion.CPython3 &&
                AvailableEngines.Any(x => x.Version == PythonEngineVersion.CPython3))
            {
                evaluatorClass = PythonEngineManager.CPythonEvaluatorClass;
                evaluationMethod = PythonEngineManager.CPythonEvaluationMethod;
                return;
            }

            // Throwing at the compilation stage is handled as a non-retryable error by the Dynamo engine.
            // Instead, we want to produce an error at the evaluation stage, so we can eventually recover.
            // We handle this by providing a dummy Python evaluator that will evaluate to an error message.
            evaluatorClass = PythonEngineManager.DummyEvaluatorClass;
            evaluationMethod = PythonEngineManager.DummyEvaluatorMethod;
        }
    }
}
