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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using InfoBubbleViewModel = Dynamo.ViewModels.InfoBubbleViewModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for PreviewInfoBubble.xaml
    /// </summary>
    public partial class InfoBubbleView : UserControl
    {
        public InfoBubbleViewModel ViewModel { get { return GetViewModel(); } }

        public InfoBubbleView()
        {
            InitializeComponent();
            this.DataContextChanged += InfoBubbleView_DataContextChanged;
        }

        private void InfoBubbleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as InfoBubbleViewModel).PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemDescription")
            {
                ContentContainer.Children.Clear();
                TextBox textBox = GetStyledTextBox(ViewModel.ItemDescription);
                ContentContainer.Children.Add(textBox);

                ContentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                ViewModel.EstimatedWidth = ContentContainer.DesiredSize.Width;
                ViewModel.EstimatedHeight = ContentContainer.DesiredSize.Height;
            }
        }

        private TextBox GetStyledTextBox(string text)
        {
            TextBox textBox = new TextBox();
            textBox.TextWrapping = ViewModel.ContentWrapping;
            textBox.Text = text;
            textBox.IsReadOnly = true;
            textBox.BorderThickness = new Thickness(0);
            textBox.Background = Brushes.Transparent;
            textBox.Foreground = ViewModel.TextForeground;
            textBox.FontWeight = ViewModel.TextFontWeight;
            textBox.FontSize = ViewModel.TextFontSize;
            textBox.Margin = ViewModel.ContentMargin;
            textBox.MaxWidth = ViewModel.MaxWidth;
            return textBox;
        }

        private InfoBubbleViewModel GetViewModel()
        {
            if (this.DataContext is InfoBubbleViewModel)
                return this.DataContext as InfoBubbleViewModel;
            else
                return null;
        }

        private void FadeInInfoBubble()
        {
            InfoBubbleViewModel viewModel = GetViewModel();
            viewModel.FadeInCommand.Execute(null);
        }

        private void FadeOutInfoBubble()
        {
            InfoBubbleViewModel viewModel = GetViewModel();
            viewModel.FadeOutCommand.Execute(null);
        }

        private void InfoBubble_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mainGrid.Opacity != 0)
                FadeInInfoBubble();
        }

        private void InfoBubble_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mainGrid.Opacity != 0)
                FadeOutInfoBubble();
        }
    }
}