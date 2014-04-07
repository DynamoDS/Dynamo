using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Models;
using System.Collections.Generic;

namespace Dynamo.Tests
{
    [TestFixture]
    class DynamoDefects : DSEvaluationUnitTest
    {
        // Note: Pelase only add test cases those are related to defects.

        [Test]
        public void T01_Defect_MAGN_110()
        {
            Assert.Inconclusive("Porting : AngleInput");

            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_110.dyn");
            RunModel(openPath);
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {2,1},
            };

            SelectivelyAssertPreviewValues("339dd778-8d2c-4ae2-9fdc-26c1572f8eb6", validationData);
        }

        [Test]
        public void Defect_MAGN_942_Equal()
        {
            Assert.Inconclusive("Porting : Formula");

            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_Equal.dyn");
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {5,1},
                {10,0},          
            };

            SelectivelyAssertPreviewValues("3806c656-56bd-4878-9082-b2d27644abd1", validationData);
        }

        [Test]
        public void Defect_MAGN_942_GreaterThan()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_GreaterThan.dyn");
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {1,0},
                {7,1},    
                {9,1},
            };

            SelectivelyAssertPreviewValues("7ec8271d-be03-4d53-ae78-b94c4db484e1", validationData);

        }

        [Test]
        public void Defect_MAGN_942_GreaterThanOrEqual()
        {
            Assert.Inconclusive("Porting : Formula");

            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_GreaterThanOrEqual.dyn");
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {2,0},
                {4,0},
                {6,1},    
                {8,1},
            };

            SelectivelyAssertPreviewValues("3806c656-56bd-4878-9082-b2d27644abd1", validationData);

        }
        
        [Test]
        public void Defect_MAGN_942_LessThan()
        {
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_LessThan.dyn");
            RunModel(openPath);

            AssertPreviewValue("7ec8271d-be03-4d53-ae78-b94c4db484e1", new int[] { 1, 1, 1, 1, 1 });
        }

        [Test]
        public void Defect_MAGN_942_LessThanOrEqual()
        {
            Assert.Inconclusive("Porting : Formula");

            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_942_LessThanOrEqual.dyn");
            RunModel(openPath);

            AssertPreviewValue("6adba162-ef10-4664-9c9c-d0280a56a52a", new int[] { 0, 1, 1, 1, 0, 1 });

        }

        [Test]
        public void Defect_MAGN_1206()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1206
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_1206.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            var add = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DSFunction>("ccb2eda9-0966-4ab8-a186-0d5f844559c1");
            Assert.AreEqual(20, add.OldValue.Data);
        }

    }
}
