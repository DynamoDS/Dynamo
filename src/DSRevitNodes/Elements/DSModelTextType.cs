using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitServices.Persistence;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit ModelTextType
    /// </summary>
    public class DSModelTextType : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.ModelTextType InternalModelTextType
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalModelTextType; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Construct from an existing Revit Element
        /// </summary>
        /// <param name="type"></param>
        private DSModelTextType(Autodesk.Revit.DB.ModelTextType type)
        {
            InternalSetModelTextType(type);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="modelTextType"></param>
        private void InternalSetModelTextType(Autodesk.Revit.DB.ModelTextType modelTextType)
        {
            this.InternalModelTextType = modelTextType;
            this.InternalElementId = modelTextType.Id;
            this.InternalUniqueId = modelTextType.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Select a ModelTextType from the current document by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DSModelTextType ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var type = DocumentManager.GetInstance().ElementsOfType<Autodesk.Revit.DB.ModelTextType>()
                .FirstOrDefault(x => x.Name == name);

            if (type == null)
            {
                throw new Exception("There is no ModelTextType of the given name in the current Document");
            }

            return new DSModelTextType(type)
            {
                IsRevitOwned = true
            };
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create from an existing Revit element
        /// </summary>
        /// <param name="modelTextType"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        public static DSModelTextType FromExisting(Autodesk.Revit.DB.ModelTextType modelTextType, bool isRevitOwned)
        {
            return new DSModelTextType(modelTextType)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
