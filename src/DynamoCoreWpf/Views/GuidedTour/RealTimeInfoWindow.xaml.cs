using System.Windows.Controls.Primitives;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for RealTimeInfoWindow.xaml
    /// </summary>
    public partial class RealTimeInfoWindow : Popup
    {

        /// <summary>
        /// This property contains the text that will be shown in the popup and it can be updated on runtime.
        /// </summary>
        public string TextContent { get; set; }

        public RealTimeInfoWindow()
        {
            InitializeComponent();

            DataContext = this;       
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
        }
    }
}
