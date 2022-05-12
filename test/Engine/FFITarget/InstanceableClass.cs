using Autodesk.DesignScript.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFITarget
{
    /// <summary>
    /// A test class that creates a pyramid using instancing.
    /// </summary>
    public class InstanceableClass : IInstanceableItem, IGraphicItem
    {
        public static InstanceableClass ByPositionAndAxes(double[] position_x_y_z_axes,double[] width_length_height)
        {
            var inst = new InstanceableClass();
            inst.position_axes = position_x_y_z_axes;
            inst.width_length_height = width_length_height;
            return inst;
        }

        internal double[] position_axes { get; set; }
        internal double[] width_length_height { get; set; }

        public Guid BaseTessellationGuid => Guid.Parse("fe63b3fd-f44a-47a2-a002-58537dbbf817");

        public bool InstanceInfoAvailable => true;

        public void AddBaseTessellation(IRenderPackage package, TessellationParameters parameters)
        {

            if (package is IRenderInstances packageInstances)
            {
                var previousMeshVertexCount = package.MeshVertexCount;

                package.AddTriangleVertex(0, 0, 0);
                package.AddTriangleVertex(-.5, .5, 1);
                package.AddTriangleVertex(0, 1, 0);

                package.AddTriangleVertex(-1, 0, 0);
                package.AddTriangleVertex(-.5,.5, 1);
                package.AddTriangleVertex(0, 0, 0);

                package.AddTriangleVertex(-1, 1, 0);
                package.AddTriangleVertex(-.5, .5, 1);
                package.AddTriangleVertex(-1, 0, 0);

                package.AddTriangleVertex(0, 1, 0);
                package.AddTriangleVertex(-.5, .5, 1);
                package.AddTriangleVertex(-1, 1, 0);

                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);
                package.AddTriangleVertexNormal(0, 0, 1);



                packageInstances.AddInstanceGuidForMeshVerticesRange(previousMeshVertexCount, package.MeshVertexCount - 1, BaseTessellationGuid);
            }
        }

        //actually create the matrix.
        public void AddInstance(IRenderPackage package, TessellationParameters parameters, string labelKey)
        {
            if (package is IRenderInstances rpinst)
            {
                var Origin = (x: position_axes[0], y: position_axes[1], z: position_axes[2]);

                var XAxis = (x: position_axes[3], y: position_axes[4], z: position_axes[5]);
                var YAxis = (x: position_axes[6], y: position_axes[7], z: position_axes[8]);
                var ZAxis = (x: position_axes[9], y: position_axes[10], z: position_axes[11]);

                var w = (float)width_length_height[0];
                var l = (float)width_length_height[1];
                var h = (float)width_length_height[2];
                var s = (float)1;

                rpinst.AddInstanceMatrix(
                    (float)XAxis.x * w, (float)XAxis.z * w, -(float)XAxis.y * w, 0,
                    (float)ZAxis.x * h, (float)ZAxis.z * h, -(float)ZAxis.y * h, 0,
                    -(float)YAxis.x * l, -(float)YAxis.z * l, (float)YAxis.y * l, 0,
                    (float)Origin.x * s, (float)Origin.z * s, -(float)Origin.y * s, 1,
                    BaseTessellationGuid);
            }
        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// This class is instanceable, but it returns false for instanceInfoAvailable so it will use regular tessellation.
    /// </summary>
    public class InstanceableClass_NoInstanceData : IInstanceableItem, IGraphicItem
    {
        public static InstanceableClass_NoInstanceData ByPositionAndAxes(double[] position)
        {
            var inst = new InstanceableClass_NoInstanceData();
            inst.position = position;
            return inst;
        }
        internal double[] position { get; set; }

        public Guid BaseTessellationGuid => Guid.Parse("fb8fa9ea-5837-4b4e-a7d2-7854d24e138a");

        public bool InstanceInfoAvailable => false;

        public void AddBaseTessellation(IRenderPackage package, TessellationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void AddInstance(IRenderPackage package, TessellationParameters parameters, string labelKey)
        {
            throw new NotImplementedException();
        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];

            package.AddTriangleVertex(0+x, 0+y, 0+z);
            package.AddTriangleVertex(-.5+x, .5+y, 1+z);
            package.AddTriangleVertex(0+x, 1+y, 0+z);
            
            package.AddTriangleVertexNormal(-1, 0, -.5);
            package.AddTriangleVertexNormal(-1, 0, -.5);
            package.AddTriangleVertexNormal(-1, 0, -.5);

            package.AddTriangleVertexColor(255, 0, 0, 255);
            package.AddTriangleVertexColor(255, 0, 0, 255);
            package.AddTriangleVertexColor(255, 0, 0, 255);
        }
    }
}
