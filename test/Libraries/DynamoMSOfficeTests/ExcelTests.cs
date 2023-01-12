using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CoreNodeModels.Input;
using DSOffice;
using Dynamo.Configuration;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.Mirror;


namespace Dynamo.Tests
{
    [TestFixture, RequiresSTA]
    public class ExcelTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
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

        [Test, Category("ExcelTest")]
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

        [Test, Category("ExcelTest")]
        public void CanGetLargeWorkbookWithinThresholdTime()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\HammersmithExcelFile_Open.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count());

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var timer = new Stopwatch();
            timer.Start();
            ViewModel.HomeSpace.Run();
            timer.Stop();
            Assert.Less(timer.Elapsed.Milliseconds, 1000); // open in less than 1s

        }


        [Test, Category("ExcelTest")]
        public void CanGetWorksheets()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\WorksheetsFromFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetWorksheetsFromExcelWorkbook");

            ViewModel.HomeSpace.Run();

            MirrorData mirror = watch.CachedValue;
            Assert.IsTrue(mirror.IsCollection);

            Assert.AreEqual(3, mirror.GetElements().ToList().Count);
        }

        [Test, Category("ExcelTest")]
        public void CanGetWorksheetByNameWithValidInput()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\WorksheetByName_ValidInput.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count());

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetExcelWorksheetByName");

            ViewModel.HomeSpace.Run();

            Assert.IsNotNull(watch.CachedValue);
        }

        [Test, Category("ExcelTest")]
        public void ThrowExceptionOnGetWorksheetByNameWithInvalidInput()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WorksheetByName_InvalidInput.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var getWorksheet = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetExcelWorksheetByName");
            var readFile = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.ReadExcelFile");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(readFile.CachedValue.Class.ClassName == "DSOffice.WorkBook");
            Assert.IsNull(getWorksheet.CachedValue.Data);
        }

        [Test, Category("ExcelTest")]
        public void CanReadWorksheetWithSingleColumnOfNumbers()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_ascending.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements().ToList();

            Assert.AreEqual(16, list.Count);

            // contents of first workbook is ascending array of numbers starting at 1
            var counter = 1;
            for (var i = 0; i < 16; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements().ToList();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(counter++, rowList[0].Data);
            }
        }

        [Test, Category("ExcelTest")]
        
        public void CanReadMultiDimensionalWorksheet()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_2Dimensional.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements().ToList();
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

        [Test, Category("ExcelTest")]
        public void CanReadWorksheetWithEmptyCellInUsedRange()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_missingCell.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();
            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();
            //Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] {new object []{"a"},new object []{null},new object []{"cell is"},new object[] {"missing"} };
            AssertPreviewValue(watch.GUID.ToString(), data);
        }

        [Test, Category("ExcelTest")]
        public void CanReadWorksheetWithMixedNumbersAndStrings()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\DataFromFile_mixedNumbersAndStrings.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] {1},new object []{"word"},new object[]{2},new object[] {3},new object[]{"palabra" }};
            
            AssertPreviewValue(watch.GUID.ToString(), data);

        }
        [Test, Category("ExcelTest")]
        public void ReadOnlyFile()
        {
            //Read from readonly Excel file and make sure Excel node in Dynamo displays value correctly.

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadOnlyFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("de5c9439-bc4c-408d-9484-798d8d8b8aed");
            ViewModel.HomeSpace.Run();
            var data = new object[] { new object[] { 4 }, new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

        }
        [Test, Category("ExcelTest")]
        public void CanReadAndWriteExcel()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadAndWriteExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            // watch displays the data from the Read node
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("de5c9439-bc4c-408d-9484-798d8d8b8aed");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            var data = new object[] { new object[] { 1 }, new object[] { "word" }, new object[] { 2 }, new object[] { 3 }, new object[] { "palabra" } };

            AssertPreviewValue(watch.GUID.ToString(), data);

            
        }
        [Test, Category("ExcelTest")]
        public void ReadChangeFilename()
        {

            // 1.  Read from an Existing excel file and Run
            // 2.  Modify the file name in file path to an existing file
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\ChangeFilename.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var fileNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<Filename>();
            foreach (var file in fileNodes)
            {
                file.Value.Replace(@"..\..\..\test", TestDirectory);
            }

            // remap the filename as Excel requires an absolute path

            ViewModel.HomeSpace.Run();
            
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("de5c9439-bc4c-408d-9484-798d8d8b8aed");
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
        [Test, Category("ExcelTest")]
        public void ReadNonExistingFile()
        {

            // 1.  Read from a non existing excel file and Run           
            // 2.  Rerun the file  - it must throw a warning
            
            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadNonExistingFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var newname = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("de5c9439-bc4c-408d-9484-798d8d8b8aed");
            ViewModel.HomeSpace.Run();
           
            Assert.IsTrue(watch.State == ElementState.Warning);
           

        }
        

        [Test, Category("ExcelTest")]
        public void CanReadExcelAsStrings()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadExcelAsStrings.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            // watch displays the data from the Read node
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("de5c9439-bc4c-408d-9484-798d8d8b8aed");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);

            var data = new object[] { new object[] { "1" }, new object[] { "word" }, new object[] { "2" }, new object[] { "3" }, new object[] { "palabra" } };
            AssertPreviewValue(watch.GUID.ToString(), data);


        }

        [Test, Category("ExcelTest")]
        public void CanReadEmptyCellsAsStrings()
        {

            string openPath = Path.Combine(TestDirectory, @"core\excel\ReadEmptyCellsAsStrings.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            // watch displays the data from the Read node
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("e1fdbd63-f1b1-43df-8a34-057600f7b925");
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

        [Test, Category("ExcelTest")]
        public void CanWrite1DDataOfMixedTypesToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddMixed1DData.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(13, ViewModel.CurrentSpace.Nodes.Count());
            
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { "doodle" }, new object[] { 0.000 }, new object[] { 21029.000 }, new object[] { null }, new object[] { -90.000 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

        }
        [Test, Category("ExcelTest")]
        public void Excel_MAGN6872()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6872
            
            
            string openPath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN6872.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(9, ViewModel.CurrentSpace.Nodes.Count());
        
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("cda3623e-f092-4488-8957-250b6a43a2dc");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements().ToList();

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
        [Test, Category("ExcelTest")]
        public void Excel_OverwriteWithnull_7213()
        {
            // Overwrite the excel sheet with null values and it must work ok.
            string openPath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN7213.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(6, ViewModel.CurrentSpace.Nodes.Count());

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("d5388751-3c9f-4b48-8498-3cf5ff5f88e2");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);

            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] {3 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
            
        }

        [Test, Category("ExcelTest")]
        public void Excel_OverWriteEmptyList_MAGN7213()
        {

            //Overwrite an existing file with EmptyList- it must run ok
            string openPath = Path.Combine(TestDirectory, @"core\excel\Excel_MAGN7213_2.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(7, ViewModel.CurrentSpace.Nodes.Count());
            
            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("d5388751-3c9f-4b48-8498-3cf5ff5f88e2");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] { 3 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
            
            
        }
        [Test, Category("ExcelTest")]
        public void CanCreateNewWorksheetInNewWorkbook()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddWorksheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count());
            var getWorksheet = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetExcelWorksheetByName");
            ViewModel.HomeSpace.Run();
            Assert.IsNull(getWorksheet.CachedValue.Data);
        }

        [Test, Category("ExcelTest")]
        public void CanAddSingleItemToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddSingleItemData.dyn");

            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, ViewModel.CurrentSpace.Nodes.Count());
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements().ToList();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements().ToList();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(100.0, rowList[0].Data);
        }

        /// <summary>
        /// This test method will execute the Excel.WriteDataToExcelWorksheet method trying to write a null value in a excel file
        /// </summary>
        [Test, Category("ExcelTest")]
        public void CanAddNullItemToExcelWorksheet()
        {
            //Arrange
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_AddNullItemData.dyn");

            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, ViewModel.CurrentSpace.Nodes.Count());
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            //Act
            //In the dyn file Dinamo will try to write a null value got from a CodeBlock node, internally will execute the WriteDataToExcelWorksheet method
            ViewModel.HomeSpace.Run();

            //Assert
            //Verify that the data is null 
            Assert.IsNull(watch.CachedValue.Data);
        }

        [Test, Category("ExcelTest")]
        public void CanAdd1DListToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_Add1DListData.dyn");

            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, ViewModel.CurrentSpace.Nodes.Count());
            var previewNode = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(previewNode.CachedValue.IsCollection);
            var list = previewNode.CachedValue.GetElements().ToList();

            Assert.AreEqual(101, list.Count());

            // contents of first workbook is ascending array of numbers starting at 1
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

        [Test, Category("ExcelTest")]
        public void CanAdd2DListToExcelWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_Add2DListData.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(11, ViewModel.CurrentSpace.Nodes.Count());
            var watch = ViewModel.Model.CurrentWorkspace.GetDSFunctionNodeFromWorkspace("Excel.GetDataFromExcelWorksheet");

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements().ToList();

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

        [Test, Category("ExcelTest")]
        public void CanWriteToExcelAndUpdateData()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNodeAndUpdateData.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("f1839832-130c-47ed-ada6-200abc6f8a86");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements().ToList();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements().ToList();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("BBB", rowList[0].Data);

            var stringNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<StringInput>();
            var inputStringNode = stringNodes.Where(x => x.Value == "BBB").FirstOrDefault();
            inputStringNode.Value = "AAA";

            ViewModel.HomeSpace.Run();

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            list = writeNode.CachedValue.GetElements().ToList();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            rowList = list[0].GetElements().ToList();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("AAA", rowList[0].Data);

        }

        [Test, Category("ExcelTest")]
        public void CanWriteJaggedArrayToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteJaggedArrayToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            // Makes use of UnitTestBase.TempFolder since that folder is always 
            // guaranteed to be different for each test, and will get deleted when 
            // the test shuts down.
            // 
            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("f1839832-130c-47ed-ada6-200abc6f8a86");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1,1,null }, new object[] {2,2,2 }, new object []{3,3,3},new object[] { null,4,null }};
            AssertPreviewValue(writeNode.GUID.ToString(), data);

        }

        [Test, Category("ExcelTest")]
        public void CanWriteEmptyArrayToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteEmptyArrayToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("9bce1a0e-8623-4360-b48b-c0db96f47c0b");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { 2 }, new object[] { null }, new object[] {3 } };
            AssertPreviewValue(writeNode.GUID.ToString(), data);
            
            
        }

        [Test, Category("ExcelTest")]
        public void CanWriteNestedEmptyListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNestedEmptyListToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("9ccbd3c2-ca04-4460-809c-7c5205dbf929");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filename.Value));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var data = new object[] { new object[] { 99,99,99},new object[] { 99 ,99,99}, new object[] { 99,null,99 }};
            AssertPreviewValue(writeNode.GUID.ToString(), data);

            // input array is nested empty list, {{}, {}, {}}
            // Ensure output is {{99, 99, 99}, {99, 99, 99}, {99, "", 99}}
            

        }

        [Test, Category("ExcelTest")]
        public void CanWriteEmptyListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteEmptyListToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("9bce1a0e-8623-4360-b48b-c0db96f47c0b");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements().ToList();

            // input array is empty list, {}
            // Ensure output is also empty list
            Assert.AreEqual(0, list.Count);

        }

        [Test, Category("ExcelTest")]
        public void CanOverWriteToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\OverWriteExcelSheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("763670b1-c982-493c-9b40-5716972b82ca");

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

        [Test, Category("ExcelTest")]
        public void CanOverWritePartiallyToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\OverWritePartialExcelSheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("763670b1-c982-493c-9b40-5716972b82ca");

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

        [Test, Category("ExcelTest")]
        public void CanWriteNullValueToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNullValuesToExcel.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("a629fc97-1f9e-438f-a9ba-748339e04acd");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            // returns empty list
            Assert.AreEqual(0, list.Count());

        }

        [Test, Category("ExcelTest")]
        public void CanWriteNullValueInListToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNullValuesToExcel1.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("9bce1a0e-8623-4360-b48b-c0db96f47c0b");
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var data = new object[] { new object []{2},new object[]{null},new object [] {3} };
            AssertPreviewValue(writeNode.GUID.ToString(), data);

            
        }

        [Test, Category("ExcelTest")]
        public void TestWriteFunctionObjectToExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteFunctionobject.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();
            stringNode.Value = filePath;

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("f236e4bd-2c1c-4846-b632-ff318f3cab42");
            ViewModel.HomeSpace.Run();

            ProtoCore.RuntimeCore runtimeCore = ViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.Default, warningEntry.ID);

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements().ToList();

            // Empty list expected
            Assert.AreEqual(0, list.Count);
        }
        [Test, Category("ExcelTest")]
        public void WriteToNonExistingFile()
        {

            // 1.  Write data into non existing file  and Run
            // 2.  Dynamo must create the file and write the data

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNonExistingFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("8651eeba-33a9-4704-b92c-346e6e5e4b34");
            ViewModel.HomeSpace.Run();
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] {4} ,new object []{5}};
            AssertPreviewValue(watch.GUID.ToString(), data);
           
        }
        [Test, Category("ExcelTest")]
        public void WriteNonExistingSheet()
        {

            // 1.  Open Sample ImportExport_Excel_ToDynamo.dyn and Run
            // 2.  NonExisting Sheet
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteNonExistingSheet.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

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

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("32dbe300-4780-4c47-b07c-63019d5285ac");
            ViewModel.HomeSpace.Run();
            
            var list = watch.CachedValue.GetElements().ToList();

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

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);
            string filePath = filename.Value;

            // get file attributes
            FileAttributes fileAttributes = File.GetAttributes(filePath);
            if (fileAttributes != FileAttributes.ReadOnly)
                File.SetAttributes(filePath, FileAttributes.ReadOnly);
            
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("88fa7b79-ddc3-4d80-9546-e377cf2434a5");
            ViewModel.HomeSpace.Run();
            Assert.IsNull(watch.CachedValue.Data);
            Assert.IsTrue(watch.State == ElementState.Warning);
            if (fileAttributes == FileAttributes.ReadOnly)
                File.SetAttributes(filePath, FileAttributes.Normal);
        }

        /// <summary>
        /// This will execute the Data.ImportExcel method
        /// </summary>
        [Test, Category("ExcelTest")]
        public void ImportExcelShowExcelReturnDataObjectArray()
        {
            //Arrange
            string openPath = Path.Combine(TestDirectory, @"core\excel\ImportExcelFileShowArray.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            //Act
            //When executing the Graph will call the Data.ImportExcel method
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("ba33d59ba05648d68d2d9aadc42ae07a");
            ViewModel.HomeSpace.Run();
            var data = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }};

            //Assert
            //Validates that the data fetch from the excel workbook are 3 elements and match the values in data array
            Assert.IsNotNull(watch);
            AssertPreviewValue(watch.GUID.ToString(), data);
            
        }

        

        [Test, Category("ExcelTest")]
        public void WriteModifyFilename()
        {

            // 1.  Write into  an Existing excel file and Run
            // 2.  Modify the file name in file path to an existing file
            //3.  Rerun the file  
            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteModifyPath.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);


            ViewModel.HomeSpace.Run();

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("88fa7b79-ddc3-4d80-9546-e377cf2434a5");
            var watch2 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("5e6a2a5b-7c69-4ab6-b33d-f27267fb2762");
             Assert.IsTrue(watch.CachedValue.IsCollection);
            var list1 = watch.CachedValue.GetElements().ToList();
            Assert.AreEqual(6, list1.Count());
            Assert.IsTrue(list1[0].IsCollection);
            var rowList = list1[0].GetElements().ToList();
            Assert.AreEqual(5, rowList[0].Data);

            // change the file path 

            ConnectorModel.Make(watch2, watch, 0, 0);
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);
        }
       

        [Test, Category("ExcelTest")]
        public void WriteModifyGraph_3()
        {
            //incomplete
            // 1.  Write into  an Existing excel file and Run
            // 2.  Increase the array size written to an excel file
            // 3.  Rerun the file 

            string openPath = Path.Combine(TestDirectory, @"core\excel\WriteModifyGraph_3.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("78370571-c7bc-4ed8-be2e-90c5be172767");
            var watch2 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("88fa7b79-ddc3-4d80-9546-e377cf2434a5");
            // Write {5,6,7,8,9,10} into excel
            var watch3 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("17bf44dd-0285-496f-a388-58649cadbff8");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch2.GUID.ToString(), data);
            // Increase the data written into excel

            ConnectorModel.Make(watch3, watch2, 0, 4);
            ViewModel.HomeSpace.Run();

            Assert.IsTrue(watch2.CachedValue.IsCollection);
            var list2 = watch2.CachedValue.GetElements().ToList();
            Assert.AreEqual(8, list2.Count());

            Assert.IsTrue(list2[0].IsCollection);
            var data2 = new object[] { new object[] {0},new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { 4 }, new object[] { 5 }, new object[] { 6 },new object[]{7} };
            AssertPreviewValue(watch2.GUID.ToString(), data2);

        }

        [Test, Category("ExcelTest")]
        public void TestOverwriteToSingleSheetExcel()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\SingleSheetOverwrite.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();

            var writeNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("37081341-ba10-435b-b775-5f26db443197");
            Assert.IsTrue(File.Exists(filename.Value));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements().ToList();

            Assert.AreEqual(3, list.Count);
            AssertPreviewValue(writeNode.GUID.ToString(), new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 } });

            var wb = Excel.ReadExcelFile(filename.Value);
            Assert.IsTrue(wb.WorkSheets.Length == 1);
        }


        [Test, Category("ExcelTest")]
        public void CanExportToExcelAsString()
        {
            // Arrange
            string openPath = Path.Combine(TestDirectory, @"core\excel\ExportToExcelAsString.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            // Act
            // remap the filename as Excel requires an absolute path
            filename.Value = filename.HintPath.Replace(@"..\..\..\test", TestDirectory);

            ViewModel.HomeSpace.Run();

            // Codeblock that holds the data that will be exported (value = 12)
            var dataCodeBlock = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("8070c45660c349d5bbc143febd22cfcf") as CodeBlockNodeModel;           
            var wb = Excel.ReadExcelFile(filename.Value);

            // Running the graph exports the value 12 to excel, 
            // getting that value here so we can check that the value has been converted to a string.
            var excelExportDataType = wb.WorkSheets
                .FirstOrDefault()
                .Data
                .FirstOrDefault()
                .FirstOrDefault()
                .GetType();

            // Assert
            Assert.AreEqual(typeof(Int64), dataCodeBlock.CachedValue.Data.GetType());
            Assert.AreEqual(typeof(string), excelExportDataType);
        }

        #endregion

        #region Saving

        [Test, Category("ExcelTest")]
        public void CanSaveAsWorksheet()
        {
            string openPath = Path.Combine(TestDirectory, @"core\excel\NewWorkbook_SaveAs.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.Combine(TempFolder, "output.xlsx");
            var stringNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>();

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
        [Test, Category("ExcelTest")]
        public void Defect_MAGN_883()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\Defect_MAGN_883.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, ViewModel.CurrentSpace.Nodes.Count());

            var workspace = ViewModel.Model.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            ViewModel.HomeSpace.Run();
            Assert.Pass("RunExpression should no longer crash (Defect_MAGN_883)");
        }
        [Test, Category("ExcelTest")]
        public void WriteIntoExcelVerify()
        {
            // Write Into Excel sheet using WriteToFile
            // Read Using ReadFromFile and verify the values are written correctly into Excel.

            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\WriteFile.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(7, ViewModel.CurrentSpace.Nodes.Count());

            var workspace = ViewModel.Model.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            Assert.AreEqual(7, ViewModel.CurrentSpace.Nodes.Count());
            ViewModel.HomeSpace.Run();
            //Write into Excel - {5,6,7,8,9,10}
            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("88fa7b79-ddc3-4d80-9546-e377cf2434a5");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            
            var data = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch.GUID.ToString(), data);

            string openPath2 = Path.Combine(testDir, @"core\excel\ReadFile.dyn");
            ViewModel.OpenCommand.Execute(openPath2);
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count());

            var filename2 = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename2.Value = filename2.Value.Replace(@"..\..\..\test", testDir);
            ViewModel.HomeSpace.Run();
            var watch2 = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("78370571-c7bc-4ed8-be2e-90c5be172767");
            Assert.IsTrue(watch2.CachedValue.IsCollection);
            //Read and verify the results are - {5,6,7,8,9,10}
            var data2 = new object[] { new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 }, new object[] { 9 }, new object[] { 10 } };
            AssertPreviewValue(watch2.GUID.ToString(), data2);

        }

        /// <summary>
        /// This test method will validate the execution of the WorkSheet.ConvertToDimensionalArray passing as a parameter the next data types
        /// - float
        /// - DateTime
        /// - null
        /// - StackValue
        /// </summary>
        [Test, Category("ExcelTest")]
        public void WriteIntoExcelSeveralTypesVerify()
        {
            //Arrange
            string testDir = TestDirectory;
            string excelPath = Path.Combine(testDir, @"core\excel\WriteFileSeveralTypes.xlsx");

            //Act
            var wb = Excel.ReadExcelFile(excelPath);

            //Creates a two dimentional array with 6 elements
            var data2 = new object[][] { new object[] { 5 }, new object[] { (float)6.1312 }, new object[] { System.DateTime.Now }, new object[] { null }, new object[] { StackValue.BuildInt(0) }, new object[] { 10 } };

            //Validates that the elements read from the excel file are 6
            Assert.AreEqual(wb.WorkSheets.First().Data.Length, 6);

            //This will write the data in the first WorkSheet adding 6 rows and internally it executes the WorkSheet.ConvertToDimensionalArray method
            wb.WorkSheets.First().WriteData(6, 0, data2);

            //Assert
            //Validates that the elements written in the excel file are 12 (6 already existing + 6 new values written)
            Assert.AreEqual(wb.WorkSheets.First().Data.Length, 12);
        }


        [Test, Category("ExcelTest")]
        public void TestFormula()
        {
            string testDir = TestDirectory;
            string openPath = Path.Combine(testDir, @"core\excel\formula.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count());

            var workspace = ViewModel.Model.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            ViewModel.HomeSpace.Run();

            var watch = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("78370571-c7bc-4ed8-be2e-90c5be172767");
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var data2 = new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 }, new object[] { null }, new object[] { 6 } };
            AssertPreviewValue(watch.GUID.ToString(), data2);
        }     
        #endregion
    }

    [TestFixture]
    public class CSVTests : UnitTestBase
    {
        [Test]
        [Category("UnitTests")]
        public static void ImportCSV_PathTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\not a valid file.csv");
            Assert.Throws<FileNotFoundException>(() => Data.ImportCSV(filePath));
        }

        [Test]
        [Category("UnitTests")]
        public static void ImportCSVTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\test1.csv");
            var CSVList = Data.ImportCSV(filePath);
            Assert.AreEqual(CSVList, new List<object> {
                new List<object> { 2, 3, 4 },
                new List<object> { 4, 6, 8 },
                new List<object> { 6, 9, 12 },
                new List<object> { 8, 12, 16 }
            });
        }

        /// <summary>
        /// This will execute the Data.ImportCSV when reading a csv in which the columns and rows have different length
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public static void ImportCSVRowsGreaterThanColumnsTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\test3.csv");
            var CSVList = Data.ImportCSV(filePath);
            Assert.AreEqual(CSVList, new List<object> {
                new List<object> { 2,       1,      3,      1,      5,      null},
                new List<object> { 5,       2,      4,      2,      null,   null},
                new List<object> { 6,       null,   5,      null,   null,   null},
                new List<object> { 7,       null,   null,   null,   null,   null}
            });
        }


        [Test]
        [Category("UnitTests")]
        public static void ImportCSVWithTransposeTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\test2.csv");
            var CSVList = Data.ImportCSV(filePath, true);
            Assert.AreEqual(CSVList, new List<object> {
                new List<object> { 1.2, 5.6, 7, 155 },
                new List<object> { 2, 0.009, 10, 3.3 },
                new List<object> { null, 3, 3, null }
            });
        }

        [Test, Category("UnitTests")]
        public void ExportCSVTest()
        {
            //Write data to CSV
            var data =
                Enumerable.Range(0, 10)
                    .Select(row => Enumerable.Range(0, 10).Select(col => row + col).Cast<object>().ToArray())
                    .ToArray();
            var fn = GetNewFileNameOnTempPath(".csv");
            Data.ExportCSV(fn, data);

            //Confirm it's correct
            Assert.AreEqual(data, Data.ImportCSV(fn));
        }

        [Test]
        [Category("UnitTests")]
        public static void OpemXML_ImportExcelTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\OpenXML-ImportExcel.xlsx");
            var data = Data.OpenXMLImportExcel(filePath, "worksheet1", 0, 0, false);
            string cellValueWithFormula = data[0][2].ToString();
            string cellValueWithoutFormula = data[1][2].ToString();
            Assert.AreEqual(cellValueWithFormula, cellValueWithoutFormula);
        }
    }
}
