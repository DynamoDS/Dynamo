using Autodesk.DesignScript.Geometry;

namespace Tessellation
{
    /// <summary>
    ///     Utility functions for UV scaling on surfaces.
    /// </summary>
    internal static class UvScalingUtilities
    {
        /// <summary>
        ///     Computes normalized UV scaling factors for a surface to handle anisotropic parameter spaces.
        ///     The scaling preserves the aspect ratio while keeping values in a reasonable numerical range.
        /// </summary>
        /// <param name="face">Surface to compute scaling factors for.</param>
        /// <returns>Tuple containing normalized scale factors in the U and V directions, and the
        /// minimum physical arc length of the surface (world units) across U and V.
        /// The minimum scale is the shorter of the two average iso-curve lengths and is used
        /// to derive world-space distance thresholds that scale correctly with the surface.</returns>
        internal static (double normU, double normV, double minPhysicalScale) GetNormalizedUvScales(Surface face)
        {
            // Physical arc length per unit U/V based on the maximum iso-curve length across
            // three interior sample positions. Sampling at boundary extrema (0 or 1) fails for
            // surfaces where those extrema are degenerate points — sphere poles, cone apex —
            // because the isoline length collapses to near-zero there. Interior sampling avoids
            // this: for a sphere the equatorial isoline (t=0.5) gives the correct scale, and
            // for a non-degenerate surface all interior isolines have similar length so the
            // result is unchanged from the previous boundary-average approach.
            double scaleU = 0.0;
            double scaleV = 0.0;
            double[] interiorSamples = { 0.25, 0.5, 0.75 };
            foreach (var t in interiorSamples)
            {
                using var uCurve = face.GetIsoline(0, t);
                using var vCurve = face.GetIsoline(1, t);
                scaleU += uCurve.Length;
                scaleV += vCurve.Length;
            }
            scaleU /= interiorSamples.Length;
            scaleV /= interiorSamples.Length;

            // Normalize scales to preserve aspect ratio; keep values in a reasonable numerical range.
            var max = System.Math.Max(scaleU, scaleV);
            if (max <= 1e-9) max = 1.0;

            var normU = scaleU / max;
            var normV = scaleV / max;

            if (normU <= 1e-9) normU = 1.0;
            if (normV <= 1e-9) normV = 1.0;

            // minPhysicalScale is the shorter physical dimension of the surface in world units.
            var minPhysicalScale = System.Math.Min(scaleU, scaleV);
            if (minPhysicalScale <= 1e-9) minPhysicalScale = 1.0;

            return (normU, normV, minPhysicalScale);
        }
    }
}
