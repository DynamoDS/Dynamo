using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.DesignScript.Structure
{
    abstract public class StructureDS
    {
        public NodeDS[] Nodes { get; protected set; }
        public BarDS[] Bars { get; protected set; }

        /** Finding node by coordinates, if not found returns null. TODO: maybe add new bar to structure?
        *
        */
        protected NodeDS GetNodeByPoint(Point p)
        {
            foreach (NodeDS n in Nodes)
            {
                //TODO: can this be written to just compare pts? redefine vertex x as a origin rather that array
               if( n.BasePoint.DistanceTo(p)<0.0001)
                    return n;
            }
            return null;
        }


    }
}
