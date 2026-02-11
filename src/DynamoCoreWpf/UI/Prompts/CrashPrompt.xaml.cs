using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.ViewModels;

namespace Dynamo.Nodes.Prompts
{
    /// <summary>
    /// Interaction logic for CrashPrompt.xaml
    /// </summary>
    public partial class CrashPrompt : Window
    {
        private string details; // Store here for clipboard copy
        private string folderPath;
        private string productName;
        private string markdownPackages;

        public CrashPrompt(string details, DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            this.CrashDetailsContent.Text = details;

            productName = dynamoViewModel.BrandingResourceProvider.ProductName;
            Title = string.Format(Wpf.Properties.Resources.CrashPromptDialogTitle, productName);
            TitleTextBlock.Text = string.Format(Wpf.Properties.Resources.CrashPromptDialogTitle, productName);
            txtOverridingText.Text = string.Format(Wpf.Properties.Resources.CrashPromptDialogCrashMessage, productName);
        }

        public CrashPrompt(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            this.CrashDetailsContent.Text = "Unknown error";
            productName = dynamoViewModel.BrandingResourceProvider.ProductName;
            Title = string.Format(Wpf.Properties.Resources.CrashPromptDialogTitle, productName);
            TitleTextBlock.Text = string.Format(Wpf.Properties.Resources.CrashPromptDialogTitle, productName);
            txtOverridingText.Text = string.Format(Wpf.Properties.Resources.CrashPromptDialogCrashMessage, productName);
        }

        public CrashPrompt(CrashPromptArgs args, DynamoViewModel dynamoViewModel) : this(dynamoViewModel, args)
        {}

        internal CrashPrompt(object sender, CrashPromptArgs args)
        {
            DynamoModel model = null;
            var dynamoViewModel = sender as DynamoViewModel;
            if (dynamoViewModel != null)
            {
                model = dynamoViewModel.Model;
            }
            else if (sender is DynamoModel dm)
            {
                model = dm;
            }

            InitializeComponent();

            var packageLoader = model?.GetPackageManagerExtension()?.PackageLoader;
            markdownPackages = Wpf.Utilities.CrashUtilities.PackagesToMakrdown(packageLoader);

            productName = dynamoViewModel?.BrandingResourceProvider.ProductName ?? Process.GetCurrentProcess().ProcessName;
            Title = string.Format(Wpf.Properties.Resources.CrashPromptDialogTitle, productName);
            TitleTextBlock.Text = string.Format(Wpf.Properties.Resources.CrashPromptDialogTitle, productName);
            txtOverridingText.Text = string.Format(Wpf.Properties.Resources.CrashPromptDialogCrashMessage, productName);

            if (args.HasDetails())
            {
                this.details = args.Details;
                this.CrashDetailsContent.Text = args.Details;
                this.btnDetails.Visibility = Visibility.Visible;
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
            DynamoViewModel.ReportABug(markdownPackages);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            this.Height = this.ActualHeight + 250;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.btnDetails.Visibility = Visibility.Collapsed;
            this.CrashDetailsContent.Visibility = Visibility.Visible;
            this.btnCopy.Visibility = Visibility.Visible;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            this.CrashDetailsContent.Visibility = Visibility.Visible;

            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName);
            Clipboard.SetData(DataFormats.Text, details + "\n\n-------- Assemblies --------\n\n" + string.Join("|", allAssemblies));
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (folderPath == null || folderPath == "")
                return;

            // Catch for exception, for cases where the directory does not exist
            try { Process.Start(new ProcessStartInfo(@folderPath) { UseShellExecute = true }); }
            catch { }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Lets the user drag this window around with their left mouse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            DragMove();
        }

        // ESC Button pressed triggers Window close        
        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
