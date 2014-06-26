using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dynamo;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class ExcelTests : DynamoUnitTest
    {
        [SetUp]
        public override void Init()
        {
            base.Init();
            // hide the excel window for tests
            ExcelInterop.ShowOnStartup = false;

            // In unit-test scenario we are redirecting 'PreferenceSettings' to 
            // load from a non-existing preference XML file. That way each test 
            // will result in an instance of 'PreferenceSettings' with its default 
            // values (since the underlying file wouldn't have existed). This 
            // ensures the preference value change in one test case (if any) does 
            // not get persisted across to the subsequent test case.
            // 
            PreferenceSettings.DYNAMO_TEST_PATH = Path.Combine(TempFolder, "UserPreferenceTest.xml");
        }

        [TearDown]
        public override void Cleanup()
        {
            try
            {
                EventArgs args = new Dynamo.Nodes.ExcelCloseEventArgs(false);
                Controller.ShutDown(false, args);
                this.Controller = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();

            GC.Collect();
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
            //Controller.DynamoModel.OnCleanup(null);
            //Thread.Sleep(100); 
            //Assert.IsFalse( ExcelInterop.IsExcelProcessRunning );
            //Assert.IsFalse(ExcelInterop.HasExcelReference);
        }

        #endregion

        #region Reading

        [Test]
        public void CanGetLargeWorkbookWithinThresholdTime()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\HammersmithExcelFile_Open.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();
                        
            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var timer = new Stopwatch();
            timer.Start();
            Controller.RunExpression(null);
            timer.Stop();
            Assert.Less(timer.Elapsed.Milliseconds, 1000); // open in less than 1s

        }


        [Test]
        public void CanGetWorksheets()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\WorksheetsFromFile.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(4, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(3, list.Count());

        }

        [Test]
        public void CanGetWorksheetByNameWithValidInput()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\WorksheetByName_ValidInput.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.CachedValue.Class.ClassName == "DSOffice.WorkSheet");

        }

        [Test]
        public void ThrowExceptionOnGetWorksheetByNameWithInvalidInput()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\WorksheetByName_InvalidInput.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var getWorksheet = Controller.DynamoModel.CurrentWorkspace.Nodes.Where(node => node is DSFunction && 
                node.NickName == "Excel.GetExcelWorksheetByName").FirstOrDefault();
            var readFile = Controller.DynamoModel.CurrentWorkspace.Nodes.Where(node => node is DSFunction &&
                node.NickName == "Excel.ReadExcelFile").FirstOrDefault();

            //Assert.Throws<AssertionException>(() => Controller.RunExpression(null));
            Controller.RunExpression(null);

            Assert.IsTrue(readFile.CachedValue.Class.ClassName == "DSOffice.WorkBook");
            Assert.IsNull(getWorksheet.CachedValue.Data);
        }

        [Test]
        public void CanReadWorksheetWithSingleColumnOfNumbers()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_ascending.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(16, list.Count());

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

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_2Dimensional.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(18, list.Count());

            // 18 x 3 array of numbers
            for (var i = 0; i < 18; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsCollection);
                var rowList = list[i].GetElements();
                Assert.AreEqual(3, rowList.Count());

                for (var j = 0; j < 3; j++)
                {
                    Assert.AreEqual(rowList[j].Data, (i+1)+j);
                }
            }
        }

        [Test]
        public void CanReadWorksheetWithEmptyCellInUsedRange()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_missingCell.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

            Assert.AreEqual(4, list.Count());

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

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_mixedNumbersAndStrings.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

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

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\ReadAndWriteExcel.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(8, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (DSCore.File.Filename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var filePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlsx";
            var stringNode = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            // watch displays the data from the Read node
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            // writeNode should have the same data contained in watch
            var writeNode = Controller.DynamoModel.CurrentWorkspace.Nodes.Where(x => x is DSFunction &&
                x.NickName == "Excel.Write").FirstOrDefault();

            Controller.RunExpression(null);

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
            rowList = list1[0].GetElements();
            Assert.AreEqual(1, rowList[0].Data);

            Assert.IsTrue(list1[1].IsCollection);
            rowList = list1[1].GetElements();
            Assert.AreEqual("word", rowList[0].Data);

            Assert.IsTrue(list1[2].IsCollection);
            rowList = list1[2].GetElements();
            Assert.AreEqual(2, rowList[0].Data);

            Assert.IsTrue(list1[3].IsCollection);
            rowList = list1[3].GetElements();
            Assert.AreEqual(3, rowList[0].Data);

            Assert.IsTrue(list1[4].IsCollection);
            rowList = list1[4].GetElements();
            Assert.AreEqual("palabra", rowList[0].Data);

        }

        #endregion

        #region Writing

        [Test]
        public void CanWrite1DDataOfMixedTypesToExcelWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_AddMixed1DData.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(13, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);

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
            //Assert.IsTrue(rowList[0].IsContainer);
            Assert.IsNull(rowList[0].Data);

            Assert.IsTrue(list[4].IsCollection);
            rowList = list[4].GetElements();
            Assert.AreEqual(-90, rowList[0].Data);

        }

        [Test]
        public void CanCreateNewWorksheetInNewWorkbook()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_AddWorksheet.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            var getWorksheet = Controller.DynamoModel.CurrentWorkspace.Nodes.Where(node => node is DSFunction &&
                node.NickName == "Excel.GetExcelWorksheetByName").FirstOrDefault();
            Controller.RunExpression(null);
            Assert.AreEqual(watch.CachedValue.Class.ClassName, "DSOffice.WorkSheet");
            Assert.IsNull(getWorksheet.CachedValue.Data);
        }

        [Test]
        public void CanAddSingleItemToExcelWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_AddSingleItemData.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
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
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_Add1DListData.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(8, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
            Assert.IsTrue(watch.CachedValue.IsCollection);
            var list = watch.CachedValue.GetElements();

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
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_Add2DListData.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(11, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.Nodes.Where(x => x is DSFunction &&
                x.NickName == "Excel.GetDataFromExcelWorksheet").FirstOrDefault();

            Controller.RunExpression(null);

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
        public void CanCreateNewWorkbook()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            Assert.AreEqual(2, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
            Assert.AreEqual(watch.CachedValue.Class.ClassName, "DSOffice.WorkBook");
        }

        [Test]
        public void CanWriteToExcelAndUpdateData()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\WriteNodeAndUpdateData.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlsx";
            var stringNode = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();
            stringNode.Value = filePath;

            var writeNode = Controller.DynamoModel.CurrentWorkspace.Nodes.Where(x => x is DSFunction &&
                x.NickName == "Excel.Write").FirstOrDefault();

            Controller.RunExpression(null);

            Assert.IsTrue(File.Exists(filePath));

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            var list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            var rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("BBB", rowList[0].Data);

            var stringNodes = Controller.DynamoModel.CurrentWorkspace.Nodes.OfType<Dynamo.Nodes.StringInput>();
            var inputStringNode = stringNodes.Where(x => x.Value == "BBB").FirstOrDefault();
            inputStringNode.Value = "AAA";

            Controller.RunExpression(null);

            Assert.IsTrue(writeNode.CachedValue.IsCollection);
            list = writeNode.CachedValue.GetElements();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsCollection);
            rowList = list[0].GetElements();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual("AAA", rowList[0].Data);

        }


        #endregion

        #region Saving

        [Test]
        public void CanSaveAsWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_SaveAs.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            var filePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlsx";
            var stringNode = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.StringInput>();

            stringNode.Value = filePath;

            Controller.RunExpression(null);

            Assert.IsTrue(File.Exists(filePath));
        }

       
        #endregion

        #region Defects

        /// <summary>
        /// TODO: This is to verify the fix for the following user report issue.
        /// Note that however this test case does not completely simulate the 
        /// user scenario -- the "Watch.Process" does not even get called for 
        /// some reason. This test case passes now, but should be revisit later
        /// for an enhancement which allows "Watch.Process" to be called (and 
        /// crash without the fix).
        /// </summary>
        [Test]
        public void Defect_MAGN_883()
        {
            string testDir = GetTestDirectory();
            string openPath = Path.Combine(testDir, @"core\excel\Defect_MAGN_883.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            Controller.RunExpression(null);
            Assert.Pass("RunExpression should no longer crash (Defect_MAGN_883)");
        }

        #endregion
    }
}
