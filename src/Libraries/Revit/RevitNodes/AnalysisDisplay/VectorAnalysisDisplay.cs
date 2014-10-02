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
        private VectorAnalysisDisplay(Autodesk.Revit.DB.View view, IEnumerable<VectorAnalysisData> data)
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
                InternalSetSpatialFieldValues(primitiveId, d);
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
        private void InternalSetSpatialFieldValues(int primitiveId, VectorAnalysisData data)
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
            var schemaIndex = GetAnalysisResultSchemaIndex();

            // Update the values
            SpatialFieldManager.UpdateSpatialFieldPrimitive(primitiveId, samplePts, sampleValues, schemaIndex);

            TransactionManager.Instance.TransactionTaskDone();
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
        public static VectorAnalysisDisplay ByViewPointsAndVectorValues(View view,
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

            var valueDict = new Dictionary<string, IList<Vector>> { { "Dynamo Data", samples } };

            var data = new VectorAnalysisData(samplePoints, valueDict);
            return new VectorAnalysisDisplay(view.InternalView, new List<VectorAnalysisData>(){data});
        }

        #endregion

    }

}
