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
        /// Screen position of the child window.
        /// </summary>
        public class WindowPosition
        {
            public double Left;
            public double Top;
            public double Width;
            public double Height;
        }

        private WindowPosition Position;

        /// <summary>
        /// Construct a ModelessChildWindow.
        /// </summary>
        /// <param name="viewParent">A UI object in the Dynamo visual tree.</param>
        public ModelessChildWindow(DependencyObject viewParent, ref WindowPosition position)
        {
            Owner = WpfUtilities.FindUpVisualTree<DynamoView>(viewParent);
            if (position == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                position = new WindowPosition();

                LocationChanged += ModelessChildWindow_LocationChanged;
                SizeChanged += ModelessChildWindow_SizeChanged;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Loaded += ModelessChildWindow_Loaded;
            }
            Position = position;
                    
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

        private void ModelessChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)sender;
            w.Left = Position.Left;
            w.Top = Position.Top;
            w.Width = Position.Width;
            w.Height = Position.Height;

            LocationChanged += ModelessChildWindow_LocationChanged;
            SizeChanged += ModelessChildWindow_SizeChanged;
        }

        private void ModelessChildWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Position.Width = e.NewSize.Width;
            Position.Height = e.NewSize.Height;
        }

        private void ModelessChildWindow_LocationChanged(object sender, EventArgs e)
        {
            Window w = (Window)sender;
            Position.Left = w.Left;
            Position.Top = w.Top;
        }
    }
}
