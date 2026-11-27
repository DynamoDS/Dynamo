using System;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>Base type for geometry that can appear in Dynamo's preview.</summary>
    public abstract class Geometry : IGraphicItem, IDisposable
    {
        internal Geometry() { }

        /// <summary>Releases resources owned by this object.</summary>
        public void Dispose()
        {
            ReleaseResources();
        }

        void IGraphicItem.Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(package);
            ArgumentNullException.ThrowIfNull(parameters);
            WritePreview(package, parameters);
        }

        internal abstract void WritePreview(IRenderPackage package, TessellationParameters parameters);

        internal virtual void ReleaseResources() { }
    }
}
