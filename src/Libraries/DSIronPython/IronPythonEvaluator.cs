using System;
using System.Collections;
using Autodesk.DesignScript.Runtime;
using IronPython.Hosting;

namespace DSIronPython
{
    /// <summary>
    ///     Evaluates a Python script in the Dynamo context.
    /// </summary>
    public class IronPythonEvaluator
    {
        private IronPythonEvaluator() { }

        public static string TestCode { get { return "OUT = 0"; } }

        /// <summary>
        ///     Executes a Python script with custom variable names. Script may be a string
        ///     read from a file, for example. Pass a list of names (matching the variable
        ///     names in the script) to bindingNames and pass a corresponding list of values
        ///     to bindingValues.
        /// </summary>
        /// <param name="code">Python script as a string.</param>
        /// <param name="bindingNames">Names of values referenced in Python script.</param>
        /// <param name="bindingValues">Values referenced in Python script.</param>
        /// <returns name="OUT">Output of the Python script.</returns>
        /// <search>python</search>
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
