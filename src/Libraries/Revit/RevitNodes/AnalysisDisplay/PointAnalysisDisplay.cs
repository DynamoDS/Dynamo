using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using Revit.Elements;
using RevitServices.Transactions;
using View = Revit.Elements.Views.View;
using RevitServices.Persistence;
using System.Runtime.Serialization;
using RevitServices.Elements;

namespace Revit.AnalysisDisplay
{
    [SupressImportIntoVM]
    [Serializable]
    public class SpmPrimitiveIdListPair : ISerializable
    {
        public int SpatialFieldManagerID { get; set; }
        public List<int> PrimitiveIDs { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SpatialFieldManagerID", SpatialFieldManagerID, typeof(int));
            info.AddValue("PrimitiveIDCount", PrimitiveIDs.Count, typeof(int));
            foreach (var id in PrimitiveIDs)
            {
                info.AddValue("ID", id, typeof(int));
            }
        }

        public SpmPrimitiveIdListPair()
        {
            SpatialFieldManagerID = int.MinValue;
            PrimitiveIDs = new List<int>();
        }

        public SpmPrimitiveIdListPair(SerializationInfo info, StreamingContext context)
        {
            SpatialFieldManagerID = (int)info.GetValue("SpatialFieldManagerID", typeof(int));

            int count = (int)info.GetValue("PrimitiveIDCount", typeof(int));
            PrimitiveIDs = new List<int>();
            for (int i = 0; i < count; ++i)
            {
                var id = (int)info.GetValue("ID", typeof(int));
                PrimitiveIDs.Add(id);
            }
        }
    }

    /// <summary>
    /// A Revit Point Analysis Display 
    /// </summary>
    [DSNodeServices.RegisterForTrace]
    public class PointAnalysisDisplay : AbstractAnalysisDisplay
    {

        private List<int> primitiveIds = new List<int>();

        #region Private constructors

        /// <summary>
        /// Create a Point Analysis Display in the current view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="sampleLocations"></param>
        /// <param name="samples"></param>
        private PointAnalysisDisplay(Autodesk.Revit.DB.View view, IEnumerable<Autodesk.Revit.DB.XYZ> sampleLocations,
            IEnumerable<double> samples)
        {
            var sfm = GetSpatialFieldManagerFromView(view);
            var sfmAndIds = GetElementAndPrimitiveIdListFromTrace();

            // the input view has changed, remove the old primitive from the old view
            if (sfmAndIds != null)
            {
                var oldSfm = sfmAndIds.Item1;
                var oldIds = sfmAndIds.Item2;

                foreach (var oldId in oldIds)
                {
                    oldSfm.RemoveSpatialFieldPrimitive(oldId); 
                }
            }

            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalSetSpatialFieldManager(sfm);
            InternalSetSpatialFieldValues(sampleLocations, samples);

            SetElementAndPrimitiveIdListTrace(SpatialFieldManager, primitiveIds);

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
        private void InternalSetSpatialFieldValues(IEnumerable<XYZ> pointLocations, IEnumerable<double> values)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var chunkSize = 1000;
            while (pointLocations.Any()) 
            {
                // Convert the analysis values to a special Revit type
                var pointLocationChunk = pointLocations.Take(chunkSize).ToList<XYZ>();
                var valuesChunk = values.Take(chunkSize).ToList();
                var valList = valuesChunk.Select(n => new ValueAtPoint(new List<double> { n })).ToList();

                // Convert the sample points to a special Revit Type
                var samplePts = new FieldDomainPointsByXYZ(pointLocationChunk.ToList<XYZ>());
                var sampleValues = new FieldValues(valList);

                // Get the analysis results schema
                var schemaIndex = GetAnalysisResultSchemaIndex();

                // Update the values
                var primitiveId = SpatialFieldManager.AddSpatialFieldPrimitive();
                primitiveIds.Add(primitiveId);
                SpatialFieldManager.UpdateSpatialFieldPrimitive(primitiveId, samplePts, sampleValues, schemaIndex);

                pointLocations = pointLocations.Skip(chunkSize);
                values = values.Skip(chunkSize);
            }

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Show a colored Point Analysis Display in the Revit view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="samplePoints"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static PointAnalysisDisplay ByViewPointsAndValues(View view,
                        Autodesk.DesignScript.Geometry.Point[] samplePoints, double[] samples) 
                        
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

            return new PointAnalysisDisplay(view.InternalView, samplePoints.ToXyzs(), samples);
        }

        #endregion

        protected Tuple<SpatialFieldManager, List<int>> GetElementAndPrimitiveIdListFromTrace()
        {
            // This is a provisional implementation until we can store both items in trace
            var id = ElementBinder.GetRawDataFromTrace();
            if (id == null)
                return null;

            var idPair = id as SpmPrimitiveIdListPair;
            if (idPair == null)
                return null;

            var primitiveIds = idPair.PrimitiveIDs;
            var sfmId = idPair.SpatialFieldManagerID;

            SpatialFieldManager sfm = null;

            // if we can't get the sfm, return null
            if (!Document.TryGetElement<SpatialFieldManager>(new ElementId(sfmId), out sfm)) return null;

            return new Tuple<SpatialFieldManager, List<int>>(sfm, primitiveIds);
        }

        protected void SetElementAndPrimitiveIdListTrace(SpatialFieldManager manager, List<int> primitiveIds)
        {
            if (manager == null)
            {
                throw new Exception();
            }

            SpmPrimitiveIdListPair idPair = new SpmPrimitiveIdListPair();
            idPair.SpatialFieldManagerID = manager.Id.IntegerValue;
            idPair.PrimitiveIDs= primitiveIds;
            ElementBinder.SetRawDataForTrace(idPair);
        }
    }

}
