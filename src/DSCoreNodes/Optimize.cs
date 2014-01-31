using System;

//TODO: Documentation and tests

namespace DSCore
{
    /// <summary>
    /// 
    /// </summary>
    public class Optimize
    {
        public static double NewtonRootFind1DNoDeriv(
            Delegate objFunc, double start, int maxIters)
        {
            double count = 0;
            double change = 1e10;
            const double maxChange = 1e-5;
            const double h = 1e-5;

            // use newton's method 
            while (count < maxIters && change > maxChange)
            {
                var fx = (double)objFunc.DynamicInvoke(start);
                var fxh = (double)objFunc.DynamicInvoke(start + h);
                var dfx = (fxh - fx)/h;

                var x1 = start - fx/dfx;
                change = start - x1;
                start = x1;

                count++;
            }

            return start;
        }

        public static double NewtonRootFind1DWithDeriv(
            Delegate objFunc, Delegate derivFunc, double start, int maxIters)
        {
            double count = 0;
            double change = 1e10;
            const double maxChange = 1e-5;

            // use newton's method 
            while (count < maxIters && change > maxChange)
            {
                var fx = (double)objFunc.DynamicInvoke(start);
                var dfx = (double)derivFunc.DynamicInvoke(start);

                var x1 = start - fx / dfx;
                change = start - x1;
                start = x1;

                count++;
            }

            return start;
        }
    }
}
