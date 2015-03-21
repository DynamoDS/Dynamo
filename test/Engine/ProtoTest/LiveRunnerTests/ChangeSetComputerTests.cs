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
    class ChangeSetComputerTests : ProtoTestBase
    {
        private ProtoScript.Runners.LiveRunner CreateLiveRunner()
        {
            return new ProtoScript.Runners.LiveRunner();
        }

        [SetUp]
        public override void Setup()
        {
            // Create a dummy runtimeCore because these tests dont really need to execute 
            // The base class will expect to cleanup the runtimeCore
            base.Setup();
            runtimeCore = new RuntimeCore(new Heap());
        }

        private Subtree CreateSubTreeFromCode(Guid guid, string code)
        {
            var cbn = ProtoCore.Utils.ParserUtils.Parse(code) as CodeBlockNode;
            var subtree = null == cbn ? new Subtree(null, guid) : new Subtree(cbn.Body, guid);
            return subtree;
        }

        [Test]
        public void TestAddedNodes01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
         
            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get expected ASTList
            // The list must be in the order that it is expected
            List<string> expectedCode = new List<string>() 
            {
                "a = 1;"
            };
            List<AssociativeNode> expectedAstList = ProtoCore.Utils.CoreUtils.BuildASTList(core, expectedCode);

            // Compare ASTs to be equal
            for (int n = 0; n < astList.Count; ++n)
            {
                AssociativeNode node1 = astList[n];
                AssociativeNode node2 = expectedAstList[n];
                bool isEqual = node1.Equals(node2);
                Assert.IsTrue(isEqual);
            }
        }

        [Test]
        public void TestAddedNodes02()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1; b = a;"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get expected ASTList
            // The list must be in the order that it is expected
            List<string> expectedCode = new List<string>() 
            {
                "a = 1;",
                "b = a;"
            };
            List<AssociativeNode> expectedAstList = ProtoCore.Utils.CoreUtils.BuildASTList(core, expectedCode);

            // Compare ASTs to be equal
            for (int n = 0; n < astList.Count; ++n)
            {
                AssociativeNode node1 = astList[n];
                AssociativeNode node2 = expectedAstList[n];
                bool isEqual = node1.Equals(node2);
                Assert.IsTrue(isEqual);
            }
        }

        [Test]
        public void TestAddedFunction01()
        {
            List<string> codes = new List<string>() 
            {
                "def f(){return = 1;}"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get expected ASTList
            // The list must be in the order that it is expected
            List<string> expectedCode = new List<string>() 
            {
                "def f(){return = 1;}"
            };
            List<AssociativeNode> expectedAstList = ProtoCore.Utils.CoreUtils.BuildASTList(core, expectedCode);

            // Compare ASTs to be equal
            for (int n = 0; n < astList.Count; ++n)
            {
                AssociativeNode node1 = astList[n];
                AssociativeNode node2 = expectedAstList[n];
                bool isEqual = node1.Equals(node2);
                Assert.IsTrue(isEqual);
            }
        }

        [Test]
        public void TestAddedFunction02()
        {
            List<string> codes = new List<string>() 
            {
                "def f(){return = 1;}",
                "def g(){return = 1;}"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Get expected ASTList
            // The list must be in the order that it is expected
            List<string> expectedCode = new List<string>() 
            {
                "def f(){return = 1;}",
                "def g(){return = 1;}"
            };
            List<AssociativeNode> expectedAstList = ProtoCore.Utils.CoreUtils.BuildASTList(core, expectedCode);

            // Compare ASTs to be equal
            for (int n = 0; n < astList.Count; ++n)
            {
                AssociativeNode node1 = astList[n];
                AssociativeNode node2 = expectedAstList[n];
                bool isEqual = node1.Equals(node2);
                Assert.IsTrue(isEqual);
            }
        }

        [Test]
        public void TestModified01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = 1;"
            };

            // Add node
            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Modify contents
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            astList = changeSetState.GetDeltaASTList(syncData);

            // Get expected ASTList
            // The list must be in the order that it is expected
            List<string> expectedCode = new List<string>() 
            {
                "b = 1;"
            };
            List<AssociativeNode> expectedAstList = ProtoCore.Utils.CoreUtils.BuildASTList(core, expectedCode);

            // Compare ASTs to be equal
            for (int n = 0; n < astList.Count; ++n)
            {
                AssociativeNode node1 = astList[n];
                AssociativeNode node2 = expectedAstList[n];
                bool isEqual = node1.Equals(node2);
                Assert.IsTrue(isEqual);
            }
        }

        [Test]
        public void TestModified02()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1; b = 1;",
                "c = 1;"
            };

            // Add nodes a = 1, b = 1
            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);

            // Get astlist from ChangeSetComputer
            ChangeSetComputer changeSetState = new ProtoScript.Runners.ChangeSetComputer(core);
            List<AssociativeNode> astList = changeSetState.GetDeltaASTList(syncData);

            // Modify contents to c = 1
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);

            // Get astlist from ChangeSetComputer
            astList = changeSetState.GetDeltaASTList(syncData);

            // Get expected ASTList
            // The list must be in the order that it is expected
            List<string> expectedCode = new List<string>() 
            {
                "c = 1;"
            };
            List<AssociativeNode> expectedAstList = ProtoCore.Utils.CoreUtils.BuildASTList(core, expectedCode);

            // Compare ASTs to be equal
            for (int n = 0; n < astList.Count; ++n)
            {
                AssociativeNode node1 = astList[n];
                AssociativeNode node2 = expectedAstList[n];
                bool isEqual = node1.Equals(node2);
                Assert.IsTrue(isEqual);
            }
        }
    }
}
