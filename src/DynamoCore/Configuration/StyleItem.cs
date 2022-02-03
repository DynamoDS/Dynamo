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
        /// This property will containt the Group Name of the stored style
        public string GroupName { get; set; }
        /// This property will containt the color in hexadecimal
        public string HexColorString { get; set; }
    }
}
