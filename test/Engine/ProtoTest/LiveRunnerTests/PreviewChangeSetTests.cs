using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;
using System.Linq;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using System.Collections;
using ProtoCore;

namespace ProtoTest.LiveRunner
{
    class PreviewChangeSetTests : ProtoTestBase
    {
        [Test]
        public void TestPreviewModify1Node01()
        {
            List<string> codes = new List<string>() 
            {
               @"
                    a = 1;
                ",
                 
               @"
                    x = a;
                    y = x;
                ",

               @"
                    a = 10;
                ",
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            // Create and run the graph  [a = 1;] and [x = a; y = x;]
            ProtoScript.Runners.LiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);


            // Modify [a = 1;] to [a = 10;] 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[2]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(liveRunner.Core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get the the preview guids (affected graphs)
            List<Guid> reachableGuidList = changeSetState.EstimateNodesAffectedByASTList(astList);

            // Check if the the affected guids are in the list
            List<Guid> expectedGuid = new List<Guid> { guid1, guid2 };
            AssertPreview(reachableGuidList, expectedGuid);
        }

        [Test]
        public void TestPreviewModify1Node02()
        {
            List<string> codes = new List<string>() 
            {
                // guid1
               @"
                    a = 1; 
                ",
                // guid2
               @"
                    x = a; 
                    y = x;
                ",
                 // guid3
               @"
                    z = a; 
                ",
                 // guid1
               @"
                    a = 10; 
                ",
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create and run the graph 
            ProtoScript.Runners.LiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);


            // Modify [a = 1;] to [a = 10;] 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[3]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(liveRunner.Core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get the preview guids (affected graphs)
            List<Guid> reachableGuidList = changeSetState.EstimateNodesAffectedByASTList(astList);

            // Check if the the affected guids are in the list
            List<Guid> expectedGuid = new List<Guid> { guid1, guid2, guid3 };
            AssertPreview(reachableGuidList, expectedGuid);
        }

        [Test]
        public void TestPreviewModify2Nodes01()
        {
            List<string> codes = new List<string>() 
            {
               @"
                    a = 1;
                ",
                 
               @"
                    b = 2;
                ",

               @"
                    x = a;
                ",

               @"
                    y = b;
                ",
                 
               @"
                    a = 10;
                ",
               @"
                    b = 20;
                ",
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            // Create and run the graph 
            ProtoScript.Runners.LiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);


            // Modify [a = 1;] to [a = 10;] 
            // Modify [b = 2;] to [b = 20;] 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[4]));
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[5]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(liveRunner.Core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get the the preview guids (affected graphs)
            List<Guid> reachableGuidList = changeSetState.EstimateNodesAffectedByASTList(astList);

            // Check if the the affected guids are in the list
            List<Guid> expectedGuid = new List<Guid> { guid1, guid2, guid3, guid4 };
            AssertPreview(reachableGuidList, expectedGuid);
        }

        [Test]
        public void TestPreviewModify2Nodes02()
        {
            List<string> codes = new List<string>() 
            {
               @"
                    a = 1;
                ",
                 
               @"
                    b = 2;
                ",

               @"
                    x = a;
                ",

               @"
                    y = b;
                ",

               @"
                    z = b;
                ",
                 
               @"
                    a = 10;
                ",
               @"
                    b = 20;
                ",
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();
            Guid guid5 = System.Guid.NewGuid();

            // Create and run the graph 
            ProtoScript.Runners.LiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid5, codes[4]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);


            // Modify [a = 1;] to [a = 10;] 
            // Modify [b = 2;] to [b = 20;] 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[5]));
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[6]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(liveRunner.Core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get the the preview guids (affected graphs)
            List<Guid> reachableGuidList = changeSetState.EstimateNodesAffectedByASTList(astList);

            // Check if the the affected guids are in the list
            List<Guid> expectedGuid = new List<Guid> { guid1, guid2, guid3, guid4, guid5 };
            AssertPreview(reachableGuidList, expectedGuid);
        }

        [Test]
        public void TestPreviewDepth01()
        {
            List<string> codes = new List<string>() 
            {
               @"
                    a = 1;
                ",
                 
               @"
                    x = a;
                ",

               @"
                    y = x;
                ",

               @"
                    a = 10;
                "
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create and run the graph  
            ProtoScript.Runners.LiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);


            // Modify [a = 1;] to [a = 10;] 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[3]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(liveRunner.Core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get the the preview guids (affected graphs)
            List<Guid> reachableGuidList = changeSetState.EstimateNodesAffectedByASTList(astList);

            // Check if the the affected guids are in the list
            List<Guid> expectedGuid = new List<Guid> { guid1, guid2, guid3 };
            AssertPreview(reachableGuidList, expectedGuid);
        }


        [Test]
        public void TestPreviewDepth02()
        {
            List<string> codes = new List<string>() 
            {
               @"
                    a = 1;
                ",
                 
               @"
                    x = a;
                ",

               @"
                    y = x;
                ",

               @"
                    z = y;
                ",

               @"
                    a = 10;
                "
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            // Create and run the graph  
            ProtoScript.Runners.LiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);


            // Modify [a = 1;] to [a = 10;] 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[4]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(liveRunner.Core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get the the preview guids (affected graphs)
            List<Guid> reachableGuidList = changeSetState.EstimateNodesAffectedByASTList(astList);

            // Check if the the affected guids are in the list
            List<Guid> expectedGuid = new List<Guid> { guid1, guid2, guid3, guid4 };
            AssertPreview(reachableGuidList, expectedGuid); 
        }



        /// <summary>
        /// Verifies that expectedGuidList is contained within previewGuidList
        /// </summary>
        /// <param name="previewGuidList"></param>
        /// <param name="expectedGuidList"></param>
        public static void AssertPreview(List<Guid> previewGuidList, List<Guid> expectedGuidList)
        {
            foreach (Guid expectedGuid in expectedGuidList)
            {
                Assert.IsTrue(previewGuidList.Contains(expectedGuid));
            }
        }
    }
}
