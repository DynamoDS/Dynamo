using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class AnalysisDisplayTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\AnalysisDisplay\AD_Points.rvt")]
        public void AnalysisDisplayPoints()
        {
            OpenAndRun(@".\AnalysisDisplay\AD_Points.dyn");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\AD_Surface.rvt")]
        public void AnalysisDisplaySurface()
        {
            OpenAndRun(@".\AnalysisDisplay\AD_Surface.dyn");
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\AD_Vector.rvt")]
        public void AnalysisDisplayVectors()
        {
            OpenAndRun(@".\AnalysisDisplay\AD_Vector.dyn");
        }
    }
}
