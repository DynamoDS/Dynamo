using System;
using System.Windows.Controls;
using Dynamo.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        private readonly Random random = new Random();
        private readonly LibraryModel libraryData = new LibraryModel();

        private int lastSelectedClassObject;

        public LibraryView()
        {
            InitializeComponent();
            CreateInitialData();
            CategoryListView.ItemsSource = libraryData.Categories;
        }

        private void CreateInitialData()
        {
            // Create some categories...
            for (int c = 0; c < 10; c++)
            {
                var category = new Category(string.Format("Category #{0}", c));
                libraryData.Add(category);

                // Generate some classes.
                var objects = random.Next(10, 20);
                for (int o = 0; o < objects; o++)
                {
                    var classObject = new ClassObject(string.Format("Class #{0}", o));
                    classObject.Index = o;
                    category.Add(classObject);

                    // Generate some data members.
                    var members = random.Next(1, 6);
                    for (int m = 0; m < members; m++)
                    {
                        classObject.CreateMembers.Add(new ClassMember(string.Format("CMember #{0}", m)));
                    }
                    members = random.Next(1, 6);
                    for (int m = 0; m < members; m++)
                    {
                        classObject.ActionMembers.Add(new ClassMember(string.Format("AMember #{0}", m)));
                    }
                    members = random.Next(1, 6);
                    for (int m = 0; m < members; m++)
                    {
                        classObject.QueryMembers.Add(new ClassMember(string.Format("QMember #{0}", m)));
                    }
                }
            }
        }

        //used to make ClassDetails unclickable
        private void OnListViewClassesMouseDown(object sender, RoutedEventArgs e)
        {
            //var item = sender as ClassObject;
            //var itm = e.OriginalSource as ListBoxItem;
            //var t = sender as FrameworkElement;
            //var s = TryFindParent(sender as ListViewItem);
            
            
            ListView SubCategoryListView = null;
            ListViewItem lvi = sender as ListViewItem;
            // Get a reference to the parent ListViewItem control
            DependencyObject temp = lvi;
            int maxlevel = 0;
            while (!(temp is ListView) && maxlevel < 1000)
            {
                temp = VisualTreeHelper.GetParent(temp);
                maxlevel++;
            }
            SubCategoryListView = temp as ListView;
            SubCategoryListView.SelectedItems.Clear();
            SubCategoryListView.SelectedItems.Add(SubCategoryListView.Items[5]);
            
            

            
        }

        public static ListView TryFindParent(ListViewItem child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            ListView parent = parentObject as ListView;
            if (parent != null)
                return parent;
            else
                return null;
        }
    }
}
