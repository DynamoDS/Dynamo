using System.Windows;
using System.Windows.Controls;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for LoadingAnimationStripeControl.xaml
    /// </summary>
    public partial class LoadingAnimationStripeControl : UserControl
    {
        public LoadingAnimationStripeControl()
        {
            InitializeComponent();
        }

        // Define a Dependency Property
        public static readonly DependencyProperty AnimationSpeedProperty =
            DependencyProperty.Register(nameof(AnimationSpeedProperty), typeof(double), typeof(LoadingAnimationStripeControl), new PropertyMetadata(1.5, OnSpeedChanged));

        // CLR Property Wrapper
        public double AnimationSpeed
        {
            get { return (double)GetValue(AnimationSpeedProperty); }
            set { SetValue(AnimationSpeedProperty, value); }
        }

        private static void OnSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingAnimationStripeControl control)
            {
                control.SetResourceValue((double)e.NewValue);
            }
        }

        private void SetResourceValue(double newValue)
        {
            this.Resources["animationSpeed"] = newValue;
        }
    }
}
