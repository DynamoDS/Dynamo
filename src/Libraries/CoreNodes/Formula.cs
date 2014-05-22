using System.Linq;
using NCalc;

namespace DSCore
{
    /// <summary>
    ///     Backend implementation for Formula node.
    /// </summary>
    //[IsVisibleInDynamoLibrary(false)]
    public static class Formula
    {
        /// <summary>
        ///     Evaluates an NCalc formula with given parameter mappings.
        /// </summary>
        /// <param name="formulaString">NCalc formula</param>
        /// <param name="parameters">Variable names</param>
        /// <param name="args">Variable bindings</param>
        /// <returns name="result">Result of the formula calculation.</returns>
        public static object Evaluate(string formulaString, string[] parameters, object[] args)
        {
            var e = new Expression(formulaString.ToLower(), EvaluateOptions.IgnoreCase);

            e.Parameters["pi"] = 3.14159265358979;
            
            foreach (var arg in args.Select((arg, i) => new { Value = arg, Index = i }))
            {
                var parameter = parameters[arg.Index];
                var value = DynamoUnits.SIUnit.ToSIUnit(arg.Value);
                e.Parameters[parameter] = value == null ? arg.Value : value.Value * DynamoUnits.BaseUnit.UiLengthConversion;
            }

            return e.Evaluate();
        }
    }
}
