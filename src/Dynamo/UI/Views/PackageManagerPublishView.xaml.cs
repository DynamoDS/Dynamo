using System.Windows.Controls;
using Dynamo.Commands;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishView.xaml
    /// </summary>
    public partial class PackageManagerPublishView : UserControl
    {
        private PackageManagerPublishViewModel viewModel;

        public PackageManagerPublishView()
        {
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(PackageManagerPublishView_Loaded);
        }

        void PackageManagerPublishView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel = (PackageManagerPublishViewModel)DataContext;
            viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(viewModel_PropertyChanged);
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Keywords":
                    DynamoCommands.SubmitCommand.RaiseCanExecuteChanged();
                    break;
                case "Description":
                    DynamoCommands.SubmitCommand.RaiseCanExecuteChanged();
                    break;
                case "MinorVersion":
                    DynamoCommands.SubmitCommand.RaiseCanExecuteChanged();
                    break;
                case "MajorVersion":
                    DynamoCommands.SubmitCommand.RaiseCanExecuteChanged();
                    break;
            }
        }
    }

}
