using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace DSRevitNodes
{
    /// <summary>
    /// Superclass of all Revit GeometryObjects
    /// </summary>
    [Browsable(false)]
    public abstract class AbstractGeometryObject
    {
        public static Document Document
        {
            get { return DocumentManager.GetInstance().CurrentDBDocument; }
        }
    }
}
