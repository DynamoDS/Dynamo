using System.IO;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FileReadingTests : DynamoUnitTest
    {
        string localDynamoStringTestFloder { get { return Path.Combine(GetTestDirectory(), "core", "files"); } }
        string localDynamoFileTestFloder { get { return Path.Combine(GetTestDirectory(), "core", "files", "future files"); } }

        [Test]
        public void CanOpenADynFileFromBefore6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "fileTests_pre6_0.dyn");

            Controller.DynamoViewModel.OpenCommand.Execute(testFilePath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null)); 
        }

        [Test]
        public void CanOpenADynFileFromAfter6_0()
        {
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "fileTests_post6_0.dyn");

            Controller.DynamoViewModel.OpenCommand.Execute(testFilePath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));
        }

        [Test]
        public void Defect_MAGN_781()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-781

            string testFilePath = Path.Combine(localDynamoStringTestFloder, "Defect_MAGN_781.dyf");

            Controller.DynamoViewModel.OpenCommand.Execute(testFilePath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));
        }

        [Test]
        public void Defect_MAGN_1380_dyn()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFloder, "future_file.dyn");
            

            Controller.DynamoViewModel.OpenCommand.Execute(testFilePath);
            WorkspaceModel wsm = Controller.DynamoViewModel.CurrentSpace;
            Assert.AreEqual(wsm.Nodes.Count, 0);
            Assert.AreEqual(wsm.Connectors.Count, 0);
        }

        [Test]
        public void Defect_MAGN_1380_dyf()
        {
            // Details steps are here: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1380

            string testFilePath = Path.Combine(localDynamoFileTestFloder, "future_file.dyf");

            Controller.DynamoViewModel.OpenCommand.Execute(testFilePath);
            WorkspaceModel wsm = Controller.DynamoViewModel.CurrentSpace;
            Assert.AreEqual(wsm.Nodes.Count, 0);
            Assert.AreEqual(wsm.Connectors.Count, 0);
        }
    }
}
