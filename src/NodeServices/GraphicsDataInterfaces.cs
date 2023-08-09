using System;
using System.Collections.Generic;

namespace Autodesk.DesignScript.Interfaces
{
    /// <summary>
    /// This interface caches render specific data.
    /// </summary>
    public interface IRenderPackage
    {
        /// <summary>
        /// A tag used to store information about the render package.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// A flag indicating whether the render package is selected.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// A flag indicating whether the render package has data.
        /// </summary>
        bool HasRenderingData { get; }

        /// <summary>
        /// A flag indicating whether the render package is displaying labels
        /// </summary>
        bool DisplayLabels { get; set; }

        /// <summary>
        /// A flag indicating whether the render package requires 
        /// per vertex coloration.
        /// </summary>
        bool RequiresPerVertexColoration { get; set; }

        /// <summary>
        /// The number of point vertices in the package.
        /// </summary>
        int PointVertexCount { get; }

        /// <summary>
        /// The number of line vertices in the package.
        /// </summary>
        int LineVertexCount { get; }

        /// <summary>
        /// The number of mesh vertices in the package.
        /// </summary>
        int MeshVertexCount { get; }
        
        /// <summary>
        /// A collection of int values representing how many vertices
        /// comprise each line segment in the package.
        /// </summary>
        IEnumerable<int> LineStripVertexCounts { get; }

        /// <summary>
        /// A collection containing all line strip vertices as x1,y1,z1,x2,y2,z2...
        /// </summary>
        IEnumerable<double> LineStripVertices { get; }

        /// <summary>
        /// A collection containing all line strip colors as r1,g1,b1,a1,r2,g2,b2,a2...
        /// </summary>
        IEnumerable<byte> LineStripVertexColors { get; }

        /// <summary>
        /// A collection containing all line strip indices.
        /// </summary>
        IEnumerable<int> LineStripIndices { get; }

        /// <summary>
        /// A collection containing all mesh vertices as x1,y1,z1,x2,y2,z2...
        /// </summary>
        IEnumerable<double> MeshVertices { get; }

        /// <summary>
        /// A collection containing all mesh vertex colors as r1,g1,b1,a1,r2,g2,b2,a2...
        /// </summary>
        IEnumerable<byte> MeshVertexColors { get; }

        /// <summary>
        /// A collection containing all mesh vertex indices.
        /// </summary>
        IEnumerable<int> MeshIndices { get; }

        /// <summary>
        /// A collection containing all mesh normals as x1,y1,z1,x2,y2,z2...
        /// </summary>
        IEnumerable<double> MeshNormals { get; }

        /// <summary>
        /// A collection containing all mesh texture coordinates as u1,v1,u2,v2...
        /// </summary>
        IEnumerable<double> MeshTextureCoordinates { get; }

        /// <summary>
        /// A collection containing all point vertices as x1,y1,z1,x2,y2,z2...
        /// </summary>
        IEnumerable<double> PointVertices { get; }

        /// <summary>
        /// A collection containing all mesh vertex colors as r1,g1,b1,a1,r2,g2,b2,a2...
        /// </summary>
        IEnumerable<byte> PointVertexColors { get; }

        /// <summary>
        /// A collection containing all point vertex indices.
        /// </summary>
        IEnumerable<int> PointIndices { get; }

        /// <summary>
        /// A collection of bytes representing RGBA colors. This field can be used to populate textures
        /// for mapping onto surfaces. Use the ColorsStride property to define the
        /// size of one dimension of the collection.
        /// </summary>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add mesh texture maps.")]
        IEnumerable<byte> Colors { get; }

        /// <summary>
        /// The size of one dimension of the Colors collection.
        /// </summary>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add mesh texture maps.")]
        int ColorsStride { get; set; }
 
        /// <summary>
        /// Add a point vertex to the render package.
        /// </summary>
        void AddPointVertex(double x, double y, double z);

        /// <summary>
        /// Add a point color to the render package.
        /// </summary>
        void AddPointVertexColor(byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// Add a triangle vertex location to the render package.
        /// </summary>
        void AddTriangleVertex(double x, double y, double z);

        /// <summary>
        /// Add a triangle vertex normal to the render package.
        /// </summary>
        void AddTriangleVertexNormal(double x, double y, double z);

        /// <summary>
        /// Add a triangle texture coordinate to the render package.
        /// </summary>
        void AddTriangleVertexUV(double u, double v);

        /// <summary>
        /// Add a triangle vertex color to the render package.
        /// </summary>
        void AddTriangleVertexColor(byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// Add a line vertex to the render package.
        /// </summary>
        void AddLineStripVertex(double x, double y, double z);

        /// <summary>
        /// Add a line strip vertex count to the render package.
        /// </summary>
        void AddLineStripVertexCount(int n);

        /// <summary>
        /// Add a line strip vertex color to the render package.
        /// </summary>
        void AddLineStripVertexColor(byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// Apply a color to each point vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add ranges of vertex colors.")]
        void ApplyPointVertexColors(byte[] colors);

        /// <summary>
        /// Apply a color to a sequence of line vertices.
        /// </summary>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add ranges of vertex colors.")]
        void ApplyLineVertexColors(byte[] colors);

        /// <summary>
        /// Apply a color to each mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add ranges of vertex colors.")]
        void ApplyMeshVertexColors(byte[] colors);

        /// <summary>
        /// Set a an array of bytes to be used as a color map.
        /// </summary>
        /// <param name="colors"></param>
        [Obsolete("Do not use! Use the methods in IRenderPackageSupplement to add mesh texture maps.")]
        void SetColors(byte[] colors);

        /// <summary>
        /// Clear all render data from the render package.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// This interface provides additional methods adding for color information to a render package.
    /// </summary>
    public interface IRenderPackageSupplement
    {
        /// <summary>
        /// The number of point vertices colors in the package (Optimized for speed).
        /// </summary>
        int PointVertexColorCount { get; }

        /// <summary>
        /// The number of line vertices colors in the package (Optimized for speed).
        /// </summary>
        int LineVertexColorCount { get; }

        /// <summary>
        /// The number of mesh vertices colors in the package (Optimized for speed).
        /// </summary>
        int MeshVertexColorCount { get; }

        /// <summary>
        /// Update a color to a range of point vertices.
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in PointVertices we want to associate with a color</param>
        /// <param name="endIndex">The index associated with the last vertex in PointVertices we want to associate with a color</param>
        /// <param name="red">byte for the red RGB value</param>
        /// <param name="green">byte for the green RGB value</param>
        /// <param name="blue">byte for the blue RGB value</param>
        /// <param name="alpha">byte for the alpha RGB value</param>
        void UpdatePointVertexColorForRange(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha);
        
        /// <summary>
        /// Append a color range for point vertices.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        void AppendPointVertexColorRange(byte[] colors);

        /// <summary>
        /// Update a color to a range of line vertices.
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in LineVertices we want to associate with a color</param>
        /// <param name="endIndex">The index associated with the last vertex in LineVertices we want to associate with a color</param>
        /// <param name="red">byte for the red RGB value</param>
        /// <param name="green">byte for the green RGB value</param>
        /// <param name="blue">byte for the blue RGB value</param>
        /// <param name="alpha">byte for the alpha RGB value</param>
        void UpdateLineVertexColorForRange(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// Append a color range for line vertices.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        void AppendLineVertexColorRange(byte[] colors);

        /// <summary>
        /// Update a color to a range of of mesh vertices.
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in MeshVertices we want to associate with a color</param>
        /// <param name="endIndex">The index associated with the last vertex in MeshVertices we want to associate with a color</param>
        /// <param name="red">byte for the red RGB value</param>
        /// <param name="green">byte for the green RGB value</param>
        /// <param name="blue">byte for the blue RGB value</param>
        /// <param name="alpha">byte for the alpha RGB value</param>
        void UpdateMeshVertexColorForRange(int startIndex, int endIndex, byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// Append a color range for mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        void AppendMeshVertexColorRange(byte[] colors);

        /// <summary>
        /// A List containing arrays of bytes representing RGBA colors.
        /// These arrays can be used to populate textures for mapping onto specific meshes
        /// </summary>
        List<byte[]> TextureMapsList { get; }

        /// <summary>
        /// A list containing the size of one dimension of the associated texture map array in TextureMapsList.
        /// </summary>
        List<int> TextureMapsStrideList { get; }

        /// <summary>
        /// A list of mesh vertices ranges that have associated texture maps
        /// </summary>
        List<Tuple<int,int>> MeshVerticesRangesAssociatedWithTextureMaps { get; }

        /// <summary>
        /// Set a color texture map for a specific range of mesh vertices
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in MeshVertices we want to associate with the texture map</param>
        /// <param name="endIndex">The index associated with the last vertex in MeshVertices we want to associate with the texture map</param>
        /// <param name="textureMap">An array of bytes representing RGBA colors to be used as a color texture map</param>
        /// <param name="stride">The size of one dimension of the colors array</param>
        void AddTextureMapForMeshVerticesRange(int startIndex, int endIndex, byte[] textureMap, int stride);

        /// <summary>
        /// Allow legacy usage of the color methods in IRenderPackage
        /// This flag is used by the UpdateRenderPackageAsyncTask implementation to flag
        /// any third party usage of deprecated color methods in IRenderPackage API
        /// </summary>
        [Obsolete("Do not use! This will be removed in Dynamo 3.0")]
        bool AllowLegacyColorOperations { get; set; }
    }
    
    /// <summary>
    /// Represents labels and positions
    /// </summary>
    public interface IRenderLabels
    {
        ///// <summary>
        ///// Get label data; label string and associated position
        ///// </summary>
        List<Tuple<string, float[]>> LabelData { get; }

        /// <summary>
        /// Add a label position to the render package with position information from an existing geometry vertex.
        /// </summary>
        /// <param name="label">Text to be displayed in the label</param>
        /// <param name="vertexType">The type of vertex geometry used to look up a position: Point, Line, or Mesh</param>
        /// <param name="index">The index of the vertex geometry used to look up a position</param>
        void AddLabel(string label, VertexType vertexType, int index);

        /// <summary>
        /// Add a label position to the render package.
        /// </summary>
        /// <param name="label">Text to be displayed in the label</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void AddLabel(string label, double x, double y, double z);

        /// <summary>
        /// A flag indicating whether the render package should auto generate labels based on replication indices
        /// </summary>
        bool AutoGenerateLabels { get; set; }

        /// <summary>
        /// Clear all label data from the render package.
        /// </summary>
        void ClearLabels();
    }

    /// <summary>
    /// Internal interface to enable adding labels that are related to an instanceableGraphicItem.
    /// </summary>
    internal interface IRenderInstancedLabels
    {
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
        void AddInstancedLabel(string label, VertexType vertexType, int vertIndex, int instanceIndex, Guid BaseTessellationId);
        /// <summary>
        /// Number of instances for a particular baseTessellation type(cuboid, sphere etc)
        /// </summary>
        /// <param name="baseTessellationID"></param>
        /// <returns>returns -1 if id cannot be found in package.</returns>
        int InstanceCount(Guid baseTessellationID);
    };

    /// <summary>
    /// Represents instance matrices and references to tessellated geometry in the RenderPackage
    /// </summary>
    internal interface IInstancingRenderPackage
    {
        /// <summary>
        /// Checks if a base tessellation guid has already been registered with this <see cref="IInstancingRenderPackage"/>.
        /// Both Line and Mesh ids are checked.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ContainsTessellationId(Guid id);

        /// <summary>
        /// Set an instance reference for a specific range of mesh vertices
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in MeshVertices we want to associate with the instance matrices></param>
        /// <param name="endIndex">The index associated with the last vertex in MeshVertices we want to associate with the instance matrices></param>
        /// <param name="id">A unique id associated with this tessellation geometry for instancing</param>
        void AddInstanceGuidForMeshVertexRange(int startIndex, int endIndex, Guid id);

        /// <summary>
        /// Set an instance reference for a specific range of line vertices
        /// </summary>
        /// <param name="startIndex">The index associated with the first vertex in LineVertices we want to associate with the instance matrices></param>
        /// <param name="endIndex">The index associated with the last vertex in LineVertices we want to associate with the instance matrices></param>
        /// <param name="id">A unique id associated with this tessellation geometry for instancing</param>
        void AddInstanceGuidForLineVertexRange(int startIndex, int endIndex, Guid id);

        /// <summary>
        /// Set the transform using a series of floats. The resulting transform is applied to the range of geometry specified by the id.
        /// Following conventional matrix notation, m11 is the value of the first row and first column, and m12
        /// is the value of the first row and second column.
        /// NOTE: This method should set the matrix exactly as described by the caller.
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
        void AddInstanceMatrix(float m11, float m12, float m13, float m14,
           float m21, float m22, float m23, float m24,
           float m31, float m32, float m33, float m34,
           float m41, float m42, float m43, float m44, Guid id);

        /// <summary>
        /// Set the transform as a float array, The resulting transform is applied to the range of geometry specified by the id.
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
        /// <param name="id"></param>
        void AddInstanceMatrix(float[] matrix, Guid id);
    }

    public enum VertexType
    {
        Point,
        Line,
        Mesh,
        MeshInstance,
        LineInstance,
    }

    /// <summary>
    /// Represents a graphics item object, that can provide tesselated data
    /// into the given render package.
    /// </summary>
    public interface IGraphicItem
    {
        /// <summary>
        /// Returns the graphics/tesselation data in given render package object.
        /// </summary>
        /// <param name="package">The render package, where graphics data to be
        /// pushed.</param>
        /// <param name="parameters"></param>
        void Tessellate(IRenderPackage package, TessellationParameters parameters);
    }

    /// <summary>
    /// An interface that defines items which have a transform property, which is a 4x4 matrix.
    /// </summary>
    public interface ITransformable
    {
        /// <summary>
        /// A flag indicating whether the render package has had its Transform property set
        /// explicitly.
        /// </summary>
        bool RequiresCustomTransform { get; set; }

        /// <summary>
        /// A 4x4 matrix that is used to transform all geometry in the render packaage.
        /// </summary>
        double[] Transform  { get; } 
        
        /// <summary>
        /// Set the transform using a series of doubles. The resulting transform is applied to all geometry in the renderPackage.
        /// Following conventional matrix notation, m11 is the value of the first row and first column, and m12
        /// is the value of the first row and second column.
        /// NOTE: This method should set the matrix exactly as described by the caller.
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
        void SetTransform(double m11, double m12, double m13, double m14,
           double m21, double m22, double m23, double m24,
           double m31, double m32, double m33, double m34,
           double m41, double m42, double m43, double m44);

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
        void SetTransform(double[] matrix);
        
    }

    /// <summary>
    /// An interface that defines items whose graphics are defined by a single base tessellation and instance transforms defined by 4x4 transformation matrices.
    /// </summary>
    internal interface IInstanceableGraphicItem
    {
        /// <summary>
        /// A Guid used to reference the base tessellation geometry that will be transformed for all related instances
        /// </summary>
        Guid BaseTessellationGuid { get; }

        /// <summary>
        /// A flag used to indicate if the current geometrical configuration of an item has instance information.
        /// </summary>
        bool InstanceInfoAvailable { get; }

        /// <summary>
        /// Adds the base graphics/tesselation data in given render package object.
        /// </summary>
        /// <param name="package">The render package, where base tessellation will be
        /// pushed.</param>
        /// <param name="parameters">tessellation parameters for the instance. Can be used to generate the
        /// correct shared base tessellation</param>
        void AddBaseTessellation(IInstancingRenderPackage package, TessellationParameters parameters);

        /// <summary>
        /// Adds an instance matrix for this geometry.
        /// </summary>
        /// <param name="package">The render package, where instance will be
        /// pushed.</param>
        /// <param name="parameters">tessellation parameters for the instance, only scale factor is generally applicable.</param>
        /// <param name="labelKey">the strig label key that specifices which result this instance represents in a node's output.</param>
        void AddInstance(IInstancingRenderPackage package, TessellationParameters parameters, string labelKey);
    }


    public class TessellationParameters
    {
        /// <summary>
        /// The tolerance for faceting.
        /// Default is -1.
        /// </summary>
        public double Tolerance { get; set; }

        /// <summary>
        /// The maximum number of divisions for tessellation.
        /// Default is 512
        /// </summary>
        public int MaxTessellationDivisions { get; set; }

        /// <summary>
        /// A flag indicating whether surface edges should be
        /// included in the RenderPackage. Default is false.
        /// </summary>
        public bool ShowEdges { get; set; }

        /// <summary>
        /// The scale factor set in the workspace that must be applied to 
        /// distance and coordinate values used in rendering only ASM geometry.
        /// This scale factor is consumed only by LibG in its Tessellate method implementation.
        /// </summary>
        internal double ScaleFactor { get; set; }

        public TessellationParameters()
        {
            Tolerance = -1;
            MaxTessellationDivisions = 512;
            ShowEdges = false;
            ScaleFactor = 1.0;
        }
    }

    /// <summary>
    /// This interface provides graphics data into the RenderPackage interface 
    /// for given set of objects.
    /// </summary>
    public interface IGraphicDataProvider
    {
        /// <summary>
        /// Returns a list of IGraphicItem owned by the given object.
        /// </summary>
        /// <param name="obj">The object for which graphics items are queried.
        /// </param>
        /// <returns>List of IGraphicItem owned by the input object.</returns>
        List<IGraphicItem> GetGraphicItems(Object obj);

        /// <summary>
        /// Returns the Graphics/Render data into the given render package.
        /// </summary>
        /// <param name="objects">Objects which owns some graphics items</param>
        /// <param name="package">RenderPackage where graphics/render data can
        /// be pushed/set.</param>
        /// <param name="parameters">A TessellationParameters object.</param>
        void Tessellate(List<Object> objects, IRenderPackage package, TessellationParameters parameters);
    }

    /// <summary>
    /// The interface that represents json data for drawing a graphic primitive 
    /// </summary>
    internal interface IGraphicPrimitives
    {
        /// <summary>
        /// Base-64 encoded array of 32 bit floats, 3 per vertex.
        /// </summary>
        string TriangleVertices { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit floats, 3 per vertex.
        /// </summary>
        string TriangleNormals { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        /// </summary>
        string TriangleVertexColors { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit floats, 2 per vertex.
        /// </summary>
        string TriangleTextureCoordinates { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit floats, 3 per vertex.
        /// </summary>
        string LineStripVertices { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit unsigned integers, 1 per line strip, giving the number of vertices in the strip.
        /// </summary>
        string LineStripCounts { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        /// </summary>
        string LineStripColors { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit floats, 3 per vertex.
        /// </summary>
        string PointVertices { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        /// </summary>
        string PointVertexColors { get; }

        /// <summary>
        /// Base-64 encoded array of 32 bit unsigned integers in RGBA format, definining a texture to apply to the triangles.
        /// </summary>
        string Colors { get; }

        /// <summary>
        /// Number of values per row in the `Colors` array.
        /// </summary>
        string ColorsStride { get; }

        /// <summary>
        ///  Whether or not the individual vertices should be colored using the data in the corresponding arrays.
        /// </summary>
        bool RequiresPerVertexColoration { get; }
    }

    /// <summary>
    /// Exception used to catch usage of Legacy IRenderPackage color APIs
    /// </summary>
    public class LegacyRenderPackageMethodException : Exception { }
}
