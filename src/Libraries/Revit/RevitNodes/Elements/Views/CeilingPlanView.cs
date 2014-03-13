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
    public class CeilingPlanView : AbstractPlanView
    {

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private CeilingPlanView(Autodesk.Revit.DB.ViewPlan view)
        {
            InternalSetPlanView(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private CeilingPlanView(Autodesk.Revit.DB.Level level)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var vd = CreatePlanView(level, ViewFamily.CeilingPlan);

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
        public static CeilingPlanView ByLevel(Level level)
        {
            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            return new CeilingPlanView(level.InternalLevel);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Create from existing Element
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        public static CeilingPlanView FromExisting(Autodesk.Revit.DB.ViewPlan plan, bool isRevitOwned)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            return new CeilingPlanView(plan)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
