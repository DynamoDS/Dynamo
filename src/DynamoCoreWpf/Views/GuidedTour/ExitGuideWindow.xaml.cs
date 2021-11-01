using Dynamo.Wpf.ViewModels.GuidedTour;
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
        public ExitGuideWindow(FrameworkElement mainRootElement, ExitGuideWindowViewModel exitGuideModel)
        {
            InitializeComponent();

            Height = exitGuideModel.Height;
            Width = exitGuideModel.Width;
            TitleLabel.Content = exitGuideModel.Title;
            ContentRichTextBox.CustomText = exitGuideModel.FormattedText ?? string.Empty;
            ContentRichTextBox.Width = Width;

            //Calculates the middle of the screen to add an offset for the modal
            VerticalOffset = (mainRootElement.ActualHeight / 2) - (exitGuideModel.Height / 2);
            HorizontalOffset = (mainRootElement.ActualWidth / 2) - (exitGuideModel.Width / 2);

            if (string.IsNullOrEmpty(exitGuideModel.FormattedText))
                HideRichTextBox();
        }


        /// <summary>
        /// This method hides the text box to give the possibility to use this popup in other steps
        /// </summary>
        private void HideRichTextBox()
        {
            Height -= ContentRichTextBox.ActualHeight;
            RootLayout.Height = Height;
            RootLayout.Width = Width;

            ContentRichTextBox.Visibility = Visibility.Collapsed;
            TitleLine.Opacity = 0;
        }
    }
}
