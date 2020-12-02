using System;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.Wpf.Windows
{
    // <summary>
    //  Display a modeless child window that is parented on the Dynamo window
    //  and will close when the Dynamo window is closed.
    // </summary>
    public class ModelessChildWindow : Window
    {
        /// <summary>
        ///  Screen position of the child window (in a reference type.)
        /// </summary>
        public class WindowRect
        {
            public double Left;
            public double Top;
            public double Width;
            public double Height;
        }

        /// <summary>
        /// Gets the window rectangle (location + size)
        /// </summary>
        public WindowRect SavedWindowRect { get; private set; }

        /// <summary>
        /// Creates a window with an initially empty window rectangle.
        /// Note: This constructor assumes Owner is set externally.
        /// </summary>
        public ModelessChildWindow()
        {
            InitWindowRect(null);
        }

        /// <summary>
        /// Construct a ModelessChildWindow.
        /// </summary>
        /// <param name="viewParent">A UI object in the Dynamo visual tree.</param>
        /// <param name="rect">A reference to the Rect object that will store the window's position during this session.</param>
        public ModelessChildWindow(DependencyObject viewParent, ref WindowRect rect)
        {
            Owner = WpfUtilities.FindUpVisualTree<DynamoView>(viewParent);

            rect = InitWindowRect(rect);
        }

        private WindowRect InitWindowRect(WindowRect rect)
        {
            if (rect == null || !IsRectVisibleOnScreen(rect, Owner))
            {
                rect = new WindowRect();
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                LocationChanged += ModelessChildWindow_LocationChanged;
                SizeChanged += ModelessChildWindow_SizeChanged;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Loaded += ModelessChildWindow_Loaded;
            }

            SavedWindowRect = rect;
            return rect;
        }

        private bool IsRectVisibleOnScreen(WindowRect windowRect, Window w)
        {
            int minimumVisiblePixels = 10;
            var source = PresentationSource.FromVisual(w);
            var toDeviceMatrix = source.CompositionTarget.TransformToDevice;

            Rect rect = new Rect(windowRect.Left, windowRect.Top, windowRect.Width, windowRect.Height);
            var pixelRect = Rect.Transform(rect, toDeviceMatrix);
            var pixelRectangle = 
                new System.Drawing.Rectangle(
                    (int)pixelRect.X, 
                    (int)pixelRect.Y, 
                    (int)pixelRect.Width, 
                    (int)pixelRect.Height
                );

            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                var intersection = pixelRectangle;
                intersection.Intersect(screen.WorkingArea);
                if (intersection.Width >= minimumVisiblePixels && 
                    intersection.Height >= minimumVisiblePixels)
                {
                    // sufficiently visible on at least one screen
                    return true;
                }
            }

            // Window rect does not overlap any current screens
            return false;
        }

        private void ModelessChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)sender;

            w.Left = SavedWindowRect.Left;
            w.Top = SavedWindowRect.Top;
            w.Width = SavedWindowRect.Width;
            w.Height = SavedWindowRect.Height;

            LocationChanged += ModelessChildWindow_LocationChanged;
            SizeChanged += ModelessChildWindow_SizeChanged;
        }

        private void ModelessChildWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                SavedWindowRect.Width = e.NewSize.Width;
                SavedWindowRect.Height = e.NewSize.Height;
            }
        }

        private void ModelessChildWindow_LocationChanged(object sender, EventArgs e)
        {
            Window w = (Window)sender;
            SavedWindowRect.Left = w.Left;
            SavedWindowRect.Top = w.Top;
        }
    }
}
