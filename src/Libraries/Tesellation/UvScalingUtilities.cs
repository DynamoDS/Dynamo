using System;
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
        // A direction whose isoline lengths vary more than this factor across the domain has a
        // non-uniform parameterization (e.g. sphere poles, cone apex). A single global scale
        // factor cannot correct for position-dependent distortion, so scaling is skipped.
        private const double NonUniformityThreshold = 3.0;

        internal static (double normU, double normV, double minPhysicalScale) GetNormalizedUvScales(Surface face)
        {
            // Sample at five positions spanning the interior, including near-boundary positions
            // (0.1 / 0.9) to amplify the signal from surfaces that are degenerate at their
            // extrema (sphere poles, cone apex) without hitting the degenerate point itself.
            ReadOnlySpan<double> samples = [0.1, 0.25, 0.5, 0.75, 0.9];
            double scaleU = 0.0, scaleV = 0.0;
            double minU = double.MaxValue, maxU = 0.0;
            double minV = double.MaxValue, maxV = 0.0;

            foreach (var t in samples)
            {
                using var uCurve = face.GetIsoline(0, t);
                using var vCurve = face.GetIsoline(1, t);
                var lu = uCurve.Length;
                var lv = vCurve.Length;
                scaleU += lu; scaleV += lv;
                minU = Math.Min(minU, lu); maxU = Math.Max(maxU, lu);
                minV = Math.Min(minV, lv); maxV = Math.Max(maxV, lv);
            }
            scaleU /= samples.Length;
            scaleV /= samples.Length;

            // minPhysicalScale is the shorter average dimension, used by callers to derive
            // world-space distance thresholds that scale with the surface size.
            var minPhysicalScale = Math.Min(scaleU, scaleV);
            if (minPhysicalScale <= 1e-9) minPhysicalScale = 1.0;

            // If isolines in either direction vary more than NonUniformityThreshold across the
            // domain, the parameterization distortion is spatially non-uniform. A global scale
            // cannot fix position-dependent distortion and would only introduce asymmetry, so
            // return identity scaling and let callers work in raw UV space.
            if (maxU > minU * NonUniformityThreshold || maxV > minV * NonUniformityThreshold)
                return (1.0, 1.0, minPhysicalScale);

            // Normalize to preserve aspect ratio; guard against degenerate (zero-area) surfaces.
            var max = Math.Max(scaleU, scaleV);
            if (max <= 1e-9) max = 1.0;

            var normU = scaleU / max;
            var normV = scaleV / max;
            if (normU <= 1e-9) normU = 1.0;
            if (normV <= 1e-9) normV = 1.0;

            return (normU, normV, minPhysicalScale);
        }
    }
}
