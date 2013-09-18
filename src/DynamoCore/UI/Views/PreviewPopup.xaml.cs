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

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for PreviewPopup.xaml
    /// </summary>
    public partial class PreviewPopup : UserControl
    {
        public Popup PopupWindow { get { return this.popup; } }

        #region Public Methods

        public PreviewPopup()
        {
            InitializeComponent();
        }

        public void UpdatePopupWindow(Point relativePoint, int treeViewItemHeight)
        {
            this.popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;

            this.popup.VerticalOffset = relativePoint.Y + (treeViewItemHeight / 2) - ((popup.Child as StackPanel).ActualHeight / 2);
        }

        public void FadeOutPopupWindow()
        {
        }

        public void FaceInPopupWindow()
        {

        }

        #endregion

    }
}