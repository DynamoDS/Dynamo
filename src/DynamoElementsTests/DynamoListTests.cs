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
    internal class DynamoListTests
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

        public dynNodeModel NodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return NodeFromCurrentSpace(model, guid);
        }

        public string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.FullName, "test");
        }

        public dynNodeModel NodeFromCurrentSpace(DynamoModel model, Guid guid)
        {
            return model.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        public dynWatch GetWatchNodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            var nodeToWatch = NodeFromCurrentSpace(model, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(dynWatch), nodeToWatch);
            return (dynWatch)nodeToWatch;
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

        public double ConvertToDouble(dynNodeModel node)
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

            string openPath = Path.Combine(GetTestDirectory(), @"dynamo_elements_samples\working\ListTests\Sort_NumbersfFromDiffInput.dyn");
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

            string openPath = Path.Combine(GetTestDirectory(), @"dynamo_elements_samples\working\ListTests\Sort_SimpleNumbers.dyn");
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

            string openPath = Path.Combine(GetTestDirectory(), @"dynamo_elements_samples\working\ListTests\Sort_Strings&Numbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentSpace.Nodes.Count);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

        }

        [Test]
        public void Sort_Strings()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"dynamo_elements_samples\working\ListTests\Sort_Strings.dyn");
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

            string openPath = Path.Combine(GetTestDirectory(), @"dynamo_elements_samples\working\ListTests\SortBy_SimpleTest.dyn");
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
    }
}

