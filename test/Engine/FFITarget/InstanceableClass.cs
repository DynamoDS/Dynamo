using Autodesk.DesignScript.Geometry;
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
    public class InstanceableClass : IInstanceableGraphicItem, IGraphicItem
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

         void IInstanceableGraphicItem.AddBaseTessellation(IInstancingRenderPackage instancingPkg, TessellationParameters parameters)
        {

            if (instancingPkg is IRenderPackage renderPackage)
            {
                var previousMeshVertexCount = renderPackage.MeshVertexCount;

                renderPackage.AddTriangleVertex(0, 0, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(0, 1, 0);

                renderPackage.AddTriangleVertex(-1, 0, 0);
                renderPackage.AddTriangleVertex(-.5,.5, 1);
                renderPackage.AddTriangleVertex(0, 0, 0);

                renderPackage.AddTriangleVertex(-1, 1, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(-1, 0, 0);

                renderPackage.AddTriangleVertex(0, 1, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(-1, 1, 0);

                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);



                instancingPkg.AddInstanceGuidForMeshVertexRange(previousMeshVertexCount, renderPackage.MeshVertexCount - 1, BaseTessellationGuid);
            }
        }

        //actually create the matrix.
        void IInstanceableGraphicItem.AddInstance(IInstancingRenderPackage package, TessellationParameters parameters, string labelKey)
        {
            
                var Origin = (x: position_axes[0], y: position_axes[1], z: position_axes[2]);

                var XAxis = (x: position_axes[3], y: position_axes[4], z: position_axes[5]);
                var YAxis = (x: position_axes[6], y: position_axes[7], z: position_axes[8]);
                var ZAxis = (x: position_axes[9], y: position_axes[10], z: position_axes[11]);

                var w = (float)width_length_height[0];
                var l = (float)width_length_height[1];
                var h = (float)width_length_height[2];
                var s = (float)1;

                package.AddInstanceMatrix(
                    (float)XAxis.x * w, (float)XAxis.z * w, -(float)XAxis.y * w, 0,
                    (float)ZAxis.x * h, (float)ZAxis.z * h, -(float)ZAxis.y * h, 0,
                    -(float)YAxis.x * l, -(float)YAxis.z * l, (float)YAxis.y * l, 0,
                    (float)Origin.x * s, (float)Origin.z * s, -(float)Origin.y * s, 1,
                    BaseTessellationGuid);
            
        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            throw new NotImplementedException();
        }
    }

    public class InstanceAndTransformable : InstanceableClass, IInstanceableGraphicItem
    {
        private CoordinateSystem transform;
        public static InstanceAndTransformable ByPositionAndAxes(double[] position_x_y_z_axes, double[] width_length_height,CoordinateSystem transform )
        {
            var inst = new InstanceAndTransformable();
            inst.position_axes = position_x_y_z_axes;
            inst.width_length_height = width_length_height;
            inst.transform = transform;
            return inst;
        }

        void IInstanceableGraphicItem.AddBaseTessellation(IInstancingRenderPackage instancingPkg, TessellationParameters parameters)
        {

            if (instancingPkg is IRenderPackage renderPackage)
            {
                var previousMeshVertexCount = renderPackage.MeshVertexCount;

                renderPackage.AddTriangleVertex(0, 0, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(0, 1, 0);

                renderPackage.AddTriangleVertex(-1, 0, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(0, 0, 0);

                renderPackage.AddTriangleVertex(-1, 1, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(-1, 0, 0);

                renderPackage.AddTriangleVertex(0, 1, 0);
                renderPackage.AddTriangleVertex(-.5, .5, 1);
                renderPackage.AddTriangleVertex(-1, 1, 0);

                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);
                renderPackage.AddTriangleVertexNormal(0, 0, 1);



                instancingPkg.AddInstanceGuidForMeshVertexRange(previousMeshVertexCount, renderPackage.MeshVertexCount - 1, BaseTessellationGuid);
            }
            if(instancingPkg is ITransformable transpkg)
            {
                var xaxis = transform.XAxis;
                var yaxis = transform.YAxis;
                var zaxis = transform.ZAxis;
                var org = transform.Origin;

                transpkg.SetTransform(xaxis.X, xaxis.Z, -xaxis.Y, 0,
                                                                        zaxis.X, zaxis.Z, -zaxis.Y, 0,
                                                                        -yaxis.X, -yaxis.Z, yaxis.Y, 0,
                                                                          org.X, org.Z, -org.Y, 1);
            }
        }

    }

    /// <summary>
    /// A test class that creates a pyramid using instancing.
    /// </summary>
    public class InstanceableLineClass : IInstanceableGraphicItem, IGraphicItem
    {
        public static InstanceableLineClass ByPositionAndAxes(double[] position_x_y_z_axes, double[] width_length_height)
        {
            var inst = new InstanceableLineClass();
            inst.position_axes = position_x_y_z_axes;
            inst.width_length_height = width_length_height;
            return inst;
        }

        internal double[] position_axes { get; set; }
        internal double[] width_length_height { get; set; }

        public Guid BaseTessellationGuid => Guid.Parse("fe63b3fd-f44a-47a2-a002-58538dbbf818");

        public bool InstanceInfoAvailable => true;

        void IInstanceableGraphicItem.AddBaseTessellation(IInstancingRenderPackage instancingPkg, TessellationParameters parameters)
        {

            if (instancingPkg is IRenderPackage renderPackage)
            {
                var prevLineCount = renderPackage.LineVertexCount;

                renderPackage.AddLineStripVertex(0, 0, 0);
                renderPackage.AddLineStripVertex(-.5, .5, 1);
                renderPackage.AddLineStripVertex(0, 1, 0);
                renderPackage.AddLineStripVertex(0, 0, 0);

                renderPackage.AddLineStripVertex(-1, 0, 0);
                renderPackage.AddLineStripVertex(-.5, .5, 1);
                renderPackage.AddLineStripVertex(0, 0, 0);
                renderPackage.AddLineStripVertex(-1, 0, 0);

                renderPackage.AddLineStripVertex(-1, 1, 0);
                renderPackage.AddLineStripVertex(-.5, .5, 1);
                renderPackage.AddLineStripVertex(-1, 0, 0);
                renderPackage.AddLineStripVertex(-1, 1, 0);


                renderPackage.AddLineStripVertex(0, 1, 0);
                renderPackage.AddLineStripVertex(-.5, .5, 1);
                renderPackage.AddLineStripVertex(-1, 1, 0);
                renderPackage.AddLineStripVertex(0, 1, 0);
                

                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);
                renderPackage.AddLineStripVertexColor(0, 0, 0, 255);

                renderPackage.AddLineStripVertexCount(16);


                instancingPkg.AddInstanceGuidForLineVertexRange(prevLineCount, renderPackage.LineVertexCount - 1, BaseTessellationGuid);
            }
        }

        //actually create the matrix.
        void IInstanceableGraphicItem.AddInstance(IInstancingRenderPackage package, TessellationParameters parameters, string labelKey)
        {

            var Origin = (x: position_axes[0], y: position_axes[1], z: position_axes[2]);

            var XAxis = (x: position_axes[3], y: position_axes[4], z: position_axes[5]);
            var YAxis = (x: position_axes[6], y: position_axes[7], z: position_axes[8]);
            var ZAxis = (x: position_axes[9], y: position_axes[10], z: position_axes[11]);

            var w = (float)width_length_height[0];
            var l = (float)width_length_height[1];
            var h = (float)width_length_height[2];
            var s = (float)1;

            package.AddInstanceMatrix(
                (float)XAxis.x * w, (float)XAxis.z * w, -(float)XAxis.y * w, 0,
                (float)ZAxis.x * h, (float)ZAxis.z * h, -(float)ZAxis.y * h, 0,
                -(float)YAxis.x * l, -(float)YAxis.z * l, (float)YAxis.y * l, 0,
                (float)Origin.x * s, (float)Origin.z * s, -(float)Origin.y * s, 1,
                BaseTessellationGuid);

        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// This class is instanceable, but it returns false for instanceInfoAvailable so it will use regular tessellation.
    /// </summary>
    public class InstanceableClass_NoInstanceData : IInstanceableGraphicItem, IGraphicItem
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

        void IInstanceableGraphicItem.AddBaseTessellation(IInstancingRenderPackage package, TessellationParameters parameters)
        {
            throw new NotImplementedException();
        }

        void IInstanceableGraphicItem.AddInstance(IInstancingRenderPackage package, TessellationParameters parameters, string labelKey)
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
