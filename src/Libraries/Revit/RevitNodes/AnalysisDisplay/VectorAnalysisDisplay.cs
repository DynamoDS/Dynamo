using System;
using System.Collections.Generic;
using System.Linq;

using Analysis;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using Revit.GeometryConversion;

using RevitServices.Transactions;
using View = Revit.Elements.Views.View;

namespace Revit.AnalysisDisplay
{
    /// <summary>
    /// A Revit Vector Analysis Display 
    /// </summary>
    [DSNodeServices.RegisterForTrace]
    public class VectorAnalysisDisplay : AbstractAnalysisDisplay
    {

        #region Private constructors

        /// <summary>
        /// Create a Point Analysis Display in the current view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private VectorAnalysisDisplay(Autodesk.Revit.DB.View view, IEnumerable<VectorAnalysisData> data, string resultsName, string description)
        {
            var sfm = GetSpatialFieldManagerFromView(view);

            //var sfmAndId = GetElementAndPrimitiveIdFromTrace();

            //// we can rebind as we're dealing with the same view
            //if (sfmAndId != null && sfmAndId.Item1.Id == sfm.Id)
            //{
            //    InternalSetSpatialFieldManager(sfm);
            //    //InternalSetPrimitiveId(sfmAndId.Item2);
            //    InternalSetSpatialFieldValues(sampleLocations, samples);
            //    return;
            //}

            //// the input view has changed, remove the old primitive from the old view
            //if (sfmAndId != null)
            //{
            //    var oldSfm = sfmAndId.Item1;
            //    var oldId = sfmAndId.Item2;

            //    oldSfm.RemoveSpatialFieldPrimitive(oldId);
            //}

            TransactionManager.Instance.EnsureInTransaction(Document);

            // TEMPORARY UNTIL WE RESOLVE TRACE
            sfm.Clear();

            var vectorAnalysisDatas = data as VectorAnalysisData[] ?? data.ToArray();
            sfm.SetMeasurementNames(vectorAnalysisDatas.SelectMany(d => d.Results.Keys).Distinct().ToList());

            var primitiveIds = new List<int>();

            InternalSetSpatialFieldManager(sfm);

            foreach (var d in vectorAnalysisDatas)
            {
                var primitiveId = SpatialFieldManager.AddSpatialFieldPrimitive();
                InternalSetSpatialFieldValues(primitiveId, d, resultsName, description);
                primitiveIds.Add(primitiveId);
            }

            //SetElementAndPrimitiveIdsForTrace(SpatialFieldManager, primitiveIds);

            TransactionManager.Instance.TransactionTaskDone();

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the spatial field values for the current spatial field primitive.  The two 
        /// input sequences should be of the same length.
        /// </summary>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private void InternalSetSpatialFieldValues(int primitiveId, VectorAnalysisData data, string schemaName, string description)
        {
            var values = data.Results.Values;

            var height = values.First().Count();
            var width = values.Count();

            // Transpose and convert the analysis values to a special Revit type
            var valList = new List<VectorAtPoint>();
            for (int i = 0; i < height; i++)
            {
                var lst = new List<XYZ>() { };

                for (int j = 0; j < width; j++)
                {
                    lst.Add(values.ElementAt(j).ElementAt(i).ToXyz());
                }
                valList.Add(new VectorAtPoint(lst));
            }

            TransactionManager.Instance.EnsureInTransaction(Document);

            var sampleValues = new FieldValues(valList);

            // Convert the sample points to a special Revit Type
            var samplePts = new FieldDomainPointsByXYZ(data.CalculationLocations.Select(p=>p.ToXyz()).ToList());

            // Get the analysis results schema
            var schemaIndex = GetAnalysisResultSchemaIndex(schemaName, description);

            // Update the values
            SpatialFieldManager.UpdateSpatialFieldPrimitive(primitiveId, samplePts, sampleValues, schemaIndex);

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Show a Vector Analysis Display in the Revit view.
        /// </summary>
        /// <param name="view">The view into which you want to draw the analysis results.</param>
        /// <param name="samplePoints">The locations at which you want to create analysis values.</param>
        /// <param name="samples">The analysis values at the given locations.</param>
        /// <param name="name">An optional analysis results name to show on the results legend.</param>
        /// <param name="description">An optional analysis results description to show on the results legend.</param>
        /// <returns>A VectorAnalysisDisplay object.</returns>
        public static VectorAnalysisDisplay ByViewPointsAndVectorValues(View view,
                        Autodesk.DesignScript.Geometry.Point[] sampleLocations, Vector[] samples,
            string name = "", string description = "")
        {

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (sampleLocations == null)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (samples == null)
            {
                throw new ArgumentNullException("samples");
            }

            if (sampleLocations.Length != samples.Length)
            {
                throw new Exception("The number of sample points and number of samples must be the same");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = Resource1.AnalysisResultsDefaultName;
            }

            if (string.IsNullOrEmpty(description))
            {
                description = Resource1.AnalysisResultsDefaultDescription;
            }

            var valueDict = new Dictionary<string, IList<Vector>> { { "Dynamo Data", samples } };

            var data = new VectorAnalysisData(sampleLocations, valueDict);
            return new VectorAnalysisDisplay(view.InternalView, new List<VectorAnalysisData>() { data }, name, description);
        }

        #endregion

    }

}
