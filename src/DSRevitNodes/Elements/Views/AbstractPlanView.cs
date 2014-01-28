using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.Elements
{
    /// <summary>
    /// Base class for Revit Plan views
    /// </summary>
    public class AbstractPlanView : AbstractView
    {

        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit element
        /// </summary>
        internal Autodesk.Revit.DB.ViewPlan InternalViewPlan
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewPlan; }
        }

        #endregion

        #region Protected mutators

        /// <summary>
        /// Set the InternalViewPlan property and the associated element id and unique id
        /// </summary>
        /// <param name="plan"></param>
        protected void InternalSetPlanView(Autodesk.Revit.DB.ViewPlan plan)
        {
            this.InternalViewPlan = plan;
            this.InternalElementId = plan.Id;
            this.InternalUniqueId = plan.UniqueId;
        }

        #endregion

        #region Protected helper methods

        protected static ViewPlan CreatePlanView(Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.ViewFamily planType)
        {
            var viewFam = DocumentManager.GetInstance().ElementsOfType<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == planType);

            if (viewFam == null)
            {
                throw new Exception("There is no such ViewFamily in the document");
            }

            return ViewPlan.Create(Document, viewFam.Id, level.Id); ;
        }

        #endregion

    }
}
