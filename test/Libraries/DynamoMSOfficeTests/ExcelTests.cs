using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DSOffice;
using NUnit.Framework;
using ProtoCore.Mirror;
using Dynamo.DSEngine.CodeCompletion;
using Dynamo.Models;
using Dynamo.Nodes;


namespace Dynamo.Tests
{
    [TestFixture]
    public class ExcelTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        public override void Setup()
        {
            base.Setup();

            // In unit-test scenario we are redirecting 'PreferenceSettings' to 
            // load from a non-existing preference XML file. That way each test 
            // will result in an instance of 'PreferenceSettings' with its default 
            // values (since the underlying file wouldn't have existed). This 
            // ensures the preference value change in one test case (if any) does 
            // not get persisted across to the subsequent test case.
            // 
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");
        }

        public override void Cleanup()
        {
            try
            {
                base.Cleanup();

                EventArgs args = new ExcelCloseEventArgs(false);
                ExcelInterop.OnProcessExit(this, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        #region COM

        [Test]
        public void ExcelAppIsClosedOnCleanup()
        {
            Assert.Inconclusive("Has trouble with sequential unit tests.  Does work with single unit test, though.");
            //Assert.IsFalse(ExcelInterop.IsExcelProcessRunning);
            //Assert.IsFalse(ExcelInterop.HasExcelReference);
            //var app = ExcelInterop.ExcelApp;
            //Assert.IsTrue(ExcelInterop.IsExcelProcessRunning);
            //Assert.IsTrue(ExcelInterop.HasExcelReference);
            //ViewModel.Model.OnCleanup(null);
            //Thread.Sleep(100); 
            //Assert.IsFalse( ExcelInterop.IsExcelProcessRunning );
            //Assert.IsFalse(ExcelInterop.HasExcelReference);
        }

        #endregion

        #region Reading

        [Test]
        public void CanGetLargeWorkbookWithinThresholdTime()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\HammersmithExcelFile_Open.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var timer = new Stopwatch();
            timer.Start();
            ViewModel.HomeSpace.Run();
            timer.Stop();
            Assert.Less(timer.Elapsed.Milliseconds, 1000); // open in less than 1s

        }


        [Test]
        public void CanGetWorksheets()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\WorksheetsFromFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetWorksheetsFromExcelWorkbook");

            ViewModel.HomeSpace.Run();

            MirrorData mirror = watch.CachedValue;
            Assert.IsTrue(mirror.IsCollection);

            Assert.AreEqual(3, mirror.GetElements().Count);
        }

        [Test]
        public void CanGetWorksheetByNameWithValidInput()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\WorksheetByName_ValidInput.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetExcelWorksheetByName");

            ViewModel.HomeSpace.Run();

            Assert.IsNotNull(watch.CachedValue);
        }

        [Test]
        public void ThrowExceptionOnGetWorksheetByNameWithInvalidInput()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WorksheetByName_InvalidInput.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var getWorksheet = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetExcelWorksheetByName");
            var readFile = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadExcelFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readFile.CachedValue.Class.ClassName == "DSOffice.WorkBook");
            Assert.IsNull(getWorksheet.CachedValue.Data);
        }

        [Test]
        public void CanReadWorksheetWithSingleColumnOfNumbers()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_ascending.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(16, list.Count);

            // contents of first workbook is ascending array of numbers starting at 1
            var counter = 1;
            for (var i = 0; i < 16; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(counter++, rowList[0].Data);
            }
        }

        [Test]
        
        public void CanReadMultiDimensionalWorksheet()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_2Dimensional.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();
            Assert.AreEqual(18, list.Count);

            // 18 x 3 array of numbers
            for (var i = 0; i < 18; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements();
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
            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_missingCell.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();
            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();
            //Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] {new object []{"a"},new object []{null},new object []{"cell is"},new object[] {"missing"} };
            AssertPreviewValue(watch.GUID.ToString(), data);
        }

        [Test]
        public void CanReadWorksheetWithMixedNumbersAndStrings()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_mixedNumbersAndStrings.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] {1},new object []{"word"},new object[]{2},new object[] {3},new object[]{"palabra" }};
            
            AssertPreviewValue(watch.GUID.ToString(), data);

        }
        [Test]
        public void ReadOnlyFile()
        {
            //Read from readonly Excel file and make sure Excel node in Dynamo displays value correctly.

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadOnlyFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            ViewModel.HomeSpace.Run();
            var data = new object[] { new object[] { 4 }, new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

        }
        [Test]
        public void CanReadAndWriteExcel()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadAndWriteExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            // watch displays the data from the Read node
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");

            // writeNode should have the same data contained in watch
            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            var data = new object[] { new object[] { 1 }, new object[] { "word" }, new object[] { 2 }, new object[] { 3 }, new object[] { "palabra" } };

            AssertPreviewValue(watch.GUID.ToString(), data);

            
        }
        [Test]
        public void ReadChangeFilename()
        {

            // 1.  Read from an Existing excel file and Run
            // 2.  Modify the file name in file path to an existing file
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\ChangeFilename.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var fileNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<DSCore.File.Filename>();
            foreach (var file in fileNodes)
            {
                file.Value.Replace(@"..\..\..\test", TestDirectory);
            }

            // remap the filename as Excel requires an absolute path

            ViewModel.HomeSpace.Run();
            
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            var excelFileName = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("b9b04f1f-9069-4eaf-a31c-eee7428aacab");
            var FileObject = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("1bd2c2da-b0e9-4b88-9f9e-33bd0906c6e9");
            
            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            
            // single column - 1, "word", 2, 3, "palabra"
            AssertPreviewValue(watch.GUID.ToString(), new object []{new object []{1},new object []{"word"},new object[]{2},new object[] { 3}, new object [] {"palabra"}});
            
            // change the file path 
            ConnectorModel.Make(excelFileName, FileObject, 0, 0);
            ViewModel.HomeSpace.Run();
            
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 10 }, new object[] { 20 }, new object[] { 30 }, new object[] { "test" }, new object[] { "Dynamo" } };
            AssertPreviewValue(watch.GUID.ToString(), data);
            
        }
        [Test]
        public void ReadNonExistingFile()
        {

            // 1.  Read from a non existing excel file and Run           
            // 2.  Rerun the file  - it must throw a warning
            
            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadNonExistingFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var newname = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("de5c9439-bc4c-408d-9484-798d8d8b8aed");
            ViewModel.HomeSpace.Run();
           
            Assert.IsTrue(watch.State == Models.ElementState.Warning);
           

        }
        

        [Test]
        public void CanReadExcelAsStrings()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadExcelAsStrings.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            // watch displays the data from the Read node
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);

            var data = new object[] { new object[] { "1" }, new object[] { "word" }, new object[] { "2" }, new object[] { "3" }, new object[] { "palabra" } };
            AssertPreviewValue(watch.GUID.ToString(), data);


        }

        [Test]
        public void CanReadEmptyCellsAsStrings()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadEmptyCellsAsStrings.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            // watch displays the data from the Read node
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);

            var data = new object[]
            {
                new object[] { 5, null, null, null, null, null, null, null, null, null },
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
                new object[] { null, null, null, null, null, null, null, null, null, 3453425 }
            };

            AssertPreviewValue(watch.GUID.ToString(), data);
        }

        #endregion

        #region Writing

        [Test]
        public void CanWrite1DDataOfMixedTypesToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddMixed1DData.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(13, ViewModel.CurrentSpace.Nodes.Count);
            
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { "doodle" }, new object[] { 0.000 }, new object[] { 21029.000 }, new object[] { null }, new object[] { -90.000 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

        }
        [Test]
        public void Excel_MAGN6872()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6872
            
            
            string openPath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN6872.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(9, ViewModel.CurrentSpace.Nodes.Count);
        
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

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
        [Test]
        public void Excel_OverwriteWithnull_7213()
        {
            // Overwrite the excel sheet with null values and it must work ok.
            string openPath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN7213.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(6, ViewModel.CurrentSpace.Nodes.Count);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);

            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] {3 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
            
        }

        [Test]
        public void Excel_OverWriteEmptyList_MAGN7213()
        {

            //Overwrite an existing file with EmptyList- it must run ok
            string openPath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN7213_2.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(7, ViewModel.CurrentSpace.Nodes.Count);
            
            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] { 3 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
            
            
        }
        [Test]
        public void CanCreateNewWorksheetInNewWorkbook()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddWorksheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);
            var getWorksheet = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetExcelWorksheetByName");
            ViewModel.HomeSpace.Run();
            Assert.IsNull(getWorksheet.CachedValue.Data);
        }

        [Test]
        public void CanAddSingleItemToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddSingleItemData.dyn");

            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, ViewModel.CurrentSpace.Nodes.Count);
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(100.0, rowList[0].Data);
        }

        [Test]
        public void CanAdd1DListToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_Add1DListData.dyn");

            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, ViewModel.CurrentSpace.Nodes.Count);
            var previewNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(previewNode.CachedValue.IsCollection);
            var list = previewNode.CachedValue.GetElements();

            Assert.AreEqual(101, list.Count());

            // contents of first workbook is ascending array of numbers starting at 1
            var counter = 0;
            for (var i = 0; i < 101; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(counter++, rowList[0].Data);
            }

        }

        [Test]
        public void CanAdd2DListToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_Add2DListData.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(11, ViewModel.CurrentSpace.Nodes.Count);
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            // 5 X 101 - each row is 0..100
            Assert.AreEqual(5, list.Count());

            // contents of first workbook is ascending array of numbers starting at 1
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
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNodeAndUpdateData.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("BBB", rowList[0].Data);

            var stringNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<Dynamo.Nodes.StringInput>();
            var inputStringNode = stringNodes.Where(x => x.Value == "BBB").FirstOrDefault();
            inputStringNode.Value = "AAA";

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("AAA", rowList[0].Data);

        }

        [Test]
        public void CanWriteJaggedArrayToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteJaggedArrayToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            // Makes use of UnitTestBase.TempFolder since that folder is always 
            // guaranteed to be different for each test, and will get deleted when 
            // the test shuts down.
            // 
            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1,1,null }, new object[] {2,2,2 }, new object []{3,3,3},new object[] { null,4,null }};
            AssertPreviewValue(writeNode.GUID.ToString(), data);

        }

        [Test]
        public void CanWriteEmptyArrayToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteEmptyArrayToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] {3 } };
            AssertPreviewValue(writeNode.GUID.ToString(), data);
            
            
        }

        [Test]
        public void CanWriteNestedEmptyListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNestedEmptyListToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filename.Value));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 99,99,99},new object[] { 99 ,99,99}, new object[] { 99,null,99 }};
            AssertPreviewValue(writeNode.GUID.ToString(), data);

            // input array is nested empty list, {{}, {}, {}}
            // Ensure output is {{99, 99, 99}, {99, 99, 99}, {99, "", 99}}
            

        }

        [Test]
        public void CanWriteEmptyListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteEmptyListToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            // input array is empty list, {}
            // Ensure output is also empty list
            Assert.AreEqual(0, list.Count);

        }

        [Test]
        public void CanOverWriteToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\OverWriteExcelSheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filename.Value));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);

            var data = new object[]
            {
                new object[] {999, 999, 999},
                new object[] {999, 999, 999},
                new object[] {999, 999, 999}
            };
            AssertPreviewValue(writeNode.GUID.ToString(), data);
        }

        [Test]
        public void CanOverWritePartiallyToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\OverWritePartialExcelSheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filename.Value));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);

            var data = new object[]
            {
                new object[] {1, 1, 1, 1, 1},
                new object[] {1, 999, 999, 999, 1},
                new object[] {1, 999, 999, 999, 1},
                new object[] {1, 999, 999, 999, 1},
                new object[] {1, 1, 1, 1, 1}
            };

            AssertPreviewValue(writeNode.GUID.ToString(), data);
        }

        [Test]
        public void CanWriteNullValuesToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNullValuesToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            // returns empty list
            Assert.AreEqual(0, list.Count());

        }

        [Test]
        public void CanWriteNullValuesToExcel1()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNullValuesToExcel1.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var data = new object[] { new object []{2},new object[]{null},new object [] {3} };
            AssertPreviewValue(writeNode.GUID.ToString(), data);

            
        }

        [Test]
        public void TestWriteFunctionObjectToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteFunctionobject.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();

            ProtoCore.RuntimeCore runtimeCore = ViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.kDefault, warningEntry.ID);

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            // Empty list expected
            Assert.AreEqual(0, list.Count);
        }
        [Test]
        public void WriteToNonExistingFile()
        {

            // 1.  Write data into non existing file  and Run
            // 2.  Dynamo must create the file and write the data

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNonExistingFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("8651eeba-33a9-4704-b92c-346e6e5e4b34");
            ViewModel.HomeSpace.Run();
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] {4} ,new object []{5}};
            AssertPreviewValue(watch.GUID.ToString(), data);
           
        }
        [Test]
        public void WriteNonExistingSheet()
        {

            // 1.  Open Sample ImportExport_Excel_ToDynamo.dyn and Run
            // 2.  NonExisting Sheet
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNonExistingSheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("32dbe300-4780-4c47-b07c-63019d5285ac");
            ViewModel.HomeSpace.Run();
            Assert.IsTrue(watch.CachedValue.IsCollection);
           
            var data = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { 4 }, new object[] { 5 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
        }
       
        public void WritenodeWithWarning()
        {
            // Write a node with warning to Excel.
            // If there is any data in excel it must not overwrite here- currently this fails
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNodewithError.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("32dbe300-4780-4c47-b07c-63019d5285ac");
            ViewModel.HomeSpace.Run();
            
            var list = watch.CachedValue.GetElements();

            // input array is empty list, {}
            // Ensure output is also empty list
            Assert.AreEqual(0, list.Count);

            Assert.IsTrue(watch.CachedValue.IsCollection);
            Assert.IsNull(watch.CachedValue.Data);
   
           
        }
       [Test, Category("Failure")]
        public void WriteToReadOnlyFile()
        {

            // asks for overwriting of file- need to find a way to ignore that.

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteToReadOnlyFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            string filePath = filename.Value;

            // get file attributes
            FileAttributes fileAttributes = File.GetAttributes(filePath);
            if (fileAttributes != FileAttributes.ReadOnly)
                File.SetAttributes(filePath, FileAttributes.ReadOnly);
            
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            ViewModel.HomeSpace.Run();
            Assert.IsNull(watch.CachedValue.Data);
            Assert.IsTrue(watch.State == Models.ElementState.Warning);
            if (fileAttributes == FileAttributes.ReadOnly)
                File.SetAttributes(filePath, FileAttributes.Normal);

        }
        
        [Test]
        public void WriteModifyFilename()
        {

            // 1.  Write into  an Existing excel file and Run
            // 2.  Modify the file name in file path to an existing file
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteModifyPath.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);


            ViewModel.HomeSpace.Run();

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            var watch2 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("5e6a2a5b-7c69-4ab6-b33d-f27267fb2762");
             Assert.IsTrue(watch.CachedValue.IsCollection);
            var list1 = watch.CachedValue.GetElements();
            Assert.AreEqual(6, list1.Count());
            Assert.IsTrue(list1[0].IsCollection);
            var rowList = list1[0].GetElements();
            Assert.AreEqual(5, rowList[0].Data);

            // change the file path 

            ConnectorModel.Make(watch2, watch, 0, 0);
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);


        }
        [Test]
        public void WriteModifyGraph()
        {

            // 1.  Write into  an Existing excel file and Run
            // 2.  Modify the contents written to an excel file wiht option overwrite
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteModifyGraph.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            var watch2 = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile"); // {5,6,7,8,9,10}
            var watch3 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("17bf44dd-0285-496f-a388-58649cadbff8");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
            // change the data written into Excel

            ConnectorModel.Make(watch3, watch2, 0, 4);
            ViewModel.HomeSpace.Run();
            Assert.IsTrue(watch2.CachedValue.IsCollection);
           
            var data2 = new object[] { new object[] { 0 }, new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { 4 }, new object[] { 5 } };
            AssertPreviewValue(watch2.GUID.ToString(), data2);


        }
        [Test,Category("Failure")]
        public void WriteModifyGraph_2()
        {
            // Failing due to -http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7515
            // 1.  Write into  an Existing excel file and Run
            // 2.  Reduce the array size written to an excel file
            //3.  Rerun the file 
            
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteModifyGraph_2.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            var watch2 = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            var watch3 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("17bf44dd-0285-496f-a388-58649cadbff8");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            //from excel - {5,6,7,8,9,10}
            var data = new object[] { new object[] {5}, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

            // decrease the data written into excel
            // Write {2,3,4,5} into excel 
            ConnectorModel.Make(watch3, watch2, 0, 4);
            
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch2.CachedValue.IsCollection);
            
            var data2 = new object[] { new object[] { 2}, new object[] { 3 }, new object[] { 4 }, new object[] { 5 }, new object[] { null}, new object[] { null} };
            AssertPreviewValue(watch2.GUID.ToString(), data2);

        }

        [Test]
        public void WriteModifyGraph_3()
        {
            //incomplete
            // 1.  Write into  an Existing excel file and Run
            // 2.  Increase the array size written to an excel file
            // 3.  Rerun the file 

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteModifyGraph_3.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            var watch2 = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            // Write {5,6,7,8,9,10} into excel
            var watch3 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("17bf44dd-0285-496f-a388-58649cadbff8");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch2.GUID.ToString(), data);
            // Increase the data written into excel

            ConnectorModel.Make(watch3, watch2, 0, 4);
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch2.CachedValue.IsCollection);
            var list2 = watch2.CachedValue.GetElements();
            Assert.AreEqual(8, list2.Count());

            Assert.IsTrue(list2[0].IsCollection);
            var data2 = new object[] { new object[] {0},new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { 4 }, new object[] { 5 }, new object[] { 6 },new object[]{7} };
            AssertPreviewValue(watch2.GUID.ToString(), data2);

        }

        [Test]
        public void TestOverwriteToSingleSheetExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\SingleSheetOverwrite.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();

            var writeNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");

            Assert.IsTrue(File.Exists(filename.Value));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(3, list.Count);
            AssertPreviewValue(writeNode.GUID.ToString(), new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 } });

            var wb = Excel.ReadExcelFile(filename.Value);
            Assert.IsTrue(wb.WorkSheets.Length == 1);
        }

        #endregion

        #region Saving

        [Test]
        public void CanSaveAsWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_SaveAs.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();

            stringNode.Value = filePath;

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));
        }


        #endregion

        #region Defects

        /// <summary>
        /// TODO: This is to verify the fix for the following user report issue.
        /// Note that however this test case does not completely simulate the 
        /// user scenario -- the "Watch.GenerateWatchViewModelForData" does not even get called for 
        /// some reason. This test case passes now, but should be revisit later
        /// for an enhancement which allows "Watch.GenerateWatchViewModelForData" to be called (and 
        /// crash without the fix).
        /// </summary>
        [Test]
        public void Defect_MAGN_883()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\Defect_MAGN_883.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, ViewModel.CurrentSpace.Nodes.Count);

            var workspace = ViewModel.Model.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            ViewModel.HomeSpace.Run();
            Assert.Pass("RunExpression should no longer crash (Defect_MAGN_883)");
        }
        [Test]
        public void WriteIntoExcelVerify()
        {
            // Write Into Excel sheet using WriteToFile
            // Read Using ReadFromFile and verify the values are written correctly into Excel.

            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\WriteFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(7, ViewModel.CurrentSpace.Nodes.Count);

            var workspace = ViewModel.Model.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            Assert.AreEqual(7, ViewModel.CurrentSpace.Nodes.Count);
            ViewModel.HomeSpace.Run();
            //Write into Excel - {5,6,7,8,9,10}
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.WriteToFile");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

            string openPath2 = Path.Combine(testDir, @"core\excel\ReadFile.dyn");
            ViewModel.OpenCommand.Execute(openPath2);
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);

            var filename2 = workspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename2.Value = filename2.Value.Replace(@"..\..\..\test", testDir);
            ViewModel.HomeSpace.Run();
            var watch2 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("78370571-c7bc-4ed8-be2e-90c5be172767");
            Assert.IsTrue(watch2.CachedValue.IsCollection);
            //Read and verify the results are - {5,6,7,8,9,10}
            var data2 = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch2.GUID.ToString(), data2);

        }
        [Test]
        public void TestFormula()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\formula.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);

            var workspace = ViewModel.Model.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            ViewModel.HomeSpace.Run();
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadFromFile");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data2 = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { null }, new object[] { 6 } };
            AssertPreviewValue(watch.GUID.ToString(), data2);
        }
        #endregion
    }
}
