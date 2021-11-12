using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Dynamo.PythonServices.EventHandlers;
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

namespace Dynamo.PythonServices.EventHandlers
{
    [SupressImportIntoVM]
    public delegate void ScopeSetAction(string name, object value);
    [SupressImportIntoVM]
    public delegate object ScopeGetAction(string name);

    [SupressImportIntoVM]
    public delegate void EvaluationStartedEventHandler(string code,
                                                       IList bindingValues,
                                                       ScopeSetAction scopeSet);
    [SupressImportIntoVM]
    public delegate void EvaluationFinishedEventHandler(EvaluationState state,
                                                        string code,
                                                        IList bindingValues,
                                                        ScopeGetAction scopeGet);
}

namespace Dynamo.PythonServices
{
    [SupressImportIntoVM]
    public enum EvaluationState { Success, Failed }

    /// <summary>
    /// This abstract class is intended to act as a base class for different python engines
    /// </summary>
    public abstract class PythonEngine
    {
        /// <summary>
        /// Data Marshaler for all data coming into a Python node.
        /// </summary>
        public abstract object InputDataMarshaler
        {
            get;
        }

        /// <summary>
        /// Data Marshaler for all data coming out of a Python node.
        /// </summary>
        public abstract object OutputDataMarshaler
        {
            get;
        }

        /// <summary>
        /// Name of the Python engine
        /// </summary>
        public abstract string Name
        {
            get;
        }

        internal PythonEngineVersion Version => Enum.TryParse(Name, out PythonEngineVersion version) ? version : PythonEngineVersion.Unspecified;

        /// <summary>
        /// Add an event handler before the Python evaluation begins
        /// </summary>
        /// <param name="callback"></param>
        public abstract event EvaluationStartedEventHandler EvaluationStarted;

        /// <summary>
        /// Add an event handler after the Python evaluation has finished
        /// </summary>
        /// <param name="callback"></param>
        public abstract event EvaluationFinishedEventHandler EvaluationFinished;

        /// <summary>
        ///     Executes a Python script with custom variable names. Script may be a string
        ///     read from a file, for example. Pass a list of names (matching the variable
        ///     names in the script) to bindingNames and pass a corresponding list of values
        ///     to bindingValues.
        /// </summary>
        /// <param name="code">Python script as a string.</param>
        /// <param name="bindingNames">Names of values referenced in Python script.</param>
        /// <param name="bindingValues">Values referenced in Python script.</param>
        public abstract object Evaluate(string code,
                        IList bindingNames,
                        [ArbitraryDimensionArrayImport] IList bindingValues);
    }

    /// <summary>
    /// Singleton class that other class can access and use for query loaded Python Engine info.
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

        #region Public members
        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        public static PythonEngineManager Instance { get { return lazy.Value; } }
        #endregion

        internal ObservableCollection<PythonEngine> AvailableEngines;

        public static object Evaluate(string engine, string code,
                                        IList bindingNames,
                                        [ArbitraryDimensionArrayImport] IList bindingValues) 
        {
            return Instance.GetEngine(engine).Evaluate(code, bindingNames, bindingValues);
        }

        #region Constant strings
        // TODO: The following fields might be removed after dynamic loading applied
        internal static string PythonEvaluatorSingletonInstance = "Instance";

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
            AvailableEngines = new ObservableCollection<PythonEngine>();

            ScanPythonEngines();
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadEventHandler);
        }

        internal PythonEngine GetEngine(string version)
        {
            return AvailableEngines.FirstOrDefault(x => x.Name == version);
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
            var IP2Engine = GetEngine(PythonEngineVersion.IronPython2.ToString());
            // Provide evaluator info when the selected engine is loaded
            if (IP2Engine != null)
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

            PythonEngine engine = null;
            // Currently we are using try-catch to validate loaded assembly and evaluation method exist
            // but we can optimize by checking all loaded types against evaluators interface later
            try
            {
                var eType = assembly.GetTypes().FirstOrDefault(x => typeof(PythonEngine).IsAssignableFrom(x));
                engine = (PythonEngine)eType?.GetProperty(PythonEvaluatorSingletonInstance, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
            }
            catch
            {
                //Do nothing for now
            }
            if (engine != null && !AvailableEngines.Any(x => x.Name == engine.Name))
            {
                AvailableEngines.Add(engine);
            }
        }
    }
}
