using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using NUnit.Framework;

using RevitServices.Persistence;
using RevitServices.Transactions;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class AnalysisDisplayTests : SystemTest
    {
        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt")]
        public void PointAnalysisDisplay_ByViewAndPointAnalysisData()
        {
            VerifyNoSpatialFieldManager();
            OpenAndRunDynamoDefinition(@".\AnalysisDisplay\PointAnalysisDisplay_ByViewAndPointAnalysisData.dyn");
            VerifySpatialFieldManagerAndValues("DynamoPoints");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt")]
        public void PointAnalysisDisplay_ByViewPointsValues()
        {
            VerifyNoSpatialFieldManager();
            OpenAndRunDynamoDefinition(@".\AnalysisDisplay\PointAnalysisDisplay_ByViewPointsValues.dyn");
            VerifySpatialFieldManagerAndValues("DynamoPoints");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt")]
        public void SurfaceAnalysisDisplay_ByViewAndPointAnalysisData()
        {
            VerifyNoSpatialFieldManager();
            OpenAndRunDynamoDefinition(@".\AnalysisDisplay\SurfaceAnalysisDisplay_ByViewAndPointAnalysisData.dyn");
            VerifySpatialFieldManagerAndValues("DynamoSurfaces");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt")]
        public void SurfaceAnalysisDisplay_ByViewAndPointsAndValues()
        {
            VerifyNoSpatialFieldManager();
            OpenAndRunDynamoDefinition(@".\AnalysisDisplay\SurfaceAnalysisDisplay_ByViewAndPointsAndValues.dyn");
            VerifySpatialFieldManagerAndValues("DynamoSurfaces");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt")]
        public void VectorAnalysisDisplay_ByViewAndVectorAnalysisData()
        {
            VerifyNoSpatialFieldManager();
            OpenAndRunDynamoDefinition(@".\AnalysisDisplay\VectorAnalysisDisplay_ByViewAndVectorAnalysisData.dyn");
            VerifySpatialFieldManagerAndValues("DynamoVectors");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt")]
        public void VectorAnalysisDisplay_ByViewPointsAndValues()
        {
            VerifyNoSpatialFieldManager();
            OpenAndRunDynamoDefinition(@".\AnalysisDisplay\VectorAnalysisDisplay_ByViewPointsAndValues.dyn");
            VerifySpatialFieldManagerAndValues("DynamoVectors");
        }

        private void VerifyNoSpatialFieldManager()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var view = doc.ActiveView;

            TransactionManager.Instance.EnsureInTransaction(doc);

            var sfm = SpatialFieldManager.GetSpatialFieldManager(view);

            Assert.Null(sfm);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void VerifySpatialFieldManagerAndValues(string displayStyleName)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var view = doc.ActiveView;

            TransactionManager.Instance.EnsureInTransaction(doc);

            var p = view.get_Parameter(BuiltInParameter.VIEW_ANALYSIS_DISPLAY_STYLE);
            p.SetValueString(displayStyleName);

            var sfm = SpatialFieldManager.GetSpatialFieldManager(view);
            
            Assert.NotNull(sfm);

            TransactionManager.Instance.TransactionTaskDone();
        }
    }
}
