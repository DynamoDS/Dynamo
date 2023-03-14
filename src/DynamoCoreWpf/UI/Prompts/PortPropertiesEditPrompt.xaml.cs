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
        private bool isStatusWarning;
        private string errorString = string.Empty;

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
        /// Shows if the port name validation has gone through or not
        /// </summary>
        public bool IsStatusWarning
        {
            get
            {
                return isStatusWarning;
            }
            set
            {
                isStatusWarning = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsStatusWarning)));
            }
        }


        private Window owner
        {
            get
            {
                var f = WpfUtilities.FindUpVisualTree<DynamoView>(this);
                if (f != null) return f;

                return null;
            }
        }


        /// <summary>
        /// Contains the notification of the status label
        /// </summary>
        public string ErrorString
        {
            get { return errorString; }
            set
            {
                errorString = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ErrorString)));
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

            Owner = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            nameBox.Focus();
        }

        #region Methods
        void OK_Click(object sender, RoutedEventArgs e)
        {
            // Prevent the prompt from closing if the validation conditions are not satisfied
            if (ValidatePortName(PortName))
            {
                return;
            }

            DialogResult = !string.IsNullOrEmpty(PortName);
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
            e.Handled = true;
            if (ValidatePortName(nameBox.Text))
            {
                IsStatusWarning = true;
            }
            else
            {
                IsStatusWarning = false;
            }
        }
        
        // Run all name validation checks here
        private bool ValidatePortName(string name)
        {
            if (IsPortNameInValid(name))
            {
                ErrorString = Dynamo.Wpf.Properties.Resources.MessagePortNameInvalid;
                return true;
            }
            if (!IsPortNameUnique(name))
            {
                ErrorString = Dynamo.Wpf.Properties.Resources.MessagePortNameInvalid;
                return true;
            }
            if (IsPortNameNumber(name))
            {
                ErrorString = Dynamo.Wpf.Properties.Resources.MessagePortNameInvalid;
                return true;
            }

            return false;
        }


        public bool IsPortNameInValid(string portName)
        {
            if (DynamoUtilities.PathHelper.IsFileNameInValid(portName))
                return true;

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
