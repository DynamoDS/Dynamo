using System;
using Autodesk.DesignScript.Runtime;
using Builtin.Properties;
using ProtoCore.DSASM;

namespace ProtoCore.DSASM
{
    //Type moved from Protocore and forwarded using type forward attribute.
    /// <summary>
    /// Type of range to create.
    /// </summary>
    public enum RangeStepOperator
    {
        /// <summary>
        /// Creating entries by stepping using the stepsize.
        /// </summary>
        StepSize,
        /// <summary>
        /// Create a requested number of entries.
        /// </summary>
        Number,
        /// <summary>
        /// Approximate step size prioritizing reaching end of the range. 
        /// </summary>
        ApproximateSize
    }
}


namespace Builtin
{
    //TODO for now suppress import into DS VM.
    [SupressImportIntoVM]
    //TODO consider moving this entire file to DesignScriptBuiltins - this was not done now
    //because DesignScriptBuiltins cannot reference protocore - where some range enums are defined
    //and this refactor did not seem high priority at the moment. 
    // cyclic dep. :)
    //!! Everything in here should stay internal so we can move it.
    public class RangeHelpers
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
        public static long[] GenerateRangeILInt(long start, long end, double step, long op, bool hasStep, bool hasAmountOp)
        {
            return RangeExpressionInt(start, end, step, (int)op, hasStep, hasAmountOp);
        }
        public static double[] GenerateRangeILDouble(double start, double end, double step, long op, bool hasStep, bool hasAmountOp)
        {
            return RangeExpressionDouble(start, end, step, (int)op, hasStep, hasAmountOp);
        }
        //TODO handle alphabetic ranges - overloads or dynamically.



        internal static long[] RangeExpressionInt(long start, long end, double step, int op, bool hasStep, bool hasAmountOp)
        {
            if (hasAmountOp && !hasStep)
            {
                throw new ArgumentException(Resources.kNoStepSizeInAmountRangeExpression);
            }

            return GenerateNumericRangeCoreInt(start, end, step, op, hasStep, hasAmountOp);
        }
        internal static double[] RangeExpressionDouble(double start, double end, double step, int op, bool hasStep, bool hasAmountOp)
        {
            if (hasAmountOp && !hasStep)
            {
                throw new ArgumentException(Resources.kNoStepSizeInAmountRangeExpression);
            }

            return GenerateNumericRangeCoreDouble(start, end, step, op, hasStep, hasAmountOp);
        }
        /*
        internal static char[] RangeExpressionChar(double start, double end, double step, int op, bool hasStep, bool hasAmountOp)
        {
            if (hasAmountOp && !hasStep)
            {
                throw new ArgumentException(Properties.DesignScriptBuiltin.kNoStepSizeInAmountRangeExpression);
            }

            return GenerateNumericRangeCoreChar(start, end, step, op, hasStep, hasAmountOp);
        }
        */

        private static long[] GenerateNumericRangeCoreInt(long start, long end, double step, int op, bool hasStep, bool hasAmountOp)
        {
            var isIntStep = Math.Truncate(step) == step;
            if (!isIntStep)
            {
                throw new ArgumentException("step val is not representable by an int, should use another overload of GenerateNumericRangeCore");
            }
            var intStep = (long)step;
            if (hasAmountOp)
            {
                long amount = end;
                if (amount < 0)
                {
                    throw new ArgumentException(Resources.kInvalidAmountInRangeExpression);
                }

                if (amount == 0)
                {
                    return new long[0];
                }
                else
                {
                    long[] range = new long[amount];
                    for (int i = 0; i < amount; ++i)
                    {
                        range[i] = start;
                        start += intStep;
                    }
                    return range;
                }
            }
            else
            {

                switch (op)
                {
                    case (int)RangeStepOperator.StepSize:
                        {
                            long stepsize = (start > end) ? -1 : 1;
                            if (hasStep)
                            {
                                stepsize = intStep;
                            }

                            if ((stepsize == 0) || (end > start && stepsize < 0) || (end < start && stepsize > 0))
                            {
                                return null;
                            }

                            decimal stepnum = Math.Truncate(new decimal(end - start) / new decimal(stepsize)) + 1;
                            if (stepnum > int.MaxValue)
                            {
                                throw new ArgumentOutOfRangeException(Resources.RangeExpressionOutOfMemory);
                            }
                            long[] range = new long[(int)stepnum];

                            long cur = start;
                            for (int i = 0; i < (int)stepnum; ++i)
                            {
                                range[i] = cur;
                                cur += stepsize;
                            }
                            return range;
                        }
                    case (int)RangeStepOperator.Number:
                        {
                            var stepNum = intStep;
                            if (stepNum <= 0)
                            {
                                return null;
                            }
                            else if (stepNum > int.MaxValue)
                            {
                                throw new ArgumentOutOfRangeException(Resources.RangeExpressionOutOfMemory);
                            }

                            return GenerateRangeByStepNumberInt(new decimal(start), new decimal(end), stepNum);


                        }
                    case (int)RangeStepOperator.ApproximateSize:
                        {
                            decimal astepsize = new decimal(intStep);
                            if (astepsize == 0)
                            {
                                return null;
                            }

                            decimal dist = end - start;
                            decimal stepNum = 1;
                            if (dist != 0)
                            {
                                decimal cstepnum = Math.Ceiling(dist / astepsize);
                                decimal fstepnum = Math.Floor(dist / astepsize);

                                if (cstepnum == 0 || fstepnum == 0)
                                {
                                    stepNum = 2;
                                }
                                else
                                {
                                    decimal capprox = Math.Abs(dist / cstepnum - astepsize);
                                    decimal fapprox = Math.Abs(dist / fstepnum - astepsize);
                                    stepNum = capprox < fapprox ? cstepnum + 1 : fstepnum + 1;
                                }
                            }

                            if (stepNum > int.MaxValue)
                            {
                                throw new ArgumentOutOfRangeException(Resources.RangeExpressionOutOfMemory);
                            }

                            return GenerateRangeByStepNumberInt(new decimal(start), new decimal(end), (int)stepNum);
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return null;
        }
        private static double[] GenerateNumericRangeCoreDouble(double start, double end, double step, int op, bool hasStep, bool hasAmountOp)
        {

            if (hasAmountOp)
            {
                long amount = (long)end;
                if (amount < 0)
                {
                    throw new ArgumentException(Resources.kInvalidAmountInRangeExpression);
                }

                if (amount == 0)
                {
                    return new double[0];
                }
                else
                {
                    double[] range = new double[amount];
                    for (int i = 0; i < amount; ++i)
                    {
                        range[i] = start;
                        start += step;
                    }
                    return range;
                }
            }
            else
            {

                switch (op)
                {
                    case (int)RangeStepOperator.StepSize:
                        {
                            decimal stepsize = (start > end) ? -1 : 1;
                            if (hasStep)
                            {
                                stepsize = new decimal(step);
                            }

                            if ((stepsize == 0) || (end > start && stepsize < 0) || (end < start && stepsize > 0))
                            {
                                return null;
                            }

                            decimal stepnum = Math.Truncate(new decimal(end - start) / stepsize) + 1;
                            if (stepnum > int.MaxValue)
                            {
                                throw new ArgumentOutOfRangeException(Resources.RangeExpressionOutOfMemory);
                            }
                            double[] range = new double[(int)stepnum];

                            decimal cur = (decimal)start;
                            for (int i = 0; i < (int)stepnum; ++i)
                            {
                                range[i] = (double)cur;
                                cur += stepsize;
                            }
                            return range;
                        }
                    case (int)RangeStepOperator.Number:
                        {
                            decimal stepNum = new decimal(Math.Round(step));
                            if (stepNum <= 0)
                            {
                                return null;
                            }
                            else if (stepNum > int.MaxValue)
                            {
                                throw new ArgumentOutOfRangeException(Resources.RangeExpressionOutOfMemory);
                            }

                            return GenerateRangeByStepNumberDouble(new decimal(start), new decimal(end), (long)stepNum);


                        }
                    case (int)RangeStepOperator.ApproximateSize:
                        {
                            decimal astepsize = new decimal(step);
                            if (astepsize == 0)
                            {
                                return null;
                            }

                            decimal dist = new decimal(end) - new decimal(start);
                            decimal stepNum = 1;
                            if (dist != 0)
                            {
                                decimal cstepnum = Math.Ceiling(dist / astepsize);
                                decimal fstepnum = Math.Floor(dist / astepsize);

                                if (cstepnum == 0 || fstepnum == 0)
                                {
                                    stepNum = 2;
                                }
                                else
                                {
                                    decimal capprox = Math.Abs(dist / cstepnum - astepsize);
                                    decimal fapprox = Math.Abs(dist / fstepnum - astepsize);
                                    stepNum = capprox < fapprox ? cstepnum + 1 : fstepnum + 1;
                                }
                            }

                            if (stepNum > int.MaxValue)
                            {
                                throw new ArgumentOutOfRangeException(Resources.RangeExpressionOutOfMemory);
                            }

                            return GenerateRangeByStepNumberDouble(new decimal(start), new decimal(end), (long)stepNum);
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return null;
        }


        //overloads to avoid returning object[] and boxing/unboxing.
        internal static long[] GenerateRangeByStepNumberInt(decimal start, decimal end, long stepnum)
        {
            decimal stepsize = (stepnum == 1) ? 0 : (end - start) / (stepnum - 1);

            long[] range = new long[stepnum > 1 ? stepnum : 1];
            range[0] = (long)start;

            decimal cur = start;
            for (int i = 1; i < stepnum - 1; ++i)
            {
                cur += stepsize;
                range[i] = (long)cur;
            }

            if (stepnum > 1)
            {
                range[stepnum - 1] = (long)end;
            }

            return range;
        }
        internal static double[] GenerateRangeByStepNumberDouble(decimal start, decimal end, long stepnum)
        {
            decimal stepsize = (stepnum == 1) ? 0 : (end - start) / (stepnum - 1);

            double[] range = new double[stepnum > 1 ? stepnum : 1];
            range[0] = (double)start;

            decimal cur = start;
            for (int i = 1; i < stepnum - 1; ++i)
            {
                cur += stepsize;
                range[i] = (double)cur;
            }

            if (stepnum > 1)
            {
                range[stepnum - 1] = (double)end;
            }

            return range;
        }

    }
}
