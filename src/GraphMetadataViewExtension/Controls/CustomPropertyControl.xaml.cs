using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.UI.Commands;

namespace Dynamo.GraphMetadata.Controls
{
    /// <summary>
    /// Interaction logic for CustomPropertyControl.xaml
    /// </summary>
    public partial class CustomPropertyControl : UserControl
    {
        public DelegateCommand EditPropertyNameCmd { get; set; }
        public DelegateCommand DeletePropertyNameCmd { get; set; }
        public bool PropertyNameEnabled { get; set; }

        public event EventHandler RequestDelete;

        private void OnRequestDelete(EventArgs e)
        {
            RequestDelete?.Invoke(this, e);
        }

        public CustomPropertyControl()
        {
            InitializeComponent();
            InitializeCommands();
            PropertyNameEnabled = false;
        }

        private void InitializeCommands()
        {
            this.EditPropertyNameCmd = new DelegateCommand(EditPropertyNameCmdExecute);
            this.DeletePropertyNameCmd = new DelegateCommand(DeletePropertyNameCmdExecute);
        }

        private void EditPropertyNameCmdExecute(object obj)
        {
            PropertyNameEnabled = !PropertyNameEnabled;
            propertyNameText.IsEnabled = PropertyNameEnabled;
            propertyNameText.Focus();

            propertyNameText.LostFocus += DisableEditable;
        }

        private void DisableEditable(object sender, RoutedEventArgs e)
        {
            propertyNameText.IsEnabled = false;
            propertyNameText.LostFocus -= DisableEditable;
        }

        private void DeletePropertyNameCmdExecute(object obj)
        {
            if (string.IsNullOrEmpty(this.PropertyName))
                return;

            OnRequestDelete(EventArgs.Empty);
        }

        #region DependencyProperties

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            nameof(PropertyName),
            typeof(string),
            typeof(CustomPropertyControl)
        );

        public string PropertyValue
        {
            get { return (string)GetValue(PropertyValueProperty); }
            set { SetValue(PropertyValueProperty, value); }
        }

        public static readonly DependencyProperty PropertyValueProperty = DependencyProperty.Register(
            nameof(PropertyValue),
            typeof(string),
            typeof(CustomPropertyControl)
        );

        #endregion
    }
}
