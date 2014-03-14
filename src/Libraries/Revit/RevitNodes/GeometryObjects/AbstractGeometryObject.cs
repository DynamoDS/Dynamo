using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Revit.GeometryObjects
{
    //[SupressImportIntoVM]
    public abstract class AbstractGeometryObject : IGraphicItem
    {
        /// <summary>
        /// A reference to the internal Geometry object
        /// </summary>
        protected abstract Autodesk.Revit.DB.GeometryObject InternalGeometryObject
        {
            get;
        }

        /// <summary>
        /// Simple implementation of ToString - override for customized behavior
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return InternalGeometryObject.ToString();
        }

        public abstract void Tessellate(IRenderPackage package);
    }
}
