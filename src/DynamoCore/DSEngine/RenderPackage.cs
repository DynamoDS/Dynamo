using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Autodesk.DesignScript.Interfaces;
using DesignScriptStudio.Renderer;
using Dynamo.Models;
using HelixToolkit.Wpf;

namespace Dynamo.DSEngine
{
    class RenderPackage: IRenderPackage, IDisposable
    {
        private IntPtr nativeRenderPackage;
        private bool selected;
        
        private List<double> lineStripVertices = new List<double>();
        private List<byte> lineStripVertexColors = new List<byte>();
        private List<int> lineStripVertexCounts = new List<int>();
        private List<double> pointVertices = new List<double>();
        private List<byte> pointVertexColors = new List<byte>();
        private List<double> triangleVertices = new List<double>();
        private List<byte> triangleVertexColor = new List<byte>();
        private List<double> triangleNormals = new List<double>();

        public RenderPackage(bool selected)
        {
            nativeRenderPackage = RenderPackageUtils.CreateNativeRenderPackage(this);
            this.selected = selected;
        }

        public IntPtr NativeRenderPackage
        {
            get 
            {
                return nativeRenderPackage;
            }
        }

        public void PushLineStripVertex(double x, double y, double z)
        {
            lineStripVertices.Add(x);
            lineStripVertices.Add(y);
            lineStripVertices.Add(z);
        }

        public void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            lineStripVertexColors.Add(red);
            lineStripVertexColors.Add(green);
            lineStripVertexColors.Add(blue);
            lineStripVertexColors.Add(alpha);
        }

        public void PushLineStripVertexCount(int n)
        {
            lineStripVertexCounts.Add(n);
        }

        public void PushPointVertex(double x, double y, double z)
        {
            pointVertices.Add(x);
            pointVertices.Add(y);
            pointVertices.Add(z);
        }

        public void PushPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            pointVertexColors.Add(red);
            pointVertexColors.Add(green);
            pointVertexColors.Add(blue);
            pointVertexColors.Add(alpha);
        }

        public void PushTriangleVertex(double x, double y, double z)
        {
            triangleVertices.Add(x);
            triangleVertices.Add(y);
            triangleVertices.Add(z);
        }

        public void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            triangleVertexColor.Add(red);
            triangleVertexColor.Add(green);
            triangleVertexColor.Add(blue);
            triangleVertexColor.Add(alpha);
        }

        public void PushTriangleVertexNormal(double x, double y, double z)
        {
            triangleNormals.Add(x);
            triangleNormals.Add(y);
            triangleNormals.Add(z);
        }

        public void AddToRenderDescription(NodeModel node, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        {
            var points = selected ? rd.SelectedPoints : rd.Points;
            for (int i = 0; i < pointVertices.Count(); i += 3)
            {
                var point = new Point3D(pointVertices[i], pointVertices[i + 1], pointVertices[i + 2]);
                points.Add(point);
            }

            int idx = 0;
            var lines = selected ? rd.SelectedLines : rd.Lines;
            foreach (var count in lineStripVertexCounts)
            {
                for (int i = 0; i < count; ++i)
                {
                    var point = new Point3D(lineStripVertices[idx], lineStripVertices[idx + 1], lineStripVertices[idx + 2]);
                    lines.Add(point);
                    if (i != 0 && i != count - 1)
                    {
                        lines.Add(point);
                    }
                    idx += 3;
                }
            }

            var builder = new MeshBuilder();
            var tex = new PointCollection();
            var norms = new Vector3DCollection();
            var triangles = new Point3DCollection();
            var tris = new List<int>();

            for (int i = 0; i < triangleVertices.Count(); i += 3)
            {
                var point = new Point3D(triangleVertices[i], triangleVertices[i + 1], triangleVertices[i + 2]); 
                var normal = new Vector3D(triangleNormals[i], triangleNormals[i + 1], triangleNormals[i + 2]);

                tris.Add((i + 1) / 3);
                triangles.Add(point);
                norms.Add(normal);
                tex.Add(new Point(0, 0));

                octree.AddNode(point.X, point.Y, point.Z, node.GUID.ToString());
            }

            builder.Append(triangles, tris, norms, tex);
            if (builder.Positions.Count > 0)
            {
                var meshes = selected ? rd.SelectedMeshes : rd.Meshes;
                meshes.Add(builder.ToMesh(true));
            }
        }

        public void Dispose()
        {
            RenderPackageUtils.DestroyNativeRenderPackage(nativeRenderPackage);
        }
    }
}
