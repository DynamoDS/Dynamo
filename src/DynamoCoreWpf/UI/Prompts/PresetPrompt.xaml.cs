﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;
using System;

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
