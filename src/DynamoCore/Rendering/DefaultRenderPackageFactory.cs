using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Interfaces;

using Dynamo.Interfaces;

namespace Dynamo
{
    public class DefaultRenderPackageFactory : IRenderPackageFactory
    {
        public int MaxTessellationDivisions { get; set; }

        public DefaultRenderPackageFactory()
        {
            MaxTessellationDivisions = 32;
        }

        public IRenderPackage CreateRenderPackage()
        {
            return new DefaultRenderPackage();
        }
    }

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
 
        public void AddPointVertex(double x, double y, double z)
        {
            pointVertices.Add(x);
            pointVertices.Add(y);
            pointVertices.Add(z);
            pointIndices.Add(pointIndices.Count - 1);
        }

        public void AddPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            pointColors.Add(red);
            pointColors.Add(green);
            pointColors.Add(blue);
            pointColors.Add(alpha);
        }

        public void AddTriangleVertex(double x, double y, double z)
        {
            meshVertices.Add(x);
            meshVertices.Add(y);
            meshVertices.Add(z);
            meshIndices.Add(meshVertices.Count - 1);
        }

        public void AddTriangleVertexNormal(double x, double y, double z)
        {
            meshNormals.Add(x);
            meshNormals.Add(y);
            meshNormals.Add(z);
        }

        public void AddTriangleVertexUV(double u, double v)
        {
            meshTexCoordinates.Add(u);
            meshTexCoordinates.Add(v);
        }

        public void AddTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            meshColors.Add(red);
            meshColors.Add(green);
            meshColors.Add(blue);
            meshColors.Add(alpha);
        }

        public void AddLineStripVertex(double x, double y, double z)
        {
            lineVertices.Add(x);
            lineVertices.Add(y);
            lineVertices.Add(z);
            lineIndices.Add(lineIndices.Count - 1);
        }

        public void AddLineStripVertexCount(int n)
        {
            lineStripVertexCounts.Add(n);
        }

        public void AddLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            lineColors.Add(red);
            lineColors.Add(green);
            lineColors.Add(blue);
            lineColors.Add(alpha);
        }

        public void ApplyPointVertexColors(byte[] colors)
        {
            pointColors.Clear();
            pointColors.AddRange(colors);
        }

        public void ApplyLineVertexColors(byte[] colors)
        {
            lineColors.Clear();
            lineColors.AddRange(colors);
        }

        public void ApplyMeshVertexColors(byte[] colors)
        {
            meshColors.Clear();
            meshColors.AddRange(colors);
        }

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

        public string Description { get; set; }

        public bool IsSelected { get; set; }

        public bool HasRenderingData
        {
            get { return pointVertices.Any() || meshVertices.Any() || lineVertices.Any(); }
        }

        public bool DisplayLabels { get; set; }

        public bool RequiresPerVertexColoration { get; set; }

        public int PointVertexCount
        {
            get { return pointVertices.Count / 3; }
        }

        public int LineVertexCount
        {
            get { return lineVertices.Count / 3; }
        }

        public int MeshVertexCount
        {
            get { return meshVertices.Count / 3; }
        }

        public IEnumerable<int> LineStripVertexCounts
        {
            get { return lineStripVertexCounts; }
        }

        public IEnumerable<double> LineStripVertices
        {
            get { return lineVertices; }
        }

        public IEnumerable<byte> LineStripVertexColors
        {
            get{ return lineColors; }
        }

        public IEnumerable<int> LineStripIndices
        {
            get { return lineIndices; }
        }

        public IEnumerable<double> MeshVertices
        {
            get { return meshVertices; }
        }

        public IEnumerable<byte> MeshVertexColors
        {
            get{ return meshColors; }
        }

        public IEnumerable<int> MeshIndices
        {
            get { return meshIndices; }
        }

        public IEnumerable<double> MeshNormals
        {
            get{ return meshNormals; }
        }

        public IEnumerable<double> MeshTextureCoordinates
        {
            get{ return meshTexCoordinates; }
        }

        public IEnumerable<double> PointVertices
        {
            get{ return pointVertices; }
        }

        public IEnumerable<byte> PointVertexColors
        {
            get{ return pointColors; }
        }

        public IEnumerable<int> PointIndices
        {
            get { return pointIndices; }
        }
    }
}
