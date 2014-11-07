using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

namespace SampleLibraryZeroTouch
{
    /// <summary>
    /// The HelloDynamoZeroTouch class demonstrates
    /// how to create a class in a zero touch library
    /// which creates geometry, and exposes public 
    /// methods and properties as nodes.
    /// </summary>
    public class HelloDynamoZeroTouch : IGraphicItem
    {
        private Point point;

        /// --------------------------------------------------
        /// A NOTE ON XML COMMENTS:
        /// 
        /// Dynamo uses the comments you've put on your code to 
        /// populate tooltips and help windows in the user 
        /// interface. In order to enable this behavior, your 
        /// project needs to be set to build xml 
        /// documentation. To do this:
        /// 1. Right click on your project in the solution explorer.
        /// 2. Select Properties.
        /// 3. Select the Build tab. 
        /// 4. Check the XML Documentation box.
        ///
        /// The generated xml file will be called the same
        /// thing as your library, and needs to live along-side
        /// your library to be picked up by the Dynamo loader.
        /// --------------------------------------------------
        
        /// <summary>
        /// Properties marked as public will show up as 
        /// nodes in the Query section of the dynamo library.
        /// </summary>
        public double Awesome { get { return 42.0; } }

        /// <summary>
        /// The Point stored on the object.
        /// </summary>
        public Point Point { get { return Point; } }

        /// <summary>
        /// Private methods, such as this constructor,
        /// will not be visible in the Dynamo library.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        private HelloDynamoZeroTouch(double x, double y, double z)
        {
            point = Point.ByCoordinates(x, y, z);
        }

        /// <summary>
        /// Dynamo uses static constructors.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static HelloDynamoZeroTouch ByCoordinates(double x, double y, double z)
        {
            return new HelloDynamoZeroTouch(x, y, z);
        }

        #region IGraphicItem interface

        /// <summary>
        /// The Tessellate method in the IGraphicItem interface allows
        /// you to specify what is drawn when dynamo's visualization is
        /// updated.
        /// </summary>
        /// <param name="package">The IRenderPackage object into which you push your render data</param>
        /// <param name="tol">An optional tessellation tolerance which defines the resolution
        /// of generated meshes for surfaces.</param>
        /// <param name="maxGridLines">An optional tesselation tolerance which specifies the maximum number
        /// of surface subdivisions to be used for tesselation.</param>
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            // This example contains information to draw a point
            package.PushPointVertex(point.X, point.Y, point.Z);
            package.PushPointVertexColor(255, 0, 0, 255);
        }

        #endregion
    }
}
