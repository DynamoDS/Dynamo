using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Utilities;
using Dynamo.ViewModels;

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
                return this.nameBox.Text.Trim();
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
            if (this.Owner != null)
            {
                var dataContext = this.Owner.DataContext as DynamoViewModel;
                var presets = dataContext == null ? null : dataContext.Model.CurrentWorkspace.Presets;
                 //get the preset names from workspace
                if (presets != null && presets.Any())
                {
                    if (dataContext.Model.CurrentWorkspace.Presets.Any(x => x.Name == Text))
                    {
                        var newDialog = new PresetOverwritePrompt()
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner,
                            Text = Wpf.Properties.Resources.PresetOverwrite
                        };

                        if (newDialog.ShowDialog() != true)
                        {                                                        
                            e.Handled = true;
                            return;
                        }
                        //If the dialog result is true, then remove the old preset
                        else
                        {
                            var oldPreset = presets.FirstOrDefault(x => x.Name == Text);
                            dataContext.Model.CurrentWorkspace.RemovePreset(oldPreset);
                        }
                    }
                }

            }
            this.DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
               
        private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Text.Length > this.nameBox.MaxLength)
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
