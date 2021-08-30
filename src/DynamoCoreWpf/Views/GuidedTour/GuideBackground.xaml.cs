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

            WindowsRect = new Rect(0, 0, System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight);
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }
                
        private Rect hole;
        private Rect windowsRect;

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
            windowsRect.Width = e.NewSize.Width;
            windowsRect.Height = e.NewSize.Height;
            RaisePropertyChanged(nameof(WindowsRect));
        }
    }
}
