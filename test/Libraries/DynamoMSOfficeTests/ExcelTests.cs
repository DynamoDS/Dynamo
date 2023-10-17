using DSOffice;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace Dynamo.Tests
{

    [TestFixture, Apartment(ApartmentState.MTA)]
    public class ExcelTests
    {
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
