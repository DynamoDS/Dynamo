using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.Elements;
using Revit.References;
using RevitServices.Transactions;

namespace Revit.AnalysisDisplay
{
    /// <summary>
    /// A Revit Point Analysis Display 
    /// </summary>
    [RegisterForTrace]
    public class FaceAnalysisDisplay : AbstractAnalysisDisplay
    {

        #region Private constructors

        /// <summary>
        /// Create a Point Analysis Display in the current view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="faceReference"></param>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private FaceAnalysisDisplay(Autodesk.Revit.DB.View view, Reference faceReference, IEnumerable<Autodesk.Revit.DB.UV> sampleLocations,
            IEnumerable<double> samples)
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

            // create a new spatial field primitive
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalSetSpatialFieldManager(sfm);

            var primitiveId = SpatialFieldManager.AddSpatialFieldPrimitive(faceReference);

            InternalSetPrimitiveId(primitiveId);
            InternalSetSpatialFieldValues(sampleLocations, samples);

            SetElementAndPrimitiveIdForTrace(SpatialFieldManager, primitiveId);

            TransactionManager.Instance.TransactionTaskDone();

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the spatial field values for the current spatial field primitive.  The two 
        /// input sequences should be of the same length.
        /// </summary>
        /// <param name="pointLocations"></param>
        /// <param name="values"></param>
        private void InternalSetSpatialFieldValues(IEnumerable<UV> pointLocations, IEnumerable<double> values)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            // Convert the analysis values to a special Revit type
            var valList = values.Select(n => new ValueAtPoint(new List<double> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            // Convert the sample points to a special Revit Type
            var samplePts = new FieldDomainPointsByUV(pointLocations.ToList<UV>());

            // Get the analysis results schema
            var schemaIndex = GetAnalysisResultSchemaIndex();

            // Update the values
            SpatialFieldManager.UpdateSpatialFieldPrimitive(SpatialFieldPrimitiveId, samplePts, sampleValues, schemaIndex);

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Show a colored Face Analysis Display in the Revit View
        /// </summary>
        /// <param name="view"></param>
        /// <param name="faceReference"></param>
        /// <param name="sampleUvPoints"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static FaceAnalysisDisplay ByViewFacePointsAndValues(AbstractView view, FaceReference faceReference,
                        double[][] sampleUvPoints, double[] samples)
        {

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (faceReference == null)
            {
                throw new ArgumentNullException("faceReference");
            }

            if (sampleUvPoints == null)
            {
                throw new ArgumentNullException("sampleUvPoints");
            }

            if (samples == null)
            {
                throw new ArgumentNullException("samples");
            }

            if (sampleUvPoints.Length != samples.Length)
            {
                throw new Exception("The number of sample points and number of samples must be the same");
            }

            return new FaceAnalysisDisplay(view.InternalView, faceReference.InternalReference, sampleUvPoints.ToUvs(), samples);
        }

        #endregion

    }

}
