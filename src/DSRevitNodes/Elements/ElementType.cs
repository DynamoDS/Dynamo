using System;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ElementType
    /// </summary>
    /// http://revitapisearch.com.s3-website-us-east-1.amazonaws.com/html/b6fd8c08-7eea-1ab4-b7ab-096778b46e8f.htm
    public class ElementType : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// An internal reference to the ElementType
        /// </summary>
        internal Autodesk.Revit.DB.ElementType InternalElementType
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalElementType; }
        }

        #endregion

        /// <summary>
        /// Select a ElementType from the document given 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ElementType ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var elementType = DocumentManager.GetInstance()
                .ElementsOfType<Autodesk.Revit.DB.ElementType>()
                .FirstOrDefault(x => x.Name == name);

            if (elementType == null)
            {
                throw new Exception("A Revit ElementType with that name could not be located in the document.");
            }

            // until there is a way to create an ElementType from Dynamo, 
            // this object should never be cleaned up
            return FromExisting(elementType, true);
        }

        #region Private constructors

        /// <summary>
        /// Private constructor for the Element
        /// </summary>
        /// <param name="elementType"></param>
        private ElementType(Autodesk.Revit.DB.ElementType elementType)
        {
            InternalSetElementType(elementType);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the ElementType property, element id, and unique id
        /// </summary>
        /// <param name="elementType"></param>
        private void InternalSetElementType(Autodesk.Revit.DB.ElementType elementType)
        {
            this.InternalElementType = elementType;
            this.InternalElementId = elementType.Id;
            this.InternalUniqueId = elementType.UniqueId;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The name of the ElementType
        /// </summary>
        public string Name
        {
            get { return InternalElementType.Name; }
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a ElementType from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static ElementType FromExisting(Autodesk.Revit.DB.ElementType elementType, bool isRevitOwned)
        {
            return new ElementType(elementType)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}
