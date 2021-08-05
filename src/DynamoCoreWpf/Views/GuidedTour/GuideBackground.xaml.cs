using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for GuideBackground.xaml
    /// </summary>
    public partial class GuideBackground : UserControl
    {
        public GuideBackground()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Width of the background
        /// </summary>
        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        /// <summary>
        /// Height of the background
        /// </summary>
        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        /// <summary>
        /// Rect with the size of the Dynamo Window 
        /// </summary>
        public Rect WindowsRect { get { return new Rect(0, 0, CanvasWidth, CanvasHeight); } }

        /// <summary>
        /// Rect used to cut the hole on the guide background 
        /// </summary>
        public Rect HoleRect { get { return new Rect (50, 100, 250, 130); } }

        //Dependency Properties defined to be able to bind values for the properties above
        public static readonly DependencyProperty CanvasWidthProperty = DependencyProperty
            .Register("CanvasWidth", typeof(double), typeof(GuideBackground),
                        new FrameworkPropertyMetadata(new double()));

        public static readonly DependencyProperty CanvasHeightProperty = DependencyProperty
            .Register("CanvasHeight", typeof(double), typeof(GuideBackground),
                        new FrameworkPropertyMetadata(new double()));

    }
}
