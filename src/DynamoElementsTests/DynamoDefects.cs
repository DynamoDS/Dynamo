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

        [Test]
        public void Defect_MAGN_942_Equal()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_Equal.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var equal = model.CurrentWorkspace.NodeFromWorkspace<Map>("3806c656-56bd-4878-9082-b2d27644abd1");
            FSharpList<FScheme.Value> actual = equal.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(11, actual.Length);

            Assert.AreEqual(0, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[5].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, actual[10].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void Defect_MAGN_942_GreaterThan()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_GreaterThan.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var equal = model.CurrentWorkspace.NodeFromWorkspace<Map>("7ec8271d-be03-4d53-ae78-b94c4db484e1");
            FSharpList<FScheme.Value> actual = equal.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(11, actual.Length);

            Assert.AreEqual(0, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, actual[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[7].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[9].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Defect_MAGN_942_GreaterThanOrEqual()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_GreaterThanOrEqual.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var equal = model.CurrentWorkspace.NodeFromWorkspace<Map>("3806c656-56bd-4878-9082-b2d27644abd1");
            FSharpList<FScheme.Value> actual = equal.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(11, actual.Length);

            Assert.AreEqual(0, actual[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, actual[4].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[6].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[8].GetDoubleFromFSchemeValue());

        }


        [Test]
        public void Defect_MAGN_942_LessThan()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_LessThan.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var equal = model.CurrentWorkspace.NodeFromWorkspace<Map>("7ec8271d-be03-4d53-ae78-b94c4db484e1");
            FSharpList<FScheme.Value> actual = equal.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, actual.Length);

            Assert.AreEqual(1, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[4].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Defect_MAGN_942_LessThanOrEqual()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_LessThanOrEqual.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var equal = model.CurrentWorkspace.NodeFromWorkspace<Map>("6adba162-ef10-4664-9c9c-d0280a56a52a");
            FSharpList<FScheme.Value> actual = equal.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, actual.Length);

            Assert.AreEqual(0, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, actual[4].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[5].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void Defect_MAGN_1206()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1206
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_1206.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var add = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.Addition>("ccb2eda9-0966-4ab8-a186-0d5f844559c1");
            Assert.AreEqual(20, add.OldValue.GetDoubleFromFSchemeValue());
        }

    }
}
