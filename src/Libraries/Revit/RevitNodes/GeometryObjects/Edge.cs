using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;

namespace Revit.GeometryObjects
{

    public class Edge : GeometryObject
    {
        internal Autodesk.Revit.DB.Edge InternalEdge
        {
            get; private set;
        }

        private Edge(Autodesk.Revit.DB.Edge x)
        {
            this.InternalEdge = x;
        }

        protected override Autodesk.Revit.DB.GeometryObject InternalGeometryObject
        {
            get { return InternalEdge; }
        }

        internal static Edge FromExisting(Autodesk.Revit.DB.Edge f)
        {
            return new Edge(f);
        }

        /// <summary>
        /// Get the underlying curve representation of the Edge
        /// </summary>
        public Autodesk.DesignScript.Geometry.Curve Curve
        {
            get { return InternalEdge.AsCurve().ToProtoType(); }
        }

        #region Tesselation

        public override void Tessellate(IRenderPackage package, double tol, int gridLines)
        {
            InternalEdge.AsCurve().Tessellate()
                .ToList()
                .ForEach(x => package.PushLineStripVertex(x.X, x.Y, x.Z));
        }

        #endregion

    }
}
