using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DSOffice;
using NUnit.Framework;
using ProtoCore.Mirror;

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
        [Category("Failure")]
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

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(4, list.Count);

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual("a", rowList[0].Data);

            Assert.IsTrue(list[1].IsCollection);
            rowList = list[1].GetElements();
            Assert.IsNull(rowList[0].Data);

            Assert.IsTrue(list[2].IsCollection);
            rowList = list[2].GetElements();
            Assert.AreEqual("cell is", rowList[0].Data);

            Assert.IsTrue(list[3].IsCollection);
            rowList = list[3].GetElements();
            Assert.AreEqual("missing", rowList[0].Data);
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
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(5, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList[0].Data);

            Assert.IsTrue(list[1].IsCollection);
            rowList = list[1].GetElements();
            Assert.AreEqual("word", rowList[0].Data);

            Assert.IsTrue(list[2].IsCollection);
            rowList = list[2].GetElements();
            Assert.AreEqual(2, rowList[0].Data);

            Assert.IsTrue(list[3].IsCollection);
            rowList = list[3].GetElements();
            Assert.AreEqual(3, rowList[0].Data);

            Assert.IsTrue(list[4].IsCollection);
            rowList = list[4].GetElements();
            Assert.AreEqual("palabra", rowList[0].Data);

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

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list1 = writeNode.CachedValue.GetElements();

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list2 = watch.CachedValue.GetElements();

            Assert.AreEqual(5, list1.Count());
            Assert.AreEqual(5, list2.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list2[0].IsCollection);
            var rowList = list2[0].GetElements();
            Assert.AreEqual(1, rowList[0].Data);

            Assert.IsTrue(list2[1].IsCollection);
            rowList = list2[1].GetElements();
            Assert.AreEqual("word", rowList[0].Data);

            Assert.IsTrue(list2[2].IsCollection);
            rowList = list2[2].GetElements();
            Assert.AreEqual(2, rowList[0].Data);

            Assert.IsTrue(list2[3].IsCollection);
            rowList = list2[3].GetElements();
            Assert.AreEqual(3, rowList[0].Data);

            Assert.IsTrue(list2[4].IsCollection);
            rowList = list2[4].GetElements();
            Assert.AreEqual("palabra", rowList[0].Data);

            Assert.IsTrue(list1[0].IsCollection);
            var rowList2 = list1[0].GetElements();
            Assert.AreEqual(1, rowList2[0].Data);

            Assert.IsTrue(list1[1].IsCollection);
            rowList2 = list1[1].GetElements();
            Assert.AreEqual("word", rowList2[0].Data);

            Assert.IsTrue(list1[2].IsCollection);
            rowList2 = list1[2].GetElements();
            Assert.AreEqual(2, rowList2[0].Data);

            Assert.IsTrue(list1[3].IsCollection);
            rowList2 = list1[3].GetElements();
            Assert.AreEqual(3, rowList2[0].Data);

            Assert.IsTrue(list1[4].IsCollection);
            rowList2 = list1[4].GetElements();
            Assert.AreEqual("palabra", rowList2[0].Data);

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
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(5, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual("doodle", rowList[0].Data);

            Assert.IsTrue(list[1].IsCollection);
            rowList = list[1].GetElements();
            Assert.AreEqual(0, rowList[0].Data);

            Assert.IsTrue(list[2].IsCollection);
            rowList = list[2].GetElements();
            Assert.AreEqual(21029, rowList[0].Data);

            Assert.IsTrue(list[3].IsCollection);
            rowList = list[3].GetElements();
            Assert.IsNull(rowList[0].Data);

            Assert.IsTrue(list[4].IsCollection);
            rowList = list[4].GetElements();
            Assert.AreEqual(-90, rowList[0].Data);

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
            var list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(4, list.Count());

            // get data returns 2d array
            // input array is Transpose of {{1,2,3,null}, {1,2,3,4}, {null,2,3}}
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual(3, rowList.Count());
            Assert.AreEqual(null, rowList[2].Data);

            Assert.IsTrue(list[3].IsCollection);
            rowList = list[3].GetElements();
            Assert.AreEqual(3, rowList.Count());
            Assert.AreEqual(null, rowList[0].Data);
            Assert.AreEqual(null, rowList[2].Data);

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
            var list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(3, list.Count());

            // get data returns 1d array
            // input array is {2, {}, 3}
            var rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(2, rowList[0].Data);

            rowList = list[1].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(null, rowList[0].Data);

            rowList = list[2].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(3, rowList[0].Data);

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
            var list = writeNode.CachedValue.GetElements();

            // input array is nested empty list, {{}, {}, {}}
            // Ensure output is {{99, 99, 99}, {99, 99, 99}, {99, "", 99}}
            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual(3, rowList.Count);
            Assert.AreEqual(99, rowList[0].Data);
            Assert.AreEqual(99, rowList[1].Data);
            Assert.AreEqual(99, rowList[2].Data);

            Assert.IsTrue(list[1].IsCollection);
            rowList = list[1].GetElements();
            Assert.AreEqual(3, rowList.Count);
            Assert.AreEqual(99, rowList[0].Data);
            Assert.AreEqual(99, rowList[1].Data);
            Assert.AreEqual(99, rowList[2].Data);

            Assert.IsTrue(list[2].IsCollection);
            rowList = list[2].GetElements();
            Assert.AreEqual(3, rowList.Count);
            Assert.AreEqual(99, rowList[0].Data);
            Assert.AreEqual(null, rowList[1].Data);
            Assert.AreEqual(99, rowList[2].Data);

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
            var list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(3, list.Count());

            // get data returns 1d array
            // input array is {2, null, 3}
            var rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(2, rowList[0].Data);

            rowList = list[1].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(null, rowList[0].Data);

            rowList = list[2].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(3, rowList[0].Data);

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

        #endregion
    }
}
