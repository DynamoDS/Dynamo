using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for GuideBackground.xaml
    /// </summary>
    public partial class GuideBackground : UserControl, INotifyPropertyChanged, IDisposable
    {
        private Rect hole;
        private Rect windowsRect;
        private Window mainWindow;

        /// <summary>
        /// Rect with the size of the Dynamo Window regularly updating its size depending the window's size
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

        public GuideBackground(Window mainWindow)
        {
            InitializeComponent();
            DataContext = this;

            //Initializate the background with the current screen size
            WindowsRect = new Rect(0, 0, System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight);

            //This event is triggered everytime that the main window changes it's size
            this.mainWindow = mainWindow;
            this.mainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        /// <summary>
        /// This method updates the width and height of the background window everytime it resizes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            windowsRect.Width = e.NewSize.Width;
            windowsRect.Height = e.NewSize.Height;
            RaisePropertyChanged(nameof(WindowsRect));
        }

        public void Dispose()
        {
            mainWindow.SizeChanged -= MainWindow_SizeChanged;
        }
    }
}
