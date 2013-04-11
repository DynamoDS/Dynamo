using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Dynamo.Nodes
{
    public class RenderDescription
    {
        public Point3DCollection points;
        public Point3DCollection lines;
        public Mesh3D[] meshes;
    }
    
    public interface IDrawable
    {
        RenderDescription Draw();
    }
}
