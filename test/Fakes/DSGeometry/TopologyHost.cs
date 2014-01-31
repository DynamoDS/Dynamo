using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class TopologyEntity : DesignScriptEntity, ITopologyEntity
    {
        public IGeometryEntity Geometry
        {
            get { throw new NotImplementedException(); }
        }
    }
}
