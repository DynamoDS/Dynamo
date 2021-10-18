using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        internal Action OnPythonEngineScan;

        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        internal static PythonEngineSelector Instance { get { return lazy.Value; } }

        // TODO: The following fields might be removed after dynamic loading applied
        internal bool IsIronPythonEnabled = false;
        internal string IronPythonEvaluatorClass = "IronPythonEvaluator";
        internal string IronPythonEvaluationMethod = "EvaluateIronPythonScript";

        internal bool IsCPythonEnabled = false;
        internal string CPythonEvaluatorClass = "CPythonEvaluator";
        internal string CPythonEvaluationMethod = "EvaluatePythonScript";

        const string DummyEvaluatorClass = "DummyPythonEvaluator";
        const string DummyEvaluatorMethod = "Evaluate";

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to evaluate a Python node
        /// </summary>
        private PythonEngineSelector()
        {
            ScanPythonEngines();
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadEventHandler);
        }

        static private void AssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
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
                if (assembly.GetName().Name.Equals("DSIronPython") &&
                    assembly.GetType("DSIronPython.IronPythonEvaluator").GetMethod(IronPythonEvaluationMethod) != null)
                {
                    IsIronPythonEnabled = true;
                    OnPythonEngineScan?.Invoke();
                }
            }
            catch
            {
                //Do nothing for now
            }
            try
            {
                if (assembly.GetName().Name.Equals("DSCPython") &&
                    assembly.GetType("DSCPython.CPythonEvaluator").GetMethod(CPythonEvaluationMethod) != null)
                {
                    IsCPythonEnabled = true;
                    OnPythonEngineScan?.Invoke();
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
            if (engine == PythonEngineVersion.IronPython2 && Instance.IsIronPythonEnabled)
            {
                evaluatorClass = Instance.IronPythonEvaluatorClass;
                evaluationMethod = Instance.IronPythonEvaluationMethod;
                return;
            }
            if (engine == PythonEngineVersion.CPython3 && Instance.IsCPythonEnabled)
            {
                evaluatorClass = Instance.CPythonEvaluatorClass;
                evaluationMethod = Instance.CPythonEvaluationMethod;
                return;
            }

            // Throwing at the compilation stage is handled as a non-retryable error by the Dynamo engine.
            // Instead, we want to produce an error at the evaluation stage, so we can eventually recover.
            // We handle this by providing a dummy Python evaluator that will evaluate to an error message.
            evaluatorClass = DummyEvaluatorClass;
            evaluationMethod = DummyEvaluatorMethod;
        }

        internal IEnumerable<PythonEngineVersion> GetEnabledEngines()
        {
            var output = new List<PythonEngineVersion>();
            if (IsIronPythonEnabled)
            {
                output.Add(PythonEngineVersion.IronPython2);
            }
            if (IsCPythonEnabled)
            {
                output.Add(PythonEngineVersion.CPython3);
            }
            return output;
        }
    }
}
