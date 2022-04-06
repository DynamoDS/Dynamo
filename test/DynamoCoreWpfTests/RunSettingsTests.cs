﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Wpf.ViewModels;
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
        [Test, Ignore]
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
