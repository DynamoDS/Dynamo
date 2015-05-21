using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;

namespace Dynamo.Wpf.Rendering
{
    /// <summary>
    /// A Helix-specific IRenderPackageFactory implementation.
    /// </summary>
    public class HelixRenderPackageFactory : IRenderPackageFactory
    {
        private const int MaxTessellationDivisionsDefault = 128;

        public int MaxTessellationDivisions { get; set; }

        public HelixRenderPackageFactory()
        {
            MaxTessellationDivisions = MaxTessellationDivisionsDefault;
        }

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
        private List<int> lineStripVertexCounts;

        #endregion

        #region constructors

        public HelixRenderPackage()
        {
            points = InitPointGeometry();
            lines = InitLineGeometry();
            mesh = InitMeshGeometry();
            lineStripVertexCounts = new List<int>();
        }

        #endregion

        #region IRenderPackage implementation

        public void Clear()
        {

            points = null;
            mesh = null;
            lines = null;

            points = InitPointGeometry();
            mesh = InitMeshGeometry();
            lines = InitLineGeometry();

            lineStripVertexCounts.Clear();

            IsSelected = false;
            DisplayLabels = false;
        }

        /// <summary>
        /// Add a point vertex to the render package.
        /// </summary>
        public void AddPointVertex(double x, double y, double z)
        {
            points.Indices.Add(points.Positions.Count);
            points.Positions.Add(Vector3ForYUp(x,y,z));
        }

        /// <summary>
        /// Add a point color to the render package.
        /// </summary>
        public void AddPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            var ptColor = Color4FromBytes(red, green, blue, alpha);
            points.Colors.Add(ptColor);
        }

        /// <summary>
        /// Add a triangle vertex location to the render package.
        /// </summary>
        public void AddTriangleVertex(double x, double y, double z)
        {
            mesh.Indices.Add(mesh.Indices.Count);
            mesh.Positions.Add(Vector3ForYUp(x, y, z));
        }

        /// <summary>
        /// Add a triangle normal to the render package.
        /// Triangle normals are per-vertex.
        /// </summary>
        public void AddTriangleVertexNormal(double x, double y, double z)
        {
            mesh.Normals.Add(Vector3ForYUp(x,y,z));
        }

        /// <summary>
        /// Add a triangle texture coordinate to the render package.
        /// </summary>
        public void AddTriangleVertexUV(double u, double v)
        {
            mesh.TextureCoordinates.Add(new Vector2((float)u, (float)v));
        }

        /// <summary>
        /// Add a triangle vertex color to the render package.
        /// </summary>
        public void AddTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            mesh.Colors.Add(Color4FromBytes(red,green,blue,alpha));
        }

        /// <summary>
        /// Add a line vertex to the render package.
        /// </summary>
        public void AddLineStripVertex(double x, double y, double z)
        {
            lines.Positions.Add(Vector3ForYUp(x,y,z));
        }

        /// <summary>
        /// Add a line strip vertex count to the render package.
        /// </summary>
        /// <param name="n"></param>
        public void AddLineStripVertexCount(int n)
        {
            // The line strip vertex count is pushed after
            // the tessellated geometry is added to the package.
            // Here we add the indices to the line strip collection
            // which correspond to line segments for the Helix viewer.
            // Because lines are represented in Helix as a start and 
            // an end point, we duplicate every point except the first
            // and the last.

            lineStripVertexCounts.Add(n);

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
        public void AddLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            lines.Colors.Add(Color4FromBytes(red,green,blue,alpha));
        }

        /// <summary>
        /// Apply a color to a sequence of point vertices.
        /// </summary>
        //public void ApplyPointVertexColors(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha)
        //{
        //    var message = string.Empty;
        //    if (!HasValidStartEnd(startIndex, endIndex, points, out message))
        //    {
        //        if (!string.IsNullOrEmpty(message))
        //        {
        //            throw new Exception(message);
        //        }

        //        return;
        //    }

        //    for (var i = startIndex; i <= endIndex; i++)
        //    {
        //        points.Colors[i] = Color4FromBytes(red, green, blue, alpha);
        //    }
        //}

        /// <summary>
        /// Apply a color to each point vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void ApplyPointVertexColors(byte[] colors)
        {
            if (colors.Count()/4 != points.Positions.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            points.Colors = null;
            points.Colors = colors.ToColor4Collection();
        }

        /// <summary>
        /// Apply a color to a sequence of line vertices.
        /// </summary>
        //public void ApplyLineVertexColors(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha)
        //{
        //    var message = string.Empty;
        //    if (!HasValidStartEnd(startIndex, endIndex, lines, out message))
        //    {
        //        if (!string.IsNullOrEmpty(message))
        //        {
        //            throw new Exception(message);
        //        }

        //        return;
        //    }

        //    for (var i = startIndex; i <= endIndex; i++)
        //    {
        //        lines.Colors[i] = Color4FromBytes(red, green, blue, alpha);
        //    }
        //}

        /// <summary>
        /// Apply a color to each line vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void ApplyLineVertexColors(byte[] colors)
        {
            if (colors.Count() / 4 != lines.Positions.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            lines.Colors = null;
            lines.Colors = colors.ToColor4Collection();
        }

        /// <summary>
        /// Apply a color to a sequence of mesh vertices.
        /// </summary>
        //public void ApplyMeshVertexColors(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha)
        //{
        //    var message = string.Empty;
        //    if (!HasValidStartEnd(startIndex, endIndex, mesh, out message))
        //    {
        //        if (!string.IsNullOrEmpty(message))
        //        {
        //            throw new Exception(message);
        //        }

        //        return;
        //    }

        //    for (var i = startIndex; i <= endIndex; i++)
        //    {
        //        mesh.Colors[i] = Color4FromBytes(red, green, blue, alpha);
        //    }
        //}

        /// <summary>
        /// Apply a color to each mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void ApplyMeshVertexColors(byte[] colors)
        {
            if (colors.Count() / 4 != mesh.Positions.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            mesh.Colors = null;
            mesh.Colors = colors.ToColor4Collection();
        }

        private bool HasValidStartEnd(int startIndex, int endIndex, Geometry3D geom, out string message)
        {
            message = string.Empty;

            if (startIndex > geom.Colors.Count ||
                endIndex > geom.Colors.Count - 1)
            {
                message = "The start and end indices must be within the bounds of the array.";
                return false;
            }

            if (endIndex < startIndex)
            {
                message = "The end index must be greater than the start index.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// A tag used to store information about the render package.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A flag indicating whether the render package is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// A flag indicating whether the render package has data.
        /// </summary>
        public bool HasRenderingData
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
        public bool DisplayLabels { get; set; }

        /// <summary>
        /// A flag indicating whether the render package requires 
        /// per vertex coloration.
        /// </summary>
        public bool RequiresPerVertexColoration { get; set; }

        /// <summary>
        /// The number of point vertices in the package.
        /// </summary>
        public int PointVertexCount
        {
            get { return points.Positions.Count; }
        }

        /// <summary>
        /// The number of line vertices in the package.
        /// </summary>
        public int LineVertexCount
        {
            get { return lines.Positions.Count; }
        }

        /// <summary>
        /// The number of mesh vertices in the package.
        /// </summary>
        public int MeshVertexCount
        {
            get { return mesh.Positions.Count; }
        }

        public IEnumerable<int> LineStripVertexCounts
        {
            get
            {
                return lineStripVertexCounts;
            }
        }

        public IEnumerable<double> LineStripVertices
        {
            get { return lines.Positions.ToEnumerable(); }
        }

        public IEnumerable<byte> LineStripVertexColors
        {
            get { return lines.Colors.ToEnumerable(); }
        }

        public IEnumerable<int> LineStripIndices
        {
            get { return lines.Indices.ToArray(); }
        }

        public IEnumerable<double> MeshVertices
        {
            get { return mesh.Positions.ToEnumerable(); }
        }

        public IEnumerable<byte> MeshVertexColors
        {
            get { return mesh.Colors.ToEnumerable(); }
        }

        public IEnumerable<int> MeshIndices
        {
            get { return mesh.Indices.ToArray(); }
        }

        public IEnumerable<double> MeshNormals
        {
            get { return mesh.Normals.ToEnumerable(); }
        }

        public IEnumerable<double> MeshTextureCoordinates
        {
            get { return mesh.TextureCoordinates.ToEnumerable(); }
        }

        public IEnumerable<double> PointVertices
        {
            get { return points.Positions.ToEnumerable(); }
        }

        public IEnumerable<byte> PointVertexColors
        {
            get { return points.Colors.ToEnumerable(); }
        }

        public IEnumerable<int> PointIndices
        {
            get { return points.Indices.ToArray(); }
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
        public static IEnumerable<double> ToEnumerable(this Vector2Collection collection)
        {
            foreach (var v in collection)
            {
                yield return v.X;
                yield return v.Y;
            }
        }

        public static IEnumerable<double> ToEnumerable(this Vector3Collection collection)
        {
            foreach (var v in collection)
            {
                yield return v.X;
                yield return v.Y;
                yield return v.Z;
            }
        }

        public static IEnumerable<byte> ToEnumerable(this Color4Collection collection)
        {
            foreach (var c in collection)
            {
                yield return (byte)(c.Red*255.0f);
                yield return (byte)(c.Green*255.0f);
                yield return (byte)(c.Blue*255.0f);
                yield return (byte)(c.Alpha*255.0f);
            }
        }

        public static Color4Collection ToColor4Collection(this byte[] collection)
        {
            var colors = new Color4Collection();
            for(var i = 0; i<collection.Count(); i+=4)
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
    }
}
