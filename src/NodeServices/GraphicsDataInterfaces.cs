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
        /// A collection containing all mesh vertex indices.
        /// </summary>
        IEnumerable<int> PointIndices { get; }

        /// <summary>
        /// A collection of bytes representing RGBA colors. This field can be used to populate textures
        /// for mapping onto surfaces. Use the ColorsStride property to define the
        /// size of one dimension of the collection.
        /// </summary>
        IEnumerable<byte> Colors { get; }
 
        /// <summary>
        /// The size of one dimension of the Colors collection.
        /// </summary>
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
        void ApplyPointVertexColors(byte[] colors);

        /// <summary>
        /// Apply a color to a sequence of line vertices.
        /// </summary>
        void ApplyLineVertexColors(byte[] colors);

        /// <summary>
        /// Apply a color to each mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        void ApplyMeshVertexColors(byte[] colors);

        /// <summary>
        /// Set a an array of bytes to be used as a color map.
        /// </summary>
        /// <param name="colors"></param>
        void SetColors(byte[] colors);

        /// <summary>
        /// Clear all render data from the render package.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Represents a graphics item object, that can provide tesselated data
    /// into the given render package.
    /// </summary>
    public interface IGraphicItem
    {
        /// <summary>
        /// Gets the graphics/tesselation data in given render package object.
        /// </summary>
        /// <param name="package">The render package, where graphics data to be
        /// pushed.</param>
        /// <param name="parameters"></param>
        void Tessellate(IRenderPackage package, TessellationParameters parameters);
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

        public TessellationParameters()
        {
            Tolerance = -1;
            MaxTessellationDivisions = 512;
            ShowEdges = false;
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
        /// Gets the Graphics/Render data into the given render package.
        /// </summary>
        /// <param name="objects">Objects which owns some graphics items</param>
        /// <param name="package">RenderPackage where graphics/render data can
        /// be pushed/set.</param>
        /// <param name="parameters">A TessellationParameters object.</param>
        void Tessellate(List<Object> objects, IRenderPackage package, TessellationParameters parameters);
    }
}
