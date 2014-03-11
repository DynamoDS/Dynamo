using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;

namespace Revit.GeometryObjects
{
    public class PlanarFace : Face
    {

        internal PlanarFace(Autodesk.Revit.DB.PlanarFace face) : base(face)
        {
        }

        internal Autodesk.Revit.DB.PlanarFace InternalPlanarFace
        {
            get; private set;
        }

        protected override GeometryObject InternalGeometryObject
        {
            get { return InternalPlanarFace; }
        }

    }
}
