using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PackageManager.UI
{
    public class PackageManagerTabControl : TabControl
    {
        internal bool SuppressHomeEndNavigation { get; set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(TryHandlePageNavigation(e))
            {
                return;
            }

            if (SuppressHomeEndNavigation && (e.Key == Key.Home || e.Key == Key.End))
            {
                // Skip base handling to prevent tab switching, but do not
                // mark as handled so WebView2 can still process the key.
                return;
            }

            base.OnKeyDown(e);
        }

        private bool TryHandlePageNavigation(KeyEventArgs e)
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
            return Keyboard.FocusedElement == this || Keyboard.FocusedElement is TabItem;
        }
    }
}
