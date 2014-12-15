using System.IO;
using System.Linq;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FileReadingTests : DynamoViewModelUnitTest
    {
        string localDynamoStringTestFloder { get { return Path.Combine(GetTestDirectory(), "core", "files"); } }
        string localDynamoFileTestFloder { get { return Path.Combine(GetTestDirectory(), "core", "files", "future files"); } }

        [Test]
        public void CanOpenADynFileFromBefore6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "fileTests_pre6_0.dyn");

            ViewModel.OpenCommand.Execute(testFilePath);
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run()); 
        }

        [Test]
        public void CanOpenADynFileFromAfter6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "fileTests_post6_0.dyn");

            ViewModel.OpenCommand.Execute(testFilePath);
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_781()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-781

            string testFilePath = Path.Combine(localDynamoStringTestFloder, "Defect_MAGN_781.dyf");

            ViewModel.OpenCommand.Execute(testFilePath);
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1380_dyn()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFloder, "future_file.dyn");
            

            ViewModel.OpenCommand.Execute(testFilePath);
            WorkspaceModel wsm = ViewModel.CurrentSpace;
            Assert.AreEqual(wsm.Nodes.Count, 0);
            Assert.AreEqual(wsm.Connectors.Count(), 0);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1380_dyf()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFloder, "future_file.dyf");

            ViewModel.OpenCommand.Execute(testFilePath);
            WorkspaceModel wsm = ViewModel.CurrentSpace;
            Assert.AreEqual(wsm.Nodes.Count, 0);
            Assert.AreEqual(wsm.Connectors.Count(), 0);
        }
    }
}
