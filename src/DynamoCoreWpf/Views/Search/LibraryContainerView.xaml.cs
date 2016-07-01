using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;

using TextBox = System.Windows.Controls.TextBox;

namespace Dynamo.Search
{
    /// <summary>
    ///     Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView
    {
        private readonly SearchViewModel viewModel;
        private readonly DynamoViewModel dynamoViewModel;

        private const string baseUrl = @"pack://application:,,,/DynamoCoreWpf;component/UI/Images/";

        private BitmapImage searchIconBitmapNormal =
            new BitmapImage(new Uri(baseUrl + "search_normal.png"));
        private BitmapImage searchIconBitmapHover =
            new BitmapImage(new Uri(baseUrl + "search_hover.png"));

        public SearchView(SearchViewModel searchViewModel, DynamoViewModel dynamoViewModel)
        {
            viewModel = searchViewModel;
            this.dynamoViewModel = dynamoViewModel;

            DataContext = viewModel;
            InitializeComponent();
            Loaded += OnSearchViewLoaded;
            Unloaded += OnSearchViewUnloaded;

            SearchTextBox.IsVisibleChanged += delegate
            {
                if (!SearchTextBox.IsVisible) return;

                this.viewModel.SearchCommand.Execute(null);
                Keyboard.Focus(SearchTextBox);
                SearchTextBlock.Text = Properties.Resources.SearchTextBlockText;
            };
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                SearchTextBox.Focus();
            }
        }

        private void OnSearchViewUnloaded(object sender, EventArgs e)
        {
            viewModel.RequestFocusSearch -= OnSearchViewModelRequestFocusSearch;
        }

        private void OnSearchViewLoaded(object sender, RoutedEventArgs e)
        {
            MouseEnter += OnSearchViewMouseEnter;
            MouseLeave += OnSearchViewMouseLeave;

            SearchTextBox.PreviewKeyDown += KeyHandler;

            viewModel.RequestFocusSearch += OnSearchViewModelRequestFocusSearch;
        }

        private void OnSearchViewMouseLeave(object sender, MouseEventArgs e)
        {
            viewModel.SearchScrollBarVisibility = false;
        }

        private void OnSearchViewMouseEnter(object sender, MouseEventArgs e)
        {
            viewModel.SearchScrollBarVisibility = true;
        }

        /// <summary>
        ///     A KeyHandler method used by SearchView, increments decrements and executes based on input.
        /// </summary>
        /// <param name="sender">Originating object for the KeyHandler </param>
        /// <param name="e">Parameters describing the key push</param>
        public void KeyHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        ClearSearchBox();
                        e.Handled = true;
                        break;
                    }

                case Key.Enter:
                    {
                        e.Handled = true;
                        if (IsAnySearchResult())
                        {
                            viewModel.ExecuteSelectedItem();
                            Keyboard.Focus(SearchTextBox);
                        }

                        break;
                    }

                case Key.Down:
                    {
                        e.Handled = true;
                        if (IsAnySearchResult())
                        {
                            viewModel.MoveSelection(SearchViewModel.Direction.Down);
                        }
                        
                        break;
                    }

                case Key.Up:
                    {
                        e.Handled = true;
                        if (IsAnySearchResult())
                        {
                            viewModel.MoveSelection(SearchViewModel.Direction.Up);
                        }
                        
                        break;
                    }
            }
        }

        private bool IsAnySearchResult()
        {
            return viewModel.CurrentMode == SearchViewModel.ViewMode.LibrarySearchView
                   && viewModel.IsAnySearchResult;
        }

        private void OnSearchViewModelRequestFocusSearch(object sender, EventArgs e)
        {
            SearchTextBox.Focus();
        }

        public void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            viewModel.SearchCommand.Execute(null);
        }

        // Not used anywhere.
        public void ListBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem)sender).IsSelected = true;
            Keyboard.Focus(SearchTextBox);
        }

        private void OnLibraryExpanderClick(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.OnSidebarClosed(this, EventArgs.Empty);
        }

        private void OnLibraryExpanderMouseEnter(object sender, MouseEventArgs e)
        {
            var b = (Button)sender;
            var g = (Grid)b.Parent;
            var lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#cccccc");
            var collapsestate = (Image)(b).Content;
            var collapsestateSource = new Uri(baseUrl + "expand_hover.png");
            collapsestate.Source = new BitmapImage(collapsestateSource);

            Cursor = CursorLibrary.GetCursor(CursorSet.LinkSelect);
        }

        private void OnLibraryExpanderMouseLeave(object sender, MouseEventArgs e)
        {
            var b = (Button)sender;
            var g = (Grid)b.Parent;
            var lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");
            var collapsestate = (Image)(b).Content;
            var collapsestateSource = new Uri(baseUrl + "expand_normal.png");
            collapsestate.Source = new BitmapImage(collapsestateSource);

            Cursor = null;
        }

        private void OnSearchTextBoxGridMouseEnter(object sender, MouseEventArgs e)
        {
            SearchIcon.Source = searchIconBitmapHover;
            SearchTextBlock.Text = Properties.Resources.SearchTextBlockText;
        }

        private void OnSearchTextBoxGridMouseLeave(object sender, MouseEventArgs e)
        {
            SearchIcon.Source = searchIconBitmapNormal;
            SearchTextBlock.Text = Properties.Resources.SearchTextBlockText;
        }

        private void OnSearchCancelButtonClick(object sender, RoutedEventArgs e)
        {
            ClearSearchBox();
        }

        private void ClearSearchBox()
        {
            SearchTextBox.Text = "";
            Keyboard.Focus(SearchTextBox);
        }

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.SearchIconAlignment = HorizontalAlignment.Left;
        }

        private void TextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (viewModel == null) return;

            viewModel.SearchIconAlignment = string.IsNullOrEmpty(viewModel.SearchText)
                ? HorizontalAlignment.Center
                : HorizontalAlignment.Left;
        }

        private void OnLibraryViewPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            SearchTextBox.Text = "";
        }


        /// <summary>
        /// On drag&drop starts change cursor to cursor, that is shown when the user is hovering over the workspace.
        /// </summary>
        private void OnLibraryContainerViewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = e.Effects.HasFlag(DragDropEffects.Copy) || e.Effects.HasFlag(DragDropEffects.Move);

            if (!e.UseDefaultCursors)
            {
                Mouse.SetCursor(CursorLibrary.GetCursor(CursorSet.DragMove));
            }

            e.Handled = true;
        }

        private void OnFilterMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FilterPopup.IsOpen = true;

            Analytics.TrackEvent(Actions.FilterButtonClicked, Categories.SearchUX);
        }

        private void OnViewLayoutSelectorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LayoutPopup.IsOpen = true;
        }

        private void OnOnlyTextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.UnSelectAllCategories();
        }
    }
}
