using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.DesignScript.Structure
{

    abstract public class FixityDS
    {
        public bool FixUX { get; protected set; }
        public bool FixUY { get; protected set; }
        public bool FixUZ { get; protected set; }
        public bool FixRX { get; protected set; }
        public bool FixRY { get; protected set; }
        public bool FixRZ { get; protected set; }

        public string LabelName { get; protected set; }

        public FixityDS Copy(FixityDS fixity)
        {
            FixRX = fixity.FixRX;
            FixRY = fixity.FixRY;
            FixRZ = fixity.FixRZ;

            FixUX = fixity.FixUX;
            FixUY = fixity.FixUY;
            FixUZ = fixity.FixUZ;

            LabelName = fixity.LabelName;

            return this;
        }

    }
}
