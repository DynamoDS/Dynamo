using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class AnalysisDisplayTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void AnalysisDisplayPoints()
        {
            OpenAndRun(@".\AnalysisDisplay\AD_Points.dyn");
        }

        [Test]
        public void AnalysisDisplaySurface()
        {
            OpenAndRun(@".\AnalysisDisplay\AD_Surface.dyn");
        }

        [Test]
        public void AnalysisDisplayVectors()
        {
            OpenAndRun(@".\AnalysisDisplay\AD_Vector.dyn");
        }
    }
}
