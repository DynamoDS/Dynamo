using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.DynamoSandbox
{
    /// <summary>
    /// Interaction logic for SettingsMigrationWindow.xaml
    /// </summary>
    public partial class SettingsMigrationWindow : Window
    {
        public SettingsMigrationWindow()
        {
            InitializeComponent();

            TitleBox.Text = Properties.Resources.SettingsMigrationDialogTitle;
            TextBoxPrompt.Text = Properties.Resources.SettingsMigrationDialogMessage;
        }
    }
}
