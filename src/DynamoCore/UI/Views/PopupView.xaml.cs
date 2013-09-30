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
using PopupViewModel = Dynamo.ViewModels.PopupViewModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for PreviewPopup.xaml
    /// </summary>
    public partial class PopupView : UserControl
    {
        public PopupViewModel ViewModel { get { return GetViewModel(); } }

        public PopupView()
        {
            InitializeComponent();
            this.DataContextChanged += PopupView_DataContextChanged;
        }

        private void PopupView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as PopupViewModel).PropertyChanged += ViewModel_PropertyChanged;
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
            return textBox;
        }

        private PopupViewModel GetViewModel()
        {
            if (this.DataContext is PopupViewModel)
                return this.DataContext as PopupViewModel;
            else
                return null;
        }

        private void FadeInPopupWindow()
        {
            PopupViewModel viewModel = GetViewModel();
            viewModel.FadeInCommand.Execute(null);
        }

        private void FadeOutPopupWindow()
        {
            PopupViewModel viewModel = GetViewModel();
            viewModel.FadeOutCommand.Execute(null);
        }

        private void Popup_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mainGrid.Opacity != 0)
                FadeInPopupWindow();
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mainGrid.Opacity != 0)
                FadeOutPopupWindow();
        }
    }
}