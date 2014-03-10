using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements.Views
{
    /// <summary>
    /// A Revit ViewSection
    /// </summary>
    [RegisterForTrace]
    public class SectionView : AbstractView
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
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewSection; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private SectionView(Autodesk.Revit.DB.ViewSection view)
        {
            InternalSetSectionView(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private SectionView( BoundingBoxXYZ bbox )
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            ViewSection vd = CreateSectionView(bbox);

            InternalSetSectionView(vd);

            TransactionManager.Instance.TransactionTaskDone();

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
            TransactionManager.Instance.EnsureInTransaction(Document);

            // (sic) From the Dynamo legacy implementation
            var viewFam = DocumentManager.Instance.ElementsOfType<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);

            if (viewFam == null)
            {
                throw new Exception("There is no three dimensional view family in the document");
            }

            var viewSection = ViewSection.CreateSection( Document, viewFam.Id, bbox);

            TransactionManager.Instance.TransactionTaskDone();

            return viewSection;

        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit ViewSection by a bounding box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static SectionView ByBoundingBox(BoundingBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return new SectionView(box.InternalBoundingBoxXyz);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a View from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static SectionView FromExisting(Autodesk.Revit.DB.ViewSection view, bool isRevitOwned)
        {
            return new SectionView(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}

