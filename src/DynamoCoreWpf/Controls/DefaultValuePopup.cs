using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Default value popup
    /// </summary>
    public class DefaultValuePopup : Popup
    {
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            bool isOpen = this.IsOpen;
            base.OnPreviewMouseLeftButtonUp(e);

            if (isOpen && !this.IsOpen)
                IsOpen = true;
        }
    }
}
