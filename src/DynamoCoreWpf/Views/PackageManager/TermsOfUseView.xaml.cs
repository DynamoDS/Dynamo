using System.Windows;


namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for TermsOfUseView.xaml
    /// </summary>
    public partial class TermsOfUseView : Window
    {
        public bool AcceptedTermsOfUse { get; private set; }

        public TermsOfUseView(string touFilePath)
        {
            InitializeComponent();
            AcceptedTermsOfUse = false;
            TermsOfUseContent.File = touFilePath;
        }

        private void AcceptTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            AcceptedTermsOfUse = true;
            Close();
        }

        private void DeclineTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
