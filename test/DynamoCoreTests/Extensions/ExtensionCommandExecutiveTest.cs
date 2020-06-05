using System.Collections.Generic;
using System.Linq;
using Dynamo.Extensions;
using Dynamo.Models;
using Dynamo.Scheduler;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests.Extensions
{
    [TestFixture]
    class ExtensionCommandExecutiveTest
    {
        int cmdExecutionState = -1;
        ICommandExecutive executive;
        Mock<IExtension> extMock;
        private Logging.NotificationMessage message;
        DynamoModel model;

        //This callback method will be used in the Mocked Extension
        private void ExtensionReadyCallback(ReadyParams ready)
        {
            executive = ready.CommandExecutive;
            Assert.IsTrue(ready.WorkspaceModels.Any());
            Assert.IsNotNull(ready.CurrentWorkspaceModel);
            ready.NotificationRecieved += (Logging.NotificationMessage obj) => message = obj;
        }

        [SetUp]
        public void Init()
        {
            extMock = new Mock<IExtension>();
            extMock.Setup(ext => ext.Ready(It.IsAny<ReadyParams>())).Callback((ReadyParams r) => ExtensionReadyCallback(r));
            cmdExecutionState = -1;

            model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    Extensions = new List<IExtension> { extMock.Object },
                    ProcessMode = TaskProcessMode.Synchronous,
                });
            model.DebugSettings.VerboseLogging = true;//Setting this flag to true will enable to execute a specific section in the ExecuteCommand() method
        }

        /// <summary>
        /// This test method will execute the ublic void ExecuteCommand() method from the ExtensionCommandExecutive class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ExtensionsCommandExecutiveExecuteCommand()
        {
            //Arrange
            var cmd = new Mock<DynamoModel.RecordableCommand>();
            model.CommandStarting += OnCommandStarting;
            model.CommandCompleted += OnCommandCompleted;

            //Assert
            //Check that the commands hasn't be executed
            Assert.AreEqual(-1, cmdExecutionState);

            //Act
            //Due that VerboseLogging = true then when executing the ExecuteCommand method will reach the Logger section 
            Assert.DoesNotThrow(() => executive.ExecuteCommand(cmd.Object, "TestRecordable", "ExtensionTests"));

            //Assert
            //Check that the command was already executed
            Assert.AreEqual(0, cmdExecutionState);

            //Unsubcribe from the command events
            model.CommandStarting -= OnCommandStarting;
            model.CommandCompleted -= OnCommandCompleted;
        }

        void OnCommandCompleted(DynamoModel.RecordableCommand command)
        {
            cmdExecutionState--;
        }

        void OnCommandStarting(DynamoModel.RecordableCommand command)
        {
            cmdExecutionState = 1;
        }

    }
}
