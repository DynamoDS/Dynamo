using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// An abstract Revit View - All view types inherit from this type
    /// </summary>
    public abstract class AbstractView : AbstractElement
    {
        /// <summary>
        /// Obtain the reference Element as a View
        /// </summary>
        internal Autodesk.Revit.DB.View InternalView
        {
            get
            {
                return (Autodesk.Revit.DB.View) InternalElement;
            }
        }
    }
}
