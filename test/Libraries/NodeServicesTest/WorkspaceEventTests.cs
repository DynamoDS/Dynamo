using System.IO;
using System.Linq;
using System.Reflection;

using SystemTestServices;

using DynamoServices;

using NUnit.Framework;

namespace DynamoServicesTests
{
    [TestFixture, RequiresSTA]
    class WorkspaceEventTests : SystemTestBase
    {
        protected override void SetupCore()
        {
            // Set the working path
            var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            workingDirectory = Path.GetFullPath(
                Path.Combine(asmDir, @"..\..\..\test\core\"));
        }

        [Test]
        public void WorkspaceAddedEventIsTriggeredWhenWorkspaceIsAdded()
        {
            // Register for the added event
            WorkspaceEvents.WorkspaceAdded += WorkspacesModified;

            // Open a definition which should trigger
            // the added event.
            OpenDynamoDefinition(@".\math\Add.dyn");
        }

        [Test]
        public void WorkspaceRemovedEventIsTriggeredWhenWorkspaceIsRemoved()
        {
            // Open a definition
            OpenDynamoDefinition(@".\math\Add.dyn");

            var ws = Model.Workspaces.FirstOrDefault();
            Assert.NotNull(ws);

            // Register for the removed event.
            WorkspaceEvents.WorkspaceRemoved += (args) => Assert.AreEqual(args.Id, ws.Guid);

            // Open a new definition which should trigger both the 
            // workspace removed and the workspace added events.
            OpenDynamoDefinition(@".\math\Subtract.dyn");
        }

        [TearDown]
        public void Teardown()
        {
            // Afer each test, we need to unhook the event handlers
            // to ensure we're in a clean state for the next run. 
            WorkspaceEvents.WorkspaceAdded -= WorkspacesModified;
            WorkspaceEvents.WorkspaceRemoved -= WorkspacesModified;
        }

        private void WorkspacesModified(WorkspacesModificationEventArgs args)
        {
            Assert.AreEqual(args.Name, "Add");
        }
    }
}
