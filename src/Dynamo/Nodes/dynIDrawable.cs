using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Dynamo.Nodes
{
    public interface IDrawable
    {
        Point3DCollection Points();
        Point3DCollection Lines(); // each point pair is a straight line
        Mesh3D[] Meshes();
    }
}
