using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Tests;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class ListTests: DynamoUnitTest
    {

        #region Sort Test Cases

        [Test]
        public void Sort_NumbersfFromDiffInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Sort_NumbersfFromDiffInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // fourth and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("de6bd134-55d1-4fb8-a605-1c486b5acb5f");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(1, listWatchVal[4].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, listWatchVal[7].GetDoubleFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("25ee495f-2d8e-4fa5-8180-6d0e45eb4675");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal2.Length);
            Assert.AreEqual(-3.76498800959146, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal2[7].GetDoubleFromFSchemeValue());
        }


        [Test]
        public void Sort_SimpleNumbers()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Sort_SimpleNumbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("de6bd134-55d1-4fb8-a605-1c486b5acb5f");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(2, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1.7, listWatchVal[7].GetDoubleFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("25ee495f-2d8e-4fa5-8180-6d0e45eb4675");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal2.Length);
            Assert.AreEqual(0, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal2[7].GetDoubleFromFSchemeValue());
        }


        [Test]
        public void Sort_StringsAndNumbers_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Sort_Strings&Numbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void Sort_Strings()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Sort_Strings.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("aa64651f-29cb-4008-b199-ec2f4ab3a1f7");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual("dddd", listWatchVal[0].GetStringFromFSchemeValue());
            Assert.AreEqual("bbbbbbbbbbbbb", listWatchVal[4].GetStringFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual("a", listWatchVal2[0].GetStringFromFSchemeValue());
            Assert.AreEqual("rrrrrrrrr", listWatchVal2[4].GetStringFromFSchemeValue());
        }
        #endregion

        #region SortBy Test Cases
        [Test]
        public void SortBy_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\SortBy_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("3cf42e26-c178-4cc4-81a5-38b1c7867f5e");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(10.23, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(8, listWatchVal[4].GetDoubleFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("c966ac1d-5caa-4cfe-bb0c-f6db9e5697c4");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual(10.23, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0.45, listWatchVal2[4].GetDoubleFromFSchemeValue());
        }
        #endregion

        #region Reverse Test Cases

        [Test]
        public void Reverse_ListWithOneNumber()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Reverse_ListWithOneNumber.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(1, listWatchVal.Length);
            Assert.AreEqual(0, listWatchVal[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Reverse_MixedList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Reverse_MixedList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(54.5, listWatchVal[0].GetDoubleFromFSchemeValue());

            // First element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", listWatchVal1[0].GetStringFromFSchemeValue());

        }

        [Test]
        public void Reverse_NumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Reverse_NumberRange.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(6, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(-1, listWatchVal[7].GetDoubleFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal1.Length);
            Assert.AreEqual(-1, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(6, listWatchVal1[7].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Reverse_UsingStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Reverse_UsingStringList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, listWatchVal.Length);
            Assert.AreEqual("Script", listWatchVal[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Dynamo", listWatchVal[3].GetStringFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", listWatchVal1[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Script", listWatchVal1[3].GetStringFromFSchemeValue());

        }

        [Test]
        public void Reverse_WithArrayInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Reverse_WithArrayInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal.Length);
            //Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[0]));
            //Assert.AreEqual("Dynamo", GetDoubleFromFSchemeValue(listWatchVal[3]));

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("2e8a3965-c908-4358-b7fc-331d0f3109ac");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal1.Length);
            //Assert.AreEqual("Dynamo", GetStringFromFSchemeValue(listWatchVal1[0]));
            //Assert.AreEqual("Script", GetStringFromFSchemeValue(listWatchVal1[3]));

        }

        [Test]
        public void Reverse_WithSingletonInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Reverse_WithSingletonInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(10, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, listWatchVal[2].GetDoubleFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("2e8a3965-c908-4358-b7fc-331d0f3109ac");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal1.Length);
            Assert.AreEqual(3, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listWatchVal1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal1[2].GetDoubleFromFSchemeValue());

        }

        #endregion

        #region Filter Tests

        [Test]
        public void Filter_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Filter_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, Second and last element in the list before Filter
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("a54127b5-decb-4750-aaf3-1b895be73984");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(11, listWatchVal.Length);
            Assert.AreEqual(0, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal[10].GetDoubleFromFSchemeValue());

            // First, Second and last element in the list after Filter
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal1.Length);
            Assert.AreEqual(6, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal1[4].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Filter_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Filter_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, second and last element in the list before Filter
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1327061f-b25d-4e91-9df7-a79850cb59e0");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(0, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(5, listWatchVal[5].GetDoubleFromFSchemeValue());

            // After filter there should not
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(0, listWatchVal1.Length);

        }

        [Test]
        public void Filter_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\ListTestFiles\Filter_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, second and last element in the first list after Filter
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fce51e58-10e1-46b4-bc4c-756dfde00de7");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(6, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal[4].GetDoubleFromFSchemeValue());

            // First, second and last element in the second list after Filter
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("412526ae-d86c-491c-a587-d43598fa9c93");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal1.Length);
            Assert.AreEqual(0, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, listWatchVal1[4].GetDoubleFromFSchemeValue());

            // First, second and last elements in the list after combining above two filtered list
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("dc27f671-4cef-480f-9ddc-218d61db7e52");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual(double.PositiveInfinity, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal2[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2.5, listWatchVal2[4].GetDoubleFromFSchemeValue());


        }

        #endregion

    }
}

