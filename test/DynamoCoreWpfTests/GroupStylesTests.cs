using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Configuration;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class GroupStylesTests : DynamoTestUIBase
    {
        public AnnotationView NodeViewWithGuid(string guid)
        {
            var annotationView =
                View.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().ChildrenOfType<AnnotationView>();
            var annotationViewOfType = annotationView.Where(x => x.ViewModel.AnnotationModel.GUID.ToString() == guid);
            Assert.AreEqual(1, annotationViewOfType.Count(), "Expected a single Annotation View with guid: " + guid);

            return annotationViewOfType.First();
        }

        public override void Open(string path)
        {
            base.Open(path);
            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// Validates that the GroupStyles in the PreferencesView match the ones in the AnnotationView
        /// </summary>
        [Test]
        public void TestDefaultGroupStyles_PreferencesView()
        {
            int defaultGroupStylesCounter = 4;
            Open(@"UI\GroupTest.dyn");

            //Creates the Preferences dialog and the ScaleFactor = 2 ( Medium)
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            var prefViewModel = preferencesWindow.DataContext as PreferencesViewModel;

            var annotationView = NodeViewWithGuid("a432d63f-7a36-45ad-b30a-7924beb20e90");

            var annotationViewModel = annotationView.DataContext as AnnotationViewModel;

            //Close the Preferences Dialog
            preferencesWindow.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Check that the GroupStyles in the AnnotationView match the ones in the PreferencesView (default ones)
            Assert.AreEqual(annotationViewModel.GroupStyleList.OfType<GroupStyleItem>().Count(), prefViewModel.StyleItemsList.Count);

        }

        /// <summary>
        /// Add a new GroupStyle and validates that the GroupStyles in the PreferencesView match the ones in the AnnotationView
        /// </summary>
        [Test]
        public void TestAddGroupStyle_ContextMenu()
        {
            int currentGroupStylesCounter = 5;
            Open(@"UI\GroupTest.dyn");

            var preferencesSettings = (View.DataContext as DynamoViewModel).PreferenceSettings;

            //Creates the Preferences dialog and the ScaleFactor = 2 ( Medium)
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            var prefViewModel = preferencesWindow.DataContext as PreferencesViewModel;

            var annotationView = NodeViewWithGuid("a432d63f-7a36-45ad-b30a-7924beb20e90");

            var annotationViewModel = annotationView.DataContext as AnnotationViewModel;

            //Check that the GroupStyles in the AnnotationView match the ones in the PreferencesView
            Assert.AreEqual(annotationViewModel.GroupStyleList.OfType<GroupStyleItem>().Count(), prefViewModel.StyleItemsList.Count);

            //Add one Custom Group Style to the PreferencesView
            preferencesSettings.GroupStyleItemsList.Add(new Dynamo.Configuration.GroupStyleItem { Name = "Custom 1", HexColorString = "FFFF00", IsDefault = false });
            
            //Close the Preferences Dialog 
            preferencesWindow.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            var groupStylesMenuItem = annotationView.AnnotationContextMenu
              .Items
              .OfType<MenuItem>()
              .First(x => x.Header.ToString() == "Group Style");

            groupStylesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            DispatcherUtil.DoEvents();

            //Check that the GroupStyles in the AnnotationView match the ones in the PreferencesView
            Assert.AreEqual(annotationViewModel.GroupStyleList.OfType<GroupStyleItem>().Count(), currentGroupStylesCounter);

        }
    }
}
