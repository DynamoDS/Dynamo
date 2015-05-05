using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Dynamo.Models;
using System.Linq;

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


        //add test cases that are related to CodeBlocks.
        #region CodeBlocks test
        [Test]
        public void Test_CodeBlocksReference()
        {
            // Create automation for Dynamo file : Code Blocks Reference.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\04 Code blocks\Code Blocks Reference.dyn");
            RunModel(openPath);

            //check preview values of CBNs
            var cbn = "27c2f333-f51f-4a0c-9f71-70dc64f2ecbe";
            AssertPreviewValue(cbn, 3.142);
            var strCbn = "4c8ddee8-e2b1-4472-9470-a4142f56ac97";
            string str = "Less is more.";
            AssertPreviewValue(strCbn, str);
            var pointCbn = "4ea1d0d8-4882-4f6f-b659-2dcb297db34e";
            var x = GetPreviewValue(pointCbn) as Point;
            Assert.AreEqual(0, x.X);
            Assert.AreEqual(0, x.Y);
            Assert.AreEqual(0, x.Z);
            var ptCBN = "613c6d53-0eab-414f-9aeb-fc7c56de6900";
            AssertPreviewValue(ptCBN, 10);
            
            //check preview values of code block nodes
            var CBN1 = "34a4b2fc-a1c5-4c18-a0fe-48b1c3b8f711";
            var CBN1Val = GetPreviewValue(CBN1) as Point;
            var CBN2 = "a1142e89-89e4-4fa2-a63d-d58489af044d";
            var CBN2Val = GetPreviewValue(CBN2);
            Assert.AreEqual(CBN1Val, CBN2Val);

            //check preview value of code block node, which contains a list
            var listCBN = "4c049b29-2792-4a66-ab89-d8fac3161e7d";
            AssertPreviewCount(listCBN, 4);

            //check preview value of CBN, which is a list and not null
            var notNullCBN = "d71e5f04-9928-437a-863c-5bf34666370b";
            AssertPreviewCount(notNullCBN, 5);
            for (int i = 0; i < 5; i++)
            {
                var element = GetPreviewValueAtIndex(notNullCBN, i) as string;
                Assert.IsNotNull(element);
            }

            //check the preview value of CBN, which value is null
            var nullCBN = "f973a3ec-70ee-475b-ad7d-50e5f21c1e04";
            AssertPreviewValue(nullCBN, null);

            //check the CBN which contains a list of nurbsCurve
            var nurbCBN = "8dce8e89-1d9d-4ffe-b6ea-e7251ab1368b";
            AssertPreviewCount(nurbCBN, 24);
            for (int i = 0; i<24; i++)
            {
                var nurbCBNVal = GetPreviewValueAtIndex(nurbCBN, i) as NurbsCurve;
                Assert.IsNotNull(nurbCBNVal);
            }
            
        }
        #endregion

    }
}
