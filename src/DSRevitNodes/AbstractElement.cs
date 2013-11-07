using System;
using System.ComponentModel;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes
{
    /// <summary>
    /// Superclass of all Revit element wrappers
    /// </summary>
    [Browsable(false)]
    public abstract class AbstractElement : IDisposable
    {
        public static Document Document
        {
            get { return DocumentManager.GetInstance().CurrentDBDocument; }
        }

        protected ElementId InternalId;
        protected string InternalUniqueId;

        /// <summary>
        /// Default implementation of dispose that removes the element from the
        /// document
        /// </summary>
        public virtual void Dispose()
        {
            DocumentManager.GetInstance().DeleteElement(this.InternalId);
        }
    }
}