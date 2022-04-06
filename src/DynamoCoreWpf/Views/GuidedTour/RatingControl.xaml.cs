using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {
        public RatingControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(RatingControl),
          new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(RatingChanged)));

        private int _max = 5;

        /// <summary>
        /// This property represent the rating(Value) provided by the user
        /// </summary>
        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }
            set
            {
                if (value < 0)
                {
                    SetValue(ValueProperty, 0);
                }
                else if (value > _max)
                {
                    SetValue(ValueProperty, _max);
                }
                else
                {
                    SetValue(ValueProperty, value);
                }
            }
        }

        /// <summary>
        /// This method will be executed everytime the RatingControl DependencyProperty change
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private static void RatingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingControl item = sender as RatingControl;
            int newval = (int)e.NewValue;
            UIElementCollection childs = ((Grid)(item.Content)).Children;

            ToggleButton button = null;

            for (int i = 0; i < newval; i++)
            {
                button = childs[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = true;
            }

            for (int i = newval; i < childs.Count; i++)
            {
                button = childs[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = false;
            }

        }

        /// <summary>
        /// This method will be executed when a specific star in the Rating controls is clicked and the rating Value is updated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void ClickEventHandler(object sender, RoutedEventArgs args)
        {
            ToggleButton button = sender as ToggleButton;
            int newvalue = int.Parse(button.Tag.ToString());
            Value = newvalue;
        }
    }
}
