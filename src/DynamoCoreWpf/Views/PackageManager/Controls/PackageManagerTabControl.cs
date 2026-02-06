using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PackageManager.UI
{
    public class PackageManagerTabControl : TabControl
    {
        public bool SuppressHomeEndNavigation { get; set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (SuppressHomeEndNavigation && (e.Key == Key.Home || e.Key == Key.End))
            {
                // Skip base handling to prevent tab switching, but do not
                // mark as handled so WebView2 can still process the key.
                return;
            }

            base.OnKeyDown(e);
        }
    }
}
