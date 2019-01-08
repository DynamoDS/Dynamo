using Autodesk.DesignScript.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Visualization
{
    /// <summary>
    /// The class that represents json data for drawing a graphic primitive 
    /// </summary>
    internal class DefaultGraphicPrimitives : IGraphicPrimitives
    {
        public DefaultGraphicPrimitives(IRenderPackage package)
        {
            TriangleTextureCoordinates = String.Empty;
            ColorsStride = String.Empty;
            Colors = String.Empty;

            if (package.Colors != null)
            {
                Colors = EncodeNumbers(package.Colors);
                TriangleTextureCoordinates = EncodeNumbers(package.MeshTextureCoordinates);
                ColorsStride = package.ColorsStride.ToString();
            }

            PointVertices = EncodeNumbers(package.PointVertices);
            PointVertexColors = EncodeNumbers(package.PointVertexColors);

            TriangleVertices = EncodeNumbers(package.MeshVertices);
            TriangleNormals = EncodeNumbers(package.MeshNormals);
            TriangleVertexColors = EncodeNumbers(package.MeshVertexColors);

            LineStripVertices = EncodeNumbers(package.LineStripVertices);
            LineStripCounts = EncodeNumbers(package.LineStripVertexCounts);
            LineStripColors = EncodeNumbers(package.LineStripVertexColors);

            RequiresPerVertexColoration = package.RequiresPerVertexColoration;
        }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string TriangleVertices { get; private set; }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string TriangleNormals { get; private set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        public string TriangleVertexColors { get; private set; }

        // Base-64 encoded array of 32 bit floats, 2 per vertex.
        public string TriangleTextureCoordinates { get; private set; }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string LineStripVertices { get; private set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per line strip, giving the number of vertices in the strip.
        public string LineStripCounts { get; private set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        public string LineStripColors { get; private set; }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string PointVertices { get; private set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        public string PointVertexColors { get; private set; }

        // Base-64 encoded array of 32 bit unsigned integers in RGBA format, definining a texture to apply to the triangles.
        public string Colors { get; private set; }

        // Number of values per row in the `Colors` array.
        public string ColorsStride { get; private set; }

        //  Whether or not the individual vertices should be colored using the data in the corresponding arrays.
        public bool RequiresPerVertexColoration { get; private set; }

        private static string EncodeNumbers<T>(IEnumerable<T> coordinates)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                if (typeof(T) == typeof(double))
                {
                    foreach (T value in coordinates)
                        writer.Write(Convert.ToSingle(value));
                }
                else
                {
                    foreach (T value in coordinates)
                        writer.Write(value as dynamic);
                }
            }

            return Convert.ToBase64String(stream.ToArray());
        }
    }
}
