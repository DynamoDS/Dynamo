using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;
using System;
using System.Windows.Controls;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for PresetPrompt.xaml
    /// </summary>
    public partial class PresetPrompt : Window
    {
        public PresetPrompt()
        {
            InitializeComponent();

            this.Owner = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Loaded +=PresetPrompt_Loaded;
            this.nameBox.Focus();
        }

        private void PresetPrompt_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextRemaining.Content = Wpf.Properties.Resources.PresetTextRemaining + " " + (this.nameBox.MaxLength - this.nameBox.Text.Length);
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public string Text
        {
            get
            {
                return this.nameBox.Text;
            }

            set
            {
                this.nameBox.Text = value;
            }
        }

        public string Description
        {
            get { return this.DescriptionInput.Text; }
        }

        public int MaxLength
        {
            get { return this.nameBox.MaxLength; }
        }


        private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.nameBox.MaxLength - this.nameBox.Text.Length < 1)
            {
                this.Text = this.Text.Substring(0, MaxLength);
            }
            this.TextRemaining.Content = Wpf.Properties.Resources.PresetTextRemaining  + " " + (this.nameBox.MaxLength - this.nameBox.Text.Length);
        }
    }
}
