using System;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A group of 3 (triangle) or 4 (quad) vertex indices defining a mesh face.</summary>
    public readonly struct IndexGroup
    {
        private IndexGroup(uint a, uint b, uint c, uint d, uint count)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            Count = count;
        }

        /// <summary>Gets the first vertex index.</summary>
        public uint A { get; }

        /// <summary>Gets the second vertex index.</summary>
        public uint B { get; }

        /// <summary>Gets the third vertex index.</summary>
        public uint C { get; }

        /// <summary>Gets the fourth vertex index (for quad faces; unused when <see cref="Count"/> is 3).</summary>
        public uint D { get; }

        /// <summary>Gets the number of indices in this group (3 for triangle, 4 for quad).</summary>
        public uint Count { get; }

        /// <summary>Creates a triangular face index group.</summary>
        public static IndexGroup ByIndices(uint a, uint b, uint c) => new(a, b, c, 0, 3);

        /// <summary>Creates a quadrilateral face index group.</summary>
        public static IndexGroup ByIndices(uint a, uint b, uint c, uint d) => new(a, b, c, d, 4);

        /// <inheritdoc />
        public override string ToString() =>
            Count == 4
                ? $"IndexGroup(A = {A}, B = {B}, C = {C}, D = {D})"
                : $"IndexGroup(A = {A}, B = {B}, C = {C})";
    }
}
