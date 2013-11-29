using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace DSRevitNodes.GeometryObjects
{
    public class DSPlanarFace : IGeometryObject
    {

        internal Autodesk.Revit.DB.PlanarFace InternalPlanarFace
        {
            get; private set;
        }

        public void Tessellate(IRenderPackage package)
        {
            throw new NotImplementedException();
        }

    }
}
