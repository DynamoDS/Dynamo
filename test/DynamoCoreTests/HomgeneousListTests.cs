using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CoreNodeModels;
using Dynamo.Events;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class HomogeneousListTests : DynamoModelTestBase
    {
        private TimeSpan lastExecutionDuration = new TimeSpan();

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;
        }

        [Test]
        public void TestMethodResolutionPerformance()
        {

            //Run once to complete initialization

            RunModel(@"core\HomogeneousList\HomogeneousInputs.dyn");

            var timeHeterogeneousFirst = new List<double>();
            for (int i = 0; i < 50; i++)
            {

                RunModel(@"core\HomogeneousList\HeterogeneousInputsFirst.dyn");

                timeHeterogeneousFirst.Add(lastExecutionDuration.TotalMilliseconds);
            }

            var timeHeterogeneousLast = new List<double>();
            for (int i = 0; i < 50; i++)
            {

                RunModel(@"core\HomogeneousList\HeterogeneousInputsLast.dyn");

                timeHeterogeneousLast.Add(lastExecutionDuration.TotalMilliseconds);
            }

            var timeHomogeneous = new List<double>();
            for (int i = 0; i < 50; i++)
            {  
                RunModel(@"core\HomogeneousList\HomogeneousInputs.dyn");

                timeHomogeneous.Add(lastExecutionDuration.TotalMilliseconds);

            }

            var homogeneousStdDev = CalculateStdDev(timeHomogeneous);
            var heterogeneousFirstStdDev = CalculateStdDev(timeHeterogeneousFirst);
            var heterogeneousLastStdDev = CalculateStdDev(timeHeterogeneousLast);

            var aveHomogeneous = TrimRange(timeHomogeneous, homogeneousStdDev, 2).Average();
            var aveHeterogeneousFirstItem = TrimRange(timeHeterogeneousFirst, heterogeneousFirstStdDev, 2).Average();
            var aveHeterogeneousLastItem = TrimRange(timeHeterogeneousLast, heterogeneousLastStdDev, 2).Average();

            Assert.LessOrEqual(aveHomogeneous, aveHeterogeneousFirstItem);
            Assert.LessOrEqual(aveHomogeneous, aveHeterogeneousLastItem);

            Console.WriteLine("Homogeneous average execution: " + aveHomogeneous + "ms");
            Console.WriteLine("Heterogeneous first item average execution: " + aveHeterogeneousFirstItem + "ms");
            Console.WriteLine("Heterogeneous last item average execution: " + aveHeterogeneousLastItem + "ms");

        }

        [Test]
        public void TestMethodResolutionforHomogeneousListInputs()
        {

            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");

            AssertPreviewValue("e3aabcc0-3d94-425b-af4e-a3171baaa78a",
                new object[]
                {
                    new object[] { 1, 3, 4 },
                    new object[] { 2, 4, 5 },
                    new object[] { 3, 5, 6 }
                });
        }

        [Test]
        public void TestMethodResolutionforHeterogeneousListInputs()
        {

            RunModel(@"core\HomogeneousList\HeterogeneousInputsValid.dyn");

            AssertPreviewValue("e3aabcc0-3d94-425b-af4e-a3171baaa78a",
                new object[]
                {
                    new object[] { 1.5, 3, 4 },
                    new object[] { 2.5, 4, 5 },
                    new object[] { 3.5, 5, 6 }
                });
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {

            double avg = values.Average();    
            double sum = values.Sum(d => Math.Pow(d - avg, 2));  
            var ret = Math.Sqrt((sum) / (values.Count() - 1));
   
            return ret;
        }

        private List<double> TrimRange(IEnumerable<double> values, double stdDev, int multiplier = 1)
        {
            double avg = values.Average();
            var newList =
                values.Where(value => value <= avg + stdDev * multiplier && value >= avg - stdDev * multiplier);
            return newList.ToList();
        }

        private void ExecutionEvents_GraphPostExecution(Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Session.ParameterKeys.LastExecutionDuration);
        }
    }
}
