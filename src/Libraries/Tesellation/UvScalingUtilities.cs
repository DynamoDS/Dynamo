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
        /// <returns>Tuple containing normalized U and V scale factors.</returns>
        internal static (double normU, double normV) GetNormalizedUvScales(Surface face)
        {
            // Physical scale per unit U/V (affine for planar Rectangle->Surface.ByPatch)
            var p00 = face.PointAtParameter(0, 0);
            var p10 = face.PointAtParameter(1, 0);
            var p01 = face.PointAtParameter(0, 1);

            var scaleU = p00.DistanceTo(p10);
            var scaleV = p00.DistanceTo(p01);

            // Normalize scales to keep values in a reasonable range, preserve aspect ratio
            var max = System.Math.Max(scaleU, scaleV);
            if (max <= 1e-9) max = 1.0;

            var normU = scaleU / max;
            var normV = scaleV / max;

            if (normU <= 1e-9) normU = 1.0;
            if (normV <= 1e-9) normV = 1.0;

            return (normU, normV);
        }
    }
}
