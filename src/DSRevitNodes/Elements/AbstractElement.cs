using System;
using System.ComponentModel;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// Superclass of all Revit element wrappers
    /// </summary>
    [Browsable(false)]
    public abstract class AbstractElement : IDisposable
    {
        /// <summary>
        /// A reference to the current Document.
        /// </summary>
        public static Document Document
        {
            get { return DocumentManager.GetInstance().CurrentDBDocument; }
        }

        /// <summary>
        /// Indicates whether the element is owned by Revit or not.  If the element
        /// is Revit owned, it should not be deleted by Dispose().
        /// </summary>
        internal bool IsRevitOwned = false;

        /// <summary>
        /// A reference to the element
        /// </summary>
        internal abstract Element InternalElement
        {
            get;
        }

        /// <summary>
        /// The element id for this element
        /// </summary>
        protected ElementId InternalElementId;

        /// <summary>
        /// The unique id for this element
        /// </summary>
        protected string InternalUniqueId;

        /// <summary>
        /// Default implementation of dispose that removes the element from the
        /// document
        /// </summary>
        public virtual void Dispose()
        {
            // Do not delete Revit owned elements
            if (!IsRevitOwned)
            {
                DocumentManager.GetInstance().DeleteElement(this.InternalElementId);
            }
        }
    }
}