using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Revit.Elements;
using Revit.GeometryConversion;
using MathNet.Numerics.NumberTheory;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.AnalysisDisplay
{
    /// <summary>
    /// Superclass for all Revit Analysis Display types
    /// 
    /// Note: We're using the user facing name from Revit (Analysis Display), rather than the same name that the Revit API
    /// uses (Spatial Field)
    /// </summary>
    [Browsable(false)]
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
                return DocumentManager.GetInstance().CurrentDBDocument;
            }
        }

        #endregion

        #region Protected properties

        /// <summary>
        /// The SpatialFieldManager governing this SpatialFieldPrimitive
        /// </summary>
        protected Autodesk.Revit.DB.Analysis.SpatialFieldManager SpatialFieldManager
        {
            get; set;
        }

        /// <summary>
        /// The Id for this SpatialFieldPrimitive
        /// </summary>
        protected int SpatialFieldPrimitiveId
        {
            get; set;
        }

        #endregion

        #region Protected mutators

        /// <summary>
        /// Set the SpatialFieldPrimitiveId
        /// </summary>
        /// <param name="primitiveId"></param>
        protected void InternalSetPrimitiveId(int primitiveId)
        {
            this.SpatialFieldPrimitiveId = primitiveId;
        }

        /// <summary>
        /// Set the SpatialFieldManager
        /// </summary>
        /// <param name="manager"></param>
        protected void InternalSetSpatialFieldManager(SpatialFieldManager manager)
        {
            this.SpatialFieldManager = manager;
        }

        #endregion

        #region Static helper methods

        /// <summary>
        /// The unique name for Dynamo's Analysis Results.  Used to identify
        /// the results scheme.
        /// </summary>
        protected static string ResultsSchemaName = "Dynamo Analysis Results";

        /// <summary>
        /// Get the AnalysisResultsSchemaIndex for the SpatialFieldManager
        /// </summary>
        /// <returns></returns>
        protected int GetAnalysisResultSchemaIndex()
        {
            // Get the AnalysisResultSchema index - there is only one for Dynamo
            var schemaIndex = 0;
            if (!SpatialFieldManager.IsResultSchemaNameUnique(AbstractAnalysisDisplay.ResultsSchemaName, -1))
            {
                var arses = SpatialFieldManager.GetRegisteredResults();
                schemaIndex =
                    arses.First(
                        x => SpatialFieldManager.GetResultSchema(x).Name == AbstractAnalysisDisplay.ResultsSchemaName);
            }
            else
            {
                var ars = new AnalysisResultSchema(AbstractAnalysisDisplay.ResultsSchemaName, "Resulting analyses from Dynamo.");
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
        protected static Autodesk.Revit.DB.Analysis.SpatialFieldManager GetSpatialFieldManagerFromView(Autodesk.Revit.DB.View view, uint numValuesPerAnalysisPoint = 1)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            Autodesk.Revit.DB.Analysis.SpatialFieldManager manager;
            return Autodesk.Revit.DB.Analysis.SpatialFieldManager.GetSpatialFieldManager(view) ??
                      Autodesk.Revit.DB.Analysis.SpatialFieldManager.CreateSpatialFieldManager(view, (int)numValuesPerAnalysisPoint);
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
                // no need to get rid of SpatialFieldManager, which may be used again
                // just remove the primitive
                this.SpatialFieldManager.RemoveSpatialFieldPrimitive( SpatialFieldPrimitiveId );
            }
        }

        #endregion

        #region Trace management

        /// <summary>
        /// Set the SpatialFieldManager PrimitiveId from Thread Local Storage
        /// </summary>
        /// <returns></returns>
        protected Tuple<SpatialFieldManager, int> GetElementAndPrimitiveIdFromTrace()
        {
            // This is a provisional implementation until we can store both items in trace
            var id = ElementBinder.GetElementIdFromTrace(Document);

            if (id == null) return null;

            var primitiveId = id.IntegerValue / PrimitiveIdPrimeFactor;
            var sfmId = id.IntegerValue % PrimitiveIdPrimeFactor;

            SpatialFieldManager sfm = null;

            // if we can't get the sfm, return null
            if (!Document.TryGetElement<SpatialFieldManager>(new ElementId(sfmId), out sfm)) return null;

            return new Tuple<SpatialFieldManager, int>(sfm, primitiveId);
        }

        /// <summary>
        /// This prime is used to store the primitive ID along with the Element Id
        /// </summary>
        private const int PrimitiveIdPrimeFactor = 1299721;

        /// <summary>
        /// Set the SpatialFieldManager and PrimitiveId in Thread Local Storage
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="primitiveId"></param>
        protected void SetElementAndPrimitiveIdForTrace(SpatialFieldManager manager, int primitiveId)
        {
            if (manager == null)
            {
                throw new Exception();
            }

            // This is provisional until we can store an Int and ElementId simultaneously in TLS
            var id = primitiveId * PrimitiveIdPrimeFactor + manager.Id.IntegerValue;
            ElementBinder.SetElementForTrace(new ElementId(id));
        }

        /// <summary>
        /// Set the current SpatialFieldManager and PrimitiveId in Thread Local Storage
        /// </summary>
        protected void SetElementAndPrimitiveIdForTrace()
        {
            SetElementAndPrimitiveIdForTrace(this.SpatialFieldManager, this.SpatialFieldPrimitiveId);
        }

        #endregion

}

}

