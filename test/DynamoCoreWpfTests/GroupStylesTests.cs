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

        [Test]
        public void CustomColorPicker_PrePopulateDefaultColors()
        {
            Open(@"UI\GroupTest.dyn");
            var tabName = "Visual Settings";
            var expanderName = "Group Styles";

            var preferencesSettings = (View.DataContext as DynamoViewModel).PreferenceSettings;

            //Creates the Preferences dialog 
            var preferencesWindow = new PreferencesView(View);
            
            //Validate the 4 default existing styles list
            Assert.AreEqual(preferencesSettings.GroupStyleItemsList.Count, 4);

            //Adds two Custom Group Styles to the PreferencesView
            preferencesSettings.GroupStyleItemsList.Add(new GroupStyleItem { Name = "Custom 1", HexColorString = "FFFF00", IsDefault = false });
            preferencesSettings.GroupStyleItemsList.Add(new GroupStyleItem { Name = "Custom 2", HexColorString = "FF00FF", IsDefault = false });

            //Finds the Visual Settings tab and open it
            var tabControl = preferencesWindow.preferencesTabControl;
            if (tabControl == null) return;
            var preferencesTab = (from TabItem tabItem in tabControl.Items
                                    where tabItem.Header.ToString().Equals(tabName)
                                    select tabItem).FirstOrDefault();
            if (preferencesTab == null) return;
            tabControl.SelectedItem = preferencesTab;

            //Finds the Group Styles section and open it 
            var listExpanders = WpfUtilities.ChildrenOfType<Expander>(preferencesTab.Content as ScrollViewer);
            var tabExpander = (from expander in listExpanders
                                where expander.Header.ToString().Equals(expanderName)
                                select expander).FirstOrDefault();
            if (tabExpander == null) return;
            tabExpander.IsExpanded = true;
                   
                 
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            //Click the AddStyle button
            preferencesWindow.AddStyleButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //The custom Colors list used by ColorPicker is empty
            Assert.AreEqual(preferencesWindow.stylesCustomColors.Count, 0);

            //Clicks the color button, this will populate the custom Colors list with the style colors added in Group Styles
            preferencesWindow.buttonColorPicker.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Validates that the 3 added custom colors are present in the custom Colors list
            Assert.AreEqual(preferencesWindow.stylesCustomColors.Count, 2);
        }
    }
}
