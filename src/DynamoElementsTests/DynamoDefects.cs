using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Tests
{
    [TestFixture]
    class DynamoDefects : DynamoUnitTest
    {
        // Note: Pelase only add test cases those are related to defects.

        [Test]
        public void T01_Defect_MAGN_110()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_110.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("339dd778-8d2c-4ae2-9fdc-26c1572f8eb6");
            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, actual.Length);

            Assert.AreEqual(1, actual[2].GetDoubleFromFSchemeValue());
        }
    }
}
