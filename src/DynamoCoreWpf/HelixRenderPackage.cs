using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Autodesk.DesignScript.Interfaces;

using Dynamo.DSEngine;
using Dynamo.Interfaces;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;

using SharpDX;

namespace Dynamo.Wpf
{
    /// <summary>
    /// A Helix-specific IRenderPackageFactory implementation.
    /// </summary>
    public class HelixRenderPackageFactory : IRenderPackageFactory
    {
        public HelixRenderPackageFactory(){}

        public IRenderPackage CreateRenderPackage()
        {
            return new HelixRenderPackage();
        }
    }

    /// <summary>
    /// A Helix-specifc IRenderPackage implementation.
    /// </summary>
    public class HelixRenderPackage : IRenderPackage
    {
        private PointGeometry3D points;
        private LineGeometry3D lines;
        private BillboardText3D text;
        private MeshGeometry3D mesh;
        private bool hasData;

        public HelixRenderPackage()
        {
            points = InitPointGeometry();
            lines = InitLineGeometry();
            text = InitText3D();
            mesh = InitMeshGeometry();

            LineStripVertexCounts = new List<int>();
            IsSelected = false;
        }

        #region IRenderPackage implementation

        public void Clear()
        {
            points = null;
            lines = null;
            text = null;
            mesh = null;

            LineStripVertexCounts.Clear();
            IsSelected = false;
        }

        public void PushPointVertex(double x, double y, double z)
        {
            //if (i == 0 && ((RenderPackage)p).DisplayLabels)
            //{
            //    text.TextInfo.Add(new TextInfo(CleanTag(Tag), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
            //}

            points.Indices.Add(points.Positions.Count);
            points.Positions.Add(Vector3ForYUp(x,y,z));
        }

        public void PushPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            var ptColor = Color4FromBytes(red, green, blue, alpha);
            points.Colors.Add(ptColor);
        }

        public void PushTriangleVertex(double x, double y, double z)
        {
            mesh.Indices.Add(mesh.Indices.Count);
            mesh.Positions.Add(Vector3ForYUp(x, y, z));
        }

        public void PushTriangleVertexNormal(double x, double y, double z)
        {
            mesh.Normals.Add(Vector3ForYUp(x,y,z));
        }

        public void PushTriangleVertexUV(double u, double v)
        {
            mesh.TextureCoordinates.Add(new Vector2((float)u, (float)v));
        }

        public void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            mesh.Colors.Add(Color4FromBytes(red,green,blue,alpha));
        }

        public void PushLineStripVertex(double x, double y, double z)
        {
            lines.Indices.Add(lines.Indices.Count);
            lines.Positions.Add(Vector3ForYUp(x,y,z));
        }

        public void PushLineStripVertexCount(int n)
        {
            LineStripVertexCounts.Add(n);
        }

        public void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            lines.Colors.Add(Color4FromBytes(red,green,blue,alpha));
        }

        public List<double> LineStripVertices
        {
            get { return lines.Positions.ToDoubles(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> PointVertices
        {
            get { return points.Positions.ToDoubles(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> TriangleVertices
        {
            get { return mesh.Positions.ToDoubles(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> TriangleNormals
        {
            get { return mesh.Normals.ToDoubles(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> TriangleUVs
        {
            get { return mesh.Normals.ToDoubles(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<byte> PointVertexColors
        {
            get { return points.Colors.ToBytes(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<byte> LineStripVertexColors
        {
            get { return lines.Colors.ToBytes(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<byte> TriangleVertexColors
        {
            get { return mesh.Colors.ToBytes(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<int> LineStripVertexCounts { get; set; }

        public string Tag { get; set; }

        public bool IsSelected { get; set; }

        public bool HasData
        {
            get
            {
                var hasData = points.Positions.Count > 0 || 
                    lines.Positions.Count > 0 ||
                    mesh.Positions.Count > 0;
                return hasData;
            }
        }

        public bool IsDisplayingLabels { get; set; }

        public IntPtr NativeRenderPackage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public MeshGeometry3D Mesh
        {
            get { return mesh; }
        }

        public LineGeometry3D Lines
        {
            get { return lines; }
        }

        public PointGeometry3D Points
        {
            get
            {
                return points;
            }
        }

        public BillboardText3D Text
        {
            get { return text; }
        }

        internal static LineGeometry3D InitLineGeometry()
        {
            var lines = new LineGeometry3D
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            return lines;
        }

        internal static PointGeometry3D InitPointGeometry()
        {
            var points = new PointGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            return points;
        }

        internal static MeshGeometry3D InitMeshGeometry()
        {
            var mesh = new MeshGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection(),
                Normals = new Vector3Collection(),
            };

            return mesh;
        }

        internal static BillboardText3D InitText3D()
        {
            var text3D = new BillboardText3D();

            return text3D;
        }

        private string CleanTag(string tag)
        {
            var splits = tag.Split(':');
            if (splits.Count() <= 1) return tag;

            var sb = new StringBuilder();
            for (int i = 1; i < splits.Count(); i++)
            {
                sb.AppendFormat("[{0}]", splits[i]);
            }
            return sb.ToString();
        }

        private static Color4 Color4FromBytes(byte red, byte green, byte blue, byte alpha)
        {
            var color = new Color4((red / 255.0f),(green / 255.0f),(blue / 255.0f), (alpha/255.0f));
            return color;
        }

        private static Vector3 Vector3ForYUp(double x, double y, double z)
        {
            return new Vector3((float)x, (float)z, (float)-y);
        } 
    }

    public static class HelixRenderExtensions
    {
        public static List<double> ToDoubles(this Vector3Collection collection)
        {
            var doubles = new List<double>();
            foreach (var v in collection)
            {
                doubles.Add(v.X);
                doubles.Add(v.Y);
                doubles.Add(v.Z);
            }

            return doubles;
        }

        public static List<byte> ToBytes(this Color4Collection collection)
        {
            var bytes = new List<byte>();
            foreach (var v in collection)
            {
                bytes.Add((byte)(v.Red * 255));
                bytes.Add((byte)(v.Green * 255));
                bytes.Add((byte)(v.Blue * 255));
                bytes.Add((byte)(v.Alpha * 255));
            }
            return bytes;
        }
    }
}
