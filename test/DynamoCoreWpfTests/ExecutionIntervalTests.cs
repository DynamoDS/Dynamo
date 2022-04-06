using System.IO;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class ExecutionIntervalTests : DynamoViewModelUnitTest
    {
        [Test]
        [Category("Failure")]
        public void EvalTwiceAndCancel()
        {
            Assert.Inconclusive("To be fixed once Execution Interval is implemented.");

            var examplePath = Path.Combine(TestDirectory, @"core\executioninterval\");

            string openPath = Path.Combine(examplePath, "pause.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            int runCount = 0;

            ViewModel.Model.EvaluationCompleted += (sender, e) =>
            {
                if (!e.EvaluationTookPlace)
                    return;

                runCount++;
                if (runCount == 2)
                {
                    //ViewModel.Model.RunCancelInternal(false, true);
                    Assert.Pass();
                }
            };

            //ViewModel.Model.Runner.RunExpression(ViewModel.Model.HomeSpace, 2000);

            //If we reach this point, then the Assert.Pass() call was never reached 
            //and so ExecutionInterval is broken.
            Assert.Fail();
        }
    }
}
