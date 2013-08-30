using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using System.Reflection;
using Dynamo.Utilities;

namespace Dynamo.Tests
{
    class ListTests
    {
        #region Startup and shutdown

        [SetUp]
        public void Init()
        {
            startDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                DynamoLogger.Instance.FinishLogging();
                controller.ShutDown();

                smptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static DynamoController controller;
        private static string TempFolder;
        private static string ExecutingDirectory { get; set; }


        #endregion

        #region Helping Methods

        private void startDynamo()
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
                    smptyTempFolder();
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

        private void smptyTempFolder()
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

        private string getTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.FullName, @"test\core\list");
        }

        private dynNodeModel nodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return nodeFromCurrentSpace(model, guid);
        }

        private dynNodeModel nodeFromCurrentSpace(DynamoModel model, Guid guid)
        {
            return model.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        private dynWatch getWatchNodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            var nodeToWatch = nodeFromCurrentSpace(model, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(dynWatch), nodeToWatch);
            return (dynWatch)nodeToWatch;
        }

        private FSharpList<FScheme.Value> getListFromFSchemeValue(FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }

        private string getStringFromFSchemeValue(FScheme.Value value)
        {
            string stringValue = string.Empty;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref stringValue));
            return stringValue;
        }

        private double getDoubleFromFSchemeValue(FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        #endregion

        #region Testing Properties

        string localDynamoListTestFolder { get { return getTestDirectory(); } }

        #endregion

        #region Test Cases

        [Test]
        public void TestBuildSublistsEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testBuildSubLists_emptyInput.dyn");
            model.Open(testFilePath);

            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");
            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestBuildSublistsInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testBuildSubLists_invalidInput.dyn");
            model.Open(testFilePath);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestBuildSublistsNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testBuildSubLists_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            FSharpList<FScheme.Value> actualChild1 = getListFromFSchemeValue(actual[0]);
            FSharpList<FScheme.Value> actualChild2 = getListFromFSchemeValue(actual[1]);

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(1, getDoubleFromFSchemeValue(actualChild1[0]));
            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual(3, getDoubleFromFSchemeValue(actualChild2[0]));
        }

        [Test]
        public void TestBuildSublistsStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testBuildSubLists_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            FSharpList<FScheme.Value> actualChild1 = getListFromFSchemeValue(actual[0]);
            FSharpList<FScheme.Value> actualChild2 = getListFromFSchemeValue(actual[1]);

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual("b", getStringFromFSchemeValue(actualChild1[0]));
            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual("d", getStringFromFSchemeValue(actualChild2[0]));
        }

        [Test]
        public void TestConcatenateListsEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testConcatenateLists_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));

            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestConcatenateListsInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testConcatenateLists_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestConcatenateListsNormalInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testConcatenateLists_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));

            Assert.AreEqual(9, actual.Length);
            Assert.AreEqual(10, getDoubleFromFSchemeValue(actual[0]));
            Assert.AreEqual(20, getDoubleFromFSchemeValue(actual[1]));
            Assert.AreEqual(10, getDoubleFromFSchemeValue(actual[2]));
            Assert.AreEqual(20, getDoubleFromFSchemeValue(actual[3]));
            Assert.AreEqual(10, getDoubleFromFSchemeValue(actual[4]));
            Assert.AreEqual("a", getStringFromFSchemeValue(actual[5]));
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[6]));
            Assert.AreEqual("a", getStringFromFSchemeValue(actual[7]));
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[8]));
        }

        [Test]
        public void TestDiagonalLeftListEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaLeftList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));

            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestDiagonalLeftListInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaLeftList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestDiagonalLeftListNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaLeftList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            FSharpList<FScheme.Value> actualChild1 = getListFromFSchemeValue(actual[0]);
            FSharpList<FScheme.Value> actualChild2 = getListFromFSchemeValue(actual[1]);
            FSharpList<FScheme.Value> actualChild3 = getListFromFSchemeValue(actual[2]);

            Assert.AreEqual(3, actual.Length);

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(1, getDoubleFromFSchemeValue(actualChild1[0]));

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual(2, getDoubleFromFSchemeValue(actualChild2[0]));
            Assert.AreEqual(3, getDoubleFromFSchemeValue(actualChild2[1]));

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual(4, getDoubleFromFSchemeValue(actualChild3[0]));
            Assert.AreEqual(5, getDoubleFromFSchemeValue(actualChild3[1]));
        }

        [Test]
        public void TestDiagonalLeftListStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaLeftList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, actual.Length);
            FSharpList<FScheme.Value> actualChild1 = getListFromFSchemeValue(actual[0]);
            FSharpList<FScheme.Value> actualChild2 = getListFromFSchemeValue(actual[1]);
            FSharpList<FScheme.Value> actualChild3 = getListFromFSchemeValue(actual[2]);

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual("a", getStringFromFSchemeValue(actualChild1[0]));

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual("b", getStringFromFSchemeValue(actualChild2[0]));
            Assert.AreEqual("a", getStringFromFSchemeValue(actualChild2[1]));

            Assert.AreEqual(1, actualChild3.Length);
            Assert.AreEqual("b", getStringFromFSchemeValue(actualChild3[0]));
        }

        [Test]
        public void TestDiagonalRightListEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaRightList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestDiagonalRightListInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaRightList_invalidInput.dyn");
            model.Open(testFilePath);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestDiagonalRightListNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testDiagonaRightList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4, actual.Length);
            FSharpList<FScheme.Value> actualChild1 = getListFromFSchemeValue(actual[0]);
            FSharpList<FScheme.Value> actualChild2 = getListFromFSchemeValue(actual[1]);
            FSharpList<FScheme.Value> actualChild3 = getListFromFSchemeValue(actual[2]);
            FSharpList<FScheme.Value> actualChild4 = getListFromFSchemeValue(actual[3]);

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(5, getDoubleFromFSchemeValue(actualChild1[0]));

            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual(3, getDoubleFromFSchemeValue(actualChild2[0]));

            Assert.AreEqual(2, actualChild3.Length);
            Assert.AreEqual(1, getDoubleFromFSchemeValue(actualChild3[0]));
            Assert.AreEqual(4, getDoubleFromFSchemeValue(actualChild3[1]));

            Assert.AreEqual(1, actualChild4.Length);
            Assert.AreEqual(2, getDoubleFromFSchemeValue(actualChild4[0]));
        }

        [Test]
        public void TestFirstOfListEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testFirstOfList_emptyInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFirstOfListInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testFirstOfList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFirstOfListNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testFirstOfList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 10;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestFirstOfListStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testFirstOfList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            string actual = getStringFromFSchemeValue(watch.GetValue(0));
            string expected = "a";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestIsEmptyListEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testIsEmptyList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 1;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestIsEmptyListInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testIsEmptyList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestIsEmptyListNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testIsEmptyList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestIsEmptyListStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testIsEmptyList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testListLength_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testListLength_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestStringLengthNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testListLength_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 5;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testListLength_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = getDoubleFromFSchemeValue(watch.GetValue(0));
            double expected = 4;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestPartitionStringEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPartitionList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            double expected = 0;
            Assert.AreEqual(expected, actual.Length);
        }

        [Test]
        public void TestPartitionStringInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPartitionList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestPartitionStringNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPartitionList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(2, actual.Length);

            FSharpList<FScheme.Value> childList1 = getListFromFSchemeValue(actual[0]);
            Assert.AreEqual(3, childList1.Length);
            Assert.AreEqual(1, getDoubleFromFSchemeValue(childList1[0]));
            Assert.AreEqual(2, getDoubleFromFSchemeValue(childList1[1]));
            Assert.AreEqual(3, getDoubleFromFSchemeValue(childList1[2]));

            FSharpList<FScheme.Value> childList2 = getListFromFSchemeValue(actual[1]);
            Assert.AreEqual(2, childList2.Length);
            Assert.AreEqual(4, getDoubleFromFSchemeValue(childList2[0]));
            Assert.AreEqual(5, getDoubleFromFSchemeValue(childList2[1]));
        }

        [Test]
        public void TestPartitionStringStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPartitionList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(2, actual.Length);

            FSharpList<FScheme.Value> childList1 = getListFromFSchemeValue(actual[0]);
            Assert.AreEqual(3, childList1.Length);
            Assert.AreEqual("a", getStringFromFSchemeValue(childList1[0]));
            Assert.AreEqual("b", getStringFromFSchemeValue(childList1[1]));
            Assert.AreEqual("a", getStringFromFSchemeValue(childList1[2]));

            FSharpList<FScheme.Value> childList2 = getListFromFSchemeValue(actual[1]);
            Assert.AreEqual(1, childList2.Length);
            Assert.AreEqual("b", getStringFromFSchemeValue(childList2[0]));
        }

        [Test]
        public void TestFlattenEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPlatten_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            double expectedLength = 0;
            Assert.AreEqual(expectedLength, actual.Length);
        }

        [Test]
        public void TestFlattenInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPlatten_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFlattenNormalInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPlatten_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6, actual.Length);

            FSharpList<FScheme.Value> childList1 = getListFromFSchemeValue(actual[0]);
            Assert.AreEqual(5, childList1.Length);
            Assert.AreEqual(0, getDoubleFromFSchemeValue(childList1[0]));
            Assert.AreEqual(1, getDoubleFromFSchemeValue(childList1[1]));
            Assert.AreEqual(2, getDoubleFromFSchemeValue(childList1[2]));
            Assert.AreEqual(3, getDoubleFromFSchemeValue(childList1[3]));
            Assert.AreEqual(4, getDoubleFromFSchemeValue(childList1[4]));

            FSharpList<FScheme.Value> childList2 = getListFromFSchemeValue(actual[1]);
            Assert.AreEqual(4, childList2.Length);
            Assert.AreEqual("a", getStringFromFSchemeValue(childList2[0]));
            Assert.AreEqual("b", getStringFromFSchemeValue(childList2[1]));
            Assert.AreEqual("c", getStringFromFSchemeValue(childList2[2]));
            Assert.AreEqual("d", getStringFromFSchemeValue(childList2[3]));

            Assert.AreEqual("a", getStringFromFSchemeValue(actual[2]));
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[3]));
            Assert.AreEqual("c", getStringFromFSchemeValue(actual[4]));
            Assert.AreEqual("d", getStringFromFSchemeValue(actual[5]));
        }

        [Test]
        public void TestFlattenCompletlyEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPlattenCompletely_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            double expectedLength = 0;
            Assert.AreEqual(expectedLength, actual.Length);
        }

        [Test]
        public void TestFlattenCompletlyInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPlattenCompletely_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFlattenCompletlyNormalInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testPlattenCompletely_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(13, actual.Length);

            Assert.AreEqual(0, getDoubleFromFSchemeValue(actual[0]));
            Assert.AreEqual(1, getDoubleFromFSchemeValue(actual[1]));
            Assert.AreEqual(2, getDoubleFromFSchemeValue(actual[2]));
            Assert.AreEqual(3, getDoubleFromFSchemeValue(actual[3]));
            Assert.AreEqual(4, getDoubleFromFSchemeValue(actual[4]));
            Assert.AreEqual("a", getStringFromFSchemeValue(actual[5]));
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[6]));
            Assert.AreEqual("c", getStringFromFSchemeValue(actual[7]));
            Assert.AreEqual("d", getStringFromFSchemeValue(actual[8]));
            Assert.AreEqual("a", getStringFromFSchemeValue(actual[9]));
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[10]));
            Assert.AreEqual("c", getStringFromFSchemeValue(actual[11]));
            Assert.AreEqual("d", getStringFromFSchemeValue(actual[12]));
        }

        [Test]
        public void TestRepeatEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRepeat_emptyInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestRepeatNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRepeat_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, actual.Length);

            FSharpList<FScheme.Value> childList1 = getListFromFSchemeValue(actual[0]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(0, getDoubleFromFSchemeValue(childList1[0]));
            Assert.AreEqual(0, getDoubleFromFSchemeValue(childList1[1]));

            FSharpList<FScheme.Value> childList2 = getListFromFSchemeValue(actual[1]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(1, getDoubleFromFSchemeValue(childList2[0]));
            Assert.AreEqual(1, getDoubleFromFSchemeValue(childList2[1]));

            FSharpList<FScheme.Value> childList3 = getListFromFSchemeValue(actual[2]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(2, getDoubleFromFSchemeValue(childList3[0]));
            Assert.AreEqual(2, getDoubleFromFSchemeValue(childList3[1]));

            FSharpList<FScheme.Value> childList4 = getListFromFSchemeValue(actual[3]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(3, getDoubleFromFSchemeValue(childList4[0]));
            Assert.AreEqual(3, getDoubleFromFSchemeValue(childList4[1]));

            FSharpList<FScheme.Value> childList5 = getListFromFSchemeValue(actual[4]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(4, getDoubleFromFSchemeValue(childList5[0]));
            Assert.AreEqual(4, getDoubleFromFSchemeValue(childList5[1]));
        }

        [Test]
        public void TestRepeatStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRepeat_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4, actual.Length);

            FSharpList<FScheme.Value> childList1 = getListFromFSchemeValue(actual[0]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("a", getStringFromFSchemeValue(childList1[0]));
            Assert.AreEqual("a", getStringFromFSchemeValue(childList1[1]));

            FSharpList<FScheme.Value> childList2 = getListFromFSchemeValue(actual[1]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("b", getStringFromFSchemeValue(childList2[0]));
            Assert.AreEqual("b", getStringFromFSchemeValue(childList2[1]));

            FSharpList<FScheme.Value> childList3 = getListFromFSchemeValue(actual[2]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("c", getStringFromFSchemeValue(childList3[0]));
            Assert.AreEqual("c", getStringFromFSchemeValue(childList3[1]));

            FSharpList<FScheme.Value> childList4 = getListFromFSchemeValue(actual[3]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("d", getStringFromFSchemeValue(childList4[0]));
            Assert.AreEqual("d", getStringFromFSchemeValue(childList4[1]));
        }

        [Test]
        public void TestRestOfListEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRestOfList_emptyInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestRestOfListInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRestOfList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestRestOfListNumberInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRestOfList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4, actual.Length);
            Assert.AreEqual(20, getDoubleFromFSchemeValue(actual[0]));
            Assert.AreEqual(10, getDoubleFromFSchemeValue(actual[1]));
            Assert.AreEqual(20, getDoubleFromFSchemeValue(actual[2]));
            Assert.AreEqual(10, getDoubleFromFSchemeValue(actual[3]));
        }

        [Test]
        public void TestRestOfListStringInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testRestOfList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, actual.Length);
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[0]));
            Assert.AreEqual("a", getStringFromFSchemeValue(actual[1]));
            Assert.AreEqual("b", getStringFromFSchemeValue(actual[2]));
        }

        [Test]
        public void TestTransposeEmptyInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testTransposeList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestTransposeInvalidInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testTransposeList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestTransposeNormalInput()
        {
            DynamoModel model = controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoListTestFolder, "testTransposeList_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = getWatchNodeFromCurrentSpace(model, "789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = getListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, actual.Length);

            FSharpList<FScheme.Value> childList1 = getListFromFSchemeValue(actual[0]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(1, getDoubleFromFSchemeValue(childList1[0]));
            Assert.AreEqual("a", getStringFromFSchemeValue(childList1[1]));

            FSharpList<FScheme.Value> childList2 = getListFromFSchemeValue(actual[1]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(2, getDoubleFromFSchemeValue(childList2[0]));
            Assert.AreEqual("b", getStringFromFSchemeValue(childList2[1]));

            FSharpList<FScheme.Value> childList3 = getListFromFSchemeValue(actual[2]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(3, getDoubleFromFSchemeValue(childList3[0]));
            Assert.AreEqual("a", getStringFromFSchemeValue(childList3[1]));

            FSharpList<FScheme.Value> childList4 = getListFromFSchemeValue(actual[3]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(4, getDoubleFromFSchemeValue(childList4[0]));
            Assert.AreEqual("b", getStringFromFSchemeValue(childList4[1]));

            FSharpList<FScheme.Value> childList5 = getListFromFSchemeValue(actual[4]);
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(5, getDoubleFromFSchemeValue(childList5[0]));
        }

        #endregion
    }
}
