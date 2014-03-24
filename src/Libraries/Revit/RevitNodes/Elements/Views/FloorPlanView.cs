
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements.Views
{
    /// <summary>
    /// A Revit ViewPlan
    /// </summary>
    [RegisterForTrace]
    public class FloorPlanView : PlanView
    {

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private FloorPlanView(Autodesk.Revit.DB.ViewPlan view)
        {
            InternalSetPlanView(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private FloorPlanView(Autodesk.Revit.DB.Level level)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var vd = CreatePlanView(level, ViewFamily.FloorPlan);

            InternalSetPlanView(vd);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Floor Plan at a given Level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static FloorPlanView ByLevel(Level level)
        {
            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            return new FloorPlanView( level.InternalLevel );
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Create from existing Element
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static FloorPlanView FromExisting( Autodesk.Revit.DB.ViewPlan plan, bool isRevitOwned )
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            return new FloorPlanView(plan)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}

