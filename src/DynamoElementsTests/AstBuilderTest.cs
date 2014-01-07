using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class AstBuilderTest: DynamoUnitTest
    {
        private class ShuffleUtil<T>
        {
            private Random random;
            private List<T> list;

            public List<T> ShuffledList
            {
                get
                {
                    list = list.OrderBy(i => random.Next()).ToList();
                    return list;
                }
            }

            public ShuffleUtil(List<T> list)
            {
                random = new Random();
                this.list = list;
            }
        }

        [Test]
        public void TestCompileToAstNodes1()
        {
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\complex.dyn");
            model.Open(openPath);

            AstBuilder builder = new AstBuilder(null);
            var astNodes = builder.CompileToAstNodes(model.CurrentWorkspace.Nodes, false);
            string code = GraphToDSCompiler.GraphUtilities.ASTListToCode(astNodes);
            Console.WriteLine(code);
        }

        [Test]
        public void TestSortNode1()
        {
            // The connections of CBNs are
            // 
            //  1 <----> 2
            //
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\cyclic.dyn");
            model.Open(openPath);

            var builder = new AstBuilder(null);

            var sortedNodes = builder.TopologicalSort(model.CurrentWorkspace.Nodes);
            Assert.AreEqual(sortedNodes.Count(), 2);
        }

        [Test]
        public void TestSortNode2()
        {
            // The connections of CBNs are
            // 
            //      +----> 2
            //     /
            //   1 ----> 3
            //     \    
            //      +----> 4
            // 
            var model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\multioutputs.dyn");
            model.Open(openPath);
            var nodes = model.CurrentWorkspace.Nodes.ToList();

            var builder = new AstBuilder(null);
            int shuffleCount = 10;
            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = builder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<int> nickNames = sortedNodes.Select(node => Int32.Parse(node.NickName)).ToList();

                Dictionary<int, int> nodePosMap = new Dictionary<int, int>();
                for (int idx = 0; idx < nickNames.Count; ++idx)
                {
                    nodePosMap[nickNames[idx]] = idx;
                }

                // no matter input nodes in whatever order, there invariants 
                // should hold
                Assert.IsTrue(nodePosMap[2] > nodePosMap[1]);
                Assert.IsTrue(nodePosMap[3] > nodePosMap[1]);
                Assert.IsTrue(nodePosMap[4] > nodePosMap[1]);
            }
        }

        [Test]
        public void TestSortNode3()
        {
            // The connections of CBNs are
            // 
            //   1 ----+ 
            //          \
            //   2 ----> 4
            //          /
            //   3 ----+
            // 
            var model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\multiinputs.dyn");
            model.Open(openPath);
            var nodes = model.CurrentWorkspace.Nodes.ToList();

            var builder = new AstBuilder(null);
            int shuffleCount = 10;
            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = builder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<int> nickNames = sortedNodes.Select(node => Int32.Parse(node.NickName)).ToList();

                Dictionary<int, int> nodePosMap = new Dictionary<int, int>();
                for (int idx = 0; idx < nickNames.Count; ++idx)
                {
                    nodePosMap[nickNames[idx]] = idx;
                }

                // no matter input nodes in whatever order, there invariants 
                // should hold
                Assert.IsTrue(nodePosMap[1] < nodePosMap[4]);
                Assert.IsTrue(nodePosMap[2] < nodePosMap[4]);
                Assert.IsTrue(nodePosMap[3] < nodePosMap[4]);
            }
        }

        [Test]
        public void TestSortNode4()
        {
            // The connections of CBNs are
            //   
            //  +---------------+    
            //  |               |
            //  |               v
            //  2 ----> 3 ----> 1
            // 
            var model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\tri.dyn");
            model.Open(openPath);
            var nodes = model.CurrentWorkspace.Nodes.ToList();

            var builder = new AstBuilder(null);
            int shuffleCount = 10;
            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = builder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 3);

                List<int> nickNames = sortedNodes.Select(node => Int32.Parse(node.NickName)).ToList();

                Dictionary<int, int> nodePosMap = new Dictionary<int, int>();
                for (int idx = 0; idx < nickNames.Count; ++idx)
                {
                    nodePosMap[nickNames[idx]] = idx;
                }

                // no matter input nodes in whatever order, there invariants 
                // should hold
                Assert.IsTrue(nodePosMap[2] < nodePosMap[3]);
                Assert.IsTrue(nodePosMap[3] < nodePosMap[1]);
            }
        }


        [Test]
        public void TestSortNode5()
        {
            // The connections of CBNs are
            //   
            // 1 <---- 2 <----> 3 <---- 4
            //
            var model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\linear.dyn");
            model.Open(openPath);
            var nodes = model.CurrentWorkspace.Nodes.ToList();

            var builder = new AstBuilder(null);
            int shuffleCount = 10;
            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = builder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<int> nickNames = sortedNodes.Select(node => Int32.Parse(node.NickName)).ToList();

                Dictionary<int, int> nodePosMap = new Dictionary<int, int>();
                for (int idx = 0; idx < nickNames.Count; ++idx)
                {
                    nodePosMap[nickNames[idx]] = idx;
                }

                // no matter input nodes in whatever order, there invariants 
                // should hold
                Assert.IsTrue(nodePosMap[4] < nodePosMap[3]);
                Assert.IsTrue(nodePosMap[4] < nodePosMap[2]);
                Assert.IsTrue(nodePosMap[3] < nodePosMap[1]);
                Assert.IsTrue(nodePosMap[2] < nodePosMap[1]);
            }
        }

        [Test]
        public void TestSortNode6()
        {
            // The connections of CBNs are
            //
            //                   1
            //                   ^
            //                   |
            //                   2
            //                   ^
            //                   |
            //  6 <---- 4 <----> 3 <----> 5 ----> 7          8 <----> 9
            // 
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\complex.dyn");
            model.Open(openPath);

            var nodes = model.CurrentWorkspace.Nodes.ToList();
            int shuffleCount = 10;
            var shuffle = new ShuffleUtil<NodeModel>(nodes);
            var builder = new AstBuilder(null);

            for (int i = 0; i < shuffleCount; ++i)
            {
                nodes = shuffle.ShuffledList;
                var unsortedNickNames = nodes.Select(node => Int32.Parse(node.NickName)).ToList();

                var sortedNodes = builder.TopologicalSort(nodes).ToList();
                Assert.AreEqual(sortedNodes.Count(), 9);

                var nickNames = sortedNodes.Select(node => Int32.Parse(node.NickName)).ToList();
                var nodePosMap = new Dictionary<int, int>();
                for (int idx = 0; idx < nickNames.Count; ++idx)
                {
                    nodePosMap[nickNames[idx]] = idx;
                }

                // no matter input nodes in whatever order, there invariants 
                // should hold
                Assert.IsTrue(nodePosMap[1] > nodePosMap[2]);
                Assert.IsTrue(nodePosMap[6] > nodePosMap[4]);
                Assert.IsTrue(nodePosMap[7] > nodePosMap[5]);
            }
        }
    }
}
