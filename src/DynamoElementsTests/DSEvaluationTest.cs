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

        private string GetVarName(string guid)
        {
            var model = dynSettings.Controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return  node.VariableToPreview;
        }

        private RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = EngineController.Instance.GetMirror(varName));
            Assert.IsNotNull(mirror);
            return mirror;
        }

        private void AssertValue(string guid, object value)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);

            Console.WriteLine(varname + " = " + mirror.GetStringData());
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
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(Int64)));
            }
            else if (value is IEnumerable<double>)
            {
                var values = (value as IEnumerable<double>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(double)));
            }
        }

        private void AssertIsPointer(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);

            StackValue svValue = mirror.GetData().GetStackValue();
            Assert.IsTrue(StackUtils.IsValidPointer(svValue));
        }

        [Test]
        public void TestCodeBlockNode01()
        {
            // (1..5) + 1;
            RunModel(@"core\dsevaluation\cbn_nolhs.dyn");
            AssertValue("4e88b4a9-397a-422e-be13-f9ffcb27bc75", new int[] { 2, 3, 4, 5, 6});
        }

        [Test]
        public void Regress561()
        {
            // 1; ----> x
            // 2; ----> y Point.ByCoordinates(x, y, z);
            // 3; ----> z
            RunModel(@"core\dsevaluation\regress561.dyn");
            AssertIsPointer("8774296c-5269-450b-959d-ce4020ddbf80");
        }
    }
}