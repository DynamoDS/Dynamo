using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dynamo.PackageManager.UI
{
    public class PackageManagerTabControl : TabControl
    {
        public static readonly DependencyProperty SuppressHomeEndWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
                "SuppressHomeEndWhenSelected",
                typeof(bool),
                typeof(PackageManagerTabControl),
                new FrameworkPropertyMetadata(false));

        public static bool GetSuppressHomeEndWhenSelected(DependencyObject element)
        {
            return (bool)element.GetValue(SuppressHomeEndWhenSelectedProperty);
        }

        public static void SetSuppressHomeEndWhenSelected(DependencyObject element, bool value)
        {
            element.SetValue(SuppressHomeEndWhenSelectedProperty, value);
        }

        public PackageManagerTabControl()
        {
            Loaded += OnLoaded;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (ReferenceEquals(e.OriginalSource, this))
            {
                FocusSelectedTabHeaderAsync();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (TryHandlePageUpDownNavigation(e))
            {
                return;
            }

            if (ShouldSuppressHomeEndNavigation(e.Key))
            {
                // Skip base handling to prevent tab switching, but do not
                // mark as handled so WebView2 can still process the key.
                return;
            }

            base.OnKeyDown(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedTabHeaderAsync();
        }

        private void FocusSelectedTabHeaderAsync()
        {
            // Defer focus until layout/selection updates are applied to avoid
            // fighting WPF's internal focus transitions during tab changes.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SelectedItem is TabItem selectedTab)
                {
                    selectedTab.Focus();
                }
                else
                {
                    Focus();
                }
            }), DispatcherPriority.Background);
        }

        private bool ShouldSuppressHomeEndNavigation(Key key)
        {
            if (key != Key.Home && key != Key.End)
            {
                return false;
            }

            if (SelectedItem == null)
            {
                return false;
            }

            return ItemContainerGenerator.ContainerFromItem(SelectedItem) is DependencyObject selectedTab &&
                   GetSuppressHomeEndWhenSelected(selectedTab);
        }

        private bool TryHandlePageUpDownNavigation(KeyEventArgs e)
        {
            if (e.Key != Key.PageUp && e.Key != Key.PageDown)
            {
                return false;
            }

            // Only use PageUp/PageDown for tab navigation while the tab strip itself has focus.
            // This lets the hosted page keep receiving these keys after a user clicks into it.
            if (!IsTabStripFocused())
            {
                return false;
            }

            var direction = e.Key == Key.PageDown ? 1 : -1;
            if (!TrySelectAdjacentTab(direction))
            {
                return false;
            }

            e.Handled = true;
            return true;
        }

        private bool TrySelectAdjacentTab(int direction)
        {
            if (SelectedIndex < 0 || direction == 0)
            {
                return false;
            }

            var index = SelectedIndex + direction;
            while (index >= 0 && index < Items.Count)
            {
                if (ItemContainerGenerator.ContainerFromIndex(index) is TabItem tabItem &&
                    tabItem.IsEnabled &&
                    tabItem.Visibility == Visibility.Visible)
                {
                    SelectedIndex = index;
                    tabItem.Focus();
                    return true;
                }

                index += direction;
            }

            return false;
        }

        private bool IsTabStripFocused()
        {
            return Keyboard.FocusedElement is TabItem tabItem &&
                ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(tabItem), this);
        }
    }
}
