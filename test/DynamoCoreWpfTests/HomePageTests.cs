using System;
using System.IO;
using System.Threading.Tasks;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Controls;
using Dynamo.UI.Views;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using Microsoft.Web.WebView2.Wpf;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    internal class HomePageTests : DynamoTestUIBase
    {
        #region initialization tests
        [Test]
        [Ignore("Test refactor")]
        public void GuidedTourItems_InitializationShouldContainExpectedItems()
        {
            // Arrange
            var vm = View.DataContext as DynamoViewModel;
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;

            // Act - Delegates are set in the constructor
            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Assert
            Assert.IsNotNull(homePage.GuidedTourItems);
            Assert.AreEqual(3, homePage.GuidedTourItems.Count);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.GetStartedGuide.TrimStart('_'), homePage.GuidedTourItems[0].Name);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.OnboardingGuide.TrimStart('_'), homePage.GuidedTourItems[1].Name);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.PackagesGuide.TrimStart('_'), homePage.GuidedTourItems[2].Name);
        }

        [Test]
        [Ignore("Test refactor")]
        public void ActionDelegates_ShouldBeProperlySetAfterConstruction()
        {
            // Arrange
            var vm = View.DataContext as DynamoViewModel;
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;

            // Act - Delegates are set in the constructor
            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Assert
            Assert.IsNotNull(homePage.RequestOpenFile);
            Assert.IsNotNull(homePage.RequestShowGuidedTour);
            Assert.IsNotNull(homePage.RequestNewWorkspace);
            Assert.IsNotNull(homePage.RequestOpenWorkspace);
            Assert.IsNotNull(homePage.RequestNewCustomNodeWorkspace);
            Assert.IsNotNull(homePage.RequestApplicationLoaded);
            Assert.IsNotNull(homePage.RequestShowSampleFilesInFolder);
            Assert.IsNotNull(homePage.RequestShowBackupFilesInFolder);
            Assert.IsNotNull(homePage.RequestShowTemplate);
        }
        #endregion

        #region integration tests

        // A custom script to execute the click event of
        private static string SCRIPT(string elementId)
        {
            return $@"(() => {{
            const optionId = `{elementId}`;
            const optionElement = document.getElementById(optionId);
            if (optionElement) {{
                optionElement.click();
                return true; // Indicate the click was attempted
            }} else {{
                console.log('Option element not found');
                return false; // Indicate failure to find the element
            }}
        }})();";
        }

        private static string FILE_SCRIPT(string dropdown, int index)
        {
            var elementId = $"{dropdown}-{index}";
            return SCRIPT(elementId);
        }

        private static string SAMPLESFOLDER_SCRIPT()
        {
            var elementId = $"showSampleFilesLink";
            return SCRIPT(elementId);
        }

        internal static string CONTAINER_SCRIPT(string elementId)
        {
            return $@"(() => {{
            const optionId = `{elementId}`;
            const optionElement = document.getElementById(optionId);
            const count = optionElement.children.length; // Return the count of elements found
            if(count !== undefined) return count;
            else return 0;
        }})();";
        }

        internal static string CONTAINER_ITEM_CLICK_SCRIPT(string elementId)
        {
            return $@"(() => {{
            const optionId = `{elementId}`;
            const optionElement = document.getElementById(optionId);
            if (optionElement && optionElement.children.length > 0) {{
                // Assuming each child div contains one <a> element
                const firstChild = optionElement.children[0];
                const anchor = firstChild.querySelector('a'); // Find the <a> tag within the first child
                if (anchor) {{
                    anchor.click(); 
                    return true; 
                }}
            }}
            return false; // Indicate failure or that the <a> tag doesn't exist
        }})();";
        }


        [Test]
        [Ignore("Test refactor")]
        public async Task CanClickRecentGraph()
        {
            // Arrange
            var script = CONTAINER_ITEM_CLICK_SCRIPT("graphContainer");
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\nodeLocationTest.dyn");
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            string receivedPath = null; 
            HomePage.TestHook = (path) =>
            {
                receivedPath = path; 
                wasTestHookInvoked = true;
            };

            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment
            // Manually add 1 recent graph to test with
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            startPage.RecentFiles.Add(new StartPageListItem(filePath) { ContextData = filePath });
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "Was not able to execute the click event.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenFile method did not invoke the test hook as expected.");
            Assert.AreEqual(filePath, receivedPath, "The command did not return the same filePath");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task CanClickSampleGraph()
        {
            // Arrange
            var script = CONTAINER_ITEM_CLICK_SCRIPT("samplesContainer");
            var rootName = "root";
            var fileName = "testFile";
            var rootPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\");
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\nodeLocationTest.dyn");
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;
            string receivedPath = null;
            HomePage.TestHook = (path) =>
            {
                receivedPath = path;
                wasTestHookInvoked = true;
            };

            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment
            // Manually add 1 sample to test with
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            var childEntity = new SampleFileEntry(fileName, filePath);
            var rootEntity = new SampleFileEntry(rootName, rootPath);
            rootEntity.AddChildSampleFile(childEntity);
            startPage.SampleFiles.Add(rootEntity);
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            //// Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            //// Assert
            Assert.IsTrue(interactCompleted, "Was not able to execute the click event.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenFile method did not invoke the test hook as expected.");
            Assert.AreEqual(filePath, receivedPath, "The command did not return the same filePath");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task CanClickTourGuide()
        {
            // Arrange
            var script = CONTAINER_ITEM_CLICK_SCRIPT("guidesContainer");
            var guideType = "UserInterface";
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;
            string receivedType = null;
            HomePage.TestHook = (type) =>
            {
                receivedType = type;
                wasTestHookInvoked = true;
            };

            Assert.IsFalse(wasTestHookInvoked);

            var homePage = View.homePage;
            
            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "Was not able to execute the click event.");
            Assert.IsTrue(wasTestHookInvoked, "The StartGuidedTour method did not invoke the test hook as expected.");
            Assert.AreEqual(guideType, receivedType, "The command did not return the expected guide type");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ReceiveCorrectNumberOfRecentGrphs()
        {
            // Arrange
            var script = CONTAINER_SCRIPT("graphContainer");
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\nodeLocationTest.dyn");
            var vm = View.DataContext as DynamoViewModel;


            // Create the startPage manually, as it is not created under Test environment
            // Manually add 1 recent graph to test with
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            startPage.RecentFiles.Add(new StartPageListItem(filePath) { ContextData=filePath });
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<int>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(1, interactCompleted, "Did not receive correct number of recent graphs.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ReceiveCorrectNumberOfSamples()
        {
            // Arrange
            var script = CONTAINER_SCRIPT("samplesContainer");
            var rootName = "root";
            var fileName = "testFile";
            var rootPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\");
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\nodeLocationTest.dyn");
            var vm = View.DataContext as DynamoViewModel;


            // Create the startPage manually, as it is not created under Test environment
            // Manually add 1 sample to test with
            // We remove the root folder on the front end side, so make sure there is a root folder to discount for the test
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            var childEntity = new SampleFileEntry(fileName, filePath);
            var rootEntity = new SampleFileEntry(rootName, rootPath);
            rootEntity.AddChildSampleFile(childEntity);
            startPage.SampleFiles.Add(rootEntity);
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<int>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(1, interactCompleted, "Did not receive correct number of sample files.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ReceiveCorrectNumberOfTourGuides()
        {
            // Arrange
            var script = CONTAINER_SCRIPT("guidesContainer");
            var homePage = View.homePage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<int>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(3, interactCompleted, "Did not receive correct number of guides.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ReceiveCorrectNumberOfCarouselVideos()
        {
            // Arrange
            var script = CONTAINER_SCRIPT("videoCarousel");
            var homePage = View.homePage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<int>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(10, interactCompleted, "Did not receive correct number of videos.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }
        
        [Test]
        [Ignore("Test refactor")]
        public async Task CanRunNewHomeWorkspaceCommandFromHomePage()
        {
            // Arrange
            var dropdown = "newDropdown";
            var index = 0;
            var script = FILE_SCRIPT(dropdown, index);
            var vm = View.DataContext as DynamoViewModel;
            var hasWorkspaceBeenCleared = false;
            void Model_WorkspaceCleared(WorkspaceModel model)
            {
                vm.Model.WorkspaceCleared -= Model_WorkspaceCleared;
                hasWorkspaceBeenCleared = true;
            }

            // A side effect of running the NewHomeWorkspaceCommand is that the workspace will be cleared
            vm.Model.WorkspaceCleared += Model_WorkspaceCleared;

            // Create the startPage manually, as it is not created under Test environment 
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The NewWorkspace script did not run as expected.");
            Assert.IsTrue(hasWorkspaceBeenCleared, "The NewWorkspace method did not trigger the command as expected.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task CanRunNewCustomNodeCommandFromHomePage()
        {
            // Arrange
            var dropdown = "newDropdown";
            var index = 1;
            var script = FILE_SCRIPT(dropdown, index);
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            HomePage.TestHook = (arg) => wasTestHookInvoked = true;
            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment 
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The OpenWorkspace script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenWorkspace method did not invoke the test hook as expected.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task CanOpenWorkspaceCommandFromHomePage()
        {
            // Arrange
            var dropdown = "openDropdown";
            var index = 0;
            var script = FILE_SCRIPT(dropdown, index);
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            HomePage.TestHook = (arg) => wasTestHookInvoked = true;
            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment 
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The OpenWorkspace script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenWorkspace method did not invoke the test hook as expected.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ShowTemplateCommandFromHomePage()
        {
            // Arrange
            var dropdown = "openDropdown";
            var index = 1;
            var script = FILE_SCRIPT(dropdown, index);
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            HomePage.TestHook = (arg) => wasTestHookInvoked = true;
            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment 
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The ShowTemplate script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The ShowTemplate method did not invoke the test hook as expected.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ShowBackupFolderCommandFromHomePage()
        {
            // Arrange
            var dropdown = "openDropdown";
            var index = 2;
            var script = FILE_SCRIPT(dropdown, index);
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            HomePage.TestHook = (arg) => wasTestHookInvoked = true;
            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment 
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The ShowBackupFolder script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The ShowBackupFolder method did not invoke the test hook as expected.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public async Task ShowSampleFilesFolderCommandFromHomePage()
        {
            // Arrange
            var script = SAMPLESFOLDER_SCRIPT();
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            HomePage.TestHook = (arg) => wasTestHookInvoked = true;
            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment 
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var interactCompleted = await InteractSimple<bool>(homePage.dynWebView, script);

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The ShowSampleFilesFolderCommand script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The ShowSampleFilesFolderCommand method did not invoke the test hook as expected.");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("Test refactor")]
        public void CanOpenGraphOnDragAndDrop()
        {
            // Arrange
            var filePath = new Uri(Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\nodeLocationTest.dyn"));
            var vm = View.DataContext as DynamoViewModel;
            var wasTestHookInvoked = false;

            string receivedPath = null;
            HomePage.TestHook = (path) =>
            {
                receivedPath = path;
                wasTestHookInvoked = true;
            };

            Assert.IsFalse(wasTestHookInvoked);

            // Create the startPage manually, as it is not created under Test environment
            var startPage = new StartPageViewModel(vm, true);
            var homePage = View.homePage;
            homePage.DataContext = startPage;

            DispatcherUtil.DoEventsLoop(() =>
            {
                return homePage.initState == DynamoUtilities.AsyncMethodState.Done;
            });

            Assert.AreEqual(DynamoUtilities.AsyncMethodState.Done, homePage.initState);

            // Act
            var result = homePage.ProcessUri(filePath.ToString());

            Assert.IsTrue(result, "The file Uri was not processed correctly.");

            // Clean up to avoid failures testing in pipeline
            var windowClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(wasTestHookInvoked, "The OpenFile method did not invoke the test hook as expected.");
            Assert.AreEqual(filePath, receivedPath, "The command did not return the same filePath");
            Assert.IsTrue(windowClosed, "Dynamo View was not closed correctly.");
        }
        #endregion

        #region helpers

        /// <summary>
        /// A helper method to make sure the View is closed before we proceed to the next test
        /// </summary>
        /// <param name="view">The DynamoView object</param>
        /// <returns></returns>
        private bool CloseViewAndCleanup(DynamoView view)
        {
            bool windowClosed = false;
            void WindowClosed(object sender, EventArgs e)
            {
                windowClosed = true;
                view.Closed -= WindowClosed; 
            }

            view.Closed += WindowClosed; 
            view.Close(); 

            return windowClosed; 
        }

        internal static async Task<T> InteractSimple<T>(WebView2 web, string script)
        {
            try
            {
                await Task.Delay(500); // Allow homepage class to catch up
                var scriptTask = await web.CoreWebView2.ExecuteScriptAsync(script);
                await Task.Delay(500); // Allow homepage class to catch up
                return DeserializeResult<T>(scriptTask);
            }
            catch (Exception ex)
            {
                // Return default value for T in case of error
                Assert.Fail($"Interaction failed with exception: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Deserealizes json based on the provided type
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="jsonResult">The json string to deserialize</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static T DeserializeResult<T>(string jsonResult)
        {
            // Handle deserialization based on the type of T
            if (typeof(T) == typeof(bool))
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<bool>(jsonResult);
                return (T)(object)result; // Cast to object first to avoid direct cast compilation error
            }
            else if (typeof(T) == typeof(int))
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<int>(jsonResult);
                return (T)(object)result;
            }
            else
            {
                throw new ArgumentException("Unsupported type for deserialization.");
            }
        }

        #endregion
    }
}
