using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    /// <summary>
    /// Example implementation of IRenderPackageFactory.
    /// DefaultRenderPackageFactory produces DefaultRenderPackages.
    /// </summary>
    public class DefaultRenderPackageFactory : IRenderPackageFactory
    {
        /// <summary>
        /// Sets or Returns Tessellation parameters.
        /// MaxTessellationDivisions equals 32.
        /// </summary>
        public TessellationParameters TessellationParameters { get; set; }

        public DefaultRenderPackageFactory()
        {
            TessellationParameters = new TessellationParameters()
            {
                MaxTessellationDivisions = 32
            };
        }

        /// <summary>
        /// Creates DefaultRenderPackage.
        /// </summary>
        /// <returns>DefaultRenderPackage</returns>
        public IRenderPackage CreateRenderPackage()
        {
            return new DefaultRenderPackage();
        }
    }

    /// <summary>
    /// Example implementation of IRenderPackage.
    /// DefaultRenderPackage can be used as package for your visualization.
    /// </summary>
    public class DefaultRenderPackage : IRenderPackage
    {
        private List<double> pointVertices = new List<double>();
        private List<byte> pointColors = new List<byte>();
        private List<int> pointIndices = new List<int>();

        private List<double> lineVertices = new List<double>();
        private List<byte> lineColors = new List<byte>();
        private List<int> lineIndices = new List<int>();

        private List<double> meshVertices = new List<double>();
        private List<double> meshNormals = new List<double>();
        private List<double> meshTexCoordinates = new List<double>();
        private List<byte> meshColors = new List<byte>();
        private List<int> meshIndices = new List<int>();

        private List<int> lineStripVertexCounts = new List<int>();
        private byte[] colors;

        /// <summary>
        /// Add a point vertex to the render package.
        /// </summary>
        public void AddPointVertex(double x, double y, double z)
        {
            pointVertices.Add(x);
            pointVertices.Add(y);
            pointVertices.Add(z);
            pointIndices.Add(pointIndices.Count - 1);
        }

        /// <summary>
        /// Add a point color to the render package.
        /// </summary>
        public void AddPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            pointColors.Add(red);
            pointColors.Add(green);
            pointColors.Add(blue);
            pointColors.Add(alpha);
        }

        /// <summary>
        /// Add a triangle vertex location to the render package.
        /// </summary>
        public void AddTriangleVertex(double x, double y, double z)
        {
            meshVertices.Add(x);
            meshVertices.Add(y);
            meshVertices.Add(z);
            meshIndices.Add(meshVertices.Count - 1);
        }

        /// <summary>
        /// Add a triangle vertex normal to the render package.
        /// </summary>
        public void AddTriangleVertexNormal(double x, double y, double z)
        {
            meshNormals.Add(x);
            meshNormals.Add(y);
            meshNormals.Add(z);
        }

        /// <summary>
        /// Add a triangle texture coordinate to the render package.
        /// </summary>
        public void AddTriangleVertexUV(double u, double v)
        {
            meshTexCoordinates.Add(u);
            meshTexCoordinates.Add(v);
        }

        /// <summary>
        /// Add a triangle vertex color to the render package.
        /// </summary>
        public void AddTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            meshColors.Add(red);
            meshColors.Add(green);
            meshColors.Add(blue);
            meshColors.Add(alpha);
        }

        /// <summary>
        /// Add a line vertex to the render package.
        /// </summary>
        public void AddLineStripVertex(double x, double y, double z)
        {
            lineVertices.Add(x);
            lineVertices.Add(y);
            lineVertices.Add(z);
            lineIndices.Add(lineIndices.Count - 1);
        }

        /// <summary>
        /// Add a line strip vertex count to the render package.
        /// </summary>
        public void AddLineStripVertexCount(int n)
        {
            lineStripVertexCounts.Add(n);
        }

        /// <summary>
        /// Add a line strip vertex color to the render package.
        /// </summary>
        public void AddLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            lineColors.Add(red);
            lineColors.Add(green);
            lineColors.Add(blue);
            lineColors.Add(alpha);
        }

        /// <summary>
        /// Apply a color to each point vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void ApplyPointVertexColors(byte[] colors)
        {
            pointColors.Clear();
            pointColors.AddRange(colors);
        }

        /// <summary>
        /// Apply a color to a sequence of line vertices.
        /// </summary>
        public void ApplyLineVertexColors(byte[] colors)
        {
            lineColors.Clear();
            lineColors.AddRange(colors);
        }

        /// <summary>
        /// Apply a color to each mesh vertex.
        /// </summary>
        /// <param name="colors">A buffer of R,G,B,A values corresponding to each vertex.</param>
        public void ApplyMeshVertexColors(byte[] colors)
        {
            meshColors.Clear();
            meshColors.AddRange(colors);
        }

        /// <summary>
        /// Sets an array of bytes that is used as a color map.
        /// </summary>
        /// <param name="colors"></param>
        public void SetColors(byte[] colors)
        {
            this.colors = colors;
            Colors = this.colors;
        }
        
        /// <summary>
        /// Clear all render data from the render package.
        /// </summary>
        public void Clear()
        {
            pointVertices.Clear();
            pointColors.Clear();
            pointIndices.Clear();

            lineVertices.Clear();
            lineColors.Clear();
            lineIndices.Clear();

            meshVertices.Clear();
            meshNormals.Clear();
            meshTexCoordinates.Clear();
            meshColors.Clear();
            meshIndices.Clear();

            lineStripVertexCounts.Clear();

            Description = string.Empty;
            IsSelected = false;
            RequiresPerVertexColoration = false;
            DisplayLabels = false;
        }

        /// <summary>
        /// A tag used to store information about the render package.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns true if the render package is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Returns true if the render package has data.
        /// </summary>
        public bool HasRenderingData
        {
            get { return pointVertices.Any() || meshVertices.Any() || lineVertices.Any(); }
        }

        /// <summary>
        /// Returns true if the render package is displaying labels.
        /// </summary>
        public bool DisplayLabels { get; set; }

        /// <summary>
        /// Returns true if the render package requires per vertex coloration
        /// </summary>
        public bool RequiresPerVertexColoration { get; set; }

        /// <summary>
        /// Returns the number of point vertices in the package divided by 3.
        /// </summary>
        public int PointVertexCount
        {
            get { return pointVertices.Count / 3; }
        }

        /// <summary>
        /// Returns the number of line vertices in the package divided by 3.
        /// </summary>
        public int LineVertexCount
        {
            get { return lineVertices.Count / 3; }
        }

        /// <summary>
        /// Returns the number of mesh vertices in the package divided by 3.
        /// </summary>
        public int MeshVertexCount
        {
            get { return meshVertices.Count / 3; }
        }

        /// <summary>
        /// Returns the collection of int values representing how many vertices
        /// comprise each line segment in the package.
        /// </summary>
        public IEnumerable<int> LineStripVertexCounts
        {
            get { return lineStripVertexCounts; }
        }

        /// <summary>
        /// Returns the collection containing all line strip vertices as x1,y1,z1,x2,y2,z2...
        /// </summary>
        public IEnumerable<double> LineStripVertices
        {
            get { return lineVertices; }
        }

        /// <summary>
        /// Returns the collection containing all line strip colors as r1,g1,b1,a1,r2,g2,b2,a2...
        /// </summary>
        public IEnumerable<byte> LineStripVertexColors
        {
            get { return lineColors; }
        }

        /// <summary>
        /// Returns the collection containing all line strip indices.
        /// </summary>
        public IEnumerable<int> LineStripIndices
        {
            get { return lineIndices; }
        }

        /// <summary>
        /// Returns the collection containing all mesh vertices as x1,y1,z1,x2,y2,z2...
        /// </summary>
        public IEnumerable<double> MeshVertices
        {
            get { return meshVertices; }
        }

        /// <summary>
        /// Returns the collection containing all mesh vertex colors as r1,g1,b1,a1,r2,g2,b2,a2...
        /// </summary>
        public IEnumerable<byte> MeshVertexColors
        {
            get { return meshColors; }
        }

        /// <summary>
        /// Returns the collection containing all mesh vertex indices.
        /// </summary>
        public IEnumerable<int> MeshIndices
        {
            get { return meshIndices; }
        }

        /// <summary>
        /// Returns the collection containing all mesh normals as x1,y1,z1,x2,y2,z2...
        /// </summary>
        public IEnumerable<double> MeshNormals
        {
            get { return meshNormals; }
        }

        /// <summary>
        /// Returns the collection containing all mesh texture coordinates as u1,v1,u2,v2...
        /// </summary>
        public IEnumerable<double> MeshTextureCoordinates
        {
            get { return meshTexCoordinates; }
        }

        /// <summary>
        /// Returns the collection containing all point vertices as x1,y1,z1,x2,y2,z2...
        /// </summary>
        public IEnumerable<double> PointVertices
        {
            get { return pointVertices; }
        }

        /// <summary>
        /// Returns the collection containing all mesh vertex colors as r1,g1,b1,a1,r2,g2,b2,a2...
        /// </summary>
        public IEnumerable<byte> PointVertexColors
        {
            get { return pointColors; }
        }

        /// <summary>
        /// Returns the collection containing all mesh vertex indices.
        /// </summary>
        public IEnumerable<int> PointIndices
        {
            get { return pointIndices; }
        }

        /// <summary>
        /// Returns the collection of bytes representing RGBA colors. This field can be used to populate textures
        /// for mapping onto surfaces. Use the ColorsStride property to define the
        /// size of one dimension of the collection.
        /// </summary>
        public IEnumerable<byte> Colors { get; private set; }

        /// <summary>
        /// The size of one dimension of the Colors collection.
        /// </summary>
        public int ColorsStride { get; set; }
    }
}
