﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.Elements;
using RevitServices.Transactions;

namespace Revit.AnalysisDisplay
{
    /// <summary>
    /// A Revit Vector Analysis Display 
    /// </summary>
    [RegisterForTrace]
    public class VectorAnalysisDisplay : AbstractAnalysisDisplay
    {

        #region Private constructors

        /// <summary>
        /// Create a Point Analysis Display in the current view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private VectorAnalysisDisplay(Autodesk.Revit.DB.View view, IEnumerable<Autodesk.Revit.DB.XYZ> sampleLocations,
            IEnumerable<Autodesk.Revit.DB.XYZ> samples)
        {

            var sfm = GetSpatialFieldManagerFromView(view);

            var sfmAndId = GetElementAndPrimitiveIdFromTrace();

            // we can rebind as we're dealing with the same view
            if (sfmAndId != null && sfmAndId.Item1.Id == sfm.Id)
            {
                InternalSetSpatialFieldManager(sfmAndId.Item1);
                InternalSetPrimitiveId(sfmAndId.Item2);
                InternalSetSpatialFieldValues(sampleLocations, samples);
                return;
            }

            // the input view has changed, remove the old primitive from the old view
            if (sfmAndId != null)
            {
                var oldSfm = sfmAndId.Item1;
                var oldId = sfmAndId.Item2;

                oldSfm.RemoveSpatialFieldPrimitive(oldId);
            }

            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalSetSpatialFieldManager(SpatialFieldManager);

            var primitiveId = SpatialFieldManager.AddSpatialFieldPrimitive();

            InternalSetPrimitiveId(primitiveId);
            InternalSetSpatialFieldValues(sampleLocations, samples);

            SetElementAndPrimitiveIdForTrace(SpatialFieldManager, primitiveId);

            TransactionManager.GetInstance().TransactionTaskDone();

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the spatial field values for the current spatial field primitive.  The two 
        /// input sequences should be of the same length.
        /// </summary>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private void InternalSetSpatialFieldValues(IEnumerable<XYZ> sampleLocations, IEnumerable<XYZ> samples)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // Convert the analysis values to a special Revit type
            var valList = samples.Select(n => new VectorAtPoint(new List<XYZ> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            // Convert the sample points to a special Revit Type
            var samplePts = new FieldDomainPointsByXYZ(sampleLocations.ToList<XYZ>());

            // Get the analysis results schema
            var schemaIndex = GetAnalysisResultSchemaIndex();

            // Update the values
            SpatialFieldManager.UpdateSpatialFieldPrimitive(SpatialFieldPrimitiveId, samplePts, sampleValues, schemaIndex);

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Show a colored Vector Analysis Display in the Revit view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="samplePoints"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static VectorAnalysisDisplay ByViewPointsAndVectorValues(AbstractView view,
                        Autodesk.DesignScript.Geometry.Point[] samplePoints, Autodesk.DesignScript.Geometry.Vector[] samples)
        {

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (samplePoints == null)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (samples == null)
            {
                throw new ArgumentNullException("samples");
            }

            if (samplePoints.Length != samples.Length)
            {
                throw new Exception("The number of sample points and number of samples must be the same");
            }

            return new VectorAnalysisDisplay(view.InternalView, samplePoints.ToXyzs(), samples.ToXyzs());
        }

        #endregion

    }

}
