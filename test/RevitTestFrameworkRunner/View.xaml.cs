using System.Windows;

namespace RevitTestFrameworkRunner
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : Window
    {
        public View(ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
