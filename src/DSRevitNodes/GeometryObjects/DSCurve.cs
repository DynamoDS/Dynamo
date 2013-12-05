using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryObjects;

namespace DSRevitNodes.GeometryObjects
{

    // this type should actually be removed, to be replaced by Autodesk.DesignScript.Geometry.Curve

    /// <summary>
    /// Class representing a Revit Curve
    /// </summary>
    public abstract class DSCurve : IGeometryObject
    {
        internal Autodesk.Revit.DB.Curve InternalCurve
        {
            get; set;
        }

        /// <summary>
        /// Tesselate the curve for visualization
        /// </summary>
        /// <param name="package"></param>
        void IGraphicItem.Tessellate(IRenderPackage package)
        {
            this.InternalCurve.Tessellate()
                .ToList()
                .ForEach(x => package.PushLineStripVertex(x.X, x.Y, x.Z));
        }

    }

}
