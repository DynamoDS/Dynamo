using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit Element of an unknown type.  This allows an arbitrary element
    /// to be passed around in the graph.
    /// </summary>
    public class DSElement : AbstractElement
    {
        /// <summary>
        /// A reference to the Revit Element
        /// </summary>
        private Autodesk.Revit.DB.Element _element;
        public override Element InternalElement
        {
            get { return _element; }
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="element"></param>
        private DSElement(Autodesk.Revit.DB.Element element)
        {
            this._element = element;
        }

        /// <summary>
        /// Wrap an element.  By default, this element is owned by Revit.  You must
        /// set this value manually if that's not what you want.
        /// </summary>
        /// <param name="element"></param>
        internal static DSElement FromExisting(Autodesk.Revit.DB.Element element)
        {
            return new DSElement(element);
        }
    }
}
