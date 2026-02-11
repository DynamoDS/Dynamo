using System;
using System.Collections;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.PythonServices;

namespace DSCore
{
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
