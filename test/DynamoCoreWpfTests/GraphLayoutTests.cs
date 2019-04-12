using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Selection;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture, RequiresSTA]
    public class GraphLayoutTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        #region Graph Layout Tests

        [Test]
        public void GraphLayoutMarkFileAsDirty()
        {
            // A graph with one node
            OpenModel(GetDynPath("GraphLayoutOneNode.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;

            Assert.IsFalse(ViewModel.CurrentSpace.HasUnsavedChanges);
            ViewModel.DoGraphAutoLayout(null);
            Assert.IsFalse(ViewModel.CurrentSpace.HasUnsavedChanges);

            // A graph with two nodes
            OpenModel(GetDynPath("GraphLayoutTwoConnectedNodes.dyn"));
            
            Assert.IsFalse(ViewModel.CurrentSpace.HasUnsavedChanges);
            ViewModel.DoGraphAutoLayout(null);
            Assert.IsTrue(ViewModel.CurrentSpace.HasUnsavedChanges);
        }

        [Test]
        public void GraphLayoutOneNode()
        {
            OpenModel(GetDynPath("GraphLayoutOneNode.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;

            var prevX = nodes.ElementAt(0).X;
            var prevY = nodes.ElementAt(0).Y;

            ViewModel.DoGraphAutoLayout(null);

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 1);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 0);

            Assert.AreEqual(nodes.ElementAt(0).X, prevX);
            Assert.AreEqual(nodes.ElementAt(0).Y, prevY);
        }

        [Test]
        public void GraphLayoutTwoConnectedNodes()
        {
            OpenModel(GetDynPath("GraphLayoutTwoConnectedNodes.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 2);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 1);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 1 }
            }, subgraphs);

            Assert.Greater(nodes.ElementAt(0).X, nodes.ElementAt(1).X);
            Assert.Less(nodes.ElementAt(0).Y, nodes.ElementAt(1).Y);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutIsolatedNodes()
        {
            OpenModel(GetDynPath("GraphLayoutIsolatedNodes.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 3);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 0);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1 },
                new int[] { 0, 1 },
                new int[] { 0, 1 }
            }, subgraphs);

            Assert.Greater(nodes.ElementAt(2).Y, nodes.ElementAt(0).Y);
            Assert.Greater(nodes.ElementAt(1).Y, nodes.ElementAt(2).Y);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutTree()
        {
            OpenModel(GetDynPath("GraphLayoutTree.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 15);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 17);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 1, 2, 1, 1, 2, 2, 2, 1, 2 }
            }, subgraphs);

            var prevX = (nodes.Min(n => n.X) + nodes.Max(n => n.X + n.Width)) / 2;
            var prevY = (nodes.Min(n => n.Y) + nodes.Max(n => n.Y + n.Height)) / 2;

            AssertNoOverlap();
            AssertMaxCrossings(5);

            Assert.Less(Math.Abs((nodes.Min(n => n.X) + nodes.Max(n => n.X + n.Width)) / 2 - prevX), 10);
            Assert.Less(Math.Abs((nodes.Min(n => n.Y) + nodes.Max(n => n.Y + n.Height)) / 2 - prevY), 10);
        }

        [Test]
        public void GraphLayoutTreeSelection()
        {
            OpenModel(GetDynPath("GraphLayoutTree.dyn"));
            List<NodeModel> nodes = ViewModel.CurrentSpace.Nodes.ToList();

            // Select 4 nodes
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(1));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(5));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(6));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(8));

            ViewModel.DoGraphAutoLayout(null);

            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 3, 1, 1, 2 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(3);

            // Deselect all
            DynamoSelection.Instance.Selection.ToList().ForEach(n => n.Deselect());
            DynamoSelection.Instance.Selection.Clear();

            // Reselect 6 nodes
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(4));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(7));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(9));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(11));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(12));
            SelectModel(ViewModel.CurrentSpace.Nodes.ElementAt(13));

            subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 2, 1, 2, 2, 1 }
            }, subgraphs);
        }

        [Test]
        public void GraphLayoutCrossings()
        {
            OpenModel(GetDynPath("GraphLayoutCrossings.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 4);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 6);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 2, 2 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(5);
        }

        [Test]
        public void GraphLayoutSubgraphs()
        {
            OpenModel(GetDynPath("GraphLayoutSubgraphs.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 19);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 20);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 1, 2, 3 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(4);
        }

        [Test]
        public void GraphLayoutCyclic()
        {
            OpenModel(GetDynPath("GraphLayoutCyclic.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 17);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 21);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 3, 2, 4, 2, 1, 2 }
            }, subgraphs);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutIsolatedOneGroup()
        {
            OpenModel(GetDynPath("GraphLayoutIsolatedOneGroup.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 19);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 20);
            Assert.AreEqual(ViewModel.CurrentSpace.Annotations.Count(), 1);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 1, 2, 3 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(4);

            // Select the group and re-run graph layout
            SelectModel(ViewModel.CurrentSpace.Annotations.First());
            subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 2, 3 }
            }, subgraphs);
        }

        [Test]
        public void GraphLayoutConnectedOneGroup()
        {
            OpenModel(GetDynPath("GraphLayoutConnectedOneGroup.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 19);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 23);
            Assert.AreEqual(ViewModel.CurrentSpace.Annotations.Count(), 1);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 1, 2, 4, 1, 2, 3 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(6);

            // Select the group and re-run graph layout
            SelectModel(ViewModel.CurrentSpace.Annotations.First());
            subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 1, 1, 2, 3 }
            }, subgraphs);
        }

        [Test]
        public void GraphLayoutConnectedGroups()
        {
            OpenModel(GetDynPath("GraphLayoutConnectedGroups.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 19);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 23);
            Assert.AreEqual(ViewModel.CurrentSpace.Annotations.Count(), 3);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 1, 1 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(5);

            // Select the leftmost group and re-run graph layout
            SelectModel(ViewModel.CurrentSpace.Annotations.First());
            subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 2, 1, 2, 3 }
            }, subgraphs);

            // Now select two groups and re-run graph layout
            // The two groups should be two different subgraphs
            SelectModel(ViewModel.CurrentSpace.Annotations.ElementAt(1));
            subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 2, 1, 2, 3 },
                new int[] { 1, 1, 2, 3 }
            }, subgraphs);
        }

        [Test]
        public void GraphLayoutUnconnectedGroups()
        {
            OpenModel(GetDynPath("GraphLayoutUnconnectedGroups.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 19);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 20);
            Assert.AreEqual(ViewModel.CurrentSpace.Annotations.Count(), 3);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1 },
                new int[] { 0, 1 },
                new int[] { 0, 1 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(4);

            // Select two groups and re-run graph layout
            SelectModel(ViewModel.CurrentSpace.Annotations.First());
            SelectModel(ViewModel.CurrentSpace.Annotations.Last());
            subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 1, 2, 3 }
            }, subgraphs);
        }

        [Test]
        public void GraphLayoutCyclicGroups()
        {
            OpenModel(GetDynPath("GraphLayoutCyclicGroups.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 19);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 24);
            Assert.AreEqual(ViewModel.CurrentSpace.Annotations.Count(), 3);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 1, 1 }
            }, subgraphs);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutComplex()
        {
            OpenModel(GetDynPath("GraphLayoutComplex.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 82);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 112);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 4, 5, 6, 9, 8, 5, 2, 4, 5, 2, 3,
                    7, 7, 1, 1, 2, 4, 3, 2, 2 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(200);
        }

        [Test]
        public void GraphLayoutDyf()
        {
            OpenModel(GetDynPath("GraphLayout.dyf"));
            ViewModel.CurrentWorkspaceIndex = 1;
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.Model is CustomNodeWorkspaceModel);

            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            var subgraphs = ViewModel.CurrentSpace.DoGraphAutoLayout();

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count(), 10);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 13);
            AssertGraphLayoutLayers(new object[] {
                new int[] { 0, 1, 1, 2, 2, 2, 2 }
            }, subgraphs);

            AssertNoOverlap();
            AssertMaxCrossings(1);
        }

        [Test]
        public void GraphLayoutNoteModels()
        {
            OpenModel(GetDynPath("GraphLayoutNotes.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;
            ViewModel.DoGraphAutoLayout(null);

            AssertNoOverlap();
        }

        [Test]
        public void GraphLayoutGroupedNotes()
        {
            // for http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-9216
            OpenModel(GetDynPath("GraphLayoutGroupedNotes.dyn"));
            IEnumerable<NodeModel> nodes = ViewModel.CurrentSpace.Nodes;

            SelectModel(ViewModel.CurrentSpace.Annotations.First());
            ViewModel.DoGraphAutoLayout(null);
        }

        #endregion

        private void AssertMaxCrossings(int maxCrossings)
        {
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
            var models = ViewModel.CurrentSpace.Nodes.Concat<ModelBase>(ViewModel.CurrentSpace.Notes);

            foreach (var a in models)
            {
                foreach (var b in models)
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

        private void AssertGraphLayoutLayers(object[] subgraphLayerCount, List<GraphLayout.Graph> layoutSubgraphs)
        {
            for (int i = 0; i < subgraphLayerCount.Length; i++)
            {
                GraphLayout.Graph g = layoutSubgraphs.ElementAt(i + 1);

                if (!g.Layers.Select(layer => layer.Count).AsEnumerable()
                    .SequenceEqual(subgraphLayerCount[i] as IEnumerable<int>))
                {
                    Assert.Fail(String.Format("Layout subgraph [{0}] should be {{ {1} }} but is actually {{ {2} }}",
                        i, String.Join(", ", subgraphLayerCount[i] as IEnumerable<int>),
                        String.Join(", ", g.Layers.Select(layer => layer.Count))));
                }
            }
        }

        private string GetDynPath(string sourceDynFile)
        {
            string sourceDynPath = TestDirectory;
            sourceDynPath = Path.Combine(sourceDynPath, @"core\GraphLayout\");
            return Path.Combine(sourceDynPath, sourceDynFile);
        }

        private void SelectModel(ISelectable model)
        {
            DynamoSelection.Instance.Selection.Add(model);
        }

    }
}
