using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class RunSettingsTests : SystemTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void RunButtonDisabledInAutomaticRun()
        {
            var homeSpace = GetHomeSpace();
            homeSpace.RunSettings.RunType = RunType.Automatic;
            Assert.False((View.RunSettingsControl.RunButton.IsEnabled));
        }

        [Test]
        public void RunButtonDisabledInPeriodicRun()
        {
            var homeSpace = GetHomeSpace();
            var node = new DoubleInput();
            Model.AddNodeToCurrentWorkspace(node, true);
            node.CanUpdatePeriodically = true;
            homeSpace.RunSettings.RunType = RunType.Periodic;
            Assert.False((View.RunSettingsControl.RunButton.IsEnabled));
        }

        [Test]
        public void RunButtonEnabledInManualRun()
        {
            var homeSpace = GetHomeSpace();
            homeSpace.RunSettings.RunType = RunType.Manual;
            Assert.True((View.RunSettingsControl.RunButton.IsEnabled));
        }

        [Test]
        public void PeriodicDisabledWithoutPeriodicNodes()
        {
            var item = View.RunSettingsControl.RunTypesComboBox.Items[2] as RunTypeItem;
            Assert.False(item.Enabled);
        }

        [Test]
        public void PeriodicEnabledWithPeriodicNodes()
        {
            var node = new DoubleInput { CanUpdatePeriodically = true };

            var homeSpace = GetHomeSpace();
            homeSpace.AddAndRegisterNode(node, true);
            
            var item = View.RunSettingsControl.RunTypesComboBox.Items[2] as RunTypeItem;
            Assert.True(item.Enabled);
        }

        [Test]
        public void RunPeriodResetsWithGarbageInput()
        {
            var homeSpace = GetHomeSpace();
            homeSpace.RunSettings.RunPeriod = 1;
            var tb = View.RunSettingsControl.RunPeriodTextBox;
            View.RunSettingsControl.RunPeriodTextBox.Text = "dingbat";
            tb.RaiseEvent(GetKeyboardEnterEventArgs(tb));
            Assert.AreEqual(GetHomeSpace().RunSettings.RunPeriod, RunSettings.DefaultRunPeriod);
        }

        [Test]
        public void RunPeriodAcceptsTextWithProperSuffix()
        {
            var tb = View.RunSettingsControl.RunPeriodTextBox;
            View.RunSettingsControl.RunPeriodTextBox.Text = "5000" + RunPeriodConverter.ExpectedSuffix;
            tb.RaiseEvent(GetKeyboardEnterEventArgs(tb));
            Assert.AreEqual(GetHomeSpace().RunSettings.RunPeriod, 5000);
        }

        [Test]
        public void RunPeriodDoesNotAllowNegativeInput()
        {
            var tb = View.RunSettingsControl.RunPeriodTextBox;
            View.RunSettingsControl.RunPeriodTextBox.Text = "-22";
            tb.RaiseEvent(GetKeyboardEnterEventArgs(tb));
            Assert.AreEqual(GetHomeSpace().RunSettings.RunPeriod, 22);
        }

        [Test]
        public void RunPeriodIsNicelyFormatted()
        {
            var tb = View.RunSettingsControl.RunPeriodTextBox;
            View.RunSettingsControl.RunPeriodTextBox.Text = "122";
            tb.RaiseEvent(GetKeyboardEnterEventArgs(tb));
            Assert.AreEqual(tb.Text, "122ms");
        }

        [Test]
        public void OnlyManualRunIsAvailableInDebug()
        {
            ViewModel.CurrentSpaceViewModel.RunSettingsViewModel.RunInDebug = true;

            var auto = View.RunSettingsControl.RunTypesComboBox.Items[1] as RunTypeItem;
            var period = View.RunSettingsControl.RunTypesComboBox.Items[2] as RunTypeItem;

            Assert.False(auto.Enabled);
            Assert.False(period.Enabled);
        }

        [Test]
        public void RunSettingsResetsOnWorkspaceClear()
        {
            var homeSpace = GetHomeSpace();
            homeSpace.RunSettings.RunType = RunType.Periodic;
            homeSpace.RunSettings.RunPeriod = 10;
            homeSpace.Clear();
            Assert.AreEqual(homeSpace.RunSettings.RunType, RunType.Automatic);
            Assert.AreEqual(homeSpace.RunSettings.RunPeriod, RunSettings.DefaultRunPeriod);
        }

        // TODO, QNTM-1100: Re-enable this test once the strategy for WorkspaceInfo has been decided on
        [Test, Ignore("Test can be re-enabled once a decision on Run settings has been made.")]
        public void RunSettingsControllerSavesAndLoads()
        {
            var homeSpace = GetHomeSpace();
            var tmpPath = Path.GetTempFileName();
            homeSpace.FileName = tmpPath;
            homeSpace.RunSettings.RunType = RunType.Periodic;
            homeSpace.RunSettings.RunPeriod = 10;
            Model.CurrentWorkspace.Save(Model.CurrentWorkspace.FileName);
            homeSpace.Clear();
            Model.OpenFileFromPath(tmpPath);
            homeSpace = GetHomeSpace();
            Assert.AreEqual(homeSpace.RunSettings.RunType, RunType.Periodic);
            Assert.AreEqual(homeSpace.RunSettings.RunPeriod, 10);
        }

        [Test]
        public void RunSettingsOnFileInsert()
        {
            var homeSpace = GetHomeSpace();
            var currentRunSettings = homeSpace.RunSettings.RunType;
            Assert.AreEqual(RunType.Automatic, currentRunSettings);

            string wspath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\callsite\RebindingSingleDimension.dyn");
            Model.InsertFileFromPath(wspath);

            currentRunSettings = homeSpace.RunSettings.RunType;
            Assert.AreEqual(RunType.Automatic, currentRunSettings);
        }

        [Test]
        public void WhenGraphEditedInManualModeAfterRunThenStatusShowsReadyToRun()
        {
            var node = AddNumberNodeAndRunAutomatically();
            GetHomeSpace().RunSettings.RunType = RunType.Manual;

            SetNumberValue(node, "2");

            Assert.AreEqual(Resources.RunReady, GetHomeSpaceViewModel().CurrentNotificationMessage);
        }

        [Test]
        public void WhenManualRunCompletesThenStatusShowsRunCompleted()
        {
            var node = AddNumberNodeAndRunAutomatically();
            GetHomeSpace().RunSettings.RunType = RunType.Manual;
            SetNumberValue(node, "2");

            RunCurrentModel();

            Assert.AreEqual(Resources.RunCompletedMessage, GetHomeSpaceViewModel().CurrentNotificationMessage);
        }

        [Test]
        public void WhenStatusRefreshedWithPendingChangesThenStatusStaysReadyToRun()
        {
            var node = AddNumberNodeAndRunAutomatically();
            GetHomeSpace().RunSettings.RunType = RunType.Manual;
            SetNumberValue(node, "2");

            // Tab switches, info bubble dismissals and file trust updates all refresh the status this way.
            GetHomeSpaceViewModel().UpdateRunStatusMsgBasedOnStates();

            Assert.AreEqual(Resources.RunReady, GetHomeSpaceViewModel().CurrentNotificationMessage);
        }

        private DoubleInput AddNumberNodeAndRunAutomatically()
        {
            var node = new DoubleInput();
            Model.AddNodeToCurrentWorkspace(node, true);
            Assert.AreEqual(Resources.RunCompletedMessage, GetHomeSpaceViewModel().CurrentNotificationMessage,
                "Automatic run did not complete.");
            return node;
        }

        private void SetNumberValue(DoubleInput node, string value)
        {
            Model.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(System.Guid.Empty, node.GUID, "Value", value));
        }

        private HomeWorkspaceViewModel GetHomeSpaceViewModel()
        {
            var homeSpaceViewModel = ViewModel.HomeSpaceViewModel as HomeWorkspaceViewModel;
            Assert.NotNull(homeSpaceViewModel);
            return homeSpaceViewModel;
        }

        private RoutedEventArgs GetKeyboardEnterEventArgs(Visual visual)
        {
            var routedEvent = Keyboard.KeyDownEvent;
            return new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(visual),
                0,
                Key.Enter) { RoutedEvent = routedEvent };
        }
        
        private HomeWorkspaceModel GetHomeSpace()
        {
            var homeSpace = Model.Workspaces.First(ws => ws is HomeWorkspaceModel) as HomeWorkspaceModel;
            Assert.NotNull(homeSpace);
            return homeSpace;
        }
    }
}
