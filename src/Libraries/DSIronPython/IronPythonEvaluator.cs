using System;
using System.Collections;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using IronPython.Hosting;

namespace DSIronPython
{
    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class IronPythonEvaluator
    {
        /// <summary>
        ///     Executes a Python script with custom variable names. Script may be a string
        ///     read from a file, for example. Pass a list of names (matching the variable
        ///     names in the script) to bindingNames and pass a corresponding list of values
        ///     to bindingValues.
        /// </summary>
        /// <param name="code">Python script as a string.</param>
        /// <param name="bindingNames">Names of values referenced in Python script.</param>
        /// <param name="bindingValues">Values referenced in Python script.</param>
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

            var result = scope.ContainsVariable("OUT") ? scope.GetVariable("OUT") : null;

            return PythonDataMarshaler.Instance.Marshal(result);
        }
    }
}
