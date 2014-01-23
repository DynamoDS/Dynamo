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
            ExcelInterop.TryQuitAndCleanup(false);
            base.Cleanup();
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
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

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
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(4, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(3, list.Count());

        }

        [Test]
        public void CanGetWorksheetByNameWithValidInput()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\WorksheetByName_ValidInput.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsContainer);

        }

        [Test]
        public void ThrowExceptionOnGetWorksheetByNameWithInvalidInput()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\WorksheetByName_InvalidInput.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var getWorksheet = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<GetExcelWorksheetByName>();
            var readFile = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<ReadExcelFile>();

            Assert.Throws<AssertionException>(() => Controller.RunExpression(null));

            Assert.IsTrue(readFile.OldValue.IsContainer);
            Assert.IsNull(getWorksheet.OldValue);
        }

        [Test]
        public void CanReadWorksheetWithSingleColumnOfNumbers()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_ascending.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(16, list.Count());

            // contents of first workbook is ascending array of numbers starting at 1
            var counter = 1;
            for (var i = 0; i < 16; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsList);
                var rowList = list[i].GetListFromFSchemeValue();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(counter++, rowList[0].GetDoubleFromFSchemeValue());
            }

        }

        [Test]
        public void CanReadMultiDimensionalWorksheet()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_2Dimensional.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(18, list.Count());

            // 18 x 3 array of numbers
            for (var i = 0; i < 16; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsList);
                var rowList = list[i].GetListFromFSchemeValue();
                Assert.AreEqual(3, rowList.Count());

                for (var j = 0; j < 3; j++)
                {
                    Assert.IsTrue(rowList[j].IsNumber);
                }

            }

        }

        [Test]
        public void CanReadWorksheetWithEmptyCellInUsedRange()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_missingCell.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(4, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsList);
            var rowList = list[0].GetListFromFSchemeValue();
            Assert.AreEqual("a", rowList[0].GetStringFromFSchemeValue());

            Assert.IsTrue(list[1].IsList);
            rowList = list[1].GetListFromFSchemeValue();
            Assert.IsTrue(rowList[0].IsContainer);
            Assert.IsNull(((FScheme.Value.Container)rowList[0]).Item);

            Assert.IsTrue(list[2].IsList);
            rowList = list[2].GetListFromFSchemeValue();
            Assert.AreEqual("cell is", rowList[0].GetStringFromFSchemeValue());

            Assert.IsTrue(list[3].IsList);
            rowList = list[3].GetListFromFSchemeValue();
            Assert.AreEqual("missing", rowList[0].GetStringFromFSchemeValue());
        }

        [Test]
        public void CanReadWorksheetWithMixedNumbersAndStrings()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_mixedNumbersAndStrings.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename)Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(5, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsList);
            var rowList = list[0].GetListFromFSchemeValue();
            Assert.AreEqual(1, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[1].IsList);
            rowList = list[1].GetListFromFSchemeValue();
            Assert.AreEqual("word", rowList[0].GetStringFromFSchemeValue());

            Assert.IsTrue(list[2].IsList);
            rowList = list[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[3].IsList);
            rowList = list[3].GetListFromFSchemeValue();
            Assert.AreEqual(3, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[4].IsList);
            rowList = list[4].GetListFromFSchemeValue();
            Assert.AreEqual("palabra", rowList[0].GetStringFromFSchemeValue());

        }

        #endregion

        #region Writing

        [Test]
        public void CanWrite1DDataOfMixedTypesToExcelWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_AddMixed1DData.dyn");
            Controller.DynamoModel.Open(openPath);
            Assert.AreEqual(12, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(5, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsList);
            var rowList = list[0].GetListFromFSchemeValue();
            Assert.AreEqual("doodle", rowList[0].GetStringFromFSchemeValue());

            Assert.IsTrue(list[1].IsList);
            rowList = list[1].GetListFromFSchemeValue();
            Assert.AreEqual(0, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[2].IsList);
            rowList = list[2].GetListFromFSchemeValue();
            Assert.AreEqual(21029, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[3].IsList);
            rowList = list[3].GetListFromFSchemeValue();
            Assert.IsTrue(rowList[0].IsContainer);
            Assert.IsNull(((FScheme.Value.Container)rowList[0]).Item);

            Assert.IsTrue(list[4].IsList);
            rowList = list[4].GetListFromFSchemeValue();
            Assert.AreEqual(-90, rowList[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void CanCreateNewWorksheetInNewWorkbook()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_AddWorksheet.dyn");
            Controller.DynamoModel.Open(openPath);
            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
            Assert.IsTrue(watch.OldValue.IsContainer);
        }

        [Test]
        public void CanAddSingleItemToExcelWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_AddSingleItemData.dyn");
            Controller.DynamoModel.Open(openPath);
            Assert.AreEqual(8, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(1, list.Count());

            // get data returns 2d array
            Assert.IsTrue(list[0].IsList);
            var rowList = list[0].GetListFromFSchemeValue();
            Assert.AreEqual(1, rowList.Count());
            Assert.AreEqual(100.0, rowList[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void CanAdd1DListToExcelWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_Add1DListData.dyn");
            Controller.DynamoModel.Open(openPath);
            Assert.AreEqual(8, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(101, list.Count());

            // contents of first workbook is ascending array of numbers starting at 1
            var counter = 0;
            for (var i = 0; i < 101; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsList);
                var rowList = list[i].GetListFromFSchemeValue();
                Assert.AreEqual(1, rowList.Count());
                Assert.AreEqual(counter++, rowList[0].GetDoubleFromFSchemeValue());
            }

        }

        [Test]
        public void CanAdd2DListToExcelWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_Add2DListData.dyn");
            Controller.DynamoModel.Open(openPath);
            Assert.AreEqual(11, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            // 101 x 5 - each column is 0..100
            Assert.AreEqual(101, list.Count());

            // contents of first workbook is ascending array of numbers starting at 1
            var counter = 0;
            for (var i = 0; i < 101; i++)
            {
                // get data returns 2d array
                Assert.IsTrue(list[i].IsList);
                var rowList = list[i].GetListFromFSchemeValue();
                Assert.AreEqual(5, rowList.Count());
                rowList.ToList().ForEach(x => Assert.AreEqual(counter, x.GetDoubleFromFSchemeValue()));
                counter++;
            }

        }

        [Test]
        public void CanCreateNewWorkbook()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook.dyn");
            Controller.DynamoModel.Open(openPath);
            Assert.AreEqual(2, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Controller.RunExpression(null);
            Assert.IsTrue(watch.OldValue.IsContainer);
        }

        #endregion

        #region Saving

        [Test]
        public void CanSaveAsWorksheet()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\NewWorkbook_SaveAs.dyn");
            Controller.DynamoModel.Open(openPath);

            var filePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlsx";
            var stringNode = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Dynamo.Nodes.String>();

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
        [Ignore, Test]
        public void Defect_MAGN_883()
        {
            string testDir = GetTestDirectory();
            string openPath = Path.Combine(testDir, @"core\excel\Defect_MAGN_883.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var filename = workspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", testDir);
            Controller.RunExpression(null);
            Assert.Pass("RunExpression should no longer crash (Defect_MAGN_883)");
        }

        #endregion
    }
}
