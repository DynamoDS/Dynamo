using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.Controls
{
    /// <summary>
    /// Overriden MenuItem which can be put outside Menu container. 
    /// Base MenuItem is not able to open its submenu on mouse hover
    /// </summary>
    class ParentMenuItem : MenuItem
    {
        public ParentMenuItem()
        {
            MouseLeave += OkMenuItem_MouseLeave;
            MouseEnter += OkMenuItem_MouseEnter;
        }

        private void OkMenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            IsSubmenuOpen = true;
        }

        private void OkMenuItem_MouseLeave(object sender, MouseEventArgs e)
        {
            IsHighlighted = false;
            IsSubmenuOpen = false;
        }
    }
}
