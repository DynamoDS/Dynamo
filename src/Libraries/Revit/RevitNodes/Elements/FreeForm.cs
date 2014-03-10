using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit FreeForm element
    /// </summary>
    [RegisterForTrace]
    public class FreeForm : AbstractElement
    {

        #region Internal properties

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal Autodesk.Revit.DB.FreeFormElement InternalFreeFormElement
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalFreeFormElement; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor that allows wrapping of an existing FreeForm element.
        /// </summary>
        /// <param name="ele"></param>
        private FreeForm(Autodesk.Revit.DB.FreeFormElement ele)
        {
            InternalSetFreeFormElement(ele);
        }

        /// <summary>
        /// Private constructor that constructs a FreeForm from a user-provided
        /// solid
        /// </summary>
        /// <param name="solid"></param>
        private FreeForm(Autodesk.Revit.DB.Solid mySolid)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var ele = 
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FreeFormElement>(Document);

            // mutate with new solid, if possible
            if (ele != null)
            {
                InternalSetFreeFormElement(ele);
                if (InternalSetSolid(mySolid))
                {
                    return;
                }
            }

            // recreate freeform
            TransactionManager.Instance.EnsureInTransaction(Document);

            var freeForm = FreeFormElement.Create(Document, mySolid);
            InternalSetFreeFormElement(freeForm);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Attempt to set the internal solid.  If this method fails, return false.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        private bool InternalSetSolid(Autodesk.Revit.DB.Solid solid)
        {
            var revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GenericForm));
            var FreeFormType = revitAPIAssembly.GetType("Autodesk.Revit.DB.FreeFormElement", true);

            var freeFormInstanceMethods = FreeFormType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var nameOfMethodUpdate = "UpdateToSolidGeometry";
            var nameOfMethodUpdateAlt = "UpdateSolidGeometry";

            var method = freeFormInstanceMethods.FirstOrDefault(
                x => x.Name == nameOfMethodUpdate || x.Name == nameOfMethodUpdateAlt);

            if (method != null)
            {
                TransactionManager.Instance.EnsureInTransaction(Document);

                method.Invoke(InternalFreeFormElement, new object[] {solid});

                TransactionManager.Instance.TransactionTaskDone();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the FreeFormElement and update it's id and unique id
        /// </summary>
        /// <param name="ele"></param>
        private void InternalSetFreeFormElement(Autodesk.Revit.DB.FreeFormElement ele)
        {
            this.InternalFreeFormElement = ele;
            this.InternalElementId = ele.Id;
            this.InternalUniqueId = ele.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Construct a FreeFrom element from a solid
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static FreeForm BySolid(Revit.GeometryObjects.Solid solid)
        {
            if (solid == null)
            {
                throw new ArgumentNullException("solid");
            }

            if (!Document.IsFamilyDocument)
            {
                throw new Exception("You can only create a FreeForm element in the Family editor.  You can then import" +
                                    "this family into a Project environment as a family.");
            }

            return new FreeForm(solid.InternalSolid);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct the Revit element by selection.
        /// </summary>
        /// <param name="freeFormElement"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static FreeForm FromExisting(Autodesk.Revit.DB.FreeFormElement freeFormElement, bool isRevitOwned)
        {
            return new FreeForm(freeFormElement)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}



