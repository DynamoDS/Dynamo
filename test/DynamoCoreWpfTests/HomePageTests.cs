using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Controls;
using Dynamo.UI.Views;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    internal class HomePageTests : DynamoTestUIBase
    {
        #region initialization tests
        [Test]
        public void GuidedTourItems_InitializationShouldContainExpectedItems()
        {
            // Arrange
            var homePage = new HomePage();

            // Act - initialization happens in the constructor

            // Assert
            Assert.IsNotNull(homePage.GuidedTourItems);
            Assert.AreEqual(3, homePage.GuidedTourItems.Count);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.GetStartedGuide.TrimStart('_'), homePage.GuidedTourItems[0].Name);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.OnboardingGuide.TrimStart('_'), homePage.GuidedTourItems[1].Name);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.PackagesGuide.TrimStart('_'), homePage.GuidedTourItems[2].Name);
        }

        [Test]
        public void ActionDelegates_ShouldBeProperlySetAfterConstruction()
        {
            // Arrange
            var homePage = new HomePage();

            // Act - Delegates are set in the constructor

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
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void CanClickRecentGraph()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "Was not able to execute the click event.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenFile method did not invoke the test hook as expected.");
            Assert.AreEqual(filePath, receivedPath, "The command did not return the same filePath");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void CanClickSampleGraph()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "Was not able to execute the click event.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenFile method did not invoke the test hook as expected.");
            Assert.AreEqual(filePath, receivedPath, "The command did not return the same filePath");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void CanClickTourGuide()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "Was not able to execute the click event.");
            Assert.IsTrue(wasTestHookInvoked, "The StartGuidedTour method did not invoke the test hook as expected.");
            Assert.AreEqual(guideType, receivedType, "The command did not return the expected guide type");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ReceiveCorrectNumberOfRecentGrphs()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForIntInteractionToComplete(homePage, script, -1);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(1, interactCompleted, "Did not receive correct number of recent graphs.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ReceiveCorrectNumberOfSamples()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForIntInteractionToComplete(homePage, script, -1);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(1, interactCompleted, "Did not receive corrent number of sample files.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ReceiveCorrectNumberOfTourGuides()
        {
            // Arrange
            var script = CONTAINER_SCRIPT("guidesContainer");
            var homePage = View.homePage;

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForIntInteractionToComplete(homePage, script, -1);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(3, interactCompleted, "Did not receive corrent number of guides.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ReceiveCorrectNumberOfCarouselVideos()
        {
            // Arrange
            var script = CONTAINER_SCRIPT("videoCarousel");
            var homePage = View.homePage;

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForIntInteractionToComplete(homePage, script, -1);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.AreEqual(10, interactCompleted, "Did not receive corrent number of videos.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }
        
        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void CanRunNewHomeWorkspaceCommandFromHomePage()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The NewWorkspace script did not run as expected.");
            Assert.IsTrue(hasWorkspaceBeenCleared, "The NewWorkspace method did not trigger the command as expected.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void CanRunNewCustomNodeCommandFromHomePage()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The OpenWorkspace script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenWorkspace method did not invoke the test hook as expected.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void CanOpenWorkspaceCommandFromHomePage()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The OpenWorkspace script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The OpenWorkspace method did not invoke the test hook as expected.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ShowTemplateCommandFromHomePage()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);


            // Assert
            Assert.IsTrue(interactCompleted, "The ShowTemplate script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The ShowTemplate method did not invoke the test hook as expected.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ShowBackupFolderCommandFromHomePage()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The ShowBackupFolder script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The ShowBackupFolder method did not invoke the test hook as expected.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }

        [Test]
        [Ignore("IsNewAppHomeEnabled flag is set to false")]
        public void ShowSampleFilesFolderCommandFromHomePage()
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

            InitializeWebView2(homePage.dynWebView);

            // Act
            var interactCompleted = WaitForBoolInteractionToComplete(homePage, script, (bool?)null);

            // Clean up to avoid failures testing in pipeline
            var windoClosed = CloseViewAndCleanup(View);

            // Assert
            Assert.IsTrue(interactCompleted, "The ShowSampleFilesFolderCommand script did not run as expected.");
            Assert.IsTrue(wasTestHookInvoked, "The ShowSampleFilesFolderCommand method did not invoke the test hook as expected.");
            Assert.IsTrue(windoClosed, "Dynamo View was not closed correctly.");
        }
        #endregion

        #region helpers

        /// <summary>
        /// A helper method to (async) await the initialization of a WebView2 component
        /// </summary>
        /// <param name="web">The WebView2 component to await</param>
        /// <exception cref="TimeoutException"></exception>
        internal static void InitializeWebView2(WebView2 web)
        {
            var navigationCompletedEvent = new ManualResetEvent(false);
            DateTime startTime = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(50); 

            void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                web.NavigationCompleted -= WebView_NavigationCompleted;
                navigationCompletedEvent.Set(); // Signal that navigation has completed
            }

            web.NavigationCompleted += WebView_NavigationCompleted;

            // Wait until we have initialized dynWebView or until the timeout is reached
            while (!navigationCompletedEvent.WaitOne(100))
            {
                if (DateTime.Now - startTime > timeout)
                {
                    throw new TimeoutException("WebView2 initialization timed out.");
                }
                DispatcherUtil.DoEvents();
            }
        }

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

        /// <summary>
        /// A helper method to synchronously await async method relying on return value change
        /// It is vital that the script has a return value which is always different than the initial value to avoid infinite loop
        /// </summary>
        /// <param name="homePage">The HomePage instance</param>
        /// <param name="script">A script expecting a bool? return value different than the initial value</param>
        /// <param name="initialValue">The initial value to compare with (different than the return value)</param>
        /// <returns></returns>
        private static bool? WaitForBoolInteractionToComplete(HomePage homePage, string script, bool? initialValue)
        {
            // Invoke the custom frontend script command we want to assert the funcionality of
            var interactCompleted = initialValue;
            homePage.Dispatcher.Invoke(async () =>
            {
                interactCompleted = await Interact<bool>(homePage.dynWebView, script);
            });

            // Wait for the interaction to complete
            while (EqualityComparer<bool?>.Default.Equals(interactCompleted, initialValue))
            {
                DispatcherUtil.DoEvents();
            }

            return interactCompleted;
        }

        /// <summary>
        /// A helper method to synchronously await async method relying on return value change
        /// It is vital that the script has a return value which is always different than the initial value to avoid infinite loop
        /// </summary>
        /// <param name="homePage">The HomePage instance</param>
        /// <param name="script">A script expecting an int return value different than the initial value</param>
        /// <param name="initialValue">The initial value to compare with (different than the return value)</param>
        /// <returns></returns>
        private static int WaitForIntInteractionToComplete(HomePage homePage, string script, int initialValue)
        {
            // Invoke the custom frontend script command we want to assert the funcionality of
            var interactCompleted = initialValue;
            homePage.Dispatcher.Invoke(async () =>
            {
                interactCompleted = await Interact<int>(homePage.dynWebView, script);
            });

            // Wait for the interaction to complete
            while (EqualityComparer<int>.Default.Equals(interactCompleted, initialValue))
            {
                DispatcherUtil.DoEvents();
            }

            return interactCompleted;
        }

        /// <summary>
        /// A helper async method to await and return the result of a script execution
        /// </summary>
        /// <typeparam name="T">The return type from the script</typeparam>
        /// <param name="web">The WebView2 control</param>
        /// <param name="script">A javascript script to be executed on the front end</param>
        /// <returns></returns>
        internal static async Task<T> Interact<T>(WebView2 web, string script)
        {
            try
            {
                var scriptTask = web.CoreWebView2.ExecuteScriptAsync(script);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(20));
                var completedTask = await Task.WhenAny(scriptTask, timeoutTask).ConfigureAwait(false); 

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Script execution timed out.");
                }


                string resultJson = await scriptTask; // Result is always returned as JSON string from ExecuteScriptAsync
                Task.Delay(200).Wait(); // Allow homepage class to catch up
                return DeserializeResult<T>(resultJson);
            }
            catch (Exception)
            {
                // Return default value for T in case of error
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
