using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using NCalc;

namespace DSCore
{
    /// <summary>
    ///     Backend implementation for Formula node.
    /// </summary>
    public static class Formula
    {
        /// <summary>
        ///     Evaluates an NCalc formula with given parameter mappings.
        /// </summary>
        /// <param name="formulaString">NCalc formula</param>
        /// <param name="parameters">Variable names</param>
        /// <param name="args">Variable bindings</param>
        public static object Evaluate(string formulaString, string[] parameters, object[] args)
        {
            //Evaluate("a+b", new[] { "a", "b" }, new object[] { new[] { 0, 1 }, 5 });


            var e = new Expression(formulaString.ToLower(), EvaluateOptions.IgnoreCase);

            e.Parameters["pi"] = 3.14159265358979;
            
            foreach (var arg in args.Select((arg, i) => new { Value = arg, Index = i }))
            {
                var parameter = parameters[arg.Index];
                e.Parameters[parameter] = arg.Value;
            }

            return e.Evaluate();
        }
    }
}
