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
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            dynSettings.Controller.RunExpression(null);
            var add = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DSFunction>("ccb2eda9-0966-4ab8-a186-0d5f844559c1");
            Assert.AreEqual(20, add.CachedValue.Data);
        }

        [Test]
        public void Defect_MAGN_2566()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2566
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_2566.dyn");
            RunModel(openPath);

             // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            //dynSettings.Controller.RunExpression(null);

            // Checking Point.X
            AssertPreviewValue("eea90465-db68-4494-a85e-4d7c687b68e6", 0);

            // Checking Sphere.Radius
            AssertPreviewValue("18dbc746-18a4-4e4e-839b-14523e648f79", 1);

            // Checking Cylinder.Radius
            AssertPreviewValue("ca9b1501-bc63-49f4-af2d-8d4b4aab2e80", new int[] { 1, 1 });

            // Checking Cylinder.Height
            AssertPreviewValue("310b22ae-e3ce-462a-8391-c7e0c9a532f2", new int[] { 10, 1 });

        }

        [Test]
        public void Defect_MAGN_3256()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3256
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3256.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            dynSettings.Controller.RunExpression(null);

            AssertPreviewValue("21780859-f85e-44ff-bedb-bd016ca7398d", 5.706339);

        }

        [Test]
        public void Defect_MAGN_3646()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3646
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3646.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);

            dynSettings.Controller.RunExpression(null);

            AssertPreviewValue("d12c17f4-2f73-42fa-9990-7fe9a723e6a1", 0.00001);
            AssertPreviewValue("d12c17f4-2f73-42fa-9990-7fe9a723e6a1", 0.0000000000000001);
        }

        [Test]
        public void Defect_MAGN_3648()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3648

            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3648.dyn");

            OpenModel(openPath);

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            //Check the CBN for input and output ports count and for error as well.
            string nodeID = "e378c03e-4aae-4bde-b4c5-f16a6bed358f";
            var cbn = model.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            Assert.DoesNotThrow(() => Controller.RunExpression(null));

            AssertPreviewValue(nodeID, 1);

        }

        [Test]
        public void Defect_MAGN_3726()
        {
            //Detail steps are here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3726
            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3726.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            dynSettings.Controller.RunExpression(null);

            AssertPreviewValue("02985f61-2ece-4fe2-b78a-dfb21aa589ff",
                new string[] { "0a", "10a", "20a", "30a", "40a", "50a" });
        }
    }
}
