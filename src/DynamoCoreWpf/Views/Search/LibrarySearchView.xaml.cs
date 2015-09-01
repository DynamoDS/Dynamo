﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.Utilities;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibrarySearchView.xaml
    /// </summary>
    public partial class LibrarySearchView : UserControl
    {
        private SearchViewModel viewModel;
        private LibraryDragAndDrop dragDropHelper = new LibraryDragAndDrop();

        public LibrarySearchView()
        {
            InitializeComponent();

            // Invalidate the DataContext here because it will be set at a later 
            // time through data binding expression. This way debugger will not 
            // display warnings for missing properties.
            this.DataContext = null;

            Loaded += OnLibrarySearchViewLoaded;
        }

        private void OnLibrarySearchViewLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as SearchViewModel;
            viewModel.SearchTextChanged +=viewModel_SearchTextChanged;
            // RequestReturnFocusToSearch calls, when workspace was clicked.
            // We should hide tooltip.
            viewModel.RequestReturnFocusToSearch += OnRequestCloseToolTip;
            // When workspace was changed, we should hide tooltip. 
            viewModel.RequestCloseSearchToolTip += OnRequestCloseToolTip;
        }

        private void viewModel_SearchTextChanged(object sender, EventArgs e)
        {
            //Get the scrollview and scroll to top on every text entered
            var scroll = CategoryTreeView.ChildOfType<ScrollViewer>();
            if (scroll != null)
            {               
                scroll.ScrollToTop();
            }
        }

        private void OnNoMatchFoundButtonClick(object sender, RoutedEventArgs e)
        {
            // Clear SearchText in ViewModel, as result search textbox clears as well.
            viewModel.SearchText = "";
        }

        private void OnMemberGroupNameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is System.Windows.Documents.Run)) return;

            var memberGroup = sender as FrameworkElement;
            var memberGroupContext = memberGroup.DataContext as SearchMemberGroup;

            // Show all members of this group.
            memberGroupContext.ExpandAllMembers();

            // Make textblock underlined.
            var textBlock = e.OriginalSource as System.Windows.Documents.Run;
            textBlock.TextDecorations = TextDecorations.Underline;
        }

        private void OnPrefixTextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        #region ToolTip methods

        private void OnMemberMouseEnter(object sender, RoutedEventArgs e)
        {
            ShowTooltip(sender);
        }

        private void OnPopupMouseLeave(object sender, MouseEventArgs e)
        {
            CloseToolTipInternal();
        }

        private void ShowTooltip(object sender)
        {
            FrameworkElement fromSender = sender as FrameworkElement;
            if (fromSender == null) return;
            var nodeVM = fromSender.DataContext as NodeSearchElementViewModel;

            var senderVM = fromSender.DataContext as NodeSearchElementViewModel;

            if (senderVM != null && senderVM.Visibility)
            {
                libraryToolTipPopup.PlacementTarget = fromSender;
                libraryToolTipPopup.SetDataContext(fromSender.DataContext);
            }
        }

        private void CloseToolTipInternal(bool closeImmediately = false)
        {
            libraryToolTipPopup.SetDataContext(null, closeImmediately);
        }

        private void OnRequestCloseToolTip(object sender, EventArgs e)
        {
            CloseToolTipInternal(true);
        }

        #endregion

        #region Drag&Drop

        private void OnButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var senderButton = e.OriginalSource as FrameworkElement;
            if (senderButton != null)
            {
                var searchElementVM = senderButton.DataContext as NodeSearchElementViewModel;

                if (searchElementVM != null)
                    dragDropHelper.HandleMouseDown(e.GetPosition(null), searchElementVM);
            }          
        }

        private void OnButtonPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var senderButton = e.OriginalSource as FrameworkElement;

            if (senderButton == null)
                return;

            var searchElementVM = senderButton.DataContext as NodeSearchElementViewModel;
            if (searchElementVM != null)
                dragDropHelper.HandleMouseMove(senderButton, e.GetPosition(null));
            else
                dragDropHelper.Clear();
        }

        #endregion
    }
}
