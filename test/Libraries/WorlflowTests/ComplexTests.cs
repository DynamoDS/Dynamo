using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    [TestFixture]
    class WorkflowTests : DSEvaluationViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        // Note: Only add test cases those are related to ASM Geometry.
        // Always create a region while creating tests for new Geometry type.

        #region Solid Tests
        [Test]
        public void TestSolidSweep()
        {
            // This will test user workflow which contains many nodes, final output is Solid using
            // sweep.
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\RandomModel_V3.dyn");
            
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
       public void IndexOutsideBounds_3399()
        {
             // This will test user workflow which contains many nodes.
             // Crash with "Index was outside the bounds of the array"
 
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\20140418_buildingSetback_standalone.dyn");

            

            var FARId = "c03065ec-fe54-40de-8c27-8089c7fe1b73";
            Assert.DoesNotThrow(()=>RunModel(openPath));
            

        }
        #endregion
    }
}
