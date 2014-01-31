using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//Formulae from: http://en.wikipedia.org/wiki/Normal_distribution
    public class NormalDistribution
    {
        public double Expectation { get; private set; }
        public double Variance { get; private set; }

        private double multFact = 1;
        private double sigma;

        /// <summary>
        /// return the unit normal distribution
        /// </summary>
        public NormalDistribution()
        {
            Expectation = 0;
            Variance = 1;

            ComputeInternalParameters();
        }

        public static NormalDistribution ByParams(double expectation, double variance)
        {
            if (Double.IsInfinity(expectation) || Double.IsNaN(expectation))
                throw new ArgumentException("The expectation must be a finite number");

            if (variance <= 0 || Double.IsInfinity(expectation) || Double.IsNaN(expectation))
                throw new ArgumentException("The variance must be a positive finite number");

            NormalDistribution distribution = new NormalDistribution();
            distribution.Expectation = expectation;
            distribution.Variance = variance;

            distribution.ComputeInternalParameters();

            return distribution;

        }
        
        private void ComputeInternalParameters()
        {
            //1 / sigma * SQRT(2PI)

            sigma = Math.Sqrt(Variance);
            double denom = sigma*(Math.Sqrt(2D*Math.PI));
            multFact = 1D/denom;

        }

        public double ProbabilityDensity(double x)
        {
            double working = x - Expectation;
            working = working/sigma;

            double fact = (working*working)*-0.5D;

            return multFact*Math.Exp(fact);
        }



    }
