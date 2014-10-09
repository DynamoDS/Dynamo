using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using DynamoUnits;

using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Autodesk.DesignScript.Runtime;

namespace Revit.AnalysisDisplay
{
    /// <summary>
    /// Hold a pair of element ID of SpatialFieldManager and primitive ID to
    /// support serialization.
    /// </summary>
    [SupressImportIntoVM]
    [Serializable]
    public class SpmPrimitiveIdPair : ISerializable
    {
        public int SpatialFieldManagerID { get; set; }
        public List<int> PrimitiveIds { get; set; }
 
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SpatialFieldManagerID", SpatialFieldManagerID, typeof(int));
            info.AddValue("PrimitiveIds", PrimitiveIds, typeof(List<int>));
        }

        public SpmPrimitiveIdPair()
        {
            SpatialFieldManagerID = int.MinValue;
            PrimitiveIds = new List<int>();
        }

        public SpmPrimitiveIdPair(SerializationInfo info, StreamingContext context)
        {
            SpatialFieldManagerID = (int) info.GetValue("SpatialFieldManagerID", typeof (int));
            PrimitiveIds =
                (List<int>)
                    info.GetValue("PrimitiveIds", typeof(List<int>));
        }
    }

    /// <summary>
    /// Superclass for all Revit Analysis Display types
    /// 
    /// Note: We're using the user facing name from Revit (Analysis Display), rather than the same name that the Revit API
    /// uses (Spatial Field)
    /// </summary>
//    [IsVisibleInDynamoLibrary(false)]    - removing as per MAGN-3382
    [SupressImportIntoVM]
    public abstract class AbstractAnalysisDisplay : IDisposable
    {
        #region Static properties

        /// <summary>
        /// A reference to the current document
        /// </summary>
        protected static Autodesk.Revit.DB.Document Document
        {
            get
            {
                return DocumentManager.Instance.CurrentDBDocument;
            }
        }

        #endregion

        #region Protected properties

        /// <summary>
        /// The SpatialFieldManager governing this SpatialFieldPrimitive
        /// </summary>
        protected Autodesk.Revit.DB.Analysis.SpatialFieldManager SpatialFieldManager
        {
            get;
            set;
        }

        protected List<int> PrimitiveIds { get; set; }

        #endregion

        #region Protected mutators

        /// <summary>
        /// Set the SpatialFieldManager
        /// </summary>
        /// <param name="manager"></param>
        protected void InternalSetSpatialFieldManager(SpatialFieldManager manager)
        {
            SpatialFieldManager = manager;
        }

        protected void InternalSetSpatialPrimitiveIds(List<int> primitiveIds)
        {
            PrimitiveIds = primitiveIds;
        }

        #endregion

        #region Static helper methods

        /// <summary>
        /// The unique name for Dynamo's Analysis Results.  Used to identify
        /// the results scheme.
        /// </summary>
        //protected static string ResultsSchemaName = "Dynamo Analysis Results";

        /// <summary>
        /// Get the AnalysisResultsSchemaIndex for the SpatialFieldManager
        /// </summary>
        /// <returns></returns>
        protected virtual int GetAnalysisResultSchemaIndex(string resultsSchemaName, string resultsDescription, Type unitType)
        {
            // Get the AnalysisResultSchema index - there is only one for Dynamo
            var schemaIndex = 0;
            if (!SpatialFieldManager.IsResultSchemaNameUnique(resultsSchemaName, -1))
            {
                var arses = SpatialFieldManager.GetRegisteredResults();
                schemaIndex =
                    arses.First(
                        x => SpatialFieldManager.GetResultSchema(x).Name == resultsSchemaName);
            }
            else
            {
                var ars = new AnalysisResultSchema(resultsSchemaName, resultsDescription);
                
                if (unitType != null)
                {
                    if (typeof(SIUnit).IsAssignableFrom(unitType))
                    {
                        var prop = unitType.GetProperty("Conversions");
                        var conversions = (Dictionary<string, double>)prop.GetValue(null, new object[] { });
                        if (conversions != null)
                        {
                            var unitNames = conversions.Keys.ToList();
                            var multipliers = conversions.Values.ToList();
                            ars.SetUnits(unitNames, multipliers);
                            ars.CurrentUnits = 0;
                        }
                    }
                }

                schemaIndex = SpatialFieldManager.RegisterResult(ars);
            }

            return schemaIndex;
        }

        /// <summary>
        /// Get the SpatialFieldManager for a particular view.  This is a singleton for every view.  Note that the 
        /// number of values per analysis point is ignored if the SpatialFieldManager has already been obtained
        /// for this view.  This field cannot be mutated once the SpatialFieldManager is set for a partiular 
        /// view.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="numValuesPerAnalysisPoint"></param>
        /// <returns></returns>
        protected static SpatialFieldManager GetSpatialFieldManagerFromView(Autodesk.Revit.DB.View view, uint numValuesPerAnalysisPoint = 1)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            TransactionManager.Instance.EnsureInTransaction(Document);

            var sfm = SpatialFieldManager.GetSpatialFieldManager(view);

            if (sfm == null)
            {
                sfm = SpatialFieldManager.CreateSpatialFieldManager(view, (int)numValuesPerAnalysisPoint);
            }
            else
            {
                // If the number of values stored
                // at each location is not equal to what we are requesting,
                // then create a new SFM
                if (sfm.NumberOfMeasurements != numValuesPerAnalysisPoint)
                {
                    DocumentManager.Instance.CurrentDBDocument.Delete(sfm);
                    sfm = SpatialFieldManager.CreateSpatialFieldManager(view, (int)numValuesPerAnalysisPoint);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            return sfm;
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Destroy
        /// </summary>
        void IDisposable.Dispose()
        {
            if (SpatialFieldManager != null)
            {
                // no need to delete SpatialFieldManager, neither primitive id.
            }
        }

        #endregion

        #region Trace management

        /// <summary>
        /// Set the SpatialFieldManager PrimitiveId from Thread Local Storage
        /// </summary>
        /// <returns></returns>
        protected Tuple<SpatialFieldManager, List<int>> GetElementAndPrimitiveIdFromTrace()
        {
            // This is a provisional implementation until we can store both items in trace
            var id = ElementBinder.GetRawDataFromTrace();
            if (id == null)
                return null;

            var idPair = id as SpmPrimitiveIdPair;
            if (idPair == null)
                return null;

            var primitiveIds = idPair.PrimitiveIds;
            var sfmId = idPair.SpatialFieldManagerID;

            SpatialFieldManager sfm = null;

            // if we can't get the sfm, return null
            if (!Document.TryGetElement(new ElementId(sfmId), out sfm)) 
                return null;

            return new Tuple<SpatialFieldManager, List<int>>(sfm, primitiveIds);
        }

        /// <summary>
        /// Set the SpatialFieldManager and PrimitiveId in Thread Local Storage
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="map"></param>
        protected void SetElementAndPrimitiveIdsForTrace(SpatialFieldManager manager, List<int> primitiveIds)
        {
            if (manager == null)
            {
                throw new Exception();
            }

            var idPair = new SpmPrimitiveIdPair
            {
                SpatialFieldManagerID = manager.Id.IntegerValue,
                PrimitiveIds = primitiveIds
            };
            ElementBinder.SetRawDataForTrace(idPair);
        }

        /// <summary>
        /// Set the current SpatialFieldManager and PrimitiveId in Thread Local Storage
        /// </summary>
        protected void SetElementAndPrimitiveIdsForTrace()
        {
            SetElementAndPrimitiveIdsForTrace(SpatialFieldManager, PrimitiveIds);
        }

        #endregion
    }
}