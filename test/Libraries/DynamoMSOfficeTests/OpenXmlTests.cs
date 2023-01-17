using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Tests;
using NUnit.Framework;
using ProtoCore.Mirror;

namespace DynamoMSOfficeTests
{
    [TestFixture]
    [RequiresSTA]
    public class OpenXmlTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
        }

        #region Reading

        [Test]
        public void CanGetLargeWorkbookWithinThresholdTime()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\HammersmithOpenXmlFile_Open.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(3, ViewModel.CurrentSpace.Nodes.Count());

            var timer = new Stopwatch();
            timer.Start();
            ViewModel.HomeSpace.Run();
            timer.Stop();
            Assert.Less(timer.Elapsed.Milliseconds, 1000); // open in less than 1s
        }

        [Test]
        public void FailsGracefullyWhenSheetIsNotFound()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\InvalidSheet_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();

            Assert.AreEqual(ElementState.Warning, node.State);
            Assert.True(node.Infos.Any(x => x.Message.Contains("A worksheet with the provided name 'NotAWorksheet' was not found in the workbook.") &&
            x.State == ElementState.Warning));
        }

        [Test]
        public void CanReadWorksheetWithSingleColumnOfNumbers()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\SingleColumnAscending_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var list = node.CachedValue.GetElements().ToList();

            Assert.AreEqual(16, list.Count);

            // contents of first workbook is ascending array of numbers starting at 1
            for (var i = 0; i < 16; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements().ToList();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(i+1, rowList[0].Data);
            }
        }

        [Test]
        public void CanReadMultiDimensionalWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\MultiColumn_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var list = node.CachedValue.GetElements().ToList();
            Assert.AreEqual(18, list.Count);

            // 18 x 3 array of numbers
            for (var i = 0; i < 18; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements().ToList();
                Assert.AreEqual(3, rowList.Count);

                for (var j = 0; j < 3; j++)
                {
                    Assert.AreEqual((i + 1) + j, rowList[j].Data);
                }
            }
        }

        [Test]
        public void CanReadWorksheetWithEmptyCellInUsedRange()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\MissingCell_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();

            var data = new object[] { new object[] { "a" }, new object[] { null }, new object[] { "cell is" }, new object[] { "missing" } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void CanReadWorksheetWithMixedNumbersAndStrings()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\MixedNumbersAndStrings_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1 }, new object[] { "word" }, new object[] { 2 }, new object[] { 3 }, new object[] { "palabra" } };

            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void ReadOnlyFile()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadOnlyFile_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();
            
            var data = new object[] { new object[] { 4 }, new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void ReadNonExistingFile()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadNonExistingFile_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.AreEqual(ElementState.Warning, node.State);
            Assert.True(node.Infos.Any(x => x.Message.Contains("A workbook was not found in the provided path.") && x.State == ElementState.Warning));
        }

        [Test]
        public void CanReadExcelAsStrings()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadAsStrings_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);

            var data = new object[] { new object[] { "1" }, new object[] { "word" }, new object[] { "2" }, new object[] { "3" }, new object[] { "palabra" } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void CanReadEmptyCellsAsStrings()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadEmptyCellsAsStrings_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);

            var data = new object[]
            {
                new object[] { "5", null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, "afsd", null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, "sfsd", null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, null },
                new object[] { null, null, null, null, null, null, null, null, null, "3453425" }
            };

            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void Defect_MAGN_883()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\Defect_MAGN_883_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            ViewModel.HomeSpace.Run();
            Assert.Pass("RunExpression should no longer crash (Defect_MAGN_883)");
        }

        [Test]
        public void TestFormula()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\formula_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var data2 = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { null }, new object[] { 6 } };
            AssertPreviewValue(node.GUID.ToString(), data2);
        }

        [Test]
        public void TestScientificNotationAsADoubleValue()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\scientificNotation_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var data2 = new object[] { new object[] { 1, 0.5d }, new object[] { 2, 0.00000000005d }, new object[] { 3, 0.0005 }, new object[] { 4, 0.05d }, new object[] { 5, 0.00000003 }, new object[] { 6, 0.0000000000000000002 } };
            AssertPreviewValue(node.GUID.ToString(), data2);
        }

        #endregion


        #region Writing

        [Test]
        public void CanWrite1DDataOfMixedTypesToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteMixed1DData_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWrite1DDataOfMixedTypesToExcelWorksheet)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);

            var data = new object[] { new object[] { "text" }, new object[] { true }, new object[] { new DateTime(2021, 4, 1, 14, 51, 30, 0) }, new object[] { 1 }, new object[] { 1.23 } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void WriteListsDifferentSizes_MAGN6872()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\MAGN6872_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(WriteListsDifferentSizes_MAGN6872)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var list = node.CachedValue.GetElements().ToList();

            Assert.AreEqual(4, list.Count());

            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();

            List<object> dataList0 = new List<object>();
            foreach (MirrorData data in rowList)
            {
                dataList0.Add(data.Data);
            }
            Assert.AreEqual(new object[] { 1.00, 2.00, 3.00, null, null, null, null }, dataList0);

            rowList = list[1].GetElements();
            List<object> dataList1 = new List<object>();
            foreach (MirrorData data in rowList)
            {
                dataList1.Add(data.Data);
            }
            Assert.AreEqual(new object[] { 1.00, 2.00, 3.00, 4.00, 5.00, null, null }, dataList1);

            Assert.IsTrue(list[2].IsCollection);
            rowList = list[2].GetElements();
            List<object> dataList2 = new List<object>();
            foreach (MirrorData data in rowList)
            {
                dataList2.Add(data.Data);
            }
            Assert.AreEqual(new object[] { 1.00, 2.00, 3.00, 4.00, 5.00, 6.00, 7.00 }, dataList2);

            Assert.IsTrue(list[3].IsCollection);
            rowList = list[3].GetElements();
            List<object> dataList3 = new List<object>();
            foreach (MirrorData data in rowList)
            {
                dataList3.Add(data.Data);
            }
            Assert.AreEqual(new object[] { 1.00, 2.00, 3.00, 4.00, 5.00, null, null }, dataList3);
        }

        /// <summary>
        /// Overwrite the excel sheet with null values and it must work ok.
        /// </summary>
        [Test]
        public void OverwriteWithNull_MAGN7213()
        {
            // Copy the file so we don't write over the original
            string excelFilePath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN7213.xlsx");
            var filePath = Path.Combine(TempFolder, $"{nameof(OverwriteWithNull_MAGN7213)}_output.xlsx");
            File.Copy(excelFilePath, filePath);

            string openPath = Path.Combine(TestDirectory, @"core\excel\MAGN7213_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] { 3 } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        /// <summary>
        /// Overwrite an existing file with EmptyList- it must run ok.
        /// </summary>
        [Test]
        public void OverwriteEmptyList_MAGN7213()
        {
            // Copy the file so we don't write over the original
            string excelFilePath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN7213_2.xlsx");
            var filePath = Path.Combine(TempFolder, $"{nameof(OverwriteEmptyList_MAGN7213)}_output.xlsx");
            File.Copy(excelFilePath, filePath);

            string openPath = Path.Combine(TestDirectory, @"core\excel\MAGN7213_2_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] { 3 } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void CanCreateNewWorksheetInNewWorkbook()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddWorksheet_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanCreateNewWorksheetInNewWorkbook)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 } };
            AssertPreviewValue(node.GUID.ToString(), data);
        }

        [Test]
        public void CanAddSingleItemToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddSingleItemData_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanAddSingleItemToExcelWorksheet)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var list = node.CachedValue.GetElements().ToList();
            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements().ToList();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(100.0, rowList[0].Data);
        }

        [Test]
        public void CanAddNullItemToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddNullItemData_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanAddNullItemToExcelWorksheet)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsNull(node.CachedValue.Data);
        }

        [Test]
        public void CanAdd1DListToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_Add1DListData_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanAdd1DListToExcelWorksheet)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var list = node.CachedValue.GetElements().ToList();

            Assert.AreEqual(101, list.Count());

            var counter = 0;
            for (var i = 0; i < 101; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements().ToList();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(counter++, rowList[0].Data);
            }
        }

        [Test]
        public void CanAdd2DListToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_Add2DListData_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanAdd2DListToExcelWorksheet)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var node = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(node.CachedValue.IsCollection);
            var list = node.CachedValue.GetElements().ToList();

            // 5 X 101 - each row is 0..100
            Assert.AreEqual(5, list.Count());

            var counter = 0;
            for (var i = 0; i < 5; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements();
                Assert.AreEqual(101, rowList.Count());
                rowList.ToList().ForEach(x => Assert.AreEqual(counter++, x.Data));
                counter = 0;
            }
        }

        [Test]
        public void CanWriteToExcelAndUpdateData()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNodeAndUpdateData_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteToExcelAndUpdateData)}_output.xlsx");
            var stringNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<CoreNodeModels.Input.StringInput>();
            var filePathNode = stringNodes.Where(n => n.Name == "String").First();
            var dataValueNode = stringNodes.Where(n => n.Name == "Value").First();
            filePathNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var list = readNode.CachedValue.GetElements().ToList();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements().ToList();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("BBB", rowList[0].Data);

            dataValueNode.Value = "AAA";
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            list = readNode.CachedValue.GetElements().ToList();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            rowList = list[0].GetElements().ToList();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("AAA", rowList[0].Data);
        }

        [Test]
        public void CanWriteJaggedArrayToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteJaggedArrayToExcel_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteJaggedArrayToExcel)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1, 1, null }, new object[] { 2, 2, 2 }, new object[] { 3, 3, 3 }, new object[] { null, 4, null } };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void CanWriteEmptyArrayToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteEmptyArrayToExcel_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteEmptyArrayToExcel)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] { 3 } };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void CanWriteNestedEmptyListToExcel()
        {
            // Copy the file so we don't write over the original
            string excelFilePath = Path.Combine(TestDirectory, @"core\excel\emptyCellInMiddle.xlsx");
            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteNestedEmptyListToExcel)}_output.xlsx");
            File.Copy(excelFilePath, filePath);

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNestedEmptyListToExcel_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var stringInput = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringInput.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 99, 99, 99 }, new object[] { 99, 99, 99 }, new object[] { 99, null, 99 } };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void CanWriteEmptyListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteEmptyListToExcel_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteEmptyListToExcel)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var list = readNode.CachedValue.GetElements().ToList();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void CanOverWriteToExcel()
        {
            // Copy the file so we don't write over the original
            string excelFilePath = Path.Combine(TestDirectory, @"core\excel\overWriteFile.xlsx");
            var filePath = Path.Combine(TempFolder, $"{nameof(CanOverWriteToExcel)}_output.xlsx");
            File.Copy(excelFilePath, filePath);

            string openPath = Path.Combine(TestDirectory, @"core\excel\OverWriteExcelSheet_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var stringInput = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringInput.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[]
            {
                new object[] {999, 999, 999},
                new object[] {999, 999, 999},
                new object[] {999, 999, 999}
            };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void CanOverWritePartiallyToExcel()
        {
            // Copy the file so we don't write over the original
            string excelFilePath = Path.Combine(TestDirectory, @"core\excel\overwriteFilePartial.xlsx");
            var filePath = Path.Combine(TempFolder, $"{nameof(CanOverWritePartiallyToExcel)}_output.xlsx");
            File.Copy(excelFilePath, filePath);

            string openPath = Path.Combine(TestDirectory, @"core\excel\OverWritePartialExcelSheet_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var stringInput = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringInput.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[]
            {
                new object[] {1, 1, 1, 1, 1},
                new object[] {1, 999, 999, 999, 1},
                new object[] {1, 999, 999, 999, 1},
                new object[] {1, 999, 999, 999, 1},
                new object[] {1, 1, 1, 1, 1}
            };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void CanWriteNullValueToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNullValuesToExcel_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteNullValueToExcel)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var list = readNode.CachedValue.GetElements();
            Assert.AreEqual(0, list.Count());
        }

        [Test]
        public void CanWriteNullValueInListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNullValuesToExcel1_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanWriteNullValueInListToExcel)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] { 3 } };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void TestWriteFunctionObjectToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteFunctionObject_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(TestWriteFunctionObjectToExcel)}_output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            ProtoCore.RuntimeCore runtimeCore = ViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.Default, warningEntry.ID);

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var list = readNode.CachedValue.GetElements().ToList();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void WriteNonExistingSheet()
        {
            // Copy the file so we don't write over the original
            string excelFilePath = Path.Combine(TestDirectory, @"core\excel\NonExistingsheet.xlsx");
            var filePath = Path.Combine(TempFolder, $"{nameof(WriteNonExistingSheet)}_output.xlsx");
            File.Copy(excelFilePath, filePath);

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNonExistingSheet_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringNode.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { 4 }, new object[] { 5 } };
            AssertPreviewValue(readNode.GUID.ToString(), data);
        }

        [Test]
        public void CanExportToExcelAsString()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\ExportToExcelAsString_OpenXml.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = Path.Combine(TempFolder, $"{nameof(CanExportToExcelAsString)}_output.xlsx");
            var stringInput = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.StringInput>();
            stringInput.Value = filePath;

            var readNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Data.OpenXMLImportExcel");
            var typeNode = ViewModel.Model.CurrentWorkspace.Nodes.First(n => n.Name == "Object.Type");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { "1.23" }, new object[] { "True" }, new object[] { "04/08/2021 10:14:00" }, new object[] { "Hello" } };
            AssertPreviewValue(readNode.GUID.ToString(), data);

            Assert.IsTrue(typeNode.CachedValue.IsCollection);
            var stringType = new object[] { "System.String" };
            var types = new object[] { stringType, stringType, stringType, stringType };
            AssertPreviewValue(typeNode.GUID.ToString(), types);
        }

        #endregion
    }
}
