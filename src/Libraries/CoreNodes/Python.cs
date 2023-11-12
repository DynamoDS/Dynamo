using System;
using System.Collections;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.PythonServices;

namespace DSCore
{
    /// <summary>
    /// Python evaluator that will only throw an error on evaluation.
    /// Used primarily to indicate that a real Python evaluator could not be found.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [Obsolete("This class will be removed in Dynamo 3.0")]
    public static class DummyPythonEvaluator
    {
        /// <summary>
        /// Throws an exception with the assigned message
        /// </summary>
        public static object Evaluate(string code, IList bindingNames, [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            throw new InvalidOperationException(Properties.Resources.MissingPythonEngine);
        }
    }

    /// <summary>
    /// Evaluate python code on any Python engine. Should only be used in VM
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class PythonEvaluator
    {
        public static object Evaluate(string engineName,
                                      string code,
                                      IList bindingNames,
                                      [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            var engine = PythonEngineManager.Instance.AvailableEngines.FirstOrDefault(x => x.Name == engineName);
            if (engine == null)
            {
                throw new InvalidOperationException(Properties.Resources.MissingPythonEngine);
            }
            return engine?.Evaluate(code, bindingNames, bindingValues) ?? null;
        }
    }
}