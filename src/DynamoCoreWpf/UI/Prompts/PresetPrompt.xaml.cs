using System.Linq;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;

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

            this.nameBox.Focus();
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

        public string Text
        {
            get { return this.nameBox.Text; }
        }

        public string Description
        {
            get { return this.DescriptionInput.Text; }
        }

      
    }
}
