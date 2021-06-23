using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class StorageExtensionTests
    {
        private const string MOCK_EXTENSION_GUID = "7de691ac-b1ea-4353-9b9b-1f57b2967895";
        private const string MOCK_EXTENSION_NAME = "Mock Extension";

        private Mock<IExtensionStorageAccess> storageExt;
        private Mock<IExtension> extensionMock;
        private DynamoModel model;
        private ExtensionManager extensionManager;

        [SetUp]
        public void Init()
        {
            storageExt = new Mock<IExtensionStorageAccess>();
            extensionMock = storageExt.As<IExtension>();

            storageExt.SetupGet(x => x.UniqueId).Returns(MOCK_EXTENSION_GUID);
            storageExt.SetupGet(x => x.Name).Returns(MOCK_EXTENSION_NAME);

            extensionMock.SetupGet(x => x.UniqueId).Returns(MOCK_EXTENSION_GUID);
            extensionMock.SetupGet(x => x.Name).Returns(MOCK_EXTENSION_NAME);

            model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    Extensions = new List<IExtension> { extensionMock.Object },
                    ProcessMode = TaskProcessMode.Synchronous
                });

            extensionManager = model.ExtensionManager as ExtensionManager;

            // Make sure events that the storageExt mock uses are not null.
            model.WorkspaceSaving += delegate { };
            model.WorkspaceOpened += delegate { };

        }

        [Test]
        public void StorageExtensionIsAdded()
        {
            // Assert
            Assert.IsTrue(extensionManager.StorageAccessExtensions.Contains(storageExt.Object));
        }


        [Test]
        public void StorageExtensionIsRemoveWhenExtensionThatImplementsItIsRemoved()
        {
            // Arrange
            var otherExtensionGuid = "afd79e28-40b6-4114-9b95-5193264dc566";
            var otherExtensionName = "OtherExtension";

            var otherStorageExt = new Mock<IExtensionStorageAccess>();
            var otherExtensionMock = otherStorageExt.As<IExtension>();

            otherStorageExt.SetupGet(x => x.UniqueId).Returns(otherExtensionGuid);
            otherStorageExt.SetupGet(x => x.Name).Returns(otherExtensionName);

            otherExtensionMock.SetupGet(x => x.UniqueId).Returns(otherExtensionGuid);
            otherExtensionMock.SetupGet(x => x.Name).Returns(otherExtensionName);

            extensionManager.Add(otherExtensionMock.Object);

            var storageExtensionsBeforeRemove = extensionManager.StorageAccessExtensions.Count();

            // Act
            extensionManager.Remove(extensionMock.Object);

            // Assert
            Assert.IsTrue(extensionManager.StorageAccessExtensions.Count() != storageExtensionsBeforeRemove);
            Assert.IsTrue(extensionManager.StorageAccessExtensions.Contains(otherStorageExt.Object));
            Assert.IsFalse(extensionManager.StorageAccessExtensions.Contains(storageExt.Object));
        }

        [Test]
        public void CannotAddSameStorageExtensionTwice()
        {
            // Arrange
            var storageExtensionBeforeAdd = extensionManager.StorageAccessExtensions.Count();

            // Act
            model.ExtensionManager.Add(extensionMock.Object);

            // Assert
            Assert.AreEqual(1, storageExtensionBeforeAdd);      
            Assert.AreEqual(1, extensionManager.StorageAccessExtensions.Count());
        }

        [Test]
        public void OnWorkspaceSavingIsCalledWhenWorkspaceIsSaving()
        {
            // Act
            model.OnWorkspaceSaving(model.CurrentWorkspace, Graph.SaveContext.Save);

            // Assert
            storageExt.Verify(x => x.OnWorkspaceSaving(It.IsAny<Dictionary<string, string>>(), It.IsAny<Graph.SaveContext>()), Times.Once());
        }

        [Test]
        public void OnWorkspaceOpenIsCalledWhenWorkspaceIsOpened()
        {
            // Act
            model.OnWorkspaceOpened(model.CurrentWorkspace);

            // Assert
            storageExt.Verify(x => x.OnWorkspaceOpen(It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Test]
        public void OnWorkspaceOpenDoesNotAddToWorkspaceExtensionData()
        {
            // Arrange
            var homeworkspace = model.CurrentWorkspace as HomeWorkspaceModel;
            var extensionDataBeforeWorkspaceOpen = homeworkspace.ExtensionData.Count;

            var dataDictionary = new Dictionary<string, string>
            {
                {"SomeNewKey", "SomeNewValue" }
            };

            SetupOnWorkspaceOpen(dataDictionary);

            // Act
            model.OnWorkspaceOpened(model.CurrentWorkspace);
            var extensionDataAfterWorkspaceOpen = homeworkspace.ExtensionData.Count;

            // Assert
            Assert.AreEqual(extensionDataBeforeWorkspaceOpen, extensionDataAfterWorkspaceOpen);
        }

        [Test]
        public void OnWorkspaceOpenDoesNotModifyExistingExtensionDataDictionary()
        {
            // Arrange
            var homeworkspace = model.CurrentWorkspace as HomeWorkspaceModel;
            var initialDict = new Dictionary<string, string>
            {
                {"A","a"},
                {"B","b"},
            };

            var onOpenedDict = new Dictionary<string, string>
            {
                {"NewKey","NewValue"},
            };

            SetupOnWorkspaceSaving(initialDict);
            SetupOnWorkspaceOpen(onOpenedDict);

            // Act
            model.OnWorkspaceSaving(model.CurrentWorkspace, Graph.SaveContext.Save);
            var extensionDataDictionaryBeforeOpen = homeworkspace.ExtensionData
                .Where(x=>x.ExtensionGuid == MOCK_EXTENSION_GUID).FirstOrDefault();

            model.OnWorkspaceOpened(model.CurrentWorkspace);
            var extensionDataDictionaryAfterOpen = homeworkspace.ExtensionData
                .Where(x => x.ExtensionGuid == MOCK_EXTENSION_GUID).FirstOrDefault();

            // Assert
            CollectionAssert.AreEquivalent(initialDict, extensionDataDictionaryBeforeOpen.Data);
            CollectionAssert.AreEquivalent(extensionDataDictionaryBeforeOpen.Data, extensionDataDictionaryAfterOpen.Data);
        }


        [Test]
        public void OnWorkspaceSavingAddsToStoredExtensionData()
        {
            // Arrange
            var homeworkspace = model.CurrentWorkspace as HomeWorkspaceModel;
            var initialDataDictionary = homeworkspace.ExtensionData
                .Where(x => x.ExtensionGuid == MOCK_EXTENSION_GUID)
                .FirstOrDefault();


            var dictionaryToAddOnSaving = new Dictionary<string, string>
            {
                {"A","a" },
                {"B","b" }
            };

            SetupOnWorkspaceSaving(dictionaryToAddOnSaving);

            // Act
            model.OnWorkspaceSaving(model.CurrentWorkspace, Graph.SaveContext.Save);
            var extensionDataDictionaryAfterSave = homeworkspace.ExtensionData
                .Where(x => x.ExtensionGuid == MOCK_EXTENSION_GUID).FirstOrDefault();

            // Assert
            Assert.IsNull(initialDataDictionary);
            Assert.IsNotNull(extensionDataDictionaryAfterSave);
            CollectionAssert.AreEquivalent(dictionaryToAddOnSaving, extensionDataDictionaryAfterSave.Data);
        }


        [Test]
        public void OnWorkspaceSavingModifiesExistingExtensionDataDictionary()
        {
            // Arrange
            var homeworkspace = model.CurrentWorkspace as HomeWorkspaceModel;
            var initialDataDictionary = new Dictionary<string, string>
            {
                {"A","a" },
                {"B","b" }
            };
            var extensionData = new ExtensionData(MOCK_EXTENSION_GUID, MOCK_EXTENSION_NAME, "0.0", initialDataDictionary);
            homeworkspace.ExtensionData.Add(extensionData);

            var dictionaryToAddOnSaving = new Dictionary<string, string>
            {
                {"C","c" },
                {"D","d" }
            };

            var expectedDictionaryAfterOnSaving = new Dictionary<string, string>(initialDataDictionary);
            dictionaryToAddOnSaving
                .ToList()
                .ForEach(x => expectedDictionaryAfterOnSaving.Add(x.Key, x.Value));

            SetupOnWorkspaceSaving(dictionaryToAddOnSaving);

            // Act
            model.OnWorkspaceSaving(model.CurrentWorkspace, Graph.SaveContext.Save);
            var extensionDataAfterSave = homeworkspace.ExtensionData
                .Where(x => x.ExtensionGuid == MOCK_EXTENSION_GUID).FirstOrDefault();

            // Assert
            Assert.AreEqual(extensionData, extensionDataAfterSave);
            CollectionAssert.AreEquivalent(extensionData.Data, extensionDataAfterSave.Data);
            CollectionAssert.AreEquivalent(expectedDictionaryAfterOnSaving, extensionDataAfterSave.Data);
        }

        private void SetupOnWorkspaceSaving(Dictionary<string, string> dataDict)
        {
            storageExt.Setup(x => x.OnWorkspaceSaving(It.IsAny<Dictionary<string, string>>(), It.IsAny<Graph.SaveContext>()))
                .Callback((Dictionary<string, string> data, Graph.SaveContext saveContext) =>
                {
                    dataDict.ToList().ForEach(x => data.Add(x.Key, x.Value));
                });

        }

        private void SetupOnWorkspaceOpen(Dictionary<string, string> dataDict)
        {

            storageExt.Setup(x => x.OnWorkspaceOpen(It.IsAny<Dictionary<string, string>>()))
                .Callback((Dictionary<string, string> data) =>
                {
                    dataDict
                    .ToList()
                    .ForEach(x => data.Add(x.Key, x.Value));
                });
        }

    }
}
