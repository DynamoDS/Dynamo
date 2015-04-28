using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

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
         //OPTIONAL:
         //IGraphicItem is an interface which allows your
         //class to participate in the rendering of geometry
         //to the background preview, and to Watch3D nodes.
         //You do not need to implement IGraphicItem unless
         //your node needs to draw geometry to the view.
        
        private Point point;

         //--------------------------------------------------
         //A NOTE ON XML COMMENTS:
         
         //Dynamo uses the comments you've put on your code to 
         //populate tooltips and help windows in the user 
         //interface. In order to enable this behavior, your 
         //project needs to be set to build xml 
         //documentation. To do this:
         //1. Right click on your project in the solution explorer.
         //2. Select Properties.
         //3. Select the Build tab. 
         //4. Check the XML Documentation box.
        
         //The generated xml file will be called the same
         //thing as your library, and needs to live along-side
         //your library to be picked up by the Dynamo loader.
         //--------------------------------------------------
        
        /// <summary>
        /// Properties marked as public will show up as 
        /// nodes in the Query section of the dynamo library.
        /// </summary>
        public double Awesome { get { return 42.0; } }

        /// <summary>
        /// The Point stored on the object.
        /// </summary>
        public Point Point { get { return point; } }
            
        /// <summary>
        /// Properties and methods marked as internal will not
        /// be visible in the Dynamo UI.
        /// </summary>
        internal double InvisibleProperty { get { return 42.0; } }

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
        /// Dynamo uses the pattern of static constructors.
        /// Don't forget to fill in the xml comments so that
        /// you will get help tips in the UI. You can also use
        /// default parameters, as we have here. With default
        /// parameters defined, you will not be required to attach
        /// any inputs to these ports in Dynamo.
        /// </summary>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        /// <param name="z">The z coordinate of the point.</param>
        /// <returns>A HelloDynamoZeroTouch object.</returns>
        public static HelloDynamoZeroTouch ByCoordinates(double x=42.0, double y=42.0, double z=42.0)
        {
            // Let's say in our example that the user is not allowed
            // to create an instance of this class if any of the 
            // coordinates is less than zero. We check the parameters
            // here because passing to the private constructor, and
            // we throw an error if the parameters do not conform.

            // These exceptions will be shown in the error bubble
            // over the node, and the node will turn yellow.

            if (x < 0)
            {
                throw new ArgumentException("x");
            }

            if (y < 0)
            {
                throw new ArgumentException("y");
            }

            if (z < 0)
            {
                throw new ArgumentException("z");
            }

            return new HelloDynamoZeroTouch(x, y, z);
        }

        /// <summary>
        /// The MultiReturn attribute can be used to specify
        /// the names of multiple output ports on a node that 
        /// returns a dictionary. The node must return a dictionary
        /// to be recognized as a multi-out node.
        /// </summary>
        /// <returns></returns>
        [MultiReturn(new[] { "thing 1", "thing 2" })]
        public static Dictionary<string, List<string>> MultiReturnExample()
        {
            return new Dictionary<string, List<string>>()
            {
                { "thing 1", new List<string>{"apple", "banana", "cat"} },
                { "thing 2", new List<string>{"Tywin", "Cersei", "Hodor"} }
            };
        }

        /// <summary>
        /// OPTIONAL:
        /// Overriding ToString allows you to control what is
        /// displayed whenever the object's string representation
        /// is used. For example, ToString is called when the 
        /// object is displayed in a Watch node.
        /// </summary>
        /// <returns>The string representation of our object.</returns>
        public override string ToString()
        {
            return string.Format("HelloDynamoZeroTouch:{0},{1},{2}", point.X, point.Y, point.Z);
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
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            // This example contains information to draw a point
            package.AddPointVertex(point.X, point.Y, point.Z);
            package.AddPointVertexColor(255, 0, 0, 255);
        }

        #endregion
    }

    /// <summary>
    /// By decorating a class with the 
    /// IsVisibleInDynamoLibrary attribute, and setting
    /// it to false, you are saying that you want this member
    /// to be available to the VM, but not be visible in the
    /// library view or search.
    ///
    /// By decorating a class with the SupressImportIntoVM
    /// attribute, you are saying that you do not want to import
    /// this class into Dynamo. BE CAREFUL! This class will then
    /// be unavailable to others that might reference it. In most
    /// cases, adding IsVisibleInDynamoLibrary(false) will suffice 
    /// to hide your method from view without needing to disable
    /// its import completely.
    /// </summary>
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public class DoesNotImportClass
    {
        /// <summary>
        /// DoesNotImportClass constructor.
        /// </summary>
        public DoesNotImportClass(){}
    }
}
