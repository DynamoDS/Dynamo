using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using ImportExport = DSCore.ImportExport;
using Dynamo;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class ImportExportTests : UnitTestBase
    {
        [Test]
        [Category("UnitTests")]
        public static void ImportCSV_PathTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\not a valid file.csv");
            Assert.Throws<FileNotFoundException>(() => ImportExport.ImportFromCSV(filePath));
        }

        [Test]
        [Category("UnitTests")]
        public static void ImportCSVTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\test1.csv");
            var CSVList = ImportExport.ImportFromCSV(filePath);
            Assert.AreEqual(CSVList, new List<object> {
                new List<object> { 2, 3, 4 },
                new List<object> { 4, 6, 8 },
                new List<object> { 6, 9, 12 },
                new List<object> { 8, 12, 16 }
            });
        }
        
        [Test]
        [Category("UnitTests")]
        public static void ImportCSVWithTransposeTest()
        {
            string filePath = Path.Combine(TestDirectory, @"core\importExport\test2.csv");
            var CSVList = ImportExport.ImportFromCSV(filePath, true);
            Assert.AreEqual(CSVList, new List<object> {
                new List<object> { 1.2, 5.6, 7, 155 },
                new List<object> { 2, 0.009, 10, 3.3 },
                new List<object> { null, 3, 3, null }
            });
        }
    }
}
