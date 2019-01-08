using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dynamo.UI.Controls
{
    public class UseLevelPopup: Popup
    {
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            bool isOpen = this.IsOpen;
            base.OnPreviewMouseLeftButtonUp(e);

            if (isOpen && !this.IsOpen)
                IsOpen = true;
        }

        /// <summary>
        /// Override EndInit().
        /// </summary>
        public override void EndInit()
        {
            this.MouseWheel += OnMouseWheel;
            base.EndInit();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            IsOpen = false;
        }
    }
}
