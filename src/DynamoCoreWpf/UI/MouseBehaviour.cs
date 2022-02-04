using Dynamo.Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Shapes;

namespace Dynamo.Wpf.UI
{
    /// <summary>
    /// Class that handles passing mouse position to Connectors for placement of ConnectorPins.
    /// Stack overflow reference: https://stackoverflow.com/questions/30047415/how-do-i-get-mouse-positions-in-my-view-model
    /// </summary>
    public class MouseBehaviour : Behavior<Path>
    {
        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
           "MouseY", typeof(double), typeof(MouseBehaviour), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
           "MouseX", typeof(double), typeof(MouseBehaviour), new PropertyMetadata(default(double)));

        /// <summary>
        /// MouseY location
        /// </summary>
        public double MouseY
        {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        /// <summary>
        /// MouseX location
        /// </summary>
        public double MouseX
        {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            MouseX = pos.X;
            MouseY = pos.Y;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}
