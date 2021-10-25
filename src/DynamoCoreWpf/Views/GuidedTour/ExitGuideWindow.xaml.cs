using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for ExitGuideWindow.xaml
    /// </summary>
    public partial class ExitGuideWindow : Popup
    {       

        public ExitGuideWindow(FrameworkElement mainRootElement)
        {
            InitializeComponent();

            ContentRichTextBox.Width = Width;

            //Calculates the middle of the screen to add an offset for the modal
            VerticalOffset = (mainRootElement.ActualHeight / 2) - (Height / 2);
            HorizontalOffset = (mainRootElement.ActualWidth / 2) - (Width / 2);
        }

        /// <summary>
        /// This method hides the text box to give the possibility to use this popup in other steps
        /// </summary>
        private void HideRichTextBox()
        {
            Height -= ContentRichTextBox.ActualHeight;
            ContentRichTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
