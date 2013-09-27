using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Dynamo
{

    /// <summary>
    /// RenderDescriptions provides the final geometry to be bound by the visualization.
    /// </summary>
    public class RenderDescription
    {
        /// <summary>
        /// A collection of Point objects used to render points
        /// </summary>
        public List<Point3D> Points { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render lines
        /// </summary>
        public List<Point3D> Lines { get; internal set; }

        /// <summary>
        /// A collection of mesh objects to be used for rendering
        /// </summary>
        public List<MeshGeometry3D> Meshes { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the x axes of transforms
        /// </summary>
        public List<Point3D> XAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the y axes of transforms
        /// </summary>
        public List<Point3D> YAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the z axes of transforms
        /// </summary>
        public List<Point3D> ZAxisPoints { get; internal set; }

        public RenderDescription()
        {
            Points = new List<Point3D>();
            Lines = new List<Point3D>();
            Meshes = new List<MeshGeometry3D>();
            XAxisPoints = new List<Point3D>();
            YAxisPoints = new List<Point3D>();
            ZAxisPoints = new List<Point3D>();
        }

        public void Clear()
        {
            Points.Clear();
            Lines.Clear();
            Meshes.Clear();
            XAxisPoints.Clear();
            YAxisPoints.Clear();
            ZAxisPoints.Clear();
        }
    }
}
