using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.DesignScript.Interfaces
{
    public class TessellationParameters2 : TessellationParameters
    {
        public TessellationParameters2() : base()
        {
            ScaleFactor = 1.0;
        } 
        /// <summary>
        /// The scale factor set in the workspace that must be applied to 
        /// distance and coordinate values used in rendering only ASM geometry.
        /// This scale factor is consumed only by LibG in its Tessellate method implementation.
        /// </summary>
        public double ScaleFactor { get; set; }

    }
}
