using ProtoCore.DSASM;
using ProtoCore.Lang;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSCore
{
    //TODO consider moving this entire file to DesignScriptBuiltins - this was not done now
    //because DesignScriptBuiltins cannot reference protocore - where some range enums are defined
    //and this refactor did not seem high priority at the moment. 
    // cyclic dep. :)
    //!! Everything in here should stay internal so we can move it.
    internal class RangeHelpers
    {
        //TODO op is specified as a long - even though it is an int.
        //I guess MSIL engine does not support coercion from long to int,
        //and all DS ints are longs.
        /// <summary>
        /// Generate a range.
        /// </summary>
        /// <param name="start">start value</param>
        /// <param name="end">end value</param>
        /// <param name="step">step value</param>
        /// <param name="op">step operation enum see <see cref="RangeStepOperator"/></param>
        /// <param name="hasStep">has step size been specified</param>
        /// <param name="hasAmountOp">has amount been specified</param>
        /// <returns></returns>
        internal static IList GenerateRangeIL(double start, double end, double step, long op, bool hasStep, bool hasAmountOp)
        {
            return RangeExpressionUtils.RangeExpressionCore(start, end, step, (int)op, hasStep, hasAmountOp);
        }
        //TODO handle alphabetic ranges - overloads or dynamically.
    }
}
