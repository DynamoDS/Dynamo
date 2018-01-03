using Dynamo.Controls;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Wpf.Windows
{
    // <summary>
    //  Display a modeless child window that is parented on the Dynamo window
    //  and will close when the Dynamo window is closed.
    // </summary>
    public class ModelessChildWindow : Window
    {
        /// <summary>
        /// Construct a ModelessChildWindow.
        /// </summary>
        /// <param name="viewParent">A UI object in the Dynamo visual tree.</param>
        public ModelessChildWindow(DependencyObject viewParent)
        {
            Owner = WpfUtilities.FindUpVisualTree<DynamoView>(viewParent);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner.Closing += OwnerWindow_Closing;
        }

        private void OwnerWindow_Closing(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ChildWindow_Closing(object sender, EventArgs e)
        {
            Owner.Closing -= OwnerWindow_Closing;
        }
    }
}
