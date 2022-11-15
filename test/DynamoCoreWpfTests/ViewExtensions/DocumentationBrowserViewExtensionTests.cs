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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Dynamo;

namespace DynamoCoreWpfTests
{
    public class DocumentationBrowserViewExtensionTests : DynamoTestUIBase
    {
        private const string docsTabName = "Documentation Browser";
        private const string externalLink = "http://dictionary.dynamobim.org";
        private const string localDocsFileLink = "ExcelNotInstalled.html";
        private const string indexPageHtmlHeader = "<h2>Dynamo Documentation Browser</h2>";
        private const string excelDocsFileHtmlHeader = "<h2>Excel not installed </h2>";
        private const string fileMissingHtmlHeader = "<h3>Error 404</h3>";
        private const string nodeDocumentationInfoHeader = "<strong>Node Information</strong>";
        private const string nodeDocumentationInfoOriginalNodeName = "<h2>Original Node Name</h2>";
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
            this.ViewModel.NewHomeWorkspaceCommand.Execute(null);
            var visibilityBeforeShowStartPageEvent = docsView.documentationBrowser.Visibility;

            // Act
            this.ViewModel.DisplayStartPageCommand.Execute(null);
            var visibilityAfterShowStartPageEvent = docsView.documentationBrowser.Visibility;

            // Assert
            Assert.AreEqual(Visibility.Visible, visibilityBeforeShowStartPageEvent);
            Assert.AreEqual(Visibility.Hidden, visibilityAfterShowStartPageEvent);
        }

        [Test]
        public void ViewExtensionIgnoresExternalEvents()
        {
            // Arrange
            var externalEvent = new OpenDocumentationLinkEventArgs(new Uri(externalLink));
            using (var viewExtension = SetupNewViewExtension())
            {
                // Act
                var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
                viewExtension.HandleRequestOpenDocumentationLink(externalEvent);
                var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;

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
            using (var viewExtension = SetupNewViewExtension(true))
            {
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
                var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
                var htmlContent = GetSidebarDocsBrowserContents();

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

                // Act
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var htmlContent = GetSidebarDocsBrowserContents();

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

                // Act
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var htmlContent = GetSidebarDocsBrowserContents();

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

                // Act
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var htmlContent = GetSidebarDocsBrowserContents();

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

                // Act
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var htmlContent = GetSidebarDocsBrowserContents();

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
                var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
                var htmlContent = GetSidebarDocsBrowserContents();

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
                var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
                viewExtension.HandleRequestOpenDocumentationLink(docsEvent);
                var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
                var htmlContent = GetSidebarDocsBrowserContents();

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

            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            node.Name = nodeRename; // Forces original name header to appear 
            var nodeAnnotationEventArgs = new OpenNodeAnnotationEventArgs(node, this.ViewModel);

            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            docBrowserviewExtension.HandleRequestOpenDocumentationLink(nodeAnnotationEventArgs);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();

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
            var expectedNodeDocumentationNamespace = $"<p><i>Package.{nodeName}</i></p>";
            var expectedAddtionalNodeDocumentationHeader = @"<h1 id=""hello-dynamo"">Hello Dynamo!</h1>";
            var expectedAddtionalNodeDocumentationImage = String.Format(@"<img id='drag--img' class='resizable--img'  src=""http://appassets/{0}"" alt=""Dynamo Icon image"" />", Path.GetFileName(localImagePath));


            // Act

            this.ViewModel.ExecuteCommand(
                 new DynamoModel.CreateNodeCommand(
                     Guid.NewGuid().ToString(), nodeName, 0, 0, false, false)
                 );

            var node = this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault();
            node.Name = nodeRename; // Forces original name header to appear 
            var nodeAnnotationEventArgs = new OpenNodeAnnotationEventArgs(node, this.ViewModel);

            var tabsBeforeExternalEventTrigger = this.View.ExtensionTabItems.Count;
            docBrowserviewExtension.HandleRequestOpenDocumentationLink(nodeAnnotationEventArgs);
            var tabsAfterExternalEventTrigger = this.View.ExtensionTabItems.Count;
            var htmlContent = GetSidebarDocsBrowserContents();
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
            var nodeWithDocumentation = "Package.Package.Hello";
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
            return GetSidebarDocsBrowserContents();
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
                Assert.IsNullOrEmpty(output);
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
                Assert.IsNullOrEmpty(output);
            }
        }

        #region Helpers
        private string[] htmlResources()
        {
            return getContentFiles(@"*.html");
        }

        private string[] mdResources()
        {
            return getContentFiles(@"*.md");
        }

        private string[] getContentFiles(string wildcard)
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            var docsDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, @"src\DocumentationBrowserViewExtension\Docs");

            return Directory.GetFiles(docsDirectory, wildcard);
        }

        #endregion
    }
}
