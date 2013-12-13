using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;

namespace DSRevitNodes.GeometryObjects
{

    public class DSEdge : AbstractGeometryObject
    {
        internal Autodesk.Revit.DB.Edge InternalEdge
        {
            get; private set;
        }



        internal DSEdge(Autodesk.Revit.DB.Edge x)
        {
            this.InternalEdge = x;
        }


        protected override GeometryObject InternalGeometryObject
        {
            get { return InternalEdge; }
        }

        #region Tesselation

        public override void Tessellate(IRenderPackage package)
        {
            InternalEdge.AsCurve().Tessellate()
                .ToList()
                .ForEach(x => package.PushLineStripVertex(x.X, x.Y, x.Z));
        }

        #endregion

    }
}
