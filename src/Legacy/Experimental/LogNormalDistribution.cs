using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Formulae from: http://en.wikipedia.org/wiki/Log-normal_distribution
public class LogNormalDistribution
{
    public double Expectation { get; private set; }
    public double Variance { get; private set; }

    private double multFact = 1;

    /// <summary>
    /// return the unit normal distribution
    /// </summary>
    public LogNormalDistribution()
    {
        Expectation = 0;
        Variance = 1;

        ComputeInternalParameters();
    }

    public static LogNormalDistribution ByParams(double expectation, double variance)
    {
        if (Double.IsInfinity(expectation) || Double.IsNaN(expectation))
            throw new ArgumentException("The expectation must be a finite number");

        if (variance <= 0 || Double.IsInfinity(expectation) || Double.IsNaN(expectation))
            throw new ArgumentException("The variance must be a positive finite number");

        LogNormalDistribution distribution = new LogNormalDistribution();
        distribution.Expectation = expectation;
        distribution.Variance = variance;

        distribution.ComputeInternalParameters();

        return distribution;

    }

    private void ComputeInternalParameters()
    {
        //1 / SQRT(2*PI*variance)

        
        double denom = (Math.Sqrt(2D * Math.PI * Variance));
        multFact = 1D / denom;

    }

    public double ProbabilityDensity(double x)
    {
        if (x <= 0)
            throw new ArgumentException("The Log-Normal distribution is only defined for x > 0");


        double working = Math.Log(x) - Expectation;
        working = working*working;

        double fact = working / (-2D * Variance);

        return multFact * (1 / x) * Math.Exp(fact);
    }



}
