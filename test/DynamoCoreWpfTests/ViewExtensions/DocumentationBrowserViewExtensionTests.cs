using Dynamo.Configuration;
using Dynamo.DocumentationBrowser;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace DynamoCoreWpfTests
{
    public class DocumentationBrowserViewExtensionTests : DynamoTestUIBase
    {
        private const string docsTabName = "Documentation Browser";
        private const string externalLink = "http://dictionary.dynamobim.org";
        private const string localDocsFileLink = "ExcelNotInstalled.html";
        private const string indexPageHtmlHeader = "<h2>Dynamo Documentation Browser</h2>";
        private const string excelDocsFileHtmlHeader = "<h2>Excel not installed </h2>";

        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = this.preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings() { CustomPackageFolders = new List<string>() { this.PackagesDirectory } }
            };
        }

        [Test]
        public void DocsExtensionAddsMenuItem()
        {
            // Arrange
            List<MenuItem> showDocsMenuItems = GetDocsMenuItems();

            // Assert
            Assert.GreaterOrEqual(showDocsMenuItems.Count, 1);
        }

        [Test]
        public void ClickingMenuItemLaunchesSidebarWithIndexContent()
        {
            // Act
            // simulate clicking the Show docs browser menu item
            ShowDocsBrowser();

            // confirm the extension loads a view into the sidebar
            // and get the html content inside
            var docsTab = GetDocsTabItem();
            var docsBrowserContent = GetSidebarDocsBrowserContents();

            // Assert
            Console.WriteLine(docsBrowserContent);
            Assert.AreEqual(docsTabName, (string)docsTab.Header);
            Assert.IsTrue(docsBrowserContent.Contains(indexPageHtmlHeader));
        }

        [Test]
        public void ShowingStartPageHidesBrowser()
        {
            // Arrange
            ShowDocsBrowser();
            var docsView = GetDocsTabItem().Content as DocumentationBrowserView;
            var visibilityBeforeShowStartPageEvent = docsView.documentationBrowser.Visibility;

            // Act
            ViewModel.DisplayStartPageCommand.Execute(null);
            var visibilityAfterShowStartPageEvent = docsView.documentationBrowser.Visibility;

            // Assert
            Assert.AreEqual(Visibility.Visible, visibilityBeforeShowStartPageEvent);
            Assert.AreEqual(Visibility.Hidden, visibilityAfterShowStartPageEvent);
        }

        [Test]
        public void ViewExtensionIgnoresExternalEventsByDefault()
        {
            // Arrange
            var externalEvent = new OpenDocumentationLinkEventArgs(new Uri(externalLink));
            var viewExtension = SetupNewViewExtension();

            // Act
            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            viewExtension.HandleRequestOpenDocumentationLink(externalEvent);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;

            // Assert
            Assert.IsTrue(externalEvent.IsRemoteResource);
            Assert.IsFalse(viewExtension.AllowRemoteResources);
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(0, tabsAfterExternalEventTrigger);
        }

        [Test]
        public void RemoteResourceEventDisplaysWarningBanner()
        {
            // Arrange
            var externalEvent = new OpenDocumentationLinkEventArgs(new Uri(externalLink, UriKind.RelativeOrAbsolute));
            var localEvent = new OpenDocumentationLinkEventArgs(new Uri(localDocsFileLink, UriKind.RelativeOrAbsolute));

            // create a view extension that allows external events
            var viewExtension = SetupNewViewExtension(true);
            viewExtension.AllowRemoteResources = true;
            
            // trigger local event first so the sidebar is opened
            viewExtension.HandleRequestOpenDocumentationLink(localEvent);
            Assert.IsFalse(viewExtension.ViewModel.IsRemoteResource);
            var docsView = GetDocsTabItem().Content as DocumentationBrowserView;

            // Act
            var visibilityBeforeEvent = docsView.RemoteLinkBanner.Visibility;
            viewExtension.HandleRequestOpenDocumentationLink(externalEvent);
            var visibilityAfterEvent = docsView.RemoteLinkBanner.Visibility;

            // Assert
            Assert.IsTrue(externalEvent.IsRemoteResource);
            Assert.IsTrue(viewExtension.AllowRemoteResources);
            Assert.IsTrue(viewExtension.ViewModel.IsRemoteResource);
            Assert.AreEqual(Visibility.Collapsed, visibilityBeforeEvent);
            Assert.AreEqual(Visibility.Visible, visibilityAfterEvent);
        }


        [Test]
        public void CanHandleDocsEventWithValidLink()
        {
            // Arrange
            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(localDocsFileLink, UriKind.Relative));
            var viewExtension = SetupNewViewExtension(true);

            // Act
            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();

            // Assert
            Assert.IsFalse(docsEvent.IsRemoteResource);
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(excelDocsFileHtmlHeader));
        }

        [Test]
        public void CanHandleDocsEventTriggeredFromDynamoViewModel()
        {
            // Arrange
            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(localDocsFileLink, UriKind.Relative));

            // Act
            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            this.ViewModel.OpenDocumentationLinkCommand.Execute(docsEvent);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();

            // Assert
            Assert.IsFalse(docsEvent.IsRemoteResource);
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(excelDocsFileHtmlHeader));
        }

        [Test]
        public void Displays404PageOnMissingDocFile()
        {
            // Arrange
            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri("missingFile.html", UriKind.Relative));
            var fileMissingHtmlHeader = "<h3>Error 404</h3>";
            var viewExtension = SetupNewViewExtension(true);

            // Act
            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();

            // Assert
            Assert.IsFalse(docsEvent.IsRemoteResource);
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(fileMissingHtmlHeader));
        }

        [Test]
        public void DisplaysNoContentPageOnEmptyDocFile()
        {
            // Arrange
            var viewExtension = SetupNewViewExtension(true);

            // make the empty file first
            var docsPath = DocumentationBrowserViewModel.GetBuiltInDocumentationFolderPath();
            var emptyFileName = "empty.html";
            var emptyFilePath = Path.Combine(docsPath, emptyFileName);
            File.WriteAllText(emptyFilePath, string.Empty);

            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(emptyFileName, UriKind.Relative));
            
            // Act
            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();
            File.Delete(emptyFilePath);

            // Assert
            Assert.IsFalse(docsEvent.IsRemoteResource);
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(indexPageHtmlHeader));
        }

        #region Helpers

        private DocumentationBrowserViewExtension SetupNewViewExtension(bool runLoadedMethod = false)
        {
            var extension = new DocumentationBrowserViewExtension();
            if (runLoadedMethod)
            {
                var loadedParams = new ViewLoadedParams(this.View, this.ViewModel);
                extension.Loaded(loadedParams);
            }
            return extension;
        }

        private List<MenuItem> GetDocsMenuItems()
        {
            var loadedParams = new ViewLoadedParams(this.View, this.ViewModel);

            // get menu items that match the extension's menu item
            return loadedParams.dynamoMenu.Items
                .Cast<MenuItem>()
                .Where(x => ((string)x.Header).Contains("View"))
                .Select(x => x.Items)
                .FirstOrDefault()
                .Cast<MenuItem>()
                .Where(x => ((string)x.Header).Equals("Show Documentation Browser"))
                .ToList();
        }

        private string GetSidebarDocsBrowserContents()
        {
            var docsTab = GetDocsTabItem();
            var docsView = docsTab.Content as DocumentationBrowserView;
            var docsViewModel = docsView.DataContext as DocumentationBrowserViewModel;
            return docsViewModel.GetContent();
        }

        private TabItem GetDocsTabItem()
        {
            return this.View.ExtensionTabItems
                .Where(x => x.Content.GetType().Equals(typeof(DocumentationBrowserView)))
                .FirstOrDefault();
        }

        private void ShowDocsBrowser()
        {
            GetDocsMenuItems().First().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        #endregion
    }
}
