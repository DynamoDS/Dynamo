using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Utilities;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSEvaluationTest : DynamoUnitTest
    {
        private void RunModel(string relativeDynFilePath)
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), relativeDynFilePath);
            model.Open(openPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));
        }

        private void AssertValue(string guid, object value)
        {
            var model = dynSettings.Controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);

            RuntimeMirror mirror = null;
            string var = node.VariableToPreview;
            Assert.DoesNotThrow(() => mirror = EngineController.Instance.GetMirror(var));
            Assert.IsNotNull(mirror);

            Console.WriteLine(var + " = " + mirror.GetStringData());
            StackValue svValue = mirror.GetData().GetStackValue();
            if (value is double)
            {
                Assert.AreEqual(svValue.opdata_d, Convert.ToDouble(value));
            }
            else if (value is int)
            {
                Assert.AreEqual(svValue.opdata, Convert.ToInt64(value));
            }
            else if (value is IEnumerable<int>)
            {
                var values = (value as IEnumerable<int>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(var, values, typeof(Int64)));
            }
            else if (value is IEnumerable<double>)
            {
                var values = (value as IEnumerable<double>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(var, values, typeof(double)));
            }
        }

        [Test]
        public void TestCodeBlockNode01()
        {
            RunModel(@"core\dsevaluation\cbn_nolhs.dyn");
            AssertValue("4e88b4a9-397a-422e-be13-f9ffcb27bc75", new int[] { 2, 3, 4, 5, 6});
        }
    }
}