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


namespace StarMathLib
{
    public static partial class StarMath
    {
        /// <summary>
        /// Makes the print string.
        /// </summary>
        /// <param name="A">The matrix, A.</param>
        /// <returns></returns>
        public static string MakePrintString(this double[,] A)
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


        /// <summary>
        /// Makes the print string.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <returns></returns>
        public static string MakePrintString(this IEnumerable<double> A)
        {
            if (A == null) return "<null>";
            var format = "{0:F" + numDecimals + "}";
            var p = "{ ";
            foreach (var d in A)
                p += formatCell(format, d) + ",";
            p = p.Remove(p.Length - 1);
            p += " }";
            return p;
        }

        /// <summary>
        /// Makes the print string.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <returns></returns>
        public static string MakePrintString(this int[,] A)
        {
            if (A == null) return "<null>";
            const string format = "{0}";
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

        /// <summary>
        /// Makes the print string.
        /// </summary>
        /// <param name="A">The A.</param>
        /// <returns></returns>
        public static string MakePrintString(this IEnumerable<int> A)
        {
            if (A == null) return "<null>";
            var format = "{0:F" + numDecimals + "}";
            var p = "{ ";
            foreach (var d in A)
                p += formatCell(format, d) + ",";
            p = p.Remove(p.Length - 1);
            p += " }";
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
    }
}