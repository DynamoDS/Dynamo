using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Dynamo.Nodes
{
    public class RenderDescription
    {
        public Point3DCollection points = null;
        public Point3DCollection lines = null;
        public List<Mesh3D> meshes = null;

        public RenderDescription()
        {
            points = new Point3DCollection();
            lines = new Point3DCollection();
            meshes = new List<Mesh3D>();
        }
    }
    
    public interface IDrawable
    {
        RenderDescription Draw();
    }

    /// <summary>
    /// An interface for nodes which maintain references to elements
    /// </summary>
    public interface IClearable
    {
        /// <summary>
        /// Clear whatever references this element contains
        /// </summary>
        void ClearReferences();
    }
}
