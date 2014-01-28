using System;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Revit.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
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
        /// Obtain all of the Parameters from an Element
        /// </summary>
        public Parameter[] Parameters
        {
            get
            {
                var parms = this.InternalElement.Parameters;
                return parms.Cast<Autodesk.Revit.DB.Parameter>().Select(x => new Parameter(x)).ToArray();
            }
        }

        /// <summary>
        /// Get the Name of the Element
        /// </summary>
        public string Name
        {
            get
            {
                return InternalElement.Name;
            }
        }

        /// <summary>
        /// Get an Axis-aligned BoundingBox of the Element
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(this.InternalElement.get_BoundingBox(null));
            }
        }

        /// <summary>
        /// A reference to the element
        /// </summary>
        [Browsable(false)]
        public abstract Autodesk.Revit.DB.Element InternalElement
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

        /// <summary>
        /// A basic implementation of ToString for Elements
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return InternalElement.ToString();
        }
    }
}