using System.Collections.ObjectModel;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using NUnit.Framework;
using System.IO;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using Dynamo.Models;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Tests
{
    [Category("MessageLog")]
    class MessageLogTests : DynamoViewModelUnitTest
    {
        [Test]
        public void TestWarningMessageLog()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\messagelog\testwarningmessage.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            ViewModel.HomeSpace.Run();

            ProtoCore.RuntimeCore runtimeCore = ViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.kDefault, warningEntry.ID);
            
        }
    }
}
