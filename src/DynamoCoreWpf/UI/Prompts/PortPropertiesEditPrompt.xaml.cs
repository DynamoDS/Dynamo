using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        #region Properties
        private PortType portType;

        /// <summary>
        /// Port name
        /// </summary>
        public string PortName
        {
            get { return nameBox.Text; }
        }
        /// <summary>
        /// Port Description
        /// </summary>
        public string Description
        {
            get { return DescriptionInput.Text; }
        }

        /// <summary>
        /// Port Type
        /// </summary>
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
        /// Contains the names of all ports
        /// Used in name validation check
        /// </summary>
        internal List<string> OutPortNames { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public PortPropertiesEditPrompt()
        {
            InitializeComponent();
            
            this.DataContext = this;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            nameBox.Focus();
        }

        #region Methods
        void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PortName))
            {
                MessageBoxService.Show
                (
                    Dynamo.Wpf.Properties.Resources.MessageCustomNodeNoName,
                    Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            else if (PathHelper.IsFileNameInValid(PortName))
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

        // Name validation 
        private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ValidatePortName(nameBox.Text))
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

        // Run all name validation checks here
        private bool ValidatePortName(string name)
        {
            if (PathHelper.IsFileNameInValid(name)) return true;
            if (!IsPortNameUnique(name)) return true;
            if (IsPortNameNumber(name)) return true;

            return false;
        }

        // Check if the name is a number
        private bool IsPortNameNumber(string name)
        {
            return int.TryParse(name, out _);
        }

        // Check if the name is unique
        private bool IsPortNameUnique(string name)
        {
            if (OutPortNames != null && OutPortNames.Any())
            {
                return !OutPortNames.Contains(name);
            }

            return true;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, e);
        }
        #endregion

    }
}
