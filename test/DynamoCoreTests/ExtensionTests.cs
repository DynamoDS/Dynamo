using System.Collections.Generic;
using System.Linq;
using Dynamo.Scheduler;
using NUnit.Framework;
using Dynamo.Extensions;
using System.IO;
using Dynamo.Models;
using Moq;

namespace Dynamo.Tests
{
    class ExtensionTests
    {
        string extensionsPath;
        Mock<IExtension> extMock;
        DynamoModel model;
        ICommandExecutive executive;
        int cmdExecutionState = -1;
        private  Logging.NotificationMessage message;

        [SetUp]
        public void Init()
        {
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
            extMock = new Mock<IExtension>();
            extMock.Setup(ext => ext.Ready(It.IsAny<ReadyParams>())).Callback((ReadyParams r) => ExtensionReadyCallback(r));
            cmdExecutionState = -1;

            model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    Extensions = new List<IExtension> { extMock.Object },
                    ProcessMode = TaskProcessMode.Synchronous
                });
            model.ExtensionManager.ExtensionAdded += OnExtensionAdded;
            model.ExtensionManager.ExtensionRemoved += OnExtensionRemoved;
        }

        void OnExtensionRemoved(IExtension obj)
        {
            Assert.AreEqual(extMock.Object, obj);
        }

        void OnExtensionAdded(IExtension obj)
        {
            Assert.AreEqual(extMock.Object, obj);
        }

        [Test]
        public void DirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(extensionsPath));
        }

        [Test]
        public void ExtensionIsStarted()
        {
            extMock.Verify(ext => ext.Startup(It.IsAny<StartupParams>()));
        }

        [Test]
        public void ExtensionIsAdded()
        {
            Assert.IsTrue(model.ExtensionManager.Extensions.Contains(extMock.Object));
        }

        [Test]
        public void CannotAddSameExtensionTwice()
        {
            Assert.AreEqual(1, model.ExtensionManager.Extensions.Count());
            model.ExtensionManager.Add(extMock.Object);
            Assert.AreEqual(1, model.ExtensionManager.Extensions.Count());
        }

        [Test]
        public void ExtensionIsReady()
        {
            extMock.Verify(ext => ext.Ready(It.IsAny<ReadyParams>()));
        }

        private void ExtensionReadyCallback(ReadyParams ready)
        {
            executive = ready.CommandExecutive;
            Assert.IsTrue(ready.WorkspaceModels.Any());
            Assert.IsNotNull(ready.CurrentWorkspaceModel);
            ready.NotificationRecieved += (Logging.NotificationMessage obj) => message = obj;
        }

        [Test]
        public void CanExecuteCommand()
        {
            Assert.IsNotNull(executive);
            model.CommandStarting += OnCommandStarting;
            model.CommandCompleted += OnCommandCompleted;
            
            var cmd = new Mock<DynamoModel.RecordableCommand>();
            Assert.AreEqual(-1, cmdExecutionState);
            Assert.DoesNotThrow(() => executive.ExecuteCommand(cmd.Object, "TestRecordable", "ExtensionTests"));
            Assert.AreEqual(0, cmdExecutionState);
            
            model.CommandStarting -= OnCommandStarting;
            model.CommandCompleted -= OnCommandCompleted;
        }

        [Test]
        public void ExecuteCommandDoesnotThrowException()
        {
            Assert.IsNotNull(executive);
            var cmd = new ThrowExceptionCommand();
            Assert.Throws<System.NotImplementedException>(() => cmd.Execute(model));
            Assert.DoesNotThrow(() => executive.ExecuteCommand(cmd, "TestRecordable", "ExtensionTests"));
            Assert.AreEqual(Logging.WarningLevel.Error, model.Logger.WarningLevel);
        }

        void OnCommandCompleted(DynamoModel.RecordableCommand command)
        {
            cmdExecutionState--;
        }

        void OnCommandStarting(DynamoModel.RecordableCommand command)
        {
            cmdExecutionState = 1;
        }

        [Test]
        public void NotificationHandler()
        {
            var sender = "ExecutionTests";
            var title = "NotificationTitle";
            var shortMessage = "Short Message";
            var detailedMessage = "Detailed Notification message";
            Assert.IsNull(message);
            model.Logger.LogNotification(sender, title, shortMessage, detailedMessage);
            Assert.IsNotNull(message);
            Assert.AreEqual(sender, message.Sender);
            Assert.AreEqual(title, message.Title);
            Assert.AreEqual(shortMessage, message.ShortMessage);
            Assert.AreEqual(detailedMessage, message.DetailedMessage);
        }

        [Test]
        public void ExtensionIsDisposed()
        {
            model.Dispose();
            extMock.Verify(ext => ext.Dispose());
        }
    }

    class ThrowExceptionCommand : DynamoModel.RecordableCommand
    {
        protected override void ExecuteCore(DynamoModel dynamoModel)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeCore(System.Xml.XmlElement element)
        {
        }
    }

}
