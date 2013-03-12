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


namespace MIConvexHull
{
    public static class StarMath
    {
        #region Printing
        private const int cellWidth = 10;
        private const int numDecimals = 3;

        /// <summary>
        /// Makes the print string.
        /// </summary>
        /// <param name = "A">The A.</param>
        public static string MakePrintString(double[,] A)
        {
            if (A == null) return "<null>";
            var format = "{0:F" + numDecimals + "}";
            var p = "";
            for (var i = 0; i < A.GetLength(0); i++)
            {
                p += "| ";
                for (var j = 0; j < A.GetLength(1); j++)
                    p += formatCell(format, A[i, j]) + ",";
                p = p.Remove(p.Length - 1);
                p += " |\n";
            }
            p = p.Remove(p.Length - 1);
            return p;
        }

        private static object formatCell(string format, double p)
        {
            var numStr = string.Format(format, p);
            numStr = numStr.TrimEnd('0');
            numStr = numStr.TrimEnd('.');
            var padAmt = ((double)(cellWidth - numStr.Length)) / 2;
            numStr = numStr.PadLeft((int)Math.Floor(padAmt + numStr.Length));
            numStr = numStr.PadRight(cellWidth);
            return numStr;
        }
        #endregion

        #region Solve
        /// <summary>
        /// Solves the specified A matrix.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static double[] solve(double[,] A, IList<double> b)
        {
            var aLength = A.GetLength(0);
            if (aLength != A.GetLength(1))
                throw new Exception("Matrix, A, must be square.");
            if (aLength != b.Count)
                throw new Exception("Matrix, A, must be have the same number of rows as the vector, b.");

            /****** need code to determine when to switch between *****
             ****** this analytical approach and the SOR approach *****/
            return multiply(inverse(A, aLength), b, aLength, aLength);
        }

        public static void solveDestructiveInto(double[,] A, double[] b, double[] target)
        {
            var aLength = A.GetLength(0);
            if (aLength != A.GetLength(1))
                throw new Exception("Matrix, A, must be square.");
            if (aLength != b.Length)
                throw new Exception("Matrix, A, must be have the same number of rows as the vector, b.");

            /****** need code to determine when to switch between *****
             ****** this analytical approach and the SOR approach *****/
            inverseInPlace(A, aLength);
            multiplyInto(A, b, aLength, aLength, target);
        }

        /// <summary>
        /// does gaussian elimination.
        /// </summary>
        /// <param name="nDim"></param>
        /// <param name="pfMatr"></param>
        /// <param name="pfVect"></param>
        /// <param name="pfSolution"></param>
        public static void gaussElimination(int nDim, double[][] pfMatr, double[] pfVect, double[] pfSolution)
        {
            double fMaxElem;
            double fAcc;

            int i, j, k, m;
            
            for (k = 0; k < (nDim - 1); k++) // base row of matrix
            {
                var rowK = pfMatr[k];

                // search of line with max element
                fMaxElem = Math.Abs(rowK[k]);
                m = k;
                for (i = k + 1; i < nDim; i++)
                {
                    if (fMaxElem < Math.Abs(pfMatr[i][k]))
                    {
                        fMaxElem = pfMatr[i][k];
                        m = i;
                    }
                }

                // permutation of base line (index k) and max element line(index m)                
                if (m != k)
                {
                    var rowM = pfMatr[m];
                    for (i = k; i < nDim; i++)
                    {                        
                        fAcc = rowK[i];
                        rowK[i] = rowM[i];
                        rowM[i] = fAcc;
                    }
                    fAcc = pfVect[k];
                    pfVect[k] = pfVect[m];
                    pfVect[m] = fAcc;
                }

                //if( pfMatr[k*nDim + k] == 0.0) return 1; // needs improvement !!!

                // triangulation of matrix with coefficients
                for (j = (k + 1); j < nDim; j++) // current row of matrix
                {
                    var rowJ = pfMatr[j];
                    fAcc = -rowJ[k] / rowK[k];
                    for (i = k; i < nDim; i++)
                    {
                        rowJ[i] = rowJ[i] + fAcc * rowK[i];
                    }
                    pfVect[j] = pfVect[j] + fAcc * pfVect[k]; // free member recalculation
                }
            }

            for (k = (nDim - 1); k >= 0; k--)
            {
                var rowK = pfMatr[k];
                pfSolution[k] = pfVect[k];
                for (i = (k + 1); i < nDim; i++)
                {
                    pfSolution[k] -= (rowK[i] * pfSolution[i]);
                }
                pfSolution[k] = pfSolution[k] / rowK[k];
            }
        }

        #endregion

        #region inverse transpose

        /// <summary>
        /// Inverses the matrix A only if the diagonal is all non-zero.
        /// A[i,i] != 0.0
        /// </summary>
        /// <param name = "A">The matrix to invert. This matrix is unchanged by this function.</param>
        /// <param name="length">The length of the number of rows/columns in the square matrix, A.</param>
        /// <returns>The inverted matrix, A^-1.</returns>
        public static double[,] inverse(double[,] A, int length)
        {
            if (length == 1) return new[,] { { 1 / A[0, 0] } };
            return inverseWithLUResult(LUDecomposition(A, length), length);
        }

        public static void inverseInPlace(double[,] A, int length)
        {
            if (length == 1)
            {
                A[0, 0] = 1 / A[0, 0];
                return;
            }
            LUDecompositionInPlace(A, length);
            inverseWithLUResult(A, length);
        }


        /// <summary>
        /// Returns the LU decomposition of A in a new matrix.
        /// </summary>
        /// <param name = "A">The matrix to invert. This matrix is unchanged by this function.</param>
        /// <param name="length">The length of the number of rows/columns in the square matrix, A.</param>
        /// <returns>A matrix of equal size to A that combines the L and U. Here the diagonals belongs to L and the U's diagonal elements are all 1.</returns>
        public static double[,] LUDecomposition(double[,] A, int length)
        {
            var B = (double[,])A.Clone();
            // normalize row 0
            for (var i = 1; i < length; i++) B[0, i] /= B[0, 0];

            for (var i = 1; i < length; i++)
            {
                for (var j = i; j < length; j++)
                {
                    // do a column of L
                    var sum = 0.0;
                    for (var k = 0; k < i; k++)
                        sum += B[j, k] * B[k, i];
                    B[j, i] -= sum;
                }
                if (i == length - 1) continue;
                for (var j = i + 1; j < length; j++)
                {
                    // do a row of U
                    var sum = 0.0;
                    for (var k = 0; k < i; k++)
                        sum += B[i, k] * B[k, j];
                    B[i, j] =
                        (B[i, j] - sum) / B[i, i];
                }
            }
            return B;
        }

        public static void LUDecompositionInPlace(double[,] A, int length)
        {
            var B = A;
            // normalize row 0
            for (var i = 1; i < length; i++) B[0, i] /= B[0, 0];

            for (var i = 1; i < length; i++)
            {
                for (var j = i; j < length; j++)
                {
                    // do a column of L
                    var sum = 0.0;
                    for (var k = 0; k < i; k++)
                        sum += B[j, k] * B[k, i];
                    B[j, i] -= sum;
                }
                if (i == length - 1) continue;
                for (var j = i + 1; j < length; j++)
                {
                    // do a row of U
                    var sum = 0.0;
                    for (var k = 0; k < i; k++)
                        sum += B[i, k] * B[k, j];
                    B[i, j] =
                        (B[i, j] - sum) / B[i, i];
                }
            }
        }

        private static double[,] inverseWithLUResult(double[,] B, int length)
        {
            // this code is adapted from http://users.erols.com/mdinolfo/matrix.htm
            // one constraint/caveat in this function is that the diagonal elts. cannot
            // be zero.
            // if the matrix is not square or is less than B 2x2, 
            // then this function won't work
            #region invert L

            for (var i = 0; i < length; i++)
                for (var j = i; j < length; j++)
                {
                    var x = 1.0;
                    if (i != j)
                    {
                        x = 0.0;
                        for (var k = i; k < j; k++)
                            x -= B[j, k] * B[k, i];
                    }
                    B[j, i] = x / B[j, j];
                }

            #endregion

            #region invert U

            for (var i = 0; i < length; i++)
                for (var j = i; j < length; j++)
                {
                    if (i == j) continue;
                    var sum = 0.0;
                    for (var k = i; k < j; k++)
                        sum += B[k, j] * ((i == k) ? 1.0 : B[i, k]);
                    B[i, j] = -sum;
                }

            #endregion

            #region final inversion

            for (var i = 0; i < length; i++)
                for (var j = 0; j < length; j++)
                {
                    var sum = 0.0;
                    for (var k = ((i > j) ? i : j); k < length; k++)
                        sum += ((j == k) ? 1.0 : B[j, k]) * B[k, i];
                    B[j, i] = sum;
                }

            #endregion

            return B;
        }
        /// <summary>
        /// Returns the determinant of matrix, A.
        /// </summary>
        /// <param name = "A">The input argument matrix. This matrix is unchanged by this function.</param>
        /// <exception cref = "Exception"></exception>
        /// <returns>a single value representing the matrix's determinant.</returns>
        public static double determinant(double[,] A)
        {
            if (A == null) throw new Exception("The matrix, A, is null.");
            var length = A.GetLength(0);
            if (length != A.GetLength(1))
                throw new Exception("The determinant is only possible for square matrices.");
            if (length == 0) return 0.0;
            if (length == 1) return A[0, 0];
            if (length == 2) return (A[0, 0] * A[1, 1]) - (A[0, 1] * A[1, 0]);
            if (length == 3)
                return (A[0, 0] * A[1, 1] * A[2, 2])
                       + (A[0, 1] * A[1, 2] * A[2, 0])
                       + (A[0, 2] * A[1, 0] * A[2, 1])
                       - (A[0, 0] * A[1, 2] * A[2, 1])
                       - (A[0, 1] * A[1, 0] * A[2, 2])
                       - (A[0, 2] * A[1, 1] * A[2, 0]);
            return determinantBig(A, length);
        }

        /// <summary>
        /// Modifies the matrix during the computation if the dimension > 3.
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static double determinantDestructive(double[,] A, int dimension)
        {
            if (A == null) throw new Exception("The matrix, A, is null.");
            var length = dimension;
            //if (length != A.GetLength(1))
            //    throw new Exception("The determinant is only possible for square matrices.");
            if (length == 0) return 0.0;
            if (length == 1) return A[0, 0];
            if (length == 2) return (A[0, 0] * A[1, 1]) - (A[0, 1] * A[1, 0]);
            if (length == 3)
                return (A[0, 0] * A[1, 1] * A[2, 2])
                       + (A[0, 1] * A[1, 2] * A[2, 0])
                       + (A[0, 2] * A[1, 0] * A[2, 1])
                       - (A[0, 0] * A[1, 2] * A[2, 1])
                       - (A[0, 1] * A[1, 0] * A[2, 2])
                       - (A[0, 2] * A[1, 1] * A[2, 0]);
            return determinantBigDestructive(A, length);
        }

        static double determinantBigDestructive(double[,] A, int length)
        {
            LUDecompositionInPlace(A, length);
            var result = 1.0;
            for (var i = 0; i < length; i++)
                if (double.IsNaN(A[i, i]))
                    return 0;
                else result *= A[i, i];
            return result;
        }

        /// <summary>
        /// Returns the determinant of matrix, A. Only used internally for matrices larger than 3.
        /// </summary>
        /// <param name="A">The input argument matrix. This matrix is unchanged by this function.</param>
        /// <param name="length">The length of the side of the square matrix.</param>
        /// <returns>
        /// a single value representing the matrix's determinant.
        /// </returns>
        public static double determinantBig(double[,] A, int length)
        {
            double[,] L, U;
            LUDecomposition(A, out L, out U, length);
            var result = 1.0;
            for (var i = 0; i < length; i++)
                if (double.IsNaN(L[i, i]))
                    return 0;
                else result *= L[i, i];
            return result;
        }
        /// <summary>
        /// Returns the LU decomposition of A in a new matrix.
        /// </summary>
        /// <param name = "A">The matrix to invert. This matrix is unchanged by this function.</param>
        /// <param name = "L">The L matrix is output where the diagonal elements are included and not (necessarily) equal to one.</param>
        /// <param name = "U">The U matrix is output where the diagonal elements are all equal to one.</param>
        /// <param name="length">The length of the number of rows/columns in the square matrix, A.</param>
        public static void LUDecomposition(double[,] A, out double[,] L, out double[,] U, int length)
        {
            L = LUDecomposition(A, length);
            U = new double[length, length];
            for (var i = 0; i < length; i++)
            {
                U[i, i] = 1.0;
                for (var j = i + 1; j < length; j++)
                {
                    U[i, j] = L[i, j];
                    L[i, j] = 0.0;
                }
            }
        }

        #endregion

        #region norm
        /// <summary>
        /// Returns to 2-norm (square root of the sum of squares of all terms)
        /// of the vector, x.
        /// </summary>
        /// <param name="x">The vector, x.</param>
        /// <param name="dontDoSqrt">if set to <c>true</c> [don't take the square root].</param>
        /// <returns>Scalar value of 2-norm.</returns>
        public static double norm2(double[] x, int dim, Boolean dontDoSqrt = false)
        {
            double norm = 0;
            for (int i = 0; i < dim; i++)
            {
                var t = x[i];
                norm += t * t;
            }
            return dontDoSqrt ? norm : Math.Sqrt(norm);
        }
        /// <summary>
        /// Returns to normalized vector (has lenght or 2-norm of 1))
        /// of the vector, x.
        /// </summary>
        /// <param name="x">The vector, x.</param>
        /// <param name="length">The length of the vector.</param>
        /// <returns>unit vector.</returns>
        public static double[] normalize(double[] x, int length)
        {
            return divide(x, norm2(x, length), length);
        }

        public static void normalizeInPlace(double[] x, int length)
        {
            double f = 1.0 / norm2(x, length);
            for (int i = 0; i < length; i++) x[i] *= f;
        }
        #endregion

        #region make extract
        /// <summary>
        /// Gets a row of matrix, A.
        /// </summary>
        /// <param name = "rowIndex">The row index.</param>
        /// <param name = "A">The matrix, A.</param>
        /// <returns>A double array that contains the requested row</returns>
        public static double[] GetRow(int rowIndex, double[,] A)
        {
            var numRows = A.GetLength(0);
            var numCols = A.GetLength(1);
            if ((rowIndex < 0) || (rowIndex >= numRows))
                throw new Exception("MatrixMath Size Error: An index value of "
                                    + rowIndex
                                    + " for getRow is not in required range from 0 up to (but not including) "
                                    + numRows + ".");
            var v = new double[numCols];
            for (var i = 0; i < numCols; i++)
                v[i] = A[rowIndex, i];
            return v;
        }
        /// <summary>
        /// Sets/Replaces the given column of matrix A with the vector v.
        /// </summary>
        /// <param name = "colIndex">Index of the col.</param>
        /// <param name = "A">The A.</param>
        /// <param name = "v">The v.</param>
        public static void SetColumn(int colIndex, double[,] A, IList<double> v)
        {
            var numRows = A.GetLength(0);
            var numCols = A.GetLength(1);
            if ((colIndex < 0) || (colIndex >= numCols))
                throw new Exception("MatrixMath Size Error: An index value of "
                                    + colIndex
                                    + " for getColumn is not in required range from 0 up to (but not including) "
                                    + numCols + ".");
            for (var i = 0; i < numRows; i++)
                A[i, colIndex] = v[i];
        }
        /// <summary>
        /// Sets/Replaces the given row of matrix A with the vector v.
        /// </summary>
        /// <param name = "rowIndex">The index of the row, rowIndex.</param>
        /// <param name = "A">The matrix, A.</param>
        /// <param name = "v">The vector, v.</param>
        public static void SetRow(int rowIndex, double[,] A, IList<double> v)
        {
            var numRows = A.GetLength(0);
            var numCols = A.GetLength(1);
            if ((rowIndex < 0) || (rowIndex >= numRows))
                throw new Exception("MatrixMath Size Error: An index value of "
                                    + rowIndex
                                    + " for getRow is not in required range from 0 up to (but not including) "
                                    + numRows + ".");
            for (var i = 0; i < numCols; i++)
                A[rowIndex, i] = v[i];
        }
        /// <summary>
        /// Makes the zero vector.
        /// </summary>
        /// <param name = "p">The p.</param>
        /// <returns></returns>
        public static double[] makeZeroVector(int p)
        {
            if (p <= 0) throw new Exception("The size, p, must be a positive integer.");
            return new double[p];
        }

        #endregion

        #region add subtract multiply
        /// <summary>
        /// The cross product of two double vectors, A and B, which are of length, 2.
        /// In actuality, there is no cross-product for 2D. This is shorthand for 2D systems 
        /// that are really simplifications of 3D. The returned scalar is actually the value in 
        /// the third (read: z) direction.
        /// </summary>
        /// <param name = "A">1D double Array, A</param>
        /// <param name = "B">1D double Array, B</param>
        /// <returns></returns>
        public static double crossProduct2(IList<double> A, IList<double> B)
        {
            if (((A.Count() == 2) && (B.Count() == 2))
                || ((A.Count() == 3) && (B.Count() == 3) && A[2] == 0.0 && B[2] == 0.0))
                return A[0] * B[1] - B[0] * A[1];
            throw new Exception("This cross product \"shortcut\" is only used with 2D vectors to get the single value in the,"
                                + "would be, Z-direction.");
        }

        /// <summary>
        /// The dot product of the two 1D double vectors A and B
        /// </summary>
        /// <param name="A">1D double Array, A</param>
        /// <param name="B">1D double Array, B</param>
        /// <param name="length">The length of both vectors A and B. This is an optional argument, but if it is already known 
        /// - there is a slight speed advantage to providing it.</param>
        /// <returns>
        /// A double value that contains the dot product
        /// </returns>
        public static double dotProduct(IList<double> A, IList<double> B, int length)
        {
            var c = 0.0;
            for (var i = 0; i != length; i++)
                c += A[i] * B[i];
            return c;
        }
        /// <summary>
        /// The dot product of the one 1D int vector and one 1D double vector
        /// </summary>
        /// <param name="A">1D int Array, A</param>
        /// <param name="B">1D double Array, B</param>
        /// <param name="length">The length of both vectors A and B. This is an optional argument, but if it is already known 
        /// - there is a slight speed advantage to providing it.</param>
        /// <returns>
        /// A double value that contains the dot product
        /// </returns>
        public static double dotProduct(IList<int> A, IList<double> B, int length)
        {
            var c = 0.0;
            for (var i = 0; i != length; i++)
                c += A[i] * B[i];
            return c;
        }
        /// <summary>
        /// Subtracts one vector (B) from the other (A). C = A - B.
        /// </summary>
        /// <param name = "A">The minuend vector, A (1D double)</param>
        /// <param name = "B">The subtrahend vector, B (1D double)</param>
        /// <param name="length">The length of the vectors.</param>
        /// <returns>Returns the difference vector, C (1D double)</returns>
        public static double[] subtract(IList<double> A, IList<double> B, int length)
        {
            var c = new double[length];
            for (var i = 0; i != length; i++)
                c[i] = A[i] - B[i];
            return c;
        }
        /// <summary>
        /// Adds arrays A and B
        /// </summary>
        /// <param name = "A">1D double array 1</param>
        /// <param name = "B">1D double array 2</param>
        /// <param name="length">The length of the array.</param>
        /// <returns>1D double array that contains sum of vectros A and B</returns>
        public static double[] add(IList<double> A, IList<double> B, int length)
        {
            var c = new double[length];
            for (var i = 0; i != length; i++)
                c[i] = A[i] + B[i];
            return c;
        }
        /// <summary>
        /// Divides all elements of a 1D double array by the double value.
        /// </summary>
        /// <param name="B">The vector to be divided</param>
        /// <param name="a">The double value to be divided by, the divisor.</param>
        /// <param name="length">The length of the vector B. This is an optional argument, but if it is already known 
        /// - there is a slight speed advantage to providing it.</param>
        /// <returns>
        /// A 1D double array that contains the product
        /// </returns>
        public static double[] divide(IList<double> B, double a, int length)
        { return multiply((1 / a), B, length); }
        /// <summary>
        /// Multiplies all elements of a 1D double array with the double value.
        /// </summary>
        /// <param name="a">The double value to be multiplied</param>
        /// <param name="B">The double vector to be multiplied with</param>
        /// <param name="length">The length of the vector B. This is an optional argument, but if it is already known 
        /// - there is a slight speed advantage to providing it.</param>
        /// <returns>
        /// A 1D double array that contains the product
        /// </returns>
        public static double[] multiply(double a, IList<double> B, int length)
        {
            // scale vector B by the amount of scalar a
            var c = new double[length];
            for (var i = 0; i != length; i++)
                c[i] = a * B[i];
            return c;
        }

        /// <summary>
        /// Product of a matrix and a vector (2D double and 1D double)
        /// </summary>
        /// <param name = "A">2D double Array</param>
        /// <param name = "B">1D double array - column vector (1 element row)</param>
        /// <param name="numRows">The number of rows.</param>
        /// <param name="numCols">The number of columns.</param>
        /// <returns>A 1D double array that is the product of the two matrices A and B</returns>
        public static double[] multiply(double[,] A, IList<double> B, int numRows, int numCols)
        {
            var C = new double[numRows];

            for (var i = 0; i != numRows; i++)
            {
                C[i] = 0.0;
                for (var j = 0; j != numCols; j++)
                    C[i] += A[i, j] * B[j];
            }
            return C;
        }

        public static void multiplyInto(double[,] A, double[] B, int numRows, int numCols, double[] target)
        {
            var C = target;

            for (var i = 0; i != numRows; i++)
            {
                C[i] = 0.0;
                for (var j = 0; j != numCols; j++)
                    C[i] += A[i, j] * B[j];
            }
        }
        /// <summary>
        /// The cross product of two double vectors, A and B, which are of length, 3.
        /// This is equivalent to calling crossProduct, but a slight speed advantage
        /// may exist in skipping directly to this sub-function.
        /// </summary>
        /// <param name = "A">1D double Array, A</param>
        /// <param name = "B">1D double Array, B</param>
        /// <returns></returns>
        public static double[] crossProduct3(IList<double> A, IList<double> B)
        {
            return new[]
                       {
                           A[1]*B[2] - B[1]*A[2],
                           A[2]*B[0] - B[2]*A[0],
                           A[0]*B[1] - B[0]*A[1]
                       };
        }

        #endregion


        public static double subtractAndDot(double[] n, double[] l, double[] r, int dim)
        {
            double acc = 0;
            for (int i = 0; i < dim; i++)
            {
                double t = l[i] - r[i];
                acc += n[i] * t;
            }

            return acc;
        }
    }
}