using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.UI.Commands;

namespace Dynamo.GraphMetadata.Controls
{
    /// <summary>
    /// Interaction logic for CustomPropertyControl.xaml
    /// </summary>
    public partial class CustomPropertyControl : UserControl, INotifyPropertyChanged
    {
        public DelegateCommand EditPropertyNameCmd { get; set; }
        public DelegateCommand DeletePropertyNameCmd { get; set; }

        public bool Deletable { get; set; }
        public bool PropertyNameEnabled { get; set; }


        public CustomPropertyControl()
        {
            InitializeComponent();
            InitializeCommands();
        }
        public CustomPropertyControl(int suffix)
        {
            InitializeComponent();
            InitializeCommands();
            PropertyName = $"Custom Property {suffix}";
            Deletable = false;
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
            // FocusManager.SetFocusedElement(propertyNameText, true)

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

            this.Deletable = true;
            //this.PropertyName = "";
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }
    }
}
