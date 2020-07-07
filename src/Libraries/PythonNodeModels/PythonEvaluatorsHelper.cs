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
    internal sealed class PythonEvaluatorsHelper
    {
        /// <summary>
        /// Use Lazy<![CDATA[PythonEvaluatorsHelper]]> to make sure the Singleton class is only initialized once
        /// </summary>
        private static readonly Lazy<PythonEvaluatorsHelper>
            lazy =
            new Lazy<PythonEvaluatorsHelper>
            (() => new PythonEvaluatorsHelper());

        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        internal static PythonEvaluatorsHelper Instance { get { return lazy.Value; } }

        // TODO: Improve the following field with dynamic loading and interface
        internal static bool IsIronPythonEnabled = false;
        internal static string IronPythonEvaluatorClass = "IronPythonEvaluator";
        internal static string IronPythonEvaluationMethod = "EvaluateIronPythonScript";

        internal static bool IsCPythonEnabled = false;
        internal static string CPythonEvaluatorClass = "CPythonEvaluator";
        internal static string CPythonEvaluationMethod = "EvaluatePythonScript";

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to evaluate a Python node
        /// </summary>
        private PythonEvaluatorsHelper()
        {
            var assems = AppDomain.CurrentDomain.GetAssemblies();
            var IronPythonAssem = assems.First(x => x.FullName.Contains("DSIronPython"));
            var CPythonAssem = assems.First(x => x.FullName.Contains("DSCPython"));
            // Currently we are validating evaluation method exists but to force dynamic loading we may want to force interface on evaluators
            if (IronPythonAssem != null &&
                IronPythonAssem.GetType("DSIronPython.IronPythonEvaluator").GetMethod(IronPythonEvaluationMethod) != null)
            {
                IsIronPythonEnabled = true;
            }
            if (CPythonAssem != null &&
                CPythonAssem.GetType("DSCPython.CPythonEvaluator").GetMethod(CPythonEvaluationMethod) != null)
            {
                IsCPythonEnabled = true;
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
            if (engine == PythonEngineVersion.IronPython2 && PythonEvaluatorsHelper.IsIronPythonEnabled)
            {
                evaluatorClass = PythonEvaluatorsHelper.IronPythonEvaluatorClass;
                evaluationMethod = PythonEvaluatorsHelper.IronPythonEvaluationMethod;
                return;
            }
            if (engine == PythonEngineVersion.CPython3 && PythonEvaluatorsHelper.IsCPythonEnabled)
            {
                evaluatorClass = PythonEvaluatorsHelper.CPythonEvaluatorClass;
                evaluationMethod = PythonEvaluatorsHelper.CPythonEvaluationMethod;
                return;
            }

            // Throw exception here will be reflected as Python node error
            throw new InvalidOperationException("Unknown Python engine " + engine);
        }
    }
}
