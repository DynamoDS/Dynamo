using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Default value popup
    /// </summary>
    public class DefaultValuePopup : Popup
    {
        /// <summary>
        /// Override this so that popup does not dismiss itself
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            bool isOpen = this.IsOpen;
            base.OnPreviewMouseLeftButtonUp(e);

            if (isOpen && !this.IsOpen)
                IsOpen = true;
        }
    }
}
