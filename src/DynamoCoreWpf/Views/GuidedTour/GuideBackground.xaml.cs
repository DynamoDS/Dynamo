using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for GuideBackground.xaml
    /// </summary>
    public partial class GuideBackground : UserControl, INotifyPropertyChanged
    {
        public GuideBackground()
        {
            InitializeComponent();
            DataContext = this;

            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }
                
        private Rect hole;
        private Rect windowsRect;

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
        /// Rect with the size of the Dynamo Window regularly updating its size depending the window's size. Those are represented by 
        /// </summary>
        public Rect WindowsRect { 
            get { 
                return windowsRect; 
            }
            set
            {
                windowsRect = value;
                RaisePropertyChanged(nameof(WindowsRect));
            }
        }

        /// <summary>
        /// Rect used to cut the hole on the guide background 
        /// </summary>
        public Rect HoleRect {
            get {
                return hole; 
            }
            set { 
                hole = value;
                RaisePropertyChanged(nameof(HoleRect));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowsRect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
        }


        //Dependency Properties defined to be able to bind values for the properties above
        public static readonly DependencyProperty CanvasWidthProperty = DependencyProperty
            .Register("CanvasWidth", typeof(double), typeof(GuideBackground),
                        new FrameworkPropertyMetadata(new double()));

        public static readonly DependencyProperty CanvasHeightProperty = DependencyProperty
            .Register("CanvasHeight", typeof(double), typeof(GuideBackground),
                        new FrameworkPropertyMetadata(new double()));

    }
}
