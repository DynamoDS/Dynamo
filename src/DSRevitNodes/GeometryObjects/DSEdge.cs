using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace DSRevitNodes.GeometryObjects
{

    public class DSEdge : IGeometryObject
    {
        internal Autodesk.Revit.DB.Edge InternalEdge
        {
            get; private set;
        }

        public DSEdge(Autodesk.Revit.DB.Edge x)
        {
            this.InternalEdge = x;
        }

        #region Tesselation

        public void Tessellate(IRenderPackage package)
        {
            InternalEdge.AsCurve().Tessellate()
                .ToList()
                .ForEach(x => package.PushLineStripVertex(x.X, x.Y, x.Z));
        }

        #endregion

    }
}
