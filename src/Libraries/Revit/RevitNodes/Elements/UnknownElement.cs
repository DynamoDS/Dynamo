using Autodesk.DesignScript.Runtime;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Element of an unknown type.  This allows an arbitrary element
    /// to be passed around in the graph.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class UnknownElement : Element
    {
        /// <summary>
        /// A reference to the Revit Element
        /// </summary>
        private Autodesk.Revit.DB.Element _element;
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return _element; }
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="element"></param>
        private UnknownElement(Autodesk.Revit.DB.Element element)
        {
            InternalSetElement(element);
        }

        private void InternalSetElement(Autodesk.Revit.DB.Element e)
        {
            _element = e;
            InternalElementId = _element.Id;
            InternalUniqueId = _element.UniqueId;
        }

        /// <summary>
        /// Wrap an element.  By default, this element is owned by Revit.  You must
        /// set this value manually if that's not what you want.
        /// </summary>
        /// <param name="element"></param>
        internal static UnknownElement FromExisting(Autodesk.Revit.DB.Element element)
        {
            return new UnknownElement(element);
        }

        public override string ToString()
        {
            return InternalElement.GetType().Name;
        }
    }
}
