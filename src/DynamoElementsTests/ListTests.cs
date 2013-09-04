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
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class ListTests
    {

        #region startup and shutdown

        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                DynamoLogger.Instance.FinishLogging();
                controller.ShutDown();

                EmptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static DynamoController controller;
        private static string TempFolder;
        private static string ExecutingDirectory { get; set; }

        private static void StartDynamo()
        {
            try
            {
                ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string tempPath = Path.GetTempPath();

                TempFolder = Path.Combine(tempPath, "dynamoTmp");

                if (!Directory.Exists(TempFolder))
                {
                    Directory.CreateDirectory(TempFolder);
                }
                else
                {
                    EmptyTempFolder();
                }

                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE);
                controller.Testing = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);


            }
        }

        #endregion

        #region utility methods

        public NodeModel NodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return NodeFromCurrentSpace(model, guid);
        }

        public string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.FullName, @"test\core\ListTestFiles");
        }

        public NodeModel NodeFromCurrentSpace(DynamoModel model, Guid guid)
        {
            return model.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        public Watch GetWatchNodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            var nodeToWatch = NodeFromCurrentSpace(model, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(Watch), nodeToWatch);
            return (Watch)nodeToWatch;
        }

        public double GetDoubleFromFSchemeValue(FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        private string GetStringFromFSchemeValue(FScheme.Value value)
        {
            string stringValue = string.Empty;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref stringValue));
            return stringValue;
        }

        public FSharpList<FScheme.Value> GetListFromFSchemeValue(FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }

        public double ConvertToDouble(NodeModel node)
        {
            //dynDoubleInput n = node as dynDoubleInput;
            Assert.AreNotEqual(null, node);
            //Assert.AreNotEqual(null, node.OldValue);
            Assert.AreEqual(true, node.OldValue.IsNumber);
            return (node.OldValue as FScheme.Value.Number).Item;
        }

        #endregion

        #region Sort Test Cases

        [Test]
        public void Sort_NumbersfFromDiffInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Sort_NumbersfFromDiffInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(15, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // fourth and last element in the list before sorting
            var watch = GetWatchNodeFromCurrentSpace(model, "de6bd134-55d1-4fb8-a605-1c486b5acb5f");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(1, GetDoubleFromFSchemeValue(listWatchVal[4]));
            Assert.AreEqual(0, GetDoubleFromFSchemeValue(listWatchVal[7]));

            // First and last element in the list after sorting
            watch = GetWatchNodeFromCurrentSpace(model, "25ee495f-2d8e-4fa5-8180-6d0e45eb4675");
            FSharpList<FScheme.Value> listWatchVal2 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(8, listWatchVal2.Length);
            Assert.AreEqual(-3.76498800959146, GetDoubleFromFSchemeValue(listWatchVal2[0]));
            Assert.AreEqual(1, GetDoubleFromFSchemeValue(listWatchVal2[7]));
        }


        [Test]
        public void Sort_SimpleNumbers()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Sort_SimpleNumbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(12, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = GetWatchNodeFromCurrentSpace(model, "de6bd134-55d1-4fb8-a605-1c486b5acb5f");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(1.7, GetDoubleFromFSchemeValue(listWatchVal[7]));

            // First and last element in the list after sorting
            watch = GetWatchNodeFromCurrentSpace(model, "25ee495f-2d8e-4fa5-8180-6d0e45eb4675");
            FSharpList<FScheme.Value> listWatchVal2 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(8, listWatchVal2.Length);
            Assert.AreEqual(0, GetDoubleFromFSchemeValue(listWatchVal2[0]));
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal2[7]));
        }


        [Test]
        public void Sort_StringsAndNumbers_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Sort_Strings&Numbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentSpace.Nodes.Count);

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

            string openPath = Path.Combine(GetTestDirectory(), "Sort_Strings.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(9, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = GetWatchNodeFromCurrentSpace(model, "aa64651f-29cb-4008-b199-ec2f4ab3a1f7");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual("dddd", GetStringFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual("bbbbbbbbbbbbb", GetStringFromFSchemeValue(listWatchVal[4]));

            // First and last element in the list after sorting
            watch = GetWatchNodeFromCurrentSpace(model, "d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            FSharpList<FScheme.Value> listWatchVal2 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual("a", GetStringFromFSchemeValue(listWatchVal2[0]));
            Assert.AreEqual("rrrrrrrrr", GetStringFromFSchemeValue(listWatchVal2[4]));
        }
        #endregion

        #region SortBy Test Cases
        [Test]
        public void SortBy_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "SortBy_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = GetWatchNodeFromCurrentSpace(model, "3cf42e26-c178-4cc4-81a5-38b1c7867f5e");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(10.23, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(8, GetDoubleFromFSchemeValue(listWatchVal[4]));

            // First and last element in the list after sorting
            watch = GetWatchNodeFromCurrentSpace(model, "c966ac1d-5caa-4cfe-bb0c-f6db9e5697c4");
            FSharpList<FScheme.Value> listWatchVal2 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual(10.23, GetDoubleFromFSchemeValue(listWatchVal2[0]));
            Assert.AreEqual(0.45, GetDoubleFromFSchemeValue(listWatchVal2[4]));
        }
        #endregion

        #region Reverse Test Cases

        [Test]
        public void Reverse_ListWithOneNumber()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Reverse_ListWithOneNumber.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(4, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First element in the list before Reversing
            var watch = GetWatchNodeFromCurrentSpace(model, "44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1, listWatchVal.Length);
            Assert.AreEqual(0, GetDoubleFromFSchemeValue(listWatchVal[0]));

        }

        [Test]
        public void Reverse_MixedList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Reverse_MixedList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First element in the list before Reversing
            var watch = GetWatchNodeFromCurrentSpace(model, "44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(54.5, GetDoubleFromFSchemeValue(listWatchVal[0]));

            // First element in the list after Reversing
            watch = GetWatchNodeFromCurrentSpace(model, "6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", GetStringFromFSchemeValue(listWatchVal1[0]));

        }

        [Test]
        public void Reverse_NumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Reverse_NumberRange.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = GetWatchNodeFromCurrentSpace(model, "44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(-1, GetDoubleFromFSchemeValue(listWatchVal[7]));

            // First and last element in the list after Reversing
            watch = GetWatchNodeFromCurrentSpace(model, "6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(8, listWatchVal1.Length);
            Assert.AreEqual(-1, GetDoubleFromFSchemeValue(listWatchVal1[0]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal1[7]));

        }

        [Test]
        public void Reverse_UsingStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Reverse_UsingStringList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = GetWatchNodeFromCurrentSpace(model, "44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4, listWatchVal.Length);
            Assert.AreEqual("Script", GetStringFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual("Dynamo", GetStringFromFSchemeValue(listWatchVal[3]));

            // First and last element in the list after Reversing
            watch = GetWatchNodeFromCurrentSpace(model, "6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", GetStringFromFSchemeValue(listWatchVal1[0]));
            Assert.AreEqual("Script", GetStringFromFSchemeValue(listWatchVal1[3]));

        }

        [Test]
        public void Reverse_WithArrayInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Reverse_WithArrayInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(16, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = GetWatchNodeFromCurrentSpace(model, "1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);
            //Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[0]));
            //Assert.AreEqual("Dynamo", GetDoubleFromFSchemeValue(listWatchVal[3]));

            // First and last element in the list after Reversing
            watch = GetWatchNodeFromCurrentSpace(model, "2e8a3965-c908-4358-b7fc-331d0f3109ac");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal1.Length);
            //Assert.AreEqual("Dynamo", GetStringFromFSchemeValue(listWatchVal1[0]));
            //Assert.AreEqual("Script", GetStringFromFSchemeValue(listWatchVal1[3]));

        }

        [Test]
        public void Reverse_WithSingletonInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Reverse_WithSingletonInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentSpace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentSpace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = GetWatchNodeFromCurrentSpace(model, "1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(3, GetDoubleFromFSchemeValue(listWatchVal[2]));

            // First and last element in the list after Reversing
            watch = GetWatchNodeFromCurrentSpace(model, "2e8a3965-c908-4358-b7fc-331d0f3109ac");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal1.Length);
            Assert.AreEqual(3, GetDoubleFromFSchemeValue(listWatchVal1[0]));
            Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal1[1]));
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal1[2]));

        }

        #endregion

        #region Filter Tests

        [Test]
        public void Filter_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Filter_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentSpace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentSpace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, Second and last element in the list before Filter
            var watch = GetWatchNodeFromCurrentSpace(model, "a54127b5-decb-4750-aaf3-1b895be73984");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(11, listWatchVal.Length);
            Assert.AreEqual(0, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(1, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal[10]));

            // First, Second and last element in the list after Filter
            watch = GetWatchNodeFromCurrentSpace(model, "41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal1.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal1[0]));
            Assert.AreEqual(7, GetDoubleFromFSchemeValue(listWatchVal1[1]));
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal1[4]));

        }

        [Test]
        public void Filter_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Filter_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentSpace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentSpace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, second and last element in the list before Filter
            var watch = GetWatchNodeFromCurrentSpace(model, "1327061f-b25d-4e91-9df7-a79850cb59e0");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(0, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(1, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(5, GetDoubleFromFSchemeValue(listWatchVal[5]));

            // After filter there should not
            watch = GetWatchNodeFromCurrentSpace(model, "41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0, listWatchVal1.Length);

        }

        [Ignore]
        public void Filter_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), "Filter_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentSpace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentSpace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            //// First, second and last element in the list before Filter
            //var watch = GetWatchNodeFromCurrentSpace(model, "1327061f-b25d-4e91-9df7-a79850cb59e0");
            //FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            //Assert.AreEqual(6, listWatchVal.Length);
            //Assert.AreEqual(0, GetDoubleFromFSchemeValue(listWatchVal[0]));
            //Assert.AreEqual(1, GetDoubleFromFSchemeValue(listWatchVal[1]));
            //Assert.AreEqual(5, GetDoubleFromFSchemeValue(listWatchVal[5]));

            //// After filter there should not
            //watch = GetWatchNodeFromCurrentSpace(model, "41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            //FSharpList<FScheme.Value> listWatchVal1 = GetListFromFSchemeValue(watch.GetValue(0));
            //Assert.AreEqual(0, listWatchVal1.Length);

        }

        #endregion

    }
}

