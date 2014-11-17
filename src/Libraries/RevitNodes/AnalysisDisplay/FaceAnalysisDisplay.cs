using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Analysis;
using Analysis.DataTypes;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using DynamoUnits;

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
        protected FaceAnalysisDisplay(
            Autodesk.Revit.DB.View view, IEnumerable<ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, double>> data, string resultsName, string description,Type unitType)
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
                InternalSetSpatialFieldValues(primitiveId, d, resultsName, description, unitType);
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
        private void 
            InternalSetSpatialFieldValues(int primitiveId, ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, 
            double> data, string schemaName, string description, Type unitType)
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
                    foreach (var loc in data.CalculationLocations)
                    {
                        var pt = data.Surface.PointAtParameter(loc.U, loc.V);
                        var faceLoc = face.Project(pt.ToXyz()).UVPoint;
                        pointLocations.Add(faceLoc);
                    }
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
            var schemaIndex = GetAnalysisResultSchemaIndex(schemaName, description, unitType);

            // Update the values
            SpatialFieldManager.UpdateSpatialFieldPrimitive(primitiveId, samplePts, sampleValues, schemaIndex);

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Show a colored Face Analysis Display in the Revit view.
        /// </summary>
        /// <param name="view">The view into which you want to draw the analysis results.</param>
        /// <param name="sampleLocations">The locations at which you want to create analysis values.</param>
        /// <param name="samples">The analysis values at the given locations.</param>
        /// <param name="name">An optional analysis results name to show on the results legend.</param>
        /// <param name="description">An optional analysis results description to show on the results legend.</param>
        /// <param name="unitType">An optional Unit type to provide conversions in the analysis results.</param>
        /// <returns>A FaceAnalysisDisplay object.</returns>
        public static FaceAnalysisDisplay ByViewFacePointsAndValues(
            View view, Surface surface,
            Autodesk.DesignScript.Geometry.UV[] sampleLocations, double[] samples, string name = "", string description = "", Type unitType = null)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (sampleLocations == null)
            {
                throw new ArgumentNullException("sampleUvPoints");
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

            var data = SurfaceAnalysisData.BySurfacePointsAndResults(surface, sampleLocations.ToList(), new List<string> { "Dynamo Data" }, new List<IList<double>>{samples});

            return new FaceAnalysisDisplay(view.InternalView, new ISurfaceAnalysisData<Autodesk.DesignScript.Geometry.UV, double>[] { data }, name, description, unitType);
        }

        /// <summary>
        /// Show a colored Face Analysis Display in the Revit view.
        /// </summary>
        /// <param name="view">The view into which you want to draw the analysis results.</param>
        /// <param name="data">A collection of SurfaceAnalysisData objects.</param>
        /// <param name="name">An optional analysis results name to show on the results legend.</param>
        /// <param name="description">An optional analysis results description to show on the results legend.</param>
        /// <param name="unitType">An optional Unit type to provide conversions in the analysis results.</param>
        /// <returns>A FaceAnalysisDisplay object.</returns>
        public static FaceAnalysisDisplay ByViewAndFaceAnalysisData(
            View view, SurfaceAnalysisData[] data, string name = "", string description = "", Type unitType = null)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (!data.Any())
            {
                throw new Exception("There is no input data.");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = Resource1.AnalysisResultsDefaultName;
            }

            if (string.IsNullOrEmpty(description))
            {
                description = Resource1.AnalysisResultsDefaultDescription;
            }

            return new FaceAnalysisDisplay(view.InternalView, data, name, description, unitType);
        }

        #endregion

    }
}
