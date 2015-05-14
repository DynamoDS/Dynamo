using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace SampleLibraryZeroTouch.Examples
{
    public class CustomRenderExample : IGraphicItem
    {
        private CustomRenderExample(){}

        /// <summary>
        /// Create an object which renders custom geometry.
        /// </summary>
        public static CustomRenderExample Create()
        {
            return new CustomRenderExample();
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            // Dynamo's renderer uses IRenderPackage objects
            // to store data for rendering. The Tessellate method
            // give you an IRenderPackage object which you can fill
            // with render data.

            // Set RequiresPerVertexColoration to let the renderer
            // know that you needs to use a per-vertex color shader.
            package.RequiresPerVertexColoration = true;

            AddColoredQuadToPackage(package);
            AddColoredLineToPackage(package);
        }

        private static void AddColoredQuadToPackage(IRenderPackage package)
        {
            // Triangle 1
            package.AddTriangleVertex(0, 0, 0);
            package.AddTriangleVertex(1, 0, 0);
            package.AddTriangleVertex(1, 1, 0);

            // For each vertex, add a color.
            package.AddTriangleVertexColor(255, 0, 0, 255);
            package.AddTriangleVertexColor(0, 255, 0, 255);
            package.AddTriangleVertexColor(0, 0, 255, 255);

            //Triangle 2
            package.AddTriangleVertex(0, 0, 0);
            package.AddTriangleVertex(1, 1, 0);
            package.AddTriangleVertex(0, 1, 0);
            package.AddTriangleVertexColor(255, 0, 0, 255);
            package.AddTriangleVertexColor(0, 255, 0, 255);
            package.AddTriangleVertexColor(0, 0, 255, 255);

            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);

            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
        }

        private static void AddColoredLineToPackage(IRenderPackage package)
        {
            package.AddLineStripVertex(0,0,0);
            package.AddLineStripVertex(5,5,5);

            package.AddLineStripVertexColor(255,0,0,255);
            package.AddLineStripVertexColor(255,0,0,255);

            // Specify line segments by adding a line vertex count.
            // Ex. The above line has two vertices, so we add a line
            // vertex count of 2. If we had tessellated a curve with n
            // vertices, we would add a line vertex count of n.
            package.AddLineStripVertexCount(2);
        }
    }
}
