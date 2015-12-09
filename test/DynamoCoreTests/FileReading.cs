using System.IO;
using System.Linq;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FileReadingTests : DynamoModelTestBase
    {
        string localDynamoStringTestFolder { get { return Path.Combine(TestDirectory, "core", "files"); } }
        string localDynamoFileTestFolder { get { return Path.Combine(TestDirectory, "core", "files", "future files"); } }

        [Test]
        public void CanOpenADynFileFromBefore6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFolder, "fileTests_pre6_0.dyn");

            RunModel(testFilePath); 
        }

        [Test]
        public void CanOpenADynFileFromAfter6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFolder, "fileTests_post6_0.dyn");

            RunModel(testFilePath); 
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_781()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-781

            string testFilePath = Path.Combine(localDynamoStringTestFolder, "Defect_MAGN_781.dyf");

            RunModel(testFilePath); 
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1380_dyn()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFolder, "future_file.dyn");
            

            OpenModel(testFilePath);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 0);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Connectors.Count(), 0);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1380_dyf()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFolder, "future_file.dyf");

            OpenModel(testFilePath);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 0);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Connectors.Count(), 0);
        }

        [Test]
        public void CanReadIsSetAsInputProperty()
        {
            string path = Path.Combine(TestDirectory, "core", "input_nodes", "NumberNodeAndNumberSlider.dyn");
            OpenModel(path);

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.IsFalse(CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(0).IsSetAsInput);
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(1).IsSetAsInput);
        }
    }
}
