using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements.Views
{
    /// <summary>
    /// A Revit ViewDrafting
    /// </summary>
    [RegisterForTrace]
    public class DraftingView : AbstractView
    {

        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit element
        /// </summary>
        internal Autodesk.Revit.DB.ViewDrafting InternalViewDrafting
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewDrafting; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private DraftingView(Autodesk.Revit.DB.ViewDrafting view)
        {
            InternalSetDraftingView(view);
        }
      
        /// <summary>
        /// Private constructor
        /// </summary>
        private DraftingView(string name)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var vd = Document.Create.NewViewDrafting();

            //rename the view
            if (!vd.Name.Equals(name))
                vd.Name = AbstractView3D.CreateUniqueViewName(name);

            InternalSetDraftingView(vd);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        #endregion

        #region Private mutators


        /// <summary>
        /// Set the InternalViewDrafting property and the associated element id and unique id
        /// </summary>
        /// <param name="floor"></param>
        private void InternalSetDraftingView(Autodesk.Revit.DB.ViewDrafting floor)
        {
            this.InternalViewDrafting = floor;
            this.InternalElementId = floor.Id;
            this.InternalUniqueId = floor.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit DraftingView given it's name
        /// </summary>
        /// <param name="name">Name of the view</param>
        /// <returns>The view</returns>
        public static DraftingView ByName( string name )
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return new DraftingView( name );
        }
        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a View from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DraftingView FromExisting(Autodesk.Revit.DB.ViewDrafting view, bool isRevitOwned)
        {
            return new DraftingView(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
