using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Structure
{
    abstract public class BarDS
    {
        public double GammaAngle { get; set; }
        public virtual NodeDS StartNode { get; protected set; }
        public virtual NodeDS EndNode { get; protected set; }
        public SectionDS Section { get; set; }

        public Autodesk.DesignScript.Geometry.Edge Edge { get; set; } //Only available when creating bars from Topology
    }
}
