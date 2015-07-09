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
        public string Description
        {
            get { return this.DescriptionInput.Text; }
        }

        public int MaxLength
        {
            get { return this.nameBox.MaxLength; }
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
            UpdateText();
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
               
        private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.nameBox.Text.Length > this.nameBox.MaxLength)
            {
                this.Text = this.Text.Substring(0, MaxLength);
            }

            UpdateText();
        }

        private void UpdateText()         
        {
            this.TextRemaining.Content =  (this.nameBox.MaxLength - this.nameBox.Text.Length) + " " +
                                                Wpf.Properties.Resources.PresetTextRemaining;
        }
    }
}
