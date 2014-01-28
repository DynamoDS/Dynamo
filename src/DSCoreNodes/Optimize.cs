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
            Func<double, double> objFunc, double start, int maxIters)
        {
            double count = 0;
            double change = 1e10;
            const double maxChange = 1e-5;
            const double h = 1e-5;

            // use newton's method 
            while (count < maxIters && change > maxChange)
            {
                var fx = objFunc(start);
                var fxh = objFunc(start + h);
                var dfx = (fxh - fx)/h;

                var x1 = start - fx/dfx;
                change = start - x1;
                start = x1;

                count++;
            }

            return start;
        }

        public static double NewtonRootFind1DWithDeriv(
            Func<double, double> objFunc, Func<double, double> derivFunc, double start, int maxIters)
        {
            double count = 0;
            double change = 1e10;
            const double maxChange = 1e-5;

            // use newton's method 
            while (count < maxIters && change > maxChange)
            {
                var fx = objFunc(start);
                var dfx = derivFunc(start);

                var x1 = start - fx / dfx;
                change = start - x1;
                start = x1;

                count++;
            }

            return start;
        }
    }
}
