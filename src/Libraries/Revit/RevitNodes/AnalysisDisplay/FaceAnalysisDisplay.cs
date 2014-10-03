using System;
using System.Collections.Generic;
using System.Linq;

using Analysis;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using Revit.GeometryConversion;

using RevitServices.Persistence;
using RevitServices.Transactions;

using UV = Autodesk.Revit.DB.UV;
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
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="data"></param>
        private FaceAnalysisDisplay(
            Autodesk.Revit.DB.View view, IEnumerable<ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, double>> data, string resultsName, string description)
        {
            var sfm = GetSpatialFieldManagerFromView(view, (uint)data.First().Results.Count());

            //var sfmAndId = GetElementAndPrimitiveIdFromTrace();
            
            // we can rebind as we're dealing with the same view
            //if (sfmAndId != null && sfmAndId.Item1.Id == sfm.Id)
            //{
            //    InternalSetSpatialFieldManager(sfmAndId.Item1);
            //    InternalSetSpatialPrimitiveIds(sfmAndId.Item2);
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

            // create a new spatial field primitive
            TransactionManager.Instance.EnsureInTransaction(Document);

            // TEMPORARY UNTIL WE RESOLVE TRACE
            sfm.Clear();

            sfm.SetMeasurementNames(data.SelectMany(d => d.Results.Keys).Distinct().ToList());

            InternalSetSpatialFieldManager(sfm);
            var primitiveIds = new List<int>();

            foreach (var d in data)
            {
                var reference = d.Surface.Tags.LookupTag(DefaultTag) as Reference;
                if (reference == null)
                {
                    continue;
                }
                
                var primitiveId = SpatialFieldManager.AddSpatialFieldPrimitive(reference);
                primitiveIds.Add(primitiveId);
                InternalSetSpatialFieldValues(primitiveId, d, resultsName, description);
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
        /// <param name="primitiveId"></param>
        /// <param name="data"></param>
        private void InternalSetSpatialFieldValues(int primitiveId, ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, double> data, string schemaName, string description)
        {
            // Get the surface reference
            var reference = data.Surface.Tags.LookupTag(DefaultTag) as Reference;

            var el = DocumentManager.Instance.CurrentDBDocument.GetElement(reference.ElementId);
            var pointLocations = new List<UV>();
            if (el != null)
            {
                var face = el.GetGeometryObjectFromReference(reference) as Autodesk.Revit.DB.Face;
                if (face != null)
                {
                    var bbox = face.GetBoundingBox();
                    var uSpan = bbox.Max.U - bbox.Min.U;
                    var vSpan = bbox.Max.V - bbox.Min.V;

                    pointLocations.AddRange(data.CalculationLocations.Select(uv => new UV(bbox.Min.U + uv.U*uSpan, bbox.Min.V + uv.V*vSpan)));
                }
            }

            var values = data.Results.Values.ToList();

            // Data will come in as:
            // A B C D
            // E F G H
            // I J K L

            // We need it in the form:
            // A E I
            // B F J
            // C G K
            // D H L

            var height = values.First().Count();
            var width = values.Count();

            var valList = new List<ValueAtPoint>();
            for (int i = 0; i < height; i++)
            {
                var lst = new List<double>() { };

                for (int j = 0; j < width; j++)
                {
                    lst.Add(values.ElementAt(j).ElementAt(i));
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
            var schemaIndex = GetAnalysisResultSchemaIndex(schemaName, description);

            // Update the values
            SpatialFieldManager.UpdateSpatialFieldPrimitive(primitiveId, samplePts, sampleValues, schemaIndex);

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Show a colored Face Analysis Display in the Revit View.
        /// </summary>
        /// <param name="view">The view into which you want to draw the analysis data.</param>
        /// <param name="surface">The surface onto which the analysis data will be displayed</param>
        /// <param name="sampleUvPoints"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static FaceAnalysisDisplay ByViewFacePointsAndValues(View view, Surface surface,
                        double[][] sampleUvPoints, double[] samples)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (surface == null)
            {
                throw new ArgumentNullException("surface");
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

            var valueDict = new Dictionary<string, IList<double>>
            {
                { "Dynamo Data", samples }
            };

            var data = new SurfaceAnalysisData(surface, sampleUvPoints.ToDSUvs(), valueDict);

            return new FaceAnalysisDisplay(view.InternalView, new ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, double>[] { data }, Resource1.AnalysisResultsDefaultName, Resource1.AnalysisResultsDefaultDescription);
        }

        /// <summary>
        /// Show a colored Face Analysis Display in the Revit View.
        /// </summary>
        /// <param name="view">The view into which you want to draw the analysis data.</param>
        /// <param name="surface">The surface onto which the analysis data will be displayed</param>
        /// <param name="sampleUvPoints"></param>
        /// <param name="samples"></param>
        /// <param name="name">The name of the analysis results.</param>
        /// <param name="description">A description of the analysis results.</param>
        /// <returns></returns>
        public static FaceAnalysisDisplay ByViewFacePointsAndValues(
            View view, Surface surface,
            double[][] sampleUvPoints, double[] samples, string name, string description)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (surface == null)
            {
                throw new ArgumentNullException("surface");
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

            if (string.IsNullOrEmpty(name))
            {
                name = Resource1.AnalysisResultsDefaultName;
            }

            if (string.IsNullOrEmpty(description))
            {
                description = Resource1.AnalysisResultsDefaultDescription;
            }

            var valueDict = new Dictionary<string, IList<double>>
            {
                { "Dynamo Data", samples }
            };

            var data = new SurfaceAnalysisData(surface, sampleUvPoints.ToDSUvs(), valueDict);

            return new FaceAnalysisDisplay(view.InternalView, new ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, double>[] { data }, name, description);
        }

        /// <summary>
        /// Show a colored Face Analysis Display in the Revit View
        /// </summary>
        /// <param name="view">The view into which you want to draw the analysis data.</param>
        /// <param name="data">A collection of SurfaceAnalysisData objects.</param>
        /// <returns></returns>
        public static FaceAnalysisDisplay ByViewAndFaceAnalysisData(
            View view, SurfaceAnalysisData[] data, string name, string description)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (data == null || !data.Any())
            {
                throw new ArgumentException("The input data does not have any locations.");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = Resource1.AnalysisResultsDefaultName;
            }

            if (string.IsNullOrEmpty(description))
            {
                description = Resource1.AnalysisResultsDefaultDescription;
            }

            return new FaceAnalysisDisplay(view.InternalView, data, name, description);
        }

        #endregion

    }
}
