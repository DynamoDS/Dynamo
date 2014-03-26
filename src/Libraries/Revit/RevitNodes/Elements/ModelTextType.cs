using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ModelTextType
    /// </summary>
    public class ModelTextType : Element
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
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalModelTextType; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Construct from an existing Revit Element
        /// </summary>
        /// <param name="type"></param>
        private ModelTextType(Autodesk.Revit.DB.ModelTextType type)
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
        public static ModelTextType ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var type = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.ModelTextType>()
                .FirstOrDefault(x => x.Name == name);

            if (type == null)
            {
                throw new Exception(String.Format("There is no ModelTextType of the name, {0}, in the current Document", name));
            }

            return new ModelTextType(type)
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
        internal static ModelTextType FromExisting(Autodesk.Revit.DB.ModelTextType modelTextType, bool isRevitOwned)
        {
            return new ModelTextType(modelTextType)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
