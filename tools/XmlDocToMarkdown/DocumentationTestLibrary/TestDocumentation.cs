using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.DocumentationTestLibrary
{
    public class TestDocumentation
    {
        /// <summary>
        /// The length property
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// The breadth property
        /// </summary>
        public double Breadth { get; set; }

        /// <summary>
        /// The height property
        /// </summary>
        public double Height { get; set; }

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
            Breadth = bre;
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
        /// <returns></returns>
        public double GetVolume()
        {
            return Length * Breadth * Height;
        }
    }
}
