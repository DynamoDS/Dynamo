using System;
using System.Linq;

namespace PythonNodeModels
{
    /// <summary>
    /// Enum of possible values of python engine versions.
    /// TODO: Remove when dynamic loading logic is there then we no longer need a hard copy of the options
    /// </summary>
    public enum PythonEngineVersion
    {
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

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to evaluate a Python node
        /// </summary>
        private PythonEngineSelector()
        {
            ScanPythonEngines();
        }

        /// <summary>
        /// Scan loaded Python engines
        /// </summary>
        internal void ScanPythonEngines()
        {
            var assems = AppDomain.CurrentDomain.GetAssemblies();
            // Currently we are using try-catch to validate loaded assembly and evaluation method exist
            // but we can optimize by checking all loaded types against evaluators interface later
            try
            {
                var IronPythonAssem = assems.FirstOrDefault(x => x.GetName().Name.Equals("DSIronPython"));
                if (IronPythonAssem != null &&
                    IronPythonAssem.GetType("DSIronPython.IronPythonEvaluator").GetMethod(IronPythonEvaluationMethod) != null)
                {
                    IsIronPythonEnabled = true;
                }
            }
            catch
            {
                //Do nothing for now
            }
            try
            {
                var CPythonAssem = assems.FirstOrDefault(x => x.GetName().Name.Equals("DSCPython"));
                if (CPythonAssem != null &&
                    CPythonAssem.GetType("DSCPython.CPythonEvaluator").GetMethod(CPythonEvaluationMethod) != null)
                {
                    IsCPythonEnabled = true;
                }
            }
            catch
            {
                //Do nothing for now
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

            // Throw exception here will be reflected as Python node error
            throw new InvalidOperationException(Properties.Resources.PythonNodeErrorUnloadedEngineMsg + engine);
        }
    }
}
