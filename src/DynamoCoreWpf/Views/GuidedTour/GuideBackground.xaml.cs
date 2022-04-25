using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Wpf.UI.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for GuideBackground.xaml
    /// </summary>
    public partial class GuideBackground : UserControl, INotifyPropertyChanged, IDisposable
    {
        private CutOffArea cutOffBackgroundArea;
        private HighlightArea highlightBackgroundArea;
        private Rect overlayRect;
        private Window mainWindow;

        /// <summary>
        /// Rect with the size of the Dynamo Window regularly updating its size depending the window's size, this will be used for drawing the overlay
        /// </summary>
        public Rect OverlayRect
        { 
            get { 
                return overlayRect; 
            }
            set
            {
                overlayRect = value;
                RaisePropertyChanged(nameof(OverlayRect));
            }
        }

        /// <summary>
        /// Property that holds information used to cut a rectangle on the guide background so the user can have interaction
        /// </summary>
        public CutOffArea CutOffBackgroundArea
        {
            get {
                return cutOffBackgroundArea; 
            }
            set {
                cutOffBackgroundArea = value;
                RaisePropertyChanged(nameof(CutOffBackgroundArea));
            }
        }

        /// <summary>
        /// Property that holds information used for drawing the Highlight rectangle on the guide background 
        /// </summary>
        public HighlightArea HighlightBackgroundArea
        {
            get
            {
                return highlightBackgroundArea;
            }
            set
            {
                highlightBackgroundArea = value;
                RaisePropertyChanged(nameof(HighlightBackgroundArea));
            }
        }

        /// <summary>
        /// This method clears the Highlight rectangle from the Canvas
        /// </summary>
        internal void ClearHighlightSection()
        {
            if (HighlightBackgroundArea != null)
            {
                HighlightBackgroundArea.HighlightColor = string.Empty;
                HighlightBackgroundArea.ClearHighlightRectangleSize();
            }
        }

        /// <summary>
        /// This method will clean the cutoff area so the removed rectangle will be shown again
        /// </summary>
        internal void ClearCutOffSection()
        {
            if (CutOffBackgroundArea != null)
                CutOffBackgroundArea.CutOffRect = new Rect();
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
            OverlayRect = new Rect(0, 0, System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight);
            CutOffBackgroundArea = new CutOffArea();
            HighlightBackgroundArea = new HighlightArea();

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
            overlayRect.Width = e.NewSize.Width;
            overlayRect.Height = e.NewSize.Height;
            RaisePropertyChanged(nameof(OverlayRect));
        }

        public void Dispose()
        {
            mainWindow.SizeChanged -= MainWindow_SizeChanged;
        }
    }
}
