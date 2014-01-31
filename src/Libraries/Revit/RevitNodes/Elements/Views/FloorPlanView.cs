
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
    public class FloorPlanView : AbstractPlanView
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
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var vd = CreatePlanView(level, ViewFamily.FloorPlan);

            InternalSetPlanView(vd);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
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
        public static FloorPlanView FromExisting( Autodesk.Revit.DB.ViewPlan plan, bool isRevitOwned )
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

