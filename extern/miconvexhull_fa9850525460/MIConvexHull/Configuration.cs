/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2014 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it 
 *  under the terms of  the GNU Lesser General Public License as published by 
 *  the Free Software Foundation; either version 2.1 of the License, or 
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, 
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser 
 *  General Public License for more details.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System;

    /// <summary>
    /// Determines the type of the point translation to use.
    /// 
    /// This is useful for handling "degenerate" data (i.e. uniform grids of points).
    /// </summary>
    public enum PointTranslationType
    {
        /// <summary>
        /// Nothing happens.
        /// </summary>
        None,

        /// <summary>
        /// The points are only translated internally, the vertexes in the result 
        /// retain their original coordinates.
        /// </summary>
        TranslateInternal
    }

    /// <summary>
    /// Configuration of the convex hull computation.
    /// </summary>
    public class ConvexHullComputationConfig
    {
        /// <summary>
        /// This value is used to determine which vertexes are eligible 
        /// to be part of the convex hull.
        /// 
        /// As an example, imagine a line with 3 points:
        /// 
        ///              A ---- C ---- B
        /// 
        /// Points A and B were already determined to be on the hull.
        /// Now, the point C would need to be at least 'PlaneDistanceTolerance'
        /// away from the line determined by A and B to be also considered
        /// a hull point.
        /// 
        /// Default = 0.00001
        /// </summary>
        public double PlaneDistanceTolerance { get; set; } 

        /// <summary>
        /// Determines what method to use for point translation.
        /// This helps with handling "degenerate" data such as uniform grids.
        /// 
        /// Default = None
        /// </summary>
        public PointTranslationType PointTranslationType { get; set; }

        /// <summary>
        /// A function used to generate translation direction.
        /// 
        /// This function is called for each coordinate of each point as
        /// Position[i] -> Position[i] + PointTranslationGenerator()
        /// 
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// From my testing the function should be set up so that the 
        /// translation magnitude is lower than the PlaneDistanceTolerance. 
        /// Otherwise, flat faces in triangulation could be created as a result.
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// 
        /// An example of the translation function that would shift each coordinate 
        /// in 0.0000005 in either direction is:
        /// var rnd = new Random(0); // use the same seed for each computation
        /// f = () => 0.000001 * rnd.NextDouble() - 0.0000005;
        /// 
        /// This is implemented by the 
        /// ConvexHullComputationConfig.RandomShiftByRadius function.
        /// 
        /// Default = null
        /// </summary>
        public Func<double> PointTranslationGenerator { get; set; }
        
        /// <summary>
        /// Create the config with default values set.
        /// </summary>
        public ConvexHullComputationConfig()
        {
            PlaneDistanceTolerance = 0.00001;
            PointTranslationType = PointTranslationType.None;
            PointTranslationGenerator = null;
        }

        static Func<double> Closure(double radius, Random rnd)
        {
            return () => radius * (rnd.NextDouble() - 0.5);
        }

        /// <summary>
        /// Creates values in range (-radius / 2, radius / 2)
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="randomSeed">If null, initialized to random default System.Random value</param>
        /// <returns></returns>
        public static Func<double> RandomShiftByRadius(double radius = 0.000001, int? randomSeed = 0)
        {
            Random rnd;
            if (randomSeed.HasValue) rnd = new Random(randomSeed.Value);
            else rnd = new Random();
            return Closure(radius, rnd);
        }
    }

    /// <summary>
    /// Configuration of the triangulation computation.
    /// </summary>
    public class TriangulationComputationConfig : ConvexHullComputationConfig
    {
        /// <summary>
        /// If using PointTranslationType.TranslateInternal, this value is
        /// used to determine which boundary cells have zero volume after the
        /// points get "translated back".
        /// 
        /// Default value is 0.00001.
        /// </summary>
        public double ZeroCellVolumeTolerance { get; set; }

        /// <summary>
        /// Create the config with default values set.
        /// </summary>
        public TriangulationComputationConfig()
            : base()
        {
            ZeroCellVolumeTolerance = 0.00001;
        }
    }
}
