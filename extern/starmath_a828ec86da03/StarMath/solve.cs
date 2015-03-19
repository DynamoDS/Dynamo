/*************************************************************************
 *     This file & class is part of the StarMath Project
 *     Copyright 2010, 2011 Matthew Ira Campbell, PhD.
 *
 *     StarMath is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     StarMath is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with StarMath.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on StarMath
 *     at http://starmath.codeplex.com/.
 *************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarMathLib
{
    public static partial class StarMath
    {
        /// <summary>
        /// Solves the specified A matrix.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <param name="b">The b.</param>
        /// <param name="initialGuess">The initial guess.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Matrix, A, must be square.
        /// or
        /// Matrix, A, must be have the same number of rows as the vector, b.</exception>
        public static double[] solve(double[,] A, IList<double> b, IList<double> initialGuess = null)
        {
            var length = A.GetLength(0);
            if (length != A.GetLength(1))
                throw new Exception("Matrix, A, must be square.");
            if (length != b.Count)
                throw new Exception("Matrix, A, must be have the same number of rows as the vector, b.");
            List<int>[] potentialDiagonals;
            if (isGaussSeidelAppropriate(A, b, out potentialDiagonals, ref initialGuess, length))
                return solveGaussSeidel(A, b, initialGuess, length, potentialDiagonals);
            /****** need code to determine when to switch between *****
             ****** this analytical approach and the SOR approach *****/
            return solveByInverse(A, b);
        }

        /// <summary>
        /// Solves the specified A.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <param name="b">The b.</param>
        /// <param name="initialGuess">The initial guess.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Matrix, A, must be square.
        /// or
        /// Matrix, A, must be have the same number of rows as the vector, b.
        /// </exception>
        public static double[] solve(int[,] A, IList<double> b, IList<double> initialGuess = null)
        {
            var length = A.GetLength(0);
            if (length != A.GetLength(1))
                throw new Exception("Matrix, A, must be square.");
            if (length != b.Count)
                throw new Exception("Matrix, A, must be have the same number of rows as the vector, b.");

            var B = new double[length, length];
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                    B[i, j] = A[i, j];
            return solve(B, b, initialGuess);
        }
        /// <summary>
        /// Solves the specified A.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <param name="b">The b.</param>
        /// <param name="initialGuess">The initial guess.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Matrix, A, must be square.
        /// or
        /// Matrix, A, must be have the same number of rows as the vector, b.
        /// </exception>
        public static double[] solve(double[,] A, IList<int> b, IList<double> initialGuess = null)
        {
            return solve(A, b.Select(Convert.ToDouble).ToArray(), initialGuess);
        }
        /// <summary>
        /// Solves the specified A.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <param name="b">The b.</param>
        /// <param name="initialGuess">The initial guess.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Matrix, A, must be square.
        /// or
        /// Matrix, A, must be have the same number of rows as the vector, b.
        /// </exception>
        public static double[] solve(int[,] A, IList<int> b, IList<double> initialGuess = null)
        {
            return solve(A, b.Select(Convert.ToDouble).ToArray(), initialGuess);
        }

        public static double[] solveByInverse(double[,] A, IList<double> b,
             int length = -1, List<int>[] potentialDiagonals = null)
        {
            if (length < 0) length = b.Count;
            double[,] C;
            double[] d;                         
            if (needToReorder(A, length, 0.0))
            {
                if (potentialDiagonals == null &&
                    !findPotentialDiagonals(A, out potentialDiagonals, length, 0.0))
                    return null;
                var order = reorderMatrixForDiagonalDominance(A, length, potentialDiagonals);
                if (order == null) return null;
                C = new double[length, length];
                d = new double[length];
                for (int i = 0; i < length; i++)
                {
                    d[i] = b[order[i]];
                    SetRow(i, C, GetRow(order[i], A));
                }
            }
            else
            {
                C = (double[,])A.Clone();
                d = b.ToArray();
            }
            return multiply(inverse(C), d);
        }

        #region Gauss-Seidel or Successive Over-Relaxation
        /// <summary>
        /// Determines whether [Gauss-Seidel is appropriate] [the specified a].
        /// </summary>
        /// <param name="A">the matrix, A</param>
        /// <param name="b">the right-hand-side values, b</param>
        /// <param name="potentialDiagonals">The potential rows.</param>
        /// <param name="initialGuess">The initial guess.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static bool isGaussSeidelAppropriate(double[,] A, IList<double> b, out List<int>[] potentialDiagonals, ref IList<double> initialGuess, int length)
        {
            potentialDiagonals = null;
            if (length < GaussSeidelMinimumMatrixSize) return false;
            ifInitialGuessIsNull(ref initialGuess, A, b, length);
            var error = norm1(subtract(b, multiply(A, initialGuess, length, length), length)) / norm1(b);
            if (error > MaxErrorForUsingGaussSeidel) return false;
            return findPotentialDiagonals(A, out potentialDiagonals, length, GaussSeidelDiagonalDominanceRatio);
        }

        private static bool findPotentialDiagonals(double[,] A, out List<int>[] potentialDiagonals, int length,
            double minimalConsideration)
        {
            potentialDiagonals = new List<int>[length];
            for (int i = 0; i < length; i++)
            {
                var rowNorm1 = A.GetRow(i).norm1();
                var potentialIndices = new List<int>();
                for (int j = 0; j < length; j++)
                    if (Math.Abs(A[i, j]) / (rowNorm1 - Math.Abs(A[i, j])) >= minimalConsideration)
                        potentialIndices.Add(j);
                if (potentialIndices.Count == 0) return false;
                potentialDiagonals[i] = potentialIndices;
            }
            return potentialDiagonals.SelectMany(x => x).Distinct().Count() == length;
        }

        /// <summary>
        /// Solves the gauss seidel.
        /// </summary>
        /// <param name="A">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="initialGuess">The initial guess.</param>
        /// <param name="length">The length.</param>
        /// <param name="potentialDiagonals">The potential indices.</param>
        /// <returns></returns>
        public static double[] solveGaussSeidel(double[,] A, IList<double> b,
            IList<double> initialGuess = null, int length = -1, List<int>[] potentialDiagonals = null)
        {
            if (length < 0) length = b.Count;
            double[,] C;
            double[] d;

            var x = new double[length];
            ifInitialGuessIsNull(ref initialGuess, A, b, length);
            for (int i = 0; i < length; i++)
                x[i] = initialGuess[i];

            if (needToReorder(A, length, GaussSeidelDiagonalDominanceRatio))
            {
                if (potentialDiagonals == null &&
                    !findPotentialDiagonals(A, out potentialDiagonals, length, GaussSeidelDiagonalDominanceRatio))
                    return null;
                var order = reorderMatrixForDiagonalDominance(A, length, potentialDiagonals);
                if (order == null) return null;
                C = new double[length, length];
                d = new double[length];
                for (int i = 0; i < length; i++)
                {
                    d[i] = b[order[i]];
                    SetRow(i, C, GetRow(order[i], A));
                }
            }
            else
            {
                C = (double[,])A.Clone();
                d = b.ToArray();
            }

            var cNorm1 = norm1(d);
            var error = norm1(subtract(d, multiply(C, x, length, length), length)) / cNorm1;
            var success = error <= GaussSeidelMaxError;
            var xWentNaN = false;
            var iteration = length * length * GaussSeidelMaxIterationFactor;
            while (!xWentNaN && !success && iteration-- > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    var adjust = d[i];
                    for (int j = 0; j < length; j++)
                        if (i != j)
                            adjust -= C[i, j] * x[j];
                    x[i] = (1 - GaussSeidelRelaxationOmega) * x[i] +
                           GaussSeidelRelaxationOmega * adjust / C[i, i];
                }
                xWentNaN = x.Any(double.IsNaN);
                error = norm1(subtract(d, multiply(C, x, length, length), length)) / cNorm1;
                success = error <= GaussSeidelMaxError;
            }
            if (!success) return null;

            return x;
        }

        /// <summary>
        /// Ifs the initial guess is null.
        /// </summary>
        /// <param name="initialGuess">The initial guess.</param>
        /// <param name="A">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="length">The length.</param>
        private static void ifInitialGuessIsNull(ref IList<double> initialGuess, double[,] A, IList<double> b, int length)
        {
            if (initialGuess == null)
            {
                initialGuess = new double[length];
                var initGuessValue = sum(b) / sum(A);
                for (int i = 0; i < length; i++) initialGuess[i] = initGuessValue;
            }
        }
        private static bool needToReorder(double[,] A, int length, double minimalConsideration)
        {
            for (int i = 0; i < length; i++)
            {
                if (A[i, i] == 0.0) return true;
                var rowSum = 0.0;
                for (int j = 0; j < length; j++)
                    if (i != j)
                        rowSum += Math.Abs(A[i, j]);
                if (Math.Abs(A[i, i] / rowSum) < minimalConsideration)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Reorders the matrix for diagonal dominance.
        /// </summary>
        /// <param name="A">a.</param>
        /// <param name="length">The length.</param>
        /// <param name="potentialIndices">The potential indices.</param>
        /// <returns></returns>
        public static int[] reorderMatrixForDiagonalDominance(double[,] A, int length, List<int>[] potentialIndices)
        {
            var popularity = new int[length];
            for (var i = 0; i < length; i++)
                popularity[i] = potentialIndices.Count(r => r.Contains(i));
            var orderToAddress = makeLinearProgression(length, 1).OrderBy(x => popularity[x]).ToList();
            var stack = new Stack<int[]>();
            var seed = new int[length];
            int[] candidate;
            var solutionFound = false;
            for (var i = 0; i < length; i++) seed[i] = -1;
            stack.Push(seed);
            do
            {
                candidate = stack.Pop();
                var numToFill = candidate.Count(x => x == -1);
                if (numToFill == 0) solutionFound = true;
                else
                {
                    var colIndex = orderToAddress[length - numToFill];
                    var possibleIndicesForRow = new List<int>();
                    for (int oldRowIndex = 0; oldRowIndex < length; oldRowIndex++)
                    {
                        if (!potentialIndices[oldRowIndex].Contains(colIndex)) continue;
                        if (candidate.Contains(oldRowIndex)) continue;
                        possibleIndicesForRow.Add(oldRowIndex);
                    }
                    if (possibleIndicesForRow.Count == 1)
                    {
                        candidate[colIndex] = possibleIndicesForRow[0];
                        stack.Push(candidate);
                    }
                    else
                    {
                        possibleIndicesForRow = possibleIndicesForRow.OrderBy(r => Math.Abs(A[r, colIndex])).ToList();
                        foreach (var i in possibleIndicesForRow)
                        {
                            var child = (int[])candidate.Clone();
                            child[colIndex] = i;
                            stack.Push(child);
                        }
                    }
                }
            } while (!solutionFound && stack.Any());
            if (solutionFound) return candidate;
            return null;
        }



        #endregion
    }
}