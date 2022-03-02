using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class stores the group styles added by the user
    /// </summary>
    public class StyleItem
    {
        /// This property will contain the Group Name of the stored style
        public string Name { get; set; }
        /// This property will contain the color in hexadecimal
        public string HexColorString { get; set; }
        /// <summary>
        /// This property describes if the Style is default (created by Dynamo automatically), those default styles will be always present and cannot be deleted
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
