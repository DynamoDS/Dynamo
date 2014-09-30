using System;
using System.Collections.Generic;
using System.Linq;

using Analysis;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using Revit.GeometryConversion;
using Revit.GeometryReferences;
using RevitServices.Transactions;
using Reference = Autodesk.Revit.DB.Reference;
using View = Revit.Elements.Views.View;

namespace Revit.AnalysisDisplay
{
    /// <summary>
    /// A Revit Point Analysis Display 
    /// </summary>
    [DSNodeServices.RegisterForTrace]
    public class FaceAnalysisDisplay : AbstractAnalysisDisplay
    {
        internal const string DefaultTag = "RevitFaceReference";

        #region Private constructors

        /// <summary>
        /// Create a Point Analysis Display in the current view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="faceReference"></param>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private FaceAnalysisDisplay(Autodesk.Revit.DB.View view, Reference faceReference, IEnumerable<UV> sampleLocations,
            IEnumerable<double> samples)
        {
            // We wrap the incoming sample collection in another list.
            // We don't want to break original functionality, but
            // we now want to support multiple values at a location.

            var samplesSets = samples.Select(sample => new List<double>() { sample }).ToList();

            ConstructCore(view, faceReference, sampleLocations, samplesSets);
        }

        private FaceAnalysisDisplay(
            Autodesk.Revit.DB.View view, ISurfaceAnalysisData data)
        {
            var faceReference = data.Surface.Tags.LookupTag(DefaultTag) as Reference;

            ConstructCore(view, faceReference, data.Locations.Select(l=>new UV(l.U, l.V)), data.Values.Values);
        }

        private void ConstructCore(
            Autodesk.Revit.DB.View view, Reference faceReference, IEnumerable<UV> sampleLocations, IEnumerable<IEnumerable<double>> samplesSets)
        {
            var samples = samplesSets.ToArray();

            var sfm = GetSpatialFieldManagerFromView(view, (uint)samples.Count());

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
        private void InternalSetSpatialFieldValues(IEnumerable<UV> pointLocations, IEnumerable<IEnumerable<double>> values)
        {
            // Data will come in as:
            // A B C D
            // E F G H
            // I J K L

            // We need it in the form:
            // A E I
            // B F J
            // C G K
            // D H L

            var enumerable = values as IEnumerable<double>[] ?? values.ToArray();
            var height = enumerable.First().Count();
            var width = enumerable.Count();

            var valList = new List<ValueAtPoint>();
            for (int i = 0; i < height; i++)
            {
                var lst = new List<double>() { };

                for (int j = 0; j < width; j++)
                {
                    lst.Add(enumerable.ElementAt(j).ElementAt(i));
                }
                valList.Add(new ValueAtPoint(lst));
            }

            TransactionManager.Instance.EnsureInTransaction(Document);

            // Convert the analysis values to a special Revit type
            //var valList = enumerable.Select(n => new ValueAtPoint(n.ToList())).ToList();
            var sampleValues = new FieldValues(valList);

            // Convert the sample points to a special Revit Type
            var samplePts = new FieldDomainPointsByUV(pointLocations.ToList());

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
        /// <param name="elementFaceReference"></param>
        /// <param name="sampleUvPoints"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static FaceAnalysisDisplay ByViewFacePointsAndValues(View view, object elementFaceReference,
                        double[][] sampleUvPoints, double[] samples)
        {

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (elementFaceReference == null)
            {
                throw new ArgumentNullException("elementFaceReference");
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

            return new FaceAnalysisDisplay(view.InternalView, ElementFaceReference.TryGetFaceReference(elementFaceReference).InternalReference, sampleUvPoints.ToUvs(), samples);
        }

        public static FaceAnalysisDisplay ByViewAndFaceAnalysisData(
            View view, SurfaceAnalysisData data)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (data.Locations == null || !data.Locations.Any())
            {
                throw new ArgumentException("The input data does not have any locations.");
            }

            return new FaceAnalysisDisplay(view.InternalView, data);
        }

        #endregion

    }

}
