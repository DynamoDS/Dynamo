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
        public Point3DCollection xAxisPoints = null;
        public Point3DCollection yAxisPoints = null;
        public Point3DCollection zAxisPoints = null;

        public RenderDescription()
        {
            points = new Point3DCollection();
            lines = new Point3DCollection();
            meshes = new List<Mesh3D>();
            xAxisPoints = new Point3DCollection();
            yAxisPoints = new Point3DCollection();
            zAxisPoints = new Point3DCollection();
        }

        public void ClearAll()
        {
            points.Clear();
            lines.Clear();
            meshes.Clear();
            xAxisPoints.Clear();
            yAxisPoints.Clear();
            zAxisPoints.Clear();
        }
    }
    
    public interface IDrawable
    {
        RenderDescription RenderDescription { get; set; }
        void Draw();
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
