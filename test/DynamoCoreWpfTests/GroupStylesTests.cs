using Dynamo.Configuration;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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

            // Manually create and open the group context menu (normally triggered by right-click)
            annotationView.CreateAndAttachAnnotationPopup();

            // Open context menu
            annotationView.GroupContextMenuPopup.IsOpen = true;
            DispatcherUtil.DoEvents();

            var stylesSubmenu = annotationView.GroupStyleSelectorGrid as Grid;
            Assert.IsNotNull(stylesSubmenu, "Styles sub-menu border not found.");

            var border = stylesSubmenu.Children.OfType<Border>().FirstOrDefault();
            var popup = stylesSubmenu.Children.OfType<Popup>().FirstOrDefault();

            Assert.IsNotNull(border, "Sub-menu border not found.");
            Assert.IsNotNull(popup, "Sub-menu popup not found.");

            // Trigger MouseEnter to populate the popup content
            border.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.MouseEnterEvent
            });
            DispatcherUtil.DoEvents();

            Assert.IsTrue(popup.IsOpen, "Popup did not open after MouseEnter.");
            Assert.IsInstanceOf<Border>(popup.Child, "Popup content is not a Border.");

            var wrapper = popup.Child as Border;
            var stackPanel = wrapper.Child as StackPanel;

            Assert.IsNotNull(stackPanel, "Could not find StackPanel inside popup.");            

            // Count group style options
            var innerStackCount = stackPanel.Children
                .OfType<Border>()
                .Select(b => b.Child)
                .OfType<StackPanel>()
                .Count();

            //Check that the GroupStyles in the AnnotationView match the ones in the PreferencesView
            Assert.AreEqual(innerStackCount, currentGroupStylesCounter);
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

        /// <summary>
        /// Validates that PreferencesViewModel does not recreate default styles when the list is empty
        /// </summary>
        [Test]
        public void PreferencesViewModel_DoesNotRecreateDefaultStyles_WhenListIsEmpty()
        {
            Open(@"UI\GroupTest.dyn");

            var dynamoViewModel = View.DataContext as DynamoViewModel;
            Assert.IsNotNull(dynamoViewModel);
            dynamoViewModel.PreferenceSettings.GroupStyleItemsList = new List<GroupStyleItem>();

            var preferencesViewModel = new PreferencesViewModel(dynamoViewModel);

            Assert.AreEqual(0, preferencesViewModel.StyleItemsList.Count);
            Assert.IsTrue(preferencesViewModel.CanResetGroupStyles);
        }

        /// <summary>
        /// Edits a default GroupStyle and validates that the Reset button restores all default values
        /// </summary>
        [Test]
        public void ResetStylesButton_RestoresDefaults_WhenDefaultStyleIsEdited()
        {
            Open(@"UI\GroupTest.dyn");

            //Create the Preferences dialog
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            var prefViewModel = preferencesWindow.DataContext as PreferencesViewModel;
            Assert.IsNotNull(prefViewModel);
            Assert.IsFalse(prefViewModel.CanResetGroupStyles);

            //Copy the current styles and edits one default style
            var editedStyles = prefViewModel.StyleItemsList.Select(style => new GroupStyleItem
            {
                Name = style.Name,
                HexColorString = style.HexColorString,
                FontSize = style.FontSize,
                GroupStyleId = style.GroupStyleId,
                IsDefault = style.IsDefault
            }).ToList();

            var editedDefaultStyle = editedStyles.First(style => style.IsDefault);
            editedDefaultStyle.Name = "Edited Default Style";
            editedDefaultStyle.HexColorString = "ABCDEF";
            editedDefaultStyle.FontSize = 14;

            prefViewModel.StyleItemsList = editedStyles.ToObservableCollection();
            DispatcherUtil.DoEvents();

            //Assert that reset is enabled after editing a default style
            Assert.IsTrue(prefViewModel.CanResetGroupStyles);
            Assert.AreEqual(Visibility.Visible, preferencesWindow.ResetStylesButton.Visibility);

            //Click the reset button
            preferencesWindow.ResetStylesButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Assert that all defaults are restored
            Assert.IsFalse(prefViewModel.CanResetGroupStyles);
            AssertGroupStylesMatchDefaults(prefViewModel.StyleItemsList);
        }

        /// <summary>
        /// Removes a default GroupStyle and validates that the Reset button restores the missing style
        /// </summary>
        [Test]
        public void ResetStylesButton_RestoresDefaults_WhenDefaultStyleIsDeleted()
        {
            Open(@"UI\GroupTest.dyn");

            //Create the Preferences dialog
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            var prefViewModel = preferencesWindow.DataContext as PreferencesViewModel;
            Assert.IsNotNull(prefViewModel);
            Assert.IsFalse(prefViewModel.CanResetGroupStyles);

            //Remove one default style from the list
            prefViewModel.StyleItemsList = prefViewModel.StyleItemsList.Skip(1).ToObservableCollection();
            DispatcherUtil.DoEvents();

            //Assert that reset is enabled after deleting a default style
            Assert.IsTrue(prefViewModel.CanResetGroupStyles);
            Assert.AreEqual(Visibility.Visible, preferencesWindow.ResetStylesButton.Visibility);

            //Click the reset button
            preferencesWindow.ResetStylesButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Assert that all defaults are restored
            Assert.IsFalse(prefViewModel.CanResetGroupStyles);
            AssertGroupStylesMatchDefaults(prefViewModel.StyleItemsList);
        }

        /// <summary>
        /// Validates that the Group Style submenu is disabled when there are no styles in preferences
        /// </summary>
        [Test]
        public void GroupStyleSubmenu_IsDisabledAndDoesNotOpen_WhenNoStylesExist()
        {
            Open(@"UI\GroupTest.dyn");

            var dynamoViewModel = View.DataContext as DynamoViewModel;
            Assert.IsNotNull(dynamoViewModel);
            dynamoViewModel.PreferenceSettings.GroupStyleItemsList = new List<GroupStyleItem>();

            //Remove all the styles from preferences
            var annotationView = NodeViewWithGuid("a432d63f-7a36-45ad-b30a-7924beb20e90");

            // Manually create and open the group context menu (normally triggered by right-click)
            annotationView.CreateAndAttachAnnotationPopup();
            annotationView.GroupContextMenuPopup.IsOpen = true;
            DispatcherUtil.DoEvents();

            var stylesSubmenu = annotationView.GroupStyleSelectorGrid as Grid;
            Assert.IsNotNull(stylesSubmenu, "Styles sub-menu grid not found.");

            var border = stylesSubmenu.Children.OfType<Border>().FirstOrDefault();
            var popup = stylesSubmenu.Children.OfType<Popup>().FirstOrDefault();
            Assert.IsNotNull(border, "Sub-menu border not found.");
            Assert.IsNotNull(popup, "Sub-menu popup not found.");

            var layoutGrid = border.Child as Grid;
            Assert.IsNotNull(layoutGrid, "Could not find submenu layout grid.");
            var submenuLabel = layoutGrid.Children.OfType<TextBlock>().FirstOrDefault();
            Assert.IsNotNull(submenuLabel, "Could not find submenu label.");
            Assert.AreEqual(0.5, submenuLabel.Opacity, 0.001, "Disabled submenu should be visually dimmed.");

            //Trigger MouseEnter to verify the submenu does not open when disabled
            border.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.MouseEnterEvent
            });
            DispatcherUtil.DoEvents();

            //Assert that the submenu does not open when there are no styles
            Assert.IsFalse(popup.IsOpen, "Disabled Group Style submenu should not open on hover.");
        }

        private static void AssertGroupStylesMatchDefaults(IEnumerable<GroupStyleItem> actualStyles)
        {
            var actualList = actualStyles.ToList();
            var defaultList = GroupStyleItem.DefaultGroupStyleItems.ToList();

            Assert.AreEqual(defaultList.Count, actualList.Count);

            foreach (var defaultStyle in defaultList)
            {
                var currentStyle = actualList.FirstOrDefault(style => style.GroupStyleId == defaultStyle.GroupStyleId);
                Assert.IsNotNull(currentStyle, $"Missing default style with id {defaultStyle.GroupStyleId}.");
                Assert.IsTrue(currentStyle.IsDefault);
                Assert.AreEqual(defaultStyle.Name, currentStyle.Name);
                Assert.AreEqual(defaultStyle.FontSize, currentStyle.FontSize);
                Assert.IsTrue(string.Equals(defaultStyle.HexColorString, currentStyle.HexColorString, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void OpenGroupContextMenu(AnnotationView groupAnnotationView)
        {
            groupAnnotationView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = UIElement.MouseRightButtonDownEvent
            });
            groupAnnotationView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = UIElement.MouseRightButtonUpEvent
            });
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// Validates that restoring default styles repopulates the Group Style submenu in the graph context menu
        /// </summary>
        [Test]
        public void GroupStyleSubmenu_LoadsRestoredDefaults_AfterResetFromEmptyStyles()
        {
            Open(@"UI\GroupTest.dyn");

            var dynamoViewModel = View.DataContext as DynamoViewModel;
            Assert.IsNotNull(dynamoViewModel);
            //Remove all the styles from preferences
            dynamoViewModel.PreferenceSettings.GroupStyleItemsList = new List<GroupStyleItem>();

            var annotationView = NodeViewWithGuid("a432d63f-7a36-45ad-b30a-7924beb20e90");

            //Create the context menu and validates that Group Style submenu is disabled when there are no styles
            OpenGroupContextMenu(annotationView);

            var emptyStylesSubmenu = annotationView.GroupStyleSelectorGrid as Grid;
            Assert.IsNotNull(emptyStylesSubmenu, "Styles sub-menu grid not found.");
            var emptyBorder = emptyStylesSubmenu.Children.OfType<Border>().FirstOrDefault();
            var emptyPopup = emptyStylesSubmenu.Children.OfType<Popup>().FirstOrDefault();
            Assert.IsNotNull(emptyBorder, "Sub-menu border not found.");
            Assert.IsNotNull(emptyPopup, "Sub-menu popup not found.");

            emptyBorder.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.MouseEnterEvent
            });
            DispatcherUtil.DoEvents();
            Assert.IsFalse(emptyPopup.IsOpen, "Disabled Group Style submenu should not open on hover.");

            //Open Preferences and restores the default styles
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();
            preferencesWindow.ResetStylesButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            var prefViewModel = preferencesWindow.DataContext as PreferencesViewModel;
            Assert.IsNotNull(prefViewModel);
            Assert.AreEqual(GroupStyleItem.DefaultGroupStyleItems.Count, prefViewModel.StyleItemsList.Count);

            //Close the Preferences Dialog
            preferencesWindow.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Recreate and open the context menu to validate that Group Style submenu contains restored defaults
            OpenGroupContextMenu(annotationView);

            var stylesSubmenu = annotationView.GroupStyleSelectorGrid as Grid;
            Assert.IsNotNull(stylesSubmenu, "Styles sub-menu grid not found.");

            var border = stylesSubmenu.Children.OfType<Border>().FirstOrDefault();
            var popup = stylesSubmenu.Children.OfType<Popup>().FirstOrDefault();
            Assert.IsNotNull(border, "Sub-menu border not found.");
            Assert.IsNotNull(popup, "Sub-menu popup not found.");

            border.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.MouseEnterEvent
            });
            DispatcherUtil.DoEvents();

            Assert.IsTrue(popup.IsOpen, "Group Style submenu did not open after restoring defaults.");
            Assert.IsInstanceOf<Border>(popup.Child, "Popup content is not a Border.");

            var wrapper = popup.Child as Border;
            var stackPanel = wrapper.Child as StackPanel;
            Assert.IsNotNull(stackPanel, "Could not find StackPanel inside popup.");

            var restoredStylesCount = stackPanel.Children
                .OfType<Border>()
                .Select(b => b.Child)
                .OfType<StackPanel>()
                .Count();

            Assert.AreEqual(GroupStyleItem.DefaultGroupStyleItems.Count, restoredStylesCount);
        }
    }
}
