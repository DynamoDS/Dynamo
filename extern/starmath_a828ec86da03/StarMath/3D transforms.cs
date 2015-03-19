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

namespace StarMathLib
{
    public static partial class StarMath
    {
        #region 3D Coordinate Transforms

        /// <summary>
        /// Creates a translated coordinate frame.
        /// </summary>
        /// <param name = "Tx">Amount of translation in x.</param>
        /// <param name = "Ty">Amount of translation in y.</param>
        /// <param name = "Tz">Amount of translation in z.</param>
        /// <returns>4-by-4 matrix translated by the amount specified.</returns>
        public static double[,] Translate(double Tx, double Ty, double Tz)
        {
            var T = makeIdentity(4);

            T[0, 3] = Tx;
            T[1, 3] = Ty;
            T[2, 3] = Tz;

            return T;
        }

        /// <summary>
        /// Creates a rotation matrix about the X-axis.
        /// </summary>
        /// <param name="angle">The amount of angle in degrees (positive is counter-clockwise).</param>
        /// <param name="inRadians">if set to <c>true</c> [in radians].</param>
        /// <returns>
        /// 4-by-4 matrix rotated by the amount specified.
        /// </returns>
        public static double[,] RotationX(double angle, Boolean inRadians = false)
        {
            var ROTX = makeIdentity(4);
            if (!inRadians)
                angle = (Math.PI * angle) / 180;

            ROTX[1, 1] = ROTX[2, 2] = Math.Cos(angle);
            ROTX[1, 2] = -Math.Sin(angle);
            ROTX[2, 1] = Math.Sin(angle);

            return ROTX;
        }

        /// <summary>
        /// Creates a rotation matrix about the Y-axis.
        /// </summary>
        /// <param name="angle">The amount of angle in degrees (positive is counter-clockwise).</param>
        /// <param name="inRadians">if set to <c>true</c> [in radians].</param>
        /// <returns>
        /// 4-by-4 matrix rotated by the amount specified.
        /// </returns>
        public static double[,] RotationY(double angle, Boolean inRadians = false)
        {
            var ROTY = makeIdentity(4);
            if (!inRadians)
             angle = (Math.PI * angle) / 180;

            ROTY[0, 0] = ROTY[2, 2] = Math.Cos(angle);
            ROTY[2, 0] = -Math.Sin(angle);
            ROTY[0, 2] = Math.Sin(angle);

            return ROTY;
        }

        /// <summary>
        /// Creates a rotation matrix about the Z-axis.
        /// </summary>
        /// <param name="angle">The amount of angle in degrees (positive is counter-clockwise).</param>
        /// <param name="inRadians">if set to <c>true</c> [in radians].</param>
        /// <returns>
        /// 4-by-4 matrix rotated by the amount specified.
        /// </returns>
        public static double[,] RotationZ(double angle, Boolean inRadians = false)
        {
            var ROTZ = makeIdentity(4); 
            if (!inRadians)
              angle = (Math.PI * angle) / 180;

            ROTZ[0, 0] = ROTZ[1, 1] = Math.Cos(angle);
            ROTZ[1, 0] = Math.Sin(angle);
            ROTZ[0, 1] = -Math.Sin(angle);

            return ROTZ;
        }

        #endregion

    }
}