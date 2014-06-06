﻿using System.Windows;
using Dynamo.Core;
using Dynamo.Services;
using Dynamo.Utilities;
using System.Windows.Controls;
using System.IO;
using System.Xml;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Diagnostics;

namespace Dynamo.Nodes.Prompts
{
    /// <summary>
    /// Interaction logic for CrashPrompt.xaml
    /// </summary>
    public partial class CrashPrompt : Window
    {
        private string details; // Store here for clipboard copy
        private string folderPath;

        public CrashPrompt(string details)
        {
            InitializeComponent();
            this.CrashDetailsContent.Text = details;
        }
        
        public CrashPrompt()
        {
            InitializeComponent();
            this.CrashDetailsContent.Text = "Unknown error";
        }

        public CrashPrompt(CrashPromptArgs args)
        {
            InitializeComponent();

            if (args.HasDetails())
            {
                this.details = args.Details;
                this.CrashDetailsContent.Text = args.Details;
                this.btnDetails.Visibility = Visibility.Visible;

                InstrumentationLogger.LogInfo("CrasphPrompt", args.Details);
            }
            else
            {
                InstrumentationLogger.LogInfo("CrasphPrompt", "No details");
                
            }

            if (args.IsFilePath())
            {
                folderPath = Path.GetDirectoryName(args.FilePath);
                btnOpenFolder.Visibility = Visibility.Visible;
            }

            if (args.IsDefaultTextOverridden())
            {
                string overridingText = args.OverridingText;

                if (args.IsFilePath())
                    overridingText = overridingText.Replace("[FILEPATH]", args.FilePath);

                ConvertFormattedTextIntoTextblock(this.txtOverridingText, overridingText);
            }
        }

        private void ConvertFormattedTextIntoTextblock(TextBlock txtBox, string text)
        {
            if (txtBox == null) return;

            txtBox.Inlines.Clear();

            string formattedText = string.Format("<Span xml:space=\"preserve\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{0}</Span>", text);

            using (var xmlReader = XmlReader.Create(new StringReader(formattedText)))
            {
                var result = (Span)XamlReader.Load(xmlReader);
                txtBox.Inlines.Add(result);
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PostOnGithub_Click(object sender, RoutedEventArgs e)
        {
            dynSettings.Controller.ReportABug(null);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            this.btnDetails.Visibility = Visibility.Collapsed;
            this.CrashDetailsContent.Visibility = Visibility.Visible;
            this.btnCopy.Visibility = Visibility.Visible;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            this.CrashDetailsContent.Visibility = Visibility.Visible;
            Clipboard.SetData(DataFormats.Text, details);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (folderPath == null || folderPath == "")
                return;

            // Catch for exception, for cases where the directory does not exist
            try { Process.Start(@folderPath); }
            catch { }
        }

    }
}
