using System;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A solid backed by a native boundary representation.</summary>
    public class Solid : Geometry
    {
        private readonly NativeGeometry.SolidHandle handle;

        internal Solid(NativeGeometry.SolidHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets the enclosed volume in cubed model units.</summary>
        public double Volume => NativeGeometry.Volume(handle);

        /// <summary>Subtracts another solid from this solid and returns the result.</summary>
        /// <param name="other">The solid to subtract.</param>
        /// <returns>The remainder after subtraction.</returns>
        public Solid Difference(Solid other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return new Solid(NativeGeometry.Difference(handle, other.handle));
        }

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            var chordTolerance = parameters.Tolerance > 0 ? parameters.Tolerance : 0;
            foreach (var vertex in NativeGeometry.Tessellate(handle, chordTolerance))
            {
                var position = vertex.Position;
                var normal = vertex.Normal;
                package.AddTriangleVertex(position.X, position.Y, position.Z);
                package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                package.AddTriangleVertexUV(0, 0);
            }
        }

        internal override void ReleaseResources()
        {
            handle.Dispose();
        }
    }
}
