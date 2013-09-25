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
        }

        [Test]
        public void CanReadWorksheetWithEmptyCellInUsedRange()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_missingCell.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanReadWorksheetWithMixedNumbersAndStrings()
        {

            string openPath = Path.Combine(GetTestDirectory(), @"core\excel\DataFromFile_mixedNumbersAndStrings.dyn");
            Controller.DynamoModel.Open(openPath);

            Assert.AreEqual(6, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            
        }

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
