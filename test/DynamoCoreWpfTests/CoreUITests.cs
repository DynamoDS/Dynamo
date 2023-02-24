using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CoreNodeModels.Input;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Extensions;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using DynamoCoreWpfTests.Utility;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SystemTestServices;
using WpfResources = Dynamo.Wpf.Properties.Resources;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class CoreUserInterfaceTests : SystemTestBase
    {
        #region SaveImageCommand

        [Test]
        [Category("DynamoUI")]
        public void CanSaveImage()
        {
            // Save image command now requires the workspace to be not empty.
            var testPath = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testPath, @"core\nodeLocationTest.dyn");

            OpenDynamoDefinition(openPath);
            DispatcherUtil.DoEvents(); // Allows visual tree to be reconstructed.

            string path = Path.Combine(TempFolder, "output.png");
            ViewModel.SaveImageCommand.Execute(path);

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }

        [Test]
        [Category("DynamoUI")]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            ViewModel.SaveImageCommand.Execute(path);

            Assert.False(File.Exists(path));
        }

        #endregion

        #region ToggleConsoleShowingCommand

        [Test]
        [Category("DynamoUI")]
        public void CanHideConsoleWhenShown()
        {
            ViewModel.ToggleConsoleShowingCommand.Execute(null);
            Assert.True(ViewModel.ConsoleHeight > 0);
        }

        [Test]
        [Category("DynamoUI")]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(ViewModel.ConsoleHeight > 0);
        }

        [Test]
        [Category("DynamoUI")]
        public void CanShowConsoleWhenHidden()
        {
            ViewModel.ToggleConsoleShowingCommand.Execute(null);
            Assert.True(ViewModel.ConsoleHeight > 0);

            ViewModel.ToggleConsoleShowingCommand.Execute(null);
            Assert.False(ViewModel.ConsoleHeight > 0);
        }

        #endregion

        #region Zoom In and Out canvas

        [Test]
        [Category("DynamoUI")]
        public void CanZoom()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;
            double zoom;
            
            // Test Zoom in
            zoom = workspaceVM.Zoom;
            if ( ViewModel.ZoomInCommand.CanExecute(null) )
                ViewModel.ZoomInCommand.Execute(null);
            Assert.Greater(workspaceVM.Zoom, zoom);

            // Test Zoom out
            zoom = workspaceVM.Zoom;
            if (ViewModel.ZoomOutCommand.CanExecute(null))
                ViewModel.ZoomOutCommand.Execute(null);
            Assert.Greater(zoom, workspaceVM.Zoom);

            // Test can set zoom (at random zoom for 10 times)
            int testLoop = 10;
            for (int i = 0; i < testLoop; i++)
            {
                // Get random number for the zoom
                double upperBound = WorkspaceViewModel.ZOOM_MAXIMUM;
                double lowerBound = WorkspaceViewModel.ZOOM_MINIMUM;
                Random random = new Random();
                double randomNumber = random.NextDouble() * (upperBound - lowerBound) + lowerBound;

                if (ViewModel.CurrentSpaceViewModel.SetZoomCommand.CanExecute(randomNumber))
                    ViewModel.CurrentSpaceViewModel.SetZoomCommand.Execute(randomNumber);

                // Check Zoom is correct
                Assert.AreEqual(randomNumber, workspaceVM.Zoom);
            }

            // Border Test for Set Zoom
            // Min zoom
            zoom = WorkspaceViewModel.ZOOM_MINIMUM;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreEqual(zoom, workspaceVM.Zoom);
            // Zoom out over limit (check that it does not zoom out)
            if (ViewModel.ZoomOutCommand.CanExecute(null))
                ViewModel.ZoomOutCommand.Execute(null);
            Assert.AreEqual(zoom, workspaceVM.Zoom);
            
            // Max zoom
            zoom = WorkspaceViewModel.ZOOM_MAXIMUM;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreEqual(zoom, workspaceVM.Zoom);
            // Zoom in over limit (check that it does not zoom in)
            if (ViewModel.ZoomInCommand.CanExecute(null))
                ViewModel.ZoomInCommand.Execute(null);
            Assert.AreEqual(zoom, workspaceVM.Zoom);

            // Above Max Limit Test
            zoom = WorkspaceViewModel.ZOOM_MAXIMUM + 0.1;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreNotEqual(zoom, workspaceVM.Zoom);

            // Below Min Limit Test
            zoom = WorkspaceViewModel.ZOOM_MINIMUM - 0.1;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreNotEqual(zoom, workspaceVM.Zoom);

            // Stress Test
            // Zoom in and out repeatly
            for (int i = 0; i < 20; i++)
            {
                for (int stepIn = 0; stepIn < 20; stepIn++)
                {
                    if (ViewModel.ZoomInCommand.CanExecute(null))
                        ViewModel.ZoomInCommand.Execute(null);
                }
                for (int stepOut = 0; stepOut < 20; stepOut++)
                {
                    if (ViewModel.ZoomOutCommand.CanExecute(null))
                        ViewModel.ZoomOutCommand.Execute(null);
                }
            }
            // Doesn't crash the system
            Assert.True(true);
        }

        #endregion

        #region Pan Left, Right, Top, Down Canvas

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanLeft()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceVM.X;
            double posY = workspaceVM.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Left"))
                    ViewModel.PanCommand.Execute("Left");
            }

            Assert.Greater(workspaceVM.X, posX);
            Assert.AreEqual(workspaceVM.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanRight()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceVM.X;
            double posY = workspaceVM.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Right"))
                    ViewModel.PanCommand.Execute("Right");
            }

            Assert.Greater(posX, workspaceVM.X);
            Assert.AreEqual(workspaceVM.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanUp()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceVM.X;
            double posY = workspaceVM.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Up"))
                    ViewModel.PanCommand.Execute("Up");
            }

            Assert.AreEqual(posX, workspaceVM.X);
            Assert.Greater(workspaceVM.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanDown()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceVM.X;
            double posY = workspaceVM.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Down"))
                    ViewModel.PanCommand.Execute("Down");
            }

            Assert.AreEqual(posX, workspaceVM.X);
            Assert.Greater(posY, workspaceVM.Y);
        }

        #endregion

        #region Fit to View

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void FitViewWithNoNodes()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            double initZoom = workspaceVM.Zoom;
            double initX = workspaceVM.X;
            double initY = workspaceVM.Y;

            // Zoom to max zoom value
            workspaceVM.FitViewInternal();
            
            // Check for no changes
            Assert.AreEqual(workspaceVM.Zoom, initZoom);
            Assert.AreEqual(workspaceVM.X, initX);
            Assert.AreEqual(workspaceVM.Y, initY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitView()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            double initZoom = workspaceVM.Zoom;
            double initX = workspaceVM.X;
            double initY = workspaceVM.Y;

            CreateNodeOnCurrentWorkspace();

            // Zoom to max zoom value
            workspaceVM.FitViewInternal();

            // Check for no changes
            Assert.AreNotEqual(workspaceVM.Zoom, initZoom);
            Assert.AreNotEqual(workspaceVM.X, initX);
            Assert.AreNotEqual(workspaceVM.Y, initY);

            ViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewTwiceForActualZoom()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            double initZoom = workspaceVM.Zoom;
            double initX = workspaceVM.X;
            double initY = workspaceVM.Y;

            CreateNodeOnCurrentWorkspace();

            // Zoom to max zoom value
            workspaceVM.FitViewInternal();

            // Check for no changes
            Assert.AreNotEqual(workspaceVM.Zoom, initZoom);
            Assert.AreNotEqual(workspaceVM.X, initX);
            Assert.AreNotEqual(workspaceVM.Y, initY);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void FitViewStressTest()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;
            
            double initZoom = workspaceVM.Zoom;
            double initX = workspaceVM.X;
            double initY = workspaceVM.Y;

            CreateNodeOnCurrentWorkspace();

            for (int i = 0; i < 100; i++)
            {
                // Zoom to max zoom value
                workspaceVM.FitViewInternal();
            }

            // Not crashed
            Assert.True(true);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewResetByZoom()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            workspaceVM.FitViewInternal();

            double curZoom = workspaceVM.Zoom;
            double curX = workspaceVM.X;
            double curY = workspaceVM.Y;

            // Do some zoom action before FitView again
            ViewModel.ZoomIn(null);

            workspaceVM.FitViewInternal();

            // Check actual zoom
            Assert.AreEqual(workspaceVM.Zoom, curZoom);
            Assert.AreEqual(workspaceVM.X, curX);
            Assert.AreEqual(workspaceVM.Y, curY);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewResetByPan()
        {
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            workspaceVM.FitViewInternal();

            double curZoom = workspaceVM.Zoom;
            double curX = workspaceVM.X;
            double curY = workspaceVM.Y;

            // Do some pan action before FitView again
            ViewModel.Pan("Up" as object);

            workspaceVM.FitViewInternal();

            // Check actual zoom
            Assert.AreEqual(workspaceVM.Zoom, curZoom);
            Assert.AreEqual(workspaceVM.X, curX);
            Assert.AreEqual(workspaceVM.Y, curY);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        // Add a number node on workspace
        private void CreateNodeOnCurrentWorkspace()
        {
            // Create number node
            var numNode = new DoubleInput { X = 100, Y = 100 };
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(numNode, true);
        }

        #endregion

        [Test,RequiresSTA]
        [Category("DynamoUI")]
        public void PreferenceSetting_BackgroundPreview_1_0API()
        {
            bool expectedValue = !ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive;
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);
          

            expectedValue = !ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive;
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);

            #region Save And Load of PreferenceSettings

            // Test if variable can be serialize and deserialize without any issue
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            // Force inital state
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.IsBackgroundPreviewActive = true;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.IsBackgroundPreviewActive, initalSetting.IsBackgroundPreviewActive);
            #endregion

            #region Second Test
            initalSetting.IsBackgroundPreviewActive = false;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.IsBackgroundPreviewActive, initalSetting.IsBackgroundPreviewActive);
            #endregion
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void PreferenceSetting_RenderPrecision()
        {
            // Test that RenderPrecision setting works as expected
            ViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions = 256;
            Assert.AreEqual(256, ViewModel.Model.PreferenceSettings.RenderPrecision);

            ViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions = 128;
            Assert.AreEqual(128, ViewModel.Model.PreferenceSettings.RenderPrecision);

            // Test serialization of RenderPrecision 
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");
            
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.RenderPrecision = 256;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.RenderPrecision, initalSetting.RenderPrecision);

            // Test loading old settings file without render precision attribute
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"settings\DynamoSettings-WithoutRenderPrecision.xml");
            PreferenceSettings WithoutRenderPrecision = PreferenceSettings.Load(filePath);
            Assert.AreEqual(WithoutRenderPrecision.RenderPrecision, 128);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void PreferenceSetting_GroupStyles()
        {
            // Test that the group style list is being initialized with a non-empty list                       
            Assert.NotNull(ViewModel.PreferenceSettings.GroupStyleItemsList);

            //Now by default we will have always 4 GroupStyles added by Dynamo
            Assert.AreEqual(4, ViewModel.PreferenceSettings.GroupStyleItemsList.Count);

            // Test serialization of GroupStyles 
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.GroupStyleItemsList.Add(new GroupStyleItem { 
                HexColorString = "000000",
                Name = "GroupName"
            });

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            // Test if the customized group styles can be loaded
            Assert.AreEqual(1, initalSetting.GroupStyleItemsList.Count);
            Assert.AreEqual(resultSetting.GroupStyleItemsList[0].Name, initalSetting.GroupStyleItemsList[0].Name);
            Assert.AreEqual(resultSetting.GroupStyleItemsList[0].HexColorString, initalSetting.GroupStyleItemsList[0].HexColorString);

            // Test loading the settings defined in the xml configuration file
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"settings\DynamoSettings-OneGroupStyle.xml");
            PreferenceSettings OneGroupStyle = PreferenceSettings.Load(filePath);
            Assert.AreEqual(1, OneGroupStyle.GroupStyleItemsList.Count);
            Assert.AreEqual(OneGroupStyle.GroupStyleItemsList[0].Name, initalSetting.GroupStyleItemsList[0].Name);
            Assert.AreEqual(OneGroupStyle.GroupStyleItemsList[0].HexColorString, initalSetting.GroupStyleItemsList[0].HexColorString);
        }

        [Test]
        [Category("DynamoUI")]
        public void PreferenceSetting_NotAgreeAnalyticsSharing()
        {
            // Test deserialization of analytics setting 
            // Test loading old settings file without agreement 
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"settings\DynamoSettings-firstrun.xml");
            var resultSetting = PreferenceSettings.Load(filePath);
            Assert.AreEqual(false, resultSetting.IsAnalyticsReportingApproved);
            Assert.AreEqual(false, resultSetting.IsUsageReportingApproved);
            Assert.DoesNotThrow(() => Dynamo.Logging.AnalyticsService.ShutDown());
        }

        [Test]
        [Category("DynamoUI")]
        public void PreferenceSetting_AgreeAnalyticsSharing()
        {
            // Test loading old settings file with agreement 
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"settings\DynamoSettings-AnalyticsTurnedOn.xml");
            var resultSetting = PreferenceSettings.Load(filePath);
            Assert.AreEqual(true, resultSetting.IsAnalyticsReportingApproved);
            Assert.AreEqual(false, resultSetting.IsUsageReportingApproved);
            Assert.DoesNotThrow(() => Dynamo.Logging.AnalyticsService.ShutDown());
        }

        #region PreferenceSettings
        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void PreferenceSetting()
        {
            // Test Case to ensure that the link for these persistent variable
            // between DynamoViewModel, Model is not broken or replaced.
            #region BackgroundPreviewActive

            var backgroundPreviewName = ViewModel.BackgroundPreviewViewModel.PreferenceWatchName;
            bool expectedValue = !ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName);
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));

            expectedValue = !ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName);
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));
            #endregion

            #region ConsoleHeight
            int expectedHeight = 100;
            ViewModel.ToggleConsoleShowing(null);
            Assert.AreEqual(expectedHeight, ViewModel.Model.PreferenceSettings.ConsoleHeight);

            expectedHeight = 0;
            ViewModel.ToggleConsoleShowing(null);
            Assert.AreEqual(expectedHeight, ViewModel.Model.PreferenceSettings.ConsoleHeight);
            #endregion

            #region ConnectorType
            ConnectorType expectedConnector = ConnectorType.BEZIER;
            ViewModel.SetConnectorType("BEZIER");
            Assert.AreEqual(expectedConnector, ViewModel.Model.PreferenceSettings.ConnectorType);
            // Now we expect bezier to be created regardless of the preference settings.
            expectedConnector = ConnectorType.BEZIER;
            ViewModel.SetConnectorType("POLYLINE");
            Assert.AreEqual(expectedConnector, ViewModel.Model.PreferenceSettings.ConnectorType);
            #endregion

            #region Collect Information Option
            {
                // Backup the value of Dynamo.IsTestMode and restore it later. The 
                // reason for this is 'IsUsageReportingApproved' only returns the 
                // actual value when not running in test mode.
                var isTestMode = DynamoModel.IsTestMode;

                // First time run, check if dynamo did set it back to false after running
                Assert.AreEqual(false, UsageReportingManager.Instance.FirstRun);

                // CollectionInfoOption To FALSE
                UsageReportingManager.Instance.SetUsageReportingAgreement(true);
                RestartTestSetup(startInTestMode: false);
                // Because IsUsageReportingApproved is now dominated by IsAnalyticsReportingApproved.
                // In this case. the value is still false because of IsAnalyticsReportingApproved
                Assert.AreEqual(false, UsageReportingManager.Instance.IsUsageReportingApproved);

                // CollectionInfoOption To FALSE
                UsageReportingManager.Instance.SetUsageReportingAgreement(false);
                RestartTestSetup(startInTestMode: false);
                Assert.AreEqual(false, UsageReportingManager.Instance.IsUsageReportingApproved);

                DynamoModel.IsTestMode = isTestMode; // Restore the orignal value.
            }
            #endregion

            #region Save And Load of PreferenceSettings
            // Test if variable can be serialize and deserialize without any issue
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            // Force inital state
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            #region First Test

            initalSetting.ConnectorType = ConnectorType.BEZIER;
            initalSetting.ConsoleHeight = 100;
            initalSetting.SetIsBackgroundPreviewActive(backgroundPreviewName, true);

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.GetIsBackgroundPreviewActive(backgroundPreviewName),
                initalSetting.GetIsBackgroundPreviewActive(backgroundPreviewName));
            Assert.AreEqual(resultSetting.ConnectorType, initalSetting.ConnectorType);
            Assert.AreEqual(resultSetting.ConsoleHeight, initalSetting.ConsoleHeight);
            #endregion

            #region Second Test
            initalSetting.ConnectorType = ConnectorType.POLYLINE;
            initalSetting.ConsoleHeight = 0;
            initalSetting.SetIsBackgroundPreviewActive(backgroundPreviewName, false);

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.GetIsBackgroundPreviewActive(backgroundPreviewName),
                initalSetting.GetIsBackgroundPreviewActive(backgroundPreviewName));
            Assert.AreEqual(resultSetting.ConnectorType, initalSetting.ConnectorType);
            Assert.AreEqual(resultSetting.ConsoleHeight, initalSetting.ConsoleHeight);

            // Now we expect bezier to be created regardless of the preference settings.
            Assert.AreEqual(Model.ConnectorType, ConnectorType.BEZIER);
            #endregion

            #endregion

            View.Close();
        }

        [Test]
        public void PreferenceSettings_ShowEdges_DefaultFalse()
        {
            var settings = new PreferenceSettings();
            Assert.False(settings.ShowEdges);
        }

        [Test]
        public void PreferenceSettings_ShowEdges_Toggle()
        {
            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.False(Model.PreferenceSettings.ShowEdges);

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = true;
            Assert.True(Model.PreferenceSettings.ShowEdges);
        }

        [Test]
        public void PreferenceSettings_ShowEdges_Save()
        {
            // Test if variable can be serialize and deserialize without any issue
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            var initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.ShowEdges = true;
            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);
            Assert.True(resultSetting.ShowEdges);
        }

        [Test]
        public void PreferenceSettings_IsFirstRun_And_HideReportOptions()
        {
            ViewModel.HideReportOptions = true;
            ViewModel.Model.PreferenceSettings.IsFirstRun = true;
            //force the dynamoview's loaded handler to be called again - 
            View.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            Assert.IsFalse(ViewModel.PreferenceSettings.IsFirstRun);
        }

        [Test]
        public void PreferenceSettingsConnectorTypeRevertsToBezier()
        {
            // Arrange
            var preferences = new PreferenceSettings()
            {
                ConnectorType = ConnectorType.POLYLINE
            };

            var config = new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = preferences,
            };

            // Act
            RestartTestSetupWithNewSettings(config, true);

            // Assert
            // Check that prefferenceSettings are set to ConnectorType.POLYLINE
            // but the Models connector type is BEZIER
            Assert.That(Model.PreferenceSettings.ConnectorType == ConnectorType.POLYLINE);
            Assert.That(Model.ConnectorType == ConnectorType.BEZIER);
        }

        private void RestartTestSetup(bool startInTestMode)
        {
            // Shutdown Dynamo and restart it
            View.Close();
            View = null;

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false, allowCancellation: false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            Model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = startInTestMode,
                    ProcessMode = startInTestMode 
                        ? TaskProcessMode.Synchronous 
                        : TaskProcessMode.Asynchronous
                });

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model
                });

            var expectedState = startInTestMode
                ? DynamoModel.DynamoModelState.StartedUIless
                : DynamoModel.DynamoModelState.StartedUI;
            Assert.AreEqual(ViewModel.Model.State, expectedState);

            //create the view
            View = new DynamoView(ViewModel);
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        private void RestartTestSetupWithNewSettings(Dynamo.Models.DynamoModel.IStartConfiguration configuration, bool startInTestMode)
        {
            // Shutdown Dynamo and restart it
            View.Close();
            View = null;

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false, allowCancellation: false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            Model = DynamoModel.Start(configuration);

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model
                });

            var expectedState = startInTestMode
                ? DynamoModel.DynamoModelState.StartedUIless
                : DynamoModel.DynamoModelState.StartedUI;
            Assert.AreEqual(ViewModel.Model.State, expectedState);

            //create the view
            View = new DynamoView(ViewModel);
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        #endregion

        #region InfoBubble

        [Test]
        [Category("DynamoUI")]
        public void UpdateInfoBubble_ErrorBubble()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel(this.ViewModel);
            string content = "This is the test infoBubble";
            InfoBubbleDataPacket inputData_ErrorBubble = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Error,
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Bottom);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_ErrorBubble);
                Assert.AreEqual(content, infoBubble.Content);
                Assert.AreEqual(InfoBubbleViewModel.Style.Error, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Bottom, infoBubble.ConnectingDirection);
                Assert.IsNull(infoBubble.DocumentationLink);
            }
        }

        [Test, Ignore]
        // Opacity is no longer affecting the visibility of infobubble. This requires opacity of UIElement
        // This test is no longer feasible. Keeping it for future reference
        [Category("DynamoUI")]
        public void Collapse()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel(this.ViewModel);
            infoBubble.OnRequestAction(new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Hide));
            //Assert.AreEqual(0, infoBubble.Opacity);
        }

        #endregion

        #region Notes

        [Test]
        [Category("DynamoUI")]
        public void CanCreateANote()
        {
            ViewModel.AddNoteCommand.Execute(null);
            var note = Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);
            
            //verify the note was created
            Assert.AreEqual(1, Model.CurrentWorkspace.Notes.Count());

            ViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test]
        [Category("DynamoUI")]
        public void CanDeleteANote()
        {
            ViewModel.AddNoteCommand.Execute(null);
            var note = Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, Model.CurrentWorkspace.Notes.Count());

            //select the note for deletion
            DynamoSelection.Instance.Selection.Add(note);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            //delete the note
            ViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0,Model.CurrentWorkspace.Notes.Count());

            ViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
           
        }


        #endregion

        [Test]
        [Category("UnitTests")]
        public void TestDraggedNode()
        {
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+")) { X = 16, Y = 32 };
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            NodeModel locatable = ViewModel.Model.CurrentWorkspace.Nodes.First();

            var startPoint = new Point2D(8, 64);
            var dn = new WorkspaceViewModel.DraggedNode(locatable, startPoint);

            // Initial node position.
            Assert.AreEqual(16, locatable.X);
            Assert.AreEqual(32, locatable.Y);

            // Move the mouse cursor to move node.
            dn.Update(new Point2D(-16, 72));
            Assert.AreEqual(-8, locatable.X);
            Assert.AreEqual(40, locatable.Y);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfOpenOnRightClick()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsNotNull(currentWs, "DynamoView does not have any WorkspaceView");
            RightClick(currentWs.zoomBorder);
            DispatcherUtil.DoEvents();
            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfNotOpenOnNodeRightClick()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsNotNull(currentWs, "DynamoView does not have any WorkspaceView");
            CreateNodeOnCurrentWorkspace();

            DispatcherUtil.DoEvents();
            var node = currentWs.ChildOfType<NodeView>();
            RightClick(node);
            DispatcherUtil.DoEvents();
            // workspace context menu shouldn't be open
            Assert.IsFalse(currentWs.ContextMenuPopup.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfInCanvasSearchHidesOnOpeningContextMenu()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();

            // show in-canvas search
            ViewModel.CurrentSpaceViewModel.ShowInCanvasSearchCommand.Execute(ShowHideFlags.Show);
            DispatcherUtil.DoEvents();
            Assert.IsTrue(currentWs.InCanvasSearchBar.IsOpen);

            // open context menu
            RightClick(currentWs.zoomBorder);
            DispatcherUtil.DoEvents();

            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);
            Assert.IsFalse(currentWs.InCanvasSearchBar.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void InCanvasSearchTextChangeTriggersOneSearchCommand()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();

            // open context menu
            RightClick(currentWs.zoomBorder);

            // show in-canvas search
            ViewModel.CurrentSpaceViewModel.ShowInCanvasSearchCommand.Execute(ShowHideFlags.Show);

            var searchControl = currentWs.ChildrenOfType<Popup>().Select(x => (x as Popup)?.Child as InCanvasSearchControl).Where(c => c != null).FirstOrDefault();
            Assert.IsNotNull(searchControl);

            DispatcherUtil.DoEvents();

            int count = 0;
            (searchControl.DataContext as SearchViewModel).SearchCommand = new Dynamo.UI.Commands.DelegateCommand((object _) => { count++; });
            searchControl.SearchTextBox.Text = "dsfdf";
            DispatcherUtil.DoEvents();

            Assert.IsTrue(currentWs.InCanvasSearchBar.IsOpen);
            Assert.AreEqual(count, 1);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfSearchTextClearsOnOpeningContextMenu()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();

            // open context menu
            RightClick(currentWs.zoomBorder);
            DispatcherUtil.DoEvents();
            // set dummy content for search text
            currentWs.ViewModel.InCanvasSearchViewModel.SearchText = "dummy";
            DispatcherUtil.DoEvents();
            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);
            Assert.IsFalse(currentWs.InCanvasSearchBar.IsOpen);

            // show in-canvas search
            ViewModel.CurrentSpaceViewModel.ShowInCanvasSearchCommand.Execute(ShowHideFlags.Show);
            DispatcherUtil.DoEvents();
            Assert.IsTrue(currentWs.InCanvasSearchBar.IsOpen);

            // check if search text is still empty
            Assert.IsTrue(currentWs.ViewModel.InCanvasSearchViewModel.SearchText.Equals(string.Empty));
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_IfSubmenuOpenOnMouseHover()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();
            RightClick(currentWs.zoomBorder);
            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);

            currentWs.WorkspaceLacingMenu.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.MouseEnterEvent
            });

            DispatcherUtil.DoEvents();

            Assert.IsTrue(currentWs.WorkspaceLacingMenu.IsSubmenuOpen);
        }


        [Test]
        public void WarningShowsWhenSavingWithLinterWarningsOrErrors()
        {
            // Arrange
            var expectedWindowTitle = WpfResources.GraphIssuesOnSave_Title;
            var recivedEvents = new List<string>();
            var savewarnHandler = new Action<SaveWarningOnUnresolvedIssuesArgs>((e) => { recivedEvents.Add(e.TaskDialog.Title); e.TaskDialog.Close(); });
            ViewModel.SaveWarningOnUnresolvedIssuesShows += savewarnHandler;
            
    
            Mock<LinterExtensionBase> mockLinter = new Mock<LinterExtensionBase>() { CallBase = true };
            SetupMockLinter(mockLinter);

            var startupParams = new StartupParams(Model);

            mockLinter.Object.InitializeBase(Model.LinterManager);
            mockLinter.Object.Startup(startupParams);
            Model.ExtensionManager.Add(mockLinter.Object);

            OpenDynamoDefinition(Path.Combine(GetTestDirectory(ExecutingDirectory), @"UI/SaveWithIssuesWarning.dyn"));

            var failureNode = new DummyNode();
            Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(failureNode, 0, 0, false, false));

            Model.LinterManager.SetActiveLinter(Model.LinterManager.AvailableLinters
                .Where(x => x.Id == mockLinter.Object.UniqueId)
                .FirstOrDefault());

            // Act
            failureNode.Name = "NewNodeName";
            Assert.That(Model.LinterManager.RuleEvaluationResults.Count > 0);

            ViewModel.ShowSaveDialogIfNeededAndSaveResult(null);

            // Assert
            Assert.That(recivedEvents.Count == 1);
            Assert.That(recivedEvents.First() == expectedWindowTitle);
            ViewModel.SaveWarningOnUnresolvedIssuesShows -= savewarnHandler;
            
        }

   

        private void SetupMockLinter(Mock<LinterExtensionBase> mockLinter)
        {
            const string MOCK_LINTER_GUID = "358321af-2633-4697-b475-81632582eba0";
            const string MOCK_LINTER_NAME = "MockLinter";
            const string MOCK_RULE_ID = "1";

            Mock<NodeLinterRule> mockRule = new Mock<NodeLinterRule> { CallBase = true };

            // Setup mock rule
            mockRule.Setup(r => r.Id).Returns(MOCK_RULE_ID);

            // Setup mock LinterExtension
            mockLinter.Setup(e => e.UniqueId).Returns(MOCK_LINTER_GUID);
            mockLinter.Setup(e => e.Name).Returns(MOCK_LINTER_NAME);

            mockRule.Setup(x => x.EvaluationTriggerEvents).
                Returns(new List<string> { nameof(NodeModel.Name) });

            mockRule.Protected().Setup<RuleEvaluationStatusEnum>("EvaluateFunction", ItExpr.IsAny<NodeModel>(), ItExpr.IsAny<string>()).
                Returns((NodeModel node, string changedEvent) => NodeRuleEvaluateFunction(node, changedEvent));

            mockRule.Protected().Setup<List<Tuple<RuleEvaluationStatusEnum, string>>>("InitFunction", ItExpr.IsAny<WorkspaceModel>()).
                Returns((WorkspaceModel wm) => NodeRuleInitFuction(wm));

            mockLinter.Object.AddLinterRule(mockRule.Object);

        }

        private RuleEvaluationStatusEnum NodeRuleEvaluateFunction(NodeModel node, string changedEvent)
        {
            return node.Name == "NewNodeName" ?
                RuleEvaluationStatusEnum.Failed :
                RuleEvaluationStatusEnum.Passed;
        }

        private List<Tuple<RuleEvaluationStatusEnum, string>> NodeRuleInitFuction(WorkspaceModel wm)
        {
            var results = new List<Tuple<RuleEvaluationStatusEnum, string>>();
            foreach (var node in wm.Nodes)
            {
                var evaluationStatus = NodeRuleEvaluateFunction(node, "init");
                if (evaluationStatus == RuleEvaluationStatusEnum.Passed)
                    continue;

                var valueTuple = Tuple.Create(evaluationStatus, node.GUID.ToString());
                results.Add(valueTuple);
            }
            return results;
        }

        private void RightClick(IInputElement element)
        {
            element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseDownEvent
            });

            element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent
            });

            DispatcherUtil.DoEvents();

            //Wait 3 seconds until the Click Right context menu is opened
            Task.WaitAll(new Task[] { Task.Delay(2000) });
        }


    }
}
