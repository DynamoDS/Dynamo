using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;

namespace DSRevitNodes.GeometryObjects
{
    public class DSPlanarFace : AbstractGeometryObject
    {

        internal Autodesk.Revit.DB.PlanarFace InternalPlanarFace
        {
            get; private set;
        }

        protected override GeometryObject InternalGeometryObject
        {
            get { return InternalPlanarFace; }
        }

        public override void Tessellate(IRenderPackage package)
        {
            throw new NotImplementedException();
        }

    }
}
