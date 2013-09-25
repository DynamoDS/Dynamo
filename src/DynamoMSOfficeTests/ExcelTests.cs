using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoMSOfficeTests
{
    [TestFixture]
    public class ExcelTests : DynamoUnitTest
    {

        #region COM interop

        [Test]
        public void HiddenExcelAppIsClosedWhenDynamoIsClosed()
        {
            Assert.Inconclusive("No implemented");
        }

        [Test]
        public void DuplicateExcelAppsAreNotCreatedWhenMultipleFilesAreOpened()
        {
            Assert.Inconclusive("No implemented");
        }

        #endregion

        #region Reading

        [Test]
        public void CanReadWorksheetWithSingleColumnOfNumbers()
        {
       
            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_ascending.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            var filename = (StringFilename) Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            dynSettings.Controller.RunExpression(null);

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

            var filename = (StringFilename) Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the filename as Excel requires an absolute path
            filename.Value = filename.Value.Replace(@"..\..\..\test", GetTestDirectory());

            var watch = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            dynSettings.Controller.RunExpression(null);

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

            dynSettings.Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(4, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsList);
            var rowList = list[0].GetListFromFSchemeValue();
            Assert.AreEqual("a", rowList[0].getStringFromFSchemeValue());

            Assert.IsTrue(list[1].IsList);
            rowList = list[1].GetListFromFSchemeValue();
            Assert.IsNull(rowList[0]);

            Assert.IsTrue(list[2].IsList);
            rowList = list[2].GetListFromFSchemeValue();
            Assert.AreEqual("cell is", rowList[0].getStringFromFSchemeValue());

            Assert.IsTrue(list[3].IsList);
            rowList = list[3].GetListFromFSchemeValue();
            Assert.AreEqual("missing", rowList[0].getStringFromFSchemeValue());
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

            dynSettings.Controller.RunExpression(null);

            Assert.IsTrue(watch.OldValue.IsList);
            var list = watch.OldValue.GetListFromFSchemeValue();

            Assert.AreEqual(5, list.Count());

            // single column - 1, "word", 2, 3, "palabra"
            Assert.IsTrue(list[0].IsList);
            var rowList = list[0].GetListFromFSchemeValue();
            Assert.AreEqual(1, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[1].IsList);
            rowList = list[1].GetListFromFSchemeValue();
            Assert.AreEqual("word", rowList[0].getStringFromFSchemeValue());

            Assert.IsTrue(list[2].IsList);
            rowList = list[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[3].IsList);
            rowList = list[3].GetListFromFSchemeValue();
            Assert.AreEqual(3, rowList[0].GetDoubleFromFSchemeValue());

            Assert.IsTrue(list[4].IsList);
            rowList = list[4].GetListFromFSchemeValue();
            Assert.AreEqual("palabra", rowList[0].getStringFromFSchemeValue());
            
        }


        // other tests: ragged array
        #endregion

        #region Writing

        [Test]
        public void CanWriteDataOfMixedTypesToExcelWorksheet()
        {
            Assert.Inconclusive("No implemented");
        }

        [Test]
        public void CanWriteLongListToExcelWorksheet()
        {
            Assert.Inconclusive("No implemented");
        }

        [Test]
        public void CanCreateNewWorksheet()
        {
            Assert.Inconclusive("No implemented");
        }

        #endregion

    }
}
