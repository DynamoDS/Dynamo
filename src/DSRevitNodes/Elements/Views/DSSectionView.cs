using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit ViewSection
    /// </summary>
    [RegisterForTrace]
    public class DSSectionView : AbstractElement
    {

        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit element
        /// </summary>
        internal Autodesk.Revit.DB.ViewSection InternalViewSection
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewSection; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSSectionView(Autodesk.Revit.DB.ViewSection view)
        {
            InternalSetSectionView(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSSectionView( BoundingBoxXYZ bbox )
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            ViewSection vd = CreateSectionView(bbox);

            InternalSetSectionView(vd);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the InternalViewSection property and the associated element id and unique id
        /// </summary>
        /// <param name="floor"></param>
        private void InternalSetSectionView(Autodesk.Revit.DB.ViewSection floor)
        {
            this.InternalViewSection = floor;
            this.InternalElementId = floor.Id;
            this.InternalUniqueId = floor.UniqueId;
        }

        #endregion

        #region Private helper methods

        private static ViewSection CreateSectionView(BoundingBoxXYZ bbox)
        {
            // (sic) From the Dynamo legacy implementation
            var viewFam = DocumentManager.GetInstance().ElementsOfType<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);

            if (viewFam == null)
            {
                throw new Exception("There is no three dimensional view family in the document");
            }

            return ViewSection.CreateSection( Document, viewFam.Id, bbox);;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Floor given it's curve outline and Level
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="level"></param>
        /// <returns>The floor</returns>
        public static DSSectionView ByBoundingBox(DSBoundingBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return new DSSectionView(box.InternalBoundingBoxXyz);
        }
        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a View from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSSectionView FromExisting(Autodesk.Revit.DB.ViewSection view, bool isRevitOwned)
        {
            return new DSSectionView(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}

