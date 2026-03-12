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
        /// <returns>Tuple containing normalized scale factors in the U and V directions and the maximum physical scale per unit UV parameter, equal to the larger of the normalized scale factors.</returns>
        internal static (double normU, double normV, double maxPhysicalScale) GetNormalizedUvScales(Surface face)
        {
            // Physical scale per unit U/V based on iso-curve lengths along surface edges.
            double scaleU;
            double scaleV;
            using (var uCurveV0 = face.GetIsoline(0, 0))
            using (var uCurveV1 = face.GetIsoline(0, 1))
            using (var vCurveU0 = face.GetIsoline(1, 0))
            using (var vCurveU1 = face.GetIsoline(1, 1))
            {
                scaleU = (uCurveV0.Length + uCurveV1.Length) / 2.0;
                scaleV = (vCurveU0.Length + vCurveU1.Length) / 2.0;
            }

            // Normalize scales to keep values in a reasonable range, preserve aspect ratio
            var max = System.Math.Max(scaleU, scaleV);
            if (max <= 1e-9) max = 1.0;

            var normU = scaleU / max;
            var normV = scaleV / max;

            if (normU <= 1e-9) normU = 1.0;
            if (normV <= 1e-9) normV = 1.0;

            return (normU, normV, max);
        }
    }
}
