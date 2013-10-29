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
using System.Diagnostics;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for PreviewInfoBubble.xaml
    /// </summary>
    public partial class InfoBubbleView : UserControl
    {
        #region Properties

        private bool isResizing = false;
        private bool isResizeHeight = false;
        private bool isResizeWidth = false;

        public InfoBubbleViewModel ViewModel { get { return GetViewModel(); } }
        #endregion

        public InfoBubbleView()
        {
            InitializeComponent();
            this.DataContextChanged += InfoBubbleView_DataContextChanged;
        }

        private void InfoBubbleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is InfoBubbleViewModel)
                (DataContext as InfoBubbleViewModel).PropertyChanged += ViewModel_PropertyChanged;
            UpdateContent();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Content")
            {
                UpdateContent();
            }
        }

        private void UpdateContent()
        {
            //The reason of changing the content from the code behind like this is due to a bug of WPF
            //  The bug if when you set the max width of an existing text box and then try to get the 
            //  expected size of it by using TextBox.Measure(..) method it will return the wrong value.
            //  The only solution that I can come up for now is clean the StackPanel content and 
            //  then add a new TextBox to it

            ContentContainer.Children.Clear();

            if (ViewModel == null) return;

            TextBox textBox = GetNewTextBox(ViewModel.Content);
            ContentContainer.Children.Add(textBox);

            ContentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            ViewModel.EstimatedWidth = ContentContainer.DesiredSize.Width;
            ViewModel.EstimatedHeight = ContentContainer.DesiredSize.Height;
        }

        private TextBox GetNewTextBox(string text)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            return textBox;
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
            this.ViewModel.ZIndex = 5;
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
            this.ViewModel.ZIndex = 3;
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
            ViewModel.FadeInCommand.Execute(null);
        }

        private void FadeOutInfoBubble()
        {
            ViewModel.FadeOutCommand.Execute(null);
        }

        private void ContentContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
            {
                ShowPreviewBubbleFullContent();
            }
            FadeInInfoBubble();
            Mouse.OverrideCursor = CursorsLibrary.UsualPointer;
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

            Mouse.OverrideCursor = CursorsLibrary.UsualPointer;
        }

        private void InfoBubble_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Mouse.OverrideCursor != CursorsLibrary.Condense)
                Mouse.OverrideCursor = CursorsLibrary.Condense;
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

        private void ResizeObject_MouseLeave(object sender, MouseEventArgs e)
        {
            //Mouse.OverrideCursor = null;
            Mouse.OverrideCursor = CursorsLibrary.UsualPointer;
        }

        private void ResizeObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            isResizing = false;
            isResizeHeight = false;
            isResizeWidth = false;
        }

        private void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizing)
                return;

            Point mouseLocation = Mouse.GetPosition(mainGrid);
            if (!isResizeHeight)
                mouseLocation.Y = double.MaxValue;
            if (!isResizeWidth)
                mouseLocation.X = double.MaxValue;

            ViewModel.ResizeCommand.Execute(mouseLocation);
        }

        private void HorizontalResizeBar_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Mouse.OverrideCursor != CursorsLibrary.ResizeVertical)
                Mouse.OverrideCursor = CursorsLibrary.ResizeVertical;
        }

        private void HorizontalResizeBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            e.Handled = true;

            isResizing = true;
            isResizeHeight = true;
        }

        private void ConnerResizePoint_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Mouse.OverrideCursor != CursorsLibrary.ResizeDiagonal)
                Mouse.OverrideCursor = CursorsLibrary.ResizeDiagonal;
        }

        private void ConnerResizePoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            e.Handled = true;

            isResizing = true;
            isResizeWidth = true;
            isResizeHeight = true;
        }

        private void VerticalResizeBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            e.Handled = true;

            isResizing = true;
            isResizeWidth = true;
        }

        private void VerticalResizeBar_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Mouse.OverrideCursor != CursorsLibrary.ResizeHorizontal)
                Mouse.OverrideCursor = CursorsLibrary.ResizeHorizontal;
        }

        private void InfoBubble_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void InfoBubble_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);

            //Debug.WriteLine(this.GetType());
            //Debug.WriteLine(mousePosition.X + "  " + this.ActualWidth);

            double offsetX = this.ActualWidth - ViewModel.EstimatedWidth;
            double offsetY = this.ActualHeight - ViewModel.EstimatedHeight;
            if (Math.Abs(mousePosition.X - offsetX - ViewModel.EstimatedWidth / 2) < 25
                && (mousePosition.Y - offsetY < 20)
                && (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Preview))
            {
                Mouse.OverrideCursor = CursorsLibrary.Expand;
            }
            else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
            {
                Mouse.OverrideCursor = CursorsLibrary.Condense;
            }
            else
                Mouse.OverrideCursor = CursorsLibrary.UsualPointer;
        }

    }
}