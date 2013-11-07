using System;
using System.ComponentModel;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes
{
    /// <summary>
    /// Superclass of all geometry
    /// </summary>
    [Browsable(false)]
    public abstract class AbstractGeometry : IDisposable
    {
        public static Document Document
        {
            get { return DocumentManager.GetInstance().CurrentDBDocument; }
        }

        protected ElementId InternalID;
        protected string InternalUniqueId;

        /// <summary>
        /// Default implementation of dispose that removes the element from the
        /// document
        /// </summary>
        public virtual void Dispose()
        {
            DocumentManager.GetInstance().DeleteElement(this.InternalID);
        }
    }
}