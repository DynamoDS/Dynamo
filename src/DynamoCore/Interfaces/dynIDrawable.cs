using System.Collections.Generic;
//using System.Windows.Media.Media3D;

namespace Dynamo.Nodes
{
    public class RenderDescription
    {
        public List<object> points = null;
        public List<object> lines = null;
        public List<object> meshes = null;
        public List<object> xAxisPoints = null;
        public List<object> yAxisPoints = null;
        public List<object> zAxisPoints = null;

        public RenderDescription()
        {
            points = new List<object>();
            lines = new List<object>();
            meshes = new List<object>();
            xAxisPoints = new List<object>();
            yAxisPoints = new List<object>();
            zAxisPoints = new List<object>();
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
