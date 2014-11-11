using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Models;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    [TestFixture]
    class WorkflowTests : DSEvaluationViewModelUnitTest
    {
        // Note: Only add test cases those are related to ASM Geometry.
        // Always create a region while creating tests for new Geometry type.

        #region Solid Tests
        [Test]
        public void TestSolidSweep()
        {
            // This will test user workflow which contains many nodes, final output is Solid using
            // sweep.
            string openPath = Path.Combine(GetTestDirectory(), @"core\WorkflowTestFiles\RandomModel_V3.dyn");
            
            RunModel(openPath);

            var solidNodeId = "ff0b24b2-73e5-4da6-8081-a1883ff9ad72";
            AssertPreviewCount(solidNodeId, 66);

            // output will be 66 Solids, so putting verification for all Solid creation
            for (int i = 0; i<=65; i++)
            {
                var sweptSolid = GetPreviewValueAtIndex(solidNodeId, i) as Solid;
                Assert.IsNotNull(sweptSolid);
            }

        }
        [Test]
        public void Defect_MAGN_3399()
        {
            // This will test user workflow which contains many nodes.

            string openPath = Path.Combine(GetTestDirectory(), @"core\WorkflowTestFiles\20140418_buildingSetback_standalone.dyn");

            RunModel(openPath);

            var FARId = "c03065ec-fe54-40de-8c27-8089c7fe1b73";
            AssertPreviewValue(FARId, 3.843);

        }
        #endregion
    }
}
