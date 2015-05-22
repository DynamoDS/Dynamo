using System.IO;
using System.Linq;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FileReadingTests : DynamoModelTestBase
    {
        string localDynamoStringTestFloder { get { return Path.Combine(TestDirectory, "core", "files"); } }
        string localDynamoFileTestFloder { get { return Path.Combine(TestDirectory, "core", "files", "future files"); } }

        [Test]
        public void CanOpenADynFileFromBefore6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "fileTests_pre6_0.dyn");

            RunModel(testFilePath); 
        }

        [Test]
        public void CanOpenADynFileFromAfter6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "fileTests_post6_0.dyn");

            RunModel(testFilePath); 
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_781()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-781

            string testFilePath = Path.Combine(localDynamoStringTestFloder, "Defect_MAGN_781.dyf");

            RunModel(testFilePath); 
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1380_dyn()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFloder, "future_file.dyn");
            

            OpenModel(testFilePath);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count, 0);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Connectors.Count(), 0);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1380_dyf()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFloder, "future_file.dyf");

            OpenModel(testFilePath);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count, 0);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Connectors.Count(), 0);
        }
    }
}
