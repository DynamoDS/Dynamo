using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.DocumentationBrowser;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests.Utility;
using DynamoUtilities;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DocumentationBrowserViewExtensionTests : DynamoTestUIBase
    {
        private const string docsTabName = "Documentation Browser";
        private const string externalLink = "http://dictionary.dynamobim.org";
        private const string localDocsFileLink = "ExcelNotInstalled.html";
        private const string indexPageHtmlHeader = "<h2>Dynamo Documentation Browser</h2>";
        private const string excelDocsFileHtmlHeader = "<h2>Excel not installed </h2>";
        private const string fileMissingHtmlHeader = "<h3>Error 404</h3>";
        private const string nodeDocumentationInfoHeader = "<strong>Node Information</strong>";
        private const string nodeDocumentationInfoOriginalNodeName = "<h2>Node Type</h2>";
        private const string nodeDocumentationInfoNodeDescription = "<h2>Description</h2>";
        private const string nodeDocumentationInfoNodeInputsAndOutputs = "<strong>Inputs and Outputs</strong>";
        private const string nodeDocumentationInfoNodeInputs = "<h2>Inputs</h2>";
        private const string nodeDocumentationInfoNodeOutputs = "<h2>Outputs</h2>";

        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), @"core\docbrowser\pkgs"); } }

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
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("BuiltIn.ds");
            libraries.Add("FunctionObject.ds");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
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

            WaitForWebView2Initialization();

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

            WaitForWebView2Initialization();

            var docsView = GetDocsTabItem().Content as DocumentationBrowserView;
            this.ViewModel.NewHomeWorkspaceCommand.Execute(null);
            var visibilityBeforeShowStartPageEvent = docsView.documentationBrowser.Visibility;

            // Act
            this.ViewModel.DisplayStartPageCommand.Execute(null);
            var visibilityAfterShowStartPageEvent = docsView.documentationBrowser.Visibility;

            // Assert
            Assert.AreEqual(Visibility.Visible, visibilityBeforeShowStartPageEvent);
            Assert.AreEqual(Visibility.Hidden, visibilityAfterShowStartPageEvent);
        }

        /// <summary>
        /// This test validates that the Virtual Directory that will be created with WebView2 exists so the images will be loaded for a package node documentation
        /// </summary>
        [Test]
        public void CanCreatePackageNodeDocumentationAndLoadImages()
        {
            // Arrange
            RaiseLoadedEvent(this.View);

            var testDirectory = GetTestDirectory(this.ExecutingDirectory);
            var localImagePath = Path.Combine(testDirectory, @"core\docbrowser\pkgs\PackageWithNodeDocumentation\doc\icon.png");
            var packageDocPath = Path.GetDirectoryName(localImagePath);

            var docBrowserviewExtension = this.View.viewExtensionManager.ViewExtensions.OfType<DocumentationBrowserViewExtension>().FirstOrDefault();
            var browserView = docBrowserviewExtension.BrowserView;

            var nodeName = "Package.Hello";
            var nodeRename = "New node name";
            var expectedImageContent = String.Format(@"<img id='drag--img' class='resizable--img'  src=""http://appassets/{0}"" alt=""Dynamo Icon image"" />", Path.GetFileName(localImagePath));

            // Act
            this.ViewModel.ExecuteCommand(
                 new DynamoModel.CreateNodeCommand(
                     Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
                 );

            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            node.Name = nodeRename;// Forces original name header to appear

            var htmlContent = RequestNodeDocs(node);

            //There are times in which the URL contain characters like backslash (%5C) then they need to be replaced by the normal slash "/"
            htmlContent = htmlContent.Replace(@"%5C", "/");

            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(browserView.VirtualFolderPath));
            Assert.IsTrue(Directory.Exists(browserView.VirtualFolderPath));
            //Check that the virtual folder will be created in the Package/doc folder so images will be loaded correctly
            Assert.IsTrue(browserView.VirtualFolderPath.Contains(packageDocPath));
            Assert.IsTrue(htmlContent.Contains(expectedImageContent));
        }

        [Test]
        public void ViewExtensionIgnoresExternalEvents()
        {
            // Arrange
            var externalEvent = new OpenDocumentationLinkEventArgs(new Uri(externalLink));
            using (var viewExtension = SetupNewViewExtension())
            {
                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                viewExtension.HandleRequestOpenDocumentationLink(externalEvent);
                DispatcherUtil.DoEventsLoop();

                Assert.AreEqual(AsyncMethodState.NotStarted, viewExtension.BrowserView.initState);
                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

                // Assert
                Assert.IsTrue(externalEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(0, tabsAfterExternalEventTrigger);
            }
        }

        [Test]
        public void CanHandleDocsEventWithValidLink()
        {
            // Arrange
            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(localDocsFileLink, UriKind.Relative));
            using (var viewExtension = SetupNewViewExtension(true))
            {
                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                var htmlContent = RequestDocLink(viewExtension, docsEvent);
                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

                // Assert
                Assert.IsFalse(docsEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(1, tabsAfterExternalEventTrigger);
                Assert.IsTrue(htmlContent.Contains(excelDocsFileHtmlHeader));
            }
        }

        [Test]
        public void CanHandleDocsEventTriggeredFromDynamoViewModel()
        {
            // Arrange
            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(localDocsFileLink, UriKind.Relative));

            // Act
            var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
            this.ViewModel.OpenDocumentationLinkCommand.Execute(docsEvent);

            WaitForWebView2Initialization();

            var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();

            // Assert
            Assert.IsFalse(docsEvent.IsRemoteResource);
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(excelDocsFileHtmlHeader));
        }

        [Test]
        public void CanHandleDocsEventTriggeredFromDynamoViewModelMultipleTimes()
        {
            // Arrange
            var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(localDocsFileLink, UriKind.Relative));

            // Act
            var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
            this.ViewModel.OpenDocumentationLinkCommand.Execute(docsEvent);
            this.ViewModel.OpenDocumentationLinkCommand.Execute(docsEvent);
            this.ViewModel.OpenDocumentationLinkCommand.Execute(docsEvent);

            WaitForWebView2Initialization();

            var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
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
            using (var viewExtension = SetupNewViewExtension(true))
            {
                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                var htmlContent = RequestDocLink(viewExtension, docsEvent);
                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

                // Assert
                Assert.IsFalse(docsEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(1, tabsAfterExternalEventTrigger);
                Assert.IsTrue(htmlContent.Contains(fileMissingHtmlHeader));
            }
        }

        [Test]
        public void DisplaysHtmlEmbeddedInLoadedAssemblies()
        {
            // Arrange
            using (var viewExtension = SetupNewViewExtension(true))
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = "DocumentationBrowserViewExtension";
                var fileName = "ArgumentNullException.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                var htmlContent = RequestDocLink(viewExtension, docsEvent);
                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                

                // Assert
                Assert.IsFalse(docsEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(1, tabsAfterExternalEventTrigger);
                Assert.IsTrue(htmlContent.Contains("<h2>Value cannot be null</h2>"));
            }

        }

        [Test]
        public void Displays404PageWhenLinkPointsToAssemblyThatCannotBeFound()
        {
            // Arrange
            using (var viewExtension = SetupNewViewExtension(true))
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = "NonExisting";
                var fileName = "Whatever.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                var htmlContent = RequestDocLink(viewExtension, docsEvent);
                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                

                // Assert
                Assert.IsFalse(docsEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(1, tabsAfterExternalEventTrigger);
                Assert.IsTrue(htmlContent.Contains(fileMissingHtmlHeader));
            }
        }

        /// <summary>
        /// Test with Dynamo running in "en-us" culture and help content requested
        /// for a package that contains localized help content for "en-us".
        /// </summary>
        [Test]
        public void DisplaysLocalizedContentWhenAvailable()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-us");
            var viewExtension = SetupNewViewExtension(true);

            try
            {

                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = "SpecificCultureDocs";
                var fileName = "DivisionByZero.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                var htmlContent = RequestDocLink(viewExtension, docsEvent);

                // Assert
                StringAssert.Contains("<h3>Division by zero - en-us</h3>", htmlContent);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                viewExtension.Dispose();
            }
        }

        /// <summary>
        /// Test with Dynamo running in "es-uy" culture and help content requested
        /// for a package that doesn't contain localized help content for "es-uy",
        /// but does contain help content for neutral culture "es".
        /// </summary>
        [Test]
        public void DisplayNeutralCultureContentWhenSpecificCultureContentIsNotAvailable()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es-uy");
            var viewExtension = SetupNewViewExtension(true);
            try
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = "NeutralCultureDocs";
                var fileName = "DivisionByZero.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                var htmlContent = RequestDocLink(viewExtension, docsEvent);

                // Assert
                StringAssert.Contains("<h3>Division entre cero - es</h3>", htmlContent);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                viewExtension.Dispose();
            }
        }

        /// <summary>
        /// Test with Dynamo running in "en" culture and help content requested
        /// for a package that doesn't contain localized help content for "en",
        /// but does contain help content for specific culture "en-us".
        /// </summary>
        [Test]
        public void DisplaySpecificCultureContentWhenNeutralCultureContentIsNotAvailable()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            var viewExtension = SetupNewViewExtension(true);
            try
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = "SpecificCultureDocs";
                var fileName = "DivisionByZero.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                var htmlContent = RequestDocLink(viewExtension, docsEvent);

                // Assert
                StringAssert.Contains("<h3>Division by zero - en-us</h3>", htmlContent);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                viewExtension.Dispose();
            }
        }

        /// <summary>
        /// Test with Dynamo running in "fr-ca" culture and help content requested
        /// for a package that doesn't contain localized help content for "fr-ca"
        /// nor the neutral culture "fr", so it falls back to invariant culture
        /// help content.
        /// </summary>
        [Test]
        public void DisplaysInvariantContentWhenNoCompatibleLocalizedContentIsAvailable()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-ca");
            var viewExtension = SetupNewViewExtension(true);
            try
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = "InvariantCultureDocs";
                var fileName = "DivisionByZero.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                var htmlContent = RequestDocLink(viewExtension, docsEvent);

                // Assert
                StringAssert.Contains("<h3>Division by zero - invariant</h3>", htmlContent);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                viewExtension.Dispose();
            }
        }

        [Test]
        public void RemovesScriptTagsFromLoadedHtml()
        {
            // Arrange
            using (var viewExtension = SetupNewViewExtension(true))
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = GetType().Assembly.GetName().Name;
                var fileName = "DocumentationBrowserScriptsTest.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                var htmlContent = RequestDocLink(viewExtension, docsEvent);

                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

                // Assert
                Assert.IsFalse(docsEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(1, tabsAfterExternalEventTrigger);
                Assert.IsTrue(htmlContent.Contains("<h2 id=\"heading\">Division by zero</h2>"));
                Assert.False(htmlContent.Contains("document.getElementById(\"heading\").innerHTML = \"Script1\";"));
            }
        }

        [Test]
        public void DPIScriptExists()
        {
            // Arrange
            using (var viewExtension = SetupNewViewExtension(true))
            {
                // Reference an embedded HTML file in a loaded assembly
                var assemblyName = GetType().Assembly.GetName().Name;
                var fileName = "DocumentationBrowserScriptsTest.html";
                var uri = $"{assemblyName};{fileName}";
                var docsEvent = new OpenDocumentationLinkEventArgs(new Uri(uri, UriKind.Relative));

                // Act
                var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;
                var htmlContent = RequestDocLink(viewExtension, docsEvent);
                var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

                // Assert
                Assert.IsFalse(docsEvent.IsRemoteResource);
                Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
                Assert.AreEqual(1, tabsAfterExternalEventTrigger);
                Assert.IsTrue(htmlContent.Contains(@"<script> function getDPIScale()"));
                Assert.IsTrue(htmlContent.Contains(@"function adaptDPI()"));
            }
        }

        [Test]
        public void GetResourceNameWithCultureNameReturnsSameAsInputWhenCultureIsNull()
        {
            var name = "MyPage.html";
            var result = ResourceUtilities.GetResourceNameWithCultureName(name, null);
            Assert.AreEqual(name, result);
        }

        [Test]
        public void GetResourceNameWithCultureNameReturnsSameAsInputWhenItDoesNotHaveAnExtension()
        {
            var name = "NotAPage";
            var result = ResourceUtilities.GetResourceNameWithCultureName(name, CultureInfo.GetCultureInfo("en-US"));
            Assert.AreEqual(name, result);
        }

        [Test]
        public void GetResourceNameWithCultureNameWorksWithValidCultureAndInputName()
        {
            var name = "MyPage.html";
            var result = ResourceUtilities.GetResourceNameWithCultureName(name, CultureInfo.GetCultureInfo("en-US"));
            Assert.AreEqual("MyPage.en-US.html", result);
        }

        [Test]
        public void CanCreateNodeDocumenationHtmlFromNodeAnnotationEventArgsWithOOTBNodeWithoutAddtionalDocumentation()
        {
            // Arrange
            RaiseLoadedEvent(this.View);
            var docBrowserviewExtension = this.View.viewExtensionManager.ViewExtensions.OfType<DocumentationBrowserViewExtension>().FirstOrDefault();
            var nodeName = "+";
            var nodeRename = "New node name";
            var expectedNodeDocumentationTitle = $"<h1>{nodeRename}</h1>";
            var expectedNodeDocumentationNamespace = $"<p><i>{nodeName}</i></p>";

            // Act
            this.ViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(
                    Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
                );

            var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            node.Name = nodeRename; // Forces original name header to appear

            var htmlContent = RequestNodeDocs(node);

            var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

            // Assert
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(expectedNodeDocumentationTitle));
            Assert.IsTrue(htmlContent.Contains(expectedNodeDocumentationNamespace));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoHeader));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeDescription));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoOriginalNodeName));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeInputsAndOutputs));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeInputs));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeOutputs));
        }

        [Test]
        public void CanCreateNodeDocumenationHtmlFromNodeAnnotationEventArgsWithPackageNodeWithAddtionalDocumentation()
        {
            // Arrange
            RaiseLoadedEvent(this.View);

            var testDirectory = GetTestDirectory(this.ExecutingDirectory);
            var localImagePath = Path.Combine(testDirectory, @"core\docbrowser\pkgs\PackageWithNodeDocumentation\doc\icon.png");

            var docBrowserviewExtension = this.View.viewExtensionManager.ViewExtensions.OfType<DocumentationBrowserViewExtension>().FirstOrDefault();
            var nodeName = "Package.Hello";
            var nodeRename = "New node name";
            var expectedNodeDocumentationTitle = $"<h1>{nodeRename}</h1>";
            var expectedNodeDocumentationNamespace = $"<p><i>PackageWithDocs.{nodeName}</i></p>";
            var expectedAddtionalNodeDocumentationHeader = @"<h1 id=""hello-dynamo"">Hello Dynamo!</h1>";
            var expectedAddtionalNodeDocumentationImage = String.Format(@"<img id='drag--img' class='resizable--img'  src=""http://appassets/{0}"" alt=""Dynamo Icon image"" />", Path.GetFileName(localImagePath));


            // Act

            this.ViewModel.ExecuteCommand(
                 new DynamoModel.CreateNodeCommand(
                     Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
                 );

            var tabsBeforeExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();

            node.Name = nodeRename; // Forces original name header to appear 

            var htmlContent = RequestNodeDocs(node);

            var tabsAfterExternalEventTrigger = this.ViewModel.SideBarTabItems.Count;

            htmlContent = htmlContent.Replace(@"%5C", "/");

            // Assert
            Assert.AreEqual(0, tabsBeforeExternalEventTrigger);
            Assert.AreEqual(1, tabsAfterExternalEventTrigger);
            Assert.IsTrue(htmlContent.Contains(expectedNodeDocumentationTitle));
            Assert.IsTrue(htmlContent.Contains(expectedNodeDocumentationNamespace));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoHeader));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeDescription));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoOriginalNodeName));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeInputs));
            Assert.IsTrue(htmlContent.Contains(nodeDocumentationInfoNodeOutputs));
            Assert.IsTrue(htmlContent.Contains(expectedAddtionalNodeDocumentationHeader));
            Assert.IsTrue(htmlContent.Contains(expectedAddtionalNodeDocumentationImage));
        }

        [Test]
        public void CanGetNodeDocumentationMarkdownFromPackageDocumentationManager()
        {
            // Arrange
            var packageName = "Package";
            var nodeWithDocumentation = "PackageWithDocs.Package.Hello";
            var nodeWithoutDocumentation = "Package.Package.Package";

            // Assert
            Assert.That(PackageDocumentationManager.Instance.ContainsAnnotationDoc(Path.Combine(packageName, nodeWithDocumentation)));
            Assert.That(!PackageDocumentationManager.Instance.ContainsAnnotationDoc(Path.Combine(packageName, nodeWithoutDocumentation)));
        }

        [Test]
        public void DocsCanBeLoadedForDSNonPackageNodesFrom_FallBackPath()
        {
            //setup the docs browser to point to our fake fallback folder.
            var testFallbackDocsPath = Path.Combine(GetTestDirectory(this.ExecutingDirectory), "Tools", "docGeneratorTestFiles", "fallback_docs");
            PackageDocumentationManager.Instance.dynamoCoreFallbackDocPath = new DirectoryInfo(testFallbackDocsPath);

            //make a request for a node we've generated docs for
            RaiseLoadedEvent(this.View);

            var nodeName = "List.Rank";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
                Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
            );
            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            var htmlContent = RequestNodeDocs(node);

            Assert.IsTrue(htmlContent.Contains("list.rank sample docs"));

            ViewModel.Model.CurrentWorkspace.RemoveAndDisposeNode(node);

            //next node
            nodeName = "LoopWhile";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
              Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
          );
            node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            htmlContent = RequestNodeDocs(node);

            Assert.IsTrue(htmlContent.Contains("loopwhile sample docs"));
        }
        [Test]
        public void DocsAreLoadedFromHostPathBeforeCorePath()
        {
            //setup the docs browser to point to our fake fallback folder.
            var testFallbackDocsPath = Path.Combine(GetTestDirectory(this.ExecutingDirectory), "Tools", "docGeneratorTestFiles", "fallback_docs");
            var testHostFallbackDocsPath = Path.Combine(GetTestDirectory(this.ExecutingDirectory), "Tools", "docGeneratorTestFiles", "host_fallback_docs");

            PackageDocumentationManager.Instance.dynamoCoreFallbackDocPath = new DirectoryInfo(testFallbackDocsPath);
            PackageDocumentationManager.Instance.hostDynamoFallbackDocPath = new DirectoryInfo(testHostFallbackDocsPath);

            //make a request for a node we've generated docs for
            RaiseLoadedEvent(this.View);

            var nodeName = "List.Rank";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
                Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
            );
            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            var htmlContent = RequestNodeDocs(node);

            Assert.IsTrue(htmlContent.Contains("list.rank sample docs from host path"));
        }

        [Test]
        public void DocsCanBeLoadedForCoreNodeModelNodesFrom_FallBackPath()
        {
            //setup the docs browser to point to our fake fallback folder.
            var testFallbackDocsPath = Path.Combine(GetTestDirectory(this.ExecutingDirectory), "Tools", "docGeneratorTestFiles", "fallback_docs");
            PackageDocumentationManager.Instance.dynamoCoreFallbackDocPath = new DirectoryInfo(testFallbackDocsPath);

            //make a request for a node we've generated docs for
            RaiseLoadedEvent(this.View);

            var nodeName = "CoreNodeModels.Watch";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
                Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
            );
            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            var htmlContent = RequestNodeDocs(node);

            Assert.IsTrue(htmlContent.Contains("corenodemodels.watch sample docs"));

            ViewModel.Model.CurrentWorkspace.RemoveAndDisposeNode(node);

            //next node
            nodeName = "CoreNodeModels.Logic.RefactoredIf";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
              Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
          );
            node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            htmlContent = RequestNodeDocs(node);

            Assert.IsTrue(htmlContent.Contains("corenodemodels.logic.refactoredif sample docs"));

            ViewModel.Model.CurrentWorkspace.RemoveAndDisposeNode(node);

            //next node
            nodeName = "CoreNodeModels.HigherOrder.Map";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
              Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
          );
            node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            htmlContent = RequestNodeDocs(node);

            Assert.IsTrue(htmlContent.Contains("corenodemodels.higherorder.map sample docs"));
        }

        private string RequestNodeDocs(Dynamo.Graph.Nodes.NodeModel node)
        {
            var docBrowserviewExtension = this.View.viewExtensionManager.ViewExtensions.OfType<DocumentationBrowserViewExtension>().FirstOrDefault();
            var nodeAnnotationEventArgs = new OpenNodeAnnotationEventArgs(node, this.ViewModel);
            docBrowserviewExtension.HandleRequestOpenDocumentationLink(nodeAnnotationEventArgs);

            WaitForWebView2Initialization(docBrowserviewExtension.BrowserView);

            return GetSidebarDocsBrowserContents();
        }

        private string RequestDocLink(DocumentationBrowserViewExtension viewExtension, OpenDocumentationLinkEventArgs docsEvent)
        {
            viewExtension.HandleRequestOpenDocumentationLink(docsEvent);

            WaitForWebView2Initialization(viewExtension.BrowserView);

            return GetSidebarDocsBrowserContents();
        }

        [Test]
        public void AddGraphInSpecificLocationToWorkspace()
        {
            //TODO see this issue:
            //https://github.com/nunit/nunit/issues/1200
            //we somehow need use single threaded sync context to force webview2 async initalization on this thread.
            //unfortunately it passes locally but then still fails on master-15.
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var tempDynDirectory = Path.Combine(testDirectory, "Temp Test Path");
            var dynFileName = Path.Combine(testDirectory, @"UI\BasicAddition.dyn");
            var insertDynFilePath = Path.Combine(tempDynDirectory, @"BasicAddition.dyn");

            var cleanTempDir = () =>
            {
                if (Directory.Exists(tempDynDirectory))
                {
                    Directory.Delete(tempDynDirectory, true);
                }
            };

            using (Disposable.Create(cleanTempDir, cleanTempDir))
            {
                //Creates a directory that has empty spaces in the name
                Directory.CreateDirectory(tempDynDirectory);
                //Copy the dyn file to the new directory with empty spaces
                File.Copy(dynFileName, insertDynFilePath);

                //Adds a Number node into the Current Workspace
                var nodeName = "Number";
                this.ViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(
                    Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
                );

                //Validates that we have just one node in the CurrentWorkspace
                Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Nodes.Count(), 1);

                var node = ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();

                RequestNodeDocs(node);
                var docsView = GetDocsTabItem().Content as DocumentationBrowserView;
                var docsViewModel = docsView.DataContext as DocumentationBrowserViewModel;

                docsViewModel.GraphPath = insertDynFilePath;

                //Insert the Graph into the current workspace
                docsViewModel.InsertGraph();
            }
            //Validates that we have 5 nodes the CurrentWorkspace (after the graph was added)
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Nodes.Count(), 5);
            DispatcherUtil.DoEvents();
        }

        [Test]
        public void Validate_GetGraphLinkFromMDLocation()
        {
            var nodeName = "Number";
            this.ViewModel.ExecuteCommand(
            new DynamoModel.CreateNodeCommand(
                Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
            );

            //Validates that we have just one node in the CurrentWorkspace
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Nodes.Count(), 1);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();

            //In this call the GetGraphLinkFromMDLocation() method is executed internally
            RequestNodeDocs(node);

            var docsView = GetDocsTabItem().Content as DocumentationBrowserView;
            var docsViewModel = docsView.DataContext as DocumentationBrowserViewModel;

            var graphPathValue = docsViewModel.GraphPath;

            var dynFileName = Path.GetFileNameWithoutExtension(docsViewModel.Link.AbsolutePath) + ".dyn";

            //This will return a path with the pkg doc + dyn file name
            var sharedFilesPath = Path.Combine(DocumentationBrowserView.SharedDocsDirectoryName, dynFileName);

            Assert.IsNotNull(graphPathValue);
            Assert.IsTrue(!string.IsNullOrEmpty(graphPathValue.ToString()));

            //check that the pathPath contains "NodeHelpSharedDocs//dynfilename"
            Assert.That(graphPathValue.Contains(sharedFilesPath));
        }
        [Test]
        public void Validate_GetGraphLinkFromPackage()
        {
            var nodeName = "Package.Hello";

            // Act
            this.ViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(
                    Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
            );

            //Validates that we have just one node in the CurrentWorkspace
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Nodes.Count(), 1);

            var node = ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();

            //In this call the GetGraphLinkFromMDLocation() method is executed internally
            RequestNodeDocs(node);

            var docsView = GetDocsTabItem().Content as DocumentationBrowserView;
            var docsViewModel = docsView.DataContext as DocumentationBrowserViewModel;

            var graphPathValue = docsViewModel.GraphPath;

            var dynFileName = Path.GetFileNameWithoutExtension(docsViewModel.Link.AbsolutePath) + ".dyn";

            Assert.IsNotNull(graphPathValue);
            Assert.IsTrue(!string.IsNullOrEmpty(graphPathValue));

            //check that the path contains "packageWithDocumentation"
            Assert.That(graphPathValue.Contains(Path.Combine("PackageWithNodeDocumentation", "doc", dynFileName)));
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
                .Where(x => (x.Header as string).Contains("E_xtensions"))
                .Select(x => x.Items)
                .FirstOrDefault()
                .Cast<MenuItem>()
                .Where(x => (x.Header as string).Equals("Show _Documentation Browser"))
                .ToList();
        }

        private void WaitForWebView2Initialization(DocumentationBrowserView docView = null)
        {
            if (docView == null)
            {
                var docsTab = GetDocsTabItem();
                docView = docsTab.Content as DocumentationBrowserView;
            }
            
            // Wait for the DocumentationBrowserView webview2 control to finish initialization
            DispatcherUtil.DoEventsLoop(() =>
            {
                return docView.initState == AsyncMethodState.Done;
            });

            Assert.AreEqual(AsyncMethodState.Done, docView.initState);
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
            return this.ViewModel.SideBarTabItems
                .Where(x => x.Content.GetType().Equals(typeof(DocumentationBrowserView)))
                .FirstOrDefault();
        }

        private void ShowDocsBrowser()
        {
            GetDocsMenuItems().First().RaiseEvent(new RoutedEventArgs(MenuItem.CheckedEvent));
        }

        public static void RaiseLoadedEvent(FrameworkElement element)
        {
            MethodInfo eventMethod = typeof(FrameworkElement).GetMethod("OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic);

            RoutedEventArgs args = new RoutedEventArgs(FrameworkElement.LoadedEvent);

            eventMethod.Invoke(element, new object[] { args });
        }

        #endregion
    }

    public class DocumentationBrowserViewExtensionContentTests : UnitTestBase
    {
        [Test, TestCaseSource(nameof(htmlResources))]
        public void CheckThatEmbeddedHtmlContentDoesNotSanitize(string htmlResource)
        {
            // Arrange
            var content = File.ReadAllText(htmlResource, Encoding.UTF8);

            // Don't test scripts
            if (content.StartsWith(@"<script>"))
            {
                return;
            }

            using (var converter = new Md2Html())
            {
                // Act
                var output = converter.SanitizeHtml(content);

                if (!string.IsNullOrEmpty(output))
                {
                    var thisIsIt = output;
                }

                // Assert
                Assert.IsTrue(string.IsNullOrEmpty(output));
            }
        }

        [Test, TestCaseSource(nameof(mdResources))]
        public void CheckThatEmbeddedMdContentDoesNotSanitize(string resource)
        {
            // Arrange
            var content = File.ReadAllText(resource, Encoding.UTF8);

            using (var converter = new Md2Html())
            {
                // Act
                var html = converter.ParseMd2Html(content, ExecutingDirectory);
                var output = converter.SanitizeHtml(html);

                // Assert
                Assert.IsTrue(string.IsNullOrEmpty(output));
            }
        }

        [Test]
        [TestCase("en-US")]
        [TestCase("cs-CZ")]
        [TestCase("ko-KR")]
        public void CheckThatExtendedCharactersAreCorrectlyInHTML(string language)
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            string mdFile = "Autodesk.DesignScript.Geometry.Curve.SweepAsSurface.md";
            string resource = Path.Combine(testDirectory, "Tools", "docGeneratorTestFiles", "fallback_docs", language, mdFile);

            Assert.True(File.Exists(resource), "The resource provided {0} doesn't exist in the path {1}", mdFile, resource);

            // Arrange
            var content = File.ReadAllText(resource, Encoding.UTF8);

            using (var converter = new Md2Html())
            {
                // Act
                var html = converter.ParseMd2Html(content, ExecutingDirectory);
                var output = converter.SanitizeHtml(html);

                // Assert
                Assert.IsTrue(string.IsNullOrEmpty(output));

                Regex rxExp = new Regex(@"#+\s[^\n]*\n(.*?)(?=\n##?\s|$)", RegexOptions.Singleline);

                //Apply RegEx expression to the md file to getting the content without headers
                MatchCollection matches = rxExp.Matches(content);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count == 0) continue;

                    var UTF8Content = match.Groups[1].Value.Trim();

                    //Discard the image due that inside the html is converted to <img .......
                    if(!UTF8Content.StartsWith("![")) 
                    {
                        // Assert
                        //Clean the content to remove characters also removed by the Md2Html.exe tool
                        var cleanedContent = UTF8Content.Replace("___", string.Empty).Trim();

                        //Apply a Regular Expresion in the HTML file for getting the first <p> 
                        Match m = Regex.Match(html, "<p>(.+?)<\\/p>", RegexOptions.IgnoreCase);
                        string specificHTMLContent = string.Empty;
                        if (m.Success)
                        {
                            specificHTMLContent = m.Groups[1].Value; 
                        }

                        Assert.IsTrue(specificHTMLContent == cleanedContent, "Part of the MD file content was not found in the HTML File");
                    }                   
                }
            }
        }

        #region Helpers
        private static string[] htmlResources()
        {
            return getContentFiles(@"*.html");
        }

        private static string[] mdResources()
        {
            return getContentFiles(@"*.md");
        }

        private static string[] getContentFiles(string wildcard)
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            var docsDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, @"src\DocumentationBrowserViewExtension\Docs");

            return Directory.GetFiles(docsDirectory, wildcard);
        }

        public static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        #endregion
    }
}
