using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Autodesk.DesignScript.Interfaces;

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
        #region private members

        private PointGeometry3D points;
        private LineGeometry3D lines;
        private MeshGeometry3D mesh;
        private bool hasData;

        #endregion

        #region constructors

        public HelixRenderPackage()
        {
            points = InitPointGeometry();
            lines = InitLineGeometry();
            mesh = InitMeshGeometry();

            LineStripVertexCounts = new List<int>();
            IsSelected = false;
        }

        #endregion

        #region IRenderPackage implementation

        public void Clear()
        {
            points = null;
            lines = null;
            mesh = null;
            
            LineStripVertexCounts.Clear();
            IsSelected = false;
        }

        /// <summary>
        /// Add a point vertex to the render package.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void PushPointVertex(double x, double y, double z)
        {
            points.Indices.Add(points.Positions.Count);
            points.Positions.Add(Vector3ForYUp(x,y,z));
        }

        /// <summary>
        /// Add a point color to the render package.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void PushPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            var ptColor = Color4FromBytes(red, green, blue, alpha);
            points.Colors.Add(ptColor);
        }

        /// <summary>
        /// Add a triangle vertex location to the render package.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void PushTriangleVertex(double x, double y, double z)
        {
            mesh.Indices.Add(mesh.Indices.Count);
            mesh.Positions.Add(Vector3ForYUp(x, y, z));
        }

        /// <summary>
        /// Add a triangle normal to the render package.
        /// Triangle normals are per-vertex.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void PushTriangleVertexNormal(double x, double y, double z)
        {
            mesh.Normals.Add(Vector3ForYUp(x,y,z));
        }

        /// <summary>
        /// Add a triangle texture coordinate to the render package.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public void PushTriangleVertexUV(double u, double v)
        {
            mesh.TextureCoordinates.Add(new Vector2((float)u, (float)v));
        }

        /// <summary>
        /// Add a triangle vertex color to the render package.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            mesh.Colors.Add(Color4FromBytes(red,green,blue,alpha));
        }

        /// <summary>
        /// Add a line vertex to the render package.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void PushLineStripVertex(double x, double y, double z)
        {
            lines.Positions.Add(Vector3ForYUp(x,y,z));
        }

        /// <summary>
        /// Add a line strip vertex count to the render package.
        /// </summary>
        /// <param name="n"></param>
        public void PushLineStripVertexCount(int n)
        {
            // The line strip vertex count is pushed after
            // the tessellated geometry is added to the package.
            // Here we add the indices to the line strip collection
            // which correspond to line segments for the Helix viewer.
            // Because lines are represented in Helix as a start and 
            // an end point, we duplicate every point except the first
            // and the last.

            LineStripVertexCounts.Add(n);

            var idxCount = lines.Indices.Any() ? lines.Indices.Last() + 1 : 0;

            for (var i = 0; i < n; ++i)
            {
                if (i != 0 && i != n - 1)
                {
                    lines.Indices.Add(idxCount);
                }

                lines.Indices.Add(idxCount);
                idxCount++;
            }
        }

        /// <summary>
        /// Add a line strip vertex color to the render package.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            lines.Colors.Add(Color4FromBytes(red,green,blue,alpha));
        }

        /// <summary>
        /// Get or set the line positions as a list of doubles.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<double> LineStripVertices
        {
            get { return lines.Positions.ToDoubles(); }
            set
            {
                if (value == null)
                    return;

                lines.Positions = null;
                lines.Positions = value.ToVector3Collection();
            }
        }

        /// <summary>
        /// Get or set the point positions as a list of doubles.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<double> PointVertices
        {
            get { return points.Positions.ToDoubles(); }
            set
            {
                if (value == null)
                    return;

                points.Positions = null;
                points.Positions = value.ToVector3Collection();
            }
        }

        /// <summary>
        /// Get or set the mesh positions as a list of doubles.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<double> TriangleVertices
        {
            get { return mesh.Positions.ToDoubles(); }
            set
            {
                if (value == null)
                    return;

                mesh.Positions = null;
                mesh.Positions = value.ToVector3Collection();
            }
        }

        /// <summary>
        /// Get or set the mesh normals as a list of doubles.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<double> TriangleNormals
        {
            get { return mesh.Normals.ToDoubles(); }
            set
            {
                if (value == null)
                    return;

                mesh.Normals = null;
                mesh.Normals = value.ToVector3Collection();
            }
        }

        /// <summary>
        /// Get or set the mesh texture coordinates as a list of doubles.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<double> TriangleUVs
        {
            get { return mesh.TextureCoordinates.ToDoubles(); }
            set
            {
                if (value == null)
                    return;

                mesh.TextureCoordinates = null;
                mesh.TextureCoordinates = value.ToVector2Collection();
            }
        }

        /// <summary>
        /// Get or set the point colors as a list of bytes.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<byte> PointVertexColors
        {
            get { return points.Colors.ToBytes(); }
            set
            {
                if (value == null)
                    return;

                points.Colors = null;
                points.Colors = value.ToColor4Collection();
            }
        }

        /// <summary>
        /// Get or set the line colors as a list of bytes.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<byte> LineStripVertexColors
        {
            get { return lines.Colors.ToBytes(); }
            set
            {
                if (value == null)
                    return;

                lines.Colors = null;
                lines.Colors = value.ToColor4Collection();
            }
        }

        /// <summary>
        /// Get or set the mesh colors as a list of bytes.
        /// Setting the positions in this way is expensive. It is recommded
        /// that you do not attempt to set individual positions using the
        /// setter, but that you set the entire collection at once.
        /// </summary>
        public List<byte> TriangleVertexColors
        {
            get { return mesh.Colors.ToBytes(); }
            set
            {
                if (value == null)
                    return;

                mesh.Colors = null;
                mesh.Colors = value.ToColor4Collection();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<int> LineStripVertexCounts { get; set; }

        /// <summary>
        /// A tag used to store information about the render package.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// A flag indicating whether the render package is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// A flag indicating whether the render package has data.
        /// </summary>
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

        /// <summary>
        /// A flag indicating whether the render package is displaying labels
        /// </summary>
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
                TextureCoordinates = new Vector2Collection()
            };

            return mesh;
        }

        internal static BillboardText3D InitText3D()
        {
            var text3D = new BillboardText3D();

            return text3D;
        }

        internal static string CleanTag(string tag)
        {
            var splits = tag.Split(':');
            return splits.Count() <= 1 ? "[0]" : string.Format("[{0}]",string.Join(",", splits.Skip(1)));
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

    internal static class HelixRenderExtensions
    {
        public static List<double> ToDoubles(this Vector2Collection collection)
        {
            var doubles = new List<double>();
            foreach (var v in collection)
            {
                doubles.Add(v.X);
                doubles.Add(v.Y);
            }

            return doubles;
        }

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

        public static Color4Collection ToColor4Collection(this List<byte> collection)
        {
            var colors = new Color4Collection();
            for(var i = 0; i<collection.Count; i+=4)
            {
                var a = collection[i]/255.0f;
                var b = collection[i + 1]/255.0f;
                var c = collection[i + 2]/255.0f;
                var d = collection[i + 3]/255.0f;
                var newColor = new Color4(a, b, c, d);
                colors.Add(newColor);
            }
            return colors;
        }

        public static Vector3Collection ToVector3Collection(this List<double> collection)
        {
            var vectors = new Vector3Collection();
            for (var i = 0; i < collection.Count; i += 3)
            {
                var a = collection[i];
                var b = collection[i + 1];
                var c = collection[i + 2];
                vectors.Add(new Vector3((float)a,(float)b,(float)c));
            }

            return vectors;
        }

        public static Vector2Collection ToVector2Collection(this List<double> collection)
        {
            var vectors = new Vector2Collection();
            for (var i = 0; i < collection.Count; i += 2)
            {
                var a = collection[i];
                var b = collection[i + 1];
                vectors.Add(new Vector2((float)a, (float)b));
            }

            return vectors;
        }
    }
}
