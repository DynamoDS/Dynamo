using System;
using System.Collections;
using Autodesk.DesignScript.Runtime;
using IronPython.Hosting;

namespace DSIronPython
{
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class IronPythonEvaluator
    {
        private IronPythonEvaluator() { }

        public static string TestCode { get { return "OUT = 0"; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="bindingNames"></param>
        /// <param name="bindingValues"></param>
        /// <returns></returns>
        public static object EvaluateIronPythonScript(
            string code, 
            IList bindingNames, 
            [ArbitraryDimensionArrayImport] IList bindingValues)
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            var amt = Math.Min(bindingNames.Count, bindingValues.Count);

            for (int i = 0; i < amt; i++)
            {
                scope.SetVariable((string)bindingNames[i], bindingValues[i]);
            }

            engine.CreateScriptSourceFromString(code).Execute(scope);

            return scope.ContainsVariable("OUT") ? scope.GetVariable("OUT") : null;
        }
    }
}
