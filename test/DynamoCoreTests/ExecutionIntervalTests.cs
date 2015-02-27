using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Dynamo.Tests;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class ExecutionIntervalTests : DynamoViewModelUnitTest
    {
        [Test]
        [Category("Failure")]
        public void EvalTwiceAndCancel()
        {
            Assert.Inconclusive("To be fixed once Execution Interval is implemented.");

            var examplePath = Path.Combine(GetTestDirectory(), @"core\executioninterval\");

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
