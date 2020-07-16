using System;
using System.Collections;
using Autodesk.DesignScript.Runtime;

namespace DSCPython
{
    /// <summary>
    /// Python evaluator that will only throw an error on evaluation.
    /// Used primarily to indicate that a real Python evaluator could not be found.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
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
}
