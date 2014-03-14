using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.References
{
    /// <summary>
    /// A base class for revit Reference objects
    /// </summary>
    //[SupressImportIntoVM]
    public abstract class AbstractReference
    {
        public static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        /// <summary>
        /// A stable reference to a Revit Element's geometry
        /// </summary>
        internal Autodesk.Revit.DB.Reference InternalReference
        {
            get;
            set;
        }
    }
}
