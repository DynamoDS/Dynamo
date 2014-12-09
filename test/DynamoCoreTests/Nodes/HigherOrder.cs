using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Dynamo.Models;
using Dynamo.Tests;

using NUnit.Framework;

namespace Dynamo.Tests
{
    public class HigherOrder : DSEvaluationViewModelUnitTest
    {
        string TestFolder { get { return GetTestDirectory(); } }

        [Test]
        public void ComposeOrder()
        {
            string testFilePath = Path.Combine(TestFolder, "core", "compose", "compose_order.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("1ac93e79-9793-44d4-a6cc-c898d47e7ba8", 75.0);
        }
        [Test]
        public void LoopWhile()
        {
            //Test Loop while 
            string testFilePath = Path.Combine(TestFolder, "core", "compose", "Sum_LoopWhile.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("b4c8c4a9-464c-41c2-91ee-db7a350b611a", 5.0);
        }
        [Test]
        public void LoopWhile_Fibonaci()
        {
            // Test Loop while - Complex case using custom node 
            string testFilePath = Path.Combine(TestFolder, "core", "compose", "LoopWhile_Fibonacci.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("07dd69e8-5354-4c69-8e4c-69725bc9883f", new int[] { 0, 1, 1, 2, 3, 5, 8, 13});
        }
        
    }
}
