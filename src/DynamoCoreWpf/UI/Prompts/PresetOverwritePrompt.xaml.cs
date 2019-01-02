using System;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for PresetOverwritePrompt.xaml
    /// </summary>
    public partial class PresetOverwritePrompt : Window
    {       
        public PresetOverwritePrompt()
        {
            InitializeComponent();

            this.Owner = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner; 
            this.Loaded +=PresetOverwritePrompt_Loaded;           
        }

        private void PresetOverwritePrompt_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.Indicator.ActualWidth > this.Width)
            {
                this.Width = this.Indicator.ActualWidth + (this.Indicator.ActualWidth - this.Width);
            }
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {            
            this.DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            e.Handled = true;
        }

        public String Text 
        {
            get { return this.Indicator.Text; }
            set
            {
                this.Indicator.Text = value;               
            }
        }

        public Visibility IsCancelButtonVisible
        {
            get
            {
                return this.cancelButton.Visibility;
            }
            set
            {
                this.cancelButton.Visibility = value;
            }
        }
     
    }
}
