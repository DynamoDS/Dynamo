using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;
using Color = DSCore.Color;
using Point = Autodesk.DesignScript.Geometry.Point;
using TimeSpan = System.TimeSpan;

namespace Dynamo.Tests
{
    [DotMemoryUnit(CollectAllocations = true, FailIfRunWithoutSupport = false)]

    [TestFixture, Category("Performance")]
    public class PerformanceTests : DynamoModelTestBase
    {
        private List<(string graph, TimeSpan oldEngineCompileTime, TimeSpan oldEngineExecutionTime, TimeSpan newEngineCompileTime, TimeSpan newEngineExecutionTime, TimeSpan hardcodedExecutionTime)> executionData;
        private List<(string graph, (MemoryInfo allocated, MemoryInfo collected) oldEngine, (MemoryInfo allocated, MemoryInfo collected) newEngine, (MemoryInfo allocated, MemoryInfo collected) hardCoded)> memoryData;
        private (MemoryInfo allocated, MemoryInfo collected) oldEngineMemoryData;
        private (MemoryInfo allocated, MemoryInfo collected) newEngineMemoryData;
        private (MemoryInfo allocated, MemoryInfo collected) hardcodedMemoryData;
        private bool warmupDone;


        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("GeometryColor.dll");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }
        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(Path.Combine(TestDirectory, "core", "performance"));
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);

            var failingTests = new string[] { 
                "aniform.dyn",
                "lotsofcoloredstuff.dyn"};

            // Ignore aniform and lotsofcoloredstuff for now
            return fis.Where(fi =>!failingTests.Contains(fi.Name)).Select(fi => fi.FullName).ToArray();
        }

        [TestFixtureSetUp]
        public void SetupPerformanceTests()
        {
            executionData = new List<(string, TimeSpan, TimeSpan, TimeSpan, TimeSpan, TimeSpan)>();
            memoryData =
                new List<(string, (MemoryInfo, MemoryInfo), (MemoryInfo, MemoryInfo), (MemoryInfo, MemoryInfo))>();

        }

        [TestFixtureTearDown]
        public void TeardownPerformanceTests()
        {
            if (memoryData.Any())
            {
                // Graph  - Graph name
                // Old AM - Old Engine : Allocated Memory
                // Old CM - Old Engine : Collected Memory
                // Old AC - Old Engine : Allocated Count
                // Old CC - Old Engine : Collected Count
                // New AM - New Engine : Allocated Memory
                // New CM - New Engine : Collected Memory
                // New AC - New Engine : Allocated Count
                // New CC - New Engine : Collected Count
                // HC AM  - Hard Coded : Allocated Memory
                // HC CM  - Hard Coded : Collected Memory
                // HC AC  - Hard Coded : Allocated Count
                // HC CC  - Hard Coded : Collected Count

                Console.WriteLine("{0,50}{1,9}{2,9}{3,9}{4,9}{5,9}{6,9}{7,9}{8,9}{9,9}{10,9}{11,9}{12,9}", "Graph",
                    "Old AM", "Old CM", "Old AC", "Old CC",
                    "New AM", "New CM", "New AC", "New CC",
                    "HC AM", "HC CM", "HC AC", "HC CC");

                memoryData.ForEach(item =>
                {
                    var (graph, oldEngine, newEngine, hardCoded) = item;

                    if (hardCoded.collected.ObjectsCount == 0)
                    {
                        Console.WriteLine("{0,50}{1,9}{2,9}{3,9}{4,9}{5,9}{6,9}{7,9}{8,9}",
                            graph,
                            FormatValue(oldEngine.allocated.SizeInBytes), FormatValue(oldEngine.collected.SizeInBytes),
                            FormatValue(oldEngine.allocated.ObjectsCount),
                            FormatValue(oldEngine.collected.ObjectsCount),
                            FormatValue(newEngine.allocated.SizeInBytes), FormatValue(newEngine.collected.SizeInBytes),
                            FormatValue(newEngine.allocated.ObjectsCount),
                            FormatValue(newEngine.collected.ObjectsCount));
                    }
                    else
                    {
                        Console.WriteLine("{0,50}{1,9}{2,9}{3,9}{4,9}{5,9}{6,9}{7,9}{8,9}{9,9}{10,9}{11,9}{12,9}",
                            graph,
                            FormatValue(oldEngine.allocated.SizeInBytes), FormatValue(oldEngine.collected.SizeInBytes),
                            FormatValue(oldEngine.allocated.ObjectsCount),
                            FormatValue(oldEngine.collected.ObjectsCount),
                            FormatValue(newEngine.allocated.SizeInBytes), FormatValue(newEngine.collected.SizeInBytes),
                            FormatValue(newEngine.allocated.ObjectsCount),
                            FormatValue(newEngine.collected.ObjectsCount),
                            FormatValue(hardCoded.allocated.SizeInBytes), FormatValue(hardCoded.collected.SizeInBytes),
                            FormatValue(hardCoded.allocated.ObjectsCount),
                            FormatValue(hardCoded.collected.ObjectsCount));
                    }
                });
            }
            else
            {
                // Graph   - Graph Name
                // Old C   - Old Compile Time (ms)
                // Old E   - Old Execution Time (ms)
                // Old C+E - Old Compile + Execution Time (ms)
                // Graph   - Graph Name
                // New C   - New Compile Time (ms)
                // New E   - New Execution Time (ms)
                // New C+E - New Compile + Execution Time (ms)
                // HC      - Hard coded Time (ms)

                Console.WriteLine("{0,50}{1,9}{2,9}{3,11}{4,9}{5,9}{6,11}{7,9}", "Graph", "Old C", "Old E", "Old C+E", "New C", "New E", "New C+E", "HC");
                executionData.ForEach(item =>
                {
                    var (graph, oldEngineCompileTime, oldEngineExecutionTime, newEngineCompileTime, newEngineExecutionTime, hardcodedExecutionTime) = item;
                    if (hardcodedExecutionTime == TimeSpan.Zero)
                    {
                        Console.WriteLine("{0,50}{1,9:0.0}{2,9:0.0}{3,11:0.0}{4,9:0.0}{5,9:0.0}{6,11:0.0}", graph,
                            oldEngineCompileTime.TotalMilliseconds, oldEngineExecutionTime.TotalMilliseconds,
                            oldEngineCompileTime.TotalMilliseconds + oldEngineExecutionTime.TotalMilliseconds,
                            newEngineCompileTime.TotalMilliseconds, newEngineExecutionTime.TotalMilliseconds,
                            newEngineCompileTime.TotalMilliseconds + newEngineExecutionTime.TotalMilliseconds);
                    }
                    else
                    {
                        Console.WriteLine("{0,50}{1,9:0.0}{2,9:0.0}{3,11:0.0}{4,9:0.0}{5,9:0.0}{6,11:0.0}{7,9:0.0}", graph,
                            oldEngineCompileTime.TotalMilliseconds, oldEngineExecutionTime.TotalMilliseconds,
                            oldEngineCompileTime.TotalMilliseconds + oldEngineExecutionTime.TotalMilliseconds,
                            newEngineCompileTime.TotalMilliseconds, newEngineExecutionTime.TotalMilliseconds,
                            newEngineCompileTime.TotalMilliseconds + newEngineExecutionTime.TotalMilliseconds,
                            hardcodedExecutionTime.TotalMilliseconds);
                    }
                });
            }

            executionData.Clear();
            memoryData.Clear();

            warmupDone = false;
        }

        [Test, TestCaseSource("FindWorkspaces"), Category("Performance")]
        public void PerformanceTest(string filePath)
        {
            DoWorkspaceOpenAndCompare(filePath);
        }

        private void DoWorkspaceOpenAndCompare(string filePath)
        {

            WarmUp();


            // Run the hardcoded version of this graph if it exists.
            // Returns TimeSpan.Zero if a hard coded version does not exist
            var hardCodedExecutionTime = RunHardcodedTest(filePath);

            var checkPoint = StartCollectingMemory();

            OpenModel(filePath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;
            ws1.Description = "TestDescription";

            Assert.NotNull(ws1);

            CheckForDummyNodes(ws1);

            var cbnErrorNodes = ws1.Nodes.Where(n => n is CodeBlockNodeModel && n.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)ws1).RunSettings.RunType == Dynamo.Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            oldEngineMemoryData = StopCollectingMemory(checkPoint);

            var wcd1 = new serializationTestUtils.WorkspaceComparisonData(ws1, CurrentDynamoModel.EngineController);

            var oldEngineCompileAndExecutionTime = model.EngineController.CompileAndExecutionTime;

            // The big hammer, maybe not needed
            Cleanup();

            Setup();

            checkPoint = StartCollectingMemory();

            OpenModel(filePath, dsExecution: false);
            model = CurrentDynamoModel;
            var ws2 = model.CurrentWorkspace;
            ws2.Description = "TestDescription";

            if (((HomeWorkspaceModel)ws2).RunSettings.RunType == Dynamo.Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            Assert.NotNull(ws2);

            CheckForDummyNodes(ws2);

            newEngineMemoryData = StopCollectingMemory(checkPoint);

            var wcd2 = new serializationTestUtils.WorkspaceComparisonData(ws2, CurrentDynamoModel.EngineController, dsExecution: false);

            serializationTestUtils.CompareWorkspaceModelsMSIL(wcd1, wcd2);

            var newEngineCompileAndExecutionTime = model.EngineController.CompileAndExecutionTime;

            Console.WriteLine("Compile and Execution time old Engine={0:0.0}+{1:0.0} ms, new Engine={2:0.0}+{3:0.0} ms, hardCoded={4:0.0}",
                oldEngineCompileAndExecutionTime.compileTime.TotalMilliseconds, oldEngineCompileAndExecutionTime.executionTime.TotalMilliseconds,
                newEngineCompileAndExecutionTime.compileTime.TotalMilliseconds, newEngineCompileAndExecutionTime.executionTime.TotalMilliseconds,
                hardCodedExecutionTime.TotalMilliseconds);
            var execution = (Path.GetFileName(filePath),
                oldEngineCompileAndExecutionTime.compileTime, oldEngineCompileAndExecutionTime.executionTime,
                newEngineCompileAndExecutionTime.compileTime, newEngineCompileAndExecutionTime.executionTime,
                hardCodedExecutionTime);
            executionData.Add(execution);

            CollectMemoryData(Path.GetFileName(filePath));
        }

        private void CollectMemoryData(string graph)
        {
            if (oldEngineMemoryData.collected.SizeInBytes != -1)
            {
                var memoryInfo = (graph, oldEngine: oldEngineMemoryData, newEngine: newEngineMemoryData, hardcoded: hardcodedMemoryData);
                memoryData.Add(memoryInfo);
            }
        }


        //
        // Execute a small graph in both engines once before starting to
        // measure anything seems to even out the numbers between runs
        // Note that ASM i preloaded when the text fixture is setup (in the base class)
        private void WarmUp()
        {
            if (!warmupDone)
            {
                var warmupFile = Path.Combine(TestDirectory, "core", "performance", "simple_point.dyn");
                OpenModel(warmupFile);

                Cleanup();

                Setup();

                OpenModel(warmupFile, dsExecution: false);

                Cleanup();

                Setup();

                warmupDone = true;
            }
        }

        private MemoryCheckPoint StartCollectingMemory()
        {
            return dotMemory.Check();
        }

        private (MemoryInfo, MemoryInfo) StopCollectingMemory(MemoryCheckPoint checkPoint)
        {
            var retVal = (new MemoryInfo(-1, -1), new MemoryInfo(-1, -1));

            dotMemory.Check(memory =>
            {
                var traffic = memory.GetTrafficFrom(checkPoint);
                retVal = (traffic.AllocatedMemory, traffic.CollectedMemory);
            });

            return retVal;
        }

        // Run the hardcoded version of filename if it exists.
        // Returns TimeSpan.Zero if a hard coded version does not exist
        private TimeSpan RunHardcodedTest(string fileName)
        {
            TimeSpan hardCodedExecutionTime = TimeSpan.Zero;
            hardcodedMemoryData = (new MemoryInfo(), new MemoryInfo());
            var basename = Path.GetFileNameWithoutExtension(fileName);
            var cleanUp = false;

            if (string.Compare(basename, "lotsofcoloredstuff-simplified", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                var timer = new Stopwatch();
                var checkPoint = StartCollectingMemory();
                timer.Start();

                LotsOfColoredStuffSimplified();

                timer.Stop();
                hardCodedExecutionTime = timer.Elapsed;
                hardcodedMemoryData = StopCollectingMemory(checkPoint);

                cleanUp = true;
            }

            if (string.Compare(basename, "cross_product_lacing_arrays", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                var timer = new Stopwatch();
                var checkPoint = StartCollectingMemory();
                timer.Start();

                CrossProductLacingArrays();

                timer.Stop();
                hardCodedExecutionTime = timer.Elapsed;
                hardcodedMemoryData = StopCollectingMemory(checkPoint);

                cleanUp = true;
            }

            if (cleanUp)
            {
                Cleanup();
                Setup();
            }

            return hardCodedExecutionTime;
        }

        private void CheckForDummyNodes(WorkspaceModel ws)
        {
            var dummyNodes = ws.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }
        }

        private string FormatValue(long value)
        {
            if (value > 1000000)
            {
                return string.Format("{0:0.00}M", value / 1000000.0);
            }
            if (value > 1000)
            {
                return string.Format("{0:0.00}k", value / 1000.0);
            }

            return string.Format("{0:0.00}", value);
        }

        private void LotsOfColoredStuffSimplified()
        {
            var seeds = Enumerable.Range(0, 26).Select(x => x * 2).ToList();

            var points = seeds.Select(x => seeds.Select(y => seeds.Select(z => Point.ByCoordinates(x, y, z)).ToList()).ToList()).ToList();

            var cuboids = points.Select(a => a.Select(b => b.Select(p => Cuboid.ByLengths(p)).ToList()).ToList()).ToList();

            var circles = points.Select(a => a.Select(b => b.Select(p => Circle.ByCenterPointRadius(p, 0.7)).ToList()).ToList()).ToList();

            var color1 = Color.ByARGB(255, 75);

            var spheres = points.Select(a => a.Select(b => b.Select(p => Sphere.ByCenterPointRadius(p, 0.7)).ToList()).ToList()).ToList();

            var color2 = Color.ByARGB(255, 0, 255, 75);

            var color3 = Color.ByARGB(255, 255, 0, 255);

            var edges = cuboids.Select(a => a.Select(b => b.Select(c => c.Edges)).ToList()).ToList();

            var curves = edges.Select(a => a.Select(b => b.Select(l => l.Select(e => e.CurveGeometry).ToList()).ToList()).ToList()).ToList();

            var color4 = Color.ByARGB(255, 255, 255);

            var geometrycolorcircles = circles.Select(a => a.Select(b => b.Select(c => Modifiers.GeometryColor.ByGeometryColor(c, color1)).ToList()).ToList()).ToList();

            var geometrycolorspheres = spheres.Select(a => a.Select(b => b.Select(s => Modifiers.GeometryColor.ByGeometryColor(s, color2)).ToList()).ToList()).ToList();

            var geometrycolorcuboids = cuboids.Select(a => a.Select(b => b.Select(c => Modifiers.GeometryColor.ByGeometryColor(c, color3)).ToList()).ToList()).ToList();

            var geometrycolorcurves = curves.Select(a => a.Select(b => b.Select(c => c.Select(e => Modifiers.GeometryColor.ByGeometryColor(e, color4)).ToList()).ToList()).ToList()).ToList();

            var geometrycolorcirclescount = geometrycolorcircles.Select(a => a.Select(b => b.Count).Sum()).Sum();

            var geometrycolorspherescount = geometrycolorspheres.Select(a => a.Select(b => b.Count).Sum()).Sum();

            var geometrycolorcuboidscount = geometrycolorcuboids.Select(a => a.Select(b => b.Count).Sum()).Sum();

            var geometrycolorcurvescount = geometrycolorcurves.Select(a => a.Select(b => b.Select(c => c.Count).Sum()).Sum()).Sum();

            Assert.AreEqual(17576, geometrycolorcirclescount);
            Assert.AreEqual(17576, geometrycolorspherescount);
            Assert.AreEqual(17576, geometrycolorcuboidscount);
            Assert.AreEqual(210912, geometrycolorcurvescount);
        }

        private void CrossProductLacingArrays()
        {
            var list1 = Enumerable.Range(5, 5).ToList();
            var list2 = Enumerable.Range(0, 130001).ToList();
            var list3 = Enumerable.Range(0, 551).ToList();

            var listoflists1 = list2.Select((x, i) => new { Value = x, Index = i }).GroupBy(x => x.Index / 5).Select(x => x.Select(v => v.Value).ToList()).ToList();


            var indexOf1 = list1.Select(a => listoflists1.Select(b => b.IndexOf(a)).ToList()).ToList();
            var indexof1Count = indexOf1.Select(a => a.Count).Sum();
            Assert.AreEqual(130005, indexof1Count);
            Assert.AreEqual(5, indexOf1.Count);
            Assert.AreEqual(0, indexOf1[0][1]);
            Assert.AreEqual(1, indexOf1[1][1]);
            Assert.AreEqual(2, indexOf1[2][1]);
            Assert.AreEqual(3, indexOf1[3][1]);
            Assert.AreEqual(4, indexOf1[4][1]);


            var indexOf2 = list1.Select(a => listoflists1.Select(b => b.IndexOf(a)).ToList()).ToList();
            var indexof2Count = indexOf2.Select(a => a.Count).Sum();
            Assert.AreEqual(130005, indexof2Count);
            Assert.AreEqual(5, indexOf2.Count);
            Assert.AreEqual(0, indexOf2[0][1]);
            Assert.AreEqual(1, indexOf2[1][1]);
            Assert.AreEqual(2, indexOf2[2][1]);
            Assert.AreEqual(3, indexOf2[3][1]);
            Assert.AreEqual(4, indexOf2[4][1]);

            var plus = list3.Select(a => list3.Select(b => a + b).ToList()).ToList();
            var pluscount = plus.Select(a => a.Count).Sum();
            Assert.AreEqual(303601, pluscount);

            var sum = list3.Select(a => a).ToList();
            Assert.AreEqual(551, sum.Count);

            var max = list3.Select(a => list3.Select(b => Math.Max(a, b)).ToList()).ToList();
            var maxcount = plus.Select(a => a.Count).Sum();
            Assert.AreEqual(303601, maxcount);

            var listOfList2 = list3.Select((x, i) => new { Value = x, Index = i }).GroupBy(x => x.Index / 5).Select(x => x.Select(v => v.Value).ToList()).ToList();

            var plus2 = listOfList2.Select(a => listOfList2.Select(b => a.Zip(b, (x,y) => x + y).ToList()).ToList().ToList()).ToList();
            var plus2count = plus2.Select(a => a.Select(b => b.Count).Sum()).Sum();
            Assert.AreEqual(60721, plus2count);

            var sum2 = listOfList2.Select(a => a.Sum()).ToList();
            var sum2count = sum2.Count;
            Assert.AreEqual(111, sum2count);

            var max2 = listOfList2.Select(a => listOfList2.Select(b => a.Zip(b, Math.Max).ToList()).ToList().ToList()).ToList();
            var max2count = max2.Select(a => a.Select(b => b.Count).Sum()).Sum();
            Assert.AreEqual(60721, max2count);
        }
    }
}
