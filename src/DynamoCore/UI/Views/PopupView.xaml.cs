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
        public PopupView()
        {
            InitializeComponent();
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