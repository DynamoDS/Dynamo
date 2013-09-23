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

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for PreviewPopup.xaml
    /// </summary>
    public partial class PopupView : UserControl
    {
        private PopupViewModel _viewModel;

        public PopupView(PopupViewModel viewmodel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewmodel;
        }

        private void FadeInPopupWindow()
        {
            _viewModel.FadeInCommand.Execute(null);
        }

        private void FadeOutPopupWindow()
        {
            _viewModel.FadeOutCommand.Execute(null);
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