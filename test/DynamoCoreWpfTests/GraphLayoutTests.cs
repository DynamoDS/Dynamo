using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Dynamo.Models;

namespace Dynamo.Tests
{
    [TestFixture, RequiresSTA]
    public class GraphLayoutTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        #region Graph Layout Tests

        [Test]
        public void GraphLayoutEmpty()
        {
            var x = ViewModel.CurrentSpace.X;
            var y = ViewModel.CurrentSpace.Y;
            var zoom = ViewModel.CurrentSpace.Zoom;

            ViewModel.DoGraphAutoLayout(null);

            Assert.IsNull(ViewModel.CurrentSpace.GraphLayoutGraph);

            Assert.Inconclusive("RequestZoomToFitView is null");

            Assert.Less(Math.Abs(ViewModel.CurrentSpace.X - x), 1);
            Assert.Less(Math.Abs(ViewModel.CurrentSpace.Y - y), 1);
            Assert.Less(Math.Abs(ViewModel.CurrentSpace.Y - y), 1);
        }

        [Test]
        public void GraphLayoutOneNode()
        {
            OpenModel(GetDynPath("GraphLayoutOneNode.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;

            var x = ViewModel.CurrentSpace.X;
            var y = ViewModel.CurrentSpace.Y;
            var zoom = ViewModel.CurrentSpace.Zoom;

            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 1 });

            Assert.AreEqual(nodes.ElementAt(0).X, 0);
            Assert.AreEqual(nodes.ElementAt(0).Y, 0);

            Assert.Inconclusive("RequestZoomToFitView is null");

            Assert.Greater(Math.Abs(ViewModel.CurrentSpace.X - x), 1);
            Assert.Greater(Math.Abs(ViewModel.CurrentSpace.Y - y), 1);
            Assert.Greater(Math.Abs(ViewModel.CurrentSpace.Y - y), 1);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutTwoConnectedNodes()
        {
            OpenModel(GetDynPath("GraphLayoutTwoConnectedNodes.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 1, 1 });

            Assert.AreEqual(nodes.ElementAt(0).Y, 0);
            Assert.AreEqual(nodes.ElementAt(1).X, 0);

            Assert.Greater(nodes.ElementAt(0).X, nodes.ElementAt(1).X);
            Assert.Less(nodes.ElementAt(0).Y, nodes.ElementAt(1).Y);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutIsolatedNodes()
        {
            OpenModel(GetDynPath("GraphLayoutIsolatedNodes.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 3 });

            Assert.AreEqual(nodes.ElementAt(0).X, 0);
            Assert.AreEqual(nodes.ElementAt(1).X, 0);
            Assert.AreEqual(nodes.ElementAt(2).X, 0);

            Assert.AreEqual(nodes.ElementAt(0).Y, 0);
            Assert.Greater(nodes.ElementAt(2).Y, nodes.ElementAt(0).Y);
            Assert.Greater(nodes.ElementAt(1).Y, nodes.ElementAt(2).Y);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutTree()
        {
            OpenModel(GetDynPath("GraphLayoutTree.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 1, 1, 2, 1, 1, 2, 2, 2, 1, 2 });

            Assert.AreEqual(nodes.ElementAt(5).X, 0);  // "put"

            Assert.AreEqual(nodes.ElementAt(8).X, 0);  // "come"
            Assert.Greater(nodes.ElementAt(8).Y, 0);

            Assert.Greater(nodes.ElementAt(3).X, 0);   // rightmost Watch node

            AssertNoOverlap();
            AssertMaxCrossings(2);
        }

        [Test]
        public void GraphLayoutCrossings()
        {
            OpenModel(GetDynPath("GraphLayoutCrossings.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 2, 2 });

            AssertNoOverlap();
            AssertMaxCrossings(5);
        }

        [Test]
        public void GraphLayoutSubgraphs()
        {
            OpenModel(GetDynPath("GraphLayoutSubgraphs.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 3, 5, 8, 3 });

            AssertNoOverlap();
            AssertMaxCrossings(4);
        }

        [Test]
        public void GraphLayoutCyclic()
        {
            OpenModel(GetDynPath("GraphLayoutCyclic.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 3, 7, 2, 2, 1, 1, 1 });

            AssertNoOverlap();
            AssertMaxCrossings(7);
        }

        [Test]
        public void GraphLayoutIsolatedOneGroup()
        {
            OpenModel(GetDynPath("GraphLayoutIsolatedOneGroup.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 2, 3, 5, 4 });

            AssertNoOverlap();
            AssertMaxCrossings(4);
        }

        [Test]
        public void GraphLayoutConnectedOneGroup()
        {
            OpenModel(GetDynPath("GraphLayoutConnectedOneGroup.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 1, 1, 2, 4, 1, 2, 3 });

            AssertNoOverlap();
            AssertMaxCrossings(6);
        }

        [Test]
        public void GraphLayoutConnectedGroups()
        {
            OpenModel(GetDynPath("GraphLayoutConnectedGroups.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 1, 1, 1 });

            AssertNoOverlap();
            AssertMaxCrossings(5);
        }

        [Test]
        public void GraphLayoutUnconnectedGroups()
        {
            OpenModel(GetDynPath("GraphLayoutUnconnectedGroups.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 3 });

            AssertNoOverlap();
            AssertMaxCrossings(4);
        }

        [Test]
        public void GraphLayoutCyclicGroups()
        {
            OpenModel(GetDynPath("GraphLayoutCyclicGroups.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertGraphLayoutLayers(new List<int> { 1, 1, 1 });

            AssertNoOverlap();
            AssertMaxCrossings(7);
        }

        #endregion

        private void AssertMaxCrossings(int maxCrossings)
        {
            GraphLayout.Graph g = ViewModel.CurrentSpace.GraphLayoutGraph;
            int crossings = 0;

            var list = ViewModel.CurrentSpace.Connectors;

            var combinations = list.Select((value, index) => new { value, index })
                .SelectMany(x => list.Skip(x.index + 1), (x, y) => Tuple.Create(x.value, y))
                .Where(x => !x.Item1.Start.Equals(x.Item2.Start));

            foreach (var pair in combinations)
            {
                var a = pair.Item1.Start.Center;
                var b = pair.Item1.End.Center;
                var c = pair.Item2.Start.Center;
                var d = pair.Item2.End.Center;

                double denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
                double numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
                double numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

                // Detect coincident lines (has a problem, read below)
                if (denominator == 0 && numerator1 == 0 && numerator2 == 0)
                {
                    crossings++;
                }
                else
                {
                    double r = numerator1 / denominator;
                    double s = numerator2 / denominator;

                    if ((r >= 0 && r <= 1) && (s >= 0 && s <= 1))
                    {
                        crossings++;
                    }
                }
            }

            if (crossings > maxCrossings)
            {
                Assert.Fail("Number of edge crossings is " + crossings +
                    ", more than the specified max " + maxCrossings);
            }
        }

        private void AssertNoOverlap()
        {
            GraphLayout.Graph g = ViewModel.CurrentSpace.GraphLayoutGraph;

            foreach (var a in g.Nodes)
            {
                foreach (var b in g.Nodes)
                {
                    if (!a.Equals(b) && 
                        (((a.X <= b.X) && (a.Y <= b.Y) && (b.X - a.X <= a.Width) && (b.Y - a.Y <= a.Height)) ||
                        ((a.X >= b.X) && (a.Y <= b.Y) && (a.X - b.X <= b.Width) && (b.Y - a.Y <= a.Height))))
                    {
                        Assert.Fail("Node overlap");
                    }
                }
            }
        }

        private void AssertGraphLayoutLayers(List<int> layerCount)
        {
            GraphLayout.Graph g = ViewModel.CurrentSpace.GraphLayoutGraph;
            bool same = layerCount.SequenceEqual(g.Layers.Select(layer => layer.Count).ToList());
            Assert.IsTrue(same);
        }

        private string GetDynPath(string sourceDynFile)
        {
            string sourceDynPath = TestDirectory;
            sourceDynPath = Path.Combine(sourceDynPath, @"core\GraphLayout\");
            return Path.Combine(sourceDynPath, sourceDynFile);
        }

    }
}
