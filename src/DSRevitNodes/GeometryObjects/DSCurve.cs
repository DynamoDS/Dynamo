using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryObjects;

namespace DSRevitNodes
{
    /// <summary>
    /// Class representing a Revit Curve
    /// </summary>
    public abstract class DSCurve
    {
        internal Autodesk.Revit.DB.Curve InternalCurve
        {
            get; set;
        }
    }

}
