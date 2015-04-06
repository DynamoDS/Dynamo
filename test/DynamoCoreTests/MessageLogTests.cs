using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Tests
{
    [Category("MessageLog")]
    class MessageLogTests : DynamoViewModelUnitTest
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
            ViewModel.OpenCommand.Execute(openPath);
            ViewModel.HomeSpace.Run();

            ProtoCore.RuntimeCore runtimeCore = ViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.kDefault, warningEntry.ID);
            
        }
    }
}
