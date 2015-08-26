using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.DocumentationTestLibrary
{
    public class TestDocumentation
    {       
        /// <summary>
        /// The length
        /// </summary>
       
        private double length;
        /// <summary>
        /// The breadth
        /// </summary>
        
        private double width;
        /// <summary>
        /// The height
        /// </summary>
        private double height;

        /// <summary>
        /// The length property
        /// </summary>
        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// The breadth property
        /// </summary>
        public double Width
        {
            get { return width; }
            set { width = value; }
        }

        /// <summary>
        /// The height property
        /// </summary>
        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDocumentation"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor 
        /// </remarks>
        public TestDocumentation()
        {
            this.length = 0;
            this.width = 0;
            this.height = 0;            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDocumentation"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>       
        public TestDocumentation(double length, double width, double height)
        {
            this.length = length;
            this.width = width;
            this.height = height;            
        }

        /// <summary>
        /// Sets the length.
        /// </summary>
        /// <param name="len">The length.</param>
        public void SetLength(double len)
        {
            Length = len;
        }

        /// <summary>
        /// Sets the breadth.
        /// </summary>
        /// <param name="bre">The bre.</param>
        public void SetBreadth(double bre)
        {
            Width = bre;
        }

        /// <summary>
        /// Sets the height.
        /// </summary>
        /// <param name="hei">The hei.</param>
        public void SetHeight(double hei)
        {
            Height = hei;
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns>The volume </returns>
        /// <exception cref="System.Exception">Exception thrown when dimensions are not set </exception>
        public double GetVolume()
        {
            try
            {
                return Length * Width * Height;
            }
            catch (Exception)
            {
                return 0;
            }
            
        }

        /// <summary>
        /// Gets the type of box <see cref="TestDocumentation"/>
        /// </summary>
        /// <returns>The box type </returns>        
        public string GetBoxType()
        {
            if (Equals(length == width, height))
            {
                return "Rectangular Prism";
            }
            return "Cube";
        }
    }
}
