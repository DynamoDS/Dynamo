using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace Revit.GeometryObjects
{
    //[SupressImportIntoVM]
    public abstract class GeometryObject : IGraphicItem
    {
        /// <summary>
        /// A reference to the internal GeometryObject
        /// </summary>
        protected abstract Autodesk.Revit.DB.GeometryObject InternalGeometryObject
        {
            get;
        }

        /// <summary>
        /// Simple implementation of ToString - override for customized behavior
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return GetType().FullName;
        }

        [IsVisibleInDynamoLibrary(false)]
        public abstract void Tessellate(IRenderPackage package, double tol, int gridLines);

        /// <summary>
        /// Obtain the geometric sub-components of this GeometryObject
        /// </summary>
        /// <returns></returns>
        public abstract object[] Explode();

    }
}
