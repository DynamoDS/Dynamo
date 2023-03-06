using System.ComponentModel;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Core;
using Dynamo.Graph.Nodes;

namespace UI.Prompts
{
    /// <summary>
    /// Interaction logic for PortPropertiesEditPrompt.xaml
    /// </summary>
    public partial class PortPropertiesEditPrompt : Window, INotifyPropertyChanged
    {
        public PortPropertiesEditPrompt()
        {
            InitializeComponent();
            
            this.DataContext = this;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            nameBox.Focus();
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                MessageBoxService.Show
                (
                    Dynamo.Wpf.Properties.Resources.MessageCustomNodeNoName,
                    Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            else if (PathHelper.IsFileNameInValid(Text))
            {
                MessageBoxService.Show
                (
                   Dynamo.Wpf.Properties.Resources.MessageCustomNodeNameInvalid,
                   Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                   MessageBoxButton.OK,
                   MessageBoxImage.Error
                );
            }
            else
            {
                DialogResult = true;
            }
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public string Text
        {
            get { return nameBox.Text; }
        }

        public string Description
        {
            get { return DescriptionInput.Text; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, e);
        }

        private PortType portType;
        public PortType PortType
        {
            get
            {
                return portType;
            }
            set
            {
                portType = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(PortType)));
            }
        }


        /// <summary>
        /// Allows for the dragging of this custom-styled window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FunctionNamePrompt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PathHelper.IsFileNameInValid(nameBox.Text))
            {
                ErrorIcon.Visibility = Visibility.Visible;
                ErrorUnderline.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorIcon.Visibility = Visibility.Collapsed;
                ErrorUnderline.Visibility = Visibility.Collapsed;
            }
        }
    }
}
