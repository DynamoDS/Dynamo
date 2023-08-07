using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using ITransformable = Autodesk.DesignScript.Interfaces.ITransformable;

namespace Dynamo.Wpf.Rendering
{
    /// <summary>
    /// A Helix-specific IRenderPackageFactory implementation.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class HelixRenderPackageFactory : IRenderPackageFactory
    {
        private const int MaxTessellationDivisionsDefault = 128;

        public TessellationParameters TessellationParameters { get; set; }

        public HelixRenderPackageFactory()
        {
            TessellationParameters = new TessellationParameters()
            {
                MaxTessellationDivisions = MaxTessellationDivisionsDefault
            };
        }

        public IRenderPackage CreateRenderPackage()
        {
            return new HelixRenderPackage();
        }
    }

    /// <summary>
    /// A Helix-specifc IRenderPackage implementation.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class HelixRenderPackage : IRenderPackage, ITransformable, IRenderLabels, IRenderPackageSupplement, IInstancingRenderPackage, IRenderInstancedLabels
    {
        #region private members

        private PointGeometry3D points;
        private LineGeometry3D lines;
        private MeshGeometry3D mesh;
        private bool hasData;
        private List<int> lineStripVertexCounts;
        private byte[] colors;
        private int colorStride;
        private List<byte[]> colorsList = new List<byte[]>();
        private List<int> colorStrideList = new List<int>();
        internal Dictionary<Guid, List<Matrix>> instanceTransforms = new Dictionary<Guid, List<Matrix>>();

        #endregion

        #region constructors

        public HelixRenderPackage()
        {
            points = InitPointGeometry();
            lines = InitLineGeometry();
            mesh = InitMeshGeometry();
            lineStripVertexCounts = new List<int>();
            Transform = System.Windows.Media.Media3D.Matrix3D.Identity.ToArray();
        }

        #endregion


        #region ITransformable implementation

        /// <summary>
        /// A flag indicating whether the render package has had its Transform property set
        /// explicitly.
        /// </summary>
        public bool RequiresCustomTransform { get; set; }
        /// <summary>
        /// A 4x4 matrix that is used to transform all geometry in the render packaage.
        /// </summary>
        public double[] Transform { get; private set; }

        /// <summary>
        /// Set the transform that is applied to all geometry in the renderPackage.
        /// </summary>
        /// <param name="transform"></param>
        [Obsolete("This method will be removed in 3.0. Use SetTransform(double[] matrix) instead.")]
        public void SetTransform(Autodesk.DesignScript.Geometry.CoordinateSystem transform)
        {
            var xaxis = transform.XAxis;
            var yaxis = transform.YAxis;
            var zaxis = transform.ZAxis;
            var org = transform.Origin;

            var csAsMat = new System.Windows.Media.Media3D.Matrix3D(xaxis.X, xaxis.Z, -xaxis.Y, 0,
                                                                    zaxis.X, zaxis.Z, -zaxis.Y, 0,
                                                                    -yaxis.X, -yaxis.Z, yaxis.Y, 0,
                                                                      org.X, org.Z, -org.Y, 1);


            this.Transform = csAsMat.ToArray();
        }


        /// <summary>
        /// Set the transform that is applied to all geometry in the renderPackage
        /// by computing the matrix that transforms between from and to.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        [Obsolete("This method will be removed in 3.0.")]
        public void SetTransform(Autodesk.DesignScript.Geometry.CoordinateSystem from, Autodesk.DesignScript.Geometry.CoordinateSystem to)
        {
            var inverse = from.Inverse();
            var final = inverse.PreMultiplyBy(to);

            this.SetTransform(final);
        }


        /// <summary>
        /// Set the transform that is applied to all geometry in the renderPackage,
        /// as this is a helix specific implementation it should be noted that the this matrix
        /// should be as follows when converting from a ProtoGeometry/Dynamo CoordinateSystem
        /// [ xaxis.X, xaxis.Z, -xaxis.Y, 0,
        /// zaxis.X, zaxis.Z, -zaxis.Y, 0,
        /// -yaxis.X, -yaxis.Z, yaxis.Y, 0,
        /// org.X, org.Z, -org.Y, 1 ]
        /// as Helix and Dynamo have their Y and Z axes reversed.
        /// </summary>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m13"></param>
        /// <param name="m14"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        /// <param name="m23"></param>
        /// <param name="m24"></param>
        /// <param name="m31"></param>
        /// <param name="m32"></param>
        /// <param name="m33"></param>
        /// <param name="m34"></param>
        /// <param name="m41"></param>
        /// <param name="m42"></param>
        /// <param name="m43"></param>
        /// <param name="m44"></param>
        public void SetTransform(double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44)
        {
            this.Transform = new System.Windows.Media.Media3D.Matrix3D(m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44).ToArray();
        }

        /// <summary>
        /// Set the transform as a double array, this transform is applied to all geometry in the renderPackage.
        /// This matrix should be laid out as follows in row vector order:
        /// [Xx,Xy,Xz, 0,
        ///  Yx, Yy, Yz, 0,
        ///  Zx, Zy, Zz, 0,
        ///  offsetX, offsetY, offsetZ, W]
        /// NOTE: This method should transform the matrix from row vector order to whatever form is needed by the implementation.
        /// When converting from ProtoGeometry CoordinateSystem form to input matrix, set the first row to the X axis of the CS,
        /// the second row to the Y axis of the CS, the third row to the Z axis of the CS, and the last row to the CS origin, where W = 1. 
        /// </summary>
        /// <param name="matrix"></param>
        public void SetTransform(double[] matrix)
        {
            this.Transform = new double[]
            {
                matrix[0],matrix[2],-matrix[1],matrix[3],
                matrix[8],matrix[10],-matrix[9],matrix[7],
                -matrix[4],-matrix[6],matrix[5],matrix[11],
                matrix[12],matrix[14],-matrix[13],matrix[15]
            };

        }
        #endregion

        #region IRenderPackage implementation

        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add mesh texture maps.")]
        public void SetColors(byte[] colors)
        {
            if (!AllowLegacyColorOperations)
            {
                throw new LegacyRenderPackageMethodException();
            }

            this.colors = colors;
        }

        public void Clear()
        {

            points = null;
            mesh = null;
            lines = null;

            points = InitPointGeometry();
            mesh = InitMeshGeometry();
            lines = InitLineGeometry();

            lineStripVertexCounts.Clear();

            Transform = System.Windows.Media.Media3D.Matrix3D.Identity.ToArray();

            IsSelected = false;
            DisplayLabels = false;

            colors = null;

            colorsList.Clear();
            colorStrideList.Clear();
            MeshVerticesRangesAssociatedWithTextureMaps.Clear();
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
        /// Apply a color to each point vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add ranges of vertex colors.")]
        public void ApplyPointVertexColors(byte[] colors)
        {
            if (!AllowLegacyColorOperations)
            {
                throw new LegacyRenderPackageMethodException();
            }

            if (colors.Count()/4 != points.Positions.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            points.Colors = null;
            points.Colors = colors.ToColor4Collection();
        }

        /// <summary>
        /// Apply a color to each line vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add ranges of vertex colors.")]
        public void ApplyLineVertexColors(byte[] colors)
        {
            if (!AllowLegacyColorOperations)
            {
                throw new LegacyRenderPackageMethodException();
            }

            if (colors.Count() / 4 != lines.Positions.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            lines.Colors = null;
            lines.Colors = colors.ToColor4Collection();
        }

        /// <summary>
        /// Apply a color to each mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add ranges of vertex colors.")]
        public void ApplyMeshVertexColors(byte[] colors)
        {
            if (!AllowLegacyColorOperations)
            {
                throw new LegacyRenderPackageMethodException();
            }

            if (colors.Count() / 4 != mesh.Positions.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            mesh.Colors = null;
            mesh.Colors = colors.ToColor4Collection();
        }

        private bool ValidateRange<T>(int startIndex, int endIndex, ICollection<T> geomDataCollection, out string message)
        {
            message = string.Empty;

            if (startIndex > geomDataCollection.Count ||
                endIndex > geomDataCollection.Count - 1)
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

        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add mesh texture maps.")]
        public IEnumerable<byte> Colors
        {
            get { return colors; }
        }

        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add mesh texture maps.")]
        public int ColorsStride
        {
            get => colorStride;
            set
            {
                if (!AllowLegacyColorOperations)
                {
                    throw new LegacyRenderPackageMethodException();
                }
                colorStride = value;
            }
        }

        #endregion

        #region IRenderLabels implementation

        ///// <summary>
        ///// Get label data; label string and associated position
        ///// </summary>
        public List<Tuple<string, float[]>> LabelData
        {
            get
            {
                var list = new List<Tuple<string, float[]>>();
                foreach (var tuple in LabelPlaces)
                {
                    list.Add(new Tuple<string, float[]>(tuple.Item1, tuple.Item2.ToArray()));
                }

                return list;
            }
        }

        /// <summary>
        /// Add a label position to the render package with position information from an existing geometry vertex.
        /// </summary>
        /// <param name="label">Text to be displayed in the label</param>
        /// <param name="vertexType">The type of vertex geometry used to look up a position: Point, Line, or Mesh</param>
        /// <param name="index">The index of the vertex geometry used to look up a position</param>
        public void AddLabel(string label, VertexType vertexType, int index)
        {
            Vector3 position;
            switch (vertexType)
            {
                case VertexType.Point:
                    if (index > points.Positions.Count) { return; }
                    position = points.Positions[index - 1];
                    break;
                case VertexType.Line:
                    if (index > lines.Positions.Count) { return; }
                    position = lines.Positions[index - 1];
                    break;
                case VertexType.Mesh:
                    if (index > mesh.Positions.Count) { return; }
                    position = Mesh.Positions[index - 1];
                    break;
                default:
                    return;
            }
            LabelPlaces.Add(new Tuple<string, Vector3>(label, position));
        }

        /// <summary>
        /// Adds a label to the render package, but first transforms the label by the transform matrix of the 
        /// relevant graphicItem.
        /// </summary>
        /// <param name="label">label</param>
        /// <param name="vertexType">type of vertex</param>
        /// <param name="vertIndex">vertex index for base label position</param>
        /// <param name="instanceIndex">index to use for transform matrix</param>
        /// <param name="BaseTessellationId">baseTessellation Id of the item this label belongs to.
        /// Aids in lookup of the correct transform matrix.</param>
        void IRenderInstancedLabels.AddInstancedLabel(string label, VertexType vertexType, int vertIndex, int instanceIndex, Guid BaseTessellationId)
        {
            Vector3 position = new Vector3();
            switch (vertexType)
            {
                case VertexType.MeshInstance:
                    //
                    if (vertIndex > mesh.Positions.Count) { return; }
                    position = Mesh.Positions[vertIndex - 1];
                    break;
                case VertexType.LineInstance:
                    //
                    if (vertIndex > Lines.Positions.Count) { return; }
                    position = Lines.Positions[vertIndex - 1];
                    break;
            }
            if (instanceTransforms.ContainsKey(BaseTessellationId) && instanceIndex <= instanceTransforms[BaseTessellationId].Count)
            {
                var transform = instanceTransforms[BaseTessellationId]?[instanceIndex - 1];
                var transformedLabelPos = transform?.ToMatrix3D().Transform(position.ToPoint3D()).ToVector3();
                if (transformedLabelPos.HasValue)
                {
                    LabelPlaces.Add(new Tuple<string, Vector3>(label, transformedLabelPos.Value));
                }
            }

        }
        /// <summary>
        /// Add a label position to the render package.
        /// </summary>
        /// <param name="label">Text to be displayed in the label</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void AddLabel(string label, double x, double y, double z)
        {
            LabelPlaces.Add(new Tuple<string, Vector3>(label, Vector3ForYUp(x, y, z)));
        }

        /// <summary>
        /// A flag indicating whether the render package should auto generate labels based on replication indices
        /// </summary>
        public bool AutoGenerateLabels { get; set; } = true;

        /// <summary>
        /// Clear all label data from the render package.
        /// </summary>
        public void ClearLabels()
        {
            LabelPlaces.Clear();
            AutoGenerateLabels = true;
        }

        #endregion

        #region IRenderPackageSupplement implementation

        /// <summary>
        /// The number of point vertices colors in the package (Optimized for speed).
        /// </summary>
        public int PointVertexColorCount => points.Colors.Count;

        /// <summary>
        /// The number of line vertices colors in the package (Optimized for speed).
        /// </summary>
        public int LineVertexColorCount => lines.Colors.Count;

        /// <summary>
        /// The number of mesh vertices colors in the package (Optimized for speed).
        /// </summary>
        public int MeshVertexColorCount => mesh.Colors.Count;

        /// <summary>
        /// Update a color to a range of point vertices.
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in PointVertices we want to associate with a color</param>
        /// <param name="endIndex">The index associated with the last vertex in PointVertices we want to associate with a color</param>
        /// <param name="red">byte for the red RGB value</param>
        /// <param name="green">byte for the green RGB value</param>
        /// <param name="blue">byte for the blue RGB value</param>
        /// <param name="alpha">byte for the alpha RGB value</param>
        public void UpdatePointVertexColorForRange(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha)
        {
            if (!ValidateRange(startIndex, endIndex, points.Colors, out var message))
            {
                throw new Exception(message);
            }

            for (var i = startIndex; i <= endIndex; i++)
            {
                points.Colors[i] = Color4FromBytes(red, green, blue, alpha);
            }
        }

        /// <summary>
        /// Append a color range for point vertices.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void AppendPointVertexColorRange(byte[] colors)
        {
            if (colors.Count() / 4 != points.Positions.Count - points.Colors.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            points.Colors.AddRange(colors.ToColor4Collection());
        }

        /// <summary>
        /// Update a color to a range of line vertices.
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in LineVertices we want to associate with a color</param>
        /// <param name="endIndex">The index associated with the last vertex in LineVertices we want to associate with a color</param>
        /// <param name="red">byte for the red RGB value</param>
        /// <param name="green">byte for the green RGB value</param>
        /// <param name="blue">byte for the blue RGB value</param>
        /// <param name="alpha">byte for the alpha RGB value</param>
        public void UpdateLineVertexColorForRange(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha)
        {
            if (!ValidateRange(startIndex, endIndex, lines.Colors, out var message))
            {
                throw new Exception(message);
            }

            for (var i = startIndex; i <= endIndex; i++)
            {
                lines.Colors[i] = Color4FromBytes(red, green, blue, alpha);
            }
        }

        /// <summary>
        /// Append a color range for line vertices.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void AppendLineVertexColorRange(byte[] colors)
        {
            if (colors.Count() / 4 != lines.Positions.Count - lines.Colors.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            lines.Colors.AddRange(colors.ToColor4Collection());
        }

        /// <summary>
        /// Update a color to a range of of mesh vertices.
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in MeshVertices we want to associate with a color</param>
        /// <param name="endIndex">The index associated with the last vertex in MeshVertices we want to associate with a color</param>
        /// <param name="red">byte for the red RGB value</param>
        /// <param name="green">byte for the green RGB value</param>
        /// <param name="blue">byte for the blue RGB value</param>
        /// <param name="alpha">byte for the alpha RGB value</param>
        public void UpdateMeshVertexColorForRange(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha)
        {
            if (!ValidateRange(startIndex, endIndex, mesh.Colors, out var message))
            {
                throw new Exception(message);
            }

            for (var i = startIndex; i <= endIndex; i++)
            {
                mesh.Colors[i] = Color4FromBytes(red, green, blue, alpha);
            }
        }

        /// <summary>
        /// Append a color range for mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void AppendMeshVertexColorRange(byte[] colors)
        {
            if (colors.Count() / 4 != mesh.Positions.Count- mesh.Colors.Count)
            {
                throw new Exception("The number of colors specified must be equal to the number of vertices.");
            }

            mesh.Colors.AddRange(colors.ToColor4Collection());
        }

        /// <summary>
        /// A List containing arrays of bytes representing RGBA colors.
        /// These arrays can be used to populate textures for mapping onto specific meshes
        /// </summary>
        public List<byte[]> TextureMapsList
        {
            get { return colorsList; }
        }

        /// <summary>
        /// A list containing the size of one dimension of the associated texture map array in TextureMapsList.
        /// </summary>
        public List<int> TextureMapsStrideList
        {
            get { return colorStrideList; }
        }

        /// <summary>
        /// A list of mesh vertices ranges that have associated texture maps
        /// </summary>
        public List<Tuple<int, int>> MeshVerticesRangesAssociatedWithTextureMaps { get; } = new List<Tuple<int, int>>();

        /// <summary>
        /// Set a color texture map for a specific range of mesh vertices
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in MeshVertices we want to associate with the texture map</param>
        /// <param name="endIndex">The index associated with the last vertex in MeshVertices we want to associate with the texture map</param>
        /// <param name="textureMap">An array of bytes representing RGBA colors to be used as a color texture map</param>
        /// <param name="stride">The size of one dimension of the colors array</param>
        public void AddTextureMapForMeshVerticesRange(int startIndex, int endIndex, byte[] textureMap, int stride)
        {
            if (!ValidateRange(startIndex, endIndex, mesh.Colors, out var message))
            {
                throw new Exception(message);
            }

            foreach (var r in MeshVerticesRangesAssociatedWithTextureMaps)
            {
                if (startIndex >= r.Item1 && startIndex <= r.Item2 || endIndex >= r.Item1 && endIndex <= r.Item2)
                {
                    throw new Exception("The start and end indices must not overlap previously defined ranges.");
                }
            }

            MeshVerticesRangesAssociatedWithTextureMaps.Add(new Tuple<int, int>(startIndex, endIndex));
            colorsList.Add(textureMap);
            colorStrideList.Add(stride);
        }

        /// <summary>
        /// Allow legacy usage of the color methods in IRenderPackage
        /// This flag is used by the UpdateRenderPackageAsyncTask implementation to flag
        /// any third party usage of deprecated color methods in IRenderPackageSupplement.MeshVerticesRangesAssociatedWithTextureMaps
        /// </summary>
        [Obsolete("Do not use! This will be removed in Dynamo 3.0")]
        public bool AllowLegacyColorOperations { get; set; } = true;

        #endregion

        /// <summary>
        /// A list of mesh vertices ranges that have associated instance references
        /// </summary>
        internal Dictionary<Guid, (int start, int end)> MeshVertexRangesAssociatedWithInstancing { get; } = new Dictionary<Guid, (int start, int end)>();

        /// <summary>
        /// Set an instance reference for a specific range of mesh vertices
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in MeshVertices we want to associate with the instance matrices></param>
        /// <param name="endIndex">The index associated with the last vertex in MeshVertices we want to associate with the instance matrices></param>
        /// <param name="id">A unique id associated with this tessellation geometry for instancing</param>
        public void AddInstanceGuidForMeshVertexRange(int startIndex, int endIndex, Guid id)
        {
            if (!ValidateRange(startIndex, endIndex, mesh.Positions, out var message))
            {
                throw new Exception(message);
            }

            if(MeshVertexRangesAssociatedWithInstancing.ContainsKey(id))
            {
                throw new Exception("The reference to the mesh range for this ID already exists.");
            }

            MeshVertexRangesAssociatedWithInstancing.Add(id, (startIndex, endIndex));
        }

        /// <summary>
        /// A list of line vertices ranges that have associated instance references
        /// </summary>
        internal Dictionary<Guid, (int start, int end)> LineVertexRangesAssociatedWithInstancing { get; } = new Dictionary<Guid, (int start, int end)>();

        /// <summary>
        /// Set an instance reference for a specific range of line vertices
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in LineVertices we want to associate with the instance matrices></param>
        /// <param name="endIndex">The index associated with the last vertex in LineVertices we want to associate with the instance matrices></param>
        /// <param name="id">A unique id associated with this tessellation geometry for instancing</param>
        public void AddInstanceGuidForLineVertexRange(int startIndex, int endIndex, Guid id)
        {
            if (!ValidateRange(startIndex, endIndex, lines.Positions, out var message))
            {
                throw new Exception(message);
            }

            if (LineVertexRangesAssociatedWithInstancing.ContainsKey(id))
            {
                throw new Exception("The reference to the line range for this ID already exists.");
            }

            LineVertexRangesAssociatedWithInstancing.Add(id, (startIndex, endIndex));
        }

        /// <summary>
        /// Set the transform using a series of doubles. The resulting transform is applied to all geometry in the renderPackage.
        /// Following conventional matrix notation, m11 is the value of the first row and first column, and m12
        /// is the value of the first row and second column.
        /// NOTE: This method will set the matrix exactly as described by the caller.
        /// </summary>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m13"></param>
        /// <param name="m14"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        /// <param name="m23"></param>
        /// <param name="m24"></param>
        /// <param name="m31"></param>
        /// <param name="m32"></param>
        /// <param name="m33"></param>
        /// <param name="m34"></param>
        /// <param name="m41"></param>
        /// <param name="m42"></param>
        /// <param name="m43"></param>
        /// <param name="m44"></param>
        /// <param name="id"></param>
        public void AddInstanceMatrix(float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44, Guid id)
        {
            if (!ContainsTessellationId(id))
            {
                throw new Exception("The reference to the graphics range(mesh or line) for this ID does not exists.");
            }

            var transform = new Matrix(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);

            List<Matrix> transforms;
            if (instanceTransforms.TryGetValue(id, out transforms))
            {
                transforms.Add(transform);
            }
            else
            {
                instanceTransforms.Add(id, new List<Matrix> {transform});
            }
        }

        /// <summary>
        /// Set the transform as a double array, this transform is applied to all geometry in the renderPackage.
        /// This matrix should be laid out as follows in row vector order:
        /// [Xx,Xy,Xz, 0,
        ///  Yx, Yy, Yz, 0,
        ///  Zx, Zy, Zz, 0,
        ///  offsetX, offsetY, offsetZ, W]
        /// NOTE: The caller of this method should transform the matrix from row vector order to whatever form is needed by the implementation.
        /// When converting from ProtoGeometry CoordinateSystem form to input matrix, set the first row to the X axis of the CS,
        /// the second row to the Y axis of the CS, the third row to the Z axis of the CS, and the last row to the CS origin, where W = 1. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="id"></param>
        public void AddInstanceMatrix(float[] matrix, Guid id)
        {
            if (!ContainsTessellationId(id))
            {
                throw new Exception("The reference to the graphics range(mesh or line) for this ID does not exists.");
            }

            var transform = new Matrix(matrix);

            List<Matrix> transforms;
            if (instanceTransforms.TryGetValue(id, out transforms))
            {
                transforms.Add(transform);
            }
            else
            {
                instanceTransforms.Add(id, new List<Matrix> { transform });
            }
        }

        /// <summary>
        /// Checks if a base tessellation guid has already been registered with this <see cref="IInstancingRenderPackage"/>.
        /// Both Line and Mesh ids are checked.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsTessellationId(Guid id)
        {
            return MeshVertexRangesAssociatedWithInstancing.ContainsKey(id) || LineVertexRangesAssociatedWithInstancing.ContainsKey(id);
        }

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

        public List<Tuple<string, Vector3>> LabelPlaces { get; } = new List<Tuple<string, Vector3>>();

        /// <summary>
        /// Number of instances for a particular baseTessellation type(cuboid, sphere etc)
        /// </summary>
        /// <param name="baseTessellationID"></param>
        /// <returns>returns -1 if id cannot be found in package.</returns>
        int IRenderInstancedLabels.InstanceCount(Guid baseTessellationID) {
            List<Matrix> res;
            if (instanceTransforms.TryGetValue(baseTessellationID, out res))
            {
                return res.Count;
            }
            else
                return -1;
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
            return splits.Count() <= 1 ? "[0]" : string.Format("[{0}]", string.Join(",", splits.Skip(1)));
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

        public static double[] ToArray(this System.Windows.Media.Media3D.Matrix3D mat)
        {
            if(mat == null)
            {
                throw new ArgumentNullException("matrix");
            }

            return new double[] {mat.M11,mat.M12,mat.M13,mat.M14,
                mat.M21,mat.M22,mat.M23,mat.M24,
                mat.M31,mat.M32,mat.M33,mat.M34,
                mat.OffsetX,mat.OffsetY,mat.OffsetZ,mat.M44,
            };
        }

        public static System.Windows.Media.Media3D.Matrix3D ToMatrix3D(this double[] matArray)
        {
            if (matArray == null || matArray.Count() < 16)
            {
                throw new ArgumentNullException("matArray");
            }

            return new System.Windows.Media.Media3D.Matrix3D(
                matArray[0], matArray[1], matArray[2], matArray[3],
                matArray[4], matArray[5], matArray[6], matArray[7],
                matArray[8], matArray[9], matArray[10], matArray[11],
                matArray[12], matArray[13], matArray[14], matArray[15]);
        }
    }
}
