using System;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A 2D parametric coordinate pair.</summary>
    public sealed class UV : IDisposable
    {
        private UV(double u, double v)
        {
            U = u;
            V = v;
        }

        /// <summary>Gets the U coordinate.</summary>
        public double U { get; }

        /// <summary>Gets the V coordinate.</summary>
        public double V { get; }

        /// <summary>Creates a UV pair from U and V coordinates.</summary>
        /// <param name="u">U coordinate.</param>
        /// <param name="v">V coordinate.</param>
        /// <returns>The created UV pair.</returns>
        public static UV ByCoordinates(double u = 0, double v = 0)
        {
            if (!double.IsFinite(u) || !double.IsFinite(v))
                throw new ArgumentException("UV coordinates must be finite.");
            return new UV(u, v);
        }

        /// <summary>Releases resources owned by this UV.</summary>
        public void Dispose() { }

        /// <inheritdoc />
        public override string ToString() => $"UV(U = {U:0.###}, V = {V:0.###})";
    }
}
