using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class AstBuilderTest : DynamoModelTestBase
    {
        private const int shuffleCount = 10;

        private class ShuffleUtil<T>
        {
            private readonly Random random;
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
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\complex.dyn");
            OpenModel(openPath);

            var builder = new AstBuilder(null, null);
            var astNodes = builder.CompileToAstNodes(CurrentDynamoModel.CurrentWorkspace.Nodes, CompilationContext.None, false);
            var codeGen = new ProtoCore.CodeGenDS(astNodes.SelectMany(t => t.Item2));
            string code = codeGen.GenerateCode();
            Console.WriteLine(code);
        }

        [Test]
        public void TestSortNode1()
        {
            // The connections of CBNs are
            // 
            //  1 <----> 2
            //
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\cyclic.dyn");
            OpenModel(openPath);

            var sortedNodes = AstBuilder.TopologicalSort(CurrentDynamoModel.CurrentWorkspace.Nodes);
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
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\multioutputs.dyn");
            OpenModel(openPath);

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);
            var id1 = "654c9a4b-3f58-4f90-9bde-c3e615100b12";
            var id2 = "8a55ca22-4424-4ccb-bd2f-3fe79bbeaccf";
            var id3 = "189689be-815b-4a6e-89cb-45b495962cca";
            var id4 = "918ac1ed-98fa-457d-bdb7-fe7456ea3fb5";

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id2] > nodePosMap[id1]);
                Assert.IsTrue(nodePosMap[id3] > nodePosMap[id1]);
                Assert.IsTrue(nodePosMap[id4] > nodePosMap[id1]);
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
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\multiinputs.dyn");
            OpenModel(openPath);

            var id1 = "1a00d6cb-f67d-4b79-a810-94145ad31486";
            var id2 = "8188eb2e-1746-4513-a51e-1dc8dfdb08e6";
            var id3 = "e5ef9374-389b-45d4-8ca3-44d52276a5cc";
            var id4 = "c342aed0-eea6-463d-b55f-ae260b0e5320";


            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id1] < nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id4]);
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
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\tri.dyn");
            OpenModel(openPath);

            var id1 = "2e7200b5-7962-450b-91a8-9b0b35eeed6d";
            var id2 = "a5b713c3-1969-4cb4-a6f4-b7a299c46d7f";
            var id3 = "91dbf72b-c2ac-4e87-b52a-cf9a0ebb3ec5";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 3);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id3]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id1]);
            }
        }


        [Test]
        public void TestSortNode5()
        {
            // The connections of CBNs are
            //   
            // 1 <---- 2 <----> 3 <---- 4
            //
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\linear.dyn");
            OpenModel(openPath);
            var id1 = "cc78b400-329a-4bd8-be9b-5dd6593b31c0";
            var id2 = "306e67f3-d97c-4be7-aab5-298c8c24ae7b";
            var id3 = "682f2b08-0f11-4248-8b98-a5aff29e5fdf";
            var id4 = "34786660-d4a5-4643-8d08-317eb16ed377";


            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSort(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id4] < nodePosMap[id3]);
                Assert.IsTrue(nodePosMap[id4] < nodePosMap[id2]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id1]);
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id1]);
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
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\complex.dyn");
            OpenModel(openPath);


            var id1 = "c7d2e0f5-78db-486a-b20b-1ee451bf48d5";
            var id2 = "014c3d64-078e-4f92-b582-79387862e306";
            var id3 = "7744fddd-450d-422f-abd7-32c568e7ee19";
            var id5 = "bc513069-7825-4f15-bb3e-fa76fa7c84a9";
            var id7 = "9afc84ab-e0d2-46c6-839f-cef34175d96b";
            var id4 = "98c54e4f-9118-4c67-a248-2926ed3516ea";
            var id6 = "6c50a8a1-4e7d-4146-9cfc-1b33de51abea";
            var id8 = "dd095dd4-1d06-4c29-b6ff-afbc853266cf";
            var id9 = "d0185e93-7d0f-47a4-a56c-4486bb2b6877";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                nodes = shuffle.ShuffledList;

                var sortedNodes = AstBuilder.TopologicalSort(nodes).ToList();
                Assert.AreEqual(sortedNodes.Count(), 9);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();
                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id1] > nodePosMap[id2]);
                Assert.IsTrue(nodePosMap[id6] > nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id7] > nodePosMap[id5]);
            }
        }

        [Test]
        public void TestTopologicalSortForGraph1()
        {
            // The connections of CBNs are
            // 
            //  1 <----> 2
            //
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\cyclic.dyn");
            OpenModel(openPath);

            var sortedNodes = AstBuilder.TopologicalSortForGraph(CurrentDynamoModel.CurrentWorkspace.Nodes);
            Assert.AreEqual(sortedNodes.Count(), 2);
        }

        [Test]
        public void TestTopologicalSortForGraph2()
        {
            // The connections of CBNs are
            // 
            //      +----> 2
            //     /
            //   1 ----> 3
            //     \    
            //      +----> 4
            // 
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\multioutputs.dyn");
            OpenModel(openPath);

            var id1 = "654c9a4b-3f58-4f90-9bde-c3e615100b12";
            var id2 = "8a55ca22-4424-4ccb-bd2f-3fe79bbeaccf";
            var id3 = "189689be-815b-4a6e-89cb-45b495962cca";
            var id4 = "918ac1ed-98fa-457d-bdb7-fe7456ea3fb5";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSortForGraph(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id2] > nodePosMap[id1]);
                Assert.IsTrue(nodePosMap[id3] > nodePosMap[id1]);
                Assert.IsTrue(nodePosMap[id4] > nodePosMap[id1]);
            }
        }

        [Test]
        public void TestTopologicalSortForGraph3()
        {
            // The connections of CBNs are
            // 
            //   1 ----+ 
            //          \
            //   2 ----> 4
            //          /
            //   3 ----+
            // 
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\multiinputs.dyn");
            OpenModel(openPath);

            var id1 = "1a00d6cb-f67d-4b79-a810-94145ad31486";
            var id2 = "8188eb2e-1746-4513-a51e-1dc8dfdb08e6";
            var id3 = "e5ef9374-389b-45d4-8ca3-44d52276a5cc";
            var id4 = "c342aed0-eea6-463d-b55f-ae260b0e5320";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSortForGraph(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id1] < nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id1] < nodePosMap[id2]);
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id3]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id4]);
            }
        }

        [Test]
        public void TestTopologicalSortForGraph4()
        {
            // The connections of CBNs are
            //   
            //  +---------------+    
            //  |               |
            //  |               v
            //  2 ----> 3 ----> 1
            // 
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\tri.dyn");
            OpenModel(openPath);

            var id1 = "2e7200b5-7962-450b-91a8-9b0b35eeed6d";
            var id2 = "a5b713c3-1969-4cb4-a6f4-b7a299c46d7f";
            var id3 = "91dbf72b-c2ac-4e87-b52a-cf9a0ebb3ec5";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSortForGraph(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 3);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id3]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id1]);
            }
        }


        [Test]
        public void TestTopologicalSortForGraph5()
        {
            // The connections of CBNs are
            //   
            // 1 <---- 2 <----> 3 <---- 4
            //
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\linear.dyn");
            OpenModel(openPath);

            var id1 = "cc78b400-329a-4bd8-be9b-5dd6593b31c0";
            var id2 = "306e67f3-d97c-4be7-aab5-298c8c24ae7b";
            var id3 = "682f2b08-0f11-4248-8b98-a5aff29e5fdf";
            var id4 = "34786660-d4a5-4643-8d08-317eb16ed377";


            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                var sortedNodes = AstBuilder.TopologicalSortForGraph(shuffle.ShuffledList).ToList();
                Assert.AreEqual(sortedNodes.Count(), 4);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();

                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id4] < nodePosMap[id3]);
                Assert.IsTrue(nodePosMap[id4] < nodePosMap[id2]);
                Assert.IsTrue(nodePosMap[id3] < nodePosMap[id1]);
                Assert.IsTrue(nodePosMap[id2] < nodePosMap[id1]);
            }
        }

        [Test]
        public void TestTopologicalSortForGraph6()
        {
            // The connections of CBNs are
            //
            //                   1
            //                   ^
            //                   |
            //                   2
            //                   ^
            //                   |
            //  6 <---- 4 <----> 3 <----> 5 ----> 7          8 ----> 9
            // 
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\complex.dyn");
            OpenModel(openPath);

            var id1 = "c7d2e0f5-78db-486a-b20b-1ee451bf48d5";
            var id2 = "014c3d64-078e-4f92-b582-79387862e306";
            var id3 = "7744fddd-450d-422f-abd7-32c568e7ee19";
            var id5 = "bc513069-7825-4f15-bb3e-fa76fa7c84a9";
            var id7 = "9afc84ab-e0d2-46c6-839f-cef34175d96b";
            var id4 = "98c54e4f-9118-4c67-a248-2926ed3516ea";
            var id6 = "6c50a8a1-4e7d-4146-9cfc-1b33de51abea";
            var id8 = "dd095dd4-1d06-4c29-b6ff-afbc853266cf";
            var id9 = "d0185e93-7d0f-47a4-a56c-4486bb2b6877";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                nodes = shuffle.ShuffledList;

                var sortedNodes = AstBuilder.TopologicalSortForGraph(nodes).ToList();
                Assert.AreEqual(sortedNodes.Count(), 9);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();
                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }

                // no matter the order of the input nodes, these invariants 
                // should hold
                Assert.IsTrue(nodePosMap[id1] > nodePosMap[id2]);
                Assert.IsTrue(nodePosMap[id6] > nodePosMap[id4]);
                Assert.IsTrue(nodePosMap[id7] > nodePosMap[id5]);
                Assert.IsTrue(nodePosMap[id5] > nodePosMap[id3]
                    || nodePosMap[id2] > nodePosMap[id3]
                    || nodePosMap[id4] > nodePosMap[id3]);
                Assert.IsTrue(nodePosMap[id9] > nodePosMap[id8]);
            }
        }

        [Test]
        public void TestTopologicalSortForGraph7()
        {
            // The connections of CBNs are
            //
            //     1 ----> 2 ----> 3 ----+
            //                           |
            //     4 ----> 5 ----> 6 ----+
            //                           |
            //                           +----> 13
            //                           |
            //     7 ----> 8 ----> 9 ----+
            //                           |
            //    10 ---> 11 ---->12 ----+
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\multiinputs2.dyn");
            OpenModel(openPath);

            var id1 = "746381f5-8821-4d0d-8858-a77b128c19db";
            var id4 = "80b9d424-420f-4666-aecd-19cfdaf43410";
            var id7 = "5cf48cb3-6c08-403d-aba2-b1054a9dd703";
            var id2 = "a4c2a32b-6a4f-483b-8e42-43ae263936e1";
            var id5= "4e51d487-f2fb-4ac7-aa5b-75249a5d138f";
            var id8 ="8a8809ef-529b-4e13-ab0f-e8762163856a";
            var id3 ="c0a4a701-2b61-491c-bd59-2f59196728f0";
            var id9 ="0a340c37-1e30-442e-b935-cc4d4816ed95";
            var id6 = "be3b4b36-4f42-485c-b346-c8e60e70b81a";
            var id13 ="3ba2305e-d151-4974-b42e-b5ae5a9a8ffb";
            var id10 ="afb714d8-74a5-4946-95d5-1b3a4d4e750b";
            var id12 = "a0b819a9-0077-4fcf-a2fa-29615d86a52c";
            var id11 = "9dcf916c-827c-446b-bb01-457561fe591b";

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList();

            var shuffle = new ShuffleUtil<NodeModel>(nodes);

            for (int i = 0; i < shuffleCount; ++i)
            {
                nodes = shuffle.ShuffledList;

                var sortedNodes = AstBuilder.TopologicalSortForGraph(nodes).ToList();
                Assert.AreEqual(sortedNodes.Count(), 13);

                List<string> ids = sortedNodes.Select(node => node.GUID.ToString()).ToList();
                var nodePosMap = new Dictionary<string, int>();
                for (int idx = 0; idx < ids.Count; ++idx)
                {
                    nodePosMap[ids[idx]] = idx;
                }
                var orderedResult = new List<string> { id1, id2, id3, id4, id5, id6, id7, id8, id9, id10, id11, id12, id13 };
                Assert.IsTrue(ids.SequenceEqual(orderedResult));
            }
        }
    }
}
