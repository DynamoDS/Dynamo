using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UnitsUI.Controls
{
    /// <summary>
    /// Interaction logic for ListBox.xaml
    /// </summary>
    public partial class ListViewCustom : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private UnitValueOutputDropdownViewCustomization vm;

        private ObservableCollection<DockPanel> obsCollection;
        public ObservableCollection<DockPanel> CustomItems
        {
            get
            {
                return obsCollection;
            } 
            set
            {
                if (value is null) return;
                obsCollection = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                MainListView.ItemsSource = obsCollection;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(nameof(CustomItems)));
                }
            }
        }
        public ListViewCustom(ObservableCollection<DockPanel> customization)
        {
            this.DataContext = this;
          
            InitializeComponent();

            CustomItems = customization;
            MainListView.ItemsSource = CustomItems;
        }

    }
}
