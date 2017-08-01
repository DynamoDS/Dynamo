using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Tests
{
    [Category("MessageLog")]
    class MessageLogTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestWarningMessageLog()
        {
            string openPath = Path.Combine(TestDirectory, @"core\messagelog\testwarningmessage.dyn");
            RunModel(openPath);

            ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.Default, warningEntry.ID);
        }
        
    }
}
