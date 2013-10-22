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
using Dynamo.ViewModels;
using Dynamo.Utilities;

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
            if (e.PropertyName == "Content")
            {
                UpdateContent();
            }
        }

        private TextBox GetNewTextBox(string text)
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

        private void UpdateContent()
        {
            //The reason of changing the content from the code behind like this is due to a bug of WPF
            //  The bug if when you set the max width of an existing text box and then try to get the 
            //  expected size of it by using TextBox.Measure(..) method it will return the wrong value.
            //  The only solution that I can come up for now is clean the StackPanel content and 
            //  then add a new TextBox to it

            ContentContainer.Children.Clear();
            TextBox textBox = GetNewTextBox(ViewModel.Content);
            ContentContainer.Children.Add(textBox);

            ContentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            ViewModel.EstimatedWidth = ContentContainer.DesiredSize.Width;
            ViewModel.EstimatedHeight = ContentContainer.DesiredSize.Height;
        }

        private InfoBubbleViewModel GetViewModel()
        {
            if (this.DataContext is InfoBubbleViewModel)
                return this.DataContext as InfoBubbleViewModel;
            else
                return null;
        }

        private void ShowPreviewBubbleFullContent()
        {
            string content = ViewModel.FullContent;
            InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.Preview;
            InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Top;
            Point topLeft = ViewModel.TargetTopLeft;
            Point botRight = ViewModel.TargetBotRight;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket(style, topLeft, botRight, content, connectingDirection);
            this.ViewModel.UpdateContentCommand.Execute(data);
        }

        private void ShowPreviewBubbleCondensedContent()
        {
            string content = ViewModel.FullContent;
            InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.PreviewCondensed;
            InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Top;
            Point topLeft = ViewModel.TargetTopLeft;
            Point botRight = ViewModel.TargetBotRight;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket(style, topLeft, botRight, content, connectingDirection);
            this.ViewModel.UpdateContentCommand.Execute(data);
        }

        private void FadeInInfoBubble()
        {
            ViewModel.FadeInCommand.Execute(null);
        }

        private void FadeOutInfoBubble()
        {
            ViewModel.FadeOutCommand.Execute(null);
        }

        private void InfoBubble_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
            {
                ShowPreviewBubbleFullContent();
            }
            FadeInInfoBubble();
        }

        private void InfoBubble_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Preview && ViewModel.IsShowPreviewByDefault)
            {
                ShowPreviewBubbleCondensedContent();
            }
            else
            {
                 FadeOutInfoBubble();
            }
        }

        private void InfoBubble_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.InfoBubbleStyle != InfoBubbleViewModel.Style.Preview && ViewModel.InfoBubbleStyle != InfoBubbleViewModel.Style.PreviewCondensed)
                return;

            if (ViewModel.IsShowPreviewByDefault)
            {
                ViewModel.IsShowPreviewByDefault = false;
                ViewModel.SetAlwaysVisibleCommand.Execute(false);
                ViewModel.InstantCollapseCommand.Execute(null);
            }
            else
            {
                ViewModel.IsShowPreviewByDefault = true;
                ViewModel.ZIndex = 3;
                ViewModel.SetAlwaysVisibleCommand.Execute(true);
                ShowPreviewBubbleCondensedContent();
            }
         }
    }
}