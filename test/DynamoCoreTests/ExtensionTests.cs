using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Scheduler;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class ExtensionTests
    {
        string extensionsPath;
        string testpkgPath;
        Mock<IExtension> extMock;
        DynamoModel model;
        ICommandExecutive executive;
        int cmdExecutionState = -1;
        private  Logging.NotificationMessage message;

        [SetUp]
        public void Init()
        {
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
            testpkgPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\test\pkgs"));
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

        [Test]
        public void RegisterService()
        {
            var sm = model.ExtensionManager;
            var id = sm.RegisterService(executive);
            Assert.IsNotNull(Guid.Parse(id));

            Assert.Throws<ArgumentNullException>(() => sm.RegisterService<ICommandExecutive>(null));

            //Re-registering a service for the same type fails
            Assert.IsNull(sm.RegisterService(new Mock<ICommandExecutive>().Object));

            var service = sm.Service<ICommandExecutive>();
            Assert.AreSame(executive, service);

            var status = sm.UnregisterService<ICommandExecutive>(id);
            Assert.IsTrue(status);

            var obj = sm.GetService(typeof(ICommandExecutive));
            Assert.IsNull(obj); //Service is already unregistered
        }

        [Test]
        public void RegisterAsBaseClassService()
        {
            var sm = model.ExtensionManager;
            var id = sm.RegisterService<ILogSource>(executive);
            Assert.IsNotNull(System.Guid.Parse(id));

            //Even though we registered ICommandExecutive as ILogSource service,
            //we can't get ICommandExecutive service from the service manager. The
            //service was registered as ILogSource service only.
            var service = sm.Service<ICommandExecutive>();
            Assert.IsNull(service);

            var logsource = sm.Service<ILogSource>();
            Assert.AreSame(executive, logsource);

            var status = sm.UnregisterService<ICommandExecutive>(id);
            Assert.IsFalse(status); //Couldn't find service of type ICommandExecutive

            var obj = sm.GetService(typeof(ILogSource));
            Assert.IsNotNull(obj); //service is still there

            status = sm.UnregisterService<ILogSource>(id);
            Assert.IsTrue(status);
        }

        [Test]
        public void RegisterSameObjectAsManyServices()
        {
            var sm = model.ExtensionManager;
            var ls = sm.RegisterService<ILogSource>(executive);
            Assert.IsNotNull(System.Guid.Parse(ls));

            var ce = sm.RegisterService<ICommandExecutive>(executive);
            Assert.IsNotNull(System.Guid.Parse(ce));

            Assert.AreNotEqual(ls, ce);

            //Get ICommandExecutive service
            var service = sm.Service<ICommandExecutive>();
            Assert.AreSame(executive, service);

            //Get ILogSource service
            var logsource = sm.Service<ILogSource>();
            Assert.AreSame(executive, logsource);

            var status = sm.UnregisterService<ICommandExecutive>(ls);
            Assert.IsFalse(status); //The service key didn't match

            var obj = sm.GetService(typeof(ICommandExecutive));
            Assert.IsNotNull(obj); //service is still there

            status = sm.UnregisterService<ICommandExecutive>(ce);
            Assert.IsTrue(status); //The service key as type matched

            obj = sm.GetService(typeof(ICommandExecutive));
            Assert.IsNull(obj); //service is still there

            status = sm.UnregisterService<ILogSource>(ls);
            Assert.IsTrue(status);

            logsource = sm.Service<ILogSource>();
            Assert.IsNull(logsource); //service is unregistered and not available any more
        }

        [Test]
        public void ExtensionLoader_LoadNodeLibraryAddsZTNodesToSearch()
        {
            var assemPath = Path.Combine(testpkgPath, "Dynamo Samples", "bin", "SampleLibraryZeroTouch.dll");
            var assembly = Assembly.LoadFrom(assemPath);
            var libraryLoader = new ExtensionLibraryLoader(model);
            libraryLoader.LoadNodeLibrary(assembly);

            var entries = model.SearchModel.SearchEntries.ToList();
            var nodesInLib = entries.Where(x => x.Assembly.Contains("SampleLibraryZeroTouch")).Select(y => y.FullName).ToList();
            Assert.AreEqual(12, nodesInLib.Count());
            Assert.IsTrue(entries.Count(x => x.FullName == "SampleLibraryZeroTouch.Examples.TransformableExample.TransformObject") == 1);

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
