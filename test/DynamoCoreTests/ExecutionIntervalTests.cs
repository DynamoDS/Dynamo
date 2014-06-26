using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Dynamo.Tests;
using NUnit.Framework;

namespace Dynamo
{
    class ExecutionIntervalTests : DynamoUnitTest
    {
        [Test]
        public void EvalTwiceAndCancel()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\executioninterval\");

            string openPath = Path.Combine(examplePath, "pause.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            int runCount = 0;

            Controller.EvaluationCompleted += delegate
            {
                runCount++;
                if (runCount == 2)
                {
                    Controller.RunCancelInternal(false, true);
                    Assert.Pass();
                }
            };

            Controller.RunExpression(2000);

            //If we reach this point, then the Assert.Pass() call was never reached 
            //and so ExecutionInterval is broken.
            Assert.Fail();
        }
    }
}
