using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.FSchemeInterop;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using String = System.String;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class ListTests : DynamoUnitTest
    {
        [Test]
        public void TestExcel()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestExcelGetDataWorksheet.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            //check input value
            var node1 = model.CurrentWorkspace.NodeFromWorkspace("32758a26-ef68-4f1e-9b7c-e6f9a580b86d");
            Assert.NotNull(node1);

            //get watch node
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("0be2e8a3-3eae-48c6-b789-79c3978f9417");
            var doubleWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            var firstLine = (doubleWatchVal[0]).GetListFromFSchemeValue();
            Assert.AreEqual(1.0, firstLine[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestCombineNode_ListNumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestCombineNode_ListNumberRange.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);

            //check input value
            var node1 = model.CurrentWorkspace.NodeFromWorkspace("7a91fd07-2ff5-4438-a077-4a36f1cb1802");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("4970acd9-8f33-4aac-b7a5-382f8a96d5d3");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("b9399521-0680-4f78-99b1-2bf12300ef27");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("8fda00fc-14b5-4416-8be5-5e947858e3fd");
            Assert.NotNull(node4);
            var node5 = model.CurrentWorkspace.NodeFromWorkspace("6fb9e407-9a30-43b4-a5a8-0ec75f6b53fb");
            Assert.NotNull(node5);
            var node6 = model.CurrentWorkspace.NodeFromWorkspace("8a4ebd23-e223-469f-873b-a310f0274ee5");
            Assert.NotNull(node6);
            var node7 = model.CurrentWorkspace.NodeFromWorkspace("98474d52-4111-4b3a-86c1-33b0a6640df7");
            Assert.NotNull(node7);
            var node8 = model.CurrentWorkspace.NodeFromWorkspace("43ef0449-d695-4e9f-8488-8e61d7f87a18");
            Assert.NotNull(node8);
            var node9 = model.CurrentWorkspace.NodeFromWorkspace("7924ed34-c4e8-43ea-bb25-0e7f4d5825d1");
            Assert.NotNull(node9);
            var node10 = model.CurrentWorkspace.NodeFromWorkspace("5858ee20-5b48-487a-be6d-fff6bdb46fca");
            Assert.NotNull(node10);
            var node11 = model.CurrentWorkspace.NodeFromWorkspace("87da809e-e141-4d50-bbac-daf73a8fe844");
            Assert.NotNull(node11);
            var node12 = model.CurrentWorkspace.NodeFromWorkspace("36e44dd2-0439-40c2-aa05-c799d838355e");
            Assert.NotNull(node12);

            //get watch node
            //var watch = GetWatchNodeFromCurrentSpace(model, "0be2e8a3-3eae-48c6-b789-79c3978f9417");
            //var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            //Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void TestCombineNode_EdgeCase()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\CombineNode_EdgeCase.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("c645474c-1d23-4acb-8b47-1a19d5f2e3e2");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("43920f71-05fb-4fe5-ae41-a9975320e641");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("7305cc0c-d246-4e87-8715-a1a55cbb0205");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("a3bd8f70-810b-4ad4-a0b9-e6322cd19bd1");
            Assert.NotNull(node4);
            var node5 = model.CurrentWorkspace.NodeFromWorkspace("0a58611d-8b58-4840-9ffd-9f45e928ca76");
            Assert.NotNull(node5);
            var node6 = model.CurrentWorkspace.NodeFromWorkspace("093b94ea-3b09-4979-9897-708b8f82ba11");
            Assert.NotNull(node6);
        }

        [Test]
        public void TestTrueForAny()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TrueForAny.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("b9bc33d1-bd81-4b1b-bd11-65f817fbb3ca");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("5ae10a50-5909-493f-a2b4-0ca826a83258");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("ad9e1311-b371-468e-ac37-2cde3a1c3280");
            Assert.NotNull(node3);
        }

        [Test]
        public void TestTrueForAll()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TrueForAll.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("c09be09c-6f17-4ad4-9831-109475761db1");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("b9bc33d1-bd81-4b1b-bd11-65f817fbb3ca");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("21d2f43e-92c4-4dd1-8f61-d17d1805c74d");
            Assert.NotNull(node3);
        }

        [Test]
        public void TestSplitList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_NumberSequence.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("bdc0a740-19c6-4cbb-bf20-7a571f13d739");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("652deea7-09b2-48f6-8aa4-255f6276b71c");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("878ccd8d-2244-4fab-a35f-c8cae8858ee7");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("59632dd2-3bac-467d-b007-460333dd6012");
            Assert.NotNull(node4);
        }

        [Test]
        public void TestAddToList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestAddToList.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("b2fa6459-fa13-481e-96fc-9d4bc46d787e");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("e2a1f641-2935-4cac-a8b3-d8856c9e695a");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("7f3cae9a-6ea6-4708-a8a9-a1b9993117d4");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("4e26cd44-de84-44c0-a623-09e2499c3fe3");
            Assert.NotNull(node4);


            var node5 = model.CurrentWorkspace.NodeFromWorkspace("f6a1bac3-3374-40f4-81ab-aedb82eee515");
            Assert.NotNull(node5);
            var doubleWatchVal = node5.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(150.0, doubleWatchVal[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestTakeFromList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestTakeFromList.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("58de2589-5439-465d-8368-046eb87a4d51");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("8c535cc9-3dff-4715-a030-673db2bb8d4f");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("1fa87ad4-3cd8-4289-a09e-84ef0efe539d");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("133952fa-f225-425e-972a-c76fdd2c2577");
            Assert.NotNull(node4);


            var node5 = model.CurrentWorkspace.NodeFromWorkspace("8caa44c1-0a87-4902-993a-bb7e4883ed9f");
            Assert.NotNull(node5);
            var doubleWatchVal = node5.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(10000000.0, doubleWatchVal[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestDropFromList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestDropFromList.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("122ff3aa-0df1-4988-b7bd-3737768305bc");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("5ceed865-90aa-4811-881c-9df1157da5ef");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("79bacd0e-f39d-444d-8d1a-e393b4a90146");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("0292e6ca-1e02-48bb-9f4e-e434ceb28d05");
            Assert.NotNull(node4);


            var node5 = model.CurrentWorkspace.NodeFromWorkspace("4cc45fd3-8b4a-43dc-866b-1e0bd7b50308");
            Assert.NotNull(node5);
            var watchList = node5.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(1022350.0, watchList[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestShiftIndeces()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestShiftIndeces.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("054884b8-393b-42d0-bf3a-af7178ba0c86");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("d29f90e3-1c9f-4ce9-a6a8-33fce81399ad");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("25ff103e-c78d-4277-9372-7c93cf53ee6c");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("24aead4d-73f6-40d5-a71b-e92f308714dd");
            Assert.NotNull(node4);


            var node5 = model.CurrentWorkspace.NodeFromWorkspace("705393f3-9eca-42c6-9e77-91bd0d781452");
            Assert.NotNull(node5);

            //var doubleWatchVal = GetListFromFSchemeValue(node5.GetValue(0));
            var doubleWatchVal = node5.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(285.0, doubleWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(288.0, doubleWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(291.0, doubleWatchVal[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(294.0, doubleWatchVal[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(297.0, doubleWatchVal[4].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestGetFromList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestGetFromList.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("2031df04-9fba-435c-b991-94df4f76f634");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("d223a746-757f-4212-b2bb-9406e6c1feb4");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("310dc80a-349f-4e30-9319-dd1edc71eb53");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("9605ee13-8dec-48d2-ba24-adaaf232ef0b");
            Assert.NotNull(node4);

            var node5 = model.CurrentWorkspace.NodeFromWorkspace("8fc7ce00-182e-4ad0-ae0a-9d0058d2f0f3");
            Assert.NotNull(node5);
            Assert.AreEqual(2000000099, node5.GetValue(0).GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestRemoveFromList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestRemoveFromList.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("9b5f59a8-43ad-4388-9f1d-c141cc7afc83");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("d223a746-757f-4212-b2bb-9406e6c1feb4");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("310dc80a-349f-4e30-9319-dd1edc71eb53");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("9605ee13-8dec-48d2-ba24-adaaf232ef0b");
            Assert.NotNull(node4);

            var node5 = model.CurrentWorkspace.NodeFromWorkspace("8fc7ce00-182e-4ad0-ae0a-9d0058d2f0f3");
            Assert.NotNull(node5);
            FSharpList<FScheme.Value> listWatchVal = node5.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2000000100, listWatchVal[99].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestDropEveryNth()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestDropEveryNth.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("d1dba071-81e0-4946-8711-833fb8c6a61c");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("d859cf13-d326-4947-bfad-395ead326160");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("4f51caa4-9dcc-4f89-a52e-4f1dfc644771");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("7a8dd3f4-ce34-485e-9fc9-0e1ec49892dc");
            Assert.NotNull(node4);

            var node5 = model.CurrentWorkspace.NodeFromWorkspace("7a33aa54-c891-4551-a3c8-98f8a1e95bb1");
            Assert.NotNull(node5);
            FSharpList<FScheme.Value> listWatchVal = node5.GetValue(0).GetListFromFSchemeValue();
            //FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(node5.GetValue(0));
            //var doubleVar = GetDoubleFromFSchemeValue(node5.GetValue(0));
            Assert.AreEqual(120000, listWatchVal[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestTakeEveryNth()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestTakeEveryNth.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("2ccbe80b-d0f9-4c5d-884b-260ea5c2336b");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("7ac8f293-30df-416a-ae27-9fa9a122b775");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("c9ac7caf-8dc4-4096-b6d4-a33d89fb41a2");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("ca1af682-d2fa-466e-9e4e-ef2b60e9bcdf");
            Assert.NotNull(node4);

            var node5 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("a0cefe95-4fa6-44c4-b829-e39d228233f0");
            Assert.NotNull(node5);
            FSharpList<FScheme.Value> listWatchVal = node5.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(100150.0, listWatchVal[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestIsEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestEmpty.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("d1dba071-81e0-4946-8711-833fb8c6a61c");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("d859cf13-d326-4947-bfad-395ead326160");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("4f51caa4-9dcc-4f89-a52e-4f1dfc644771");
            Assert.NotNull(node3);
            var node4 = model.CurrentWorkspace.NodeFromWorkspace("7a8dd3f4-ce34-485e-9fc9-0e1ec49892dc");
            Assert.NotNull(node4);

            var node5 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fbaef2c4-380f-4c60-b2ed-c0c644ee0ce9");
            Assert.NotNull(node5);
            Assert.AreEqual(1.0, node5.GetValue(0).GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestSplitList_edge()
        {
            var model = dynSettings.Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TestSplitList.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

            var node1 = model.CurrentWorkspace.NodeFromWorkspace("67f11474-4143-4069-bd1a-90105e432ac8");
            Assert.NotNull(node1);
            var node2 = model.CurrentWorkspace.NodeFromWorkspace("30ef2258-2c62-43f8-a1f9-00d1ae340f34");
            Assert.NotNull(node2);
            var node3 = model.CurrentWorkspace.NodeFromWorkspace("e225d5a9-2216-470b-b5fb-fd2967ff2131");
            Assert.NotNull(node3);

            var node5 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("5c320c9e-cc38-47b1-8cab-e6ce643c31db");
            Assert.NotNull(node5);
            Assert.AreEqual(0.0, node5.GetValue(0).GetDoubleFromFSchemeValue());
        }
    }
}