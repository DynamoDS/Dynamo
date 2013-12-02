using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading;
using DSNodeServices;
using DSRevitNodes.Elements;
using RevitServices.Persistence;

namespace DSRevitNodes
{
    [RegisterForTrace]
    public class DSMaterial : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// Internal reference to the Element
        /// </summary>
        internal Autodesk.Revit.DB.Material InternalMaterial
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalMaterial; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor for DSMaterial
        /// </summary>
        /// <param name="material"></param>
        private DSMaterial(Autodesk.Revit.DB.Material material)
        {
            InternalSetMaterial(material);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ELementId, and UniqueId
        /// </summary>
        /// <param name="material"></param>
        private void InternalSetMaterial(Autodesk.Revit.DB.Material material)
        {
            this.InternalMaterial = material;
            this.InternalElementId = material.Id;
            this.InternalUniqueId = material.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Select a material from the current document by the name
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <returns></returns>
        public static DSMaterial ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var mat = DocumentManager.GetInstance().ElementsOfType<Autodesk.Revit.DB.Material>()
                .FirstOrDefault(x => x.Name == name);

            if (mat == null)
            {
                throw new Exception("A Material with the given name does not exist in the current Document");
            }

            return new DSMaterial(mat)
            {
                IsRevitOwned = true
            };
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Wrap an element in the associated DS type
        /// </summary>
        /// <param name="material">The material</param>
        /// <returns></returns>
        internal static DSMaterial FromExisting(Autodesk.Revit.DB.Material material, bool isRevitOwned)
        {
            return new DSMaterial(material)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
