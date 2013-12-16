using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.GeometryObjects;
using Nuclex.Game.Packing;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit ViewSheet
    /// </summary>
    [RegisterForTrace]
    public class DSSheet : AbstractElement
    {

        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit element
        /// </summary>
        internal Autodesk.Revit.DB.ViewSheet InternalViewSheet
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewSheet; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSSheet(Autodesk.Revit.DB.ViewSheet view)
        {
            InternalSetViewSheet(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSSheet(string nameOfSheet, string numberOfSheet, IEnumerable<View> views)
        {

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ViewSheet>(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetViewSheet(oldEle);
                InternalSetSheetName(nameOfSheet);
                InternalSetSheetNumber(numberOfSheet);
                InternalAddViewsToSheetView(views);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // create sheet without title block
            var sheet = Autodesk.Revit.DB.ViewSheet.Create(Document, ElementId.InvalidElementId);

            InternalSetViewSheet( sheet );
            InternalSetSheetName(nameOfSheet);
            InternalSetSheetNumber(numberOfSheet);
            InternalAddViewsToSheetView(views);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="titleBlockFamilySymbol"></param>
        /// <param name="views"></param>
        public DSSheet(string sheetName, string sheetNumber, FamilySymbol titleBlockFamilySymbol, IEnumerable<View> views)
        {

            //Phase 1 - Check to see if the object exists
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ViewSheet>(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetViewSheet(oldEle);
                InternalSetSheetName(sheetName);
                InternalSetSheetNumber(sheetNumber);
                InternalSetTitleBlock(titleBlockFamilySymbol.Id);
                InternalAddViewsToSheetView(views);

                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // create sheet with title block ID
            var sheet = Autodesk.Revit.DB.ViewSheet.Create(Document, titleBlockFamilySymbol.Id);

            InternalSetViewSheet(sheet);
            InternalSetSheetName(sheetName);
            InternalSetSheetNumber(sheetNumber);
            InternalAddViewsToSheetView(views);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// This method adds the collection of views to the existing ViewSheet and packs them 
        /// </summary>
        /// <param name="views"></param>
        private void InternalAddViewsToSheetView(IEnumerable<View> views)
        {
            var sheet = InternalViewSheet;

            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // (sic) from Dynamo Legacy
            var width = sheet.Outline.Max.U - sheet.Outline.Min.U;
            var height = sheet.Outline.Max.V - sheet.Outline.Min.V;
            var packer = new CygonRectanglePacker(width, height);

            foreach (var view in views)
            {

                var viewWidth = view.Outline.Max.U - view.Outline.Min.U;
                var viewHeight = view.Outline.Max.V - view.Outline.Min.V;

                UV placement = null;
                if (packer.TryPack(viewWidth, viewHeight, out placement))
                {
                    if (sheet.Views.Contains(view))
                    {
                        //move the view
                        //find the corresponding viewport
                        var enumerable =
                            DocumentManager.GetInstance().ElementsOfType<Autodesk.Revit.DB.Viewport>()
                                .Where(x => x.SheetId == sheet.Id && x.ViewId == view.Id).ToArray();

                        if (!enumerable.Any())
                            continue;

                        var viewport = enumerable.First();
                        viewport.SetBoxCenter(new XYZ(placement.U + viewWidth / 2, placement.V + viewHeight / 2, 0));
                    }
                    else
                    {
                        //place the view on the sheet
                        if (Viewport.CanAddViewToSheet(Document, sheet.Id, view.Id))
                        {
                            var viewport = Viewport.Create(Document, sheet.Id, view.Id,
                                                           new XYZ(placement.U + viewWidth / 2, placement.V + viewHeight / 2, 0));
                        }
                    }
                }
                else
                {

                    throw new Exception("View could not be packed on sheet.");
                }
            }

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        /// <summary>
        /// Set the InternalViewSheet property and the associated element id and unique id
        /// </summary>
        /// <param name="floor"></param>
        private void InternalSetViewSheet(Autodesk.Revit.DB.ViewSheet floor)
        {
            this.InternalViewSheet = floor;
            this.InternalElementId = floor.Id;
            this.InternalUniqueId = floor.UniqueId;
        }

        /// <summary>
        /// Set the name of the sheet
        /// </summary>
        /// <param name="name"></param>
        private void InternalSetSheetName(string name)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalViewSheet.Name = name;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        /// <summary>
        /// Set the sheet number of the sheet
        /// </summary>
        /// <param name="number"></param>
        private void InternalSetSheetNumber(string number)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalViewSheet.SheetNumber = number;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        /// <summary>
        /// Set the title block id for the view
        /// </summary>
        /// <param name="newTitleBlockId"></param>
        private void InternalSetTitleBlock(ElementId newTitleBlockId)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // element collector result
            new FilteredElementCollector(Document, InternalViewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks)
                .ToElements().ToArray().ForEach(x => x.ChangeTypeId(newTitleBlockId));

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Get the SheetName of the Sheet
        /// </summary>
        public string SheetName
        {
            get
            {
                return InternalViewSheet.Name;
            }
        }

        /// <summary>
        /// Get the SheetNumber of the Sheet
        /// </summary>
        public string SheetNumber
        {
            get
            {
                return InternalViewSheet.SheetNumber;
            }
        }

        /// <summary>
        /// Get the Views on a Sheet
        /// </summary>
        public AbstractView[] Views
        {
            get
            {
                return
                    InternalViewSheet.Views.Cast<Autodesk.Revit.DB.View>()
                        .ToList()
                        .Select(x => (AbstractView) ElementWrappingExtensions.ToDSType(x, true))
                        .ToArray();
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Sheet by the sheet name, number, and a collection of views.  This method will automatically
        /// pack the views onto the sheet.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="views"></param>
        /// <returns></returns>
        public static DSSheet ByNameNumberAndViews(string sheetName, string sheetNumber, AbstractView[] views)
        {
            if (sheetName == null)
            {
                throw new ArgumentNullException("sheetName");
            }

            if (sheetNumber == null)
            {
                throw new ArgumentNullException("sheetNumber");
            }

            return new DSSheet(sheetName, sheetNumber, views.Select(x => x.InternalView));
        }

        /// <summary>
        /// Create a Revit Sheet by the sheet name, number, and a view.  This method will automatically
        /// pack the views onto the sheet.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static DSSheet ByNameNumberAndView(string sheetName, string sheetNumber, AbstractView view)
        {
            return DSSheet.ByNameNumberAndViews(sheetName, sheetNumber, new[] { view });
        }

        /// <summary>
        /// Create a Revit Sheet by the sheet name, number, a title block FamilySymbol, and a collection of views.  This method will automatically
        /// pack the views onto the sheet. 
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="titleBlockFamilySymbol"></param>
        /// <param name="views"></param>
        /// <returns></returns>
        public static DSSheet ByNameNumberTitleBlockAndViews(string sheetName, string sheetNumber, DSFamilySymbol titleBlockFamilySymbol, AbstractView[] views)
        {
            if (sheetName == null)
            {
                throw new ArgumentNullException("sheetName");
            }

            if (sheetNumber == null)
            {
                throw new ArgumentNullException("sheetNumber");
            }

            if (titleBlockFamilySymbol == null)
            {
                throw new ArgumentNullException("titleBlockFamilySymbol");
            }

            return new DSSheet(sheetName, sheetNumber, titleBlockFamilySymbol.InternalFamilySymbol, views.Select(x => x.InternalView));
        }

        /// <summary>
        /// Create a Revit Sheet by the sheet name, number, a title block FamilySymbol, and a collection of views.  This method will automatically
        /// pack the view onto the sheet.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="titleBlockFamilySymbol"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static DSSheet ByNameNumberTitleBlockAndView(string sheetName, string sheetNumber, DSFamilySymbol titleBlockFamilySymbol, AbstractView view)
        {
            return DSSheet.ByNameNumberTitleBlockAndViews(sheetName, sheetNumber, titleBlockFamilySymbol, new[] { view });
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a View from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSSheet FromExisting(Autodesk.Revit.DB.ViewSheet view, bool isRevitOwned)
        {
            return new DSSheet(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
