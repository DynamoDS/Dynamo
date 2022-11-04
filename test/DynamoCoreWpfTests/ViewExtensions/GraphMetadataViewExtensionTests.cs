using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphMetadata;
using Dynamo.GraphMetadata.Controls;
using NUnit.Framework;

namespace DynamoCoreWpfTests.ViewExtensions
{
    public class GraphMetadataViewExtensionTests : DynamoTestUIBase
    {
        [Test]
        public void SettingPropertiesInExtensionUpdatesWorkspace()
        {
            // Arrange
            var graphDescription = "Test description..";
            var graphAuthor = "Test author..";
            var graphHelpLink = new Uri("https://dynamobim.org/");
            var thumbnail = new BitmapImage(
                new Uri(Path.Combine(
                    DynamoTestUIBase.GetTestDirectory(this.ExecutingDirectory),
                    @"UI/thumbnail_test.jpg")));

            var extensionManager = View.viewExtensionManager;

            var propertiesExt = extensionManager.ViewExtensions
                .FirstOrDefault(x => x as GraphMetadataViewExtension != null )
                as GraphMetadataViewExtension;

            var hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;

            // Act
            var graphDescriptionBefore = hwm.Description; 
            var graphAuthorBefore = hwm.Author; 
            var graphHelpLinkBefore = hwm.GraphDocumentationURL; 
            var graphThumbnailBefore = hwm.Thumbnail;

            propertiesExt.viewModel.GraphDescription = graphDescription;
            propertiesExt.viewModel.GraphAuthor = graphAuthor;
            propertiesExt.viewModel.HelpLink = graphHelpLink;
            propertiesExt.viewModel.Thumbnail = thumbnail;

            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            var thumbnailB64 = Convert.ToBase64String(data);

            // Assert
            Assert.IsFalse(graphDescriptionBefore == hwm.Description);
            Assert.IsFalse(graphAuthorBefore == hwm.Author);
            Assert.IsFalse(graphHelpLinkBefore == hwm.GraphDocumentationURL);
            Assert.IsFalse(graphThumbnailBefore == hwm.Thumbnail);

            Assert.IsTrue(hwm.Description == graphDescription);
            Assert.IsTrue(hwm.Author == graphAuthor);
            Assert.IsTrue(hwm.GraphDocumentationURL == graphHelpLink);
            Assert.IsTrue(hwm.Thumbnail == thumbnailB64);
        }

        [Test]
        public void CustomPropertiesGetsAddedToExtensionDataOnSave()
        {
            // Arrange
            var propName = "TestPropertyName";
            var propValue = "TestPropertyValue";
            var extensionManager = View.viewExtensionManager;

            var propertiesExt = extensionManager.ViewExtensions
                .FirstOrDefault(x => x as GraphMetadataViewExtension != null)
                as GraphMetadataViewExtension;

            // Act
            var hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;
            var propertiesExtExtenisonDataBefore = hwm.ExtensionData
                .Where(x => x.ExtensionGuid == propertiesExt.UniqueId)
                .FirstOrDefault();

            propertiesExt.viewModel.AddCustomProperty("TestPropertyName", "TestPropertyValue");
            this.ViewModel.Model.OnWorkspaceSaving(hwm, Dynamo.Graph.SaveContext.Save);

            var propertiesExtExtenisonDataAfterSave = hwm.ExtensionData
                .Where(x => x.ExtensionGuid == propertiesExt.UniqueId)
                .FirstOrDefault();

            // Assert
            Assert.That(propertiesExtExtenisonDataBefore is null);
            Assert.That(propertiesExtExtenisonDataAfterSave.Data.ToList().Count == 1);
            Assert.That(propertiesExtExtenisonDataAfterSave.Data.TryGetValue(propName, out string kvp));
            Assert.That(kvp == propValue);
        }


        [Test]
        public void ExistingGraphWithCustomPropertiesWillBeAddedToExtension()
        {
            // Arrange
            var expectedCP1Key = "My prop 1";
            var expectedCP2Key = "My prop 2";
            var expectedCP3Key = "Custom Property 3";

            var expectedCP1Value = "My value 1";
            var expectedCP2Value = "My Value 2";
            var expectedCP3Value = "";

            // Act
            var extensionManager = View.viewExtensionManager;
            var propertiesExt = extensionManager.ViewExtensions
                .FirstOrDefault(x => x as GraphMetadataViewExtension != null)
                as GraphMetadataViewExtension;

            var customPropertiesBeforeOpen = propertiesExt.viewModel.CustomProperties.Count;
            Open(@"core\CustompropertyTest.dyn");

            // Assert
            Assert.IsFalse(ViewModel.HomeSpace.HasUnsavedChanges);
            Assert.IsTrue(customPropertiesBeforeOpen == 0);
            Assert.That(propertiesExt.viewModel.CustomProperties.Count == 3);
            Assert.That(propertiesExt.viewModel.CustomProperties[0].PropertyName == expectedCP1Key);
            Assert.That(propertiesExt.viewModel.CustomProperties[0].PropertyValue == expectedCP1Value);
            Assert.That(propertiesExt.viewModel.CustomProperties[1].PropertyName == expectedCP2Key);
            Assert.That(propertiesExt.viewModel.CustomProperties[1].PropertyValue == expectedCP2Value);
            Assert.That(propertiesExt.viewModel.CustomProperties[2].PropertyName == expectedCP3Key);
            Assert.That(propertiesExt.viewModel.CustomProperties[2].PropertyValue == expectedCP3Value);
        }

        [Test]
        public void ExistingGraphOpenModifiedAndClosedWillSetAndClearModifiedFlag()
        {
            var extensionManager = View.viewExtensionManager;
            var propertiesExt = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphMetadataViewExtension != null)
                as GraphMetadataViewExtension;

            var customPropertiesBeforeOpen = propertiesExt.viewModel.CustomProperties.Count;
            Open(@"core\CustompropertyTest.dyn");

            Assert.IsFalse(ViewModel.HomeSpace.HasUnsavedChanges);

            propertiesExt.viewModel.AddCustomProperty("TestPropertyName-X", "TestPropertyValue-X");

            Assert.IsTrue(ViewModel.HomeSpace.HasUnsavedChanges);

            ViewModel.HomeSpace.HasUnsavedChanges = false;
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);
            ViewModel.MakeNewHomeWorkspace(null);

            Assert.IsFalse(ViewModel.HomeSpace.HasUnsavedChanges);
        }
    }
}
